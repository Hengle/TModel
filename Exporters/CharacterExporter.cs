using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using TModel.Modules;

namespace TModel.Exporters
{
    public class CharacterExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/Characters/" } };

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaCharacterItemDefinition Character)
            {
                TextureRef previewIcon = null;
                if (Character.HeroDefinition is UFortHeroType HeroDefinition)
                    if (HeroDefinition.LargePreviewImage is FSoftObjectPath IconImagePath)
                        previewIcon = new TextureRef(IconImagePath.Load<UTexture2D>());
                return new ExportPreviewInfo()
                {
                    Name = Character.DisplayName,
                    PreviewIcon = previewIcon,
                };
            }
            return null;
        }

        public override ItemTileInfo GetTileInfo(IPackage package)
        {
            if (package.Base is UAthenaCharacterItemDefinition Character)
                return Character.GetPreviewInfo();
            return null;
        }
    }
}
