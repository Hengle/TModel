using CUE4Parse.FN.Exports.FortniteGame.NoProperties;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using TModel.Modules;

namespace TModel.Exporters
{
    public class BackpackExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/Backpacks/" } };

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaBackpackItemDefinition Backpack)
            {
                TextureRef previewIcon = null;
                if (Backpack.LargePreviewImage is FSoftObjectPath ImagePath)
                    previewIcon = new TextureRef(ImagePath.Load<UTexture2D>());
                return new ExportPreviewInfo() 
                { 
                    Name = Backpack.DisplayName,
                    PreviewIcon = previewIcon 
                };
            }
            return null;
        }

        public override ItemTileInfo GetTileInfo(IPackage package)
        {
            if (package.Base is UAthenaBackpackItemDefinition Backpack)
                return Backpack.GetPreviewInfo();
            return null;
        }
    }
}
