using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.UObject;

namespace CUE4Parse.UE4.Assets.Exports.Material
{
    public class UMaterialInstanceConstant : UMaterialInstance
    {
        public MaterialParameterDictionary<FTextureParameterValue> TextureParameterValues { get; } = new();
        public MaterialParameterDictionary<FScalarParameterValue> ScalarParameterValues { get; } = new();
        public MaterialParameterDictionary<FVectorParameterValue> VectorParameterValues { get; } = new();

        public new FMaterialInstanceBasePropertyOverrides? BasePropertyOverrides;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            try
            {
                base.Deserialize(Ar, validPos);
            }
            catch
            {

            }
            GetParameter(this);
        }

        public void GetParameter(UMaterialInstanceConstant Base)
        {
            if (Parent != null)
            {
                UMaterialInterface ParentMaterial = Parent.Load<UMaterialInterface>();
                if (ParentMaterial is UMaterialInstanceConstant contant)
                {
                    contant.GetParameter(Base);
                    Base.LoadParameters(contant);
                }
            }
        }

        // Takes the parameters from this material and applies them to 'material'
        public void LoadParameters(UMaterialInstanceConstant material)
        {
            FTextureParameterValue[] TempTextureParameterValues = GetOrDefault(nameof(material.TextureParameterValues), Array.Empty<FTextureParameterValue>());
            foreach (var item in TempTextureParameterValues)
                material.TextureParameterValues.AddParameter(item);

            FScalarParameterValue[] TempScalarParameterValues = GetOrDefault(nameof(material.ScalarParameterValues), Array.Empty<FScalarParameterValue>());
            foreach (var item in TempScalarParameterValues)
                material.ScalarParameterValues.AddParameter(item);

            FVectorParameterValue[] TempVectorParameterValues = GetOrDefault(nameof(material.VectorParameterValues), Array.Empty<FVectorParameterValue>());
            foreach (var item in TempVectorParameterValues)
                material.VectorParameterValues.AddParameter(item);
            material.BasePropertyOverrides = GetOrDefault<FMaterialInstanceBasePropertyOverrides>(nameof(material.BasePropertyOverrides));
        }
    }

    [StructFallback]
    public class BaseParameterValue : IUStruct
    {
        public readonly FName ParameterName;

        public BaseParameterValue(FStructFallback fallback)
        {
            ParameterName = fallback.GetOrDefault<FName>(nameof(ParameterName));
        }

        public BaseParameterValue() { }
    }

    public class MaterialParameterDictionary<T> : Dictionary<string, T>
    {
        // Adds the parameter if it doesnt already exist
        public void AddParameter(T a)
        {
            BaseParameterValue Name = a as BaseParameterValue;
            if (!TryGetValue(Name.ParameterName, out _))
            {
                this[Name.ParameterName] = a;
            }
        }
    }
}
