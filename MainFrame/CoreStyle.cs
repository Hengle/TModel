using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static TModel.ColorConverters;
using static TModel.CoreStyle;

namespace TModel
{
    public static class CoreStyle
    {
        public static FontFamily CoreFont = new FontFamily("Segoe UI");
    }

    public class CoreTextBlock : TextBlock
    {
        public CoreTextBlock(string text, double size = 15)
        {
            Text = text;
            Style = new DefaultText(15);
        }
    }

    public class ReadonlyText : TextBox
    {
        public ReadonlyText(double size = 15)
        {
            IsReadOnly = true;
            Style = new DefaultText(size);
            Background = Brushes.Transparent;
            BorderThickness = new Thickness(0);
        }

        public ReadonlyText(string text, double size = 15) : this(size)
        {
            Text = text;
        }
    }

    // TODO: make this inherit from TextBlock
    public class DefaultText : Style
    {
        public DefaultText(double size = 15)
        {
            Setters.Add(new Setter(TextBlock.FontFamilyProperty, CoreFont));
            Setters.Add(new Setter(TextBlock.FontSizeProperty, size));
            Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
            Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.White));
        }
    }
}
