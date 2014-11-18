using System;
using System.Diagnostics;
using System.Windows.Forms;
using CrowEditor.UIForms;
using OpenTK.Graphics;

namespace CrowEditor
{
	static class Program
	{
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main ()
		{
			NativeMethods.LoadLibrary ( "FreeImage.dll" );

			GraphicsContext.DirectRendering = false;
			GraphicsContext.ShareContexts = false;

			//TextWriterTraceListener writer = new TextWriterTraceListener ( System.Console.Out );
			//Debug.Listeners.Add ( writer );

			CrowEditorApp.Init ();

			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault ( false );
			using ( var window = new MainWindow () )
			{
				CrowEditorApp.m_MainForm = window;

				window.Show ();

				CrowEditorApp.Start ();

				Application.Run ( window );
			}

			CrowEditorApp.Terminate ();
		}
	}
}
