using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Gwen.Sample.OpenTK;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Platform;

namespace CrowEditor2
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		[LoaderOptimization ( LoaderOptimization.MultiDomain )]
		static void Main ()
		{
			using ( var host = new EditorHost () )
			{
				host.Run ();
			}

			//using ( var win = new MainWindow () )
			//using ( var sandbox = new Isolated<EditorSandbox> () )
			//{
			//	win.Visible = true;

			//	var hdc = wgl.GetCurrentDC ();
			//	var hrc = wgl.GetCurrentContext ();
			//	win.Context.MakeCurrent ( null );

			//	Console.WriteLine ( "Start sandbox" );

			//	var remote = sandbox.Value;
			//	remote.Width = win.Width;
			//	remote.Height = win.Height;
			//	remote.Init ( AppDomain.CurrentDomain,
			//		OpenTK.Graphics.OpenGL.GL.EntryPoints,
			//		OpenTK.Graphics.OpenGL4.GL.EntryPoints,
			//		hdc, hrc );

			//	remote.Run ();

			//	win.KeyUp += ( sender, e ) =>
			//	{

			//	};

			//	while ( !win.IsExiting )
			//	{
			//		win.ProcessEvents ();
			//		Thread.Sleep ( 1 );
			//	}

			//	remote.Exit ();
			//}
		}
	}

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

			hDeviceContext = wgl.GetCurrentDC ();
			hRenderContext = wgl.GetCurrentContext ();
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

		void m_MainWindow_KeyUp ( object sender, OpenTK.Input.KeyboardKeyEventArgs e )
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

	class EditorSandboxRemote : MarshalByRefObject
	{
		[NonSerialized]
		private RemoteHostRemote Host;
		[NonSerialized]
		private EditorSandbox Logic;

		public int Width;
		public int Height;

		public void Init ( AppDomain parent, IntPtr[] glFuncPtrs, IntPtr[] gl4FuncPtrs, IntPtr dc, IntPtr hdr )
		{
			var type = typeof ( RemoteHostRemote );
			Host = (RemoteHostRemote)parent.CreateInstanceFromAndUnwrap (
				type.Assembly.Location,
				type.FullName
			);
			Host.TestHost ();

			Logic = new EditorSandbox ( this, glFuncPtrs, gl4FuncPtrs, dc, hdr );

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
		}

		void CurrentDomain_UnhandledException ( object sender, UnhandledExceptionEventArgs e )
		{
			Debug.WriteLine ( "Appdomain Unhandled " + e.ExceptionObject );
		}

		void CurrentDomain_FirstChanceException ( object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e )
		{
			Debug.WriteLine ( "Appdomain FirstChanceException " + e.Exception );
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


	class WindowHandler
	{
		public IntPtr hDeviceContext;
		public IntPtr hRenderContext;
	}


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
		HelloGL3 hello;
		SimpleWindow editorUI;

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

			dummyWindowInfo = OpenTK.Platform.Utilities.CreateDummyWindowInfo ();
			dummyContext = new GraphicsContext ( new ContextHandle ( hrc ), null );
			dummyContext.MakeCurrent ( dummyWindowInfo );

			glFuncPtrs.CopyTo ( OpenTK.Graphics.OpenGL.GL.EntryPoints, 0 );
			gl4FuncPtrs.CopyTo ( OpenTK.Graphics.OpenGL4.GL.EntryPoints, 0 );

			//var ptr = wgl.GetProcAddress ( "glViewport" );
			//Util.CheckWin32Error ();

			hello = new HelloGL3 ();
			hello.OnLoad ();

			editorUI = new SimpleWindow ();
			editorUI.OnLoad ( 640, 480 );

			GL.ClearColor ( System.Drawing.Color.MidnightBlue );

			GL.Viewport ( 0, 0, 640, 480 );

			var timer = Stopwatch.StartNew ();
			float deltaTime = 0;
			while ( Running )
			{
				// render graphics

				GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

				GL.Enable ( EnableCap.DepthTest );

				hello.OnUpdateFrame ( deltaTime );
				hello.OnRenderFrame ();


				GL.Disable ( EnableCap.DepthTest );

				editorUI.OnUpdateFrame ();
				editorUI.OnRenderFrame ( 640, 480 );

				wgl.SwapBuffers ( hdc );

				deltaTime = (float)timer.Elapsed.TotalSeconds;
				timer.Restart ();
			}

			editorUI.Dispose ();
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


	class RemoteHostRemote : MarshalByRefObject
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


	class Util
	{
		public static void CheckWin32Error ()
		{
			int err = Marshal.GetLastWin32Error ();
			if ( err != 0 )
			{
				throw new System.ComponentModel.Win32Exception ( err );
			}
		}


		[DllImport ( "kernel32.dll" )]
		internal static extern IntPtr GetProcAddress ( IntPtr handle, string funcname );

		[DllImport ( "kernel32.dll" )]
		internal static extern IntPtr GetProcAddress ( IntPtr handle, IntPtr funcname );
	}

	#region class PixelFormatDescriptor

	[StructLayout ( LayoutKind.Sequential )]
	public class PixelFormatDescriptor
	{
		public short size = PfdSize;
		public short version = 1;
		public PfdPixelFormat flags = PfdPixelFormat.DRAW_TO_WINDOW | PfdPixelFormat.SUPPORT_OPENGL | PfdPixelFormat.DOUBLEBUFFER;
		public PfdPixelType pixelType = PfdPixelType.RGBA;
		public byte colorBits = 32;
		public byte redBits = 8;
		public byte redShift;
		public byte greenBits = 8;
		public byte greenShift;
		public byte blueBits = 8;
		public byte blueShift;
		public byte alphaBits = 8;
		public byte alphaShift;
		public byte accumBits = 0;
		public byte accumRedBits;
		public byte accumGreenBits;
		public byte accumBlueBits;
		public byte accumAlphaBits;
		public byte depthBits = 16;
		public byte stencilBits;
		public byte auxBuffers;
		public PfdLayerType layerType = PfdLayerType.MAIN_PLANE;
		public byte reserved;
		public int layerMask;
		public int visibleMask;
		public int damageMask;


		private const short PfdSize = 40;
	}

	#endregion

	#region enum PfdPixelFormat

	[Flags]
	public enum PfdPixelFormat : uint
	{
		DOUBLEBUFFER = 0x00000001,
		STEREO = 0x00000002,
		DRAW_TO_WINDOW = 0x00000004,
		DRAW_TO_BITMAP = 0x00000008,
		SUPPORT_GDI = 0x00000010,
		SUPPORT_OPENGL = 0x00000020,
		GENERIC_FORMAT = 0x00000040,
		NEED_PALETTE = 0x00000080,
		NEED_SYSTEM_PALETTE = 0x00000100,
		SWAP_EXCHANGE = 0x00000200,
		SWAP_COPY = 0x00000400,
		SWAP_LAYER_BUFFERS = 0x00000800,
		GENERIC_ACCELERATED = 0x00001000,
		SUPPORT_DIRECTDRAW = 0x00002000,

		/* PIXELFORMATDESCRIPTOR flags for use in ChoosePixelFormat only */
		PFD_DEPTH_DONTCARE = 0x20000000,
		PFD_DOUBLEBUFFER_DONTCARE = 0x40000000,
		PFD_STEREO_DONTCARE = 0x80000000,
	}

	#endregion enum PfdPixelFormat

	#region enum PfdPixelType

	[Flags]
	public enum PfdPixelType : byte
	{
		RGBA = 0,
		COLORINDEX = 1
	}

	#endregion enum PfdPixelType

	#region enum PfdLayerType

	[Flags]
	public enum PfdLayerType : byte
	{
		MAIN_PLANE = 0,
		OVERLAY_PLANE = 1,
		UNDERLAY_PLANE = 0xff
	}

	#endregion enum PfdLayerType

	#region wgl

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

		[DllImport ( "opengl32.dll", EntryPoint = "wglGetProcAddress", SetLastError = true ), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetProcAddress ( string functionName );
	}

	#endregion

	#region win32

	public static class win32
	{
		[DllImport ( "gdi32", EntryPoint = "ChoosePixelFormat" ), System.Security.SuppressUnmanagedCodeSecurity]
		public static extern int ChoosePixelFormat ( IntPtr hdc, PixelFormatDescriptor p_pfd );

		[DllImport ( "gdi32", EntryPoint = "SetPixelFormat" ), System.Security.SuppressUnmanagedCodeSecurity]
		public static extern bool SetPixelFormat ( IntPtr hdc, int iPixelFormat, PixelFormatDescriptor p_pfd );

		[DllImport ( "gdi32", EntryPoint = "DescribePixelFormat" ), System.Security.SuppressUnmanagedCodeSecurity]
		public static extern int DescribePixelFormat ( IntPtr deviceContext, int pixel, int pfdSize, PixelFormatDescriptor pixelFormat );

		[DllImport ( "user32", EntryPoint = "GetDC", SetLastError = true ), System.Security.SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetDC ( IntPtr hwnd );

		[DllImport ( "user32", EntryPoint = "ReleaseDC" ), System.Security.SuppressUnmanagedCodeSecurity]
		public static extern int ReleaseDC ( IntPtr hwnd, IntPtr dc );

		[DllImport ( "user32.dll", SetLastError = true )]
		public static extern int SetWindowLong ( IntPtr hWnd, int nIndex, int dwNewLong );

		[DllImport ( "user32.dll", SetLastError = true )]
		public static extern int GetWindowLong ( IntPtr hWnd, int nIndex );

		[DllImport ( "user32.dll", SetLastError = true )]
		public static extern IntPtr SetParent ( IntPtr hWndChild, IntPtr hWndNewParent );
	}

	#endregion

}
