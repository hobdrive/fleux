using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Microsoft.Maui.Controls;
using Fleux.Core;
using Fleux.Core.Scaling;

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
    /// </summary>
    protected bool offUpdateInProgress = false;

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
        IsAntialias = false,
        FilterQuality = SKFilterQuality.High,
        Style = SKPaintStyle.Fill
    };

    void OnCanvasViewPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args)
    {
        var t0 = System.Environment.TickCount;
        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        var t1 = System.Environment.TickCount;
        canvas.Clear(SKColors.Blue);
        var tClear = System.Environment.TickCount - t1;

        var t2 = System.Environment.TickCount;
        CreateGraphicBuffers(info, args.Surface.Context);
        var tCreateBuffer = System.Environment.TickCount - t2;

        var ctime = System.Environment.TickCount;
        Fleux.UIElements.Canvas.drawtime = 0;

        var tDraw = 0;
        if (offUpdated)
        {
            lock (offBmp)
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
        lock (offBmp)
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
            
            var skImage = offBmp.GetSkImage();
            canvas.DrawImage(skImage,
                new SKRect(0, 0, info.Width, info.Height), cPaint);
            tBitmapDraw = System.Environment.TickCount - t4;

            updcntflush++;
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

        if (offUpdateInProgress)
            return;

        // TODO: should we check if drawing is in progress???
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

    protected virtual void CreateGraphicBuffers(SKImageInfo info, GRRecordingContext grContext)
    {
        if (offBmp != null && offBmp.GetSurface().Context != grContext)
        {
            // Context changed - need to recreate
            FleuxApplication.Log("DoubleBufferedControl: Graphics context changed - recreating buffers");
            ReleaseGraphicBuffers();
        }
        
        if (info.Width != OffBmpWidth || info.Height != OffBmpHeight)
        {
            ReleaseGraphicBuffers();
        }

        if (info.Width > 0 && info.Height > 0 && !IsDisposed && offBmp == null)
        {
            OffBmpWidth = (int)(info.Width / DownScale);
            OffBmpHeight = (int)(info.Height / DownScale);

            this.offBmp = new Bitmap(grContext, OffBmpWidth, OffBmpHeight);

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
