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

    internal class WinImageProvider : IImageResourceProvider
    {
        private readonly IImagingFactory _factory;

        public WinImageProvider()
        {
            _factory = GetIImageFactory();
        }

        public IImageWrapper GetIImageFromEmbeddedResource(string resourceName, Assembly asm)
        {
            var keyName = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(resourceName));

            IImage imagingResource;
            var rstream = asm.GetManifestResourceStream(keyName);
            if(rstream is MemoryStream)
            {
                using(var strm = (rstream as MemoryStream))
                {
                    var pbBuf = strm.GetBuffer();
                    var cbBuf = (uint)strm.Length;
                    _factory.CreateImageFromBuffer(pbBuf, cbBuf, BufferDisposalFlag.BufferDisposalFlagNone, out imagingResource);
                }
            }else{
                using(var strm = rstream)
                {
                    var pbBuf = new byte[strm.Length];
                    strm.Read(pbBuf, 0, (int)strm.Length);
                    var cbBuf = (uint)strm.Length;
                    _factory.CreateImageFromBuffer(pbBuf, cbBuf, BufferDisposalFlag.BufferDisposalFlagNone, out imagingResource);
                }
            }
            return new IImageWrapper(imagingResource);
        }

        public Bitmap GetBitmapFromEmbeddedResource(string resourceName, Assembly asm)
        {
            var original = resourceName;

            var keyName = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(resourceName)) ??
                          asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(original));

            // if there is not a dpi aware image just use the original name for find it
            Bitmap bitmap;
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                using (var strm = (MemoryStream) asm.GetManifestResourceStream(keyName))
                {
                    bitmap = new Bitmap(strm);
                }
            }
            else
            {
                bitmap = new Bitmap(asm.GetManifestResourceStream(keyName));
            }
            return bitmap;
        }

        public Bitmap GetBitmap(string imagePath)
        {
            Bitmap bitmap = null;
            try{
                bitmap = new Bitmap(imagePath);
            }catch(Exception){
            }
            return bitmap;
        }


        public IImageWrapper GetIImage(string imagePath)
        {
            IImage image;
            _factory.CreateImageFromFile(imagePath, out image);
            return new IImageWrapper(image);
        }

        private static IImagingFactory GetIImageFactory()
        {
            IImagingFactory factory;
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                try
                {
#if !SILVERLIGHT
					factory = (IImagingFactory)Activator.CreateInstance (Type.GetTypeFromCLSID (new Guid ("327ABDA8-072B-11D3-9D7B-0000F81EF32E")));
#endif
#if SILVERLIGHT
                    factory = new Win32ImagingFactory();
#endif
                }
                catch (Exception)
                {
                    // some winCE do not have even this
                    factory = new Win32ImagingFactory();
                }

            }
            else
            {
                factory = new Win32ImagingFactory();
            }
            return factory;

        }
    }
}
