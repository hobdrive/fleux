using System;
using System.Collections.Generic;
using System.Drawing;
using SkiaSharp;

namespace Fleux.Core;

public class SkRuntimeEffectBlink : RuntimeEffect
{
    SKColor color = SKColors.Red;
    float level = 0.9f;
    float clevel = 0;
    int interval = 1000;
    int loops = 3;

    public SkRuntimeEffectBlink(string name, Dictionary<string, object> parameters)
    : base(name, parameters)
    {
    }

    public override void Start()
    {
        base.Start();

        if (Name == "blink")
        {
            if (Parameters.ContainsKey("color"))
                color = ((System.Drawing.Color)Parameters["color"]).ToSKColor();
            if (Parameters.ContainsKey("level"))
                level = float.Parse(Parameters["level"].ToString());
            if (Parameters.ContainsKey("interval"))
                interval = int.Parse(Parameters["interval"].ToString());
            if (Parameters.ContainsKey("loops"))
                loops = int.Parse(Parameters["loops"].ToString());
            EffectDuration = interval * loops * MS_TO_TICKS;
        }
        if (Name == "blur")
        {
            float BlurDistance = 0.003f;
            if (Parameters.ContainsKey("distance"))
                BlurDistance = (float)Parameters["distance"];
            //currentEffect.Parameters["BlurDistance"].SetValue(BlurDistance);
        }
    }

    public override bool Process()
    {
        base.Process();
        if (IsDisposed)
            return false;
        if (Name == "blink")
        {
            var ts_mod = ((DateTime.Now.Ticks - EffectStart) / MS_TO_TICKS) % interval;
            if (ts_mod > interval / 2)
                ts_mod = interval - ts_mod;
            clevel = level * 2 * ts_mod / interval;
        }
        else if (Name == "blur")
        {
            // Process blur effect
        }
        return true;
    }

    public override object GetState()
    {
        /*
        float percentage = 0.5f;
        SKColor targetColor = SKColors.Red;

        // The original shader (e.g., from an image or gradient)
        SKShader originalShader = ...;

        // The target color as a shader
        SKShader colorShader = SKShader.CreateColor(targetColor);

        // Lerp between the two shaders
        SKShader lerpedShader = SKShader.Lerp(originalShader, colorShader, percentage);

        // Apply to paint
        var paint = new SKPaint
        {
            Shader = lerpedShader
        };
        */
        /*
        var lerpFilter = SKColorFilter.CreateLerp(
            clevel,
            SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcIn),
            SKColorFilter.CreateBlendMode(SKColors.Transparent, SKBlendMode.Src)
        );

        // Apply to paint
        var paint = new SKPaint
        {
            ColorFilter = lerpFilter
        };
        var lerpColor = color.WithAlpha((byte)(255 * clevel));
        var paint = new SKPaint
        {
            Color = lerpColor,
            BlendMode = SKBlendMode.SrcOver
        };*/
        // Compute lerp factors
        float inv = level - clevel;

        // Build color matrix for lerp
        float[] colorMatrix = new float[]
        {
            inv, 0,   0,   0, clevel*color.Red/255f,
            0,   inv, 0,   0, clevel*color.Green/255f,
            0,   0,   inv, 0, clevel*color.Blue/255f,
            0,   0,   0,   1, 0
        };
        var maskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10 * clevel);
        var blurFilter = SKImageFilter.CreateBlur(5, 5);

        var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(colorMatrix),
            //MaskFilter = maskFilter,
            ImageFilter = blurFilter,
        };
        return paint;
    }

}
