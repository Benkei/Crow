using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CrowEditor.UIForms
{
	public partial class MainWindow : Form
	{
		public MainWindow ()
		{
			InitializeComponent ();

			var f2 = new Window ();
			f2.Show ( dockPanel1, DockState.DockLeft );
			var f3 = new Window ();
			f3.Show ( dockPanel1, DockState.DockRight );
			var f4 = new DockSceneView ();
			f4.Show ( dockPanel1, DockState.Document );
		}
	}
}
