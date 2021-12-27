using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TModel.Modules;

namespace TModel.Exporters
{
    // Base class for exporting Fortnite cosmetics, playsets and props.
    public abstract class ExporterBase
    {
        public bool bHasGameFiles => GameFiles != null && GameFiles.Count > 0;

        public bool bHasLoadedAllPreviews => GameFiles.Count == LoadedPreviews.Count;

        public List<GameFile> GameFiles { set; get; }

        public List<ItemTileInfo> LoadedPreviews { get; } = new List<ItemTileInfo>();

        public abstract SearchTerm SearchTerm { get; }

        public abstract ItemTileInfo GetTileInfo(IPackage package);

        public abstract ExportPreviewInfo GetExportPreviewInfo(IPackage package);
    }
}
