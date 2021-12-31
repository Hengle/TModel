using System;
using System.Windows;
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
                if (IsEnabled)
                {
                    Background = ClickBrush;
                    Click?.Invoke();
                }
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

        CTextBlock? CTextBlock;

        public CButton(string text, double size = 20, Action clickEvent = null) : this()
        {
            Padding = new Thickness(5);
            if (clickEvent != null)
                Click += clickEvent;
            CTextBlock = new CTextBlock(text, size)
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };
            Child = CTextBlock;
        }

        public CButton(string text, Action? clickEvent) : this(text)
        {
            if (clickEvent != null)
                Click += clickEvent;
        }

        public void SetText(string newText)
        {
            if (CTextBlock != null)
                CTextBlock.Text = newText;
        }
    }
}
