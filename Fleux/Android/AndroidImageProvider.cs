namespace Fleux.Core
{
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using NativeHelpers;

    public class AndroidImageProvider : IImageResourceProvider
	{
		protected readonly IImagingFactory _factory;

		public AndroidImageProvider()
		{
			_factory = new AndroidImagingFactory();
		}

		public IImageWrapper GetIImageFromEmbeddedResource(string resourceName, Assembly asm)
		{
			var keyName = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(resourceName));

            if (keyName == null)
                return null;

			IImage imagingResource;
			using (var strm = (MemoryStream)asm.GetManifestResourceStream(keyName))
			{
				var pbBuf = strm.GetBuffer();
				var cbBuf = (uint)strm.Length;
				_factory.CreateImageFromBuffer(pbBuf, cbBuf, BufferDisposalFlag.BufferDisposalFlagNone, out imagingResource);
			}
			return new IImageWrapper(imagingResource);
		}

		public Bitmap GetBitmapFromEmbeddedResource(string resourceName, Assembly asm)
		{
			var original = resourceName;

			var keyName = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(resourceName)) ??
				asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(original));

            if (keyName == null)
                return null;

            return new Bitmap(asm.GetManifestResourceStream(keyName));
		}

		virtual public IImageWrapper GetIImage(string imagePath)
		{
			IImage image;
			_factory.CreateImageFromFile(imagePath, out image);
			return new IImageWrapper(image);
		}

		virtual public Bitmap GetBitmap(string imagePath)
		{
			return new Bitmap(imagePath);
		}
	}
}