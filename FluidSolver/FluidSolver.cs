using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace FluidSolver
{
    public class FluidSolver
    {
        public float dt { get; private set; }   // delta time

        private int n;  // num cells
        private int numCellsX;
        private int numCellsY;
        private int size;   // array size (rename these)

        public float[] vx; // velocities
        public float[] vy;
        public float[] vx_prev;
        public float[] vy_prev;

        public float[] df; // densities
        public float[] df_prev;
        
        public float[] src_df;  // source/sink density

        private float Diffusion = 0f;
        private float Viscosity = 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int I(int x, int y, int w) { return ((w + 2) * y) + x; }

        private static void swap(ref float[] a, ref float[] b)
        {
            float[] temp = a;
            a = b;
            b = temp;
        }

        public FluidSolver ( SolverParams Params )
        {
            numCellsX = Params.Width;
            numCellsY = n = Params.Height;
            size = (numCellsX + 2) * (numCellsY + 2);
            dt = Params.dt;
            Diffusion = Params.Diffusion;
            Viscosity = Params.Viscosity;

            Init();
        }

        public void Init ()
        {
            vx      = new float[size];
            vy      = new float[size];
            vx_prev = new float[size];
            vy_prev = new float[size];
            df      = new float[size];
            df_prev = new float[size];
            src_df  = new float[size];
        }

        public void Run ()
        {
            velocity_step(vx, vy, vx_prev, vy_prev);
            density_step(df, df_prev, vx, vy);
        }

        private void add_source(float[] dest, float[] src)
        {
            for (int i = 0; i < size; i++) dest[i] += dt * src[i];
        }

        private void density_step(float[] df, float[] df_prev, float[] vx, float[] vy)
        {
            add_source(df, df_prev);
            swap(ref df_prev, ref df); diffuse(df, df_prev, Diffusion, 0);
            swap(ref df_prev, ref df); advect(df, df_prev, vx, vy, 0);
        }

        private void velocity_step(float[] vx, float[] vy, float[] vx_prev, float[] vy_prev)
        {
            add_source(vx, vx_prev); add_source(vy, vy_prev);

            swap(ref vx_prev, ref vx); diffuse(vx, vx_prev, Viscosity, 1);
            swap(ref vy_prev, ref vy); diffuse(vy, vy_prev, Viscosity, 2);

            project(vx, vy, vx_prev, vy_prev);

            swap(ref vx_prev, ref vx); swap(ref vy_prev, ref vy);

            advect(vx, vx_prev, vx_prev, vy_prev, 1);
            advect(vy, vy_prev, vx_prev, vy_prev, 2);

            project(vx, vy, vx_prev, vy_prev);
        }

        /**
        * DENSITY FUNCTIONS
        * */

        private void diffuse(float[] cur, float[] prev, float fac, int b)
        {
            float a = dt * fac * numCellsX * numCellsY;
            float frac = (4 * a) + 1;

            linear(cur, prev, a, frac, b);
        }

        private void advect(float[] df, float[] df_prev, float[] vx, float[] vy, int b)
        {
            int i0, j0, i1, j1;
            float x, y, s0, t0, s1, t1;
            float dt0 = dt * n;
            float dtX = dt * numCellsX;
            float dtY = dt * numCellsY;
            float NPHalf = n + .5f;
            float XPHalf = numCellsX + .5f;
            float YPHalf = numCellsY + .5f;

            for (int i = 1; i <= numCellsX; i++)
            {
                for (int j = 1; j <= numCellsY; j++)
                {
                    x = i - dtX * vx[I(i, j, numCellsY)];  // step backwards
                    y = j - dtY * vy[I(i, j, numCellsY)];
                    
                    // clamp positions
                    if (x < .5f) x = .5f;
                    if (x > XPHalf) x = XPHalf;
                    i0 = (int)x;
                    i1 = i0 + 1;

                    if (y < .5f) y = .5f;
                    if (y > YPHalf) y = YPHalf;
                    j0 = (int)y;
                    j1 = j0 + 1;

                    // find pos
                    s1 = x - i0; s0 = 1f - s1; t1 = y - j0; t0 = 1f - t1;

                    // adjust grid
                    df[I(i, j, numCellsY)] = s0 * (t0 * df_prev[I(i0, j0, numCellsY)] + t1 * df_prev[I(i0, j1, numCellsY)]) +
                                             s1 * (t0 * df_prev[I(i1, j0, numCellsY)] + t1 * df_prev[I(i1, j1, numCellsY)]);
                }
            }

            bounds(b,  df);
        }

        /**
         * VELOCITY FUNCTIONS
         * */

        private void project(float[] vx, float[] vy, float[] p, float[] div)
        {
            float h = 1f / numCellsX;
            int x, y, pos;
            int N2 = numCellsY + 2;

            for (x = 1; x <= numCellsX; x++)
            {
                for (y = 1; y <= numCellsY; y++)
                {
                    pos = I(x, y, numCellsY);
                    div[pos] = -.5f * h * (vx[pos +  1] - vx[pos -  1] +
                                           vy[pos + N2] - vy[pos - N2]);
                    p[pos] = 0f;
                }
            }

            bounds(0, div);
            bounds(0, p);

            linear(p, div, 1f, 4f, 0);

            for (x = 1; x <= numCellsX; x++)
            {
                for (y = 1; y <= numCellsY; y++)
                {
                    pos = I(x, y, numCellsY);
                    vx[pos] -= .5f * (p[pos +  1] - p[pos -  1]) * numCellsX;
                    vy[pos] -= .5f * (p[pos + N2] - p[pos - N2]) * numCellsY;
                }
            }

            bounds(1,  vx);
            bounds(2,  vy);
        }

        private void linear (float[] cur, float[] prev, float a, float f, int b)
        {
            int N2 = numCellsY + 2;
            int pos;
            for (int k = 0; k < 20; k++)
            {
                for (int x = 1; x <= numCellsX; x++)
                {
                    for (int y = 1; y <= numCellsY; y++)
                    {
                        pos = I(x, y, numCellsY);
                        cur[pos] = (prev[pos] + a * (cur[pos -  1] + cur[pos +  1] +
                                                     cur[pos - N2] + cur[pos + N2])) / f;
                    }
                }

                bounds(b, cur);
            }
        }

        private void bounds(int b,  float[] x)
        {
            // walls
            for (int i = 1; i <= n; i++)
            {
                x[I(0    , i, n)] = b == 1 ? -x[I(1, i, n)] : x[I(1, i, n)];
                x[I(n + 1, i, n)] = b == 1 ? -x[I(n, i, n)] : x[I(n, i, n)];
                x[I(i    , 0, n)] = b == 2 ? -x[I(i, 1, n)] : x[I(i, 1, n)]; //-x[I(i, 1)]
                x[I(i, n + 1, n)] = b == 2 ? -x[I(i, n, n)] : x[I(i, n, n)];
            }
            // corners
            x[I(0    ,     0, n)] = .5f * (x[I(1,     0, n)] + x[I(0    , 1, n)]);
            x[I(0    , n + 1, n)] = .5f * (x[I(1, n + 1, n)] + x[I(0    , n, n)]);
            x[I(n + 1,     0, n)] = .5f * (x[I(n,     0, n)] + x[I(n + 1, 1, n)]);
            x[I(n + 1, n + 1, n)] = .5f * (x[I(n, n + 1, n)] + x[I(n + 1, n, n)]);
        }
    }
}
