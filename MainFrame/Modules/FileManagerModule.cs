using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Vfs;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TModel.MainFrame.Modules;
using TModel.MainFrame.Widgets;
using TModel.Sorters;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    // Manages loading of VFS
    public class FileManagerModule : ModuleBase
    {
        public override string ModuleName => "File Manager";

        StackPanel FilesPanel = new StackPanel();

        public static Action FilesLoaded;

        public FileManagerModule() : base()
        {

        }

        public override void StartupModule()
        {
            Grid grid = new Grid();
            Content = grid;
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(120) });
            grid.RowDefinitions.Add(new RowDefinition());

            StackPanel ButtonPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            ButtonPanel.HorizontalAlignment = HorizontalAlignment.Center;
            ButtonPanel.VerticalAlignment = VerticalAlignment.Center;
            
            ScrollViewer FilePanelScroller = new ScrollViewer();
            FilePanelScroller.Content = FilesPanel;
            Grid.SetRow(ButtonPanel, 0);
            Grid.SetRow(FilePanelScroller, 1);

            grid.Children.Add(ButtonPanel);
            grid.Children.Add(FilePanelScroller);

            grid.Background = HexBrush("#2e3d54");

            CButton LoadButton = new CButton("Load", 40);
            LoadButton.Click += () =>
            {
                Log.Information("Loading files");
                Task.Run(() => 
                    {
                        if (App.IsValidGameDirectory(Preferences.GameDirectory))
                        {
                            if (App.FileProvider == null)
                            {
                                App.FileProvider = new DefaultFileProvider(Preferences.GameDirectory, SearchOption.TopDirectoryOnly, false, new VersionContainer(EGame.GAME_UE5_1));
                                App.FileProvider.Initialize();
                            }
                            LoadGame();
                        }
                        else
                        {
                            App.LogMessage(string.IsNullOrEmpty(Preferences.GameDirectory) ? "Please set the Game Directory in settings" : $"\'{Preferences.GameDirectory}\' is not a valid directory (Change this in settings)", MessageLevel.Error);
                        }
                    }
                ).GetAwaiter().OnCompleted(() =>
                {
                    if (App.FileProvider != null)
                    {
                        List<IAesVfsReader> AllVFS = new List<IAesVfsReader>();
                        AllVFS.AddRange(App.FileProvider.MountedVfs);
                        AllVFS.AddRange(App.FileProvider.UnloadedVFS.Keys);
                        AllVFS.Sort(new NameSort());
                        FilesPanel.Children.Clear();
                        LoadFiles(AllVFS, true);
                        FilesLoaded();
                    }
                });
            };

            LoadButton.Height = 90;
            LoadButton.Width = 180;

            ButtonPanel.Children.Add(LoadButton);

            Preferences.PreferencesChanged += () =>
            {
                if (App.IsValidGameDirectory(Preferences.GameDirectory))
                {
                    if (App.FileProvider == null)
                    {
                        App.FileProvider = new DefaultFileProvider(Preferences.GameDirectory, SearchOption.TopDirectoryOnly, false, new VersionContainer(EGame.GAME_UE5_1));
                        App.FileProvider.Initialize();
                    }
                    LoadGame();
                    LoadFiles(App.FileProvider.UnloadedVFS.Keys);
                }
            };

            // App.FileProvider.UnloadedVfs.Sort(new NameSort()); 
            if (App.FileProvider != null)
                LoadFiles(App.FileProvider.UnloadedVFS.Keys);
        }

        public static void LoadGame()
        {
            // Submits AES keys
            AesKeys AesKeys;
            string JsonString = "";
            string AesKeysFile = Path.Combine(Preferences.StorageFolder, "AesKeys.json");
            if (File.Exists(AesKeysFile))
            {
                // Load aes keys from file
                JsonString = File.ReadAllText(AesKeysFile);
            }
            else
            {
                // Download aes keys from Fortnite API
                WebClient Client = new WebClient();
                JsonString = Client.DownloadString(@"https://fortnite-api.com/v2/aes");
                // Save the keys on disk
                File.WriteAllText(AesKeysFile, JsonString);
            }
            AesKeys = JsonConvert.DeserializeObject<MainAesKeys>(JsonString).data;
            // Main key
            App.FileProvider.SubmitKey(new FGuid(), new FAesKey(AesKeys.mainkey));
            // Dynamic keys
            foreach (DynamicAesKey key in AesKeys.dynamicKeys)
                App.FileProvider.SubmitKey(new FGuid(key.pakGuid), new FAesKey(key.key));

            App.FileProvider.LoadMappings();
        }

        struct MainAesKeys
        {
            public AesKeys data;
        }

        struct AesKeys
        {
            public string mainkey;
            public DynamicAesKey[] dynamicKeys;
        }

        struct DynamicAesKey
        {
            public string pakGuid;
            public string key;
        }

        private void LoadFiles(ICollection<IAesVfsReader> files, bool tryload = false)
        {
            foreach (var item in files)
            {
                if (item.Name.EndsWith(".utoc"))
                    FilesPanel.Children.Add(new FileManagerItem(item, tryload));
            }
        }
    }

    public class FileManagerItem : ContentControl
    {

        // Needs name (and file size probs)
        public FileManagerItem(IAesVfsReader reader, bool tryload = false)
        {
            Brush BackgroundBrush = reader.FileCount > 0 ? HexBrush("073880") : HexBrush("031c40");
            Border RootBorder = new Border()
            {
                Padding = new Thickness(5),
                Background = BackgroundBrush,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            MouseEnter += (sender, args) =>
            {
                RootBorder.Background = HexBrush("#0f00b2");
            };

            MouseLeave += (sender, args) =>
            {
                RootBorder.Background = BackgroundBrush;
            };

            MouseLeftButtonUp += (sender, args) =>
            {
                RootBorder.Background = HexBrush("#0f00b2");
            };

            CoreTextBlock FilesCountText = new CoreTextBlock(reader.FileCount.ToString())
            {
                TextAlignment = TextAlignment.Right,
                Width = 60,
                Margin = new Thickness(0, 0, 40, 0)
            };

            Grid MainPanel = new Grid();
            MainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            MainPanel.ColumnDefinitions.Add(new ColumnDefinition());
            RootBorder.Child = MainPanel;

            CoreTextBlock FileNameText = new CoreTextBlock(reader.Name)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            StackPanel DetailsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            Grid.SetColumn(DetailsPanel, 1);
            MainPanel.Children.Add(FileNameText);

            DetailsPanel.Children.Add(FilesCountText);


            MainPanel.Children.Add(DetailsPanel);

            Content = RootBorder;
        }
    }
}
