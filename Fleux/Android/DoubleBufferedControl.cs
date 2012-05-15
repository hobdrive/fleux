namespace Fleux.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Android.App;
    using Android.Content;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using Android.OS;
    using Core;

    public class DoubleBufferedControl : IDisposable
    {
        protected HostView AndroidView;
        protected Bitmap offBmp;
        protected Graphics offGr;
        protected bool offBmpDraw = false;
        protected bool offUpdated;
        protected bool resizing;

        public class HostView : View
        {
            DoubleBufferedControl Control;
            public event Action Measured;

            Android.Graphics.Matrix hMatrix;
            Android.Graphics.Matrix vMatrix;
            Android.Graphics.Matrix origMatrix;
            int x, y;

            public HostView(Android.Content.Context ctx, DoubleBufferedControl c) : base(ctx)
            {
                this.Control = c;
                c.AndroidView = this;

                Activate();
            }

            protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
            {
                base.SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
                Measured();
            }

            protected override void OnLayout (bool changed, int left, int top, int right, int bottom)
            {
                base.OnLayout (changed, left, top, right, bottom);
            }

            protected override void OnDraw (Android.Graphics.Canvas canvas)
            {
                this.Draw(canvas);
            }

            Android.Graphics.Paint paint = new Android.Graphics.Paint();

            static Android.Graphics.Paint spaint = new Android.Graphics.Paint();
            int updcnt = 0;
            int updcntflush = 0;


            public override void Draw (Android.Graphics.Canvas canvas)
            {
                Control.CreateGraphicBuffers(MeasuredWidth, MeasuredHeight);

                if (origMatrix == null)
                {
                    origMatrix = new Android.Graphics.Matrix();
                    origMatrix.Set (canvas.Matrix);
                }
                if (hMatrix == null)
                {
                    hMatrix = new Android.Graphics.Matrix();
                    hMatrix.Set(origMatrix);
                    hMatrix.PostTranslate(-Control.offBmp.Width, 0);
                    hMatrix.PostScale(-1, 1);
                }
                if (vMatrix == null)
                {
                    vMatrix = new Android.Graphics.Matrix();
                    vMatrix.Set(origMatrix);
                    vMatrix.PostTranslate(0, -Control.offBmp.Height);
                    vMatrix.PostScale(1, -1);
                }

                var ctime = System.Environment.TickCount;
                Fleux.UIElements.Canvas.drawtime = 0;

                if (Control.offUpdated)
                lock(Control.offBmp){
                    Control.offUpdated = false;
                    Control.offBmpDraw = true;
                    Control.Draw(new PaintEventArgs(Control.offGr, new Rectangle(0,0, Control.offBmp.Width, Control.offBmp.Height)));
                    Control.offBmpDraw = false;
                    updcnt++;
                }
                lock(Control.offBmp)
                {
                    if (Fleux.Core.FleuxApplication.HorizontalMirror)
                    {
                        canvas.Matrix = hMatrix;
                    }else if (Fleux.Core.FleuxApplication.VerticalMirror)
                    {
                        canvas.Matrix = vMatrix;
                    }else
                        canvas.Matrix = origMatrix;
                    canvas.DrawBitmap(Control.offBmp.ABitmap, x,y, paint);
                    updcntflush++;
                }
                ctime = System.Environment.TickCount - ctime;
//#if DEBUG
                canvas.DrawText(""+updcnt+":"+updcntflush+" t: "+ctime+" canvas: "+Fleux.UIElements.Canvas.drawtime, x,y+20, spaint);
//#endif
            }

            //TODO???
            public new void Dispose()
            {
                paint.Dispose();
                hMatrix.Dispose();
                vMatrix.Dispose();
                paint = null;
                Control = null;
            }


    
            public void Activate()
            {
                base.Touch += (v, te) => {
                    MotionEvent e = te.Event;
                    if (((int)e.Action&0xFF) == (int)MotionEventActions.Down)
                        Control.OnMouseDown(new MouseEventArgs((int)e.GetX(), (int)e.GetY()));
                    if (((int)e.Action&0xFF) == (int)MotionEventActions.Move)
                        Control.OnMouseMove(new MouseEventArgs((int)e.GetX(), (int)e.GetY()));
                    if (((int)e.Action&0xFF) == (int)MotionEventActions.Up)
                        Control.OnMouseUp(new MouseEventArgs((int)e.GetX(), (int)e.GetY()));

                };
            }
    
        }

        protected virtual void Draw(System.Windows.Forms.PaintEventArgs e){
        }

        protected virtual void OnMouseDown(MouseEventArgs e){}

        protected virtual void OnMouseMove(MouseEventArgs e){}

        protected virtual void OnMouseUp(MouseEventArgs e){}

        protected virtual void ForcedInvalidate(){
            CreateGraphicBuffers(this.AndroidView.MeasuredWidth, this.AndroidView.MeasuredHeight);
            /*
            lock(offBmp){
                offBmpDraw = true;
                Draw(new PaintEventArgs(offGr, new Rectangle(0,0, offBmp.Width, offBmp.Height)));
                offBmpDraw = false;
            }
            */
            offUpdated = true;
            AndroidView.PostInvalidate();
        }

        public void Invoke(Action a)
        {
            a();
        }

        public virtual void Draw(Action<Graphics> drawAction)
        {
            //if (!IsDisposed && this.offGr != null)
            {
                drawAction(this.offGr);
            }
        }

        public virtual Color BackColor{ get; set;}

        protected virtual void CreateGraphicBuffers(int Width, int Height)
        {
            var Control  = this;
            if (Control.offBmp == null){
                Control.offBmp = new Bitmap(Width, Height);
                Control.offGr = new Graphics(Control.offBmp);
            }
        }


        public virtual void Dispose()
        {
            this.ReleaseGraphicBuffers();
        }

        public virtual void ReleaseGraphicBuffers()
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


    }
}