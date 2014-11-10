using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrowEditor.UIForms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace CrowEditor
{
	static class CrowEditorApp
	{
		internal static GLRenderThread m_GLRenderThread;
		internal static GLBackgroundThread m_GLBackgroundThread;
		internal static LogicThread m_LogicThread;
		internal static MainWindow m_MainForm;

		public static event EventHandler UpdateLogicThread;

		public static float DeltaTime
		{
			get { return m_LogicThread.DeltaTime; }
		}

		public static void Init ()
		{
			GraphicsContext.ShareContexts = true;
#if DEBUG
			const GraphicsContextFlags flags = GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug;
#else
			const GraphicsContextFlags flags = GraphicsContextFlags.ForwardCompatible;
#endif
			//new GraphicsMode ( ColorFormat.Empty, 0, 0, 0, ColorFormat.Empty, 0, false );
			GraphicsMode mode = GraphicsMode.Default;

			m_GLRenderThread = new GLRenderThread ( mode, flags );
			m_GLBackgroundThread = new GLBackgroundThread ( mode, flags );

			GraphicsContext.ShareContexts = false;

			m_LogicThread = new LogicThread ();

			Application.Idle += Application_Idle;
		}

		public static void Terminate ()
		{
			m_GLBackgroundThread.Stop ();
			m_GLRenderThread.Stop ();
		}

		public static void Start ()
		{
			using ( var waitReady = new EventWaitHandle ( false, EventResetMode.ManualReset ) )
			{
				m_GLBackgroundThread.Start ( waitReady );

				waitReady.WaitOne ();
				waitReady.Reset ();

				m_GLRenderThread.Start ( waitReady );

				waitReady.WaitOne ();
			}
			m_LogicThread.Start ();
		}


		static void Application_Idle ( object sender, EventArgs e )
		{
			m_MainForm.Text = DeltaTime.ToString ();

		}

		internal static void OnUpdateLogicThread ()
		{
			var callbacks = UpdateLogicThread;
			if ( callbacks != null )
				callbacks ( null, EventArgs.Empty );
		}

	}
}
