using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SharpDX;

namespace CrowEngine
{
	public class MeshPrimitive
	{
		public struct Vertex
		{
			public Vector3 Position;
			public Vector2 UV;
			public Color Color;

			public Vertex ( Vector3 position, Vector2 uv, Color color )
			{
				Position = position;
				UV = uv;
				Color = color;
			}
		}


		public unsafe static Mesh CreateBox ()
		{
			Vertex* v = stackalloc Vertex[8];
			v[0] = new Vertex ( new Vector3 ( -0.8f, -0.8f, -0.8f ), new Vector2 ( 1, 0 ), new Color ( 1f, 0f, 0f ) );
			v[1] = new Vertex ( new Vector3 ( 0.8f, -0.8f, -0.8f ), new Vector2 ( 0, 0 ), new Color ( 0f, 0f, 1f ) );
			v[2] = new Vertex ( new Vector3 ( 0.8f, 0.8f, -0.8f ), new Vector2 ( 0, 1 ), new Color ( 0f, 1f, 0f ) );
			v[3] = new Vertex ( new Vector3 ( -0.8f, 0.8f, -0.8f ), new Vector2 ( 1, 1 ), new Color ( 1f, 0f, 0f ) );

			v[4] = new Vertex ( new Vector3 ( -0.8f, -0.8f, 0.8f ), new Vector2 ( 1, 0 ), new Color ( 0f, 0f, 1f ) );
			v[5] = new Vertex ( new Vector3 ( 0.8f, -0.8f, 0.8f ), new Vector2 ( 0, 0 ), new Color ( 0f, 1f, 0f ) );
			v[6] = new Vertex ( new Vector3 ( 0.8f, 0.8f, 0.8f ), new Vector2 ( 0, 1 ), new Color ( 1f, 0f, 0f ) );
			v[7] = new Vertex ( new Vector3 ( -0.8f, 0.8f, 0.8f ), new Vector2 ( 1, 1 ), new Color ( 0f, 0f, 1f ) );

			ushort* i = stackalloc ushort[36];
			i[0] = 0;
			i[1] = 7;
			i[2] = 3;
			i[3] = 0;
			i[4] = 4;
			i[5] = 7;
			i[6] = 1;
			i[7] = 2;
			i[8] = 6;
			i[9] = 6;
			i[10] = 5;
			i[11] = 1;
			i[12] = 0;
			i[13] = 2;
			i[14] = 1;
			i[15] = 0;
			i[16] = 3;
			i[17] = 2;
			i[18] = 4;
			i[19] = 5;
			i[20] = 6;
			i[21] = 6;
			i[22] = 7;
			i[23] = 4;
			i[24] = 2;
			i[25] = 3;
			i[26] = 6;
			i[27] = 6;
			i[28] = 3;
			i[29] = 7;
			i[30] = 0;
			i[31] = 1;
			i[32] = 5;
			i[33] = 0;
			i[34] = 5;
			i[35] = 4;

			var mesh = new Mesh ();

			mesh.m_Indices = 36;
			mesh.m_Ibo = new IndicesBuffer ( DrawElementsType.UnsignedShort );
			mesh.m_Ibo.Bind ();
			mesh.m_Ibo.Setup ( 36 * sizeof ( ushort ), (IntPtr)i, BufferUsageHint.StaticDraw );

			mesh.m_Vbo = new GLBuffer ( BufferTarget.ArrayBuffer );
			mesh.m_Vbo.Bind ();
			mesh.m_Vbo.Setup ( 8 * Utilities.SizeOf<Vertex> (), (IntPtr)v, BufferUsageHint.StaticDraw );

			/*
			// bind VBO to VAO
			mesh.m_Vao = VertexArrayObject.Create ();
			mesh.m_Vao.Bind ();

			// Position 3 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.POSITION );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.POSITION,
				3,
				VertexAttribPointerType.Float,
				false,
				Utilities.SizeOf<Vertex> (), 0 );
			// UV 2 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.TEXCOORD );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.TEXCOORD,
				2,
				VertexAttribPointerType.Float,
				true,
				Utilities.SizeOf<Vertex> (), Utilities.SizeOf<Vector3> () );
			// Color 4 byte
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.COLOR );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.COLOR,
				4,
				VertexAttribPointerType.UnsignedByte,
				true,
				Utilities.SizeOf<Vertex> (), Utilities.SizeOf<Vector3> () + Utilities.SizeOf<Vector2> () );

			GL.BindVertexArray ( 0 );
			*/

			return mesh;
		}

	}
}
