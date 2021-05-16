using System;
using System.Drawing;
using System.IO;

namespace Fleux.Core.NativeHelpers
{
    
    public class AImage : IImage
    {
        public Bitmap bitmap;
        
        public AImage(Bitmap b)
        {
            this.bitmap = b;
        }
        public uint GetPhysicalDimension(out System.Drawing.Size size)
        {
            size = new Size(bitmap.Width, bitmap.Height);
            return 0;
        }
        
        public uint GetImageInfo(out ImageInfo info)
        {
            info = new ImageInfo();
            info.Height = (uint)bitmap.Height;
            info.pixelFormat = PixelFormatID.PixelFormat32bppARGB;
            info.TileHeight = (uint)bitmap.Height;
            info.TileWidth = (uint)bitmap.Width;
            info.Width = (uint)bitmap.Width;
            info.Xdpi = 92;
            info.Ydpi = 92;
            return 0;
    		}
        
        public uint SetImageFlags(uint flags)
        {
            throw new NotImplementedException();
        }
        
        public uint Draw(Graphics g, ref System.Drawing.Rectangle dstRect, IntPtr NULL)
        {
            g.DrawImage(bitmap, 0,0, dstRect, GraphicsUnit.Pixel);
            return 0;
        }

        public uint Draw(IntPtr hdc, ref System.Drawing.Rectangle dstRect, IntPtr NULL)
        {
            throw new NotImplementedException();
        }
        
        public uint PushIntoSink()
        {
            throw new NotImplementedException();
        }
        
        public uint GetThumbnail(uint thumbWidth, uint thumbHeight, out IImage thumbImage)
        {
            throw new NotImplementedException();
        }
        
        public uint Release()
        {
            throw new NotImplementedException();
        }
    }
            
        
    public class AndroidImagingFactory : IImagingFactory
    {
        public AndroidImagingFactory()
        {
        }
        
        public uint CreateImageFromStream(Stream input, out IImage image)
        {
            image = new AImage(new Bitmap(input));
            return 0;
        }

        public uint CreateImageFromFile(string filename, out IImage image)
        {
            image = new AImage(new Bitmap(filename));
            return 0;
        }
        
        public uint CreateImageFromBuffer(byte[] buffer, uint size, BufferDisposalFlag disposalFlag, out IImage image)
        {
            image = new AImage(new Bitmap(new MemoryStream(buffer, 0, (int)size)));
            return 0;
        }
        
        public uint CreateNewBitmap(uint width, uint height, PixelFormatID pixelFormat, out IBitmapImage bitmap)
        {
            throw new NotImplementedException();
        }
        
        public uint CreateBitmapFromImage(IImage image, uint width, uint height, PixelFormatID pixelFormat, InterpolationHint hints, out IBitmapImage bitmap)
        {
            throw new NotImplementedException();
        }
        
        public uint CreateBitmapFromBuffer()
        {
            throw new NotImplementedException();
        }
        
        public uint CreateImageDecoder()
        {
            throw new NotImplementedException();
        }
        
        public uint CreateImageEncoderToStream()
        {
            throw new NotImplementedException();
        }
        
        public uint CreateImageEncoderToFile()
        {
            throw new NotImplementedException();
        }
        
        public uint GetInstalledDecoders()
        {
            throw new NotImplementedException();
        }
        
        public uint GetInstalledEncoders()
        {
            throw new NotImplementedException();
        }
        
        public uint InstallImageCodec()
        {
            throw new NotImplementedException();
        }
        
        public uint UninstallImageCodec()
        {
            throw new NotImplementedException();
        }
    }
}
