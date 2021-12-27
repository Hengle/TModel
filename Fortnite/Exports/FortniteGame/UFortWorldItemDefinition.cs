using CUE4Parse.FN.Enums.FortniteGame;
using CUE4Parse.FN.Structs.Engine;
using CUE4Parse.FN.Structs.FortniteGame;
using CUE4Parse.FN.Structs.GA;
using CUE4Parse.FN.Structs.GT;
using CUE4Parse.FN.Structs.SlateCore;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.GameplayTags;
using CUE4Parse.UE4.Objects.UObject;
using System;

namespace CUE4Parse.FN.Exports.FortniteGame
{
    public class UFortWorldItemDefinition : UFortItemDefinition
    {
        public Lazy<FGameplayTagContainer> RequiredEquipTags;
        public Lazy<FFortPickupRestrictionLists[]?> PickupRestrictionListEntry;
        public Lazy<EWorldItemDropBehavior> DropBehavior;
        public Lazy<bool> bIgnoreRespawningForDroppingAsPickup;
        public Lazy<bool> bCanAutoEquipByClass;
        public Lazy<bool> bPersistInInventoryWhenFinalStackEmpty;
        public Lazy<bool> bSupportsQuickbarFocus;
        public Lazy<bool> bSupportsQuickbarFocusForGamepadOnly;
        public Lazy<bool> bShouldActivateWhenFocused;
        public Lazy<bool> bForceFocusWhenAdded;
        public Lazy<bool> bForceIntoOverflow;
        public Lazy<bool> bForceStayInOverflow;
        public Lazy<bool> bDropCurrentItemOnOverflow;
        public Lazy<bool> bShouldShowItemToast;
        public Lazy<bool> bShowDirectionalArrowWhenFarOff;
        public Lazy<bool> bCanBeDropped;
        public Lazy<bool> bCanBeReplacedByPickup;
        public Lazy<bool> bItemCanBeStolen;
        public Lazy<bool> bCanBeDepositedInStorageVault;
        public Lazy<bool> bItemHasDurability;
        public Lazy<bool> bAllowedToBeLockedInInventory;
        public Lazy<bool> bOverridePickupMeshTransform;
        public Lazy<bool> bAlwaysCountForCollectionQuest;
        public Lazy<bool> bDropOnDeath;
        public Lazy<bool> bDropOnLogout;
        public Lazy<bool> bDropOnDBNO;
        public Lazy<bool> bDoesNotNeedSourceSchematic;
        public Lazy<bool> bUsesGoverningTags;
        public Lazy<int> DropCount;
        public Lazy<float> MiniMapViewableDistance;
        public Lazy<FSlateBrush?> MiniMapIconBrush;
        public Lazy<FText?> OwnerPickupText;
        public Lazy<FDataTableCategoryHandle?> LootLevelData;
        public Lazy<FTransform> PickupMeshTransform;
        public Lazy<bool> bIsPickupASpecialActor;
        public Lazy<FGameplayTag?> SpecialActorPickupTag;
        public Lazy<FSpecialActorSingleStatData[]?> SpecialActorPickupStatList;
        public Lazy<FName> PickupSpecialActorUniqueID;
        public Lazy<FSlateBrush?> PickupMinimapIconBrush;
        public Lazy<FVector2D> PickupMinimapIconScale;
        public Lazy<FSlateBrush?> PickupCompassIconBrush;
        public Lazy<FVector2D> PickupCompassIconScale;
        public Lazy<FScalableFloat?> PickupDespawnTime;
        public Lazy<FScalableFloat?> InStormPickupDespawnTime;
        public Lazy<FScalableFloat?> NetworkCullDistanceOverride;
        public Lazy<FSoftObjectPath> PickupStaticMesh; // UStaticMesh
        public Lazy<FSoftObjectPath> PickupSkeletalMesh; // USkeletalMesh
        public Lazy<FSoftObjectPath> PickupEffectOverride; // UClass
        public Lazy<FSoftObjectPath> PickupSound; // USoundBase
        public Lazy<FSoftObjectPath> PickupByNearbyPawnSound; // USoundBase
        public Lazy<FSoftObjectPath> DropSound; // USoundBase
        public Lazy<FSoftObjectPath> DroppedLoopSound; // USoundBase
        public Lazy<FSoftObjectPath> LandedSound; // USoundBase
        public Lazy<FDataTableRowHandle?> DisassembleRecipe;
        public Lazy<float> DisassembleDurabilityDegradeMinLootPercent;
        public Lazy<float> DisassembleDurabilityDegradeMaxLootPercent;
        public Lazy<int> PreferredQuickbarSlot;
        public Lazy<int> MinLevel;
        public Lazy<int> MaxLevel;
        public Lazy<int> NumberOfSlotsToTake;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
#if false
            RequiredEquipTags = GetOrDefault<FGameplayTagContainer>(nameof(RequiredEquipTags));
            PickupRestrictionListEntry = GetOrDefault<FFortPickupRestrictionLists[]>(nameof(PickupRestrictionListEntry));
            DropBehavior = GetOrDefault<EWorldItemDropBehavior>(nameof(DropBehavior));
            bIgnoreRespawningForDroppingAsPickup = GetOrDefault<bool>(nameof(bIgnoreRespawningForDroppingAsPickup));
            bCanAutoEquipByClass = GetOrDefault<bool>(nameof(bCanAutoEquipByClass));
            bPersistInInventoryWhenFinalStackEmpty = GetOrDefault<bool>(nameof(bPersistInInventoryWhenFinalStackEmpty));
            bSupportsQuickbarFocus = GetOrDefault<bool>(nameof(bSupportsQuickbarFocus));
            bSupportsQuickbarFocusForGamepadOnly = GetOrDefault<bool>(nameof(bSupportsQuickbarFocusForGamepadOnly));
            bShouldActivateWhenFocused = GetOrDefault<bool>(nameof(bShouldActivateWhenFocused));
            bForceFocusWhenAdded = GetOrDefault<bool>(nameof(bForceFocusWhenAdded));
            bForceIntoOverflow = GetOrDefault<bool>(nameof(bForceIntoOverflow));
            bForceStayInOverflow = GetOrDefault<bool>(nameof(bForceStayInOverflow));
            bDropCurrentItemOnOverflow = GetOrDefault<bool>(nameof(bDropCurrentItemOnOverflow));
            bShouldShowItemToast = GetOrDefault<bool>(nameof(bShouldShowItemToast));
            bShowDirectionalArrowWhenFarOff = GetOrDefault<bool>(nameof(bShowDirectionalArrowWhenFarOff));
            bCanBeDropped = GetOrDefault<bool>(nameof(bCanBeDropped));
            bCanBeReplacedByPickup = GetOrDefault<bool>(nameof(bCanBeReplacedByPickup));
            bItemCanBeStolen = GetOrDefault<bool>(nameof(bItemCanBeStolen));
            bCanBeDepositedInStorageVault = GetOrDefault<bool>(nameof(bCanBeDepositedInStorageVault));
            bItemHasDurability = GetOrDefault<bool>(nameof(bItemHasDurability));
            bAllowedToBeLockedInInventory = GetOrDefault<bool>(nameof(bAllowedToBeLockedInInventory));
            bOverridePickupMeshTransform = GetOrDefault<bool>(nameof(bOverridePickupMeshTransform));
            bAlwaysCountForCollectionQuest = GetOrDefault<bool>(nameof(bAlwaysCountForCollectionQuest));
            bDropOnDeath = GetOrDefault<bool>(nameof(bDropOnDeath));
            bDropOnLogout = GetOrDefault<bool>(nameof(bDropOnLogout));
            bDropOnDBNO = GetOrDefault<bool>(nameof(bDropOnDBNO));
            bDoesNotNeedSourceSchematic = GetOrDefault<bool>(nameof(bDoesNotNeedSourceSchematic));
            bUsesGoverningTags = GetOrDefault<bool>(nameof(bUsesGoverningTags));
            DropCount = GetOrDefault<int>(nameof(DropCount));
            MiniMapViewableDistance = GetOrDefault<float>(nameof(MiniMapViewableDistance));
            MiniMapIconBrush = GetOrDefault<FSlateBrush>(nameof(MiniMapIconBrush));
            OwnerPickupText = GetOrDefault<FText>(nameof(OwnerPickupText));
            LootLevelData = GetOrDefault<FDataTableCategoryHandle>(nameof(LootLevelData));
            PickupMeshTransform = GetOrDefault<FTransform>(nameof(PickupMeshTransform));
            bIsPickupASpecialActor = GetOrDefault<bool>(nameof(bIsPickupASpecialActor));
            SpecialActorPickupTag = GetOrDefault<FGameplayTag>(nameof(SpecialActorPickupTag));
            SpecialActorPickupStatList = GetOrDefault<FSpecialActorSingleStatData[]>(nameof(SpecialActorPickupStatList));
            PickupSpecialActorUniqueID = GetOrDefault<FName>(nameof(PickupSpecialActorUniqueID));
            PickupMinimapIconBrush = GetOrDefault<FSlateBrush>(nameof(PickupMinimapIconBrush));
            PickupMinimapIconScale = GetOrDefault<FVector2D>(nameof(PickupMinimapIconScale));
            PickupCompassIconBrush = GetOrDefault<FSlateBrush>(nameof(PickupCompassIconBrush));
            PickupCompassIconScale = GetOrDefault<FVector2D>(nameof(PickupCompassIconScale));
            PickupDespawnTime = GetOrDefault<FScalableFloat>(nameof(PickupDespawnTime));
            InStormPickupDespawnTime = GetOrDefault<FScalableFloat>(nameof(InStormPickupDespawnTime));
            NetworkCullDistanceOverride = GetOrDefault<FScalableFloat>(nameof(NetworkCullDistanceOverride));
            PickupStaticMesh = GetOrDefault<FSoftObjectPath>(nameof(PickupStaticMesh));
            PickupSkeletalMesh = GetOrDefault<FSoftObjectPath>(nameof(PickupSkeletalMesh));
            PickupEffectOverride = GetOrDefault<FSoftObjectPath>(nameof(PickupEffectOverride));
            PickupSound = GetOrDefault<FSoftObjectPath>(nameof(PickupSound));
            PickupByNearbyPawnSound = GetOrDefault<FSoftObjectPath>(nameof(PickupByNearbyPawnSound));
            DropSound = GetOrDefault<FSoftObjectPath>(nameof(DropSound));
            DroppedLoopSound = GetOrDefault<FSoftObjectPath>(nameof(DroppedLoopSound));
            LandedSound = GetOrDefault<FSoftObjectPath>(nameof(LandedSound));
            DisassembleRecipe = GetOrDefault<FDataTableRowHandle>(nameof(DisassembleRecipe));
            DisassembleDurabilityDegradeMinLootPercent = GetOrDefault<float>(nameof(DisassembleDurabilityDegradeMinLootPercent));
            DisassembleDurabilityDegradeMaxLootPercent = GetOrDefault<float>(nameof(DisassembleDurabilityDegradeMaxLootPercent));
            PreferredQuickbarSlot = GetOrDefault<int>(nameof(PreferredQuickbarSlot));
            MinLevel = GetOrDefault<int>(nameof(MinLevel));
            MaxLevel = GetOrDefault<int>(nameof(MaxLevel));
            NumberOfSlotsToTake = GetOrDefault<int>(nameof(NumberOfSlotsToTake));
#endif
        }
    }
}