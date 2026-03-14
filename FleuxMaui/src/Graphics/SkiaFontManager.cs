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
            // Try to load OpenSans-Regular.ttf which has Cyrillic support
            var fallbackPath = GetFontPath("OpenSans-Regular.ttf");
            if (fallbackPath != null)
            {
                _defaultFallback = SKTypeface.FromFile(fallbackPath);
                _fallbackFonts["default"] = _defaultFallback;
            }
            
            // Also load OpenSans-Semibold for bold fallback
            var boldPath = GetFontPath("OpenSans-Semibold.ttf");
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
            // On iOS/Mac, check the app bundle
            var bundlePath = Foundation.NSBundle.MainBundle.BundlePath;
            var fontPath = Path.Combine(bundlePath, "Fonts", fontFileName);
            if (File.Exists(fontPath))
                return fontPath;
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
            // Check if character is in Cyrillic range (common issue)
            if (c >= CYRILLIC_START && c <= CYRILLIC_END)
            {
                // Check if typeface has this glyph
                var glyphId = typeface.GetGlyph(c);
                if (glyphId == 0) // Glyph ID 0 means missing glyph
                    return false;
            }
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
            // Determine if we need fallback based on sample text
            bool needsFallback = false;
            
            // If we have sample text and it contains Cyrillic, check for fallback need
            if (!string.IsNullOrEmpty(sampleText) && ContainsCyrillic(sampleText))
            {
                // Use a cache key that includes the fallback indicator
                var fallbackKey = $"{fontFamily}_{weight}_{width}_{slant}_fallback";
                
                if (_cachedTypefaces.TryGetValue(fallbackKey, out var cachedFallback))
                    return cachedFallback;
                
                // Try to create the requested typeface
                var typeface = SKTypeface.FromFamilyName(fontFamily, weight, width, slant);
                
                // Check if typeface supports the text
                needsFallback = !ContainsGlyphs(typeface, sampleText);
                
                if (needsFallback)
                {
                    Console.WriteLine($"SkiaFontManager: Font '{fontFamily}' missing Cyrillic glyphs, using fallback");
                    typeface?.Dispose();
                    
                    // Use fallback font
                    if (weight == SKFontStyleWeight.Bold && _fallbackFonts.TryGetValue("bold", out var boldFallback))
                    {
                        typeface = boldFallback;
                    }
                    else if (_defaultFallback != null)
                    {
                        typeface = _defaultFallback;
                    }
                    else
                    {
                        // No fallback available, use system font anyway
                        typeface = SKTypeface.FromFamilyName(fontFamily, weight, width, slant);
                    }
                }
                
                // Cache the result with fallback indicator
                _cachedTypefaces[fallbackKey] = typeface;
                return typeface;
            }
            else
            {
                // No special text requirements, use standard cache
                var cacheKey = $"{fontFamily}_{weight}_{width}_{slant}";
                
                if (_cachedTypefaces.TryGetValue(cacheKey, out var cached))
                    return cached;
                
                // Create standard typeface
                var typeface = SKTypeface.FromFamilyName(fontFamily, weight, width, slant);
                
                // Still check if it has basic Cyrillic support for future use
                var cyrillicSample = "АБВабв"; // Russian А, Б, В in upper and lower case
                if (!ContainsGlyphs(typeface, cyrillicSample))
                {
                    // Pre-emptively cache the fallback version too
                    var fallbackKey = $"{fontFamily}_{weight}_{width}_{slant}_fallback";
                    if (!_cachedTypefaces.ContainsKey(fallbackKey))
                    {
                        var fallback = (weight == SKFontStyleWeight.Bold && _fallbackFonts.TryGetValue("bold", out var bf)) 
                            ? bf 
                            : _defaultFallback ?? typeface;
                        _cachedTypefaces[fallbackKey] = fallback;
                    }
                }
                
                _cachedTypefaces[cacheKey] = typeface;
                return typeface;
            }
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
