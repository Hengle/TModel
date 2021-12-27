using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using TModel.Modules;

namespace TModel.Exporters
{
    public class PickaxeExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/PickAxes/" } };

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaPickaxeItemDefinition Pickaxe)
            {
                TextureRef previewIcon = null;
                if (Pickaxe.LargePreviewImage is FSoftObjectPath ImagePath)
                    previewIcon = new TextureRef(ImagePath.Load<UTexture2D>());
                return new ExportPreviewInfo()
                {
                    Name = Pickaxe.DisplayName,
                    PreviewIcon = previewIcon
                };
            }
            return null;
        }

        public override ItemTileInfo GetTileInfo(IPackage package)
        {
            if (package.Base is UAthenaPickaxeItemDefinition Pickaxe)
                return Pickaxe.GetPreviewInfo();
            return null;
        }
    }
}
