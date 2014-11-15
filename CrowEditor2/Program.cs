using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace CrowEditor2
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		[LoaderOptimization ( LoaderOptimization.MultiDomainHost )]
		static void Main ()
		{
			//Application.EnableVisualStyles ();
			//Application.SetCompatibleTextRenderingDefault ( false );
			//Application.Run ( new Form1 () );


			OpenTK.Toolkit.Init ();

			using ( var win = new MainWindow () )
			//using ( var sandbox = new Isolated<RemoteChildObj> () )
			{
				var hdc = wgl.GetCurrentDC ();
				var hrc = wgl.GetCurrentContext ();
				win.Context.MakeCurrent ( null );

				Console.WriteLine ( "Start sandbox" );

				//var remote = sandbox.Value;
				var remote = new RemoteChildObj ();
				remote.Width = win.Width;
				remote.Height = win.Height;
				remote.Init ( AppDomain.CurrentDomain, win.WindowInfo.Handle, hdc, hrc );

				remote.Run ();

				win.Visible = true;

				//win.MouseMove += ( a, b ) =>
				//{
				//	Console.WriteLine ( "Move" );
				//};

				//OpenTK.Platform.Utilities.

				while ( !win.IsExiting )
				{
					win.ProcessEvents ();
					Thread.Sleep ( 1 );
				}
			}
		}
	}

	class ChildSandbox
	{
		private Thread Thread;
		private bool Running = true;
		private IntPtr window;
		private IntPtr hdc;
		private IntPtr hrc;
		GraphicsContext dummyContext;
		IWindowInfo dummyWindowInfo;
		RemoteChildObj remote;

		public ChildSandbox ( RemoteChildObj remote, IntPtr window, IntPtr hdc, IntPtr hrc )
		{
			this.remote = remote;
			this.window = window;
			this.hdc = hdc;
			this.hrc = hrc;
		}

		public void Run ()
		{
			Thread = new Thread ( Logic );
			Thread.IsBackground = true;
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

			//dummyWindowInfo = OpenTK.Platform.Utilities.CreateWindowsWindowInfo ( window );
			//dummyContext = new GraphicsContext ( new ContextHandle ( hrc ), null );
			//dummyContext.MakeCurrent ( dummyWindowInfo );

			GL.Viewport ( 0, 0, remote.Width, remote.Height );

			while ( Running )
			{
				// render graphics
				GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

				GL.MatrixMode ( MatrixMode.Projection );
				GL.LoadIdentity ();
				GL.Ortho ( -1.0, 1.0, -1.0, 1.0, 0.0, 4.0 );

				GL.Begin ( PrimitiveType.Triangles );

				GL.Color3 ( Color.MidnightBlue );
				GL.Vertex2 ( -1.0f, 1.0f );
				GL.Color3 ( Color.SpringGreen );
				GL.Vertex2 ( 0.0f, -1.0f );
				GL.Color3 ( Color.Ivory );
				GL.Vertex2 ( 1.0f, 1.0f );

				GL.End ();

				wgl.SwapBuffers ( hdc );

				Thread.Sleep ( 1 );
			}
		}

		private void MakeCurrent ( bool bind = true )
		{
			var ok = bind ? wgl.MakeCurrent ( hdc, hrc ) : wgl.MakeCurrent ( IntPtr.Zero, IntPtr.Zero );
			if ( ok == 0 )
			{
				int err = Marshal.GetLastWin32Error ();
				Exception ex = new System.ComponentModel.Win32Exception ( err );
				throw ex;
			}
		}
	}

	#region public static class wgl

	public static class wgl
	{
		[DllImport ( "opengl32.dll", EntryPoint = "wglGetCurrentDC" ), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetCurrentDC ();

		[DllImport ( "opengl32.dll", EntryPoint = "wglGetCurrentContext" ), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetCurrentContext ();

		[DllImport ( "opengl32.dll", EntryPoint = "wglCreateContext", SetLastError = true ), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr CreateContext ( IntPtr hdc );

		[DllImport ( "opengl32.dll", EntryPoint = "wglMakeCurrent", SetLastError = true ), SuppressUnmanagedCodeSecurity]
		public static extern int MakeCurrent ( IntPtr hdc, IntPtr hrc );

		[DllImport ( "opengl32.dll", EntryPoint = "wglSwapBuffers", SetLastError = true ), SuppressUnmanagedCodeSecurity]
		public static extern uint SwapBuffers ( IntPtr hdc );
	}

	#endregion

	public sealed class Isolated<T> : IDisposable
		where T : MarshalByRefObject
	{
		private AppDomain _domain;
		private T _value;

		public Isolated ()
		{
			AppDomainSetup setup = new AppDomainSetup ()
			{
				LoaderOptimization = LoaderOptimization.MultiDomain,
			};
			_domain = AppDomain.CreateDomain ( "Isolated:" + Guid.NewGuid (), null, setup );

			Type type = typeof ( T );
			_value = (T)_domain.CreateInstanceAndUnwrap ( type.Assembly.FullName, type.FullName );
		}

		public T Value
		{
			get { return _value; }
		}

		public void Dispose ()
		{
			if ( _domain != null )
			{
				AppDomain.Unload ( _domain );

				_domain = null;
			}
		}
	}

	class RemoteChildObj : MarshalByRefObject
	{
		[NonSerialized]
		private RemoteHost Host;
		[NonSerialized]
		private ChildSandbox Logic;

		public int Width;
		public int Height;

		public void Init ( AppDomain parent, IntPtr window, IntPtr dc, IntPtr hdr )
		{
			var type = typeof ( RemoteHost );
			Host = (RemoteHost)parent.CreateInstanceFromAndUnwrap (
				type.Assembly.Location,
				type.FullName
			);
			Host.TestHost ();

			Logic = new ChildSandbox ( this, window, dc, hdr );
		}

		public void Run ()
		{
			Logic.Run ();
		}

		public void Exit ()
		{
			Logic.Exit ();
		}
	}

	class RemoteHost : MarshalByRefObject
	{
		public void TestHost ()
		{
			Console.WriteLine ( AppDomain.CurrentDomain.FriendlyName );
			Console.WriteLine ( "Hallo host" );
		}
	}

	class MainWindow : GameWindow
	{
		public MainWindow ()
			: base ( 640, 480,
			GraphicsMode.Default,
			"CrowEditor", GameWindowFlags.Default,
			DisplayDevice.Default,
			1, 0, GraphicsContextFlags.Default | GraphicsContextFlags.ForwardCompatible )
		{

		}
	}
}
