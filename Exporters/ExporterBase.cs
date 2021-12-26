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
        public abstract SearchTerm SearchTerm { get; }

        public abstract Lazy<ItemPreviewInfo>? GetPreviewInfo(IPackage package);
    }
}
