using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class GpuBuffer : BaseHandler
	{
		protected BufferTarget m_Type;

		public GpuBuffer ( BufferTarget type )
		{
			m_Type = type;
			GL.GenBuffers ( 1, out m_Handler );
		}

		public void Bind ()
		{
			GL.BindBuffer ( m_Type, m_Handler );
		}

		public void SetData<T> ( BufferUsageHint usage, T[] data, int byteSize )
			where T : struct
		{
			GL.BufferData<T> ( m_Type, (IntPtr)byteSize, data, usage );
		}

		public void SetData ( BufferUsageHint usage, IntPtr data, int byteSize )
		{
			GL.BufferData ( m_Type, (IntPtr)byteSize, data, usage );
		}

	}
}
