using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace CrowStudio
{
	class EditorHost : IDisposable
	{
		GameWindow m_MainWindow;
		IntPtr hDeviceContext;
		IntPtr hRenderContext;

		Isolated<EditorSandboxRemote> m_SandboxDomain;

		public EditorHost ()
		{
			m_MainWindow = new GameWindow ();
			m_MainWindow.Visible = true;

			hDeviceContext = Wgl.GetCurrentDC ();
			hRenderContext = Wgl.GetCurrentContext ();
			m_MainWindow.Context.MakeCurrent ( null );

			m_MainWindow.KeyUp += m_MainWindow_KeyUp;
			m_MainWindow.Closing += m_MainWindow_Closing;

			// init sandbox
			StartEditorSandbox ();
		}

		public void Run ()
		{
			while ( !m_MainWindow.IsExiting )
			{
				m_MainWindow.ProcessEvents ();
				Thread.Sleep ( 1 );
			}
			Dispose ();
		}

		public void Dispose ()
		{
			ReleaseEditorSandbox ();
			m_MainWindow.Dispose ();
		}

		void StartEditorSandbox ()
		{
			ReleaseEditorSandbox ();
			m_SandboxDomain = new Isolated<EditorSandboxRemote> ();
			m_SandboxDomain.Value.Init ( AppDomain.CurrentDomain,
				OpenTK.Graphics.OpenGL.GL.EntryPoints,
				OpenTK.Graphics.OpenGL4.GL.EntryPoints,
				hDeviceContext, hRenderContext );
			m_SandboxDomain.Value.Run ();
		}

		void ReleaseEditorSandbox ()
		{
			if ( m_SandboxDomain != null )
			{
				m_SandboxDomain.Value.Exit ();
				m_SandboxDomain.Dispose ();
			}
			m_SandboxDomain = null;
		}

		void m_MainWindow_KeyUp ( object sender, KeyboardKeyEventArgs e )
		{
			if ( e.Key == OpenTK.Input.Key.P )
			{
				Debug.WriteLine ( "Playmode Restart sandbox" );
				StartEditorSandbox ();
			}
			if ( e.Key == OpenTK.Input.Key.Escape )
			{
				m_MainWindow.Exit ();
			}
		}

		void m_MainWindow_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
		{
			ReleaseEditorSandbox ();
		}

	}
}
