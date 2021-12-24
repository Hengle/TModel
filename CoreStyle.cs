using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static TModel.ColorConverters;
using static TModel.CoreStyle;

namespace TModel
{
    public static class CoreStyle
    {
        public static class CoreColors
        {
            public static Brush White { get; } = HexBrush("#ffffff");
        }

        public static class STextBlock
        {

        }
    }

    public class DefaultText : Style
    {
        public DefaultText(double size = 15)
        {
            Setters.Add(new Setter(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI")));
            Setters.Add(new Setter(TextBlock.FontSizeProperty, size));
            Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
            Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.White));
        }
    }
}
