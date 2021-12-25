using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using Newtonsoft.Json;
using TModel.MainFrame.Widgets;
using static CUE4Parse.Utils.StringUtils;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    class ObjectViewerModule : ModuleBase
    {
        public ObjectViewerModule() : base()
        {
            ModuleName = "Object Viewer";
        }

        public static Border GeneratePropertiesUI(FPropertyTag[] Properties, ObjectViewerModule owner)
        {
            Border Root = new Border()
            {
                Background = HexBrush("#333333"),
                BorderBrush = HexBrush("#686868"),
                BorderThickness = new Thickness(3, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            StackPanel stackPanel = new StackPanel() 
            {
                Margin = new Thickness(6, 2, 2, 2)
            };

            Root.Child = stackPanel;

            int Counter = 0;
            foreach (FPropertyTag? property in Properties)
            {
                Counter++;

                StackPanel StackRoot = new StackPanel();
                WrapPanel PropertyPanel = new WrapPanel() 
                { 
                    Orientation = Orientation.Horizontal, 
                    Margin = new Thickness(3),
                };
                StackRoot.Children.Add(PropertyPanel);
                PreviewOverrideData? CustomData = null;

                if (property.Tag is IPreviewOverride DataPreview)
                {
                    CustomData = DataPreview.GetCustomData(owner);
                }

                // Property Type
                PropertyPanel.Children.Add(new ReadonlyText(15)
                {
                    Text = CustomData?.OverrideTypeName ?? property.Tag?.GenericValue?.GetType().ToString().SubstringAfterLast('.') ?? "NO TYPE",
                    Foreground = HexBrush("#7277ff"),
                    Margin = new Thickness(5),
                });

                // Property Name
                PropertyPanel.Children.Add(new ReadonlyText(15)
                {
                    Text = property.Name.Text,
                    Style = new DefaultText(),
                    Foreground = HexBrush("#c0dbff"),
                    Margin = new Thickness(5),
                });

                // Property Value
                FrameworkElement ValueElement = GenerateValueUI(property.Tag, out bool IsCustom, owner);
                if (IsCustom) // Vertically
                {
                    Grid DropdownGrid = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0) };
                    DropdownGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
                    DropdownGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    // Dropdown button
                    CButton cButton = new CButton()
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Width = 20,
                        Height = 20,
                    };
                    cButton.Click += () =>
                    {
                        ValueElement.Visibility = ValueElement.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                    };

                    ValueElement.Margin = new Thickness(5);

                    DropdownGrid.Children.Add(cButton);

                    Grid.SetColumn(ValueElement, 1);
                    DropdownGrid.Children.Add(ValueElement);

                    StackRoot.Children.Add(DropdownGrid);
                }
                else // Horizontally
                    PropertyPanel.Children.Add(ValueElement);
                stackPanel.Children.Add(StackRoot);
            }

            return Root;
        }

        public static FrameworkElement GenerateValueUI(FPropertyTagType? value, out bool IsCustom, ObjectViewerModule owner)
        {

            PreviewOverrideData? CustomData = null;
            FrameworkElement Result = new FrameworkElement();


            if (value is IPreviewOverride DataPreview)
            {
                CustomData = DataPreview.GetCustomData(owner);
            }

            if (CustomData?.OverrideElement is FrameworkElement ValidElement)
            {
                Result = ValidElement;
                Result.Margin = new Thickness(40,0,0,0);
            }
            else
            {
                Result = new ReadonlyText(15)
                {
                    Text = value?.ToString() ?? "NULL",
                    Style = new DefaultText(),
                    Foreground = HexBrush("#d59eff"),
                    Margin = new Thickness(0),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
            }

            IsCustom = CustomData?.OverrideElement is not null;

            return Result;
        }

        public ScrollViewer scrollViewer { get; } = new ScrollViewer() { Background = HexBrush("#1b1b1b"), VerticalScrollBarVisibility = ScrollBarVisibility.Visible, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };

        public override void StartupModule()
        {
            StackPanel ObjectsPanel = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Left };
            scrollViewer.Content = ObjectsPanel;
            Content = scrollViewer;
            DirectoryModule.SelectedItemChanged += (Exports) =>
            {
                ObjectsPanel.Children.Clear();
                Task.Run(() =>
                {
                    if (Exports is IEnumerable<UObject> UObjects)
                    {
                        // Runs this code on the UI thread
                        App.Refresh(() =>
                        {
                            foreach (var uObject in UObjects)
                            {
                                ObjectsPanel.Children.Add(new ReadonlyText($"Name: {uObject.Name}", 35));

                                if (uObject.Class != null)
                                    ObjectsPanel.Children.Add(new ReadonlyText($"Class: {uObject.Class.Name}", 22));
                                if (uObject.Outer != null)
                                    ObjectsPanel.Children.Add(new ReadonlyText($"Outer: {uObject.Outer.Name}", 22));
                                if (uObject.Outer != null && uObject.Outer.Outer != null)
                                    ObjectsPanel.Children.Add(new ReadonlyText($"Outer-Outer: {uObject.Outer.Outer.Name}", 22));
                                if (uObject.Super != null)
                                    ObjectsPanel.Children.Add(new ReadonlyText($"Super: {uObject.Super.Name}", 22));
                                if (uObject.Template != null)
                                    ObjectsPanel.Children.Add(new ReadonlyText($"Template: {uObject.Template.Name}", 22));

                                if (uObject.Properties.Count > 0)
                                {
                                    Border PropertiesUI = GeneratePropertiesUI(uObject.Properties.ToArray(), this);
                                    PropertiesUI.Margin = new Thickness(30);
                                    ObjectsPanel.Children.Add(PropertiesUI);
                                }
                                else
                                {
                                    ObjectsPanel.Children.Add(new TextBlock()
                                    {
                                        Text = "Object has no properties",
                                        Style = new DefaultText(18),
                                        Foreground = HexBrush("#eb5f5f"),
                                        Margin = new Thickness(5),
                                    });
                                }
                            }
                        });
                    }
                    else
                    {
                        App.Refresh(() => ObjectsPanel.Children.Add(new TextBlock()
                        {
                            Style = new DefaultText(60),
                            Foreground = HexBrush("#ff1f1f"),
                            Text = "Failed to load package"
                        }));
                    }
                });
            };
        }
    }
}
