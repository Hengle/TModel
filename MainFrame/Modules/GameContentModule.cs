using CUE4Parse.FileProvider;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TModel.Exporters;
using TModel.MainFrame.Modules;
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
        private static readonly int PageSize = 300;

        public static Action<GameContentItem> SelectionChanged;

        public static GameContentItem? SelectedItem { get; private set; }

        // Cant be below one
        int PageNum = 1;
        int TotalPages = 0;

        EItemFilterType Filter = EItemFilterType.Pickaxe;

        public override string ModuleName => "Game Content";

        WrapPanel ItemPanel = new WrapPanel() { Background = HexBrush("#21284d") };

        CButton LoadButton = new CButton("Load") { Width = 150 };
        CButton RefreshButton = new CButton("Refresh") { Width = 150 };

        TextBlock PageNumberText = new TextBlock() { MinWidth = 80, Style = new DefaultText(15), Text = "Page Number", Margin = new Thickness(6, 0, 6, 0) };
        TextBlock LoadedCountText = new TextBlock() { MinWidth = 80, Style = new DefaultText(15), Text = "Loaded Count", Margin = new Thickness(6, 0, 6, 0) };

        CButton LeftButton = new CButton("Previous Page") { Width = 150 };
        CButton RightButton = new CButton("Next Page") { Width = 150 };


        public static readonly double MinItemSize = 50;

        CancellationTokenSource cTokenSource = new CancellationTokenSource();

        ExporterBase CurrentExporter = FortUtils.characterExporter;

        public GameContentModule() : base()
        {

        }

        public override void StartupModule()
        {
            Grid Root = new Grid() { Background = HexBrush("#1a1b21") };
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) }); // Buttons panel
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) }); // Filter options
            Root.RowDefinitions.Add(new RowDefinition()); // Items Panel

            WrapPanel FilterOptions = new WrapPanel();

            Root.Children.Add(FilterOptions);
            ScrollViewer ItemPanelScroller = new ScrollViewer();
            ItemPanelScroller.Content = ItemPanel;
            Grid.SetRow(ItemPanelScroller, 2);
            Root.Children.Add(ItemPanelScroller);

            WrapPanel ButtonPanel = new WrapPanel() { Orientation = Orientation.Horizontal};

            ButtonPanel.Children.Add(LoadButton);
            ButtonPanel.Children.Add(RefreshButton);

            ButtonPanel.Children.Add(PageNumberText);
            ButtonPanel.Children.Add(LoadedCountText);

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
                        PageNum = 0;
                        LoadFilterType();
                        UpdatePageCount();
                    });
                    cTokenSource.Cancel();
                };
            }

            Grid.SetRow(FilterTypesPanel, 1);
            Root.Children.Add(FilterTypesPanel);

            LeftButton.Click += () =>
            {
                if (PageNum > 1)
                {
                    PageNum--;
                    UpdatePageCount();
                    LoadPage();
                }
            };

            RightButton.Click += () =>
            {
                if (PageNum < TotalPages)
                {
                    PageNum++;
                    UpdatePageCount();
                    LoadPage();
                }
            };

            LoadButton.Click += () => LoadFilterType();

            RefreshButton.Click += () => LoadPage();


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

            Root.Children.Add(ButtonPanel);

#if false // Shows filters
            foreach (var name in Enum.GetNames(typeof(GameContentFilterTypes)))
            {
                RadioButton radioButton = new RadioButton() { Content = new CoreTextBlock(name, 50) };
                FilterOptions.Children.Add(radioButton);
            }
