using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.UObject;
using Serilog;
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

        public UMaterialInstanceConstant Material { get; private set; }

        protected Dictionary<string, float> Scalars => Material.ScalarParameterValues;
        protected Dictionary<string, FPackageIndex> Textures => Material.TextureParameterValues;
        protected Dictionary<string, FLinearColor> Vectors => Material.VectorParameterValues;
        protected Dictionary<string, bool> Switches => Material.StaticSwitchParameterValues;

        protected FLinearColor SkinBoostColor = FLinearColor.MAX;

        protected TextureRef? Diffuse = null;
        protected TextureRef? SpecularMasks = null;
        protected TextureRef? Normals = null;
        protected TextureRef? Emissive = null;
        protected TextureRef? Metallic = null;
        protected TextureRef? SkinFX_Mask = null;

        // Rox character colors
        protected TextureRef? LayerMask = null;
        // Layer gradients
        protected TextureRef? Layer1_Gradient = null; // SkinFX_Mask - Red channel
        protected TextureRef? Layer2_Gradient = null;
        protected TextureRef? Layer3_Gradient = null;
        protected TextureRef? Layer4_Gradient = null;
        protected TextureRef? Layer5_Gradient = null;

        protected bool useGmapGradientLayers = false;

        protected CMaterial(UMaterialInstanceConstant material)
        {
            Log.Information("       Material: " + material.Name);
            Material = material;
        }

        public void SaveAndWriteBinary(CBinaryWriter Writer)
        {
            Log.Information("Writing material: " + Material.Name);

            ReadParameters();

            Writer.Write(Name);

            WriteTexture(Diffuse);
            WriteTexture(SpecularMasks);
            WriteTexture(Normals);
            WriteTexture(Emissive);
            WriteTexture(SkinFX_Mask);
            WriteTexture(Metallic);

            Writer.Write(useGmapGradientLayers);
            if (useGmapGradientLayers)
            {
                WriteTexture(LayerMask);
                WriteTexture(Layer1_Gradient);
                WriteTexture(Layer2_Gradient);
                WriteTexture(Layer3_Gradient);
                WriteTexture(Layer4_Gradient);
                WriteTexture(Layer5_Gradient);
            }

            WriteVector(SkinBoostColor);

            void WriteTexture(TextureRef texture)
            {
                // Parameter could not be found
                if (texture != null)
                {
                    string TexturePath = Path.Combine(Preferences.ExportsPath, texture.Name + ".png");
                    bool Successful = texture.Save(TexturePath);
                    Writer.Write(Successful);
                    if (Successful)
                    {
                        Writer.Write(TexturePath);
                    }
                }
                else
                {
                    Writer.Write(false);
                }
            }

            void WriteVector(FLinearColor color)
            {
                Writer.Write(color.R);
                Writer.Write(color.G);
                Writer.Write(color.B);
                Writer.Write(color.A);
            }
        }

        protected bool TrySetVector(string name, ref FLinearColor value)
        {
            if (Vectors.TryGetValue(name, out FLinearColor color))
            {
                value = color;
                return true;
            }
            return false;
        }

        protected bool TrySetTexture(string name, ref TextureRef value)
        {
            if (Textures.TryGetValue(name, out FPackageIndex texture))
            {
                value = new TextureRef(texture.Load<UTexture2D>());
                return true;
            }
            return false;
        }

        protected bool TrySetSwitch(string name, ref bool value)
        {
            if (Switches.TryGetValue(name, out bool boolean)) 
            {
                value = boolean;
                return true;
            }
            return false;
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

        public FStaticMaterial GetStaticMaterial()
        {
            return new FStaticMaterial() { MaterialSlotName = Name, MaterialInterface = new ResolvedLoadedObject(Material) };
        }

        protected abstract void ReadParameters();
    }
}
