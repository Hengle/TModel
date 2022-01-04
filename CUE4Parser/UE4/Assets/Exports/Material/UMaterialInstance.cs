using CUE4Parse.UE4.Assets.Exports.Material.Parameters;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CUE4Parse.UE4.Assets.Exports.Material
{
    public class UMaterialInstance : UMaterialInterface
    {
        public ResolvedObject? Parent;
        public bool bHasStaticPermutationResource;
        public FMaterialInstanceBasePropertyOverrides BasePropertyOverrides;
        public FStaticParameterSet? StaticParameters;
        public FStructFallback? CachedData;

        public Dictionary<string, bool> StaticSwitchParameterValues { get; } = new();
        public Dictionary<string, (bool R, bool G, bool B, bool A)> StaticComponentMaskParameterValues { get; } = new();
        public Dictionary<string, (int WeightmapIndex, bool bWeightBasedBlend)> TerrainLayerWeightParameterValues { get; } = new();
        public Dictionary<string, FMaterialLayersFunctions> MaterialLayersParameterValues { get; } = new();

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            Parent = GetOrDefault<ResolvedObject>(nameof(Parent));
            bHasStaticPermutationResource = GetOrDefault<bool>("bHasStaticPermutationResource");
            BasePropertyOverrides = GetOrDefault<FMaterialInstanceBasePropertyOverrides>(nameof(BasePropertyOverrides));
            StaticParameters = GetOrDefault<FStaticParameterSet>(nameof(StaticParameters));

            if (StaticParameters.StaticSwitchParameters != null)
                foreach (FStaticSwitchParameter staticSwitch in StaticParameters.StaticSwitchParameters)
                    StaticSwitchParameterValues.Add(staticSwitch.ParameterInfo.Name, staticSwitch.Value);

            if (StaticParameters.StaticComponentMaskParameters != null)
                foreach (FStaticComponentMaskParameter componentMask in StaticParameters.StaticComponentMaskParameters)
                    StaticComponentMaskParameterValues.Add(componentMask.ParameterInfo.Name, (componentMask.R, componentMask.G, componentMask.B, componentMask.A));


            // TODO: parse TerrainLayerWeightParameterValues and MaterialLayersParameterValues

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

#if READ_SHADER_MAPS
                    DeserializeInlineShaderMaps(Ar, LoadedMaterialResources);
#else
                    Ar.Position = validPos; // TODO This skips every data after the inline shader map data, find a way to properly skip it
#endif
                }
            }

#if !READ_SHADER_MAPS
            Ar.Position = validPos;
#endif
        }

        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);

            if (CachedData != null)
            {
                writer.WritePropertyName("CachedData");
                serializer.Serialize(writer, CachedData);
            }
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
            StaticSwitchParameters = Ar.ReadArray(() => new FStaticSwitchParameter(Ar));
            StaticComponentMaskParameters = Ar.ReadArray(() => new FStaticComponentMaskParameter(Ar));
            TerrainLayerWeightParameters = Ar.ReadArray(() => new FStaticTerrainLayerWeightParameter(Ar));

            if (FReleaseObjectVersion.Get(Ar) >= FReleaseObjectVersion.Type.MaterialLayersParameterSerializationRefactor)
            {
                MaterialLayersParameters = Ar.ReadArray(() => new FStaticMaterialLayersParameter(Ar));
            }
        }

        public FStaticParameterSet(FStructFallback fallback)
        {
            StaticSwitchParameters = fallback.GetOrDefault<FStaticSwitchParameter[]>(nameof(StaticSwitchParameters));
        }
    }
}