#endif
            Content = Root;
        }

        void UpdatePageCount() => PageNumberText.Text = $"{PageNum}/{TotalPages = (CurrentExporter.LoadedPreviews.Count / PageSize) + 1}";

        void LoadFilterType()
        {
            if (!CurrentExporter.bHasGameFiles)
                CurrentExporter.GameFiles = FortUtils.GetPossibleFiles(Filter);

            Task.Run(() =>
            {
                if (!CurrentExporter.bHasLoadedAllPreviews)
                {
                    for (int i = CurrentExporter.LoadedPreviews.Count; i < CurrentExporter.GameFiles.Count; i++)
                    {
                        if (i > CurrentExporter.GameFiles.Count)
                            break;
                        cTokenSource.Token.ThrowIfCancellationRequested();
                        if (FortUtils.TryLoadItemPreview(Filter, CurrentExporter.GameFiles[i], out ItemPreviewInfo itemPreviewInfo))
                        {
                            CurrentExporter.LoadedPreviews.Add(itemPreviewInfo);
                            App.Refresh(() =>
                            {
                                UpdatePageCount();
                                LoadedCountText.Text = $"{CurrentExporter.LoadedPreviews.Count}/{CurrentExporter.GameFiles.Count}";
                                if (PageNum == TotalPages)
                                {
                                    ItemPanel.Children.Add(new GameContentItem(itemPreviewInfo));
                                }
                            });
                        }
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

        void LoadPage()
        {
            ItemPreviewInfo[] Previews = CurrentExporter.LoadedPreviews.ToArray();
            ItemPanel.Children.Clear();

            int FinalSize = (PageSize * PageNum) > Previews.Length ? Previews.Length : (PageSize * PageNum);

            for (int i = (PageSize * PageNum) - PageSize; i < FinalSize; i++)
            {
                ItemPanel.Children.Add(new GameContentItem(Previews[i]));
            }
        }

        public class GameContentItem : ContentControl
        {
            public static double ShownSize { set; get; } = 80;

            public ItemPreviewInfo Info { get; private set; }

            private static Brush CBorder = HexBrush("#485abb");
            private static Brush CBackground = HexBrush("#002661");
            private static Brush Selected = HexBrush("#ffde59");

            bool IsSelected => SelectedItem == this;

            Grid Root = new Grid();

            Border BackBorder = new Border()
            {
                BorderBrush = CBorder,
                Background = CBackground,
                BorderThickness = new Thickness(3),
            };

            public void UpdateSize()
            {
                Root.Width = ShownSize;
                Root.Height = ShownSize;
                if (ShownSize == MinItemSize)
                {
                    BackBorder.BorderThickness = new Thickness(0);
                    Margin = new Thickness(0);
                }
                else
                {
                    BackBorder.BorderThickness = new Thickness(ShownSize / 24);
                    Margin = new Thickness(ShownSize / 20);
                }

            }

            public GameContentItem()
            {
                UpdateSize();

                // Hover FX
                MouseEnter += (sender, args) => BackBorder.BorderBrush = IsSelected ? Selected : Brushes.White;
                MouseLeave += (sender, args) => BackBorder.BorderBrush = IsSelected ? Selected : CBorder;

                MouseLeftButtonDown += (sender, args) => Select();

                Margin = new Thickness(4);
                Root.Children.Add(BackBorder);
                Content = Root;
            }

            void Select()
            {
                if (SelectedItem is GameContentItem Item)
                    Item.Deselect();
                SelectedItem = this;
                if (SelectionChanged is not null)
                    SelectionChanged(this);
                BackBorder.BorderBrush = Selected;
            }

            void Deselect()
            {
                BackBorder.BorderBrush = CBorder;
            }

            public GameContentItem(ItemPreviewInfo info) : this()
            {
                Info = info;

                // Sets preview icon
                Task.Run(() =>
                {
                    if (Info.PreviewIcon.TryGet_BitmapImage(out BitmapImage bitmapImage))
                        App.Refresh(() => Root.Children.Add(new Image() { Source = bitmapImage }));
                });

            }
        }
    }

    // Base class for all item info to be displayed onto GameContent module.
    public class ItemPreviewInfo
    {
        public string Name { set; get; } = "";
        public string FullPath { set; get; } = "";
        public TextureRef? PreviewIcon { set; get; } = TextureRef.Empty;
    }

    // Fortnite cosmetics
    public class ItemPreviewCosmeticInfo : ItemPreviewInfo
    {

    }
}
