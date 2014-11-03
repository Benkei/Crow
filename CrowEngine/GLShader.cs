using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class GLShader : GLHandler
	{
		public GLShader ( ShaderType type, string sourceText )
			: this ( type )
		{
			SetSource ( sourceText );
			Compile ();
		}

		public GLShader ( ShaderType type )
		{
			m_Handler = GL.CreateShader ( type );
		}

		public bool IsCompiled
		{
			get
			{
				int status;
				GL.GetShader ( m_Handler, ShaderParameter.CompileStatus, out status );
				return status == 1;
			}
		}

		public ShaderType Type
		{
			get
			{
				int type;
				GL.GetShader ( m_Handler, ShaderParameter.ShaderType, out type );
				return (ShaderType)type;
			}
		}

		public string GetSource ()
		{
			int length;
			GL.GetShader ( m_Handler, ShaderParameter.ShaderSourceLength, out length );
			var sb = new StringBuilder ( length );
			GL.GetShaderSource ( m_Handler, length, out length, sb );
			return sb.ToString ();
		}

		public void SetSource ( string text )
		{
			GL.ShaderSource ( m_Handler, text );
		}

		public void SetBinarySource ( IntPtr data, int length )
		{
			GL.ShaderBinary ( 1, ref m_Handler, (BinaryFormat)0, data, length );
		}

		public void Compile ()
		{
			GL.CompileShader ( m_Handler );

			if ( IsCompiled )
			{
				var info = GL.GetShaderInfoLog ( m_Handler );
				Console.WriteLine ( info );
			}
		}

		public void Delete ()
		{
			GL.DeleteShader ( m_Handler );
		}

		public static void ReleaseShaderCompiler ()
		{
			GL.ReleaseShaderCompiler ();
		}
	}
}
