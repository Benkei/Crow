using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace CrowEditor
{
	internal interface GLRenderView
	{
		CrowGLControl GLControl { get; }

		void GLRender ();
	}

	internal class CrowGLControl : GLControl
	{
		private bool m_IsReady;
		private IGraphicsContext m_Context;

		public IWindowInfo _WindowInfo;
		public bool IsDirty = true;

		public CrowGLControl ()
		{
			InitializeComponent ();
		}

		public bool IsReady
		{
			get { return m_IsReady && !IsDisposed && m_Context != null && !m_Context.IsDisposed && IsHandleCreated; }
		}

		public void GLRepaint ()
		{
			System.Threading.Thread.MemoryBarrier ();
			IsDirty = true;
		}

		protected override void OnHandleCreated ( EventArgs e )
		{
			base.OnHandleCreated ( e );

			Context.MakeCurrent ( null );

			lock ( this )
			{
				m_Context = Context;
				_WindowInfo = WindowInfo;
				m_IsReady = true;
				IsDirty = true;
			}
		}

		protected override void OnHandleDestroyed ( EventArgs e )
		{
			lock ( this )
			{
				m_IsReady = false;
				m_Context = null;
				_WindowInfo = null;
				IsDirty = false;
			}
			base.OnHandleDestroyed ( e );
		}

		protected override void OnPaint ( System.Windows.Forms.PaintEventArgs e )
		{
			base.OnPaint ( e );
			GLRepaint ();
		}

		private void InitializeComponent ()
		{
			this.SuspendLayout ();
			// 
			// CrowGLControl
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF ( 6F, 13F );
			this.Name = "CrowGLControl";
			this.ResumeLayout ( false );

		}
	}

}
