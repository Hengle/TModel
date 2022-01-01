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
using TModel.MainFrame.Modules;
using TModel.MainFrame.Widgets;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    class ItemPreviewModule : ModuleBase
    {
        public override string ModuleName => "Item Preview";

        public static BitmapImage IconImage = new BitmapImage(new Uri(@"C:\Users\bigho\Desktop\Frozen Peely.png"));

        public ItemPreviewModule()
        {
            GameContentModule.SelectionChanged += (Item) => SelectionChanged(Item);

            Grid Root = new Grid();

            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Pixel) });
            Root.RowDefinitions.Add(new RowDefinition());

            Grid LowerPanel = new Grid();
            Grid.SetRow(LowerPanel, 1);
            Root.Children.Add(LowerPanel);
            LowerPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Pixel)});
            LowerPanel.ColumnDefinitions.Add(new ColumnDefinition());

            Grid LowerRightPanel = new Grid() { Background = HexBrush("#1d1c3b") };
            LowerRightPanel.RowDefinitions.Add(new RowDefinition());
            LowerRightPanel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Pixel) });
            Grid.SetColumn(LowerRightPanel, 1);
            LowerPanel.Children.Add(LowerRightPanel);

            CScrollViewer StylesScroller = new CScrollViewer();
            StackPanel StylesPanel = new StackPanel()
            {
                Margin = new Thickness(0,60,0,0)
            };
            StylesScroller.Content = StylesPanel;
            LowerRightPanel.Children.Add(StylesScroller);

            for (int i = 0; i < 5; i++)
            {
                Grid StyleSetPanel = new Grid()
                {
                    Background = HexBrush("#282682"),
                };
                Border StyleSetNameBorder = new Border()
                {
                    Background = HexBrush("#1a1956"),
                    Margin = new Thickness(0, -30, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                WrapPanel StyleSetOptionsPanel = new WrapPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 10, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                StyleSetPanel.Children.Add(StyleSetOptionsPanel);

                for (int j = 0; j < 20; j++)
                {
                    Border StyleOptionBorder = new Border() 
                    {
                        Background = HexBrush("#3d3d59"),
                        BorderBrush = HexBrush("#5957a3"),
                        BorderThickness = new Thickness(2),
                        Width = 60,
                        Height = 60,
                        Margin = new Thickness(3),
                    };
                    StyleOptionBorder.ToolTip = new CTooltip("Name");
                    Image StyleOptionImage = new Image() { Source = IconImage };
                    StyleOptionBorder.Child = StyleOptionImage;
                    StyleSetOptionsPanel.Children.Add(StyleOptionBorder);
                }

                StyleSetNameBorder.Width = 150;
                StyleSetNameBorder.Height = 40;

                CTextBlock StyleSetNameText = new CTextBlock("Set Name", 20)
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                StyleSetNameBorder.Child = StyleSetNameText;
                StyleSetPanel.Children.Add(StyleSetNameBorder);
                StyleSetPanel.Margin = new Thickness(20,0,20,60);
                StyleSetPanel.MinHeight = 100;
                StyleSetPanel.HorizontalAlignment = HorizontalAlignment.Center;
                StylesPanel.Children.Add(StyleSetPanel); 
            }

            Border ButtonsBorder = new Border() 
            {
                Background = HexBrush("#0d2d59"),
                BorderBrush = HexBrush("#537cb3"),
                BorderThickness = new Thickness(0,6,0,0),
            };
            Grid ButtonsGrid = new Grid();
            ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());

            Grid ButtonsRowGrid = new Grid();
            ButtonsRowGrid.RowDefinitions.Add(new RowDefinition());
            ButtonsRowGrid.RowDefinitions.Add(new RowDefinition());

            CButton SaveButton = new CButton("Save", 20) { Margin = new Thickness(6, 6, 2, 2) };
            CButton DirectoryButton = new CButton("Directory", 20) { Margin = new Thickness(6, 2, 2, 6) };
            CButton ExportButton = new CButton("Export", 30) { Margin = new Thickness(4, 6, 2, 6) };
            CButton ShowModelButton = new CButton("Show\nModel", 20) { Margin = new Thickness(2, 6, 6, 6) };

            Grid.SetRow(SaveButton, 0);
            Grid.SetRow(DirectoryButton, 1);
            ButtonsRowGrid.Children.Add(SaveButton);
            ButtonsRowGrid.Children.Add(DirectoryButton);

            Grid.SetColumn(ExportButton, 1);
            Grid.SetColumn(ShowModelButton, 2);
            ButtonsGrid.Children.Add(ExportButton);
            ButtonsGrid.Children.Add(ShowModelButton);

            ButtonsGrid.Children.Add(ButtonsRowGrid);
            ButtonsBorder.Child = ButtonsGrid;

            Grid.SetRow(ButtonsBorder, 1);
            LowerRightPanel.Children.Add(ButtonsBorder);
            Border RelatedCosmeticsBorder = new Border();
            RelatedCosmeticsBorder.Background = HexBrush("#2d3f76");
            RelatedCosmeticsBorder.BorderBrush = HexBrush("#5366a2");
            RelatedCosmeticsBorder.BorderThickness = new Thickness(0,0,5,0);
            LowerPanel.Children.Add(RelatedCosmeticsBorder);

            CScrollViewer RelatedCosmeticsScroller = new CScrollViewer();
            StackPanel RelatedCosmeticsStack = new StackPanel();
            RelatedCosmeticsScroller.Content = RelatedCosmeticsStack;
            RelatedCosmeticsBorder.Child = RelatedCosmeticsScroller;
            RelatedCosmeticsStack.Margin = new Thickness(10);


            for (int i = 0; i < 20; i++)
            {
                Border CosmeticBorder = new Border();
                CosmeticBorder.Height = 80;
                CosmeticBorder.Margin = new Thickness(0,0,0,10);
                CosmeticBorder.Background = HexBrush("#0a1c51");
                CosmeticBorder.BorderBrush = HexBrush("#3755b1");
                CosmeticBorder.BorderThickness = new Thickness(2);

                Image CosmeticIcon = new Image() { Source = IconImage };
                CosmeticBorder.Child = CosmeticIcon;

                RelatedCosmeticsStack.Children.Add(CosmeticBorder);
            }

            Border TopBorder = new Border();

            Grid TopGrid = new Grid();
            TopBorder.Child = TopGrid;
            TopBorder.BorderThickness = new Thickness(0,0,0,10);
            TopBorder.BorderBrush = HexBrush("#03195d");
            TopBorder.Background = HexBrush("#1b3176");

            TopGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(140, GridUnitType.Pixel) });
            TopGrid.ColumnDefinitions.Add(new ColumnDefinition());

            CTextBlock DisplayName = new CTextBlock("Frozen Peely", 40) 
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid TopRightGrid = new Grid();

            Grid TopLowerRightGrid = new Grid();

            TopLowerRightGrid.ColumnDefinitions.Add(new ColumnDefinition());
            TopLowerRightGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            Grid.SetRow(TopLowerRightGrid, 1);
            TopRightGrid.Children.Add(TopLowerRightGrid);

            CTextBlock Description = new CTextBlock("Ice cold banana", 20);
            CTextBlock Introduced = new CTextBlock("Chapter 3, Season 1", 20);
            Grid.SetColumn(Introduced, 1);

            TopLowerRightGrid.Children.Add(Description);
            TopLowerRightGrid.Children.Add(Introduced);

            TopRightGrid.RowDefinitions.Add(new RowDefinition());
            TopRightGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            TopRightGrid.Children.Add(DisplayName);
            TopRightGrid.Margin = new Thickness(0,0,10,10);
            Grid.SetColumn(TopRightGrid, 1);
            TopGrid.Children.Add(TopRightGrid);
            Image PreviewIcon = new Image() { Source = IconImage };


            Root.Children.Add(TopBorder);
            TopGrid.Children.Add(PreviewIcon);
            Content = Root;
        }

        private void SelectionChanged(ExportPreviewInfo Item)
        {

        }
    }
}
