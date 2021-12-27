using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel.Export
{
    public class BlenderExportInfo
    {
        public List<ModelRef> Models { get; } = new List<ModelRef>();

        public void Save()
        {
            List<string> SavePaths = new List<string>();

            foreach (var model in Models)
            {
                SavePaths.Add(model.Save());
            }

            string BlenderDataPath = Path.Combine(Preferences.StorageFolder, "BlenderData.export");

            CBinaryWriter Writer = new CBinaryWriter(File.Open(BlenderDataPath, FileMode.OpenOrCreate, FileAccess.Write));

            Writer.Write((byte)SavePaths.Count);

            foreach (string path in SavePaths)
            {
                Writer.Write(path);
            }

            Writer.Close();
        }
    }
}
