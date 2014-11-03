using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class Mesh
	{
		public int m_Indices;
		public IndicesBuffer m_Ibo;
		public VertexArrayObject m_Vao;
		public GLBuffer m_Vbo;

		public Mesh ()
		{
		}
	}
}
