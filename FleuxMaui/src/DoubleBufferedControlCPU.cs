using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Microsoft.Maui.Controls;
using Fleux.Core;
using Fleux.Core.Scaling;

namespace Fleux.Controls;

/// <summary>
/// CPU-based rendering control - faster on iOS simulator than OpenGL
/// Use this variant on iOS Simulator for better performance
/// </summary>
public class DoubleBufferedControlCPU : SKCanvasView
{
    protected Bitmap? offBmp;
    protected Graphics? offGr;

    protected bool offBmpDraw = false;

    public int OffBmpWidth { get; protected set; }
    public int OffBmpHeight { get; protected set; }

    public virtual System.Drawing.Color BackColor { get; set; }

    public event EventHandler? CanvasSizeChanged;

    protected bool offUpdated;
    protected bool offUpdateInProgress = false;

    protected bool resizing;
    bool IsDisposed = false;

    public static bool PerfData = false;

    public float DownScale = 1f;

    public DoubleBufferedControlCPU()
    {
        this.PaintSurface += OnCanvasViewPaintSurface;
        this.EnableTouchEvents = true;
        this.IgnorePixelScaling = false;
        this.Touch += OnTouch;
        
        Fleux.Core.FleuxApplication.Log("DoubleBufferedControlCPU: Using CPU rendering (faster on simulator)");
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

    void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
    {
        var t0 = System.Environment.TickCount;
        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        var t1 = System.Environment.TickCount;
        canvas.Clear(SKColors.Blue);
        var tClear = System.Environment.TickCount - t1;

        var t2 = System.Environment.TickCount;
        CreateGraphicBuffers(info);
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
            var cPaint = currentEffect?.GetState() as SKPaint;
            if (cPaint == null)
            {
                cPaint = CanvasPaint;
            }
            var t4 = System.Environment.TickCount;
            var skImage = offBmp.GetSkImage();  // Don't dispose - owned by image
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

                    var y = paint.FontMetrics.Descent - paint.FontMetrics.Ascent;

                    int offset = 10;
                    int lineHeight = (int)(y * 1.2f);

                    canvas.DrawRect(new SKRect(offset, 0, 520.ToPixels(), lineHeight * 4), new SKPaint { Color = SKColors.White.WithAlpha(200), Style = SKPaintStyle.Fill, });
                    canvas.DrawText($"CPU Frame #{updcnt} Total:{tTotal}ms Clear:{tClear}ms Buf:{tCreateBuffer}ms", offset, y, paint);
                    canvas.DrawText($"Draw:{tDraw}ms BmpDraw:{tBitmapDraw}ms Canvas:{Fleux.UIElements.Canvas.drawtime}ms", offset, y + lineHeight, paint);
                    canvas.DrawText($"Size:{info.Width}x{info.Height} OffBmp:{OffBmpWidth}x{OffBmpHeight} Scale:{DownScale}", offset, y + lineHeight * 2, paint);
                    canvas.DrawText($"IgnorePixelScaling:{IgnorePixelScaling} SKCanvasView Avg:{cavg}ms", offset, y + lineHeight * 3, paint);
                }
        }
    }

    protected void OnTouch(object sender, SKTouchEventArgs e)
    {
        bool touchPointMoved = false;

        int sx = (int)(e.Location.X / DownScale);
        int sy = (int)(e.Location.Y / DownScale);

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
                // this.offGr.Dispose();
                // this.offBmp.Dispose();
                OffBmpWidth = OffBmpHeight = 0;
                this.offBmp = null;
                this.offGr = null;
            }
        }
    }
}
