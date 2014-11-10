using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CrowEditor.Streams
{
	class NativeStream : Stream
	{
		private IntPtr m_Handler;
		private long m_Length;
		private long m_Position;
		private bool m_OwnHandler;

		public NativeStream ( IntPtr handler, int size )
			: this ( handler, size, false )
		{ }

		public NativeStream ( int size )
			: this ( Marshal.AllocHGlobal ( size ), size, true )
		{ }

		private NativeStream ( IntPtr handler, int size, bool ownHandler )
		{
			m_Handler = handler;
			m_Length = size;
			m_OwnHandler = ownHandler;
		}

		public IntPtr Handler
		{
			get { return m_Handler; }
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override long Length
		{
			get { return m_Length; }
		}

		public override long Position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

		public override void Flush ()
		{
		}

		public override long Seek ( long offset, SeekOrigin origin )
		{
			switch ( origin )
			{
				case SeekOrigin.Begin:
					m_Position = offset;
					return m_Position;
				case SeekOrigin.Current:
					m_Position += offset;
					return m_Position;
				case SeekOrigin.End:
					m_Position = m_Length - offset;
					return m_Position;
			}

			throw new ArgumentException ();
		}

		public override int ReadByte ()
		{
			if ( m_Position >= m_Length )
				return -1;
			unsafe
			{
				m_Position++;
				return *((byte*)m_Handler + m_Position - 1);
			}
		}

		public override int Read ( byte[] buffer, int offset, int count )
		{
			if ( buffer == null )
				throw new ArgumentNullException ();
			if ( offset < 0 || count < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( offset + count >= buffer.Length )
				throw new ArgumentOutOfRangeException ();
			unsafe
			{
				fixed ( byte* b = buffer )
				{
					byte* target = b + offset;
					byte* source = (byte*)m_Handler + m_Position;
					offset = 0;
					for ( ; offset < count && m_Position < m_Length; offset++ )
					{
						*target = *source;
						target++;
						source++;
						m_Position++;
					}
					return offset;
				}
			}
		}

		public override void WriteByte ( byte value )
		{
			if ( m_Position >= m_Length )
				throw new ArgumentOutOfRangeException ();
			unsafe
			{
				*((byte*)m_Handler + m_Position) = value;
				m_Position++;
			}
		}

		public override void Write ( byte[] buffer, int offset, int count )
		{
			if ( buffer == null )
				throw new ArgumentNullException ();
			if ( offset < 0 || count < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( offset + count >= buffer.Length )
				throw new ArgumentOutOfRangeException ();
			if ( m_Position + count >= m_Length )
				throw new ArgumentOutOfRangeException ();
			unsafe
			{
				fixed ( byte* b = buffer )
				{
					byte* target = b + offset;
					byte* source = (byte*)m_Handler + m_Position;
					m_Position += count;
					offset = 0;
					for ( ; offset < count; offset++ )
					{
						*source = *target;
						source++;
						target++;
					}
				}
			}
		}

		public override void SetLength ( long value )
		{
			throw new NotSupportedException ();
		}

		protected override void Dispose ( bool disposing )
		{
			base.Dispose ( disposing );
			if ( m_OwnHandler && m_Handler != IntPtr.Zero )
			{
				Marshal.FreeHGlobal ( m_Handler );
			}
			m_Handler = IntPtr.Zero;
		}
	}
}
