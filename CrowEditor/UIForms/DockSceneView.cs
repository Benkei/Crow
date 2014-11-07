using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL4;
using WeifenLuo.WinFormsUI.Docking;

namespace CrowEditor.UIForms
{
	public partial class DockSceneView : DockContent, GLRenderView
	{
		public DockSceneView ()
		{
			InitializeComponent ();
		}

		public void Render ()
		{
			int w = glView.Width;
			int h = glView.Height;

			GL.ClearColor ( OpenTK.Graphics.Color4.Aqua );

			OpenTK.Graphics.OpenGL.GL.MatrixMode ( OpenTK.Graphics.OpenGL.MatrixMode.Projection );
			OpenTK.Graphics.OpenGL.GL.LoadIdentity ();
			OpenTK.Graphics.OpenGL.GL.Ortho ( 0, w, 0, h, -1, 1 ); // Bottom-left corner pixel has coordinate (0, 0)
			OpenTK.Graphics.OpenGL.GL.Viewport ( 0, 0, w, h ); // Use all of the glControl painting area


			GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			OpenTK.Graphics.OpenGL.GL.MatrixMode ( OpenTK.Graphics.OpenGL.MatrixMode.Modelview );
			OpenTK.Graphics.OpenGL.GL.LoadIdentity ();
			OpenTK.Graphics.OpenGL.GL.Color3 ( Color.Yellow );
			OpenTK.Graphics.OpenGL.GL.Begin ( OpenTK.Graphics.OpenGL.PrimitiveType.Triangles );
			OpenTK.Graphics.OpenGL.GL.Vertex2 ( 10, 20 );
			OpenTK.Graphics.OpenGL.GL.Vertex2 ( 100, 20 );
			OpenTK.Graphics.OpenGL.GL.Vertex2 ( 100, 50 );
			OpenTK.Graphics.OpenGL.GL.End ();

		}

		private void glView_Paint ( object sender, PaintEventArgs e )
		{
			CrowEditorApp.RegisterRenderView ( this, glView );
		}
	}
}
