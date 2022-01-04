﻿using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TModel.Export;
using TModel.Export.Exporters;
using TModel.MainFrame.Widgets;
using TModel.Sorters;
using System;
using System.Collections.Generic;
using System.Linq;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    // Module for selecting items for exporting.
    // ItemPreviewModule shows the selected item from this module.
    public class GameContentModule : ModuleBase
    {
        // The number of items that can be shown on a page.
        // It limits the number of items that 
        // be shown at once to increase peformance.
        private static readonly int PageSize = 100;

        public static Action<ExportPreviewInfo> SelectionChanged;

        CScrollViewer ItemPanelScroller = new CScrollViewer();
        public static GameContentItem? SelectedItem { get; private set; }

        int PageNum = 1;
        int TotalPages = 0;

        EItemFilterType Filter = EItemFilterType.Character;

        public override string ModuleName => "Game Content";

        WrapPanel ItemsPanel = new WrapPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        CTextBlock PageNumberText = new CTextBlock("") 
        { 
            Margin = new Thickness(20, 0, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        CTextBlock LoadedCountText = new CTextBlock("") 
        { 
            Margin = new Thickness(0, 0, 20, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        CButton OpenExportsButton = new CButton("Open Exports", 15, () => Process.Start("explorer.exe", Preferences.ExportsPath)) { MaxWidth = 110 };

        CButton B_LeftPage = new CButton("Previous Page");
        CButton B_RightPage = new CButton("Next Page");

        public static readonly double MinItemSize = 50;

        CancellationTokenSource cTokenSource = new CancellationTokenSource();
        CTextBox SearchBox = new CTextBox() { MinHeight = 40 };

        WrapPanel FilterTypesPanel = new WrapPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        List<ItemTileInfo> VisiblePreviews = new List<ItemTileInfo>();

        string? SearchTerm = null;

        Grid TopPanel = new Grid();

        Grid ButtonPanel = new Grid();

        Grid PageButtonsPanel = new Grid();

        public static ExporterBase CurrentExporter = FortUtils.characterExporter;

        Grid Root = new Grid() { Background = Theme.BackDark };
        WrapPanel FilterOptions = new WrapPanel();

        bool CanZoom;

        public GameContentModule() : base()
        {

            Root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // SearchBar
            Root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Buttons panel
            Root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Filter options
            Root.RowDefinitions.Add(new RowDefinition()); // Items Panel
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) }); // Page Buttons

            ItemPanelScroller.Content = ItemsPanel;

            ButtonPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ButtonPanel.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            Grid.SetColumn(PageNumberText, 0);
            Grid.SetColumn(LoadedCountText, 2);

            ButtonPanel.Children.Add(PageNumberText);
            ButtonPanel.Children.Add(LoadedCountText);

            Grid.SetColumn(OpenExportsButton, 1);

            Grid.SetColumn(B_LeftPage, 0);
            Grid.SetColumn(B_RightPage, 1);

            PageButtonsPanel.ColumnDefinitions.Add(new ColumnDefinition());
            PageButtonsPanel.ColumnDefinitions.Add(new ColumnDefinition());

            PageButtonsPanel.Children.Add(B_LeftPage);
            PageButtonsPanel.Children.Add(B_RightPage);

            TopPanel.ColumnDefinitions.Add(new ColumnDefinition());
            TopPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TopPanel.Children.Add(SearchBox);
            TopPanel.Children.Add(OpenExportsButton);

            Grid.SetRow(TopPanel, 0);
            Grid.SetRow(ButtonPanel, 1);
            Grid.SetRow(FilterTypesPanel, 2);
            Grid.SetRow(ItemPanelScroller, 3);
            Grid.SetRow(PageButtonsPanel, 4);

            Root.Children.Add(TopPanel);
            Root.Children.Add(ButtonPanel);
            Root.Children.Add(FilterTypesPanel);
            Root.Children.Add(ItemPanelScroller);
            Root.Children.Add(PageButtonsPanel);

            GenerateFilterTypes();

            SearchBox.TextChanged += (sender, args) =>
            {
                SearchTerm = SearchBox.Text.Normalize().Trim();
                if (string.IsNullOrWhiteSpace(SearchTerm)) SearchTerm = null;
                ItemTileInfo[] ItemTiles = CurrentExporter.LoadedPreviews.ToArray();
                if (SearchTerm != null)
                {
                    List<ItemTileInfo> MatchingPreviews = new List<ItemTileInfo>();
                    foreach (var item in ItemTiles)
                    {
                        if (item.Name.Replace(" ", "").Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            MatchingPreviews.Add(item);
                        }
                    }
                    if (VisiblePreviews.Count != MatchingPreviews.Count) // Something changed
                    {
                        VisiblePreviews = MatchingPreviews.ToArray().ToList();
                        LoadPages();
                    }
                }
                else // Nothing typed in search bar
                {
                    VisiblePreviews = CurrentExporter.LoadedPreviews.ToArray().ToList(); // Show all loaded items
                    LoadPages();
                }
                PageNum = 1;
                UpdatePageCount();
            };

            B_LeftPage.Click += () =>
            {
                if (PageNum > 1)
                {
                    PageNum--;
                    UpdatePageCount();
                    LoadPages();
                }
            };

            FileManagerModule.FilesLoaded += () =>
            {
                // Automaticlly loads items once File Manager is done loading files
                // TODO: make this optional in settings
                LoadFilterType();
            };

            B_RightPage.Click += () =>
            {
                if (PageNum < TotalPages)
                {
                    PageNum++;
                    UpdatePageCount();
                    LoadPages();
                }
            };

            ItemsPanel.MouseWheel += (sender, args) =>
            {
                if (CanZoom)
                {
                    double Multiplier = (args.Delta / 50F);
                    double Result = GameContentItem.ShownSize + Multiplier;
                    GameContentItem.ShownSize = Result > MinItemSize ? Result : MinItemSize;
                    foreach (var item in ItemsPanel.Children)
                    {
                        ((GameContentItem)item).UpdateSize();
                    }
                }
            };

            Root.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.LeftCtrl && !SearchBox.IsFocused)
                {
                    ItemPanelScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    CanZoom = true;
                }
            };

            Root.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.LeftCtrl)
                {
                    ItemPanelScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    CanZoom = false;
                }
            };

            Content = Root;
        }

        void GenerateFilterTypes()
        {
            foreach (string name in Enum.GetNames(typeof(EItemFilterType)))
            {
                RadioButton radioButton = new RadioButton()
                {
                    Margin = new Thickness(4),
                    Content = name,
                    FontSize = 16,
                    Foreground = Brushes.White,
                };
                radioButton.Tag = name;
                radioButton.IsChecked = Filter.ToString() == name.ToString();
                FilterTypesPanel.Children.Add(radioButton);

                radioButton.Click += (sender, args) =>
                {
                    cTokenSource = new CancellationTokenSource();
                    string Name = (string)((RadioButton)sender).Tag;
                    EItemFilterType FilterType = Enum.Parse<EItemFilterType>(Name);
                    Filter = FilterType;
                    CurrentExporter = FortUtils.Exporters[FilterType];
                    VisiblePreviews = CurrentExporter.LoadedPreviews.ToArray().ToList();
                    PageNum = 1;
                    if (FileManagerModule.HasLoaded)
                    {
                        ItemsPanel.Children.Clear();
                        cTokenSource.Token.Register(() =>
                        {
                            LoadPages();
                            LoadFilterType();
                            UpdatePageCount();
                            UpdateLoadedCount();
                            cTokenSource = new CancellationTokenSource();
                        });
                        cTokenSource.Cancel();
                    }
                };
            }
        }

        void UpdatePageCount() => PageNumberText.Text = $"Page {PageNum}/{TotalPages = (VisiblePreviews.Count / PageSize) + 1}";

        void UpdateLoadedCount() => LoadedCountText.Text = $"Loaded {CurrentExporter.LoadedPreviews.Count}/{CurrentExporter.GameFiles.Count - 1}";

        void LoadFilterType()
        {
            if (!CurrentExporter.bHasGameFiles)
            {
                CurrentExporter.GameFiles = FortUtils.GetPossibleFiles(Filter);
                CurrentExporter.GameFiles.Sort(new NameSort());
                CurrentExporter.GameFiles.Reverse();
            }

            Task.Run(() =>
            {
                if (!CurrentExporter.bHasLoadedAllPreviews)
                {
                    GameFile[] Files = CurrentExporter.GameFiles.ToArray();
                    foreach (var file in Files)
                    {
                        cTokenSource.Token.ThrowIfCancellationRequested();
                        try
                        {
                            if (!file.IsItemLoaded)
                            {
                                if (
                                     // !file.Name.Contains("_NPC_", StringComparison.OrdinalIgnoreCase) &&
                                     // !file.Name.Contains("_TBD_", StringComparison.OrdinalIgnoreCase) &&
                                     // !file.Name.Contains("_VIP_", StringComparison.OrdinalIgnoreCase) &&
                                     // file.Name.Contains("StrawberryPilot", StringComparison.OrdinalIgnoreCase) &&
FortUtils.TryLoadItemPreviewInfo(Filter, file, out ItemTileInfo itemPreviewInfo))
                                {
                                    CurrentExporter.LoadedPreviews.Add(itemPreviewInfo);
                                    App.Refresh(() =>
                                    {
                                        UpdatePageCount();
                                        UpdateLoadedCount();
                                        if (SearchTerm == null && PageNum == TotalPages) // On same page
                                        {
                                            ItemsPanel.Children.Add(new GameContentItem(itemPreviewInfo));
                                        }
                                        VisiblePreviews.Add(itemPreviewInfo);
                                    });
                                }
                                file.IsItemLoaded = true;
                            }
                        }
                        catch
                        {
                            file.IsItemLoaded = true; // Don't try to load again
                        }
                    }
                }
                else
                {
                    App.Refresh(() =>
                    {
                        foreach (var preview in CurrentExporter.LoadedPreviews)
                        {
                            ItemsPanel.Children.Add(new GameContentItem(preview));
                        }
                    });
                }
            });
        }

        void LoadPages()
        {
            ItemsPanel.Children.Clear();

            int FinalSize = (PageSize * PageNum) > VisiblePreviews.Count ? VisiblePreviews.Count : (PageSize * PageNum);

            for (int i = (PageSize * PageNum) - PageSize; i < FinalSize; i++)
            {
                ItemsPanel.Children.Add(new GameContentItem(VisiblePreviews[i]));
            }
        }

        public class GameContentItem : ContentControl
        {
            public static double ShownSize { set; get; } = 80;

            public ItemTileInfo Info { get; private set; }

            bool IsSelected => SelectedItem == this;

            Grid Root = new Grid();

            Border BackBorder = new Border()
            {
                BorderBrush = Theme.BorderNormal,
                Background = Theme.BackNormal,
                BorderThickness = new Thickness(ShownSize / 30),
            };

            public void UpdateSize()
            {
                Root.Width = ShownSize;
                Root.Height = ShownSize;
                BackBorder.BorderThickness = new Thickness(ShownSize / 30);
                Margin = new Thickness(ShownSize / 20);
            }

            public GameContentItem()
            {
                UpdateSize();

                // Hover FX
                MouseEnter += (sender, args) => BackBorder.BorderBrush = IsSelected ? Theme.BorderSelected : Theme.BorderHover;
                MouseLeave += (sender, args) => BackBorder.BorderBrush = IsSelected ? Theme.BorderSelected : Theme.BorderNormal;

                MouseLeftButtonDown += (sender, args) => Select();

                Margin = new Thickness(ShownSize / 20);
                Root.Children.Add(BackBorder);
                Content = Root;
            }

            void Select()
            {
                if (SelectedItem is GameContentItem Item)
                    Item.Deselect();
                SelectedItem = this;
                if (SelectionChanged is not null)
                {
                    App.ShowModule<ItemPreviewModule>();
                    try
                    {
                        SelectionChanged(CurrentExporter.GetExportPreviewInfo(Info.Package));
                    }
                    catch (Exception e)
                    {
                        Log.Error("Could not select item: \n" + e.ToString());
                    }

                }
                BackBorder.BorderBrush = Theme.BorderSelected;
            }

            void Deselect()
            {
                BackBorder.BorderBrush = Theme.BorderNormal;
            }

            public GameContentItem(ItemTileInfo info) : this()
            {
                Info = info;

                Root.ToolTip = new CTooltip(Info?.Name ?? "NULL");

                // Sets preview icon
                Task.Run(() =>
                {
                    if (Info.PreviewIcon.TryGet_BitmapImage(out BitmapImage bitmapImage))
                        App.Refresh(() => 
                        {
                            Root.Children.Add(new Image()
                            {
                                Source = bitmapImage,
                                Margin = new Thickness(ShownSize / 30),
                            }); 
                        });
                });

                UpdateSize();
            }
        }
    }

    // Holds information for a single item to be displayed in GameContentModule.
    public class ItemTileInfo
    {
        public IPackage Package { set; get; }
        public string Name { set; get; } = "";
        public string FullPath { set; get; } = "";
        public TextureRef? PreviewIcon { set; get; } = null;
    }

    // Holds information to be displayed in ItemPreviewModule
    public class ExportPreviewInfo
    {
        public IPackage Package { set; get; }

        public string Name { set; get; } = string.Empty;

        public string Description { set; get; } = string.Empty;

        public TextureRef? PreviewIcon { set; get; } = null;

        public List<ExportPreviewSet>? Styles { set; get; } = null;

        public ExportPreviewInfo()
        {

        }
    }

    public class ExportPreviewSet
    {
        public string Name { set; get; } = string.Empty;

        public List<ExportPreviewOption> Options { get; } = new List<ExportPreviewOption>();
    }

    public class ExportPreviewOption
    {
        public string Name { set; get; } = string.Empty;

        public TextureRef? Icon { set; get; }
    }
}
