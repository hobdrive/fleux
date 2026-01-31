using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;

using SkiaSharp;
using SkiaSharp.Views.Android;

using Fleux.Core;
using Fleux.Core.Scaling;

namespace Fleux.Controls;

public class DoubleBufferedControlAGLView : SKGLSurfaceView
{
    protected Bitmap? offBmp;
    protected Graphics? offGr;

    protected bool offBmpDraw = false;

    public int OffBmpWidth { get; protected set; }
    public int OffBmpHeight { get; protected set; }

    public virtual System.Drawing.Color BackColor { get; set; }

    public event EventHandler? CanvasSizeChanged;

    /// <summary>
    /// If true, means invalidate request was send and still pending.
    /// </summary>
    protected bool offUpdated;
    /// <summary>
    /// </summary>
    protected bool offUpdateInProgress = false;

    public event Action Measured;

    protected bool resizing;
    bool IsDisposed = false;

    // Dedicated lock object for buffer synchronization (never lock on offBmp itself)
    private readonly object bufferLock = new object();

    public static bool PerfData = false;

    /// Main Canvas scaledown resolution
    public float DownScale = 1f;

    public DoubleBufferedControlAGLView(Context context) : base(context)
    {
        Initialize();
    }

    public DoubleBufferedControlAGLView(Context context, IAttributeSet attrs) : base(context, attrs)
    {
        Initialize();
    }

    private void Initialize()
    {
        this.PaintSurface += OnCanvasViewPaintSurface;
        // Native Android view handles touch by default via OnTouchEvent
    }

    internal int totime;
    internal int updcnt = 0;
    internal int updcntflush = 0;
    internal int updcntinval = 0;

    SKPaint CanvasPaint = new SKPaint
    {
        IsAntialias = false,
        FilterQuality = SKFilterQuality.High,
        Style = SKPaintStyle.Fill
    };

