using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrowEditor.UIForms;

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
			CrowEditorApp.Init ();

			OpenTK.Graphics.GraphicsContext.ShareContexts = false;

			Thread renderer = new Thread ( Renderer );
			renderer.IsBackground = true;
			renderer.Start ();

			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault ( false );
			using ( var window = new MainWindow () )
			{
				Application.Run ( window );
			}
		}


		static void Renderer ()
		{
			while ( true )
			{
				CrowEditorApp.UpdateRenderView ();

				Thread.Sleep ( 1 );
			}
		}
	}
}
