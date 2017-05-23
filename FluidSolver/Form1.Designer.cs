namespace FluidSolver
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripNew = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripRenderObst = new System.Windows.Forms.ToolStripButton();
            this.toolStripPaintMode = new System.Windows.Forms.ToolStripDropDownButton();
            this.densityToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.pressureToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripRenderMode = new System.Windows.Forms.ToolStripDropDownButton();
            this.densityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.heatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pressureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fluidControl = new FluidSolver.FluidControl();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripNew,
            this.toolStripSeparator1,
            this.toolStripPaintMode,
            this.toolStripRenderObst,
            this.toolStripRenderMode});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(284, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripNew
            // 
            this.toolStripNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripNew.Image = global::FluidSolver.Properties.Resources.NewFile_16x_24;
            this.toolStripNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripNew.Name = "toolStripNew";
            this.toolStripNew.Size = new System.Drawing.Size(23, 22);
            this.toolStripNew.Text = "toolStripNew";
            this.toolStripNew.Click += new System.EventHandler(this.toolStripNew_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.fluidControl, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(284, 287);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripRenderObst
            // 
            this.toolStripRenderObst.Checked = true;
            this.toolStripRenderObst.CheckOnClick = true;
            this.toolStripRenderObst.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripRenderObst.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRenderObst.Image = ((System.Drawing.Image)(resources.GetObject("toolStripRenderObst.Image")));
            this.toolStripRenderObst.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRenderObst.Name = "toolStripRenderObst";
            this.toolStripRenderObst.Size = new System.Drawing.Size(23, 22);
            this.toolStripRenderObst.Text = "toolStripRenderObst";
            // 
            // toolStripPaintMode
            // 
            this.toolStripPaintMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripPaintMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.densityToolStripMenuItem2,
            this.pressureToolStripMenuItem1});
            this.toolStripPaintMode.Image = ((System.Drawing.Image)(resources.GetObject("toolStripPaintMode.Image")));
            this.toolStripPaintMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripPaintMode.Name = "toolStripPaintMode";
            this.toolStripPaintMode.Size = new System.Drawing.Size(29, 22);
            this.toolStripPaintMode.Text = "toolStripDropDownButton1";
            this.toolStripPaintMode.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStripPaintMode_DropDownItemClicked);
            // 
            // densityToolStripMenuItem2
            // 
            this.densityToolStripMenuItem2.AccessibleName = "Density";
            this.densityToolStripMenuItem2.Name = "densityToolStripMenuItem2";
            this.densityToolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
            this.densityToolStripMenuItem2.Text = "Density";
            // 
            // pressureToolStripMenuItem1
            // 
            this.pressureToolStripMenuItem1.AccessibleName = "Obstacles";
            this.pressureToolStripMenuItem1.Name = "pressureToolStripMenuItem1";
            this.pressureToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.pressureToolStripMenuItem1.Text = "Obstacles";
            // 
            // toolStripRenderMode
            // 
            this.toolStripRenderMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRenderMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.densityToolStripMenuItem,
            this.heatToolStripMenuItem,
            this.pressureToolStripMenuItem});
            this.toolStripRenderMode.Image = ((System.Drawing.Image)(resources.GetObject("toolStripRenderMode.Image")));
            this.toolStripRenderMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRenderMode.Name = "toolStripRenderMode";
            this.toolStripRenderMode.Size = new System.Drawing.Size(29, 22);
            this.toolStripRenderMode.Text = "toolStripDropDownButton2";
            // 
            // densityToolStripMenuItem
            // 
            this.densityToolStripMenuItem.AccessibleName = "Density";
            this.densityToolStripMenuItem.Name = "densityToolStripMenuItem";
            this.densityToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.densityToolStripMenuItem.Text = "Density";
            // 
            // heatToolStripMenuItem
            // 
            this.heatToolStripMenuItem.AccessibleName = "Heat";
            this.heatToolStripMenuItem.Name = "heatToolStripMenuItem";
            this.heatToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.heatToolStripMenuItem.Text = "Heat";
            // 
            // pressureToolStripMenuItem
            // 
            this.pressureToolStripMenuItem.AccessibleName = "Pressure";
            this.pressureToolStripMenuItem.Name = "pressureToolStripMenuItem";
            this.pressureToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.pressureToolStripMenuItem.Text = "Pressure";
            // 
            // fluidControl
            // 
            this.fluidControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fluidControl.BackColor = System.Drawing.SystemColors.ControlDark;
            this.fluidControl.Location = new System.Drawing.Point(3, 28);
            this.fluidControl.Name = "fluidControl";
            this.fluidControl.Size = new System.Drawing.Size(278, 256);
            this.fluidControl.TabIndex = 0;
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(284, 287);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private FluidControl fluidControl;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStripButton toolStripNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripRenderObst;
        private System.Windows.Forms.ToolStripDropDownButton toolStripPaintMode;
        private System.Windows.Forms.ToolStripMenuItem densityToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem pressureToolStripMenuItem1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripRenderMode;
        private System.Windows.Forms.ToolStripMenuItem densityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem heatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pressureToolStripMenuItem;
    }
}

