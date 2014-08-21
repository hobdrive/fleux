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
        private static ResourceManager instance;
		private static IImageResourceProvider _imageResourceProvider;

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
            RootImagePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
        }

        public static ResourceManager Instance
        {
            get { return instance ?? (instance = new ResourceManager()); }
        }

		public static void SetImageResourceProvider(IImageResourceProvider provider)
		{
			_imageResourceProvider = provider;
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

        public IImageWrapper GetIImageFromEmbeddedResource(string resourceName, Assembly assembly)
        {
            return this.CreateOrGet(this.iimagesMap,
                                    resourceName,
                                    () => GetImageResourceProvider().GetIImageFromEmbeddedResource(resourceName, assembly));
        }

        public IImageWrapper GetIImageFromNoResEmbeddedResource(string resourceName, string defaultName, Assembly asm)
        {
            return this.CreateOrGet(this.iimagesMap,
                                    resourceName,
                                    () => GetImageResourceProvider().GetIImageFromEmbeddedResource(resourceName,asm));
        }

        public Bitmap GetBitmapFromEmbeddedResource(string resourceName)
        {
            return this.GetBitmapFromEmbeddedResource(resourceName, Assembly.GetCallingAssembly());
        }

        public Bitmap GetBitmapFromEmbeddedResource(string resourceName, Assembly asm)
		{
			return this.CreateOrGet (this.bitmapMap,
			                                 resourceName,
			                                 () => GetImageResourceProvider ().GetBitmapFromEmbeddedResource (resourceName, asm));
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
            if (imagePath == null)
                return null;
            var fullPath = Path.Combine(RootImagePath, imagePath);
			return this.CreateOrGet (this.iimagesMap,
			                                 imagePath,
                                             () => GetImageResourceProvider().GetIImage(fullPath));
        }

        public Bitmap GetBitmap(string imagePath)
        {
            if (imagePath == null)
                return null;
            var fullPath = Path.Combine(RootImagePath, imagePath);
            return this.CreateOrGet (this.bitmapMap,
                                     imagePath,
                                     () => GetImageResourceProvider().GetBitmap(fullPath));
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

		private IImageResourceProvider GetImageResourceProvider()
		{
			return _imageResourceProvider ?? (_imageResourceProvider = CreateWinImageResourceProvider ());
		}

		private IImageResourceProvider CreateWinImageResourceProvider ()
		{
#if WIN32 || WINCE
			//todo: inject like xna or better
			return new WinImageProvider (); 
#else
            return null;
#endif

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
                    System.Console.WriteLine("CreateOrGet failed key="+key + " " + e.Message + "\n" + e.StackTrace);
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

    /// <summary>
    /// Abstract imaging resource provider interface
    /// ImageWrapper is a generic transparent interface (it is separated because of WinCE specifics
    /// Bitmap is a generic bitmap which could be non transparent (but support transparency on all except WinCE platforms)
    /// </summary>
    public interface IImageResourceProvider
    {
        IImageWrapper GetIImageFromEmbeddedResource(string resourceName, Assembly asm);
        Bitmap GetBitmapFromEmbeddedResource(string resourceName, Assembly asm);
        IImageWrapper GetIImage(string imagePath);
        Bitmap GetBitmap(string imagePath);
    }

}
