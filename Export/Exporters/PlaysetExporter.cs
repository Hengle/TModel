using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TModel.Fortnite.Exports.FortniteGame;
using TModel.Modules;

namespace TModel.Export.Exporters
{
    public class PlaysetExporter : ExporterBase
    {
        public override SearchTerm SearchTerm { get; } = new SearchTerm() { SpecificPaths = new string[] 
        {
            "FortniteGame/Content/Playsets/"
        }};

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            if (package.Base is UFortPlaysetItemDefinition Playset)
            {
                TextureRef previewIcon = null;
                if ((Playset.LargePreviewImage ?? Playset.SmallPreviewImage) is FSoftObjectPath ImagePath)
                {
                    previewIcon = new TextureRef(ImagePath.Load<UTexture2D>());
                }

                return new ExportPreviewInfo()
                {
                    Name = Playset.DisplayName,
                    Package = package,
                    PreviewIcon = previewIcon
                };
            }
            return null;
        }

        public override ItemTileInfo GetTileInfo(IPackage package)
        {
            if (package.Base is UFortPlaysetItemDefinition Playset)
                return Playset.GetPreviewInfo();
            return null;
        }
    }
}
