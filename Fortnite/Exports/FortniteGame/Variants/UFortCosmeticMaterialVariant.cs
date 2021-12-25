using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Objects.UObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUE4Parse.FN.Exports.FortniteGame
{
    class UFortCosmeticMaterialVariant : UFortCosmeticVariant
    {
        public MaterialVariantDef[] MaterialOptions;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            MaterialOptions = GetOrDefault<MaterialVariantDef[]>(nameof(MaterialOptions), Array.Empty<MaterialVariantDef>());
        }
    }

    [StructFallback]
    public class MaterialVariantDef
    {
        public MaterialVariant[] VariantMaterials;

        public MaterialVariantDef(FStructFallback fallback)
        {
            VariantMaterials = fallback.GetOrDefault<MaterialVariant[]>(nameof(VariantMaterials));
        }
    }

    [StructFallback]
    public class MaterialVariant
    {
        public FSoftObjectPath MaterialToSwap;
        public int MaterialOverrideIndex;
        public FSoftObjectPath OverrideMaterial;

        public MaterialVariant(FStructFallback fallback)
        {
            MaterialToSwap = fallback.GetOrDefault<FSoftObjectPath>(nameof(MaterialToSwap));
            MaterialOverrideIndex = fallback.GetOrDefault<int>(nameof(MaterialOverrideIndex));
            OverrideMaterial = fallback.GetOrDefault<FSoftObjectPath>(nameof(OverrideMaterial));
        }
    }

}
