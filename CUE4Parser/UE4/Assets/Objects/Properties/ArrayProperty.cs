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