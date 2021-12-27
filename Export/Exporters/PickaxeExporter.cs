using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using TModel.Modules;

namespace TModel.Export.Exporters
{
    public class PickaxeExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/PickAxes/" } };

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaPickaxeItemDefinition Pickaxe)
            {
                TextureRef previewIcon = null;
                if (Pickaxe.WeaponDefinition is UFortWeaponMeleeItemDefinition WeaponDef)
                {
                    if (WeaponDef.LargePreviewImage is FSoftObjectPath ImagePath)
                    {
                        previewIcon = new TextureRef(ImagePath.Load<UTexture2D>());
                    }
                }

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

        public override BlenderExportInfo GetBlenderExportInfo(IPackage package)
        {
            BlenderExportInfo ExportInfo = new BlenderExportInfo();

            if (package.Base is UAthenaPickaxeItemDefinition Pickaxe)
            {
                Pickaxe.DeepDeserialize();
                if (Pickaxe.WeaponDefinition is UFortWeaponMeleeItemDefinition WeaponDef)
                {
                    WeaponDef.DeepDeserialize();
                    USkeletalMesh WeaponTestMesh = WeaponDef.WeaponMeshOverride.Load<USkeletalMesh>();
                    ExportInfo.Models.Add(new ModelRef(WeaponTestMesh));
                }
                
            }

            return ExportInfo;
        }
    }
}
