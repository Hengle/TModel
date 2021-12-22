using CUE4Parse.UE4.Assets.Exports.Material.Parameters;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Exports.Material
{
    public class UMaterialInstance : UMaterialInterface
    {
        public ResolvedObject? Parent;
        public bool bHasStaticPermutationResource;
        public FMaterialInstanceBasePropertyOverrides BasePropertyOverrides;
        public FStaticParameterSet? StaticParameters;
        public FStructFallback? CachedData;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            Parent = GetOrDefault<ResolvedObject>(nameof(Parent));
            bHasStaticPermutationResource = GetOrDefault<bool>("bHasStaticPermutationResource");
            BasePropertyOverrides = GetOrDefault<FMaterialInstanceBasePropertyOverrides>(nameof(BasePropertyOverrides));
            StaticParameters = GetOrDefault<FStaticParameterSet>(nameof(StaticParameters));

            var bSavedCachedData = FUE5MainStreamObjectVersion.Get(Ar) >= FUE5MainStreamObjectVersion.Type.MaterialSavedCachedData && Ar.ReadBoolean();
            if (bSavedCachedData)
            {
                CachedData = new FStructFallback(Ar, "MaterialInstanceCachedData");
            }

            if (bHasStaticPermutationResource)
            {
                if (Ar.Ver >= EUnrealEngineObjectUE4Version.PURGED_FMATERIAL_COMPILE_OUTPUTS)
                {
                    if (FRenderingObjectVersion.Get(Ar) < FRenderingObjectVersion.Type.MaterialAttributeLayerParameters)
                    {
                        StaticParameters = new FStaticParameterSet(Ar);
                    }

                    Ar.Position = validPos; // TODO This skips every data after the inline shader map data, find a way to properly skip it
                }
            }
            Ar.Position = validPos;
        }
    }

    [StructFallback]
    public class FStaticParameterSet
    {
        public FStaticSwitchParameter[] StaticSwitchParameters;
        public FStaticComponentMaskParameter[] StaticComponentMaskParameters;
        public FStaticTerrainLayerWeightParameter[] TerrainLayerWeightParameters;
        public FStaticMaterialLayersParameter[] MaterialLayersParameters;

        public FStaticParameterSet(FArchive Ar)
        {
            // static switch parameters
            StaticSwitchParameters = Ar.ReadArray(() => new FStaticSwitchParameter(Ar));
            // static component mask parameters
            StaticComponentMaskParameters = Ar.ReadArray(() => new FStaticComponentMaskParameter(Ar));
            // terrain layer weight parameters
            TerrainLayerWeightParameters = Ar.ReadArray(() => new FStaticTerrainLayerWeightParameter(Ar));

            if (FReleaseObjectVersion.Get(Ar) >= FReleaseObjectVersion.Type.MaterialLayersParameterSerializationRefactor)
                MaterialLayersParameters = Ar.ReadArray(() => new FStaticMaterialLayersParameter(Ar));
        }
    }
}