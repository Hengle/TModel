﻿using CUE4Parse.UE4.Assets.Exports.Texture;
using SkiaSharp;
using System.Windows.Media.Imaging;
using static CUE4Parse_Conversion.Textures.TextureDecoder;
using System.IO;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;

namespace TModel.Export
{
    // Holds references to the same image in different
    // formats and supports conversions between those formats.
    // Supported formats are: UTexture2D, BitmapImage, and SKImage.
    public class TextureRef
    {
        private string Name;

        private UTexture2D? _UTexture2D;

        private BitmapImage? _BitmapImage;

        private SKImage? _SKImage;

        private FPackageIndex? _PackageIndex;

        private byte[] ImageBuffer;

        // For unloaded UTexture2D
        public TextureRef(FPackageIndex? index)
        {
            _PackageIndex = index;
        }

        public TextureRef(BitmapImage? image)
        {
            _BitmapImage = image;
            Name = StringUtils.RandomString();
        }

        public TextureRef(UTexture2D? texture)
        {
            _UTexture2D = texture;
            Name = texture.Name;
        }

        public TextureRef(SKImage? image)
        {
            _SKImage = image;
            Name = StringUtils.RandomString();
        }

        // Try get images
        public bool TryGet_UTexture2D(out UTexture2D? result)
        {
            if (_UTexture2D == null && _PackageIndex != null)
            {
                _UTexture2D = _PackageIndex.Load<UTexture2D>();
            }
            result = _UTexture2D;
            return _UTexture2D != null;
        }

        public bool TryGet_BitmapImage(out BitmapImage? result)
        {
            if (_BitmapImage == null)
                if (TryGet_SKImage(out var OutSKImage) && OutSKImage is SKImage ValidSKImage)
                {
                    using var data = ValidSKImage.Encode(false ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png, 100);
                    using var stream = new MemoryStream(ImageBuffer = data.ToArray(), false);
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

        public bool Save(out string SavePath)
        {
            SavePath = Path.Combine(Preferences.ExportsPath, Name + ".png");

            if (TryGet_BitmapImage(out BitmapImage bitmap))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                try
                {
                    FileStream fileStream = new FileStream(SavePath, FileMode.Create);
                    encoder.Save(fileStream);
                }
                catch
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public override string ToString() => _UTexture2D?.Name ?? Name;
    }
}
