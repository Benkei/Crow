using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

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
			//Application.EnableVisualStyles ();
			//Application.SetCompatibleTextRenderingDefault ( false );
			//Application.Run ( new Form1 () );


			var appDomain = AppDomain.CreateDomain ( "ChildDomain 0" );
			//appDomain.ExecuteAssemblyByName()
			var type = typeof ( RemoteObject );
			var remote = (RemoteObject)appDomain.CreateInstanceFromAndUnwrap (
				type.Assembly.Location,
				type.FullName
			);

			Console.WriteLine ( "My thread!" );
			Console.WriteLine ();

			remote.Parent = AppDomain.CurrentDomain;
			remote.Init ();

			remote.Test ();

			Console.WriteLine ( remote.Text );

			AppDomain.Unload ( appDomain );


			using ( var win = new MainWindow () )
			{
				win.MouseMove += ( a, b ) =>
				{
					Console.WriteLine ( "Move" );
				};

				win.Run ();
			}
		}
	}

	class RemoteObject : MarshalByRefObject
	{
		public AppDomain Parent;
		public string Text = "WoW";

		[NonSerialized]
		private RemoteHost Host;

		public void Init ()
		{
			var type = typeof ( RemoteHost );
			Host = (RemoteHost)Parent.CreateInstanceFromAndUnwrap (
				type.Assembly.Location,
				type.FullName
			);
			Host.TestHost ();
		}

		public void Test ()
		{
			Console.WriteLine ( Parent.FriendlyName );
			Console.WriteLine ( AppDomain.CurrentDomain.FriendlyName );
			Console.WriteLine ( Text );
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

	class WindowHandler
	{
		public void ProcessEvent ()
		{
			MSG msg;
			while ( Native.PeekMessage ( out msg, IntPtr.Zero, 0, 0, PeekMessageFlags.Remove ) != 0 )
			{
				Native.TranslateMessage ( ref msg );
				Native.DispatchMessage ( ref msg );
			}
		}
	}

}