    void OnCanvasViewPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args)
    {
        if (IsDisposed)
            return;

        // Validate surface dimensions to avoid GL_INVALID_FRAMEBUFFER_OPERATION
        SKImageInfo info = args.Info;
        if (info.Width <= 0 || info.Height <= 0)
            return;

        SKSurface surface = args.Surface;
        if (surface == null)
            return;

        SKCanvas canvas = surface.Canvas;
        if (canvas == null)
            return;

        var t0 = System.Environment.TickCount;

        var t1 = System.Environment.TickCount;
        try
        {
            canvas.Clear(SKColors.Blue);
        }
        catch (Exception ex)
        {
            // Framebuffer not ready during orientation change
            FleuxApplication.Log($"Canvas clear failed: {ex.Message}");
            return;
        }
        var tClear = System.Environment.TickCount - t1;

        var t2 = System.Environment.TickCount;
        CreateGraphicBuffers(info, args.Surface.Context);
        var tCreateBuffer = System.Environment.TickCount - t2;

        // Check if buffers are ready after CreateGraphicBuffers
        if (offBmp == null || offGr == null)
            return;

        var ctime = System.Environment.TickCount;
        Fleux.UIElements.Canvas.drawtime = 0;

        var tDraw = 0;
        if (offUpdated)
        {
            lock (bufferLock)
            {
                if (offBmp != null && offGr != null)
                {
                    offUpdateInProgress = true;
                    var t3 = System.Environment.TickCount;
                    
                    Draw(new PaintEventArgs(offGr, new Rectangle(0, 0, offBmp.Width, offBmp.Height)));
                    offBmp.InvalidateSnapshot();

                    tDraw = System.Environment.TickCount - t3;
                    offUpdateInProgress = false;
                    updcnt++;
                }
            }
        }
        offUpdated = false;

        if (currentEffect?.Process() ?? false)
        {
            //
        }
        else
        {
            currentEffect = null;
        }

        var tBitmapDraw = 0;
        lock (bufferLock)
        {
            if (offBmp != null)
            {
                /*
                TODO: handle mirroring here
                if (Fleux.Core.FleuxApplication.HorizontalMirror)
                {
                    canvas.Save();
                    canvas.Scale(-1, 1);
                    canvas.Translate(-(float)Control.drect.Width(), 0);
                }
                else if (Fleux.Core.FleuxApplication.VerticalMirror)
                {
                    canvas.Save();
                    canvas.Scale(1, -1);
                    canvas.Translate(0, -(float)Control.drect.Height());
                }
                offGr.Flush();
                */

                var cPaint = currentEffect?.GetState() as SKPaint;
                if (cPaint == null)
                {
                    cPaint = CanvasPaint;
                }
                var t4 = System.Environment.TickCount;
                
                try
                {
                    var skImage = offBmp.GetSkImage();
                    if (skImage != null)
                    {
                        canvas.DrawImage(skImage,
                            new SKRect(0, 0, info.Width, info.Height), cPaint);
                    }
                }
                catch (Exception ex)
                {
                    // Framebuffer operation failed - likely during orientation change
                    FleuxApplication.Log($"DrawImage failed: {ex.Message}");
                }
                tBitmapDraw = System.Environment.TickCount - t4;

                updcntflush++;
            }
        }

        if (PerfData)
        {
            ctime = System.Environment.TickCount - ctime;
            var tTotal = System.Environment.TickCount - t0;
            {
                totime += ctime;
            }
            var cavg = totime / (updcnt + 1);
            if (updcnt > 1000)
            {
                updcnt = 0;
                totime = 0;
            }

            using (SKPaint paint = new SKPaint())
                {
                    paint.Color = SKColors.Black;
                    paint.TextAlign = SKTextAlign.Left;
                    paint.TextSize = (int)Fleux.Core.FleuxApplication.ScaleFromLogic(8);

                    var y = paint.FontMetrics.Descent - paint.FontMetrics.Ascent; // normalize

                    int offset = 10;
                    int lineHeight = (int)(y * 1.2f);

                    canvas.DrawRect(new SKRect(offset, 0, 200.ToPixels(), lineHeight * 4), new SKPaint { Color = SKColors.White.WithAlpha(200), Style = SKPaintStyle.Fill, });
                    canvas.DrawText($"Frame #{updcnt} Total:{tTotal.ToString().PadLeft(3)}ms Clear:{tClear}ms Buf:{tCreateBuffer}ms", offset, y, paint);
                    canvas.DrawText($"Avg1K:{cavg.ToString().PadLeft(3)}ms Draw:{tDraw.ToString().PadLeft(3)}ms BmpDraw:{tBitmapDraw}ms Canvas:{Fleux.UIElements.Canvas.drawtime.ToString().PadLeft(3)}ms", offset, y + lineHeight, paint);
                    canvas.DrawText($"Size:{info.Width}x{info.Height} OffBmp:{OffBmpWidth}x{OffBmpHeight} Scale:{DownScale}", offset, y + lineHeight * 2, paint);
                }
        }
    }

    protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
    {
        base.SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
        if (Measured != null)
            Measured();
    }

    public override bool OnTouchEvent(MotionEvent e)
    {
        if (e == null)
            return base.OnTouchEvent(e);

        int sx = (int)(e.GetX() / DownScale);
        int sy = (int)(e.GetY() / DownScale);

        switch (e.Action)
        {
            case MotionEventActions.Down:
                var mouseDownArgs = new MouseEventArgs(sx, sy);
                OnMouseDown(mouseDownArgs);
                OnMouseMove(mouseDownArgs);
                return true;

            case MotionEventActions.Move:
                var mouseMoveArgs = new MouseEventArgs(sx, sy);
                OnMouseMove(mouseMoveArgs);
                return true;

            case MotionEventActions.Up:
            case MotionEventActions.Cancel:
                var mouseUpArgs = new MouseEventArgs(sx, sy);
                OnMouseUp(mouseUpArgs);
                return true;

            default:
                return base.OnTouchEvent(e);
        }
    }

    protected virtual void Draw(PaintEventArgs e)
    {
    }

    protected virtual void OnMouseDown(MouseEventArgs e) { }

    protected virtual void OnMouseMove(MouseEventArgs e) { }

    protected virtual void OnMouseUp(MouseEventArgs e) { }

    long lastRedraw;

    const long MaxNoUpdate = 1 * 1000 * 1000 * 10; // 10 seconds

    protected virtual void ForcedInvalidate()
    {
        if (IsDisposed)
            return;

        if (offUpdateInProgress)
            return;

        // TODO: should we check if drawing is in progress???
        if (!offUpdated || (DateTime.Now.Ticks - lastRedraw > MaxNoUpdate))
        {
            lastRedraw = DateTime.Now.Ticks;
            offUpdated = true;
            try
            {
                // Use Android's Handler to post to UI thread
                Post(() => base.Invalidate());
            }
            catch (Exception e)
            {
                FleuxApplication.Log(e.Message, e);
            }
        }
    }

    public void Invoke(Action a)
    {
        a();
    }

    RuntimeEffect? currentEffect;

    public virtual void AttachEffect(string effect, Dictionary<string, object> p)
    {
        AttachEffect(new SkRuntimeEffectBlink(effect, p));
    }

    public virtual void AttachEffect(RuntimeEffect effect)
    {
        currentEffect = effect;
        currentEffect?.Start();

        Thread invalidateThread = null;
        invalidateThread = new Thread(() =>
        {
            var stime = currentEffect?.EffectStart ?? 0;
            var duration = currentEffect?.EffectDuration ?? 0;
            while (DateTime.Now.Ticks - stime < duration)
            {
                ForcedInvalidate();
                Thread.Sleep(30);
            }
            ForcedInvalidate();
        });
        invalidateThread.IsBackground = true;
        invalidateThread.Start();
    }

    protected virtual void CreateGraphicBuffers(SKImageInfo info, GRRecordingContext grContext)
    {
        if (IsDisposed || info.Width <= 0 || info.Height <= 0)
            return;

        lock (bufferLock)
        {
            int newWidth = (int)(info.Width / DownScale);
            int newHeight = (int)(info.Height / DownScale);

            // Check if we need to recreate buffers
            bool needsRecreate = false;
            
            if (offBmp != null)
            {
                try
                {
                    // Check if context changed
                    if (offBmp.GetSurface()?.Context != grContext)
                    {
                        FleuxApplication.Log("DoubleBufferedControl: Graphics context changed - recreating buffers");
                        needsRecreate = true;
                    }
                    // Check if dimensions changed (orientation change)
                    else if (OffBmpWidth != newWidth || OffBmpHeight != newHeight)
                    {
                        FleuxApplication.Log($"DoubleBufferedControl: Dimensions changed from {OffBmpWidth}x{OffBmpHeight} to {newWidth}x{newHeight} - recreating buffers");
                        needsRecreate = true;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Surface was already disposed, release and recreate
                    needsRecreate = true;
                }

                if (needsRecreate)
                {
                    ReleaseGraphicBuffersInternal();
                }
            }

            if (offBmp == null)
            {
                try
                {
                    OffBmpWidth = newWidth;
                    OffBmpHeight = newHeight;

                    this.offBmp = new Bitmap(grContext, OffBmpWidth, OffBmpHeight);
                    this.offGr = Graphics.FromImage(offBmp);

                    FleuxApplication.Log($"DoubleBufferedControl: Created buffers {OffBmpWidth}x{OffBmpHeight}");
                    CanvasSizeChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    FleuxApplication.Log($"Error creating graphic buffers: {ex.Message}", ex);
                    ReleaseGraphicBuffersInternal();
                }
            }
        }
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            IsDisposed = true;
            ReleaseGraphicBuffers();
        }
        base.Dispose(disposing);
    }

    public virtual void ReleaseGraphicBuffers()
    {
        lock (bufferLock)
        {
            ReleaseGraphicBuffersInternal();
        }
    }

    private void ReleaseGraphicBuffersInternal()
    {
        // This method should only be called while holding bufferLock
        if (this.offBmp != null)
        {
            try
            {
                // Dispose resources
                this.offGr?.Dispose();
                this.offBmp?.Dispose();
            }
            catch (Exception ex)
            {
                FleuxApplication.Log($"Error disposing graphic buffers: {ex.Message}", ex);
            }
            finally
            {
                OffBmpWidth = OffBmpHeight = 0;
                this.offBmp = null;
                this.offGr = null;
            }
        }
    }


}
