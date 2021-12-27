using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using static CUE4Parse.Utils.StringUtils;

namespace TModel.Modules
{
    class ObjectViewerModule : ModuleBase
    {
        public override string ModuleName => "Object Viewer";

        public ObjectViewerModule() : base()
        {

        }

        public override void StartupModule()
        {
            ScrollViewer Scroller = new ScrollViewer();
            StackPanel ObjectPanel = new StackPanel();

            Scroller.Content = ObjectPanel;

            DirectoryModule.SelectedItemChanged += (IEnumerable<UObject>? Item) =>
            {
                ObjectPanel.Children.Clear();
                if (Item is IEnumerable<UObject> uObjects)
                {
                    foreach (UObject obj in uObjects)
                    {
                        StackPanel stackPanel = new StackPanel();
                        obj.GenerateWidget(stackPanel);
                        ObjectPanel.Children.Add(stackPanel);
                    }
                }
            };

            Content = Scroller;
        }
    }
}
