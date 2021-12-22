using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            Border border = new Border();
            border.BorderThickness = new Thickness(8);
            border.BorderBrush = Brushes.Brown;
            border.Background = Brushes.DeepPink;
            border.Child = new TextBlock() { Text = "Objector", FontFamily = new FontFamily("Microsoft Sans Serif Bold"), FontSize = 80, Foreground = Brushes.SandyBrown, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            Content = border;
        }
    }
}
