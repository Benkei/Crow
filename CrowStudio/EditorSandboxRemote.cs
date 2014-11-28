using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowStudio
{
	class EditorSandboxRemote : MarshalByRefObject
	{
		[NonSerialized]
		private EditorHostRemote Host;
		[NonSerialized]
		private EditorSandbox Logic;

		public int Width;
		public int Height;

		public void Init ( AppDomain parent, IntPtr[] glFuncPtrs, IntPtr[] gl4FuncPtrs, IntPtr dc, IntPtr hdr )
		{
			var type = typeof ( EditorHostRemote );
			Host = (EditorHostRemote)parent.CreateInstanceFromAndUnwrap (
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
}
