using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	public struct VertexArrayObject
	{
		int m_Handler;

		private VertexArrayObject ( int handler )
		{
			m_Handler = handler;
		}

		public void Bind ()
		{
			GL.BindVertexArray ( m_Handler );
		}

		public void Delete ()
		{
			GL.DeleteVertexArray ( m_Handler );
		}

		public static VertexArrayObject Create ()
		{
			return new VertexArrayObject ( GL.GenVertexArray () );
		}
	}
}
