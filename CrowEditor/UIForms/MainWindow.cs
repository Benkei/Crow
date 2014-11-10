using System;
using System.Windows.Forms;
using CrowEditor.Jobs;
using WeifenLuo.WinFormsUI.Docking;

namespace CrowEditor.UIForms
{
	public partial class MainWindow : Form
	{
		public MainWindow ()
		{
			InitializeComponent ();

			var f4 = new DockSceneView ();
			f4.Show ( dockPanel1, DockState.Document );
			f4 = new DockSceneView ();
			f4.Show ( dockPanel1, DockState.Document );
			f4 = new DockSceneView ();
			f4.Show ( dockPanel1, DockState.Document );
			f4 = new DockSceneView ();
			f4.Show ( dockPanel1, DockState.Document );
		}

		protected override void OnLoad ( EventArgs e )
		{
			base.OnLoad ( e );

			if ( AssetDatabase.LoadProjectFile ( "../../../Sample_ProjectTest/ProjectTest.proj" ) )
			{
				CrowEditorApp.m_LogicThread.JobScheduler.AddJob ( new AssetValidation () );
			}
		}
	}
}
