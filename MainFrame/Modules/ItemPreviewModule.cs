﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TModel.Modules
{
    class ItemPreviewModule : ModuleBase
    {
        public ItemPreviewModule()
        {
            ModuleName = "Item Preview";
        }

        public override void StartupModule()
        {
            Border border = new Border();
            border.BorderThickness = new Thickness(8);
            border.BorderBrush = Brushes.DarkRed;
            border.Background = Brushes.DarkSalmon;
            border.Child = new TextBlock() { Text = "Item Previewer", FontSize = 70, FontFamily = new FontFamily("Microsoft Sans Serif Bold"), Foreground = Brushes.SeaShell, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            Content = border;
        }
    }
}