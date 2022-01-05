﻿using CUE4Parse.FileProvider;
using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using TModel.Export.Exporters;
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
        public static WeaponExporter weaponExporter { get; } = new WeaponExporter();
        public static PropExporter propExporter { get; } = new PropExporter();
        public static PlaysetExporter PlaysetExporter { get; } = new PlaysetExporter();

        public static Dictionary<EItemFilterType, ExporterBase> Exporters { get; } = new()
        {
            [EItemFilterType.Character] = characterExporter,
            [EItemFilterType.Backpack] = backpackExporter,
            [EItemFilterType.Glider] = gliderExporter,
            [EItemFilterType.Pickaxe] = pickaxeExporter,
            [EItemFilterType.Weapon] = weaponExporter,
            [EItemFilterType.Prop] = propExporter,
#if !RELEASE
            [EItemFilterType.Playset] = PlaysetExporter,
#endif
        };

        // Gets all paths that could be a possible valid export (using the SearchTerm) for the given exporter
        public static IEnumerable<GameFile> GetGameFiles(ExporterBase exporter)
        {
            foreach (var path in App.FileProvider.Files)
                if (exporter.SearchTerm.CheckName(path.Key))
                    yield return path.Value;
        }

        public static bool TryLoadItemPreviewInfo(ExporterBase exporter, GameFile gameFile, out ItemTileInfo itemPreviewInfo)
        {
            itemPreviewInfo = null;
            if (App.FileProvider.TryLoadPackage(gameFile, out IPackage package))
            {
                itemPreviewInfo = exporter.GetTileInfo(package);
                if (itemPreviewInfo != null)
                    itemPreviewInfo.Package = package;
            }
            return itemPreviewInfo != null;
        }
    }

    public class FortGameContentItem
    {
        public ItemTileInfo? Info;
        public GameFile File;
    }

    // TODO: rename this because this is'nt a filter
    public enum EItemFilterType
    {
        Character,
        Backpack,
        Glider,
        Pickaxe,
        Weapon,
        Prop,
#if !RELEASE
        Playset
#endif
    }
}
