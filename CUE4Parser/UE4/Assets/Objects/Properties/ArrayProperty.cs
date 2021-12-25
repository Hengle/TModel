using CUE4Parse.UE4.Assets.Readers;
using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Controls;
using TModel;
using TModel.Modules;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(ArrayPropertyConverter))]
    public class ArrayProperty : FPropertyTagType<UScriptArray>, IPreviewOverride
    {
        public ArrayProperty(FAssetArchive Ar, FPropertyTagData? tagData, ReadType type)
        {
            Value = type switch
            {
                ReadType.ZERO => new UScriptArray(tagData?.InnerType ?? "ZeroUnknown"),
                _ => new UScriptArray(Ar, tagData)
            };
        }

        public PreviewOverrideData GetCustomData(object data)
        {
            StackPanel ArrayValues = new StackPanel();
            int Counter = 1;
            foreach (var item in Value.Properties)
            {
                WrapPanel PropertyPanel = new WrapPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0,20,0,0) };

                PropertyPanel.Children.Add(new ReadonlyText(18)
                {
                    Text = Counter.ToString(),
                    Margin = new Thickness(5),
                    MinWidth = 5,
                });
                PropertyPanel.Children.Add(ObjectViewerModule.GenerateValueUI(item, out _, (ObjectViewerModule)data));
                Counter++;

                ArrayValues.Children.Add(PropertyPanel);
            }

            return new PreviewOverrideData() { OverrideTypeName = $"{Value.InnerType}[{Value.Properties.Count}]", OverrideElement = ArrayValues };
        }
    }

    public class ArrayPropertyConverter : JsonConverter<ArrayProperty>
    {
        public override void WriteJson(JsonWriter writer, ArrayProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override ArrayProperty ReadJson(JsonReader reader, Type objectType, ArrayProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}