using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class GpuProgram
	{
		private int m_ProgramId;

		private int m_FragmentShader;
		private int m_VertexShader;
		private int m_GeometryShader;
		private int m_TessEvaluationShader;
		private int m_TessControlShader;
		private int m_ComputeShader;

		public GpuProgram ()
		{
			m_ProgramId = GL.CreateProgram ();
		}

		public void LoadShader ( ShaderType type, string sourceText )
		{
			int shaderId = GL.CreateShader ( type );
			GL.ShaderSource ( shaderId, sourceText );

			GL.CompileShader ( shaderId );

			GL.AttachShader ( m_ProgramId, shaderId );

			var info = GL.GetShaderInfoLog ( shaderId );
			if ( !string.IsNullOrWhiteSpace ( info ) )
				Console.WriteLine ( sourceText + "\n" + info );
			info = GL.GetProgramInfoLog ( m_ProgramId );
			if ( !string.IsNullOrWhiteSpace ( info ) )
				Console.WriteLine ( info );

			switch ( type )
			{
				case ShaderType.ComputeShader:
					m_ComputeShader = shaderId;
					break;
				case ShaderType.FragmentShader:
					m_FragmentShader = shaderId;
					break;
				case ShaderType.GeometryShader:
					m_GeometryShader = shaderId;
					break;
				case ShaderType.TessControlShader:
					m_TessControlShader = shaderId;
					break;
				case ShaderType.TessEvaluationShader:
					m_TessEvaluationShader = shaderId;
					break;
				case ShaderType.VertexShader:
					m_VertexShader = shaderId;
					break;
			}
		}

		public void Init ()
		{
			GL.LinkProgram ( m_ProgramId );

			Console.WriteLine ( GL.GetProgramInfoLog ( m_ProgramId ) );

			int attributeCount;
			int UniformCount;

			GL.GetProgram ( m_ProgramId, GetProgramParameterName.ActiveAttributes, out attributeCount );
			GL.GetProgram ( m_ProgramId, GetProgramParameterName.ActiveUniforms, out UniformCount );

			StringBuilder nameBuffer = new StringBuilder ( 256 );
			for ( int i = 0; i < attributeCount; i++ )
			{
				int size;
				ActiveAttribType type;
				int length;
				nameBuffer.Length = 0;

				GL.GetActiveAttrib ( m_ProgramId, i, 256, out length, out size, out type, nameBuffer );

				var name = nameBuffer.ToString ();
				var address = GL.GetAttribLocation ( m_ProgramId, name );
			}

			for ( int i = 0; i < UniformCount; i++ )
			{
				int size;
				ActiveUniformType type;
				int length;

				GL.GetActiveUniform ( m_ProgramId, i, 256, out length, out size, out type, nameBuffer );

				var name = nameBuffer.ToString ();
				var address = GL.GetUniformLocation ( m_ProgramId, name );
			}
		}

		public void Link ()
		{
			GL.LinkProgram ( m_ProgramId );
		}

		public int GetAttributeLocation ( string name )
		{
			return GL.GetAttribLocation ( m_ProgramId, name );
		}

		public int GetUniformLocation ( string name )
		{
			return GL.GetUniformLocation ( m_ProgramId, name );
		}
	}
}
