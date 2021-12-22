using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.UObject;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Exports.Material
{
    [StructFallback]
    public class FVectorParameterValue : BaseParameterValue
    {
        [JsonIgnore]
        public string Name => (!ParameterName.IsNone ? ParameterName : ParameterInfo.Name).Text;
        public readonly FMaterialParameterInfo ParameterInfo;
        public readonly FLinearColor? ParameterValue;
        public readonly FGuid ExpressionGUID;

        public FVectorParameterValue(FStructFallback fallback) : base(fallback)
        {
            ParameterInfo = fallback.GetOrDefault<FMaterialParameterInfo>(nameof(ParameterInfo));
            ParameterValue = fallback.GetOrDefault<FLinearColor>(nameof(ParameterValue));
            ExpressionGUID = fallback.GetOrDefault<FGuid>(nameof(ExpressionGUID));
        }

        public FVectorParameterValue(FAssetArchive Ar)
        {
            ParameterInfo = new FMaterialParameterInfo(Ar);
            ParameterValue = Ar.Read<FLinearColor>();
            ExpressionGUID = Ar.Read<FGuid>();
        }

        public override string ToString() => $"{Name}: {ParameterValue}";
    }
}
