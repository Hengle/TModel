using System;
using System.Collections.Generic;
using CUE4Parse.FN.Enums.FortniteGame;
using CUE4Parse.FN.Exports.FortniteGame.NoProperties;
using CUE4Parse.FN.Structs.FortniteGame;
using CUE4Parse.FN.Structs.GT;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using TModel.Modules;

namespace CUE4Parse.FN.Exports.FortniteGame
{
    public class UAthenaCharacterItemDefinition : UAthenaCosmeticItemDefinition
    {
        public Lazy<Dictionary<FName, UClass>>          RequestedDataStores = new();
        public Lazy<FSoftObjectPath[]?>                 BaseCharacterParts;
        public Lazy<UFortHeroType?>                     HeroDefinition;
        public Lazy<UAthenaBackpackItemDefinition?>     DefaultBackpack;
        public Lazy<UAthenaCosmeticItemDefinition[]?>   RequiredCosmeticItems;
        public Lazy<float>                              PreviewPawnScale;
        public Lazy<EFortCustomGender>                  Gender;
        public Lazy<FSoftObjectPath>                    FeedbackBank; // UFortFeedbackBank
        public Lazy<Dictionary<FGameplayTag, FAthenaCharacterTaggedPartsList>> TaggedPartsOverride = new();

        public override ItemPreviewInfo? GetPreviewInfo()
        {
            if (HeroDefinition.Value is null)
                return null;
            var SmallImagePath = HeroDefinition.Value.SmallPreviewImage.Value;
            UTexture2D SmallImage = SmallImagePath?.Load<UTexture2D>() ?? null;
            TextureRef SmallImageRef = new TextureRef(SmallImage);

            return new ItemPreviewCosmeticInfo() { PreviewIcon = SmallImageRef };
        }

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
#if false
            var dataStores = GetOrDefault(nameof(RequestedDataStores), new UScriptMap());
            foreach (var (key, value) in dataStores.Properties)
            {
                if (key?.GenericValue is FName name && value?.GenericValue is FPackageIndex i && i.TryLoad<UClass>(out var store))
                {
                    RequestedDataStores.Add(name, store);
                }
            }
#endif
            BaseCharacterParts =        new Lazy<FSoftObjectPath[]?>(() => GetOrDefault<FSoftObjectPath[]>(nameof(BaseCharacterParts)));
            HeroDefinition =            new Lazy<UFortHeroType?>(() => GetOrDefault<UFortHeroType>(nameof(HeroDefinition)));
            DefaultBackpack =           new Lazy<UAthenaBackpackItemDefinition?>(() => GetOrDefault<UAthenaBackpackItemDefinition>(nameof(DefaultBackpack)));
            RequiredCosmeticItems =     new Lazy<UAthenaCosmeticItemDefinition[]?>(() => GetOrDefault<UAthenaCosmeticItemDefinition[]>(nameof(RequiredCosmeticItems)));
            PreviewPawnScale =          new Lazy<float>(() => GetOrDefault<float>(nameof(PreviewPawnScale)));
            Gender =                    new Lazy<EFortCustomGender>(() => GetOrDefault<EFortCustomGender>(nameof(Gender)));
            FeedbackBank =              new Lazy<FSoftObjectPath>(() => GetOrDefault<FSoftObjectPath>(nameof(FeedbackBank)));

#if false
            var taggedParts = GetOrDefault(nameof(TaggedPartsOverride), new UScriptMap());
            foreach (var (key, value) in taggedParts.Properties)
            {
                if (key?.GenericValue is UScriptStruct { StructType: FStructFallback tag } && value?.GenericValue is UScriptStruct { StructType: FStructFallback parts })
                {
                    TaggedPartsOverride.Add(new FGameplayTag(tag), new FAthenaCharacterTaggedPartsList(parts));
                }
            }
#endif
        }
    }
}
