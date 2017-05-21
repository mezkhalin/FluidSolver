using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidSolver
{
    public class SolverParams
    {
        /*
        public int Width { get; set; }
        public int Height { get; set; }
        public float Diffusion { get; set; }
        public float Viscosity { get; set; }
        public float dt { get; set; }*/

        public int Width        = 64;
        public int Height       = 64;
        public int Depth        = 1;
        public float Dt         = 0.1f;
        public float Source     = 10f;
        public float Force      = 10f;
        public bool Vorticity   = true;   // vorticity confinement
    }
}
