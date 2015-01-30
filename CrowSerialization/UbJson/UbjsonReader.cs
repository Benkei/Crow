using System;
using System.IO;
using System.Text;

namespace CrowSerialization.UbJson
{
	internal class UbjsonReader
	{
		private const int MaxCharBytesSize = 128;
		private BinaryReader m_Reader;
		private Decoder m_Decoder;
		private int m_MaxCharsSize;
		private byte[] m_CharBytes;
		private char[] m_CharBuffer;
		private StringBuilder m_StringBuilder;

		public UbjsonReader ( Stream input, Encoding encoding )
		{
			if ( !input.CanSeek )
				throw new System.IO.IOException ();

			m_Reader = new BinaryReader ( input, encoding );
			m_Decoder = encoding.GetDecoder ();
			m_MaxCharsSize = encoding.GetMaxCharCount ( MaxCharBytesSize );
		}

		public Token CurrentToken
		{
			get;
			private set;
		}

		public void Seek ( int offset )
		{
			if ( offset != 0 )
				m_Reader.BaseStream.Position += offset;
		}

		public Token ReadToken ()
		{
			return CurrentToken = (Token)m_Reader.ReadByte ();
		}

		public Token PeekToken ()
		{
			var value = m_Reader.PeekChar ();
			return value < 0 ? Token.None : (Token)value;
		}

		public bool PeekReadToken ( Token isToken )
		{
			var value = (Token)m_Reader.PeekChar ();
			if ( value == isToken )
			{
				CurrentToken = value;
				m_Reader.BaseStream.Position += 1;
				return true;
			}
			return false;
		}

		public void Reset ()
		{
		}

		public string ReadString ()
		{
			int length = ReadLength ();
			if ( length < 0 )
				throw new IOException ();
			if ( length == 0 )
				return string.Empty;
			if ( m_CharBytes == null )
				m_CharBytes = new byte[MaxCharBytesSize];
			if ( m_CharBuffer == null )
				m_CharBuffer = new char[m_MaxCharsSize];
			if ( m_StringBuilder != null )
			{
				m_StringBuilder.Length = 0;
				m_StringBuilder.EnsureCapacity ( length );
			}
			int offset = 0;
			while ( true )
			{
				int count = (length - offset > MaxCharBytesSize) ? MaxCharBytesSize : (length - offset);
				int readed = m_Reader.Read ( m_CharBytes, 0, count );
				if ( readed == 0 )
					throw new EndOfStreamException ();
				int chars = m_Decoder.GetChars ( m_CharBytes, 0, readed, m_CharBuffer, 0 );
				if ( offset == 0 && readed == length )
				{
					return new string ( m_CharBuffer, 0, chars );
				}
				if ( m_StringBuilder == null )
					m_StringBuilder = new StringBuilder ( length );
				m_StringBuilder.Append ( m_CharBuffer, 0, chars );
				offset += readed;
				if ( offset >= length )
				{
					var str = m_StringBuilder.ToString ();
					m_StringBuilder.Length = 0;
					return str;
				}
			}
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
				//case Token.NoOp: return null;
				case Token.True: return true;
				case Token.False: return false;
				case Token.Int8: return m_Reader.ReadSByte ();
				case Token.Int16: return m_Reader.ReadInt16 ();
				case Token.Int32: return m_Reader.ReadInt32 ();
				case Token.Int64: return m_Reader.ReadInt64 ();
				case Token.Float32: return m_Reader.ReadSingle ();
				case Token.Float64: return m_Reader.ReadDouble ();

				case Token.String:
					ReadToken ();
					return ReadString ();

				case Token.Array:
				case Token.Object:
				case Token.End:
				case Token.Type:
				case Token.Count:
					throw new InvalidOperationException ();

				case Token.Float128: return m_Reader.ReadDecimal ();
			}
			throw new NotImplementedException ();
		}

		public int ReadLength ()
		{
			switch ( CurrentToken )
			{
				case Token.Int8: return m_Reader.ReadByte ();
				case Token.Int16: return m_Reader.ReadInt16 ();
				case Token.Int32: return m_Reader.ReadInt32 ();

				default: throw new InvalidOperationException ();
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

		public void SkipPropertyName ()
		{
			Seek ( ReadLength () );
		}

		public void SkipRead ()
		{
			SkipReadByToken ( CurrentToken );
		}

		public void SkipReadByToken ( Token token )
		{
			switch ( token )
			{
				case Token.Null: return;
				//case Token.NoOp: return null;
				case Token.True: return;
				case Token.False: return;
				case Token.Int8: Seek ( 1 ); return;
				case Token.Int16: Seek ( 2 ); return;
				case Token.Int32: Seek ( 4 ); return;
				case Token.Int64: Seek ( 8 ); return;
				case Token.Float32: Seek ( 4 ); return;
				case Token.Float64: Seek ( 8 ); return;
				case Token.Float128: Seek ( 16 ); return;

				case Token.String:
					ReadToken ();
					SkipPropertyName ();
					return;

				case Token.Array:
				case Token.Object:
				case Token.End:
				case Token.Type:
				case Token.Count:
					throw new InvalidOperationException ();
			}
			throw new NotImplementedException ();
		}
	}
}