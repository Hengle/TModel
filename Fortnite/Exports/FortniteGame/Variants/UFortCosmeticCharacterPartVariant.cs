using CUE4Parse.FN.Exports.FortniteGame.NoProperties;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;
using TModel.Modules;

namespace CUE4Parse.FN.Exports.FortniteGame
{
    public class UFortCosmeticCharacterPartVariant : UFortCosmeticVariant
    {
        public StyleOptionBase[]? PartOptions;

        public override void GetPreviewStyle(ExportPreviewSet style)
        {
            base.GetPreviewStyle(style);
            SetOptions(style, PartOptions);
        }

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            PartOptions = GetOrDefault<StyleOptionBase[]>(nameof(PartOptions));
        }
    }

    [StructFallback]
    public class StyleOptionBase
    {
        public FText VariantName;
        public FSoftObjectPath PreviewImage;

        public StyleOptionBase(FStructFallback fallback)
        {
            VariantName = fallback.GetOrDefault<FText>(nameof(VariantName));
            PreviewImage = fallback.GetOrDefault<FSoftObjectPath>(nameof(PreviewImage));
        }
    }
}