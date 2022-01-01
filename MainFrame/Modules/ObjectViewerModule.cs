#if !RELEASE

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using TModel.MainFrame.Widgets;
using static CUE4Parse.Utils.StringUtils;

namespace TModel.Modules
{
    class ObjectViewerModule : ModuleBase
    {
        public override string ModuleName => "Object Viewer";

        public ObjectViewerModule() : base()
        {
            CScrollViewer Scroller = new CScrollViewer();
            StackPanel ObjectPanel = new StackPanel();

            Scroller.Background = Theme.BackDark;

            Scroller.Content = ObjectPanel;

            DirectoryModule.SelectedItemChanged += (IEnumerable<UObject>? Item) =>
            {
                StackPanel stackPanel = new StackPanel();
                ObjectPanel.Children.Clear();
                if (Item is IEnumerable<UObject> uObjects)
                {
                    foreach (UObject obj in uObjects)
                    {
                        UIElement ObjWidget = obj.GenerateWidget();
                        ContentControl Frame = new ContentControl();
                        Frame.Margin = new Thickness(0, 20, 0, 20);
                        Frame.Content = ObjWidget;
                        ObjectPanel.Children.Add(Frame);
                    }
                }
                else
                {
                    ObjectPanel.Children.Add(new CTextBlock("Failed", 60));
                }
            };

            Content = Scroller;
        }
    }
}

#endif