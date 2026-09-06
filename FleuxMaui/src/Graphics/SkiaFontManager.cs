using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace System.Drawing;

/// <summary>
/// Manages font fallback for Skia to handle missing glyphs (e.g., Cyrillic in English-only system fonts)
/// </summary>
public static class SkiaFontManager
{
    private static readonly Dictionary<string, SKTypeface> _fallbackFonts = new();
    private static readonly Dictionary<string, SKTypeface> _cachedTypefaces = new();
    private static readonly object _lock = new();
    private static SKTypeface _defaultFallback;
    
    // Cyrillic Unicode range: U+0400 to U+04FF
    private const int CYRILLIC_START = 0x0400;
    private const int CYRILLIC_END = 0x04FF;
    
    static SkiaFontManager()
    {
        LoadFallbackFonts();
    }
    
    /// <summary>
    /// Load built-in fonts that support extended character sets (Cyrillic, etc.)
    /// </summary>
    private static void LoadFallbackFonts()
    {
        try
        {
            // Segoe UI Semibold covers Cyrillic and modern currency glyphs
            // (including ₽). Fall back to OpenSans when it is not bundled.
            var fallbackPath = GetFontPath("SegoeUI-Semibold.ttf")
                ?? GetFontPath("OpenSans-Regular.ttf");
            if (fallbackPath != null)
            {
                _defaultFallback = SKTypeface.FromFile(fallbackPath);
                _fallbackFonts["default"] = _defaultFallback;
            }
            
            // Prefer the dedicated bold fallback when the app bundles one.
            var boldPath = GetFontPath("SegoeUI-Semibold.ttf")
                ?? GetFontPath("OpenSans-Semibold.ttf");
            if (boldPath != null)
            {
                _fallbackFonts["bold"] = SKTypeface.FromFile(boldPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SkiaFontManager: Failed to load fallback fonts: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get the file path for a MAUI embedded font
    /// </summary>
    private static string GetFontPath(string fontFileName)
    {
        try
        {
#if ANDROID
            // On Android, try multiple locations
            var context = Android.App.Application.Context;
            
            // Try from assets first (most reliable)
            try
            {
                using var testStream = context.Assets.Open($"Fonts/{fontFileName}");
                // If we can open it, copy to cache and return path
                var cachePath = Path.Combine(context.CacheDir.AbsolutePath, fontFileName);
                if (!File.Exists(cachePath))
                {
                    using var fileStream = File.Create(cachePath);
                    testStream.CopyTo(fileStream);
                }
                return cachePath;
            }
            catch
            {
                // Try without Fonts/ prefix
                try
                {
                    using var testStream = context.Assets.Open(fontFileName);
                    var cachePath = Path.Combine(context.CacheDir.AbsolutePath, fontFileName);
                    if (!File.Exists(cachePath))
                    {
                        using var fileStream = File.Create(cachePath);
                        testStream.CopyTo(fileStream);
                    }
                    return cachePath;
                }
                catch
                {
                    // Asset not found
                }
            }
            
            // Try from files directory
            var filesDir = context.FilesDir.AbsolutePath;
            var paths = new[]
            {
                Path.Combine(filesDir, "..", "fonts", fontFileName),
                Path.Combine(filesDir, "fonts", fontFileName),
            };
            
            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return path;
            }
#elif IOS || MACCATALYST
            // MAUI currently copies these fonts to the bundle root on iOS,
            // while older builds used a Fonts subdirectory. Support both so
            // the shared fallback behaves consistently across platforms.
            var bundlePath = Foundation.NSBundle.MainBundle.BundlePath;
            var paths = new[]
            {
                Path.Combine(bundlePath, fontFileName),
                Path.Combine(bundlePath, "Fonts", fontFileName),
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return path;
            }
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SkiaFontManager: Error locating font {fontFileName}: {ex.Message}");
        }
        return null;
    }
    
    /// <summary>
    /// Check if a typeface contains glyphs for the given text
    /// </summary>
    private static bool ContainsGlyphs(SKTypeface typeface, string text)
    {
        if (typeface == null || string.IsNullOrEmpty(text))
            return false;
            
        foreach (var c in text)
        {
            if (char.IsControl(c) || char.IsWhiteSpace(c))
                continue;

            // Glyph ID 0 means the requested typeface cannot render it.
            if (typeface.GetGlyph(c) == 0)
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Detect if text contains Cyrillic characters
    /// </summary>
    public static bool ContainsCyrillic(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;
            
        foreach (var c in text)
        {
            if (c >= CYRILLIC_START && c <= CYRILLIC_END)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Get a typeface with fallback support for missing glyphs
    /// </summary>
    public static SKTypeface GetTypeface(string fontFamily, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string sampleText = null)
    {
        lock (_lock)
        {
            var cacheKey = $"{fontFamily}_{weight}_{width}_{slant}";
            if (!_cachedTypefaces.TryGetValue(cacheKey, out var typeface))
            {
                typeface = SKTypeface.FromFamilyName(fontFamily, weight, width, slant);
                _cachedTypefaces[cacheKey] = typeface;
            }

            // Use the bundled cross-platform font whenever the requested
            // typeface cannot render the actual text (Cyrillic, ₽, etc.).
            if (string.IsNullOrEmpty(sampleText) || ContainsGlyphs(typeface, sampleText))
                return typeface;

            var fallbackKey = $"{fontFamily}_{weight}_{width}_{slant}_fallback";
            if (_cachedTypefaces.TryGetValue(fallbackKey, out var cachedFallback))
                return cachedFallback;

            Console.WriteLine($"SkiaFontManager: Font '{fontFamily}' is missing glyphs, using fallback");
            var fallback = (weight == SKFontStyleWeight.Bold && _fallbackFonts.TryGetValue("bold", out var boldFallback))
                ? boldFallback
                : _defaultFallback ?? typeface;
            _cachedTypefaces[fallbackKey] = fallback;
            return fallback;
        }
    }
    
    /// <summary>
    /// Clear cached typefaces (useful for testing or memory management)
    /// </summary>
    public static void ClearCache()
    {
        lock (_lock)
        {
            foreach (var tf in _cachedTypefaces.Values)
            {
                if (tf != _defaultFallback && !_fallbackFonts.ContainsValue(tf))
                {
                    tf?.Dispose();
                }
            }
            _cachedTypefaces.Clear();
        }
    }
}
