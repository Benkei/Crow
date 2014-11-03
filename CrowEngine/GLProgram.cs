using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class GLProgram : BaseHandler
	{
		private GLShader m_FragmentShader;
		private GLShader m_VertexShader;
		private GLShader m_GeometryShader;
		private GLShader m_TessEvaluationShader;
		private GLShader m_TessControlShader;
		private GLShader m_ComputeShader;

		//private Dictionary<string, object> 

		public GLProgram ()
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

		public bool IsValid
		{
			get
			{
				int status;
				GL.GetProgram ( m_Handler, GetProgramParameterName.ValidateStatus, out status );
				return status == 1;
			}
		}

		public int AttachedShaders
		{
			get
			{
				int count;
				GL.GetProgram ( m_Handler, GetProgramParameterName.AttachedShaders, out count );
				return count;
			}
		}

		public void Delete ()
		{
			GL.DeleteProgram ( m_Handler );
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

			DetachShader ( ref m_FragmentShader );
			DetachShader ( ref m_VertexShader );
			DetachShader ( ref m_GeometryShader );
			DetachShader ( ref m_TessEvaluationShader );
			DetachShader ( ref m_TessControlShader );
			DetachShader ( ref m_ComputeShader );

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

		public void Validate ()
		{
			GL.ValidateProgram ( m_Handler );
		}

		public GLShader GetShader ( ShaderType type )
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

		public void SetShader ( GLShader shader )
		{
			GLShader oldShader = null;
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


		private void DetachShader ( ref GLShader shader )
		{
			if ( shader != null )
			{
				GL.DetachShader ( m_Handler, shader.Handler );
				shader = null;
			}
		}

	}
}
