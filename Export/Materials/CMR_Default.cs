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
    // M_FN_Weapon_MASTER
    public class CMR_Default : CMaterial
    {
        public CMR_Default(UMaterialInstanceConstant material) : base(material)
        {

        }

        protected override void ReadParameters()
        {
            TrySetTexture("Diffuse", ref Diffuse);
            TrySetTexture("SpecularMasks", ref SpecularMasks);
            TrySetTexture("Normals", ref Normals);
            TrySetTexture("Emissive", ref Emissive);
            TrySetTexture("SkinFX_Mask", ref SkinFX_Mask);
            TrySetTexture("M", ref Metallic);

            // Gradient maps for Rox skin
            if (TrySetSwitch("useGmapGradientLayers", ref useGmapGradientLayers))
                if (useGmapGradientLayers = TrySetTexture("Layer Mask", ref LayerMask))
                {
                    TrySetTexture("Layer1_Gradient", ref Layer1_Gradient);
                    TrySetTexture("Layer2_Gradient", ref Layer2_Gradient);
                    TrySetTexture("Layer3_Gradient", ref Layer3_Gradient);
                    TrySetTexture("Layer4_Gradient", ref Layer4_Gradient);
                    TrySetTexture("Layer5_Gradient", ref Layer5_Gradient);
                }


            TrySetVector("Skin Boost Color And Exponent", ref SkinBoostColor);
        }
    }
}
