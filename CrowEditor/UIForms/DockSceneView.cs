using System;
using System.Windows.Forms;
using CrowEngine;
using CrowEngine.Components;
using OpenTK.Graphics.OpenGL4;
using SharpDX;
using WeifenLuo.WinFormsUI.Docking;

namespace CrowEditor.UIForms
{
	public partial class DockSceneView : DockContent, GLRenderView
	{
		GameObject m_Camera;
		Camera m_MainCamera;
		Vector2 m_MousePoint;
		bool m_RotateCamera;

		bool[] keys = new bool[256];
		bool Alt;
		bool Control;
		bool Shift;

		public DockSceneView ()
		{
			InitializeComponent ();

			CrowEditorApp.m_GLRenderThread.RegisterRenderView ( this );

			m_Camera = new GameObject ();
			m_Camera.Transform.Position = new SharpDX.Vector3 ( 0, 0, -3 );

			m_MainCamera = m_Camera.AddComponent<Camera> ();
			m_MainCamera.FieldOfView = 75;
			m_MainCamera.FarClipPlane = 40f;
			m_MainCamera.NearClipPlane = 1f;
			m_MainCamera.Aspect = 1f;
			m_MainCamera.PixelScreenSize = new SharpDX.Rectangle ( 0, 0, ClientSize.Width, ClientSize.Height );
		}

		CrowGLControl GLRenderView.GLControl
		{
			get { return glView; }
		}

		unsafe void GLRenderView.GLRender ()
		{
			int w = glView.Width;
			int h = glView.Height;

			m_MainCamera.PixelScreenSize = new SharpDX.Rectangle ( 0, 0, w, h );

			int uniform_mview = CrowEditorApp.m_GLRenderThread.uniform_mview;
			int uniform_diffuse = CrowEditorApp.m_GLRenderThread.uniform_diffuse;
			GLProgram m_Program = CrowEditorApp.m_GLRenderThread.m_Program;
			Texture2D m_Texture = CrowEditorApp.m_GLRenderThread.List[CrowEditorApp.m_GLRenderThread.texutreIdx];
			GameObject m_Cube = CrowEditorApp.m_GLRenderThread.m_Cube;

			GL.Enable ( EnableCap.DepthTest );
			GL.ClearColor ( OpenTK.Graphics.Color4.CornflowerBlue );

			GL.Viewport ( 0, 0, w, h );
			GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			if ( m_Program != null && m_Cube != null && m_Texture != null )
			{
				var mesh = m_Cube.GetComponent<MeshRenderer> ().Mesh;
				if ( mesh != null )
				{
					m_Program.Use ();

					var matrix = m_Cube.Transform.WorldMatrix * m_MainCamera.ViewMatrix * m_MainCamera.ProjectionMatrix;

					GL.UniformMatrix4 ( uniform_mview, 1, false, (float*)&matrix );
					GL.Uniform1 ( uniform_diffuse, 0 );

					GL.ActiveTexture ( TextureUnit.Texture0 + 0 );
					GL.BindTexture ( TextureTarget.Texture2D, m_Texture.Handler );

					mesh.CheckVao ();
					mesh.m_Vao.Bind ();

					mesh.m_Ibo.Bind ();

					GL.DrawElements ( BeginMode.Triangles, mesh.m_Indices, mesh.m_Ibo.ElementType, 0 );
				}
			}

		}

		protected override void OnLoad ( EventArgs e )
		{
			base.OnLoad ( e );

			CrowEditorApp.UpdateLogicThread += CrowEditorApp_UpdatingEngine;
		}

		protected override void OnFormClosed ( FormClosedEventArgs e )
		{
			base.OnFormClosed ( e );

			CrowEditorApp.UpdateLogicThread -= CrowEditorApp_UpdatingEngine;
		}

