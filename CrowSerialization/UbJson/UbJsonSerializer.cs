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
	enum TokenType : byte
	{
		// value token
		Null = (byte)'Z', // 1 byte
		NoOp = (byte)'N', // 1 byte
		True = (byte)'T', // 1 byte
		False = (byte)'F', // 1 byte
		Int8 = (byte)'i', // 2 byte
		Uint8 = (byte)'U', // 2 byte
		Int16 = (byte)'I', // 3 byte
		Int32 = (byte)'l', // 5 byte
		Int64 = (byte)'L', // 9 byte
		Float32 = (byte)'d', // 5 byte
		Float64 = (byte)'D', // 9 byte
		HPN = (byte)'H', // high-precision number; 1 byte + int num val + string byte len
		Char = (byte)'C', // 2 byte
		String = (byte)'S', // 1 byte + int num val + string byte len

		// container token
		ArrayBegin = (byte)'[', // 2+ bytes
		ArrayEnd = (byte)']',
		ObjectBegin = (byte)'{', // 2+ bytes
		ObjectEnd = (byte)'}',

		// http://ubjson.org/type-reference/container-types/#optimized-format
		// Optimized Format for Array container
		//  If a type is specified, it must be done so before a count.
		//  If a type is specified, a count must be specified as well.
		Type = (byte)'$',
		Count = (byte)'#',

		// custom token type 
		Float128 = (byte)'h', // 17 byte;
		IntEncoded = (byte)'E', // 2 - 6 bytes; int encoded length max 40 bit;
		//DictBegin = (byte)'(',
		//DictEnd = (byte)')',
	}

	static class UbJsonEx
	{
		public static void WriteToken ( this BinaryWriter writer, TokenType token )
		{
			writer.Write ( (byte)token );
		}

		public static void Write7BitEncodedInt ( this BinaryWriter writer, int value )
		{
			uint num = (uint)value;
			while ( num >= 128u )
			{
				writer.Write ( (byte)(num | 128u) );
				num >>= 7;
			}
			writer.Write ( (byte)num );
		}
	}

	static class UbJsonSerializer
	{
		public static void Serialize ( object obj, Stream stream )
		{
			using ( var writer = new BinaryWriter ( stream, Encoding.UTF8, true ) )
			{
				WriteValue ( obj, writer, 0 );
			}
		}

		public static void Deserialize<T> ( Stream stream )
		{

		}

		private static void WriteProperty ( string name, BinaryWriter writer )
		{
			writer.WriteToken ( TokenType.IntEncoded );
			writer.Write ( name );
		}

		private static void WriteValue ( object obj, BinaryWriter writer, int depth )
		{
			if ( depth > 100 )
				throw new Exception ( string.Format ( "Max allowed object depth reached while trying to export from type {0}", obj.GetType () ) );

			var objType = obj.GetType ();

			// add user defined converter

			switch ( Type.GetTypeCode ( objType ) )
			{
				case TypeCode.Boolean:
					writer.Write ( (byte)((bool)obj ? TokenType.True : TokenType.False) );
					break;
				case TypeCode.Byte:
					writer.WriteToken ( TokenType.Int8 );
					writer.Write ( (byte)obj );
					break;
				case TypeCode.Char:
					var c = (char)obj;
					if ( c <= byte.MaxValue )
						goto case TypeCode.Byte;
					else
						goto case TypeCode.Int16;
				case TypeCode.DateTime:
					writer.WriteToken ( TokenType.Int64 );
					writer.Write ( ((DateTime)obj).ToBinary () );
					break;
				case TypeCode.Decimal:
					writer.WriteToken ( TokenType.Float128 );
					writer.Write ( (decimal)obj );
					break;
				case TypeCode.Double:
					writer.WriteToken ( TokenType.Float64 );
					writer.Write ( (double)obj );
					break;
				case TypeCode.Empty:
					writer.WriteToken ( TokenType.Null );
					break;
				case TypeCode.Int16:
					writer.WriteToken ( TokenType.Int16 );
					writer.Write ( (short)obj );
					break;
				case TypeCode.Int32:
					writer.WriteToken ( TokenType.Int32 );
					writer.Write ( (int)obj );
					break;
				case TypeCode.Int64:
					writer.WriteToken ( TokenType.Int64 );
					writer.Write ( (long)obj );
					break;

				case TypeCode.Object:
					if ( objType.IsEnum )
					{
						object underlyingValue = Convert.ChangeType ( obj, Enum.GetUnderlyingType ( objType ) );
						WriteValue ( underlyingValue, writer, depth + 1 );
						break;
					}
					if ( objType.IsArray )
					{
						if ( WriteArrayPrimitiveType ( (Array)obj, objType, writer ) )
						{
							// was a primitive array type
							break;
						}
						// write a normal object array
						writer.WriteToken ( TokenType.ArrayBegin );
						writer.WriteToken ( TokenType.Count );
						writer.WriteToken ( TokenType.IntEncoded );
						var array = (Array)obj;
						writer.Write7BitEncodedInt ( array.Length );
						for ( int i = 0; i < array.Length; i++ )
						{
							WriteValue ( array.GetValue ( i ), writer, depth + 1 );
						}
						break;
					}
					if ( obj is IList )
					{
						if ( ((IList)obj).IsReadOnly )
						{
							break;
						}
						if ( objType.IsGenericType && objType.GetGenericTypeDefinition () == typeof ( IList<> ) )
						{
							Type itemType = objType.GetGenericArguments ()[0];
							if ( WriteListPrimitiveType ( (IList)obj, itemType, writer ) )
							{
								// was a primitive list type
								break;
							}
						}
						// write a normal array
						writer.WriteToken ( TokenType.ArrayBegin );
						writer.WriteToken ( TokenType.Count );
						writer.WriteToken ( TokenType.IntEncoded );
						var list = (IList)obj;
						writer.Write7BitEncodedInt ( list.Count );
						for ( int i = 0; i < list.Count; i++ )
						{
							WriteValue ( list[i], writer, depth + 1 );
						}
						break;
					}
					if ( obj is IDictionary )
					{
						if ( ((IDictionary)obj).IsReadOnly )
						{
							break;
						}
						// todo add optimisation case
						// http://ubjson.org/type-reference/container-types/#optimized-format-example-object

						writer.WriteToken ( TokenType.ObjectBegin );
						foreach ( IDictionaryEnumerator pair in (IDictionary)obj )
						{
							WriteProperty ( (string)pair.Key, writer );
							WriteValue ( pair.Value, writer, depth + 1 );
						}
						writer.WriteToken ( TokenType.ObjectEnd );
						break;
					}

					// write as object

					List<PropertyMetadata> meta;
					ReflectionCache.GetTypeProperties ( objType, out meta );

					writer.WriteToken ( TokenType.ObjectBegin );
					for ( int i = 0, len = meta.Count; i < len; i++ )
					{
						var info = meta[i];
						WriteProperty ( info.Info.Name, writer );
						if ( info.IsField )
						{
							WriteValue ( ((FieldInfo)info.Info).GetValue ( obj ), writer, depth + 1 );
						}
						else
						{
							WriteValue ( ((PropertyInfo)info.Info).GetValue ( obj, null ), writer, depth + 1 );
						}
					}
					writer.WriteToken ( TokenType.ObjectEnd );
					break;

				case TypeCode.SByte:
					writer.WriteToken ( TokenType.Int8 );
					writer.Write ( (sbyte)obj );
					break;
				case TypeCode.Single:
					writer.WriteToken ( TokenType.Float32 );
					writer.Write ( (float)obj );
					break;
				case TypeCode.String:
					writer.WriteToken ( TokenType.String );
					writer.WriteToken ( TokenType.IntEncoded );
					writer.Write ( (string)obj );
					break;
				case TypeCode.UInt16:
					writer.WriteToken ( TokenType.Int16 );
					writer.Write ( (short)(ushort)obj );
					break;
				case TypeCode.UInt32:
					writer.WriteToken ( TokenType.Int32 );
					writer.Write ( (int)(uint)obj );
					break;
				case TypeCode.UInt64:
					writer.WriteToken ( TokenType.Int64 );
					writer.Write ( (long)(ulong)obj );
					break;
			}
		}


		private static bool WriteArrayPrimitiveType ( Array obj, Type objType, BinaryWriter writer )
		{
			int i;
			var eType = objType.GetElementType ();
			switch ( Type.GetTypeCode ( eType ) )
			{
				case TypeCode.Boolean:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (bool[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( (byte)(b[i] ? TokenType.True : TokenType.False) );
						}
					}
					return true;
				case TypeCode.Byte:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Uint8 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (byte[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Char:
					goto case TypeCode.Int16;
				case TypeCode.DateTime:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Int64 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (DateTime[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( ((DateTime)b[i]).ToBinary () );
						}
					}
					return true;
				case TypeCode.Decimal:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Float128 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (decimal[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Double:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Float64 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (double[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Int16:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Int16 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (char[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( (short)b[i] );
						}
					}
					return true;
				case TypeCode.Int32:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Int32 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (int[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Int64:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Int64 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (long[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.SByte:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Int8 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (sbyte[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.Single:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Float32 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (float[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.String:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.String );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (string[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.WriteToken ( TokenType.IntEncoded );
							writer.Write ( b[i] );
						}
					}
					return true;
				case TypeCode.UInt16:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Int16 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (ushort[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( (short)b[i] );
						}
					}
					return true;
				case TypeCode.UInt32:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Int32 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (uint[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( (int)b[i] );
						}
					}
					return true;
				case TypeCode.UInt64:
					writer.WriteToken ( TokenType.ArrayBegin );
					writer.WriteToken ( TokenType.Type );
					writer.WriteToken ( TokenType.Int64 );
					writer.WriteToken ( TokenType.Count );
					writer.WriteToken ( TokenType.IntEncoded );
					{
						var b = (ulong[])obj;
						writer.Write7BitEncodedInt ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Write ( (long)b[i] );
						}
					}
					return true;

				default:
					return false;
			}
		}

		private static bool WriteListPrimitiveType ( IList obj, Type itemType, BinaryWriter writer )
		{
			//IList<int> muh = new List<int> ();
			return false;
		}

	}

}
