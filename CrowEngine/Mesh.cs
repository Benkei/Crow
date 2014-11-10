using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SharpDX;

namespace CrowEngine
{
	public class Mesh
	{
		public int m_Indices;
		public IndicesBuffer m_Ibo;
		public VertexArrayObject m_Vao;
		public GLBuffer m_Vbo;

		public Mesh ()
		{
		}

		public void CheckVao ()
		{
			if ( m_Vao.IsValid ) return;

			m_Vbo.Bind ();

			// bind VBO to VAO
			m_Vao = VertexArrayObject.Create ();
			m_Vao.Bind ();

			// Position 3 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.POSITION );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.POSITION,
				3,
				VertexAttribPointerType.Float,
				false,
				Utilities.SizeOf<MeshPrimitive.Vertex> (), 0 );
			// UV 2 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.TEXCOORD );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.TEXCOORD,
				2,
				VertexAttribPointerType.Float,
				true,
				Utilities.SizeOf<MeshPrimitive.Vertex> (), Utilities.SizeOf<Vector3> () );
			// Color 4 byte
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.COLOR );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.COLOR,
				4,
				VertexAttribPointerType.UnsignedByte,
				true,
				Utilities.SizeOf<MeshPrimitive.Vertex> (), Utilities.SizeOf<Vector3> () + Utilities.SizeOf<Vector2> () );

			GL.BindVertexArray ( 0 );
		}
	}
}
