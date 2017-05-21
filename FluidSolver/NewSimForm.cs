using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluidSolver
{
    public partial class NewSimForm : Form
    {
        public SolverParams Params;

        public NewSimForm(SolverParams Params)
        {
            InitializeComponent();
            this.Params = (Params != null) ? Params : new SolverParams();
            // TODO: setup values from params
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dtAutoCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            dtCtrl.Enabled = !dtAutoCheckBox.Checked;
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            Params.Width = (int)WidthCtrl.Value;
            Params.Height = (int)HeightCtrl.Value;
            Params.Depth = (int)DepthCtrl.Value;
            Params.Force = (float)ForceCtrl.Value;
            Params.Source = (float)SourceCtrl.Value;
            Params.Vorticity = VorticityCtrl.Checked;
            Params.Dt = (dtAutoCheckBox.Enabled) ? (60 / 1000f) : (float)dtCtrl.Value;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
