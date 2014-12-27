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
		private FramebufferTarget m_Target;

		public Framebuffer ( FramebufferTarget target )
		{
			m_Target = target;
			m_Handler = GL.GenFramebuffer ();

			//GL.FramebufferParameter ( FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultWidth, 0 );
			//GL.GetFramebufferAttachmentParameter ( FramebufferTarget.Framebuffer, FramebufferAttachment.Color, FramebufferParameterName.FramebufferAttachmentObjectName, null );

			//GL.FramebufferRenderbuffer ( FramebufferTarget.Framebuffer, FramebufferAttachment.Color, RenderbufferTarget.Renderbuffer, 0 );
			//GL.FramebufferTexture ( FramebufferTarget.Framebuffer, FramebufferAttachment.Color, 0, 0 );
			//GL.FramebufferTexture1D ( FramebufferTarget.Framebuffer, FramebufferAttachment.Color, TextureTarget.Texture2D, 0, 0 );
			//GL.FramebufferTexture2D ( FramebufferTarget.Framebuffer, FramebufferAttachment.Color, TextureTarget.Texture2D, 0, 0 );
			//GL.FramebufferTexture3D ( FramebufferTarget.Framebuffer, FramebufferAttachment.Color, TextureTarget.Texture2D, 0, 0, 0 );
			//GL.FramebufferTextureLayer ( FramebufferTarget.Framebuffer, FramebufferAttachment.Color, 0, 0, 0 );
		}

		public FramebufferErrorCode Status
		{
			get { return GL.CheckFramebufferStatus ( m_Target ); }
		}

		public void Delete ()
		{
			GL.DeleteFramebuffer ( m_Handler );
			m_Handler = 0;
		}

		public void Bind ()
		{
			GL.BindFramebuffer ( m_Target, m_Handler );
		}

		public Parameter AttachmentParameter ( FramebufferAttachment attachment )
		{
			Parameter param = new Parameter ();
			GL.GetFramebufferAttachmentParameter ( m_Target, attachment, FramebufferParameterName.FramebufferAttachmentObjectType, out param.ObjectType );

			return param;
		}

		public void AttachRenderbuffer ( FramebufferAttachment attachment, RenderBuffer renderbuffer )
		{
			GL.FramebufferRenderbuffer ( m_Target, attachment, renderbuffer.Target, renderbuffer.Handler );
		}

		public void AttachTexture ( FramebufferAttachment attachment, Texture texture, int level )
		{
			GL.FramebufferTexture ( m_Target, attachment, texture.Handler, level );
		}

		public void AttachTexture1D ( FramebufferAttachment attachment, Texture1D texture, int level )
		{
			GL.FramebufferTexture1D ( m_Target, attachment, TextureTarget.Texture1D, texture.Handler, level );
		}

		public void AttachTexture2D ( FramebufferAttachment attachment, Texture2D texture, int level )
		{
			GL.FramebufferTexture2D ( m_Target, attachment, TextureTarget.Texture2D, texture.Handler, level );
		}

		public void AttachTexture3D ( FramebufferAttachment attachment, Texture3D texture, int level, int zOffset )
		{
			GL.FramebufferTexture3D ( m_Target, attachment, TextureTarget.Texture3D, texture.Handler, level, zOffset );
		}

		public void AttachTextureLayer ( FramebufferAttachment attachment, Texture texture, int level, int layer )
		{
			GL.FramebufferTextureLayer ( m_Target, attachment, texture.Handler, level, layer );
		}
	}

	struct Parameter
	{
		public int ObjectType;
		public int ObjectName;
		public int ColorEncoding;
		public int ComponentType;
		public int RedSize;
		public int GreenSize;
		public int BlueSize;
		public int AlphaSize;
		public int DepthSize;
		public int StencilSize;
		public int TextureLevel;
		public int TextureCubeMapFace;
		public int TextureLayer;
		public int Texture3DZoffsetExt;
		public int Layered;
	}
}
