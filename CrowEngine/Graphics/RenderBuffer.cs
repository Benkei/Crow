using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class RenderBuffer : GLObject
	{
		private RenderbufferTarget m_Target;

		public RenderBuffer ( RenderbufferTarget target, int width, int height, int samples, RenderbufferStorage type )
		{
			if ( target != RenderbufferTarget.Renderbuffer )
				throw new ArgumentException ();
			var maxSize = GL.GetInteger ( GetPName.MaxRenderbufferSize );
			if ( maxSize == width * height )
				throw new ArgumentOutOfRangeException ();

			m_Target = target;
			m_Handler = GL.GenRenderbuffer ();

			GL.BindRenderbuffer ( m_Target, m_Handler );

			GL.RenderbufferStorageMultisample (
				m_Target,
				samples,
				type,
				width,
				height );
		}

		public RenderbufferTarget Target
		{
			get { return m_Target; }
			set { m_Target = value; }
		}

		public int Samples
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferSamples, out value );
				return value;
			}
		}
		public int Width
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferWidth, out value );
				return value;
			}
		}
		public int Height
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferHeight, out value );
				return value;
			}
		}
		public RenderbufferStorage InternalFormat
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferInternalFormat, out value );
				return (PixelInternalFormat)value;
			}
		}
		public int RedSize
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferRedSize, out value );
				return value;
			}
		}
		public int GreenSize
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferGreenSize, out value );
				return value;
			}
		}
		public int BlueSize
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferBlueSize, out value );
				return value;
			}
		}
		public int AlphaSize
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferAlphaSize, out value );
				return value;
			}
		}
		public int DepthSize
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferDepthSize, out value );
				return value;
			}
		}
		public int StencilSize
		{
			get
			{
				int value;
				GL.GetRenderbufferParameter ( m_Target, RenderbufferParameterName.RenderbufferStencilSize, out value );
				return value;
			}
		}


		public void Delete ()
		{
			GL.DeleteRenderbuffer ( m_Handler );
		}
	}
}
