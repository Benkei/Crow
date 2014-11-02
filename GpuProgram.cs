using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class GpuProgram : BaseHandler
	{
		private GpuShader m_FragmentShader;
		private GpuShader m_VertexShader;
		private GpuShader m_GeometryShader;
		private GpuShader m_TessEvaluationShader;
		private GpuShader m_TessControlShader;
		private GpuShader m_ComputeShader;

		//private Dictionary<string, object> 

		public GpuProgram ()
		{
			m_Handler = GL.CreateProgram ();
		}

		public bool IsLinked
		{
			get
			{
				int status;
				GL.GetProgram ( m_Handler, GetProgramParameterName.LinkStatus, out status );
				return status == 1;
			}
		}

		public GpuShader GetShader ( ShaderType type )
		{
			switch ( type )
			{
				case ShaderType.ComputeShader:
					return m_ComputeShader;
				case ShaderType.FragmentShader:
					return m_FragmentShader;
				case ShaderType.GeometryShader:
					return m_GeometryShader;
				case ShaderType.TessControlShader:
					return m_TessControlShader;
				case ShaderType.TessEvaluationShader:
					return m_TessEvaluationShader;
				case ShaderType.VertexShader:
					return m_VertexShader;
				default: throw new ArgumentOutOfRangeException ();
			}
		}

		public void SetShader ( GpuShader shader )
		{
			GpuShader oldShader = null;
			switch ( shader.Type )
			{
				case ShaderType.ComputeShader:
					oldShader = m_ComputeShader;
					m_ComputeShader = shader;
					break;
				case ShaderType.FragmentShader:
					oldShader = m_FragmentShader;
					m_FragmentShader = shader;
					break;
				case ShaderType.GeometryShader:
					oldShader = m_GeometryShader;
					m_GeometryShader = shader;
					break;
				case ShaderType.TessControlShader:
					oldShader = m_TessControlShader;
					m_TessControlShader = shader;
					break;
				case ShaderType.TessEvaluationShader:
					oldShader = m_TessEvaluationShader;
					m_TessEvaluationShader = shader;
					break;
				case ShaderType.VertexShader:
					oldShader = m_VertexShader;
					m_VertexShader = shader;
					break;
			}

			if ( oldShader != shader && oldShader != null )
				GL.DetachShader ( m_Handler, oldShader.Handler );

			if ( oldShader != shader && shader != null )
				GL.AttachShader ( m_Handler, shader.Handler );


			var info = GL.GetProgramInfoLog ( m_Handler );
			if ( !string.IsNullOrWhiteSpace ( info ) )
				Console.WriteLine ( info );
		}

		public void Use ()
		{
			GL.UseProgram ( m_Handler );
		}

		public bool Link ()
		{
			GL.LinkProgram ( m_Handler );

			if ( !IsLinked )
			{
				Console.WriteLine ( GL.GetProgramInfoLog ( m_Handler ) );
				return false;
			}

			int count, nameLength;

			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveAttributes, out count );
			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveAttributeMaxLength, out nameLength );

			StringBuilder nameBuffer = new StringBuilder ( nameLength );
			for ( int i = 0; i < count; i++ )
			{
				int size;
				ActiveAttribType type;
				int length;

				GL.GetActiveAttrib ( m_Handler, i, nameLength, out length, out size, out type, nameBuffer );

				var name = nameBuffer.ToString ();
				var address = GL.GetAttribLocation ( m_Handler, name );

				nameBuffer.Length = 0;
				Console.WriteLine ( "Attribute " + name + " " + type + " " + size );
			}

			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveUniforms, out count );
			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveUniformMaxLength, out nameLength );

			nameBuffer.Capacity = nameLength;

			for ( int i = 0; i < count; i++ )
			{
				int size;
				ActiveUniformType type;
				int length;

				GL.GetActiveUniform ( m_Handler, i, nameLength, out length, out size, out type, nameBuffer );

				var name = nameBuffer.ToString ();
				var address = GL.GetUniformLocation ( m_Handler, name );

				nameBuffer.Length = 0;
				Console.WriteLine ( "Uniform " + name + " " + type + " " + size );
			}

			return true;
		}

		public void BindAttributeLocation ( string name, int index )
		{
			GL.BindAttribLocation ( m_Handler, index, name );
		}

		public int GetAttributeLocation ( string name )
		{
			return GL.GetAttribLocation ( m_Handler, name );
		}

		public int GetUniformLocation ( string name )
		{
			return GL.GetUniformLocation ( m_Handler, name );
		}
	}
}
