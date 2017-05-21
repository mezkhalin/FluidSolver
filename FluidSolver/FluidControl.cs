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
    public partial class FluidControl : UserControl
    {
        public void GridSize (int width, int height)
        {
            this.width = width;
            this.height = height;
            Init();
        }
        
        private int width = 2;
        private int height = 2;
        private Bitmap bitmap;
        private BitmapData bmData;
        private int byteSize;
        private byte[] tmpData;

        public bool MouseIsDown { get {return mouseDown;} }
        private bool mouseDown = false;
        private Point mousePrevPos;

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

        public void Render (float[] src, Solver solver)
        {
            bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int pos, x, y;
            byte val;

            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    pos = (x << 2) + y * (width << 2); // position in byte array (x*4)+y*(w*4)
                    val = (byte)(Math.Max( Math.Min( src[solver.IX(x, y, 0)], 1f), 0f) * 255);
                    tmpData[pos] = tmpData[pos + 1] = tmpData[pos + 2] = val;   // BGR
                    tmpData[pos + 3] = 255; // alpha
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(tmpData, 0, bmData.Scan0, tmpData.Length);

            bitmap.UnlockBits(bmData);

            Invalidate();
        }
    }
}
