using System;
using System.Windows.Controls;
using System.Windows.Media;
using static TModel.ColorConverters;

namespace TModel.MainFrame.Widgets
{
    internal class CButton : Border
    {
        static Brush NormalBrush = HexBrush("#08265f");
        static Brush HoverBrush = HexBrush("#06358c");
        static Brush ClickBrush = HexBrush("#1453c6");

        public Action? Click { get; set; }

        public CButton(Brush? normal = null, Brush? hover = null, Brush? click = null)
        {
            NormalBrush = normal ?? NormalBrush;
            HoverBrush = hover ?? HoverBrush;
            ClickBrush = click ?? ClickBrush;

            Background = NormalBrush;
            BorderBrush = HexBrush("#18469c");
            BorderThickness = new System.Windows.Thickness(2);



            MouseLeftButtonDown += (sender, args) => 
            {
                Background = ClickBrush;
                Click?.Invoke();
            };

            MouseLeftButtonUp += (sender, args) =>
            {
                Background = HoverBrush;
            };

            MouseEnter += (sender,args) => 
            {
                Background = HoverBrush;
            };

            MouseLeave += (sender, args) =>
            {
                Background = NormalBrush;
            };
        }

        public CButton(string text, double size = 20, Action clickEvent = null) : this()
        {
            if (clickEvent != null)
                Click += clickEvent;
            Child = new TextBlock()
            {
                Text = text,
                Style = new DefaultText(size),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };
        }

        public CButton(string text, Action? clickEvent) : this(text)
        {
            if (clickEvent != null)
                Click += clickEvent;
        }
    }
}
