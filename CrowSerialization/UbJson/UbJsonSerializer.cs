using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrowSerialization.UbJson
{
	// http://ubjson.org/type-reference/
	enum Token : byte
	{
		None = 0, // error

		// note: length token == int8, int16, int32, int64 or intVR

		// value token
		Null = (byte)'Z', // 1 byte
		//NoOp = (byte)'N', // 1 byte

		True = (byte)'T', // 1 byte
		False = (byte)'F', // 1 byte

		Int8 = (byte)'i', // 2 byte
		Int16 = (byte)'I', // 3 byte
		Int32 = (byte)'l', // 5 byte
		Int64 = (byte)'L', // 9 byte

		Float32 = (byte)'d', // 5 byte; float
		Float64 = (byte)'D', // 9 byte; double
		Float128 = (byte)'h', // 17 byte; decimal

		String = (byte)'S', // 1 byte + IntVL + string char

		//IntVL = (byte)'z', // int variable length; 2 - 10 bytes

		// container token
		ArrayBegin = (byte)'[', // 2+ bytes
		ArrayEnd = (byte)']',
		ObjectBegin = (byte)'{', // 2+ bytes
		ObjectEnd = (byte)'}',

		// http://ubjson.org/type-reference/container-types/#optimized-format
		// Optimized Format for Array container
		//  If a type is specified, it must be done so before a count.
		//  If a type is specified, a count must be specified as well.
		Type = (byte)'$', // 1  byte + value token
		Count = (byte)'#', // 1 byte + length token

		// Custom --------------------------------------------------------
		// value token type 
		//RefType = (byte)'!', // 1 byte + length token + string char; reflection type 
	}

	//static class UbJsonEx
	//{
	//	public static void WriteToken ( this BinaryWriter writer, Token token )
	//	{
	//		writer.Write ( (byte)token );
	//	}

	//	public static void Write7BitEncodedInt ( this BinaryWriter writer, int value )
	//	{
	//		uint num = (uint)value;
	//		while ( num >= 128u )
	//		{
	//			writer.Write ( (byte)(num | 128u) );
	//			num >>= 7;
	//		}
	//		writer.Write ( (byte)num );
	//	}
	//}

	public class UbjsonSerializer
	{
		public void Serialize ( object obj, Stream stream )
		{
			var writer = new UbjsonWriter ( stream, Encoding.UTF8, true );
			WriteValue ( obj, writer, 0 );
		}

		public T Deserialize<T> ( Stream stream )
		{
			return (T)Deserialize ( typeof ( T ), stream );
		}
		public object Deserialize ( Type instanceType, Stream stream )
		{
			var reader = new UbjsonReader ( stream, Encoding.UTF8, true );
			return ReadValue ( instanceType, reader );
		}


		private object ReadValue ( Type objType, UbjsonReader reader )
		{
			object instance = null;

			reader.ReadToken ();

			if ( reader.CurrentToken == Token.ObjectBegin || reader.CurrentToken == Token.ArrayBegin )
			{
				var bToken = reader.CurrentToken;

				Token? valueType = null;
				int? valueCount = null;

				var t = reader.PeekToken ();
				if ( t == Token.Type )
				{
					reader.Seek ( 1 );
					valueType = reader.ReadToken ();
				}
				t = reader.PeekToken ();
				if ( t == Token.Count )
				{
					reader.Seek ( 1 );
					reader.ReadToken ();
					valueCount = reader.ReadLength ();
				}

				if ( bToken == Token.ObjectBegin )
				{
					// todo add optimization case for IDictionary<,>
					// http://ubjson.org/type-reference/container-types/#optimized-format-example-object

					#region target object is a IDictionary<,> (Key:Value pairs)
					if ( objType.IsGenericType )
					{
						if ( HasGenericInterface ( objType, typeof ( IDictionary<,> ) ) )
						{
							instance = Activator.CreateInstance ( objType );

							if ( ((IDictionary)instance).IsReadOnly ) return null;

							Type[] argumentType = objType.GetGenericArguments ();

							if ( argumentType[0] != typeof ( string ) ) return null;

							while ( true )
							{
								reader.ReadToken ();
								var name = reader.ReadString ();

								var value = ReadValue ( argumentType[1], reader );

								value = Convert.ChangeType ( value, argumentType[1] );

								((IDictionary)instance).Add ( name, value );

								if ( reader.PeekToken () == Token.ObjectEnd )
								{
									reader.ReadToken ();
									break;
								}
							}
							return instance;
						}
					}
					#endregion


					#region Handle Object
					instance = Activator.CreateInstance ( objType );

					ObjectMetadata metadata;
					ReflectionCache.GetObjectMetadata ( objType, out metadata );

					while ( true )
					{
						reader.ReadToken ();
						var name = reader.ReadString ();

						PropertyMetadata data;
						if ( metadata.properties.TryGetValue ( name, out data ) )
						{
							var value = ReadValue ( data.Type, reader );
							value = Convert.ChangeType ( value, data.Type );
							data.SetValue ( instance, value );
						}
						else
						{
							SkipReadValue ( reader );
						}

						if ( reader.PeekToken () == Token.ObjectEnd )
						{
							reader.ReadToken ();
							break;
						}
					}

					return instance;
					#endregion
				}
				else
				{
					#region Handle Array
					// Primitive case
					//if ( valueType.HasValue )
					//{
					//	if ( !valueCount.HasValue )
					//	{
					//		throw new System.IO.InvalidDataException ();
					//	}

					// fix size array
					if ( objType.IsArray )
					{
						var itemType = objType.GetElementType ();
						Array array = Array.CreateInstance ( itemType, valueCount.Value );

						//var typeCode = Type.GetTypeCode ( eleType );
						//if ( typeCode != TypeCode.DBNull && typeCode == TypeCode.Object )
						//{
						//	// todo fast generic
						//	for ( int i = 0; i < array.Length; i++ )
						//	{
						//		var value = reader.ReadValueByToken ( valueType.Value );
						//		value = Convert.ChangeType ( value, eleType );
						//		array.SetValue ( value, i );
						//	}
						//}
						//else
						//{
						for ( int i = 0; i < array.Length; i++ )
						{
							var value = ReadValue ( itemType, reader );
							value = Convert.ChangeType ( value, itemType );
							array.SetValue ( value, i );
						}
						//}

						return array;
					}
					// IList<>
					if ( objType.IsGenericType && HasGenericInterface ( objType, typeof ( IList<> ) ) )
					{
						instance = Activator.CreateInstance ( objType );
						if ( ((IList)instance).IsReadOnly ) return null;

						Type itemType = objType.GetGenericArguments ()[0];

						// todo fast generic
						for ( int i = 0; i < valueCount.Value; i++ )
						{
							var value = ReadValue ( itemType, reader ); ;
							value = Convert.ChangeType ( value, itemType );
							((IList)instance).Add ( value );
						}
						return instance;
					}
					//}
					#endregion
				}

				return null;
			}

			if ( reader.CurrentToken == Token.Type || reader.CurrentToken == Token.Count )
			{
				throw new InvalidDataException ();
			}


			instance = reader.ReadValue ();
			if ( objType.IsEnum )
			{
				if ( reader.CurrentToken == Token.Int8 || reader.CurrentToken == Token.Int16
					|| reader.CurrentToken == Token.Int32 || reader.CurrentToken == Token.Int64 )
				{
					instance = Enum.ToObject ( objType, instance );
				}
				else
				{
					return Enum.ToObject ( objType, 0 );
				}
			}
			else if ( objType == typeof ( DateTime ) )
			{
				if ( instance is long )
				{
					instance = DateTime.FromBinary ( (long)instance );
				}
				else
				{
					instance = default ( DateTime );
				}
			}
			return instance;
		}

		private void WriteValue ( object obj, UbjsonWriter writer, int depth )
		{
			if ( depth > 100 )
				throw new Exception ( string.Format ( "Max allowed object depth reached while trying to export from type {0}", obj.GetType () ) );

			if ( obj == null )
			{
				writer.WriteToken ( Token.Null );
				return;
			}

			var objType = obj.GetType ();

			// add user defined converter

			switch ( Type.GetTypeCode ( objType ) )
			{
				case TypeCode.Boolean: writer.WriteValue ( (bool)obj ); break;
				case TypeCode.Byte: writer.WriteValue ( (byte)obj ); break;
				case TypeCode.Char: writer.WriteValue ( (char)obj ); break;
				case TypeCode.DateTime: writer.WriteValue ( ((DateTime)obj).ToBinary () ); break;
				case TypeCode.Decimal: writer.WriteValue ( (decimal)obj ); break;
				case TypeCode.Double: writer.WriteValue ( (double)obj ); break;
				case TypeCode.Empty: writer.WriteToken ( Token.Null ); break;
				case TypeCode.Int16: writer.WriteValue ( (short)obj ); break;
				case TypeCode.Int32: writer.WriteValue ( (int)obj ); break;
				case TypeCode.Int64: writer.WriteValue ( (long)obj ); break;

				case TypeCode.Object:
					#region Object ser
					if ( objType.IsEnum )
					{
						object underlyingValue = Convert.ChangeType ( obj, Enum.GetUnderlyingType ( objType ) );
						WriteValue ( underlyingValue, writer, depth + 1 );
						break;
					}
					if ( objType.IsArray )
					{
						/*
						if ( WriteArrayPrimitiveType ( (Array)obj, objType, writer ) )
						{
							// was a primitive array type
							break;
						}
						*/
						// write a normal object array
						writer.WriteToken ( Token.ArrayBegin );
						writer.WriteToken ( Token.Count );
						var array = (Array)obj;
						writer.WriteLength ( array.Length );
						for ( int i = 0; i < array.Length; i++ )
						{
							WriteValue ( array.GetValue ( i ), writer, depth + 1 );
						}
						break;
					}
					if ( objType.IsGenericType )
					{
						if ( HasGenericInterface ( objType, typeof ( IList<> ) ) )
						{
							if ( ((IList)obj).IsReadOnly ) break;

							Type itemType = objType.GetGenericArguments ()[0];
							/*
							if ( WriteListPrimitiveType ( (IList)obj, itemType, writer ) )
							{
								// was a primitive list type
								break;
							}
							*/

							// write a normal array
							writer.WriteToken ( Token.ArrayBegin );
							writer.WriteToken ( Token.Count );
							var list = (IList)obj;
							writer.WriteLength ( list.Count );
							for ( int i = 0; i < list.Count; i++ )
							{
								WriteValue ( list[i], writer, depth + 1 );
							}
							break;
						}
						if ( HasGenericInterface ( objType, typeof ( IDictionary<,> ) ) )
						{
							if ( ((IDictionary)obj).IsReadOnly ) break;

							Type[] argumentType = objType.GetGenericArguments ();

							if ( argumentType[0] != typeof ( string ) ) break;

							// todo add optimization case
							// http://ubjson.org/type-reference/container-types/#optimized-format-example-object

							writer.WriteToken ( Token.ObjectBegin );
							foreach ( DictionaryEntry pair in (IDictionary)obj )
							{
								writer.WritePropertyName ( (string)pair.Key );
								WriteValue ( pair.Value, writer, depth + 1 );
							}
							writer.WriteToken ( Token.ObjectEnd );
							break;
						}
					}

					// write as object

					Dictionary<string, PropertyMetadata> meta;
					ReflectionCache.GetTypeProperties ( objType, out meta );

					writer.WriteToken ( Token.ObjectBegin );
					foreach ( var info in meta.Values )
					{
						writer.WritePropertyName ( info.Info.Name );
						WriteValue ( info.GetValue ( obj ), writer, depth + 1 );
					}
					writer.WriteToken ( Token.ObjectEnd );
					#endregion
					break;

				case TypeCode.SByte: writer.WriteValue ( (sbyte)obj ); break;
				case TypeCode.Single: writer.WriteValue ( (float)obj ); break;
				case TypeCode.String: writer.WriteValue ( (string)obj ); break;
				case TypeCode.UInt16: writer.WriteValue ( (ushort)obj ); break;
				case TypeCode.UInt32: writer.WriteValue ( (uint)obj ); break;
				case TypeCode.UInt64: writer.WriteValue ( (ulong)obj ); break;
			}
		}


		private void SkipReadValue ( UbjsonReader reader )
		{
			reader.ReadToken ();

			if ( reader.CurrentToken == Token.ObjectBegin || reader.CurrentToken == Token.ArrayBegin )
			{
				var bToken = reader.CurrentToken;

				Token? valueType = null;
				int? valueCount = null;

				var t = reader.PeekToken ();
				if ( t == Token.Type )
				{
					reader.Seek ( 1 );
					valueType = reader.ReadToken ();
				}
				t = reader.PeekToken ();
				if ( t == Token.Count )
				{
					reader.Seek ( 1 );
					reader.ReadToken ();
					valueCount = reader.ReadLength ();
				}

				if ( bToken == Token.ObjectBegin )
				{
					// todo add optimization case for IDictionary<,>
					// http://ubjson.org/type-reference/container-types/#optimized-format-example-object

					while ( true )
					{
						reader.ReadToken ();
						reader.SkipPropertyName ();

						SkipReadValue ( reader );

						if ( reader.PeekToken () == Token.ObjectEnd )
						{
							reader.ReadToken ();
							break;
						}
					}
				}
				else
				{
					for ( int i = valueCount.Value - 1; i >= 0; i-- )
					{
						SkipReadValue ( reader );
					}
				}
			}
			else
			{
				reader.SkipRead ();
			}
		}


		private bool WriteArrayPrimitiveType ( Array obj, Type objType, UbjsonWriter writer )
		{
			int i;
			var eType = objType.GetElementType ();
			switch ( Type.GetTypeCode ( eType ) )
			{
				case TypeCode.Boolean:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Count );
					{
						var b = (bool[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.WriteToken ( b[i] ? Token.True : Token.False );
						}
					}
					return true;
				case TypeCode.Byte:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int8 );
					writer.WriteToken ( Token.Count );
					{
						var b = (byte[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Char:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int16 );
					writer.WriteToken ( Token.Count );
					{
						var b = (char[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.DateTime:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int64 );
					writer.WriteToken ( Token.Count );
					{
						var b = (DateTime[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( ((DateTime)b[i]).ToBinary () );
						}
					}
					return true;
				case TypeCode.Decimal:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Float128 );
					writer.WriteToken ( Token.Count );
					{
						var b = (decimal[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Double:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Float64 );
					writer.WriteToken ( Token.Count );
					{
						var b = (double[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Int16:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int16 );
					writer.WriteToken ( Token.Count );
					{
						var b = (short[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Int32:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int32 );
					writer.WriteToken ( Token.Count );
					{
						var b = (int[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Int64:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int64 );
					writer.WriteToken ( Token.Count );
					{
						var b = (long[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.SByte:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int8 );
					writer.WriteToken ( Token.Count );
					{
						var b = (sbyte[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Single:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Float32 );
					writer.WriteToken ( Token.Count );
					{
						var b = (float[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.String:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.String );
					writer.WriteToken ( Token.Count );
					{
						var b = (string[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.UInt16:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int16 );
					writer.WriteToken ( Token.Count );
					{
						var b = (ushort[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( (short)b[i] );
						}
					}
					return true;
				case TypeCode.UInt32:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int32 );
					writer.WriteToken ( Token.Count );
					{
						var b = (uint[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( (int)b[i] );
						}
					}
					return true;
				case TypeCode.UInt64:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int64 );
					writer.WriteToken ( Token.Count );
					{
						var b = (ulong[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( (long)b[i] );
						}
					}
					return true;

				default:
					return false;
			}
		}

		private bool WriteListPrimitiveType ( IList obj, Type itemType, UbjsonWriter writer )
		{
			int i;
			switch ( Type.GetTypeCode ( itemType ) )
			{
				case TypeCode.Boolean:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<bool>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.WriteToken ( b[i] ? Token.True : Token.False );
						}
					}
					return true;
				case TypeCode.Byte:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int8 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<byte>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Char:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int16 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<char>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.DateTime:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int64 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<DateTime>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( ((DateTime)b[i]).ToBinary () );
						}
					}
					return true;
				case TypeCode.Decimal:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Float128 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<decimal>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Double:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Float64 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<double>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Int16:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int16 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<short>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Int32:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int32 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<int>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Int64:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int64 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<long>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.SByte:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int8 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<sbyte>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Single:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Float32 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<float>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.String:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.String );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<string>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.UInt16:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int16 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<ushort>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.UInt32:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int32 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<uint>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.UInt64:
					writer.WriteToken ( Token.ArrayBegin );
					writer.WriteToken ( Token.Type );
					writer.WriteToken ( Token.Int64 );
					writer.WriteToken ( Token.Count );
					{
						var b = (IList<ulong>)obj;
						writer.WriteLength ( b.Count );
						for ( i = 0; i < b.Count; i++ )
						{
							writer.Writer.Write ( b[i] );
						}
					}
					return true;

				default:
					return false;
			}
		}


		private static bool HasGenericInterface ( Type type, Type genericInterfaceType )
		{
			foreach ( Type item in type.GetInterfaces () )
			{
				if ( item.IsGenericType && item.GetGenericTypeDefinition () == genericInterfaceType )
				{
					return true;
				}
			}
			return false;
		}
	}





}
