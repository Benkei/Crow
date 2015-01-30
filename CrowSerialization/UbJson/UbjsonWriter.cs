using System;
using System.IO;
using System.Text;

namespace CrowSerialization.UbJson
{
	internal class UbjsonWriter
	{
		private const int LargeByteBufferSize = 256;
		private BinaryWriter m_Writer;
		private Encoding m_Encoding;
		private byte[] m_LargeByteBuffer;
		private int m_MaxChars;

		public UbjsonWriter ( Stream output, Encoding encoding )
		{
			m_Writer = new BinaryWriter ( output, encoding );
			m_Encoding = encoding;
		}

		public BinaryWriter Writer
		{
			get { return m_Writer; }
		}

		public virtual void WriteToken ( Token value )
		{
			m_Writer.Write ( (byte)value );
		}

		// used for key : value pair objects
		public virtual void WriteObjectBegin ( Token type, int count )
		{
			WriteToken ( Token.Object );
			WriteToken ( Token.Type );
			WriteToken ( type ); // todo: need type validation
			WriteToken ( Token.Count );
			WriteIntEncoded ( count );
		}

		public virtual void WriteObjectEnd ()
		{
			WriteToken ( Token.End );
		}

		public virtual void WriteArrayBegin ( Token type, int count )
		{
			WriteToken ( Token.Array );
			WriteToken ( Token.Type );
			WriteToken ( type ); // todo: need type validation
			WriteToken ( Token.Count );
			WriteIntEncoded ( count );
		}

		public virtual void WriteArrayEnd ()
		{
			WriteToken ( Token.End );
		}

		public virtual void WritePropertyName ( string name )
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

		public void WriteIntEncoded ( long value )
		{
			ulong num = (ulong)value;
			while ( num >= 128u )
			{
				m_Writer.Write ( (byte)(num | 128u) );
				num >>= 7;
			}
			m_Writer.Write ( (byte)num );
		}

		// writes: length token + string bytes
		public virtual void Write ( string value )
		{
			if ( value == null )
				throw new ArgumentNullException ( "value" );

			int byteCount = m_Encoding.GetByteCount ( value );
			WriteLength ( byteCount );
			if ( m_LargeByteBuffer == null )
			{
				m_LargeByteBuffer = new byte[LargeByteBufferSize];
				m_MaxChars = LargeByteBufferSize / m_Encoding.GetMaxByteCount ( 1 );
			}
			if ( byteCount <= LargeByteBufferSize )
			{
				m_Encoding.GetBytes ( value, 0, value.Length, m_LargeByteBuffer, 0 );
				m_Writer.Write ( m_LargeByteBuffer, 0, byteCount );
				return;
			}

			int offset = 0, count;
			for ( int i = value.Length; i > 0; i -= count )
			{
				count = (i > m_MaxChars) ? m_MaxChars : i;
				int bytes = m_Encoding.GetBytes ( value, offset, count, m_LargeByteBuffer, LargeByteBufferSize );
				m_Writer.Write ( m_LargeByteBuffer, 0, bytes );
				offset += count;
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
}