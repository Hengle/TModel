using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Objects.UObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel.Export.Materials
{
    // M_FN_Weapon_MASTER
    public class CMR_Default : CMaterial
    {
        public CMR_Default(UMaterialInstanceConstant material) : base(material)
        {

        }

        protected override void ReadParameters()
        {
            TryGetSetTex("Diffuse", ref Diffuse);
            TryGetSetTex("SpecularMasks", ref SpecularMasks);
            TryGetSetTex("Normals", ref Normals);
        }
    }
}
