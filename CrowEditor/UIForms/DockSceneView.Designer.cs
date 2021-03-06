﻿namespace CrowEditor.UIForms
{
	partial class DockSceneView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose ( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose ();
			}
			base.Dispose ( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DockSceneView));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.glView = new CrowEditor.CrowGLControl();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox1,
            this.toolStripButton1});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(680, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripComboBox1
			// 
			this.toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBox1.Items.AddRange(new object[] {
            "Textured",
            "Wireframe",
            "Textured Wire"});
			this.toolStripComboBox1.Name = "toolStripComboBox1";
			this.toolStripComboBox1.Size = new System.Drawing.Size(75, 25);
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(38, 22);
			this.toolStripButton1.Text = "Light";
			// 
			// glView
			// 
			this.glView.BackColor = System.Drawing.Color.Black;
			this.glView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glView.Location = new System.Drawing.Point(0, 25);
			this.glView.Name = "glView";
			this.glView.Size = new System.Drawing.Size(680, 455);
			this.glView.TabIndex = 1;
			this.glView.VSync = false;
			this.glView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glView_KeyDown);
			this.glView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.glView_KeyUp);
			this.glView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glView_MouseDown);
			this.glView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glView_MouseMove);
			this.glView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glView_MouseUp);
			// 
			// DockSceneView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(680, 480);
			this.Controls.Add(this.glView);
			this.Controls.Add(this.toolStrip1);
			this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.HelpButton = true;
			this.Name = "DockSceneView";
			this.Text = "Scene";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
		private CrowGLControl glView;

	}
}