using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// TODO:
/// CHECK FOR_EACH VERSUS FOR_EACH_INTERNAL
/// (replace clamp function with inline MinMax statements)
/// move initializing of vars from inside for_each to outside
/// </summary>

namespace FluidSolver
{
    public struct IntPos
    {
        public int X, Y, Z;
        public IntPos (int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class Solver
    {
        public float[] DensityField { get { return density; } }
        public float[] DensityFieldPrev { get { return density_prev; } }
        public float[] VelXPrev { get { return vel_x_prev; } }
        public float[] VelYPrev { get { return vel_y_prev; } }
        public int[] Obstacles { get { return obstacles; } }

        private int NX, NY, NZ; // number of cells
        private IntPos dims;    // total dimensions
        private int array_size; // size of data arrays

        private float dt;   // simulation time step
        private float dt_inv; // negative time step * NX
        private float force, source;    // default values of user velocity/density
        private float tAmb; // ambient temperature

        #region flags
        private bool vorticity;
        private bool temperature;
        #endregion

        #region data fields
        private float[] vel_x, vel_y, vel_z, vel_x_prev, vel_y_prev, vel_z_prev;
        private float[] density, density_prev;
        private float[] heat, heat_prev;
        private float[] pressure, pressure_prev;
        private float[] curl;
        private float[] compressibility;
        private float[] divergence;
        private int[] obstacles;
        #endregion

        #region helper functions

        /// <summary>
        /// get array index from three coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int IX(int x, int y, int z) { return (z > 1) ? (x + (y * NX)) : (x + (y * NX) + (z * NX * NY)); }
        public int IXBLAS(int x, int y, int z) { return x + (z * NX) + (y * NX * NZ); }
        public static float MIX(float a, float b, float t) { return ((1.0f) - (t)) * (a) + ((t) * (b)); }
        public static float Clamp(float x, float min, float max) { return ((x) > (max)) ? (max) : (((x) < (min)) ? (min) : (x)); }
        public static int Clamp(int x, int min, int max) { return ((x) > (max)) ? (max) : (((x) < (min)) ? (min) : (x)); }

        public static void SWAP(ref float[] a, ref float[] b)
        {
            float[] tmp = a;
            a = b;
            b = tmp;
        }
        
        private float get_data(float[] d, int x, int y, int z)
        {
            x = Clamp(x, 0, NX - 1);
            y = Clamp(y, 0, NY - 1);
            z = Clamp(z, 0, NZ - 1);
            return d[IX(x, y, z)];
        }
        private int get_data(int[] d, int x, int y, int z)
        {
            x = Clamp(x, 0, NX - 1);
            y = Clamp(y, 0, NY - 1);
            z = Clamp(z, 0, NZ - 1);
            return d[IX(x, y, z)];
        }
        private bool is_obs_cell(int[] obs, int x, int y, int z)
        {
            return get_data(obs, x, y, z) != 0;
        }

        #endregion

        public Solver () { }
        public Solver (SolverParams param) { Init(param); }

        /// <summary>
        /// Initialize the solver with parameters
        /// </summary>
        /// <param name="param"></param>
        public void Init (SolverParams param)
        {
            NX = param.Width;
            NY = param.Height;
            NZ = param.Depth;
            dims = new IntPos(NX, NY, NZ);
            array_size = NX * NY * NZ;

            dt = param.Dt;
            dt_inv = -dt * NX;

            force = param.Force;
            source = param.Source;
            tAmb = param.Tamb;
            temperature = param.Temperature;
            vorticity = param.Vorticity;

            vel_x       = new float[array_size];
            vel_y       = new float[array_size];
            vel_z       = new float[array_size];
            vel_x_prev  = new float[array_size];
            vel_y_prev  = new float[array_size];
            vel_z_prev  = new float[array_size];
            density     = new float[array_size];
            density_prev = new float[array_size];
            heat        = new float[array_size];
            heat_prev   = new float[array_size];
            pressure    = new float[array_size];
            pressure_prev = new float[array_size];
            curl        = new float[array_size];
            compressibility = new float[array_size];
            divergence  = new float[array_size];
            obstacles   = new int[array_size];
        }

        /// <summary>
        /// Step simulation one frame
        /// Comments courtesy of github.com/kristofe
        /// </summary>
        public void Step()
        {
            advect_vel_RK2(vel_x, vel_y, vel_z, vel_x_prev, vel_y_prev, vel_z_prev, obstacles);

            if (temperature)
            {
                //Bouyance should be considered an external force and should be applied immediately after velocity advection
                buoyancy(vel_y, heat);
                //Need to advect heat
                advect_RK2(heat, heat_prev, vel_x, vel_y, vel_z, obstacles);
                //maybe damp it too
            }
            
            project(vel_x, vel_y, vel_z, divergence, pressure, pressure_prev, obstacles);

            advect_RK2(density, density_prev, vel_x, vel_y, vel_z, obstacles);

            if (vorticity)
                vorticity_confinement(vel_x, vel_y, vel_z, vel_x_prev, vel_y_prev, vel_z_prev);

            SWAP(ref vel_x, ref vel_x_prev);
            SWAP(ref vel_y, ref vel_y_prev);
            SWAP(ref density, ref density_prev);
            SWAP(ref heat, ref heat_prev);
        }

        #region simulation functions

        private void advect_vel_RK2(float[] x, float[] y, float[] z,
                                     float[] x_prev, float[] y_prev, float[] z_prev,
                                     int[] obs)
        {
            for (int k = 0; k < NZ; k++)
            {
                for (int j = 0; j < NY; j++)
                {
                    for (int i = 0; i < NX; i++)
                    {
                        //bool is_obs = is_obs_cell(obs, i, j, k);
                        Vector3 pos = new Vector3(i, j, k);
                        Vector3 orig_vel = new Vector3(x_prev[IX(i, j, k)], y_prev[IX(i, j, k)], z_prev[IX(i, j, k)]);

                        float halfDt = dt_inv * .5f;
                        Vector3 halfway_pos = new Vector3(pos.X + (halfDt * orig_vel.X),
                                                          pos.Y + (halfDt * orig_vel.Y),
                                                          pos.Z + (halfDt * orig_vel.Z));

                        // clamp
                        halfway_pos.X = Clamp(halfway_pos.X, 0.0f, NX - 1);
                        halfway_pos.Y = Clamp(halfway_pos.Y, 0.0f, NY - 1);
                        halfway_pos.Z = Clamp(halfway_pos.Z, 0.0f, NZ - 1);

                        Vector3 halfway_vel = new Vector3(
                                get_interpolated_val_with_obs(x_prev, obs, halfway_pos, 1f, dims),
                                get_interpolated_val_with_obs(y_prev, obs, halfway_pos, 1f, dims),
                                get_interpolated_val_with_obs(z_prev, obs, halfway_pos, 1f, dims)
                            );

                        Vector3 backtraced_pos = new Vector3(
                                pos.X + dt_inv * halfway_vel.X,
                                pos.Y + dt_inv * halfway_vel.Y,
                                pos.Z + dt_inv * halfway_vel.Z
                            );

                        // clamp
                        backtraced_pos.X = Clamp(backtraced_pos.X, 0.0f, NX - 1);
                        backtraced_pos.Y = Clamp(backtraced_pos.Y, 0.0f, NY - 1);
                        backtraced_pos.Z = Clamp(backtraced_pos.Z, 0.0f, NZ - 1);

                        // interpolate at new point
                        Vector3 traced_velocity = new Vector3(
                                get_interpolated_val_with_obs(x_prev, obs, backtraced_pos, 1f, dims),
                                get_interpolated_val_with_obs(y_prev, obs, backtraced_pos, 1f, dims),
                                get_interpolated_val_with_obs(z_prev, obs, backtraced_pos, 1f, dims)
                            );

                        x[IX(i, j, k)] = traced_velocity.X;
                        y[IX(i, j, k)] = traced_velocity.Y;
                        z[IX(i, j, k)] = traced_velocity.Z;

                    }
                }
            }
        }

        private void advect_RK2(float[] q, float[] q_prev,
                                 float[] x_prev, float[] y_prev, float[] z_prev,
                                 int[] obs
            )
        {
            for (int k = 0; k < NZ; k++)
            {
                for (int j = 0; j < NY; j++)
                {
                    for (int i = 0; i < NX; i++)
                    {
                        Vector3 pos = new Vector3(i, j, k);
                        Vector3 orig_vel = new Vector3(
                                x_prev[IX(i, j, k)],
                                y_prev[IX(i, j, k)],
                                z_prev[IX(i, j, k)]
                            );

                        //backtrace based upon current velocity at cell center.
                        float halfDT = .5f * dt_inv;
                        Vector3 halfway_pos = new Vector3(
                                pos.X + (halfDT * orig_vel.X),
                                pos.Y + (halfDT * orig_vel.Y),
                                pos.Z + (halfDT * orig_vel.Z)
                            );

                        // clamp
                        halfway_pos.X = Clamp(halfway_pos.X, 0.0f, NX - 1);
                        halfway_pos.Y = Clamp(halfway_pos.Y, 0.0f, NY - 1);
                        halfway_pos.Z = Clamp(halfway_pos.Z, 0.0f, NZ - 1);

                        // interpolate at new point
                        Vector3 halfway_vel = new Vector3(
                                get_interpolated_val_with_obs(x_prev, obs, halfway_pos, 1f, dims),
                                get_interpolated_val_with_obs(y_prev, obs, halfway_pos, 1f, dims),
                                get_interpolated_val_with_obs(z_prev, obs, halfway_pos, 1f, dims)
                            );

                        Vector3 backtraced_pos = new Vector3(
                                pos.X + dt_inv * halfway_vel.X,
                                pos.Y + dt_inv * halfway_vel.Y,
                                pos.Z + dt_inv * halfway_vel.Z
                            );

                        // clamp
                        backtraced_pos.X = Clamp(backtraced_pos.X, 0.0f, NX - 1);
                        backtraced_pos.Y = Clamp(backtraced_pos.Y, 0.0f, NY - 1);
                        backtraced_pos.Z = Clamp(backtraced_pos.Z, 0.0f, NZ - 1);

                        float traced_q = get_interpolated_val_with_obs(q_prev, obs, backtraced_pos, 1f, dims);
                        q[IX(i, j, k)] = traced_q;
                    }
                }
            }
        }

        private void buoyancy (float[] x, float[] temp)
        {
            float a = 0.0000625f;
            float b = 0.025f;

            for(int i = 0; i < array_size; i++)
            {
                x[i] -= a * temp[i] + -b * (temp[i] - (tAmb * dt));
            }
        }

        private void project (float[] x, float[] y, float[] z, float[] divergence,
                              float[] pressure, float[] pressure_prev, int[] obs)
        {
            calc_divergence_with_obs(divergence, x, y, z, obs);
            pressure_solve(pressure, pressure_prev, divergence, obs);
            pressure_apply_with_obs(x, y, z, pressure, obs);
            //check_divergence(x, y, z);
        }

        private void vorticity_confinement (float[] x, float[] y, float[] z,
                                            float[] x_prev, float[] y_prev, float[] z_prev)
        {
            Vector3 curl;
            Vector3 curl_xplus1, curl_xminus1;
            Vector3 curl_yplus1, curl_yminus1;
            Vector3 curl_zplus1, curl_zminus1;

            Vector3 n = new Vector3(), f = new Vector3();

            for(int k = 1; k < NZ - 1; k++)
            {
                for(int j = 1; j < NY - 1; j++)
                {
                    for(int i = 1; i < NX - 1; i++)
                    {
                        // calculate gradient of curl magnitude
                        curl_xplus1 = get_curl(x_prev, y_prev, z_prev, i + 1, j, k);
                        curl_xminus1 = get_curl(x_prev, y_prev, z_prev, i - 1, j, k);

                        curl_yplus1 = get_curl(x_prev, y_prev, z_prev, i, j + 1, k);
                        curl_yminus1 = get_curl(x_prev, y_prev, z_prev, i, j - 1, k);

                        curl_zplus1 = get_curl(x_prev, y_prev, z_prev, i, j, k + 1);
                        curl_zminus1 = get_curl(x_prev, y_prev, z_prev, i, j, k - 1);

                        n.X = 0.5f * (curl_xplus1.Length() - curl_xminus1.Length());
                        n.Y = 0.5f * (curl_yplus1.Length() - curl_yminus1.Length());
                        n.Z = 0.5f * (curl_zplus1.Length() - curl_zminus1.Length());
                        
                        // safe normalize
                        if(n.Length() > 0f)
                        {
                            float invSqrt = 1f / (float)Math.Sqrt(n.Length());
                            n.X *= invSqrt;
                            n.Y *= invSqrt;
                            n.Z *= invSqrt;
                        }
                        else
                        {
                            n.X = n.Y = n.Z = 0f;
                        }

                        curl = get_curl(x_prev, y_prev, z_prev, i, j, k);

                        float e = 0.275f;

                        f = Vector3.Cross(n, curl);

                        x[IX(i, j, k)] += f.X * e * dt;
                        y[IX(i, j, k)] += f.Y * e * dt;
                        z[IX(i, j, k)] += f.Z * e * dt;
                    }
                }
            }
        }

        private Vector3 get_curl (float[] x, float[] y, float[] z, int i, int j, int k)
        {
            float dzdj = 0.5f * (get_data(z, i, j + 1, k) - get_data(z, i, j - 1, k));
            float dzdi = 0.5f * (get_data(z, i + 1, j, k) - get_data(z, i - 1, j, k));

            float dxdk = 0.5f * (get_data(x, i, j, k + 1) - get_data(x, i, j, k - 1));
            float dxdj = 0.5f * (get_data(x, i, j + 1, k) - get_data(x, i, j - 1, k));

            float dydk = 0.5f * (get_data(y, i, j, k + 1) - get_data(y, i, j, k - 1));
            float dydi = 0.5f * (get_data(y, i + 1, j, k) - get_data(y, i - 1, j, k));

            return new Vector3(dzdj - dydk, dxdk - dzdi, dydi - dxdj);
        }

        private float get_interpolated_val_with_obs(float[] x, int[] obs, Vector3 pos, float h, IntPos n)
        {
            //The grid world pos is 0-1.
            int i0 = (int)(pos.X / h);
            int j0 = (int)(pos.Y / h);
            int k0 = (int)(pos.Z / h);
            int i1 = Clamp(i0 + 1, 0, n.X - 1); // returns 1??
            int j1 = Clamp(j0 + 1, 0, n.Y - 1);
            int k1 = Clamp(k0 + 1, 0, n.Z - 1);

            //Calculate t per component
            float it = (pos.X - i0 * h);
            float jt = (pos.Y - j0 * h);
            float kt = (pos.Z - k0 * h);

            // Assume a cube with 8 points
            //Front face
            //Top Front MIX
            float xFrontLeftTop = x[IX(i0, j1, k0)] * (1 - obs[IX(i0, j1, k0)]);
            float xFrontRightTop = x[IX(i1, j1, k0)] * (1 - obs[IX(i1, j1, k0)]);
            float xFrontTopInterp = MIX(xFrontLeftTop, xFrontRightTop, it);
            //Bottom Front MIX
            float xFrontLeftBottom = x[IX(i0, j0, k0)] * (1 - obs[IX(i0, j0, k0)]);
            float xFrontRightBottom = x[IX(i1, j0, k0)] * (1 - obs[IX(i1, j0, k0)]);
            float xFrontBottomInterp = MIX(xFrontLeftBottom, xFrontRightBottom, it);

            //Back face
            //Top Back MIX
            float xBackLeftTop = x[IX(i0, j1, k1)] * (1 - obs[IX(i0, j1, k1)]);
            float xBackRightTop = x[IX(i1, j1, k1)] * (1 - obs[IX(i1, j1, k1)]);
            float xBackTopInterp = MIX(xBackLeftTop, xBackRightTop, it);
            //Bottom Back MIX
            float xBackLeftBottom = x[IX(i0, j0, k1)] * (1 - obs[IX(i0, j0, k1)]);
            float xBackRightBottom = x[IX(i1, j0, k1)] * (1 - obs[IX(i1, j0, k1)]);
            float xBackBottomInterp = MIX(xBackLeftBottom, xBackRightBottom, it);


            //Now get middle of front -The bilinear interp of the front face
            float xBiLerpFront = MIX(xFrontBottomInterp, xFrontTopInterp, jt);

            //Now get middle of back -The bilinear interp of the back face
            float xBiLerpBack = MIX(xBackBottomInterp, xBackTopInterp, jt);

            //Now get the interpolated point between the points calculated in the front and back faces - The trilinear interp part
            float xTriLerp = MIX(xBiLerpFront, xBiLerpBack, kt);

            return xTriLerp;
        }
        
        private void calc_divergence_with_obs (float[] divergence, float[] x, float[] y, float[] z, int[] obs)
        {
            for(int k = 0; k < NZ; k++)
            {
                for(int j = 0; j < NY; j++)
                {
                    for(int i = 0; i < NX; i++)
                    {
                        float u1 = get_data(x, i + 1, j, k) * (1 - get_data(obs, i + 1, j, k));
                        float u0 = get_data(x, i - 1, j, k) * (1 - get_data(obs, i - 1, j, k));
                        float v1 = get_data(y, i, j + 1, k) * (1 - get_data(obs, i, j + 1, k));
                        float v0 = get_data(y, i, j - 1, k) * (1 - get_data(obs, i, j - 1, k));
                        float w1 = get_data(z, i, j, k + 1) * (1 - get_data(obs, i, j, k + 1));
                        float w0 = get_data(z, i, j, k - 1) * (1 - get_data(obs, i, j, k - 1));

                        float du = u1 - u0;
                        float dv = v1 - v0;
                        float dw = w1 - w0;
                        float div = .5f * (du + dv + dw);

                        divergence[IX(i, j, k)] = div;
                    }
                }
            }
        }
        
        private void pressure_solve (float[] pressure, float[] pressure_prev, float[] divergence, int[] obs)
        {
            for (int i = 0; i < array_size; i++)
                pressure[i] = 0f;

            for(int ii = 0; ii < 40; ii++)
            {
                for(int k = 0; k < NZ; k++)
                {
                    for(int j = 0; j < NY; j++)
                    {
                        for(int i = 0; i < NX; i++)
                        {
                            if (is_obs_cell(obs, i, j, k))
                            {
                                pressure[IX(i, j, k)] = 0.0f;
                            }
                            else
                            {
                                // jacobi solver
                                float p = -get_data(divergence, i, j, k);
                                float pC = get_data(pressure, i, j, k);

                                float a = get_data(pressure, i - 1, j, k);
                                float b = get_data(pressure, i + 1, j, k);
                                float c = get_data(pressure, i, j + 1, k);
                                float d = get_data(pressure, i, j - 1, k);
                                float e = get_data(pressure, i, j, k - 1);
                                float f = get_data(pressure, i, j, k + 1);

                                if (is_obs_cell(obs, i - 1, j, k)) a = pC;
                                if (is_obs_cell(obs, i + 1, j, k)) b = pC;
                                if (is_obs_cell(obs, i, j - 1, k)) c = pC;
                                if (is_obs_cell(obs, i, j + 1, k)) d = pC;
                                if (is_obs_cell(obs, i, j, k - 1)) e = pC;
                                if (is_obs_cell(obs, i, j, k + 1)) f = pC;

                                float v = (p + (a + b + c + d + e + f)) / 6.0f;
                                pressure[IX(i, j, k)] = v;
                            }
                        }
                    }
                }
            }
        }
        
        private void pressure_apply_with_obs (float[] x, float[] y, float[] z,
                                              float[] pressure, int[] obs)
        {
            for(int k = 0; k < NZ; k++)
            {
                for(int j = 0; j < NY; j++)
                {
                    for(int i = 0; i < NX; i++)
                    {
                        if(is_obs_cell(obs,i,j,k))
                        {
                            x[IX(i, j, k)] = 0f;
                            y[IX(i, j, k)] = 0f;
                            z[IX(i, j, k)] = 0f;
                            continue;
                        }

                        // calculate pressure gradient
                        float pC = get_data(pressure, i, j, k);
                        float pL = get_data(pressure, i - 1, j, k);
                        float pR = get_data(pressure, i + 1, j, k);
                        float pB = get_data(pressure, i, j - 1, k);
                        float pT = get_data(pressure, i, j + 1, k);
                        float pD = get_data(pressure, i, j, k - 1);
                        float pU = get_data(pressure, i, j, k + 1);


                        Vector3 obstV = Vector3.Zero;
                        Vector3 vMask = Vector3.One;

                        //If an adjacent cell is a solid, ignore its pressure and use it's velocity
                        if (is_obs_cell(obs, i - 1, j, k))
                        {
                            pL = pC; obstV.X = 0.0f; vMask.X = 0;
                        }
                        if (is_obs_cell(obs, i + 1, j, k))
                        {
                            pR = pC; obstV.X = 0.0f; vMask.X = 0;
                        }
                        if (is_obs_cell(obs, i, j - 1, k) )
                        {
                            pB = pC; obstV.Y = 0.0f; vMask.Y = 0;
                        }
                        if (is_obs_cell(obs, i, j + 1, k))
                        {
                            pT = pC; obstV.Y = 0.0f; vMask.Y = 0;
                        }
                        if (is_obs_cell(obs, i, j, k - 1))
                        {
                            pD = pC; obstV.Z = 0.0f; vMask.Z = 0;
                        }
                        if (is_obs_cell(obs, i, j, k + 1))
                        {
                            pU = pC; obstV.Z = 0.0f; vMask.Z = 0;
                        }

                        //compute the gradient of pressure at the current cell by taking the central diff
                        //of neighboring pressure values
                        Vector3 gradP = new Vector3( pR - pL, pT - pB, pU - pD );
                        gradP.X *= 0.5f;
                        gradP.Y *= 0.5f;
                        gradP.Z *= 0.5f;

                        //Project the velocity onto its divergence-free componenet by
                        //subtracting the gradient of pressure
                        Vector3 vOld = new Vector3( x[IX(i, j, k)], y[IX(i, j, k)], z[IX(i, j, k)] );
                        Vector3 vNew = new Vector3( vOld.X - gradP.X, vOld.Y - gradP.Y, vOld.Z - gradP.Z );

                        //Explicityly enforce the free-slip boundary condition by replacing the appropriate
                        //components of the new velocity with obstacle velocities
                        x[IX(i, j, k)] = vMask.X * vNew.X + obstV.X;
                        y[IX(i, j, k)] = vMask.Y * vNew.Y + obstV.Y;
                        z[IX(i, j, k)] = vMask.Z * vNew.Z + obstV.Z;
                    }
                }
            }
        }
        
        private bool check_divergence (float[] x, float[] y, float[] z)
        {
            for (int k = 0; k < NZ; k++)
            {
                for (int j = 0; j < NY; j++)
                {
                    for (int i = 0; i < NX; i++)
                    {
                        float du = get_data(x, i + 1, j, k) - get_data(x, i - 1, j, k);
                        float dv = get_data(y, i, j + 1, k) - get_data(y, i, j - 1, k);
                        float dw = get_data(z, i, j, k + 1) - get_data(z, i, j, k - 1);
                        float div = 0.5f * (du + dv + dw);

                        if (div > 0.001)
                        {
                            //printf("Velocity field is not divergence free\t");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        #endregion
    }
}
