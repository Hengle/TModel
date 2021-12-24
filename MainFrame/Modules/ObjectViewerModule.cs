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

        public override void StartupModule()
        {
            ScrollViewer scrollViewer = new ScrollViewer();
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
                    }
                    if (Exports is IEnumerable<UObject> UObjects)
                    {
                        // Runs this code on the UI thread
                        App.Refresh(() =>
                        {
                            foreach (var uObject in UObjects)
                            {
                                ObjectsPanel.Children.Add(new TextBlock() 
                                {
                                    Text = uObject.Name,
                                    Style = new DefaultText(25),
                                });


                                if (uObject.Properties.Count > 0)
                                {
                                    foreach (var property in uObject.Properties)
                                    {
                                        StackPanel PropertyData = new StackPanel() { Orientation = Orientation.Horizontal };

                                        // Property type
                                        PropertyData.Children.Add(new TextBlock()
                                        {
                                            Text = property.Tag?.GenericValue?.GetType().ToString().SubstringAfterLast('.') ?? "NO TYPE",
                                            Style = new DefaultText(),
                                            Foreground = HexBrush("#7277ff"),
                                            Margin = new Thickness(5),
                                        });

                                        // Name of property
                                        PropertyData.Children.Add(new TextBlock()
                                        {
                                            Text = property.Name.Text,
                                            Style = new DefaultText(),
                                            FontSize = 15,
                                            Foreground = HexBrush("#c0dbff"),
                                            Margin = new Thickness(5),
                                        });

                                        // Value of property
                                        PropertyData.Children.Add(new TextBlock()
                                        {
                                            Text = property.Tag?.ToString() ?? "NO VALUE",
                                            Style = new DefaultText(),
                                            Foreground = HexBrush("#d59eff"),
                                            Margin = new Thickness(5),
                                        });

                                        ObjectsPanel.Children.Add(PropertyData);
                                    }
                                }
                                else
                                {
                                    ObjectsPanel.Children.Add(new TextBlock()
                                    {
                                        Text = "Object has none properties",
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
