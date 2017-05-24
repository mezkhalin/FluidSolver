using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluidSolver
{
    public enum PaintMode
    {
        Density,
        Obstacle
    }

    public partial class Form1 : Form
    {
        private Solver solver;
        private SolverParams Params;

        private Timer timer;

        #region UI related
        private PaintMode paintMode = PaintMode.Density;
        private RenderMode renderMode = RenderMode.Density;
        private bool renderObstacles = true;
        #endregion

        public Form1()
        {
            InitializeComponent();

            Shown += Form1_Shown;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            NewSimulation();

            KeyUp += Form1_KeyUp;
        }

        /// <summary>
        /// Brings up the new simulation form
        /// and responds accordingly
        /// </summary>
        private void NewSimulation ()
        {
            pauseSim();
            using (NewSimForm form = new NewSimForm(Params))
            {
                if(form.ShowDialog() == DialogResult.OK)
                {
                    solver = new Solver();
                    Params = form.Params;
                    initSim();
                }
                else
                {
                    resumeSim();
                }
            }
        }

        /// <summary>
        /// Initialize simulation using local parameters
        /// </summary>
        private void initSim ()
        {
            if (timer != null)
            {
                timer.Tick -= Timer_Tick;
                timer.Dispose();
            }

            solver.Init(Params);
            fluidControl.GridSize(Params.Width, Params.Height);

            timer = new Timer();
            timer.Interval = (int)(1f / Params.Dt);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void pauseSim ()
        {
            if (timer == null) return;
            timer.Stop();
        }

        private void resumeSim ()
        {
            if (timer == null) return;
            timer.Start();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Console.WriteLine("> " + e.KeyCode);
            //if (e.KeyCode == Keys.Space) fs.Init();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            HandleInput();
            solver.Step();
            fluidControl.Render(solver, renderMode, renderObstacles);
        }

        private void HandleInput ()
        {
            for (int x = (Params.Width / 2) - 5; x < (Params.Width / 2) + 5; x++)
                solver.HeatField[solver.IX(x, Params.Height - 5, 0)] += 25f * Params.Dt;
            //solver.setup_sources_and_forces();

            if (!fluidControl.MouseIsDown) return;

            Point pos = fluidControl.GridMousePosition();

            if (pos.X <= 0 || pos.Y <= 0) return;

            if (MouseButtons == MouseButtons.Left)
            {
                Point vel = fluidControl.MouseVelocity();
                solver.VelXPrev[solver.IX(pos.X, pos.Y, 0)] = vel.X * Params.Force * Params.Dt;
                solver.VelYPrev[solver.IX(pos.X, pos.Y, 0)] = vel.Y * Params.Force * Params.Dt;
            }
            else if (MouseButtons == MouseButtons.Right)
            {
                switch(paintMode)
                {
                    case PaintMode.Density:
                        solver.DensityFieldPrev[solver.IX(pos.X, pos.Y, 0)] += Params.Source * Params.Dt;
                        break;
                    case PaintMode.Obstacle:
                        solver.Obstacles[solver.IX(pos.X, pos.Y, 0)] = 1;
                        break;
                }
            }
        }

        #region Toolstrip Handlers

        private void toolStripNew_Click(object sender, EventArgs e)
        {
            NewSimulation();
        }

        private void toolStripPaintMode_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            toolStripPaintMode.Image = e.ClickedItem.Image;
            UncheckDropDownItems(e.ClickedItem as ToolStripMenuItem);
            switch(e.ClickedItem.AccessibleName)
            {
                case "Density":
                    paintMode = PaintMode.Density;
                    break;
                case "Obstacles":
                    paintMode = PaintMode.Obstacle;
                    break;
            }
        }

        private void toolStripRenderMode_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            toolStripRenderMode.Image = e.ClickedItem.Image;
            UncheckDropDownItems(e.ClickedItem as ToolStripMenuItem);
            switch (e.ClickedItem.AccessibleName)
            {
                case "Density":
                    renderMode = RenderMode.Density;
                    break;
                case "Heat":
                    renderMode = RenderMode.Heat;
                    break;
                case "Pressure":
                    renderMode = RenderMode.Pressure;
                    break;
            }
        }

        private void toolStripRenderObst_Click(object sender, EventArgs e)
        {
            renderObstacles = toolStripRenderObst.Checked;
        }

        /// <summary>
        /// Unchecks all other drop down items from the parent list of selectedItem
        /// selectedItem itself is excluded and remains only checked item
        /// </summary>
        /// <param name="item"></param>
        private void UncheckDropDownItems (ToolStripMenuItem selectedItem)
        {
            selectedItem.Checked = true;

            // Select the other MenuItens from the ParentMenu(OwnerItens) and unchecked this,
            // The current Linq Expression verify if the item is a real ToolStripMenuItem
            // and if the item is a another ToolStripMenuItem to uncheck this.
            foreach (var ltoolStripMenuItem in (from object
                                                    item in selectedItem.Owner.Items
                                                let ltoolStripMenuItem = item as ToolStripMenuItem
                                                where ltoolStripMenuItem != null
                                                where !item.Equals(selectedItem)
                                                select ltoolStripMenuItem))
            {
                (ltoolStripMenuItem).Checked = false;
            }
        }

        #endregion
    }
}
