using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse_Conversion.Meshes;
using System.Collections.Generic;
using System.IO;
using TModel.Export.Materials;

namespace TModel.Export
{
    public class ModelRef
    {
        USkeletalMesh? SkeletalMesh;

        UStaticMesh? StaticMesh;

        public List<CMaterial> Materials = new List<CMaterial>();

        public ModelRef(USkeletalMesh skeletalMesh)
        {
            SkeletalMesh = skeletalMesh;
            foreach (var item in SkeletalMesh.Materials)
            {
                UMaterialInterface Material = item.Material.Load<UMaterialInterface>();
                if (Material is UMaterialInstanceConstant InstanceConstant)
                {
                    Materials.Add(CMaterial.CreateReader(InstanceConstant));
                }
            }
        }

        public ModelRef(UStaticMesh staticMesh)
        {
            StaticMesh = staticMesh;
        }

        // This saves the model as a .psk(x) to the Storage Folder defined in Preferences
        public void SaveMesh(CBinaryWriter writer)
        {
            MeshExporter meshExporter = null;
            if (SkeletalMesh is USkeletalMesh skeletalMesh)
            {
                meshExporter = new MeshExporter(skeletalMesh);
            }
            else if (StaticMesh is UStaticMesh staticMesh)
            {
                meshExporter = new MeshExporter(staticMesh);
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
