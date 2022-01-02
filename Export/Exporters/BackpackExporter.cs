using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.FN.Exports.FortniteGame.NoProperties;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using TModel.Export;
using TModel.Export.Exporters;
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
                    Description = Backpack.Description,
                    Package = package,
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

        public override BlenderExportInfo GetBlenderExportInfo(IPackage package, int[]? styles = null)
        {
            BlenderExportInfo ExportInfo = new BlenderExportInfo();
            ExportInfo.Name = package.Name;
            if (package.Base is UAthenaBackpackItemDefinition Backpack)
            {
                Backpack.DeepDeserialize();
                if (Backpack.CharacterParts is UCustomCharacterPart[] CustomParts)
                {
                    foreach (var part in CustomParts)
                    {
                        ExportInfo.Models.Add(part.GetModelRef());
                    }
                }

            }

            return ExportInfo;
        }
    }
}
