using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.InteropServices;
using Extension;

namespace Common;
public static class ImageExtension
{
    public static ImageData GetImageData(string iconPath)
    {
        using var image = SixLabors.ImageSharp.Image.Load(Configuration.Default, iconPath);
        return image.CloneAs<Rgba32>().GetPixelData() is var pixelSpan &&
        MemoryMarshal.AsBytes(pixelSpan).ToArray() is byte[] imageBytes ?
        new ImageData(image.Width, image.Height, imageBytes) :
        throw new Exception("GetImageData error");
    }

    public static WindowIcon CreateWindowIcon(string iconPath) =>
        GetImageData(iconPath) is ImageData imageData ?
        new WindowIcon(new OpenTK.Windowing.Common.Input.Image(imageData.Width, imageData.Height, imageData.Value)) :
        throw new Exception("CreateWindowIcon error");

    public static Span<TPixel> GetPixelData<TPixel>(this Image<TPixel> image)
        where TPixel : unmanaged, IPixel<TPixel> =>
        image.Frames.RootFrame.Size() is var size &&
            new TPixel[size.Width * size.Height] is var pixelSpan ?
            pixelSpan.With(x => image.CopyPixelDataTo(x)) :
            throw new Exception("CreateWindowIcon error");
}

public class ImageData
{
    public int Width { get; init; }
    public int Height { get; init; }
    public byte[] Value { get; init; }

    public ImageData(int width, int height, byte[] value)
    {
        this.Width = width;
        this.Height = height;
        this.Value = value;
    }
}

