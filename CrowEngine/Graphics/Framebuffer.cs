using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class Framebuffer : GLObject
	{
		public Framebuffer ()
		{
			m_Handler = GL.GenFramebuffer ();

			//GL.FramebufferParameter ( FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultWidth, 0 );
			//GL.GetFramebufferAttachmentParameter ( FramebufferTarget.Framebuffer, FramebufferAttachment.Color, FramebufferParameterName.FramebufferAttachmentObjectName, null );
		}

		public void Delete ()
		{
			GL.DeleteFramebuffer ( m_Handler );
			m_Handler = 0;
		}
	}
}
