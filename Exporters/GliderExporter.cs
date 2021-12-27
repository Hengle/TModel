using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using TModel.Modules;

namespace TModel.Exporters
{
    public class GliderExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/Gliders/" } };

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaGliderItemDefinition Glider)
            {
                TextureRef SmallImageRef = null;
                if (Glider.LargePreviewImage is FSoftObjectPath ImagePath)
                    SmallImageRef = new TextureRef(ImagePath.Load<UTexture2D>());
                return new ExportPreviewInfo()
                {
                    Name = Glider.DisplayName,
                    PreviewIcon = SmallImageRef
                };
            }
            return null;
        }

        public override ItemTileInfo GetTileInfo(IPackage package)
        {
            if (package.Base is UAthenaGliderItemDefinition Glider)
                return Glider.GetPreviewInfo();
            return null;
        }
    }
}
