using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace CrowEditor
{
	interface GLRenderView
	{
		void Render ();
	}

	class CrowGLControl : GLControl
	{
		public IGraphicsContext _Context;
		public IWindowInfo _WindowInfo;

		public bool IsReady { get; private set; }

		protected override void OnHandleCreated ( EventArgs e )
		{
			base.OnHandleCreated ( e );

			Context.MakeCurrent ( null );

			lock ( this )
			{
				IsReady = true;
				_Context = Context;
				_WindowInfo = WindowInfo;
			}
		}

		protected override void OnHandleDestroyed ( EventArgs e )
		{
			lock ( this )
			{
				IsReady = false;
				_Context = null;
				_WindowInfo = null;
			}
			base.OnHandleDestroyed ( e );
		}

		//protected override void OnPaint ( System.Windows.Forms.PaintEventArgs e )
		//{
		//	base.OnPaint ( e );

		//	CrowEditorApp.RegisterRenderView ( this, glView );
		//}
	}

}
