using CUE4Parse.FN.Exports.FortniteGame.NoProperties;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Objects.UObject;

namespace CUE4Parse.FN.Exports.FortniteGame
{
    public class UFortCosmeticCharacterPartVariant : UFortCosmeticVariant
    {
        public PartVariantDef[]? PartOptions;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            PartOptions = GetOrDefault<PartVariantDef[]>(nameof(PartOptions));
        }
    }

    [StructFallback]
    public class PartVariantDef : MaterialVariantDef
    {
        public FSoftObjectPath[] VariantParts;

        public PartVariantDef(FStructFallback fallback) : base(fallback)
        {
            VariantParts = fallback.GetOrDefault<FSoftObjectPath[]>(nameof(VariantParts));
        }
    }
}