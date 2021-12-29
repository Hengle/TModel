using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TModel.Export;
using TModel.MainFrame.Widgets;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    class ItemPreviewModule : ModuleBase
    {
        public override string ModuleName => "Item Preview";

        IPackage? Package = null;

        public ItemPreviewModule()
        {
            Grid Root = new Grid();
            Root.RowDefinitions.Add(new RowDefinition());
        }

        public override void StartupModule()
        {
            Grid Root = new Grid() 
            { 
                Background = HexBrush("#17162e"),
            };
            Root.RowDefinitions.Add(new RowDefinition() { });
            Root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid ItemDisplay = new Grid() { Margin = new Thickness(20) };
            ItemDisplay.RowDefinitions.Add(new RowDefinition());
            ItemDisplay.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Root.Children.Add(ItemDisplay);

            WrapPanel ButtonPanel = new WrapPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            CButton ExportButton = null;

            ExportButton = new CButton("Export", 60, () =>
            {
                try
                {
                    App.Refresh(() => 
                    {
                        ExportButton.SetText("        ");
                        ExportButton.IsEnabled = false;
                    });
                    Task.Run(() =>
                    {
                        if (Package != null)
                            GameContentModule.CurrentExporter.GetBlenderExportInfo(Package).Save();
                    })
                    .GetAwaiter()
                    .OnCompleted(() => 
                    {
                        ExportButton.SetText("Complete");
                        ExportButton.IsEnabled = true;
                    });
                }
                catch (Exception e)
                {
                    ExportButton.SetText("Failed");
                    App.LogMessage(e.ToString());
#if DEBUG
                    throw;
#endif
                }
            })
            {
                Margin = new Thickness(0, 0, 0, 40)
            };

            ButtonPanel.Children.Add(ExportButton);
            Grid.SetRow(ButtonPanel, 1);
            Root.Children.Add(ButtonPanel);
            GameContentModule.SelectionChanged += (ItemTileInfo Item) =>
            {
                ExportButton.SetText("Export");
                this.Package = Item.Package;
                ItemDisplay.Children.Clear();
                ExportPreviewInfo Preview = GameContentModule.CurrentExporter.GetExportPreviewInfo(Item.Package);

                if (Preview.PreviewIcon is TextureRef ImageRef)
                    if (ImageRef.TryGet_BitmapImage(out BitmapImage? previewIcon))
                    {
                        ItemDisplay.Children.Add(new Image() { Source = previewIcon, MaxHeight = 500 });
                    }
                ReadonlyText NameText = new ReadonlyText(Item?.Name ?? "NULL", 60) 
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                };
                Grid.SetRow(NameText, 1);
                ItemDisplay.Children.Add(NameText);
            };

            Content = Root;
        }
    }
}
