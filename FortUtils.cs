using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TModel.Exporters;
using TModel.Modules;

namespace TModel
{
    // Holds static exporters
    public static class FortUtils
    {
        public static CharacterExporter characterExporter { get; } = new CharacterExporter();

        public static Dictionary<FilterType, ExporterBase> Exporters { get; } = new()
        {
            [FilterType.Character] = characterExporter,
        };

        // Gets all paths that could be a possible valid export for the given type
        public static List<GameFile> GetPossibleFiles(FilterType type)
        {
            List<GameFile> Result = new();
            if (Exporters.TryGetValue(type, out ExporterBase Exporter))
            {
                foreach (var path in App.FileProvider.Files)
                {
                    if (Exporter.SearchTerm.CheckName(path.Key))
                    {
                        Result.Add(path.Value);
                    }
                }
            }
            else
                throw new Exception($"Unsupported type: {type}");
            return Result;
        }

        public static bool TryLoadItemPreview(FilterType type, GameFile gameFile, out Lazy<ItemPreviewInfo>? itemPreviewInfo)
        {
            itemPreviewInfo = null;
            if (App.FileProvider.TryLoadPackage(gameFile, out IPackage package))
            {
                if (Exporters.TryGetValue(type, out ExporterBase Exporter))
                {
                    itemPreviewInfo = Exporter.GetPreviewInfo(package);
                    return itemPreviewInfo != null;
                }
            };

            return false;
        }
    }

    public enum FilterType
    {
        Character,
        Backpack,
        Glider,
        Pickaxe
    }
}
