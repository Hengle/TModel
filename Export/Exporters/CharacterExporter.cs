using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using System.Collections.Generic;
using TModel.Modules;

namespace TModel.Export.Exporters
{
    public class CharacterExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/Characters/" } };

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaCharacterItemDefinition Character)
            {
                Character.DeepDeserialize();
                TextureRef previewIcon = null;
                if (Character.HeroDefinition is UFortHeroType HeroDefinition)
                    if (HeroDefinition.LargePreviewImage is FSoftObjectPath IconImagePath)
                        previewIcon = new TextureRef(IconImagePath.Load<UTexture2D>());

                List<ExportPreviewSet>? Styles = null;

                if (Character.ItemVariants is FPackageIndex[] Variants)
                {
                    Styles = new();
                    foreach (var variant in Variants)
                    {
                        UFortCosmeticVariant CosmeticVariant = variant.Load<UFortCosmeticVariant>();
                        ExportPreviewSet PreviewSet = new ExportPreviewSet();
                        CosmeticVariant.GetPreviewStyle(PreviewSet);
                        Styles.Add(PreviewSet);
                    }
                }

                return new ExportPreviewInfo()
                {
                    Name = Character.DisplayName,
                    PreviewIcon = previewIcon,
                    Styles = Styles
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

        public override BlenderExportInfo GetBlenderExportInfo(IPackage package)
        {
            BlenderExportInfo ExportInfo = new BlenderExportInfo();

            if (package.Base is UAthenaCharacterItemDefinition Character)
            {
                Character.DeepDeserialize();
                if (Character.BaseCharacterParts is FSoftObjectPath[] CustomParts)
                {
                    foreach (var part in CustomParts)
                    {
                        UCustomCharacterPart CharacterPart = part.Load<UCustomCharacterPart>();
                        ExportInfo.Models.Add(CharacterPart.GetModelRef());
                    }
                }
            }

            return ExportInfo;
        }
    }
}
