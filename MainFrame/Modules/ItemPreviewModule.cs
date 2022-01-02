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
            Grid Root = new Grid();

            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Pixel) });
            Root.RowDefinitions.Add(new RowDefinition());

            Grid LowerPanel = new Grid();
            Grid.SetRow(LowerPanel, 1);
            Root.Children.Add(LowerPanel);
            LowerPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80, GridUnitType.Pixel)});
            LowerPanel.ColumnDefinitions.Add(new ColumnDefinition());

            Grid LowerRightPanel = new Grid() { Background = Theme.BackDark };
            LowerRightPanel.RowDefinitions.Add(new RowDefinition());
            LowerRightPanel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Pixel) });
            Grid.SetColumn(LowerRightPanel, 1);
            LowerPanel.Children.Add(LowerRightPanel);

            StackPanel StyleSetNamesPanel = new StackPanel() 
            { 
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            CScrollViewer StylesScroller = new CScrollViewer();
            StackPanel StylesPanel = new StackPanel()
            {
                Margin = new Thickness(0,20,0,0),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            StylesScroller.Content = StylesPanel;
            CScrollViewer StylePickerScroller = new CScrollViewer();
            Grid StylePickerPanel = new Grid();
            StylePickerScroller.Content = StylePickerPanel;
            StylePickerPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            StylePickerPanel.ColumnDefinitions.Add(new ColumnDefinition());
            StylePickerPanel.Children.Add(StyleSetNamesPanel);
            Grid.SetColumn(StylesScroller, 1);
            // StylePickerPanel.Children.Add(StylesScroller);
            LowerRightPanel.Children.Add(StylePickerScroller);

            Border ButtonsBorder = new Border() 
            {
                Background = HexBrush("#222228"),
                BorderBrush = HexBrush("#595959"),
                BorderThickness = new Thickness(0,2,0,0),
            };
            Grid ButtonsGrid = new Grid();
            ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());

            Grid ButtonsRowGrid = new Grid();
            ButtonsRowGrid.RowDefinitions.Add(new RowDefinition());
            ButtonsRowGrid.RowDefinitions.Add(new RowDefinition());

            CButton SaveButton = new CButton("Save to Library", 20) { Margin = new Thickness(6, 6, 2, 2) };
            CButton DirectoryButton = new CButton("Show in Directory", 20) { Margin = new Thickness(6, 2, 2, 6) };
            CButton ExportButton = new CButton("Export", 50) { Margin = new Thickness(4, 6, 2, 6) };
            CButton ShowModelButton = new CButton("Show in\nModel Viewer", 20) { Margin = new Thickness(2, 6, 6, 6) };

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
            RelatedCosmeticsBorder.Background = HexBrush("#27282d");
            RelatedCosmeticsBorder.BorderBrush = HexBrush("#515156");
            RelatedCosmeticsBorder.BorderThickness = new Thickness(0,0,2,0);
            LowerPanel.Children.Add(RelatedCosmeticsBorder);

            CScrollViewer RelatedCosmeticsScroller = new CScrollViewer();
            StackPanel RelatedCosmeticsStack = new StackPanel();
            RelatedCosmeticsScroller.Content = RelatedCosmeticsStack;
            RelatedCosmeticsBorder.Child = RelatedCosmeticsScroller;
            RelatedCosmeticsStack.Margin = new Thickness(1);


            for (int i = 0; i < 20; i++)
            {
                Border CosmeticBorder = new Border();
                CosmeticBorder.SizeChanged += (sender, args) =>
                {
                    CosmeticBorder.Height = CosmeticBorder.Width;
                };
                
                CosmeticBorder.Margin = new Thickness(3);
                CosmeticBorder.Background = Theme.BackNormal;
                CosmeticBorder.BorderBrush = Theme.BorderNormal;
                CosmeticBorder.BorderThickness = new Thickness(2);

                Image CosmeticIcon = new Image() { Source = IconImage };
                CosmeticBorder.Child = CosmeticIcon;

                RelatedCosmeticsStack.Children.Add(CosmeticBorder);
            }

            Border TopBorder = new Border();

            Grid TopGrid = new Grid();
            TopBorder.Child = TopGrid;
            TopBorder.BorderThickness = new Thickness(0,0,0,2);
            TopBorder.BorderBrush = HexBrush("#454652");
            TopBorder.Background = HexBrush("#1a1b25");

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

            GameContentModule.SelectionChanged += (ExportPreviewInfo Item) =>
            {
                StylePickerPanel.Children.Clear();

                DisplayName.Text = Item.Name;
                if (Item.PreviewIcon.TryGet_BitmapImage(out BitmapImage Source))
                {
                    PreviewIcon.Source = Source;
                }

                for (int i = 0; i < Item.Styles.Count; i++)
                {
                    ExportPreviewSet style = Item.Styles[i];
                    Grid StyleSetPanel = new Grid();
                    Border StyleSetNameBorder = new Border()
                    {
                        Padding = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    StyleSetNameBorder.Child = new CTextBlock(style.Name, 20);


                    StylePickerPanel.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    Grid.SetRow(StyleSetNameBorder, i);
                    StylePickerPanel.Children.Add(StyleSetNameBorder);
                    StyleSetPanel.HorizontalAlignment = HorizontalAlignment.Left;

                    StyleSetWidget SetWidget = new StyleSetWidget(style);
                    Grid.SetRow(SetWidget, i);
                    Grid.SetColumn(SetWidget, 1);
                    StylePickerPanel.Children.Add(SetWidget);
                }
            };
        }

        // Only contains options not name
        public class StyleSetWidget : ContentControl
        {
            public StyleOptionWidget SelectedOption;

            public StyleSetWidget(ExportPreviewSet style)
            {
                WrapPanel StyleSetOptionsPanel = new WrapPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(3),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                Content = StyleSetOptionsPanel;

                for (int i = 0; i < style.Options.Count; i++)
                {
                    StyleOptionWidget OptionWidget = new StyleOptionWidget(style.Options[i], this);
                    if (i == 0)
                    {
                        SelectedOption = OptionWidget;
                        OptionWidget.Select();
                    }
                    StyleSetOptionsPanel.Children.Add(OptionWidget);
                }
            }
        }

        public class StyleOptionWidget : ContentControl
        {
            public StyleSetWidget Owner;
            public bool IsSelected => Owner.SelectedOption == this;

            Border StyleOptionBorder = new Border()
            {
                BorderBrush = Theme.BorderNormal,
                Background = Theme.BackNormal,
                BorderThickness = new Thickness(3),
                Width = 80,
                Height = 80,
                Margin = new Thickness(5),
            };

            public StyleOptionWidget(ExportPreviewOption option, StyleSetWidget owner)
            {
                Owner = owner;

                // Hover FX
                MouseEnter += (sender, args) => StyleOptionBorder.BorderBrush = IsSelected ? Theme.BorderSelected : Theme.BorderHover;
                MouseLeave += (sender, args) => StyleOptionBorder.BorderBrush = IsSelected ? Theme.BorderSelected : Theme.BorderNormal;

                MouseLeftButtonDown += (sender, args) => Select();

                StyleOptionBorder.ToolTip = new CTooltip(option.Name);
                if (option.Icon is TextureRef VTexture)
                {
                    if (VTexture.TryGet_BitmapImage(out BitmapImage bitmapImage))
                    {
                        Image StyleOptionImage = new Image() { Source = bitmapImage };
                        StyleOptionBorder.Child = StyleOptionImage;
                    }
                }

                Content = StyleOptionBorder;
            }

            public void Select()
            {
                Owner.SelectedOption.Deselect();
                Owner.SelectedOption = this;
                StyleOptionBorder.BorderBrush = Theme.BorderSelected;
            }

            void Deselect()
            {
                StyleOptionBorder.BorderBrush = Theme.BorderNormal;
            }
        }
    }
}
