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
            TextBlock textBlock = new TextBlock() { Style = CoreStyle.STextBlock.DefaultSmall };
            textBlock.Background = Brushes.Black;
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Content = textBlock;
            Content = scrollViewer;
            DirectoryModule.SelectedItemChanged += (name) =>
            {
                string Final = "";
                textBlock.Text = "";
                Task LoadObject = new Task(() =>
                {
                    try
                    {
                        IEnumerable<UObject> Exports = App.FileProvider.LoadObjectExports(name);
                        Final = JsonConvert.SerializeObject(Exports, Formatting.Indented);
                    }
                    catch
                    {
                        Final = "FAILED";
                    }

                });
                LoadObject.GetAwaiter().OnCompleted(() => textBlock.Text = Final);
                LoadObject.Start();
            };

            Background = Brushes.Black;
        }
    }
}
