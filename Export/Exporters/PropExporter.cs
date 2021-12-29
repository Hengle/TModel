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
    public class PropExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] 
        {
            "FortniteGame/Content/Playsets/PlaysetProps/"
        }};

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            if (package.Base is UFortPlaysetPropItemDefinition Prop)
            {
                TextureRef previewIcon = null;
                if (Prop.SmallPreviewImage is FSoftObjectPath ImagePath)
                {
                    previewIcon = new TextureRef(ImagePath.Load<UTexture2D>());
                }

                return new ExportPreviewInfo()
                {
                    Name = Prop.DisplayName,
                    PreviewIcon = previewIcon
                };
            }
            return null;
        }

        public override ItemTileInfo GetTileInfo(IPackage package)
        {
            if (package.Base is UFortPlaysetPropItemDefinition Prop)
                return Prop.GetPreviewInfo();
            return null;
        }
    }
}
