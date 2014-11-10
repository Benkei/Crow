using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace CrowEditor
{
	abstract class GLThread : EditorThread
	{
		// cache callback
		private static DebugProc m_DebugCallback = GLDebugCallback;
		protected GraphicsMode m_Mode;
		protected GraphicsContextFlags m_Flags;
		protected NativeWindow m_Window;
		protected GraphicsContext m_Context;

		public GLThread ( GraphicsMode mode, GraphicsContextFlags flags )
		{
			m_Mode = mode;
			m_Flags = flags;

			m_Window = new NativeWindow ();
			m_Context = new GraphicsContext ( m_Mode, m_Window.WindowInfo, 1, 0, m_Flags );
			m_Context.MakeCurrent ( null );
		}


		protected abstract void GLBeginUpdate ();

		protected abstract void GLUpdate ();

		protected abstract void GLEndUpdate ();

		protected override void OnThreadUpdate ( object context )
		{
			m_Context.MakeCurrent ( m_Window.WindowInfo );

			((EventWaitHandle)context).Set ();
			context = null;

			GraphicsContext.Assert ();

			GL.Enable ( EnableCap.DebugOutput );
			GL.DebugMessageCallback ( m_DebugCallback, IntPtr.Zero );

			GLBeginUpdate ();

			while ( m_Running )
			{
				m_Window.ProcessEvents ();
				GLUpdate ();
				Thread.Sleep ( 1 );
			}

			GLEndUpdate ();
		}

		static void GLDebugCallback ( DebugSource source, DebugType type,
			int id, DebugSeverity severity,
			int length, IntPtr message,
			IntPtr userParam )
		{
			var mes = System.Runtime.InteropServices.Marshal.PtrToStringAnsi ( message, length );
			Console.WriteLine ( "ID: {0};\nSource: {1};\nType: {2};\nSeverity: {3};\n  {4}\n", id, source, type, severity, mes );
		}

	}
}
