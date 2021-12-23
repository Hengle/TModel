using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static TModel.ColorConverters;

namespace TModel
{
    public static class CoreStyle
    {
        public static class CoreColors
        {
            public static Brush White { get; } = HexBrush("#ffffff");
        }

        public static class SButton
        {
            public static Style Default { get; } = new _base();

            private class _base : Style
            {
                public _base()
                {
                    Setters.Add(new Setter(Button.BackgroundProperty, HexBrush("0b2750")));
                    Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(2,2,0,0)));
                    Setters.Add(new Setter(Button.BorderBrushProperty, HexBrush("1f57a8")));

                    Trigger EnabledTrigger = new Trigger();
                    EnabledTrigger.Property = Button.IsMouseOverProperty;
                    EnabledTrigger.Value = true;

                    EnabledTrigger.Setters.Add(new Setter(Button.BackgroundProperty, HexBrush("f50204")));

                    Triggers.Add(EnabledTrigger);
                }
            }
        }

        public static class STextBlock
        {
            public static Style DefaultSmall { get; } = new _default(12);
            public static Style DefaultMedium { get; } = new _default(20);
            public static Style DefaultLarge { get; } = new _default(30);

            private class _base : Style
            {
                public _base(double size)
                {
                    Setters.Add(new Setter(TextBlock.FontFamilyProperty, new FontFamily("Microsoft Sans Serif Bold")));
                    Setters.Add(new Setter(TextBlock.FontSizeProperty, size));
                    Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                }
            }

            private class _default : _base
            {
                public _default(double size) : base(size)
                {
                    Setters.Add(new Setter(TextBlock.ForegroundProperty, CoreColors.White));
                }
            }
        }
    }
}
