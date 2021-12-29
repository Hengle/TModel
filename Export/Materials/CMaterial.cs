using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.UObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel.Export.Materials
{
    // CMR stands for Custom Material Reader
    public abstract class CMaterial
    {
        public string Name => Material.Name;

        protected UMaterialInstanceConstant Material;

        protected Dictionary<string, float> Scalars => Material.ScalarParameterValues;
        protected Dictionary<string, FPackageIndex> Textures => Material.TextureParameterValues;
        protected Dictionary<string, FLinearColor?> Vectors => Material.VectorParameterValues;


        protected TextureRef? Diffuse = null;
        protected TextureRef? SpecularMasks = null;
        protected TextureRef? Normals = null;

        protected CMaterial(UMaterialInstanceConstant material)
        {
            Material = material;
        }

        public void SaveAndWriteBinary(CBinaryWriter Writer)
        {
            ReadParameters();

            Writer.Write(Name);

            WriteTexture(Diffuse);
            WriteTexture(SpecularMasks);
            WriteTexture(Normals);

            void WriteTexture(TextureRef texture)
            {
                Writer.Write((bool)(texture != null));
                if (texture != null)
                {
                    texture.Save(out string TexturePath);
                    Writer.Write(TexturePath);
                }
            }
        }

        protected void TryGetSetTex(string name, ref TextureRef value)
        {
            if (Textures.TryGetValue(name, out FPackageIndex texture))
                value = new TextureRef(texture);
        }

        public static CMaterial CreateReader(UMaterialInstanceConstant material)
        {
            var Parent = material.Parent.Name.Text;
            switch (Parent)
            {
                default:
#if NOTIFY_MISSING_MATERIAL_TYPE
                    throw new Exception($"Material type '{Parent}' is unsupported.");
#else
                    return new CMR_Default(material);
#endif
            }
        }

        protected abstract void ReadParameters();
    }
}
