using System.Threading;

namespace Fleux.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Core;

    public class DoubleBufferedControl : Control
    {
        protected Graphics controlGr;
        protected Bitmap offBmp;
        protected Graphics offGr;
        protected bool offUpdated;
        protected bool resizing;

        internal int totime;
        internal int updcnt;
        internal int updcntinval;
        internal int updcntflush;
        
        public static bool PerfData = false;
        
        public virtual void Draw(Action<Graphics> drawAction)
        {
            if (!IsDisposed && this.offGr != null)
            {
                drawAction(this.offGr);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var ctime = System.Environment.TickCount;
            
            lock (this)
            {
                this.CreateGraphicBuffers();
                if (this.offBmp != null)
                {
                    
                    if (!this.offUpdated)
                    {
                        this.Draw(new PaintEventArgs(this.offGr, ClientRectangle));
                        this.offUpdated = true;
                    }

                    this.controlGr.DrawImage(this.offBmp, 0, 0);
 
                    ctime = System.Environment.TickCount - ctime;
                    this.totime += ctime;
                    this.updcnt++;
                }
                else
                {
                    this.DrawBackground(e);
                }
                this.updcntflush++;
            }
        }

        protected virtual void Draw(PaintEventArgs e)
        {
        }

        protected virtual void DrawBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected virtual void CreateGraphicBuffers()
        {
            if (this.offBmp != null)
            {
                this.ReleaseGraphicBuffers();
            }

            this.controlGr = CreateGraphics();

            if (Height > 0 && Width > 0)
            {
                if (this.offBmp == null)
                {
                    if (this.controlGr == null)
                    {
                        this.controlGr = CreateGraphics();
                    }

                    this.offBmp = new Bitmap(Width, Height);
                    this.offGr = Graphics.FromImage(this.offBmp);
                    FleuxApplication.ApplyGraphicsSettings(this.offGr);
                    this.offUpdated = false;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.ReleaseGraphicBuffers();
            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected virtual void ReleaseGraphicBuffers()
        {
            lock (this)
            {
                if (this.offBmp != null)
                {
                    // Dispose resources
                    this.offGr.Dispose();
                    this.offBmp.Dispose();
                    this.offBmp = null;
                    this.offGr = null;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ReleaseGraphicBuffers();
        }

        Int32[] rgbValues = new Int32[1];
        // cached width
        protected int offBmpWidth = 0;
        protected int offBmpHeight = 0;

        protected virtual void ForcedInvalidate()
        {
            if (!IsDisposed && this.offBmp != null)
            {
                var ctime = System.Environment.TickCount;

                this.offUpdated = false;

                this.Draw(new PaintEventArgs(this.offGr, new Rectangle(0, 0, this.offBmp.Width, this.offBmp.Height)));

                // FIXME: On Linux high loaded Updates() makes other threads to stuck, causing huge number of useless redraw cycles
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    Thread.Sleep(5);

                if (FleuxApplication.HorizontalMirror || FleuxApplication.VerticalMirror)
                {
                    Rectangle rect = new Rectangle(0, 0, offBmp.Width, offBmp.Height);
                    System.Drawing.Imaging.BitmapData bmpData = offBmp.LockBits(rect,
                        System.Drawing.Imaging.ImageLockMode.ReadWrite,
                        System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    offBmpWidth  = offBmp.Width;
                    offBmpHeight = offBmp.Height;
                    IntPtr ptr = bmpData.Scan0;
                    int bytes  = bmpData.Stride * offBmp.Height / 4;
                    if (rgbValues.Length != bytes){
                        rgbValues = new Int32[bytes];
                    }

                    System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                    if (FleuxApplication.HorizontalMirror)
                    {
                        for(int x = 0; x < this.offBmp.Width/2; x++)
                        for(int y = 0; y < this.offBmp.Height; y++)
                        {
                            Int32 r0 = rgbValues[ x + y*bmpData.Stride/4 ];
                            Int32 r1 = rgbValues[ (offBmp.Width-x-1) + y*bmpData.Stride/4 ];

                            rgbValues[ x + y*bmpData.Stride/4 ] = r1;
                            rgbValues[ (offBmp.Width-x-1) + y*bmpData.Stride/4 ] = r0;
                        }
                    }
                    if (FleuxApplication.VerticalMirror)
                    {
                        for(int x = 0; x < this.offBmp.Width; x++)
                        for(int y = 0; y < this.offBmp.Height/2; y++)
                        {
                            Int32 r0 = rgbValues[ x + y*bmpData.Stride/4 ];
                            Int32 r1 = rgbValues[ x + (offBmp.Height-y-1)*bmpData.Stride/4 ];

                            rgbValues[ x + y*bmpData.Stride/4 ] = r1;
                            rgbValues[ x + (offBmp.Height-y-1)*bmpData.Stride/4 ] = r0;
                        }
                    }

                    System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
                    offBmp.UnlockBits(bmpData);
                }
                /*
                */
                try{
                    this.controlGr.DrawImage(this.offBmp, 0, 0);
                }catch(Exception){}
                
                ctime = System.Environment.TickCount - ctime;
                this.totime += ctime;
                this.updcnt++;
                this.updcntinval++;

                if (PerfData)
                {
                    var cavg = totime / (updcnt+1);

                    this.controlGr.FillRectangle(new SolidBrush(Color.White), 0,0, 400, 20);
                    this.controlGr.DrawString("upd:"+updcnt+" flush:"+updcntflush+" int:"+updcntinval+" ctime:"+ctime+" cavg:"+cavg+" canvtime:"+Fleux.UIElements.Canvas.drawtime,
                                              new Font(FontFamily.GenericMonospace, 15, FontStyle.Regular), new SolidBrush(Color.Black), 0,0);
                    if (updcnt > 500)
                    {
                        totime = 0;
                        updcnt = 0;
                    }
                }
                                  
            }
        }
    }
}