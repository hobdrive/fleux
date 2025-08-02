using System;
using System.Threading;
using System.Timers;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Fleux.Core;

namespace Fleux.Controls
{
    public class DoubleBufferedControl : IDisposable
    {
        protected HostView AndroidView;
        protected Bitmap offBmp;
        protected Graphics offGr;
        protected bool offBmpDraw = false;
        Android.Graphics.Rect srect, drect;
        
        public int OffBmpWidth, OffBmpHeight;

        /// <summary>
        /// If true, means invalidate request was send and still pending.
        /// </summary>
        protected bool offUpdated;
        /// <summary>
        /// If true, means next invalidate flush should also invoke full redraw
        /// </summary>
        protected bool offNeedExtraDraw = false;

        protected bool resizing;
        bool IsDisposed = false;

        public static bool PerfData = false;
        
        /// Main Canvas scaledown resolution
        public float DownScale = 1f;

        public class HostView : View
        {
            DoubleBufferedControl Control;
            public event Action Measured;

            Android.Graphics.Matrix hMatrix;
            Android.Graphics.Matrix vMatrix;
            Android.Graphics.Matrix origMatrix;

            Android.Graphics.Paint paint = new Android.Graphics.Paint();
            
            static Android.Graphics.Paint spaint = new Android.Graphics.Paint();

            internal int totime;
            internal int updcnt = 0;
            internal int updcntflush = 0;
            internal int updcntinval = 0;

            public HostView(Android.Content.Context ctx, DoubleBufferedControl c) : base(ctx)
            {
                this.Control = c;
                c.AndroidView = this;
                
                paint.FilterBitmap = true;

                Activate();
            }

            public HostView(System.IntPtr obj, Android.Runtime.JniHandleOwnership jho) : base(obj, jho)
            {
                // fictive constructor for Dummy objects
            }

            protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
            {
                base.SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
                if (Measured != null)
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

            public override void Draw (Android.Graphics.Canvas canvas)
            {
                if (Control == null) return;

                Control.CreateGraphicBuffers();

                var ctime = System.Environment.TickCount;
                Fleux.UIElements.Canvas.drawtime = 0;

                if (Control.offUpdated)
                {
		            lock(Control.offBmp)
		            {

		                Control.Draw(new PaintEventArgs(Control.offGr, new Rectangle(0,0, Control.offBmp.Width, Control.offBmp.Height)));

		                updcnt++;
		            }
		        }
                Control.offUpdated = false;
		        
                lock(Control.offBmp)
                {
                    if (Fleux.Core.FleuxApplication.HorizontalMirror)
                    {
                        canvas.Save();
                        canvas.Scale (-1, 1);
                        canvas.Translate (-(float)Control.drect.Width(), 0);
                    }else if (Fleux.Core.FleuxApplication.VerticalMirror)
                    {
                        canvas.Save();
                        canvas.Scale (1, -1);
                        canvas.Translate (0, -(float)Control.drect.Height());
                    }

                    Control.offGr.Flush();

                    canvas.DrawBitmap(Control.offBmp.ABitmap, Control.srect, Control.drect, paint);

                    updcntflush++;
                }
                if (PerfData)
                {
                    ctime = System.Environment.TickCount - ctime;
                    {
                        totime += ctime;
                    }
                    var cavg = totime / (updcnt+1);

                    var cpaint = new Android.Graphics.Paint();
                    cpaint.Color = new Android.Graphics.Color(0xA0, 0xFF, 0xFF, 0xFF);
                    cpaint.SetStyle(Android.Graphics.Paint.Style.Fill);
                    canvas.DrawRect(0,0, 250, 20, cpaint);
                    canvas.DrawText(""+updcnt+":"+updcntflush+":"+updcntinval+" ctime: "+ctime+"cavg:"+cavg+" canv: "+Fleux.UIElements.Canvas.drawtime, 0,20, spaint);
                    cpaint.Dispose();
                    if (updcnt > 100)
                    {
                        totime = 0;
                        updcnt = 0;
                    }
                }
            }

            public new void Dispose()
            {
                Android.Util.Log.Info("HOBD", "HostView Dispose");
                paint.Dispose();
                paint = null;
                Control = null;

                base.Dispose();
            }


    
            public void Activate()
            {
                base.Touch += (v, te) => {
                    MotionEvent e = te.Event;
                    float x = e.GetX() / Control.DownScale;
                    float y = e.GetY() / Control.DownScale;
                    if (((int)e.Action&0xFF) == (int)MotionEventActions.Down)
                        Control.OnMouseDown(new MouseEventArgs((int)x, (int)y));
                    if (((int)e.Action&0xFF) == (int)MotionEventActions.Move)
                        Control.OnMouseMove(new MouseEventArgs((int)x, (int)y));
                    if (((int)e.Action&0xFF) == (int)MotionEventActions.Up)
                        Control.OnMouseUp(new MouseEventArgs((int)x, (int)y));

                };
            }

            //-----------------------------------------------------------------
            private void SetSoftwareLayer ( )
            {
                // TODO: Figure out what causes flickering
                this.SetLayerType(LayerType.Software, null);
            }
        }

        protected virtual void Draw(System.Windows.Forms.PaintEventArgs e){
        }

        protected virtual void OnMouseDown(MouseEventArgs e){}

        protected virtual void OnMouseMove(MouseEventArgs e){}

        protected virtual void OnMouseUp(MouseEventArgs e){}

        long lastRedraw;

        protected virtual void ForcedInvalidate(){
            if (IsDisposed)
                return;

            if (!offUpdated || (DateTime.Now.Ticks - lastRedraw > 1*1000*1000*100))
            {
                lastRedraw = DateTime.Now.Ticks;
                offUpdated = true;
                // AndroidView in java may occasionally be already dead!
                try{
                    AndroidView.PostInvalidate();
                    AndroidView.updcntinval++;
                }catch(Exception e){
                    System.Console.WriteLine(e.StackTrace);
                }
            }
        }

        public void Invoke(Action a)
        {
            a();
        }

        public virtual void AttachEffect(string effect, Dictionary<string, object> p)
        {
        }

        public virtual void Draw(Action<Graphics> drawAction)
        {
            //if (!IsDisposed && this.offGr != null)
            {
                drawAction(this.offGr);
            }
        }

        public virtual Color BackColor{ get; set;}

        protected virtual void CreateGraphicBuffers()
        {
            var Control  = this;
            if (Control.offBmp == null && this.AndroidView != null){
                OffBmpWidth = (int)(AndroidView.MeasuredWidth/DownScale);
                OffBmpHeight = (int)(AndroidView.MeasuredHeight/DownScale);
                
                if (OffBmpWidth > 0 && OffBmpHeight > 0)
                {
                    Control.offBmp = new Bitmap(OffBmpWidth, OffBmpHeight);
                    Control.offGr = new Graphics(Control.offBmp);
                }
                srect = new Android.Graphics.Rect(0,0, OffBmpWidth, OffBmpHeight);
                drect = new Android.Graphics.Rect(0,0, AndroidView.MeasuredWidth, AndroidView.MeasuredHeight);
            }
        }


        public virtual void Dispose()
        {
            Android.Util.Log.Info("HOBD", "DoubleBufferedControl Dispose");
            IsDisposed = true;
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
