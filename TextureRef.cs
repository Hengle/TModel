using CUE4Parse.UE4.Assets.Exports.Texture;
using SkiaSharp;
using System.Windows.Media.Imaging;
using static CUE4Parse_Conversion.Textures.TextureDecoder;
using System.IO;


// Holds references to the same image in different
// formats and supports conversions between those formats.
// Supported formats are: UTexture2D, BitmapImage, and SKImage.
public class TextureRef
{
    public static TextureRef Empty { get; } = new TextureRef();

    private UTexture2D? _UTexture2D;

    private BitmapImage? _BitmapImage;

    private SKImage? _SKImage;

    private TextureRef() { }

    // Public Constructors
    public TextureRef(BitmapImage? image)
    {
        _BitmapImage = image;
    }

    public TextureRef(UTexture2D? texture)
    {
        _UTexture2D = texture;
    }

    public TextureRef(SKImage? image)
    {
        _SKImage = image;
    }

    // Try get images
    public bool TryGet_UTexture2D(out UTexture2D? result)
    {
        result = _UTexture2D;
        return _UTexture2D != null;
    }

    public bool TryGet_BitmapImage(out BitmapImage? result)
    {
        if (_BitmapImage == null)
            if (TryGet_SKImage(out var OutSKImage) && OutSKImage is SKImage ValidSKImage)
            {
                using var data = ValidSKImage.Encode(false ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png, 100);
                using var stream = new MemoryStream(data.ToArray(), false);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.None;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                _BitmapImage = image;
            }

        result = _BitmapImage;
        return _BitmapImage != null;
    }

    public bool TryGet_SKImage(out SKImage? result)
    {

        if (_SKImage == null && _UTexture2D is UTexture2D ValidUTexture2D)
            _SKImage = ValidUTexture2D.Decode();

        result = _SKImage;
        return _SKImage != null;
    }
}