using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrowEditor.Jobs;
using CrowEngine;
using CrowEngine.Components;
using CrowEngine.GpuPrograms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace CrowEditor
{
	class GLRenderThread : GLThread
	{
		List<GLRenderView> m_GLRenderViews = new List<GLRenderView> ();
		List<GLRenderView> m_Tmp = new List<GLRenderView> ();

		public readonly JobScheduler JobScheduler = new JobScheduler ();

		public GLRenderThread ( GraphicsMode mode, GraphicsContextFlags flags )
			: base ( mode, flags )
		{
		}

		public void RegisterRenderView ( GLRenderView view )
		{
			lock ( m_GLRenderViews )
			{
				m_GLRenderViews.Add ( view );
			}
		}

		public void DeregisterRenderView ( GLRenderView view )
		{
			lock ( m_GLRenderViews )
			{
				m_GLRenderViews.Remove ( view );
			}
		}


		protected override void GLBeginUpdate ()
		{
			LoadAsset ();
		}

		protected override void GLUpdate ()
		{
			m_Tmp.Clear ();
			lock ( m_GLRenderViews )
			{
				m_Tmp.AddRange ( m_GLRenderViews );
			}

			for ( int i = m_Tmp.Count - 1; i >= 0; i-- )
			{
				var renderView = m_Tmp[i];
				try
				{
					var control = renderView.GLControl;
					lock ( control )
					{
						if ( !control.IsReady || !control.IsDirty || !control.IsIdle )
							continue;

						m_Context.MakeCurrent ( control._WindowInfo );
						renderView.GLRender ();
						m_Context.SwapBuffers ();

						control.IsDirty = false;
					}
					m_Context.MakeCurrent ( null );
				}
				catch ( Exception ex )
				{
					Console.WriteLine ( ex.ToString () );
				}
			}

			if ( JobScheduler.Count > 0 )
			{
				m_Context.MakeCurrent ( m_Window.WindowInfo );
				JobScheduler.Execute ();
				GL.Flush ();
			}
		}

		protected override void GLEndUpdate ()
		{
		}


		private bool loaded;
		public int uniform_mview;
		public int uniform_diffuse;
		public int texutreIdx;
		public GLProgram m_Program;
		public GameObject m_Cube;

		public System.Collections.Generic.List<Texture2D> List = new System.Collections.Generic.List<Texture2D> ();

		private void LoadAsset ()
		{
			if ( loaded )
			{
				return;
			}
			loaded = true;

			var m_Texture = TextureFactory.Load ( "Assets/guid.JPG" );

			List.Add ( m_Texture );

			m_Program = GpuProgramFactory.Load ( "Assets/Simple.gfx" );

			uniform_mview = m_Program.GetUniformLocation ( "modelview" );
			uniform_diffuse = m_Program.GetUniformLocation ( "diffuse" );

			m_Cube = new GameObject ();
			m_Cube.AddComponent<MeshRenderer> ();

			var rendere = m_Cube.GetComponent<MeshRenderer> ();
			rendere.Mesh = MeshPrimitive.CreateBox ();

			rendere.Mesh.CheckVao ();
		}
	}
}
