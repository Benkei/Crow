using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	public class GLBuffer : GLObject
	{
		//todo change this! it's a binding target
		protected BufferTarget m_Type;

		public GLBuffer ( BufferTarget type )
		{
			m_Type = type;
			m_Handler = GL.GenBuffer ();
		}

		public bool IsImmutable
		{
			get
			{
				int value;
				GL.GetBufferParameter ( m_Type, BufferParameterName.BufferImmutableStorage, out value );
				return value == 1;
			}
		}

		public int Size
		{
			get
			{
				int value;
				GL.GetBufferParameter ( m_Type, BufferParameterName.BufferSize, out value );
				return value;
			}
		}

		public BufferUsageHint Usage
		{
			get
			{

				int value;
				GL.GetBufferParameter ( m_Type, BufferParameterName.BufferUsage, out value );
				return (BufferUsageHint)value;
			}
		}

		public bool IsMapped
		{
			get
			{
				int value;
				GL.GetBufferParameter ( m_Type, BufferParameterName.BufferMapped, out value );
				return value == 1;
			}
		}

		public BufferAccess MapAccess
		{
			get
			{
				int value;
				GL.GetBufferParameter ( m_Type, BufferParameterName.BufferAccess, out value );
				return (BufferAccess)value;
			}
		}

		public BufferAccessMask MapAccessMask
		{
			get
			{
				int value;
				GL.GetBufferParameter ( m_Type, BufferParameterName.BufferAccessFlags, out value );
				return (BufferAccessMask)value;
			}
		}

		public long MapOffset
		{
			get
			{
				long value;
				GL.GetBufferParameter ( m_Type, BufferParameterName.BufferMapOffset, out value );
				return value;
			}
		}

		public long MapLength
		{
			get
			{
				long value;
				GL.GetBufferParameter ( m_Type, BufferParameterName.BufferMapLength, out value );
				return value;
			}
		}

		public void Setup ( int size, BufferUsageHint usage )
		{
			GL.BufferData ( m_Type, (IntPtr)size, IntPtr.Zero, usage );
		}

		public void Setup<T> ( int size, T[] data, BufferUsageHint usage )
			where T : struct
		{
			GL.BufferData<T> ( m_Type, (IntPtr)size, data, usage );
		}

		public void Setup ( int size, IntPtr data, BufferUsageHint usage )
		{
			GL.BufferData ( m_Type, (IntPtr)size, data, usage );
		}

		public void Bind ()
		{
			GL.BindBuffer ( m_Type, m_Handler );
		}

		public void Bind ( BufferRangeTarget target, int index )
		{
			GL.BindBufferBase ( target, index, m_Handler );
		}

		public void Bind ( BufferRangeTarget target, int index, int offset, int length )
		{
			GL.BindBufferRange ( target, index, m_Handler, (IntPtr)offset, (IntPtr)length );
		}

		public void Flush ( int offset, int length )
		{
			GL.FlushMappedBufferRange ( m_Type, (IntPtr)offset, (IntPtr)length );
		}

		public IntPtr Map ( BufferAccess access )
		{
			return GL.MapBuffer ( m_Type, access );
		}

		public IntPtr Map ( BufferAccessMask accessMask, int offset, int length )
		{
			return GL.MapBufferRange ( m_Type, (IntPtr)offset, (IntPtr)length, accessMask );
		}

		public bool Unmap ()
		{
			return GL.UnmapBuffer ( m_Type );
		}

		public void Delete ()
		{
			GL.DeleteBuffer ( m_Handler );
		}

		public void SetData<T> ( int offset, int length, T[] data )
			where T : struct
		{
			GL.BufferSubData ( m_Type, (IntPtr)offset, (IntPtr)length, data );
		}

		public void SetData ( int offset, int length, IntPtr data )
		{
			GL.BufferSubData ( m_Type, (IntPtr)offset, (IntPtr)length, data );
		}

		public void GetData<T> ( int offset, int length, T[] data )
			where T : struct
		{
			GL.GetBufferSubData ( m_Type, (IntPtr)offset, (IntPtr)length, data );
		}

		public void GetData ( int offset, int length, IntPtr data )
		{
			GL.GetBufferSubData ( m_Type, (IntPtr)offset, (IntPtr)length, data );
		}

		public void CopyTo ()
		{
			//GL.CopyBufferSubData()
		}
	}
}
