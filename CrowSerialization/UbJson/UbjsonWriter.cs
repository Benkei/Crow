using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowSerialization.UbJson
{
	class UbjsonWriter
	{
		private const int LargeByteBufferSize = 256;
		private BinaryWriter m_Writer;
		private Encoding m_encoding;
		private Encoder m_encoder;
		private byte[] m_largeByteBuffer;
		private int m_maxChars;

		public UbjsonWriter ( Stream output, Encoding encoding, bool leaveOpen )
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

		// used for key : value pair objects
		public virtual void WriteObjectBegin ( Token type, int count )
		{
			WriteToken ( Token.ObjectBegin );
			WriteToken ( Token.Type );
			WriteToken ( type ); // todo: need type validation
			WriteToken ( Token.Count );
			WriteIntEncoded ( count );
		}

		public virtual void WriteObjectEnd ()
		{
			WriteToken ( Token.ObjectEnd );
		}

		public virtual void WriteArrayBegin ( Token type, int count )
		{
			WriteToken ( Token.ArrayBegin );
			WriteToken ( Token.Type );
			WriteToken ( type ); // todo: need type validation
			WriteToken ( Token.Count );
			WriteIntEncoded ( count );
		}

		public virtual void WriteArrayEnd ()
		{
			WriteToken ( Token.ArrayEnd );
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
}
