using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowSerialization.UbJson
{
	class UbjsonReader
	{
		private const int MaxCharBytesSize = 128;
		private Decoder m_decoder;
		private int m_maxCharsSize;
		private byte[] m_charBytes;
		private char[] m_charBuffer;
		private StringBuilder stringBuilder;
		private BinaryReader m_Reader;

		public UbjsonReader ( Stream input, Encoding encoding, bool leaveOpen )
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

		public long ReadIntEncoded ()
		{
			long value = 0;
			int offset = 0;
			while ( offset != 35 )
			{
				byte b = m_Reader.ReadByte ();
				value |= (long)(b & 127) << offset;
				offset += 7;
				if ( (b & 128) == 0 )
				{
					return value;
				}
			}
			throw new FormatException ();
		}
	}
}
