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

    public class CoreTextBox : TextBox
    {
        public CoreTextBox()
        {
            FontFamily = CoreFont;
            Foreground = Brushes.White;
            Background = HexBrush("#0f1243");
            FontSize = 15;
            VerticalContentAlignment = VerticalAlignment.Center;
        }
    }

    public class CTooltip : ToolTip
    {
        static Brush background = HexBrush("#070032");
        static Brush Border = HexBrush("#412db3");
        public CTooltip(string text)
        {
            Content = new CoreTextBlock(text);
            Background = background;
            Padding = new Thickness(5);
            BorderThickness = new Thickness(1   );
            BorderBrush = Border;
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        }
    }

    public class CoreTextBlock : TextBlock
    {
        public CoreTextBlock(string text, double size = 15, Thickness? margin = null)
        {
            Text = text;
            Foreground = Brushes.White;
            Margin = margin ?? new Thickness(0);
            FontSize = size;
        }

        public CoreTextBlock() : this("")
        {

        }
    }

    public class ReadonlyText : TextBox
    {
        public ReadonlyText(double size = 15)
        {
            FontSize = size;
            IsReadOnly = true;
            Foreground = Brushes.White;
            Background = Brushes.Transparent;
            BorderThickness = new Thickness(0);
        }

        public ReadonlyText(string text, double size = 15) : this(size)
        {
            Text = text;
        }
    }
}
