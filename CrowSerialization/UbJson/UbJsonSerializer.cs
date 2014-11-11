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
		NoOp = (byte)'N', // 1 byte

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

		IntVL = (byte)'z', // int variable length; 2 - 10 bytes

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
		RefType = (byte)'!', // 1 byte + length token + string char; reflection type 
	}

	static class UbJsonEx
	{
		public static void WriteToken ( this BinaryWriter writer, Token token )
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

	class UbJsonSerializer
	{
		public void Serialize ( object obj, Stream stream )
		{
			var writer = new UbJsonWriter ( stream, Encoding.UTF8, true );
			WriteValue ( obj, writer, 0 );
		}

		public object Deserialize ( Type instanceType, Stream stream )
		{
			var reader = new UbJsonReader ( stream, Encoding.UTF8, true );

			reader.Read ();

			return ReadValue ( null, instanceType, reader );
		}


		private object ReadValue ( object obj, Type objType, UbJsonReader reader )
		{
			object instance = null;
			if ( reader.CurrentTokenType == TokenType.Value )
			{
				var value = reader.ReadValue ();
				if ( objType.IsEnum )
				{
					value = Enum.ToObject ( objType, value );
				}
				return value;
			}
			if ( reader.CurrentTokenType == TokenType.Property )
			{
				return reader.ReadString ();
			}
			if ( reader.CurrentTokenType == TokenType.ObjectBegin || reader.CurrentTokenType == TokenType.ArrayBegin )
			{
				Token? valueType = null;
				int? valueCount = null;

				reader.Read ();
				if ( reader.CurrentTokenType == TokenType.OptionType )
				{
					valueType = reader.CurrentToken;
					// read next count
					reader.Read ();
				}
				if ( reader.CurrentTokenType == TokenType.OptionCount )
				{
					valueCount = reader.ReadLength ();
				}

				if ( reader.CurrentTokenType == TokenType.ObjectBegin )
				{
					// todo add optimization case
					// http://ubjson.org/type-reference/container-types/#optimized-format-example-object

					#region Dict
					if ( typeof ( IDictionary ).IsAssignableFrom ( objType ) )
					{
						instance = obj != null ? obj : Activator.CreateInstance ( objType );
						if ( ((IDictionary)instance).IsReadOnly )
						{
							return null;
						}

						((IDictionary)instance).Clear ();

						ObjectMetadata metadata;
						ReflectionCache.GetObjectMetadata ( objType, out metadata );

						while ( true )
						{
							var name = reader.ReadString ();

							PropertyMetadata data;
							if ( metadata.properties.TryGetValue ( name, out data ) )
							{
								var value = ReadValue ( data.GetValue ( instance ), data.Type, reader );
								data.SetValue ( instance, value );
							}
							else
							{
								// todo add skip value
							}

							reader.Read ();
							if ( reader.CurrentTokenType == TokenType.ObjectEnd )
							{
								break;
							}
						}

						return instance;
					}
					#endregion

					instance = obj != null ? obj : Activator.CreateInstance ( objType );

					while ( true )
					{
						var name = reader.ReadString ();

						//PropertyMetadata data;
						//if ( metadata.properties.TryGetValue ( name, out data ) )
						//{
						//	var value = ReadValue ( data.GetValue ( instance ), data.Type, reader );
						//	data.SetValue ( instance, value );
						//}
						//else
						//{
						//	// todo add skip value
						//}

						reader.Read ();
						if ( reader.CurrentTokenType == TokenType.ObjectEnd )
						{
							break;
						}
					}

					return instance;
				}
				else
				{
					// array case

					// Primitive case
					if ( valueType.HasValue )
					{
						if ( !valueCount.HasValue )
						{
							throw new System.IO.InvalidDataException ();
						}

						// fix size array
						if ( objType.IsArray )
						{
							var element = objType.GetElementType ();
							Array array = (Array)obj;
							if ( array.Length != valueCount.Value )
								array = Array.CreateInstance ( element, valueCount.Value );

							if ( element.IsPrimitive )
							{
								// todo fast generic
								for ( int i = 0; i < array.Length; i++ )
								{
									var value = reader.ReadValueByToken ( valueType.Value );
									value = Convert.ChangeType ( value, element );
									array.SetValue ( value, i );
								}
							}
							else
							{
								for ( int i = 0; i < array.Length; i++ )
								{
									var value = ReadValue ( array.GetValue ( i ), element, reader );
									array.SetValue ( value, i );
								}
							}

							return array;
						}
						// IList interface
						if ( typeof ( IList ).IsAssignableFrom ( objType ) )
						{
							if ( ((IList)instance).IsReadOnly )
							{
								return null;
							}
							if ( objType.IsGenericType && objType.GetGenericTypeDefinition () == typeof ( IList<> ) )
							{
								// todo fast generic
							}

							if ( obj != null )
							{
								instance = obj;
								((IList)instance).Clear ();
							}
							else
							{
								instance = Activator.CreateInstance ( objType );
							}

							for ( int i = 0; i < valueCount.Value; i++ )
							{
								var value = ReadValue ( null, null, reader );
								((IList)instance).Add ( value );
							}
						}
					}


				}
			}


			switch ( reader.CurrentTokenType )
			{
				case TokenType.Value: return reader.ReadValue ();
				case TokenType.Property: return reader.ReadString ();

				case TokenType.ObjectBegin:
					break;
				case TokenType.ObjectEnd:
					break;

				case TokenType.ArrayBegin:
					break;
				case TokenType.ArrayEnd:
					break;

				case TokenType.OptionType:
					break;
				case TokenType.OptionCount:
					break;
				case TokenType.OptionValue:
					break;
				default:
					break;
			}

			return null;
		}


		private void WriteValue ( object obj, UbJsonWriter writer, int depth )
		{
			if ( depth > 100 )
				throw new Exception ( string.Format ( "Max allowed object depth reached while trying to export from type {0}", obj.GetType () ) );

			var objType = obj.GetType ();

			// add user defined converter

			switch ( Type.GetTypeCode ( objType ) )
			{
				case TypeCode.Boolean: writer.WriteValue ( (bool)obj ); break;
				case TypeCode.Byte: writer.WriteValue ( (byte)obj ); break;
				case TypeCode.Char:
					if ( (char)obj <= byte.MaxValue )
						goto case TypeCode.Byte;
					else
						goto case TypeCode.Int16;
				case TypeCode.DateTime: writer.WriteValue ( ((DateTime)obj).ToBinary () ); break;
				case TypeCode.Decimal: writer.WriteValue ( (decimal)obj ); break;
				case TypeCode.Double: writer.WriteValue ( (double)obj ); break;
				case TypeCode.Empty: writer.WriteToken ( Token.Null ); break;
				case TypeCode.Int16: writer.WriteValue ( (short)obj ); break;
				case TypeCode.Int32: writer.WriteValue ( (int)obj ); break;
				case TypeCode.Int64: writer.WriteValue ( (long)obj ); break;

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
					if ( obj is IDictionary )
					{
						if ( ((IDictionary)obj).IsReadOnly )
						{
							break;
						}
						// todo add optimization case
						// http://ubjson.org/type-reference/container-types/#optimized-format-example-object

						writer.WriteToken ( Token.ObjectBegin );
						foreach ( IDictionaryEnumerator pair in (IDictionary)obj )
						{
							writer.WriteProperty ( (string)pair.Key );
							WriteValue ( pair.Value, writer, depth + 1 );
						}
						writer.WriteToken ( Token.ObjectEnd );
						break;
					}

					// write as object

					Dictionary<string, PropertyMetadata> meta;
					ReflectionCache.GetTypeProperties ( objType, out meta );

					writer.WriteToken ( Token.ObjectBegin );
					foreach ( var info in meta.Values )
					{
						writer.WriteProperty ( info.Info.Name );
						WriteValue ( info.GetValue ( obj ), writer, depth + 1 );
					}
					writer.WriteToken ( Token.ObjectEnd );
					break;

				case TypeCode.SByte: writer.WriteValue ( (sbyte)obj ); break;
				case TypeCode.Single: writer.WriteValue ( (float)obj ); break;
				case TypeCode.String: writer.WriteValue ( (string)obj ); break;
				case TypeCode.UInt16: writer.WriteValue ( (ushort)obj ); break;
				case TypeCode.UInt32: writer.WriteValue ( (uint)obj ); break;
				case TypeCode.UInt64: writer.WriteValue ( (ulong)obj ); break;
			}
		}


		private bool WriteArrayPrimitiveType ( Array obj, Type objType, UbJsonWriter writer )
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
					goto case TypeCode.Int16;
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
						var b = (char[])obj;
						writer.WriteLength ( b.Length );
						for ( i = 0; i < b.Length; i++ )
						{
							writer.Writer.Write ( (short)b[i] );
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

		private bool WriteListPrimitiveType ( IList obj, Type itemType, UbJsonWriter writer )
		{
			//IList<int> muh = new List<int> ();
			return false;
		}

	}


	enum TokenType : byte
	{
		None,

		Value,

		Property,

		ObjectBegin,
		ObjectEnd,

		ArrayBegin,
		ArrayEnd,

		OptionType,
		OptionCount,
		OptionValue,
	}


	class UbJsonWriter
	{
		private const int LargeByteBufferSize = 256;
		private BinaryWriter m_Writer;
		private Encoding m_encoding;
		private Encoder m_encoder;
		private byte[] m_largeByteBuffer;
		private int m_maxChars;

		public UbJsonWriter ( Stream output, Encoding encoding, bool leaveOpen )
		{
			m_Writer = new BinaryWriter ( output, encoding, leaveOpen );
			m_encoding = encoding;
			m_encoder = encoding.GetEncoder ();
		}

		public BinaryWriter Writer
		{
			get { return m_Writer; }
		}


		public virtual void WriteToken ( Token value )
		{
			m_Writer.Write ( (byte)value );
		}

		public virtual void WriteProperty ( string name )
		{
			Write ( name );
		}

		public virtual void WriteValue ( bool value )
		{
			m_Writer.Write ( (byte)(value ? Token.True : Token.False) );
		}
		public virtual void WriteValue ( byte value )
		{
			m_Writer.Write ( (byte)Token.Int8 );
			m_Writer.Write ( value );
		}
		public virtual void WriteValue ( char ch )
		{
			m_Writer.Write ( (byte)Token.Int16 );
			m_Writer.Write ( (short)ch );
		}
		public virtual void WriteValue ( decimal value )
		{
			m_Writer.Write ( (byte)Token.Float128 );
			m_Writer.Write ( value );
		}
		public virtual void WriteValue ( double value )
		{
			m_Writer.Write ( (byte)Token.Float64 );
			m_Writer.Write ( value );
		}
		public virtual void WriteValue ( float value )
		{
			m_Writer.Write ( (byte)Token.Float32 );
			m_Writer.Write ( value );
		}
		public virtual void WriteValue ( int value )
		{
			m_Writer.Write ( (byte)Token.Int32 );
			m_Writer.Write ( value );
		}
		public virtual void WriteValue ( long value )
		{
			m_Writer.Write ( (byte)Token.Int64 );
			m_Writer.Write ( value );
		}
		public virtual void WriteValue ( sbyte value )
		{
			m_Writer.Write ( (byte)Token.Int8 );
			m_Writer.Write ( value );
		}
		public virtual void WriteValue ( short value )
		{
			m_Writer.Write ( (byte)Token.Int16 );
			m_Writer.Write ( value );
		}
		public virtual void WriteValue ( string value )
		{
			if ( value == null )
				throw new ArgumentNullException ( "value" );

			m_Writer.Write ( (byte)Token.String );

			Write ( value );
		}
		public virtual void WriteValue ( uint value )
		{
			m_Writer.Write ( (byte)Token.Int32 );
			m_Writer.Write ( (int)value );
		}
		public virtual void WriteValue ( ulong value )
		{
			m_Writer.Write ( (byte)Token.Int64 );
			m_Writer.Write ( (long)value );
		}
		public virtual void WriteValue ( ushort value )
		{
			m_Writer.Write ( (byte)Token.Int16 );
			m_Writer.Write ( (short)value );
		}

		// writes: length token + string bytes
		public unsafe virtual void Write ( string value )
		{
			if ( value == null )
				throw new ArgumentNullException ( "value" );

			int byteCount = m_encoding.GetByteCount ( value );
			WriteLength ( byteCount );
			if ( m_largeByteBuffer == null )
			{
				m_largeByteBuffer = new byte[LargeByteBufferSize];
				m_maxChars = LargeByteBufferSize / m_encoding.GetMaxByteCount ( 1 );
			}
			if ( byteCount <= LargeByteBufferSize )
			{
				m_encoding.GetBytes ( value, 0, value.Length, m_largeByteBuffer, 0 );
				m_Writer.Write ( m_largeByteBuffer, 0, byteCount );
				return;
			}

			fixed ( char* ptr = value )
			fixed ( byte* largeByteBuffer = m_largeByteBuffer )
			{
				int num = 0;
				int num2;
				for ( int i = value.Length; i > 0; i -= num2 )
				{
					num2 = (i > m_maxChars) ? m_maxChars : i;
					int bytes;
					bytes = m_encoder.GetBytes ( ptr + num, num2, largeByteBuffer, LargeByteBufferSize, num2 == i );
					m_Writer.Write ( m_largeByteBuffer, 0, bytes );
					num += num2;
				}
			}
		}

		// writes token (uint8, int16 or int32) based on the length value
		public void WriteLength ( int length )
		{
			if ( length <= byte.MaxValue )
			{
				m_Writer.Write ( (byte)Token.Int8 );
				m_Writer.Write ( (byte)length );
			}
			else if ( length <= short.MaxValue )
			{
				m_Writer.Write ( (byte)Token.Int16 );
				m_Writer.Write ( (short)length );
			}
			else if ( length <= int.MaxValue )
			{
				m_Writer.Write ( (byte)Token.Int32 );
				m_Writer.Write ( (int)length );
			}
		}
	}


	class UbJsonReader
	{
		private const int MaxCharBytesSize = 128;
		private Decoder m_decoder;
		private int m_maxCharsSize;
		private byte[] m_charBytes;
		private char[] m_charBuffer;
		private StringBuilder stringBuilder;
		private BinaryReader m_Reader;

		public UbJsonReader ( Stream input, Encoding encoding, bool leaveOpen )
		{
			m_Reader = new BinaryReader ( input, encoding, leaveOpen );
			m_decoder = encoding.GetDecoder ();
			m_maxCharsSize = encoding.GetMaxCharCount ( 128 );
		}


		public TokenType CurrentTokenType
		{
			get;
			private set;
		}

		public Token CurrentToken
		{
			get;
			private set;
		}


		public bool Read ()
		{
			var token = (Token)m_Reader.ReadByte ();

			switch ( token )
			{
				case Token.ArrayBegin:
					CurrentTokenType = TokenType.ArrayBegin;
					break;
				case Token.ArrayEnd:
					CurrentTokenType = TokenType.ArrayEnd;
					break;
				case Token.ObjectBegin:
					CurrentTokenType = TokenType.ObjectBegin;
					break;
				case Token.ObjectEnd:
					CurrentTokenType = TokenType.ObjectEnd;
					break;

				case Token.Type:
					CurrentTokenType = TokenType.OptionType;
					break;
				case Token.Count:
					CurrentTokenType = TokenType.OptionCount;
					break;

				default:
					// is value
					if ( CurrentTokenType == TokenType.OptionType || CurrentTokenType == TokenType.OptionCount )
					{
						CurrentTokenType = TokenType.OptionValue;
					}
					else if ( CurrentTokenType >= TokenType.ObjectBegin )
					{
						CurrentTokenType = TokenType.Property;
					}
					else if ( CurrentTokenType == TokenType.Property )
					{
						CurrentTokenType = TokenType.Value;
					}
					else
					{
						CurrentTokenType = TokenType.Property;
					}
					break;
			}

			CurrentToken = token;

			return true;
		}


		public object ReadValue ()
		{
			return ReadValueByToken ( CurrentToken );
		}

		public object ReadValueByToken ( Token token )
		{
			switch ( token )
			{
				case Token.Null: return null;
				case Token.NoOp: return null;
				case Token.True: return true;
				case Token.False: return false;
				case Token.Int8: return m_Reader.ReadSByte ();
				case Token.Int16: return m_Reader.ReadInt16 ();
				case Token.Int32: return m_Reader.ReadInt32 ();
				case Token.Int64: return m_Reader.ReadInt64 ();
				case Token.Float32: return m_Reader.ReadSingle ();
				case Token.Float64: return m_Reader.ReadDouble ();

				case Token.String: return ReadString ();

				case Token.ArrayBegin:
				case Token.ArrayEnd:
				case Token.ObjectBegin:
				case Token.ObjectEnd:
				case Token.Type:
				case Token.Count:
					throw new InvalidOperationException ();

				case Token.Float128: return m_Reader.ReadDecimal ();
			}
			throw new NotImplementedException ();
		}


		public int ReadLength ()
		{
			Token type = (Token)m_Reader.ReadByte ();
			switch ( type )
			{
				case Token.Int8: return m_Reader.ReadByte ();
				case Token.Int16: return m_Reader.ReadInt16 ();
				case Token.Int32: return m_Reader.ReadInt32 ();

				default: throw new InvalidDataException ();
			}
		}

		public string ReadString ()
		{
			int length = ReadLength ();
			if ( length < 0 )
				throw new IOException ();
			if ( length == 0 )
				return string.Empty;
			if ( m_charBytes == null )
				m_charBytes = new byte[MaxCharBytesSize];
			if ( m_charBuffer == null )
				m_charBuffer = new char[m_maxCharsSize];
			if ( stringBuilder != null )
			{
				stringBuilder.Length = 0;
				stringBuilder.EnsureCapacity ( length );
			}
			int offset = 0;
			while ( true )
			{
				int count = (length - offset > MaxCharBytesSize) ? MaxCharBytesSize : (length - offset);
				int readed = m_Reader.Read ( m_charBytes, 0, count );
				if ( readed == 0 )
					throw new EndOfStreamException ();
				int chars = m_decoder.GetChars ( m_charBytes, 0, readed, m_charBuffer, 0 );
				if ( offset == 0 && readed == length )
				{
					return new string ( m_charBuffer, 0, chars );
				}
				if ( stringBuilder == null )
					stringBuilder = new StringBuilder ( length );
				stringBuilder.Append ( m_charBuffer, 0, chars );
				offset += readed;
				if ( offset >= length )
				{
					var str = stringBuilder.ToString ();
					stringBuilder.Length = 0;
					return str;
				}
			}
		}


	}

}
