using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse_Conversion.Meshes;
using Serilog;
using System.Collections.Generic;
using System.IO;
using TModel.Export.Materials;

namespace TModel.Export
{
    public class ModelRef
    {
        USkeletalMesh? SkeletalMesh;

        UStaticMesh? StaticMesh;

        public List<CMaterial> Materials { get; } = new List<CMaterial>();

        public ModelRef(USkeletalMesh skeletalMesh)
        {
            Log.Information("Skeletal Mesh: " + skeletalMesh.Name);
            SkeletalMesh = skeletalMesh;
            if (SkeletalMesh.Materials is not null)
            {
                foreach (var item in SkeletalMesh.Materials)
                {
                    if (item.Material != null)
                    {
                        UMaterialInterface Material = item.Material.Load<UMaterialInterface>();
                        if (Material is UMaterialInstanceConstant InstanceConstant)
                        {
                            Materials.Add(CMaterial.CreateReader(InstanceConstant));
                        }
                    }
                    else
                    {
                        Log.Warning($"Material is null | Slot Name: {item.MaterialSlotName.PlainText ?? "NULL"}");
                    }
                }
            }
        }

        public ModelRef(UStaticMesh staticMesh)
        {
            Log.Information("Static Mesh: " + staticMesh.Name);
            StaticMesh = staticMesh;
            if (staticMesh is not null)
            {
                foreach (FStaticMaterial material in staticMesh.StaticMaterials)
                {
                    UMaterialInterface LoadedMaterial = material.MaterialInterface.Load<UMaterialInterface>();
                    if (LoadedMaterial is UMaterialInstanceConstant InstanceConstant)
                    {
                        Materials.Add(CMaterial.CreateReader(InstanceConstant));
                    }
                }
            }
        }

        // This saves the model as a .psk(x) to the Storage Folder defined in Preferences
        public void SaveMesh(CBinaryWriter writer)
        {
            Log.Information("Wring Mesh: " + SkeletalMesh?.Name ?? StaticMesh.Name);

            MeshExporter meshExporter = null;
            if (SkeletalMesh is USkeletalMesh skeletalMesh)
            {
                for (int i = 0; i < Materials.Count; i++)
                {
                    skeletalMesh.Materials[i] = Materials[i].GetSkeletalMaterial();
                }
                meshExporter = new MeshExporter(skeletalMesh);
            }
            else if (StaticMesh is UStaticMesh staticMesh)
            {
                meshExporter = new MeshExporter(staticMesh);
                for (int i = 0; i < Materials.Count; i++)
                {
                    staticMesh.StaticMaterials[i] = Materials[i].GetStaticMaterial();
                    staticMesh.Materials[i] = new ResolvedLoadedObject(Materials[i].Material);
                }
            }
            else
            {
#if DEBUG
                throw new System.Exception("Failed to save mesh");
#endif
            }

            if (meshExporter != null)
            {
                Directory.CreateDirectory(Preferences.ExportsPath);
                meshExporter.TryWriteToDir(new DirectoryInfo(Preferences.ExportsPath), out string SaveName);
                writer.Write(SaveName);
            }
        }
    }
}
