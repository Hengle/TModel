using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using TModel.Modules;

namespace TModel.Exporters
{
    public class GliderExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/Gliders/" } };

        public override ItemPreviewInfo GetPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaGliderItemDefinition Glider)
                return Glider.GetPreviewInfo();
            return null;
        }
    }
}
