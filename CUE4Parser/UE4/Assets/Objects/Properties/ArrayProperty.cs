using CUE4Parse.UE4.Assets.Readers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(ArrayPropertyConverter))]
    public class ArrayProperty : FPropertyTagType<UScriptArray>
    {
        public ArrayProperty(FAssetArchive Ar, FPropertyTagData? tagData, ReadType type)
        {
            Value = type switch
            {
                ReadType.ZERO => new UScriptArray(tagData?.InnerType ?? "ZeroUnknown"),
                _ => new UScriptArray(Ar, tagData)
            };

            List<FPropertyTagType> FoundProperties = ((UScriptArray)Value).Properties;
            if (FoundProperties.Count > 0)
            {
                if (FoundProperties[0] is StructProperty StructCast)
                {
                    PropertyTypeName = StructCast.PropertyTypeName;
                }
                else
                {
                    PropertyTypeName = FoundProperties[0].GetType().ToString();
                }
            }
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