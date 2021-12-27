using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TModel.MainFrame.Widgets;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    class ItemPreviewModule : ModuleBase
    {
        public override string ModuleName => "Item Preview";

        public ItemPreviewModule()
        {
            Grid Root = new Grid();
            Root.RowDefinitions.Add(new RowDefinition());
        }

        public override void StartupModule()
        {
            Grid Root = new Grid() { Background = HexBrush("#17162e") };
            Root.RowDefinitions.Add(new RowDefinition() { });
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });

            Grid ItemDisplay = new Grid();
            Root.Children.Add(ItemDisplay);
            WrapPanel ButtonPanel = new WrapPanel();
            ButtonPanel.Children.Add(new CButton("Export", 30));
            Grid.SetRow(ButtonPanel, 1);
            Root.Children.Add(ButtonPanel);
            GameContentModule.SelectionChanged += (ItemTileInfo Item) =>
            {
                ItemDisplay.Children.Clear();
                ExportPreviewInfo Preview = GameContentModule.CurrentExporter.GetExportPreviewInfo(Item.Package);

                if (Preview.PreviewIcon is TextureRef ImageRef)
                    if (ImageRef.TryGet_BitmapImage(out BitmapImage? previewIcon))
                    {
                        ItemDisplay.Children.Add(new Image() { Source = previewIcon });
                    }
            };

            Content = Root;
        }
    }
}
