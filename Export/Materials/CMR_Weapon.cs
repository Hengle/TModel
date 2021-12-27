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
    public class CMR_Weapon : CMaterial
    {
        public CMR_Weapon(UMaterialInstanceConstant material) : base(material)
        {

        }

        protected override void ReadParameters()
        {
            if (Textures.TryGetValue("Diffuse", out FPackageIndex diffuse))
            {
                Diffuse = new TextureRef(diffuse);
            }
        }
    }
}
