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
    internal class GameContentModule : ModuleBase
    {
        public GameContentModule() : base()
        {
            ModuleName = "Game Content";
        }

        public override void StartupModule()
        {
            StackPanel stackPanel = new StackPanel() { Background = Brushes.DarkCyan };
            stackPanel.Children.Add(new TextBlock()
            {
                Text = "Game Content",
                FontSize = 60,
                FontFamily = new FontFamily("Microsoft Sans Serif Bold"),
                Foreground = Brushes.Black,
                Background = Brushes.LightBlue,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            WrapPanel wrapPanel = new WrapPanel() { Background = Brushes.DarkSlateGray, HorizontalAlignment = HorizontalAlignment.Center };
            for (int i = 0; i < 50; i++)
            {
                wrapPanel.Children.Add(new Border()
                {
                    Background = Brushes.Ivory,
                    BorderBrush = Brushes.Black,
                    Margin = new Thickness(10),
                    BorderThickness = new Thickness(4),
                    Width = 120,
                    Height = 120
                });
            }
            stackPanel.Children.Add(wrapPanel);
            Content = stackPanel;
        }
    }
}
