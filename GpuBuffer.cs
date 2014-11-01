using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class GpuBuffer
	{
		private int m_BufferId;

		public GpuBuffer ()
		{
			GL.GenBuffers ( 1, out m_BufferId );
		}

		public void Bind ( BufferTarget type )
		{
			GL.BindBuffer ( type, m_BufferId );
		}

		public void SetData<T> ( BufferTarget type, BufferUsageHint usage, T[] data, int byteSize )
			where T : struct
		{
			GL.BufferData<T> ( type, (IntPtr)byteSize, data, usage );
		}

	}
}
