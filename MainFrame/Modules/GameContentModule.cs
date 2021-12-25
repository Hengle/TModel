using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    internal class GameContentModule : ModuleBase
    {
        public GameContentModule() : base()
        {
            ModuleName = "Game Content";
        }

        public override void StartupModule()
        {
            Grid Root = new Grid() { Background = HexBrush("#1a1b21") };
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80) });
            Root.RowDefinitions.Add(new RowDefinition());

            WrapPanel FilterOptions = new WrapPanel();

            Root.Children.Add(FilterOptions);

            foreach (var name in Enum.GetNames(typeof(GameContentFilterTypes)))
            {
                RadioButton radioButton = new RadioButton() { Content = new CoreTextBlock(name, 50) };
                FilterOptions.Children.Add(radioButton);
            }

            ScrollViewer scrollViewer = new ScrollViewer();
            WrapPanel ItemPanel = new WrapPanel() { Background = HexBrush("#21284d") };
            scrollViewer.Content = ItemPanel;
            Grid.SetRow(scrollViewer, 1);
            Root.Children.Add(scrollViewer);

            for (int i = 0; i < 200; i++)
            {
                ItemPanel.Children.Add(new GameContentItem(new ItemPreviewInfo()));
            }

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
