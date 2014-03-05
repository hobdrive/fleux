namespace Fleux.Core
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
#if WINCE
    using Microsoft.WindowsCE.Forms;
#endif
    using NativeHelpers;

    public class ResourceManager
    {
        private readonly Dictionary<string, Font> fontMap = new Dictionary<string, Font>();
        private readonly Dictionary<string, Brush> brushMap = new Dictionary<string, Brush>();
        private readonly Dictionary<string, Pen> penMap = new Dictionary<string, Pen>();
        private readonly Dictionary<string, Bitmap> bitmapMap = new Dictionary<string, Bitmap>();
        private readonly Dictionary<string, Bitmap> bitmapSizedMap = new Dictionary<string, Bitmap>();
        private readonly Dictionary<string, IImageWrapper> iimagesMap = new Dictionary<string, IImageWrapper>();
        private readonly IImagingFactory factory;

        private static ResourceManager instance;

        public const int FontQualityDraft = 0;
        public const int FontQualityNormal = 1;
        public const int FontQualityAntialiased = 2;
        public const int FontQualityClearType = 3;

        #pragma warning disable 0219, 0414
        int fontQuality = FontQualityNormal;

        /// <summary>
        /// Gets or sets the root image search path.
        /// </summary>
        public string RootImagePath{ get; set; }

        private ResourceManager()
        {
#if WINCE
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                try{
                    this.factory = (IImagingFactory)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("327ABDA8-072B-11D3-9D7B-0000F81EF32E")));
                }catch(Exception){
                    // some winCE do not have even this
                    this.factory = new Win32ImagingFactory();
                }
            }
#endif
#if WIN32
            {
                this.factory = new Win32ImagingFactory();
            }
#endif
#if __ANDROID__
            // todo
