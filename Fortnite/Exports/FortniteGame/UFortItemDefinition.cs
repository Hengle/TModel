using CUE4Parse.FN.Enums.FortniteGame;
using CUE4Parse.FN.Structs.Engine;
using CUE4Parse.FN.Structs.GA;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.GameplayTags;
using CUE4Parse.UE4.Objects.UObject;
using System;
using TModel.Modules;

namespace CUE4Parse.FN.Exports.FortniteGame
{
    public class UFortItemDefinition : UObject
    {
        public Lazy<EFortRarity> Rarity;
        public Lazy<EFortItemType> ItemType;
        public Lazy<EFortItemType> PrimaryAssetIdItemTypeOverride;
        public Lazy<EFortInventoryFilter> FilterOverride;
        public Lazy<EFortItemTier> Tier;
        public Lazy<EFortItemTier> MaxTier;
        public Lazy<EFortTemplateAccess> Access;
        public Lazy<bool> bIsAccountItem;
        public Lazy<bool> bNeverPersisted;
        public Lazy<bool> bAllowMultipleStacks;
        public Lazy<bool> bAutoBalanceStacks;
        public Lazy<bool> bForceAutoPickup;
        public Lazy<bool> bInventorySizeLimited;
        public Lazy<FText?> ItemTypeNameOverride;
        public Lazy<FText?> DisplayName;
        public Lazy<FText?> ShortDescription;
        public Lazy<FText?> Description;
        public Lazy<FText?> DisplayNamePrefix;
        public Lazy<FText?> SearchTags;
        public Lazy<FName> GiftBoxGroupName;
        public Lazy<FGameplayTagContainer> GameplayTags;
        public Lazy<FGameplayTagContainer> AutomationTags;
        public Lazy<FGameplayTagContainer> SecondaryCategoryOverrideTags;
        public Lazy<FGameplayTagContainer> TertiaryCategoryOverrideTags;
        public Lazy<FScalableFloat?> MaxStackSize;
        public Lazy<FScalableFloat>? PurchaseItemLimit;
        public Lazy<float> FrontendPreviewScale;
        public Lazy<UClass?> TooltipClass;
        public Lazy<UFortTooltipDisplayStatsList?> StatList;
        public Lazy<FCurveTableRowHandle?> RatingLookup;
        public Lazy<object?> WidePreviewImage;
        public Lazy<FSoftObjectPath?> SmallPreviewImage;
        public Lazy<FSoftObjectPath?> LargePreviewImage;
        public Lazy<FSoftObjectPath> DisplayAssetPath;
        public Lazy<FDataTableRowHandle?> PopupDetailsTag;
        public Lazy<UFortItemSeriesDefinition?> Series;
        public Lazy<FVector> FrontendPreviewPivotOffset;
        public Lazy<FRotator> FrontendPreviewInitialRotation;
        public Lazy<UStaticMesh?> FrontendPreviewMeshOverride;
        public Lazy<USkeletalMesh?> FrontendPreviewSkeletalMeshOverride;

