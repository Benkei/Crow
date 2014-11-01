using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class Texture2D : BaseHandler
	{
		public Texture2D ( int width, int height, IntPtr data )
		{
			m_Handler = GL.GenTexture ();
			GL.BindTexture ( TextureTarget.Texture2D, m_Handler );

			GL.TexImage2D (
				TextureTarget.Texture2D,
				0,
				PixelInternalFormat.Rgba,
				width,
				height,
				0,
				PixelFormat.Bgra,
				PixelType.UnsignedByte,
				data
			);

			GL.TexParameter (
				TextureTarget.Texture2D,
				TextureParameterName.TextureMinFilter,
				(int)TextureMinFilter.Nearest );
			GL.TexParameter (
				TextureTarget.Texture2D,
				TextureParameterName.TextureMagFilter,
				(int)TextureMagFilter.Nearest );
		}
	}
}
