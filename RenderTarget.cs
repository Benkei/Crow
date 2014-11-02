using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class RenderTarget : BaseHandler
	{
		public RenderTarget ( int width, int height )
		{
			var maxSize = GL.GetInteger ( GetPName.MaxRenderbufferSize );

			if ( maxSize == width * height )
			{
				throw new ArgumentOutOfRangeException ();
			}

			m_Handler = GL.GenRenderbuffer ();

			GL.BindRenderbuffer ( RenderbufferTarget.Renderbuffer, m_Handler );

			GL.RenderbufferStorageMultisample (
				RenderbufferTarget.Renderbuffer,
				0,
				RenderbufferStorage.Rgb8,
				width,
				height );
		}

		public void Delete ()
		{
			GL.DeleteRenderbuffer ( m_Handler );
		}
	}
}
