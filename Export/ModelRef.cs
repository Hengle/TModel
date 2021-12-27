using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse_Conversion.Meshes;
using System.IO;

namespace TModel.Export
{
    public class ModelRef
    {
        USkeletalMesh? SkeletalMesh;

        UStaticMesh? StaticMesh;

        public ModelRef(USkeletalMesh skeletalMesh)
        {
            SkeletalMesh = skeletalMesh;
        }

        public ModelRef(UStaticMesh staticMesh)
        {
            StaticMesh = staticMesh;
        }

        // This saves the model as a .psk(x) to the Storage Folder defined in Preferences
        public string Save()
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
                string ExportsPath = Path.Combine(Preferences.StorageFolder, "Exports");
                Directory.CreateDirectory(ExportsPath);
                meshExporter.TryWriteToDir(new DirectoryInfo(ExportsPath), out string SaveName);
                return Path.Combine(SaveName);
            }

            return null;
        }
    }
}