        public virtual ItemPreviewInfo? GetPreviewInfo()
        {
            UTexture2D SmallImage = SmallPreviewImage.Value?.Load<UTexture2D>();

            return new ItemPreviewInfo() { PreviewIcon = new TextureRef(SmallImage) };
        }

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            Rarity =                                new Lazy<EFortRarity>(() => GetOrDefault(nameof(Rarity), EFortRarity.Uncommon));
            ItemType =                              new Lazy<EFortItemType>(() => GetOrDefault<EFortItemType>(nameof(ItemType)));
            PrimaryAssetIdItemTypeOverride =        new Lazy<EFortItemType>(() => GetOrDefault<EFortItemType>(nameof(PrimaryAssetIdItemTypeOverride)));
            FilterOverride =                        new Lazy<EFortInventoryFilter>(() => GetOrDefault<EFortInventoryFilter>(nameof(FilterOverride)));
            Tier =                                  new Lazy<EFortItemTier>(() => GetOrDefault<EFortItemTier>(nameof(Tier)));
            MaxTier =                               new Lazy<EFortItemTier>(() => GetOrDefault<EFortItemTier>(nameof(MaxTier)));
            Access =                                new Lazy<EFortTemplateAccess>(() => GetOrDefault<EFortTemplateAccess>(nameof(Access)));
            bIsAccountItem =                        new Lazy<bool>(() => GetOrDefault<bool>(nameof(bIsAccountItem)));
            bNeverPersisted =                       new Lazy<bool>(() => GetOrDefault<bool>(nameof(bNeverPersisted)));
            bAllowMultipleStacks =                  new Lazy<bool>(() => GetOrDefault<bool>(nameof(bAllowMultipleStacks)));
            bAutoBalanceStacks =                    new Lazy<bool>(() => GetOrDefault<bool>(nameof(bAutoBalanceStacks)));
            bForceAutoPickup =                      new Lazy<bool>(() => GetOrDefault<bool>(nameof(bForceAutoPickup)));
            bInventorySizeLimited =                 new Lazy<bool>(() => GetOrDefault<bool>(nameof(bInventorySizeLimited)));
            ItemTypeNameOverride =                  new Lazy<FText?>(() => GetOrDefault<FText>(nameof(ItemTypeNameOverride)));
            DisplayName =                           new Lazy<FText?>(() => GetOrDefault<FText>(nameof(DisplayName)));
            ShortDescription =                      new Lazy<FText?>(() => GetOrDefault<FText>(nameof(ShortDescription)));
            Description =                           new Lazy<FText?>(() => GetOrDefault<FText>(nameof(Description)));
            DisplayNamePrefix =                     new Lazy<FText?>(() => GetOrDefault<FText>(nameof(DisplayNamePrefix)));
            SearchTags =                            new Lazy<FText?>(() => GetOrDefault<FText>(nameof(SearchTags)));
            GiftBoxGroupName =                      new Lazy<FName>(() => GetOrDefault<FName>(nameof(GiftBoxGroupName)));
            GameplayTags =                          new Lazy<FGameplayTagContainer>(() => GetOrDefault<FGameplayTagContainer>(nameof(GameplayTags)));
            AutomationTags =                        new Lazy<FGameplayTagContainer>(() => GetOrDefault<FGameplayTagContainer>(nameof(AutomationTags)));
            SecondaryCategoryOverrideTags =         new Lazy<FGameplayTagContainer>(() => GetOrDefault<FGameplayTagContainer>(nameof(SecondaryCategoryOverrideTags)));
            TertiaryCategoryOverrideTags =          new Lazy<FGameplayTagContainer>(() => GetOrDefault<FGameplayTagContainer>(nameof(TertiaryCategoryOverrideTags)));
            MaxStackSize =                          new Lazy<FScalableFloat>(() => GetOrDefault<FScalableFloat>(nameof(MaxStackSize)));
            PurchaseItemLimit =                     new Lazy<FScalableFloat>(() => GetOrDefault<FScalableFloat>(nameof(PurchaseItemLimit)));
            FrontendPreviewScale =                  new Lazy<float>(() => GetOrDefault<float>(nameof(FrontendPreviewScale)));
            TooltipClass =                          new Lazy<UClass>(() => GetOrDefault<UClass>(nameof(TooltipClass)));
            StatList =                              new Lazy<UFortTooltipDisplayStatsList>(() => GetOrDefault<UFortTooltipDisplayStatsList>(nameof(StatList)));
            RatingLookup =                          new Lazy<FCurveTableRowHandle>(() => GetOrDefault<FCurveTableRowHandle>(nameof(RatingLookup)));
            WidePreviewImage =                      new Lazy<object?>(() => GetOrDefault<object>(nameof(WidePreviewImage)));
            SmallPreviewImage =                     new Lazy<FSoftObjectPath?>(() => GetOrDefault<FSoftObjectPath?>(nameof(SmallPreviewImage)));
            LargePreviewImage =                     new Lazy<FSoftObjectPath?>(() => GetOrDefault<FSoftObjectPath?>(nameof(LargePreviewImage)));
            DisplayAssetPath =                      new Lazy<FSoftObjectPath>(() => GetOrDefault<FSoftObjectPath>(nameof(DisplayAssetPath)));
            PopupDetailsTag =                       new Lazy<FDataTableRowHandle>(() => GetOrDefault<FDataTableRowHandle>(nameof(PopupDetailsTag)));
            Series =                                new Lazy<UFortItemSeriesDefinition>(() => GetOrDefault<UFortItemSeriesDefinition>(nameof(Series)));
            FrontendPreviewPivotOffset =            new Lazy<FVector>(() => GetOrDefault<FVector>(nameof(FrontendPreviewPivotOffset)));
            FrontendPreviewInitialRotation =        new Lazy<FRotator>(() => GetOrDefault<FRotator>(nameof(FrontendPreviewInitialRotation)));
            FrontendPreviewMeshOverride =           new Lazy<UStaticMesh>(() => GetOrDefault<UStaticMesh>(nameof(FrontendPreviewMeshOverride)));
            FrontendPreviewSkeletalMeshOverride =   new Lazy<USkeletalMesh>(() => GetOrDefault<USkeletalMesh>(nameof(FrontendPreviewSkeletalMeshOverride)));
        }
    }
}