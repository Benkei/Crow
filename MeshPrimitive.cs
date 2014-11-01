using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SharpDX;

namespace CrowEngine
{
	class MeshPrimitive
	{
		struct Vertex
		{
			public Vector3 Position;
			public Color Color;

			public Vertex ( Vector3 position, Color color )
			{
				Position = position;
				Color = color;
			}
		}


		public unsafe static Mesh CreateBox ()
		{
			Vertex* block = stackalloc Vertex[8];
			block[0] = new Vertex ( new Vector3 ( -0.8f, -0.8f, -0.8f ), new Color ( 1f, 0f, 0f ) );
			block[1] = new Vertex ( new Vector3 ( 0.8f, -0.8f, -0.8f ), new Color ( 0f, 0f, 1f ) );
			block[2] = new Vertex ( new Vector3 ( 0.8f, 0.8f, -0.8f ), new Color ( 0f, 1f, 0f ) );
			block[3] = new Vertex ( new Vector3 ( -0.8f, 0.8f, -0.8f ), new Color ( 1f, 0f, 0f ) );
			block[4] = new Vertex ( new Vector3 ( -0.8f, -0.8f, 0.8f ), new Color ( 0f, 0f, 1f ) );
			block[5] = new Vertex ( new Vector3 ( 0.8f, -0.8f, 0.8f ), new Color ( 0f, 1f, 0f ) );
			block[6] = new Vertex ( new Vector3 ( 0.8f, 0.8f, 0.8f ), new Color ( 1f, 0f, 0f ) );
			block[7] = new Vertex ( new Vector3 ( -0.8f, 0.8f, 0.8f ), new Color ( 0f, 0f, 1f ) );

			byte* indices = stackalloc byte[36];
			indices[0] = 0;
			indices[1] = 7;
			indices[2] = 3;
			indices[3] = 0;
			indices[4] = 4;
			indices[5] = 7;
			indices[6] = 1;
			indices[7] = 2;
			indices[8] = 6;
			indices[9] = 6;
			indices[10] = 5;
			indices[11] = 1;
			indices[12] = 0;
			indices[13] = 2;
			indices[14] = 1;
			indices[15] = 0;
			indices[16] = 3;
			indices[17] = 2;
			indices[18] = 4;
			indices[19] = 5;
			indices[20] = 6;
			indices[21] = 6;
			indices[22] = 7;
			indices[23] = 4;
			indices[24] = 2;
			indices[25] = 3;
			indices[26] = 6;
			indices[27] = 6;
			indices[28] = 3;
			indices[29] = 7;
			indices[30] = 0;
			indices[31] = 1;
			indices[32] = 5;
			indices[33] = 0;
			indices[34] = 5;
			indices[35] = 4;

			var mesh = new Mesh ();

			mesh.m_Indices = 36;
			mesh.m_Ibo = new IndicesBuffer ( DrawElementsType.UnsignedByte );
			mesh.m_Ibo.Bind ();
			mesh.m_Ibo.SetData ( BufferUsageHint.StaticDraw, (IntPtr)indices, 36 );

			mesh.m_Vbo = new GpuBuffer ( BufferTarget.ArrayBuffer );
			mesh.m_Vbo.Bind ();
			mesh.m_Vbo.SetData ( BufferUsageHint.StaticDraw, (IntPtr)block, Utilities.SizeOf<Vertex> () * 8 );

			// bind VBO to VAO
			mesh.m_Vao = VertexArrayObject.Create ();
			mesh.m_Vao.Bind ();
			
			// Position 3 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.POSITION );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.POSITION, 3, VertexAttribPointerType.Float, false, Utilities.SizeOf<Vertex> (), 0 );
			// Color 4 byte
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.COLOR );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.COLOR, 4, VertexAttribPointerType.UnsignedByte, true, Utilities.SizeOf<Vertex> (), Utilities.SizeOf<Vector3> () );

			GL.BindVertexArray ( 0 );

			return mesh;
		}

	}
}
