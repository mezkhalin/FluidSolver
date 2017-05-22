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
    public partial class Form1 : Form
    {
        private Solver solver;
        private SolverParams Params;

        private Timer timer;

        public Form1()
        {
            InitializeComponent();

            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
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
            using (NewSimForm form = new NewSimForm(Params))
            {
                if(form.ShowDialog() == DialogResult.OK)
                {
                    solver = new Solver();
                    Params = form.Params;
                    initSim();
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

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Space) fs.Init();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            HandleInput(solver.DensityFieldPrev, solver.VelXPrev, solver.VelYPrev);
            solver.Step();
            fluidControl.Render(solver.DensityField, solver);
            /*HandleInput(fs.df_prev, fs.vx_prev, fs.vy_prev);
            fs.Run();
            fluidControl.Render(fs.df);*/
        }

        private void HandleInput (float[] d, float[] x, float[] y)
        {
            //solver.setup_sources_and_forces();

            if (!fluidControl.MouseIsDown) return;

            Point pos = fluidControl.GridMousePosition();

            if (pos.X <= 0 || pos.Y <= 0) return;

            if (MouseButtons == MouseButtons.Left)
            {
                Point vel = fluidControl.MouseVelocity();
                x[solver.IX(pos.X, pos.Y, 0)] = vel.X * Params.Force;
                y[solver.IX(pos.X, pos.Y, 0)] = vel.Y * Params.Force;
            }
            else if (MouseButtons == MouseButtons.Right)
            {
                d[solver.IX(pos.X, pos.Y, 0)] = Params.Source;
            }
        }

        private Point trans_point(Point p)
        {
            p.X = (int)((p.X / (float)ClientSize.Width) * Params.Height + 1);
            p.Y = (int)((p.Y / (float)ClientSize.Height) * Params.Height + 1);
            if (p.X < 1) p.X = 1; if (p.X > Params.Height) p.X = Params.Height;
            if (p.Y < 1) p.Y = 1; if (p.Y > Params.Height) p.Y = Params.Height;
            return p;
        }

        #region Toolstrip Handlers

        private void toolStripNew_Click(object sender, EventArgs e)
        {
            NewSimulation();
        }

        #endregion
    }
}