#endif
            RootImagePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
        }

        public static ResourceManager Instance
        {
            get { return instance ?? (instance = new ResourceManager()); }
        }

        public void SetFontQuality(int fontQuality)
        {
            this.fontQuality = fontQuality;
        }
        
        public Font GetFont(string fontFace, System.Drawing.FontStyle fontStyle, int size)
        {
            return this.CreateOrGet(this.fontMap,
                                    CreateKey(fontFace, fontStyle, size),
                                    () => this.CreateFont(fontFace, size, fontStyle));
        }

        public Font CreateFont(string fontFace, int size, FontStyle fontStyle)
        {
#if WINCE
            var logFont = new LogFont
                              {
                                  FaceName = fontFace,
                                  Height = FleuxApplication.FromPointsToPixels(size),
                                  //// Width = (int) (FleuxApplication.FromPointsToPixels(size) * 0.425),
                                  Weight = ((fontStyle & FontStyle.Bold) == FontStyle.Bold) ? LogFontWeight.ExtraLight : LogFontWeight.Thin
                              };
            switch(this.fontQuality){
                case FontQualityNormal:
                    logFont.Quality = LogFontQuality.Default;
                    break;
                case FontQualityDraft:
                    logFont.Quality = LogFontQuality.Draft;
                    break;
                case FontQualityAntialiased:
                    logFont.Quality = LogFontQuality.AntiAliased;
                    break;
                case FontQualityClearType:
                    logFont.Quality = LogFontQuality.ClearType;
                    break;
                default:
                    break;
            }
            return Font.FromLogFont(logFont);
#else
            return new Font(fontFace, size, fontStyle);
#endif
        }

        public Brush GetBrush(Color color)
        {
            return this.CreateOrGet(this.brushMap,
                                    color.GetHashCode().ToString(),
                                    () => new SolidBrush(color));
        }

        public Pen GetPen(Color color, int width)
        {
            return this.CreateOrGet(this.penMap,
                                    color.GetHashCode() + width.ToString(),
                                    () => new Pen(color, width));
        }

        public IImageWrapper GetIImageFromEmbeddedResource(string resourceName)
        {
            return this.GetIImageFromEmbeddedResource(resourceName, Assembly.GetCallingAssembly());
        }

        public IImageWrapper GetIImageFromEmbeddedResource(string resourceName, Assembly asm)
        {
            return this.CreateOrGet(this.iimagesMap,
                                    resourceName,
                                    () =>
                                    {
                                        var original = resourceName;
                                        var keyName = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(resourceName)) ??
                                                         asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(original));

                                        // if there is not a dpi aware image just use the original name for find it
                                        IImage imagingResource;
                                        using (var strm = asm.GetManifestResourceStream(keyName))
                                        {
                                            var cbBuf = (uint)strm.Length;
                                            byte[] pbBuf = new byte[strm.Length];
                                            strm.Read(pbBuf, 0, unchecked((int)strm.Length));
                                            //var pbBuf = strm.GetBuffer();
                                            factory.CreateImageFromBuffer(pbBuf, cbBuf, BufferDisposalFlag.BufferDisposalFlagNone, out imagingResource);
                                        }
                                        return new IImageWrapper(imagingResource);
                                    });
        }

        public IImageWrapper GetIImageFromNoResEmbeddedResource(string resourceName, string defaultName, Assembly asm)
        {
            return this.CreateOrGet(this.iimagesMap,
                                    resourceName,
                                    () =>
                                    {
                                        var keyName = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(resourceName));

                                        IImage imagingResource;
                                        using (var strm = (MemoryStream)asm.GetManifestResourceStream(keyName))
                                        {
                                            var pbBuf = strm.GetBuffer();
                                            var cbBuf = (uint)strm.Length;
                                            factory.CreateImageFromBuffer(pbBuf, cbBuf, BufferDisposalFlag.BufferDisposalFlagNone, out imagingResource);
                                        }
                                        return new IImageWrapper(imagingResource);
                                    });
        }

        public Bitmap GetBitmapFromEmbeddedResource(string resourceName)
        {
            return this.GetBitmapFromEmbeddedResource(resourceName, Assembly.GetCallingAssembly());
        }

        public Bitmap GetBitmapFromEmbeddedResource(string resourceName, Assembly asm)
        {
            return this.CreateOrGet(this.bitmapMap,
                                    resourceName,
                                    () =>
                                    {
                                        var original = resourceName;

                                        var keyName = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(resourceName)) ??
                                                         asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(original));

                                        // if there is not a dpi aware image just use the original name for find it
                                        Bitmap bitmap;
                                        if (Environment.OSVersion.Platform == PlatformID.WinCE)
                                        {
                                            using (var strm = (MemoryStream)asm.GetManifestResourceStream(keyName))
                                            {
                                                bitmap = new Bitmap(strm);
                                            }
                                        }
                                        else
                                        {
                                            bitmap = new Bitmap(asm.GetManifestResourceStream(keyName));
                                        }
                                        return bitmap;
                                    });
        }

        public Bitmap GetBitmapFromEmbeddedResource(string resourceName, int width, int height, Assembly asm)
        {
            var locator = string.Format("{0}|{1}|{2}", resourceName, width, height);
            var original = this.GetBitmapFromEmbeddedResource(resourceName, asm);

            return this.CreateOrGet(this.bitmapSizedMap,
                                    locator,
                                    () =>
                                    {
                                        var scaledBitmap = new Bitmap(width, height);

                                        using (var gr = Graphics.FromImage(scaledBitmap))
                                        {
                                            gr.DrawImage(original,
                                                new Rectangle(0, 0, width, height),
                                                new Rectangle(0, 0, original.Width, original.Height),
                                                GraphicsUnit.Pixel);
                                        }
                                        return scaledBitmap;
                                    });
        }

        public IImageWrapper GetIImage(string imagePath)
        {
            var fullPath = Path.Combine(RootImagePath, imagePath);
            return this.CreateOrGet(this.iimagesMap,
                        imagePath,
                        delegate
                        {
                            IImage image;
                            this.factory.CreateImageFromFile(fullPath, out image);
                            return new IImageWrapper(image);
                        });
                }

        public void ReleaseAllResources()
        {
            ReleaseAllFromDictionary(this.brushMap, b => b.Dispose());
            ReleaseAllFromDictionary(this.fontMap, f => f.Dispose());
            ReleaseAllFromDictionary(this.penMap, p => p.Dispose());
            ReleaseAllFromDictionary(this.bitmapMap, b => b.Dispose());
            ReleaseAllFromDictionary(this.bitmapSizedMap, b => b.Dispose());
            ReleaseAllFromDictionary(this.iimagesMap, i => { });
        }

        private static void CleanUpMap<T>(Dictionary<string, T> dictionary, int max)
            where T : IDisposable
        {
            if (dictionary.Count > max)
            {
                while (dictionary.Count > 0)
                {
                    var k = dictionary.Keys.First();
                    var i = dictionary[k];
                    dictionary.Remove(k);
                    i.Dispose();
                }
            }
        }

        private static void ReleaseAllFromDictionary<T>(Dictionary<string, T> source, Action<T> releaseAction)
        {
            source.Values.ToList().ForEach(releaseAction);
            source.Clear();
        }

        private static string CreateKey(string p, FontStyle fontStyle, int size)
        {
            return string.Format("{0}-{1}-{2}", p, fontStyle, size);
        }

        private T CreateOrGet<T>(IDictionary<string, T> source, string key, Func<T> creator)
        {
            this.CleanUpResources();

            if (!source.ContainsKey(key))
            {
                try{
                    T val = creator();
                    source.Add(key, val);
                }catch(Exception e){
#if DEBUG
                    System.Console.Write(e);
#endif
                    return default(T);
                }
            }
            return source[key];
        }

        private void CleanUpResources()
        {
            CleanUpMap(this.iimagesMap, 15);
        }
    }
}
