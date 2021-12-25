using System;
using System.Windows;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using TModel;
using TModel.Modules;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(StructPropertyConverter))]
    public class StructProperty : FPropertyTagType<UScriptStruct>, IPreviewOverride
    {
        public StructProperty(FAssetArchive Ar, FPropertyTagData? tagData, ReadType type)
        {
            Value = new UScriptStruct(Ar, tagData?.StructType, tagData?.Struct, type);
        }

        public PreviewOverrideData GetCustomData()
        {
            FrameworkElement? OverrideUI = null;
            if (Value.StructType is FStructFallback fallback)
            {
                OverrideUI = ObjectViewerModule.GeneratePropertiesUI(fallback.Properties.ToArray());
            }

            return new PreviewOverrideData() { OverrideElement = OverrideUI, OverrideTypeName = $"{Value.StructName} (Struct)" };
        }

        public override string ToString() => Value.ToString().SubstringBeforeLast(')');
    }

    public class StructPropertyConverter : JsonConverter<StructProperty>
    {
        public override void WriteJson(JsonWriter writer, StructProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override StructProperty ReadJson(JsonReader reader, Type objectType, StructProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}