		void CrowEditorApp_UpdatingEngine ( object sender, EventArgs e )
		{
			float boost = 1;
			if ( Shift )
			{
				boost = 5;
			}
			if ( keys[(int)Keys.W] )
			{
				m_Camera.Transform.Position += m_Camera.Transform.Forward * boost * CrowEditorApp.DeltaTime;
				glView.GLRepaint ();
			}
			if ( keys[(int)Keys.A] )
			{
				m_Camera.Transform.Position -= m_Camera.Transform.Right * boost * CrowEditorApp.DeltaTime;
				glView.GLRepaint ();
			}
			if ( keys[(int)Keys.S] )
			{
				m_Camera.Transform.Position -= m_Camera.Transform.Forward * boost * CrowEditorApp.DeltaTime;
				glView.GLRepaint ();
			}
			if ( keys[(int)Keys.D] )
			{
				m_Camera.Transform.Position += m_Camera.Transform.Right * boost * CrowEditorApp.DeltaTime;
				glView.GLRepaint ();
			}
		}

		private void glView_KeyDown ( object sender, KeyEventArgs e )
		{
			if ( e.Shift )
			{
				Shift = true;
			}
			if ( e.Alt )
			{
				Alt = true;
			}
			if ( e.Control )
			{
				Control = true;
			}
			if ( (int)e.KeyCode >= 0 && (int)e.KeyCode < 256 )
				keys[(int)e.KeyCode] = true;

			if ( e.KeyCode == Keys.N )
			{
				var idx = CrowEditorApp.m_GLRenderThread.texutreIdx;
				idx++;
				if ( idx >= CrowEditorApp.m_GLRenderThread.List.Count )
				{
					idx = 0;
				}
				CrowEditorApp.m_GLRenderThread.texutreIdx = idx;
				Console.WriteLine ( "Tex idx " + idx );
				glView.GLRepaint ();
			}
		}

		private void glView_KeyUp ( object sender, KeyEventArgs e )
		{
			if ( e.Shift )
			{
				Shift = false;
			}
			if ( e.Alt )
			{
				Alt = false;
			}
			if ( e.Control )
			{
				Control = false;
			}
			if ( (int)e.KeyCode >= 0 && (int)e.KeyCode < 256 )
				keys[(int)e.KeyCode] = false;
		}

		private void glView_MouseDown ( object sender, MouseEventArgs e )
		{
			if ( e.Button == System.Windows.Forms.MouseButtons.Right )
			{
				m_RotateCamera = true;
				LockMouse ();
			}
		}

		private void glView_MouseUp ( object sender, MouseEventArgs e )
		{
			m_RotateCamera = false;
			UnlockMouse ();
		}

		private void glView_MouseMove ( object sender, MouseEventArgs e )
		{
			var pos = new Vector2 ( Cursor.Position.X, Cursor.Position.Y );
			if ( m_MousePoint == pos )
			{
				return;
			}
			var delta = (m_MousePoint - pos) * CrowEditorApp.DeltaTime;
			m_MousePoint = pos;

			if ( m_RotateCamera )
			{
				const int BORDER = 5;
				var screen = SystemInformation.VirtualScreen;
				System.Drawing.Point point = Cursor.Position;
				if ( pos.X <= BORDER )
				{
					point.X = screen.Width - (BORDER + 1);
				}
				if ( pos.Y <= BORDER )
				{
					point.Y = screen.Height - (BORDER + 1);
				}
				if ( pos.X >= screen.Width - BORDER )
				{
					point.X = BORDER + 1;
				}
				if ( pos.Y >= screen.Height - BORDER )
				{
					point.Y = BORDER + 1;
				}
				Cursor.Position = point;
				m_MousePoint = new Vector2 ( point.X, point.Y );

				m_Camera.Transform.EulerAngles -= new Vector3 ( delta.Y, delta.X, 0 );
				//m_Camera.Transform.Rotation *= Quaternion.RotationAxis ( Vector3.UnitY, delta.X );
				//m_Camera.Transform.Rotation *= Quaternion.RotationAxis ( Vector3.UnitX, delta.Y );
				glView.Invalidate ();
			}
		}



		bool _lockMouse;
		System.Drawing.Point _origCursorPosition;

		protected void LockMouse ()
		{
			_lockMouse = true;
			_origCursorPosition = Cursor.Position;
			Cursor.Hide ();
		}

		protected void UnlockMouse ()
		{
			if ( _lockMouse )
			{
				_lockMouse = false;
				Cursor.Show ();
				Cursor.Position = _origCursorPosition;
			}
		}
	}
}
