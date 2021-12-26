using CUE4Parse.FN.Exports.FortniteGame.NoProperties;
using CUE4Parse.UE4.Assets;
using TModel.Modules;

namespace TModel.Exporters
{
    public class BackpackExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/Backpacks/" } };

        public override ItemPreviewInfo GetPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaBackpackItemDefinition Backpack)
                return Backpack.GetPreviewInfo();
            return null;
        }
    }
}
