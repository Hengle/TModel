using CUE4Parse.FileProvider;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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

        public GameContentModule() : base()
        {
            ModuleName = "Game Content";
        }

        public override void StartupModule()
        {

            Grid Root = new Grid() { Background = HexBrush("#1a1b21") };
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80) });
            Root.RowDefinitions.Add(new RowDefinition());
            //                     
            WrapPanel FilterOptions = new WrapPanel();

            Root.Children.Add(FilterOptions);

            ScrollViewer scrollViewer = new ScrollViewer();
            WrapPanel ItemPanel = new WrapPanel() { Background = HexBrush("#21284d") };
            scrollViewer.Content = ItemPanel;
            Grid.SetRow(scrollViewer, 1);
            Root.Children.Add(scrollViewer);

            CButton LoadButton = new CButton("Load");


            LoadButton.Click += () =>
            {
                List<GameFile> GameFiles = FortUtils.GetPossibleFiles(FilterType.Character);

                for (int i = 0; i < 100; i++)
                {
                    if (FortUtils.TryLoadItemPreview(FilterType.Character, GameFiles[i], out Lazy<ItemPreviewInfo>? itemPreviewInfo))
                    {
                        ItemPanel.Children.Add(new GameContentItem(itemPreviewInfo.Value));
                    }
                }


            };

            Root.Children.Add(LoadButton);

#if false // Shows filters
            foreach (var name in Enum.GetNames(typeof(GameContentFilterTypes)))
            {
                RadioButton radioButton = new RadioButton() { Content = new CoreTextBlock(name, 50) };
                FilterOptions.Children.Add(radioButton);
            }
#endif
            Content = Root;
        }

        class GameContentItem : ContentControl
        {
            private ItemPreviewInfo Info;

            public GameContentItem(ItemPreviewInfo info)
            {
                Info = info;

                Grid Root = new Grid();
                Content = Root;

                Border BackBorder = new Border()
                {
                    BorderBrush = HexBrush("#485abb"),
                    Background = HexBrush("#002661"),
                    BorderThickness = new Thickness(3),
                };

                Root.Children.Add(BackBorder);

                if (Info.PreviewIcon.TryGet_BitmapImage(out BitmapImage PreviewIcon))
                {
                    Root.Children.Add(new Image() { Source = PreviewIcon });
                }


                Margin = new Thickness(4);

                Root.Width = 80;
                Root.Height = 80;

                // Sets preview icon
                if (Info.PreviewIcon.TryGet_BitmapImage(out BitmapImage bitmapImage))
                    Root.Children.Add(new Image() { Source = bitmapImage });
            }
        }
    }

    // Base class for all item info to be displayed onto GameContent module.
    public class ItemPreviewInfo
    {
        public string Name { set; get; } = "";
        public string FullPath { set; get; } = "";
        public TextureRef PreviewIcon { set; get; } = TextureRef.Empty;
    }

    // Fortnite cosmetics
    public class ItemPreviewCosmeticInfo : ItemPreviewInfo
    {

    }

    public enum GameContentFilterTypes 
    {
        Character,
        Backpack,
        Glider,
        Pickaxe
    }
}
