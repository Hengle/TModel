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

        public List<ItemPreviewInfo> LoadedPreviews { get; } = new List<ItemPreviewInfo>();

        public abstract SearchTerm SearchTerm { get; }

        public abstract ItemPreviewInfo GetPreviewInfo(IPackage package);
    }
}
