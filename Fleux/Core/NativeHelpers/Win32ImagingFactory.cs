using System;
using System.Drawing;
using System.IO;

namespace Fleux.Core.NativeHelpers
{
    
public class Win32ImagingFactory : IImagingFactory
{
    public Win32ImagingFactory()
    {
    }
    
    
    public uint CreateImageFromStream(Stream stream, out IImage image)
    {
        image = new BasicImage(new Bitmap(stream));
        return 0;
    }
    
    public uint CreateImageFromFile(string filename, out IImage image)
    {
        image = new BasicImage(new Bitmap(filename));
        return 0;
    }
    
    public uint CreateImageFromBuffer(byte[] buffer, uint size, BufferDisposalFlag disposalFlag, out IImage image)
    {
        image = new BasicImage(new Bitmap(new MemoryStream(buffer, 0, (int)size)));
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
