using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluidSolver
{
    public enum RenderMode
    {
        Density,
        Heat,
        Pressure
    }

    public partial class FluidControl : UserControl
    {
        public void GridSize (int width, int height)
        {
            this.width = width;
            this.height = height;
            Init();
        }

        private float rminVal = float.MaxValue;  // render min/max values
        private float rmaxVal = float.MinValue;
        private float rminValOld = 0f;
        private float rmaxValOld = 1f;
        
        private int width = 2;
        private int height = 2;
        private Bitmap bitmap;
        private BitmapData bmData;
        private int byteSize;
        private byte[] tmpData;

        public bool MouseIsDown { get {return mouseDown;} }
        private bool mouseDown = false;
        private Point mousePrevPos;

        private static float map (float value, float low1, float high1, float low2, float high2)
        {
            return low2 + (high2 - low2) * (value - low1) / (high1 - low1);
        }

        public FluidControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Init();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (bitmap != null)
                e.Graphics.DrawImage(bitmap, ClientRectangle);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!ClientRectangle.Contains(PointToClient(MousePosition))) return;
            mouseDown = true;
            mousePrevPos = PointToClient(MousePosition);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown = false;
        }

        private void Init ()
        {
            if (width < 1 || height < 1) return;
            bitmap = new Bitmap(width, height);
            byteSize = (4 * width) * height;   // 4 bytes per pixel
            tmpData = new byte[byteSize];
        }

        public Point GridMousePosition ()
        {
            Point pos = PointToClient(MousePosition);
            if (pos.X > ClientSize.Width || pos.Y > ClientSize.Height) return new Point(-1, -1);
            return new Point(
                    (int)((pos.X / (float)ClientSize.Width) * (width - 1)) + 1,
                    (int)((pos.Y / (float)ClientSize.Height) * (height - 1)) + 1
                );
        }

        public Point MouseVelocity ()
        {
            Point cur = PointToClient(MousePosition);
            Point vel = new Point(
                    cur.X - mousePrevPos.X,
                    cur.Y - mousePrevPos.Y
                );
            mousePrevPos = cur;
            return vel;
        }

        public void Render (Solver solver, RenderMode renderMode, bool drawObstacles = true)
        {
            float[] src;
            switch(renderMode)
            {
                default:
                case RenderMode.Density:
                    src = solver.DensityField;
                    break;
                case RenderMode.Heat:
                    src = solver.HeatField;
                    break;
                case RenderMode.Pressure:
                    src = solver.PressureField;
                    break;
            }

            bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int pos, x, y;
            float val;
            float r, g, b;

            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    pos = (x << 2) + y * (width << 2); // position in byte array (x*4)+y*(w*4)

                    // obstacles takes precedence
                    if (drawObstacles)   
                    {
                        if(solver.Obstacles[solver.IX(x,y,0)] != 0)
                        {
                            tmpData[pos] = tmpData[pos + 1] = 0;
                            tmpData[pos + 2] = tmpData[pos + 3] = 255;
                            continue;
                        }
                    }

                    val = src[solver.IX(x, y, 0)];
                    rminVal = Math.Min(val, rminVal);
                    rmaxVal = Math.Max(val, rmaxVal);

                    // get RGB values depending on render mode
                    switch (renderMode)
                    {
                        default:
                        case RenderMode.Density:
                            render_density(val, out r, out g, out b);
                            break;
                        case RenderMode.Heat:
                            render_heat(val, out r, out g, out b);
                            break;
                        case RenderMode.Pressure:
                            render_pressure(val, out r, out g, out b);
                            break;
                    }

                    // update bitmap
                    tmpData[pos]     = (byte)(b * 255);
                    tmpData[pos + 1] = (byte)(g * 255);
                    tmpData[pos + 2] = (byte)(r * 255);
                    tmpData[pos + 3] = 255;

                    /*val = (byte)(Math.Max( Math.Min( src[solver.IX(x, y, 0)], 1f), 0f) * 255);
                    tmpData[pos] = tmpData[pos + 1] = tmpData[pos + 2] = val;   // BGR
                    tmpData[pos + 3] = 255; // alpha*/
                }
            }

            rminValOld = rminVal;
            rmaxValOld = rmaxVal;
            rminVal = float.MaxValue;
            rmaxVal = float.MinValue;

            System.Runtime.InteropServices.Marshal.Copy(tmpData, 0, bmData.Scan0, tmpData.Length);

            bitmap.UnlockBits(bmData);

            Invalidate();
        }

        private void render_density (float val, out float r, out float g, out float b)
        {
            val = Math.Min(Math.Max(val, 0f), 1f);
            r = g = b = val;
        }

        private void render_pressure (float val, out float r, out float g, out float b)
        {
            val = map(val, rminValOld, rmaxValOld, 0f, 1f);
            r = g = 0f;
            b = val;
        }

        private void render_heat(float val, out float r, out float g, out float b)
        {
            float ratio = 2f * (val - rminValOld) / (rmaxValOld - rminValOld);
            b = Math.Max(0f, 1f - ratio);
            r = Math.Max(0f, ratio - 1f);
            g = 1f - b - r;
        }
    }
}
