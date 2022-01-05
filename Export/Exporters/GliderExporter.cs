﻿using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using System.Collections.Generic;
using TModel.Modules;

namespace TModel.Export.Exporters
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

                List<ExportPreviewSet>? Styles = null;

                if (Glider.ItemVariants is FPackageIndex[] Variants && Variants.Length > 0)
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
                    Name = Glider.DisplayName,
                    Description = Glider.Description,
                    Package = package,
                    PreviewIcon = SmallImageRef,
                    Styles = Styles
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

        public override BlenderExportInfo GetBlenderExportInfo(IPackage package, int[]? styles = null)
        {
            BlenderExportInfo ExportInfo = new BlenderExportInfo();
            ExportInfo.Name = package.Name;
            if (package.Base is UAthenaGliderItemDefinition Glider)
            {
                Glider.DeepDeserialize();
                USkeletalMesh GliderMesh = Glider.SkeletalMesh.Load<USkeletalMesh>();
                ExportInfo.Models.Add(new ModelRef(GliderMesh));
            }

            return ExportInfo;
        }
    }
}
