using CUE4Parse.UE4.Assets.Exports.Material;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel.Export.Materials
{
    public class CMR_Prop : CMaterial
    {
        public CMR_Prop(UMaterialInstanceConstant material) : base(material)
        {
        }

        protected override void ReadParameters()
        {
            throw new NotImplementedException();
        }
    }
}
