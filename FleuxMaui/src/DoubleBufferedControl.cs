using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Fleux.Core;

namespace Fleux.Controls;

public class DoubleBufferedControl : SKGLView
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
    /// If true, means next invalidate flush should also invoke full redraw
    /// </summary>
    protected bool offNeedExtraDraw = false;

    protected bool resizing;
    bool IsDisposed = false;

    public static bool PerfData = false;

    /// Main Canvas scaledown resolution
    public float DownScale = 1f;

    public DoubleBufferedControl()
    {
        this.PaintSurface += OnCanvasViewPaintSurface;
        this.EnableTouchEvents = true;
        // This causes 1x1 pixel mapping. Need to account performance!
        this.IgnorePixelScaling = false;
        this.Touch += OnTouch;
    }

    internal int totime;
    internal int updcnt = 0;
    internal int updcntflush = 0;
    internal int updcntinval = 0;

    SKPaint CanvasPaint = new SKPaint
    {
        IsAntialias = true,
        FilterQuality = SKFilterQuality.High,
        Style = SKPaintStyle.Fill
    };

    void OnCanvasViewPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args)
    {
        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear(SKColors.Blue);

        CreateGraphicBuffers(info);

        var ctime = System.Environment.TickCount;
        Fleux.UIElements.Canvas.drawtime = 0;

        if (offUpdated)
        {
            lock (offBmp)
            {
                Draw(new PaintEventArgs(offGr, new Rectangle(0, 0, offBmp.Width, offBmp.Height)));
                updcnt++;
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

        lock (offBmp)
        {
            /*
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
            canvas.DrawBitmap(offBmp.skBitmap,
                    new SKRect(0, 0, info.Width, info.Height), cPaint);

            updcntflush++;
        }

        if (PerfData)
        {
            ctime = System.Environment.TickCount - ctime;
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

                    canvas.DrawRect(new SKRect(0, 0, 300, y), new SKPaint { Color = SKColors.White.WithAlpha(200), Style = SKPaintStyle.Fill, });
                    canvas.DrawText("" + updcnt + "" + " ctime: " + ctime + " cavg:" + cavg + " canv: " + Fleux.UIElements.Canvas.drawtime, 0, y, paint);
                    //canvas.DrawText("PERF", 10, 10, paint);
                }
        }
    }

    protected void OnTouch(object sender, SKTouchEventArgs e)
    {
        bool touchPointMoved = false;

        int sx = (int)(e.Location.X / DownScale); //* this.Width / this.OffBmpWidth
        int sy = (int)(e.Location.Y / DownScale); //* this.Height / this.OffBmpHeight

        if (e.ActionType == SKTouchAction.Pressed)
        {
            touchPointMoved = true;
            var mouseEventArgs = new MouseEventArgs(sx, sy);
            OnMouseDown(mouseEventArgs);
            OnMouseMove(mouseEventArgs);
        }
        else if (e.ActionType == SKTouchAction.Moved)
        {
            touchPointMoved = true;
            var mouseEventArgs = new MouseEventArgs(sx, sy);
            OnMouseMove(mouseEventArgs);
        }
        else if (e.ActionType == SKTouchAction.Released || e.ActionType == SKTouchAction.Cancelled)
        {
            var mouseEventArgs = new MouseEventArgs(sx, sy);
            OnMouseUp(mouseEventArgs);
        }
        else if (e.ActionType == SKTouchAction.WheelChanged)
        {
            // Handle mouse wheel events if needed
            // var mouseEventArgs = new MouseEventArgs(MouseButtons.None, 0, sx, sy);
            // OnMouseWheel(mouseEventArgs);
        }
        else if (e.ActionType == SKTouchAction.Entered || e.ActionType == SKTouchAction.Exited)
        {
            // Handle mouse enter/leave events if needed
            // var mouseEventArgs = new MouseEventArgs(MouseButtons.None, 0, sx, sy);
            // OnMouseEnter(mouseEventArgs);
            // OnMouseLeave(mouseEventArgs);
        }

        e.Handled = true;
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

        if (!offUpdated || (DateTime.Now.Ticks - lastRedraw > MaxNoUpdate))
        {
            lastRedraw = DateTime.Now.Ticks;
            offUpdated = true;
            try
            {
                Microsoft.Maui.Controls.Device.BeginInvokeOnMainThread(() => base.InvalidateSurface());
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

    protected virtual void CreateGraphicBuffers(SKImageInfo info)
    {
        if (info.Width > 0 && info.Height > 0 && !IsDisposed && offBmp == null)
        {
            OffBmpWidth = (int)(info.Width / DownScale);
            OffBmpHeight = (int)(info.Height / DownScale);

            this.offBmp = new Bitmap(OffBmpWidth, OffBmpHeight);

            this.offGr = Graphics.FromImage(offBmp);

            CanvasSizeChanged?.Invoke(this, EventArgs.Empty);
        }
    }


    public virtual void Dispose()
    {
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
                OffBmpWidth = OffBmpHeight = 0;
                this.offBmp = null;
                this.offGr = null;
            }
        }
    }


}
