using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class GpuShader : BaseHandler
	{
		ShaderType m_Type;


		public GpuShader ( ShaderType type, string sourceText )
			: this ( type )
		{
			SetSource ( sourceText );
			Compile ();
		}

		public GpuShader ( ShaderType type )
		{
			m_Handler = GL.CreateShader ( type );
			m_Type = type;
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
			get { return m_Type; }
		}

		public void SetSource ( string text )
		{
			GL.ShaderSource ( m_Handler, text );
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
	}
}
