using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
using TModel.Exporters;
using TModel.Modules;

namespace TModel
{
    // Holds static exporters
    public static class FortUtils
    {
        public static CharacterExporter characterExporter { get; } = new CharacterExporter();
        public static BackpackExporter backpackExporter { get; } = new BackpackExporter();
        public static GliderExporter gliderExporter { get; } = new GliderExporter();
        public static PickaxeExporter pickaxeExporter { get; } = new PickaxeExporter();

        public static Dictionary<EItemFilterType, ExporterBase> Exporters { get; } = new()
        {
            [EItemFilterType.Character] = characterExporter,
            [EItemFilterType.Backpack] = backpackExporter,
            [EItemFilterType.Glider] = gliderExporter,
            [EItemFilterType.Pickaxe] = pickaxeExporter,
        };

        // Gets all paths that could be a possible valid export for the given type
        public static List<GameFile> GetPossibleFiles(EItemFilterType type)
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

        public static bool TryLoadItemPreview(EItemFilterType type, GameFile gameFile, out ItemPreviewInfo itemPreviewInfo)
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

    public enum EItemFilterType
    {
        Character,
        Backpack,
        Glider,
        Pickaxe
    }
}
