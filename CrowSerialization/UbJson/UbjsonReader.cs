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
			if ( !input.CanSeek )
				throw new System.IO.IOException ();

			m_Reader = new BinaryReader ( input, encoding, leaveOpen );
			m_decoder = encoding.GetDecoder ();
			m_maxCharsSize = encoding.GetMaxCharCount ( 128 );
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
