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
    class ItemPreviewModule : ModuleInterface
    {
        public override string ModuleName => "Item Preview";

        public override void StartupModule()
        {
            Border border = new Border();
            border.BorderThickness = new Thickness(8);
            border.BorderBrush = Brushes.DarkRed;
            border.Background = Brushes.DarkSalmon;
            border.Child = new TextBlock() { Text = "Item Previewer", FontSize = 70, Foreground = Brushes.SeaShell, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            Content = border;
        }
    }
}
