using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidSolver
{
    public class SolverParams
    {
        public int Width        = 32;
        public int Height       = 32;
        public int Depth        = 1;
        public float Dt         = 0.1f;
        public float Tamb       = 0f; // ambient temperature
        public float Source     = 10f;
        public float Force      = 10f;
        public bool Vorticity   = true;   // vorticity confinement
        public bool Temperature = true;   // simulate temperature?
    }
}
