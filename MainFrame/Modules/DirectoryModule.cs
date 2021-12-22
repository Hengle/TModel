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
    internal class DirectoryModule : ModuleBase
    {
        public DirectoryModule() : base()
        {
            ModuleName = "Directory";
        }

        public override void StartupModule()
        {
            Border border = new Border();
            border.BorderThickness = new Thickness(8);
            border.BorderBrush = Brushes.Coral;
            border.Background = Brushes.DarkCyan;
            border.Child = new TextBlock() { Text = "Directory", FontSize = 50, Foreground = Brushes.Black, FontFamily = new FontFamily("Microsoft Sans Serif Bold"), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            Content = border;
        }
    }
}
