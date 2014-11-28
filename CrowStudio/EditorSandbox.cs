using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Platform;

namespace CrowStudio
{
	class EditorSandbox
	{
		private Thread Thread;
		private bool Running = true;
		private IntPtr[] glFuncPtrs;
		private IntPtr[] gl4FuncPtrs;
		private IntPtr hdc;
		private IntPtr hrc;
		GraphicsContext dummyContext;
		IWindowInfo dummyWindowInfo;
		EditorSandboxRemote remote;


		public EditorSandbox ( EditorSandboxRemote remote,
			IntPtr[] glFuncPtrs,
			IntPtr[] gl4FuncPtrs,
			IntPtr hdc, IntPtr hrc )
		{
			this.remote = remote;
			this.glFuncPtrs = glFuncPtrs;
			this.gl4FuncPtrs = gl4FuncPtrs;
			this.hdc = hdc;
			this.hrc = hrc;
		}

		public void Run ()
		{
			Thread = new Thread ( Logic );
			Thread.IsBackground = true;
			Thread.SetApartmentState ( ApartmentState.STA );
			Thread.Start ();
		}

		public void Exit ()
		{
			Running = false;
			if ( !Thread.Join ( 1000 * 15 ) )
			{
				Thread.Abort ();
			}
		}

		private void Logic ()
		{
			OpenTK.Toolkit.Init ();

			MakeCurrent ();

			dummyWindowInfo = Utilities.CreateDummyWindowInfo ();
			dummyContext = new GraphicsContext ( new ContextHandle ( hrc ), null );
			dummyContext.MakeCurrent ( dummyWindowInfo );

			glFuncPtrs.CopyTo ( OpenTK.Graphics.OpenGL.GL.EntryPoints, 0 );
			gl4FuncPtrs.CopyTo ( OpenTK.Graphics.OpenGL4.GL.EntryPoints, 0 );


			GL.ClearColor ( Color4.MidnightBlue );

			GL.Viewport ( 0, 0, 640, 480 );

			var timer = Stopwatch.StartNew ();
			float deltaTime = 0;
			while ( Running )
			{
				// render graphics

				GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );




				Wgl.SwapBuffers ( hdc );

				deltaTime = (float)timer.Elapsed.TotalSeconds;
				timer.Restart ();
			}
		}

		private void MakeCurrent ( bool bind = true )
		{
			var ok = bind ? Wgl.MakeCurrent ( hdc, hrc ) : Wgl.MakeCurrent ( IntPtr.Zero, IntPtr.Zero );
			if ( ok == 0 )
			{
				int err = Marshal.GetLastWin32Error ();
				Exception ex = new System.ComponentModel.Win32Exception ( err );
				throw ex;
			}
		}
	}
}
