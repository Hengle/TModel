using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TModel.Export.Materials;

namespace TModel.Export
{
    public class BlenderExportInfo
    {
        public List<ModelRef> Models { get; } = new List<ModelRef>();


        // Returns list of all materials of all Models combined excluding duplicates.
        // (Some models may use the same materials so it doesnt need to load the same one
        // more than once)
        public List<CMaterial> GetMaterials()
        {
            List<CMaterial> Final = new List<CMaterial>();
            foreach (var model in Models)
            {
                foreach (var material in model.Materials)
                {
                    foreach (var item in Final)
                    {
                        if (item.Name == material.Name)
                        {
                            break;
                        }
                    }
                    Final.Add(material);
                }
            }
            return Final;
        }

        public void Save()
        {
            string BlenderDataPath = Path.Combine(Preferences.StorageFolder, "BlenderData.export");
            CBinaryWriter Writer = new CBinaryWriter(File.Open(BlenderDataPath, FileMode.OpenOrCreate, FileAccess.Write));

            Writer.Write((byte)Models.Count);
            foreach (var model in Models)
            {
                model.SaveMesh(Writer);
            }

            List<CMaterial> Materials = GetMaterials();
            Writer.Write((byte)Materials.Count);
            foreach (var material in Materials)
            {
                material.SaveAndWriteBinary(Writer);
            }

            Writer.Close();
        }
    }
}
