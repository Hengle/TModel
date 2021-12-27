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
            StackPanel Root = new StackPanel();

            DirectoryModule.SelectedItemChanged += (IEnumerable<UObject>? Item) =>
            {
                Root.Children.Clear();

                if (Item is IEnumerable<UObject> uObjects)
                {
                    foreach (UObject obj in uObjects)
                    {
                        UObjectWidget uobjectWidget = new UObjectWidget(obj);
                        Root.Children.Add(uobjectWidget);
                    }
                }
            };

            Content = Root;
        }

        public class UObjectWidget : ContentControl
        {
            StackPanel Root = new StackPanel();

            public UObjectWidget(UObject uObject)
            {
                CoreTextBlock NameText = new CoreTextBlock(uObject.Name, 40);
                PropertiesPanelWidget PropertiesPanel = new PropertiesPanelWidget(uObject.Properties);

                Root.Children.Add(NameText);
                Root.Children.Add(PropertiesPanel);

                Content = Root;
            }
        }

        public class PropertiesPanelWidget : ContentControl
        {
            StackPanel Root = new StackPanel();
            public PropertiesPanelWidget(List<FPropertyTag> properties)
            {
                foreach (var property in properties)
                {
                    PropertyWidget propertyWidget = new PropertyWidget(property);
                    Root.Children.Add(propertyWidget);
                }

                Content = Root;
            }
        }

        public class PropertyWidget : ContentControl
        {
            StackPanel Root = new StackPanel() { Orientation = Orientation.Horizontal };

            public PropertyWidget(FPropertyTag property)
            {
                CoreTextBlock TypeText = new CoreTextBlock(property.Tag.GenericValue.GetType().ToString().SubstringAfterLast('.'), 15, new Thickness(0, 0, 40, 0));
                CoreTextBlock NameText = new CoreTextBlock(property.Name, 15, new Thickness(0,0,40,0));
                PropertyValueWidget ValueWidget = new PropertyValueWidget(property.Tag);

                Root.Children.Add(TypeText);
                Root.Children.Add(NameText);
                Root.Children.Add(ValueWidget);

                Content = Root;
            }
        }

        public class PropertyValueWidget : ContentControl
        {
            public PropertyValueWidget(FPropertyTagType property)
            {
                CoreTextBlock ValueText = new CoreTextBlock(property.ToString());
                Content = ValueText;
            }
        }
    }
}
