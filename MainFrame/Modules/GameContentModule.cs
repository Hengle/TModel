﻿using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
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
using static TModel.ColorConverters;

namespace TModel.Modules
{
    // Module for selecting items for exporting.
    // ItemPreviewModule shows the selected item from this module.
    internal class GameContentModule : ModuleBase
    {
        // The number of items that can be shown on a page.
        // It limits the number of items that 
        // be shown at once to increase peformance.
        private static readonly int PageSize = 100;

        public static Action<ItemTileInfo> SelectionChanged;

        public static GameContentItem? SelectedItem { get; private set; }

        // Cant be below one
        int PageNum = 1;
        int TotalPages = 0;

        EItemFilterType? Filter = null;

        public override string ModuleName => "Game Content";

        WrapPanel ItemPanel = new WrapPanel() { Background = HexBrush("#21284d") };

        CoreTextBlock PageNumberText = new CoreTextBlock("Page Number") 
        { 
            Margin = new Thickness(20, 0, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        CoreTextBlock LoadedCountText = new CoreTextBlock("Loaded Count") 
        { 
            Margin = new Thickness(0, 0, 20, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        CButton LeftButton = new CButton("Previous Page") { Width = 150 };
        CButton RightButton = new CButton("Next Page") { Width = 150 };

        Dictionary<ItemTileInfo, GameContentItem> LoadedContentItems = new Dictionary<ItemTileInfo, GameContentItem>();

        public static readonly double MinItemSize = 50;

        CancellationTokenSource cTokenSource = new CancellationTokenSource();

        public static ExporterBase CurrentExporter = FortUtils.characterExporter;

        public GameContentModule() : base()
        {

        }

        public override void StartupModule()
        {
            Grid Root = new Grid() { Background = HexBrush("#1a1b21") };
            Root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // SearchBar
            Root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Buttons panel
            Root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Filter options
            Root.RowDefinitions.Add(new RowDefinition()); // Items Panel

            CoreTextBlock LoadFilesWarningText = new CoreTextBlock("Load files in File Manager to use this window", 25)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
            };

            FileManagerModule.FilesLoaded += () =>
            {
                LoadFilesWarningText.Visibility = Visibility.Collapsed;
                if (Filter != null)
                    LoadFilterType();
            };

            WrapPanel FilterOptions = new WrapPanel();

            ScrollViewer ItemPanelScroller = new ScrollViewer();
            ItemPanelScroller.Content = ItemPanel;

            Grid ButtonPanel = new Grid();
            ButtonPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ButtonPanel.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonPanel.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            CoreTextBox SearchBox = new CoreTextBox() { MinHeight = 40 };

            SearchBox.TextChanged += (sender, args) =>
            {
                string SearchTerm = SearchBox.Text.Trim().Normalize();
                ItemTileInfo[] ItemTiles = CurrentExporter.LoadedPreviews.ToArray();
                List<ItemTileInfo> MatchingPreviews = new List<ItemTileInfo>();
                foreach (var item in ItemTiles)
                {
                    if (item.Name.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase))
                    {
                        MatchingPreviews.Add(item);
                    }
                }
                LoadPages(MatchingPreviews);
            };

            Grid.SetColumn(PageNumberText, 1);
            Grid.SetColumn(LoadedCountText, 2);

            ButtonPanel.Children.Add(PageNumberText);
            ButtonPanel.Children.Add(LoadedCountText);

            Grid.SetColumn(LeftButton, 0);
            Grid.SetColumn(RightButton, 3);

            ButtonPanel.Children.Add(LeftButton);
            ButtonPanel.Children.Add(RightButton);

            WrapPanel FilterTypesPanel = new WrapPanel() 
            { 
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            foreach (string name in Enum.GetNames(typeof(EItemFilterType)))
            {
                CRadioButton radioButton = new CRadioButton() { Content = new CoreTextBlock(name, 20) };
                radioButton.Tag = name;
                FilterTypesPanel.Children.Add(radioButton);

                radioButton.Click += (sender, args) =>
                {
                    ItemPanel.Children.Clear();
                    cTokenSource.Token.Register(() => 
                    {
                        cTokenSource = new CancellationTokenSource();
                        string Name = (string)((CRadioButton)sender).Tag;
                        EItemFilterType FilterType = Enum.Parse<EItemFilterType>(Name);
                        Filter = FilterType;
                        CurrentExporter = FortUtils.Exporters[FilterType];
                        PageNum = 1;
                        LoadedContentItems.Clear();
                        LoadPages(CurrentExporter.LoadedPreviews.ToArray());
                        LoadFilterType();
                        UpdatePageCount();
                    });
                    cTokenSource.Cancel();
                };
            }

            Root.Children.Add(FilterTypesPanel);

            LeftButton.Click += () =>
            {
                if (PageNum > 1)
                {
                    PageNum--;
                    UpdatePageCount();
                    LoadPages(CurrentExporter.LoadedPreviews.ToArray());
                }
            };

            RightButton.Click += () =>
            {
                if (PageNum < TotalPages)
                {
                    PageNum++;
                    UpdatePageCount();
                    LoadPages(CurrentExporter.LoadedPreviews.ToArray());
                }
            };

            bool CanZoom = false;

            Root.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.LeftCtrl)
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

            ItemPanel.MouseWheel += (sender, args) =>
            {
                if (CanZoom)
                {
                    double Multiplier = (args.Delta / 50F);
                    double Result = GameContentItem.ShownSize + Multiplier;
                    GameContentItem.ShownSize = Result > MinItemSize ? Result : MinItemSize;
                    foreach (var item in ItemPanel.Children)
                    {
                        ((GameContentItem)item).UpdateSize();
                    }
                }
            };

            Grid.SetRow(SearchBox, 0);
            Grid.SetRow(ButtonPanel, 1);
            Grid.SetRow(FilterTypesPanel, 2);
            Grid.SetRow(ItemPanelScroller, 3);
            Grid.SetRow(LoadFilesWarningText, 3);

            Root.Children.Add(SearchBox);
            Root.Children.Add(ButtonPanel);
            Root.Children.Add(FilterOptions);
            Root.Children.Add(ItemPanelScroller);

            Root.Children.Add(LoadFilesWarningText);

            Content = Root;
        }

        void UpdatePageCount() => PageNumberText.Text = $"{PageNum}/{TotalPages = (CurrentExporter.LoadedPreviews.Count / PageSize) + 1}";

        void LoadFilterType()
        {
            if (!CurrentExporter.bHasGameFiles)
                CurrentExporter.GameFiles = FortUtils.GetPossibleFiles(Filter ?? EItemFilterType.Character);

            Task.Run(() =>
            {
                if (!CurrentExporter.bHasLoadedAllPreviews)
                {
                    foreach (var gamefile in CurrentExporter.GameFiles)
                    {
                        cTokenSource.Token.ThrowIfCancellationRequested();
                        try
                        {
                            if (FortUtils.TryLoadItemPreviewInfo(Filter ?? EItemFilterType.Character, gamefile, out ItemTileInfo itemPreviewInfo))
                            {
                                if (!gamefile.IsItemLoaded)
                                {
                                    CurrentExporter.LoadedPreviews.Add(itemPreviewInfo);
                                    App.Refresh(() =>
                                    {
                                        UpdatePageCount();
                                        LoadedCountText.Text = $"{CurrentExporter.LoadedPreviews.Count + 1}/{CurrentExporter.GameFiles.Count}";
                                        if (PageNum == TotalPages)
                                        {
                                            ItemPanel.Children.Add(new GameContentItem(itemPreviewInfo));
                                        }
                                    });
                                    gamefile.IsItemLoaded = true;
                                }
                            }
                        }
                        catch { } // Don't care
                    }
                }
                else
                {
                    App.Refresh(() =>
                    {
                        foreach (var preview in CurrentExporter.LoadedPreviews)
                        {
                            ItemPanel.Children.Add(new GameContentItem(preview));
                        }
                    });
                }
            });
        }

        void LoadPages(IList<ItemTileInfo> Previews)
        {
            ItemPanel.Children.Clear();

            int FinalSize = (PageSize * PageNum) > Previews.Count ? Previews.Count : (PageSize * PageNum);

            for (int i = (PageSize * PageNum) - PageSize; i < FinalSize; i++)
            {
                if (LoadedContentItems.TryGetValue(Previews[i], out GameContentItem LoadedItem))
                    ItemPanel.Children.Add(LoadedItem);
                else
                    ItemPanel.Children.Add(LoadedContentItems[Previews[i]] = new GameContentItem(Previews[i]));
            }
        }

        public class GameContentItem : ContentControl
        {
            public static double ShownSize { set; get; } = 80;

            public ItemTileInfo Info { get; private set; }

            private static Brush CBorder = HexBrush("#485abb");
            private static Brush CBackground = HexBrush("#002661");
            private static Brush Selected = HexBrush("#ffde59");

            bool IsSelected => SelectedItem == this;

            Grid Root = new Grid();

            Border BackBorder = new Border()
            {
                BorderBrush = CBorder,
                Background = CBackground,
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
                MouseEnter += (sender, args) => BackBorder.BorderBrush = IsSelected ? Selected : Brushes.White;
                MouseLeave += (sender, args) => BackBorder.BorderBrush = IsSelected ? Selected : CBorder;

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
                    SelectionChanged(Info);
                }
                BackBorder.BorderBrush = Selected;
            }

            void Deselect()
            {
                BackBorder.BorderBrush = CBorder;
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
                            }); 
                        });
                });

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
        public string Name { set; get; } = "";
        public TextureRef? PreviewIcon { set; get; } = null;

        public ExportPreviewInfo()
        {

        }

        public ExportPreviewInfo(ItemTileInfo info)
        {
            Name = info.Name;
            PreviewIcon = info.PreviewIcon;
        }
    }
}
