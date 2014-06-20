using System;
using System.Drawing;
using System.IO;

namespace Fleux.Core.NativeHelpers
{

public class BasicImage : IImage
{
    public Bitmap bitmap;
    
    public BasicImage(Bitmap b)
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

}
