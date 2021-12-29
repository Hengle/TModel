using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TModel.Fortnite.Exports.FortniteGame;
using TModel.Modules;

namespace TModel.Export.Exporters
{
    public class PlaysetExporter : ExporterBase
    {
        public override SearchTerm SearchTerm { get; } = new SearchTerm() { SpecificPaths = new string[] 
        {
            "FortniteGame/Content/Playsets/"
        }};

        public override ExportPreviewInfo GetExportPreviewInfo(IPackage package)
        {
            throw new NotImplementedException();
        }

        public override ItemTileInfo GetTileInfo(IPackage package)
        {
            if (package.Base is UFortPlaysetItemDefinition Playset)
                return Playset.GetPreviewInfo();
            return null;
        }
    }
}
