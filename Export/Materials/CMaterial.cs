using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.UObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel.Export.Materials
{
    // CMR stands for Custom Material Reader
    public abstract class CMaterial
    {
        protected UMaterialInstanceConstant Material;

        protected Dictionary<string, float> Scalars => Material.ScalarParameterValues;
        protected Dictionary<string, FPackageIndex> Textures => Material.TextureParameterValues;
        protected Dictionary<string, FLinearColor?> Vectors => Material.VectorParameterValues;


        protected TextureRef? Diffuse = null;

        public CMaterial(UMaterialInstanceConstant material)
        {
            Material = material;
        }

        public abstract void ReadParameters();
    }
}
