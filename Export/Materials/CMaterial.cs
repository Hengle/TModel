using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;
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
                if (texture != null)
                {
                    bool Successful = texture.Save(out string TexturePath);
                    Writer.Write((bool)(Successful));
                    if (Successful)
                    {
                        Writer.Write(TexturePath);
                    }
                }
            }
        }

        protected void TrySetTexture(string name, ref TextureRef value)
        {
            if (Textures.TryGetValue(name, out FPackageIndex texture))
                value = new TextureRef(texture.Load<UTexture2D>());
        }

        public static CMaterial CreateReader(UMaterialInstanceConstant material)
        {
            var Parent = material.Parent.Name.Text;
            switch (Parent)
            {
                default:
                    return new CMR_Default(material);
            }
        }

        public FSkeletalMaterial GetSkeletalMaterial() 
        {
            return new FSkeletalMaterial() { MaterialSlotName = Name, Material = new ResolvedLoadedObject(Material) };
        }

        protected abstract void ReadParameters();
    }
}
