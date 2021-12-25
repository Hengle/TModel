using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using Newtonsoft.Json;
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

        public static FrameworkElement GeneratePropertiesUI(FPropertyTag[] Properties)
        {
            Border Root = new Border() 
            {
                Background = HexBrush("#333333"),
                BorderBrush = HexBrush("#686868"),
                BorderThickness = new Thickness(3,0,0,0),
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
                StackPanel PropertyPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                StackRoot.Children.Add(PropertyPanel);
                PreviewOverrideData? CustomData = null;

                if (property.Tag is IPreviewOverride DataPreview)
                {
                    CustomData = DataPreview.GetCustomData();
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
                var ValueElement = GenerateValueIU(property.Tag, out bool IsCustom);
                if (IsCustom) // Vertically
                {
                    StackRoot.Children.Add(ValueElement);
                }
                else // Horizontally
                    PropertyPanel.Children.Add(ValueElement);
                stackPanel.Children.Add(StackRoot);
            }

            return Root;
        }

        public static UIElement GenerateValueIU(FPropertyTagType? value, out bool IsCustom)
        {
            PreviewOverrideData? CustomData = null;

            FrameworkElement Result = new FrameworkElement();

            if (value is IPreviewOverride DataPreview)
            {
                CustomData = DataPreview.GetCustomData();
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
                    Text = value?.ToString() ?? "NO VALUE",
                    Style = new DefaultText(),
                    Foreground = HexBrush("#d59eff"),
                    Margin = new Thickness(5),
                };
            }

            IsCustom = CustomData?.OverrideElement is FrameworkElement;
            return Result;
        }

        public override void StartupModule()
        {
            ScrollViewer scrollViewer = new ScrollViewer() { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel ObjectsPanel = new StackPanel() { Background = HexBrush("#1b1b1b") };
            scrollViewer.Content = ObjectsPanel;
            Content = scrollViewer;
            DirectoryModule.SelectedItemChanged += (name) =>
            {
                ObjectsPanel.Children.Clear();
                Task.Run(() =>
                {
                    IEnumerable<UObject>? Exports = null;
                    try
                    {
                        Exports = App.FileProvider.LoadObjectExports(name);
                    }
                    catch
                    {
                        App.Refresh(() => ObjectsPanel.Children.Add(new TextBlock() { Style = new DefaultText(60), Foreground = HexBrush("#ff1f1f"), Text = "Failed to load package" }));
                    }
                    if (Exports is IEnumerable<UObject> UObjects)
                    {
                        // Runs this code on the UI thread
                        App.Refresh(() =>
                        {
                            foreach (var uObject in UObjects)
                            {
                                ObjectsPanel.Children.Add(new ReadonlyText(25) 
                                {
                                    Text = uObject.Name
                                });


                                if (uObject.Properties.Count > 0)
                                {
                                    FrameworkElement uIElement = GeneratePropertiesUI(uObject.Properties.ToArray());
                                    uIElement.Margin = new Thickness(30);
                                    ObjectsPanel.Children.Add(uIElement);
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
                });
            };
        }
    }
}
