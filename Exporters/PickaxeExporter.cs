using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using TModel.Modules;

namespace TModel.Exporters
{
    public class PickaxeExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { SpecificPaths = new string[] { "FortniteGame/Content/Athena/Items/Cosmetics/PickAxes/" } };

        public override ItemPreviewInfo GetPreviewInfo(IPackage package)
        {
            if (package.Base is UAthenaPickaxeItemDefinition Pickaxe)
                return Pickaxe.GetPreviewInfo();
            return null;
        }
    }
}
