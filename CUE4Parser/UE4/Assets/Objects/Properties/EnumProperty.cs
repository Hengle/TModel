using System;
using System.Collections.Generic;
using System.Windows.Controls;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using TModel;
using static TModel.ColorConverters;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(EnumPropertyConverter))]
    public class EnumProperty : FPropertyTagType<EnumValue>, IPreviewOverride
    {
        public EnumProperty(FAssetArchive Ar, FPropertyTagData? tagData, ReadType type)
        {
            if (type == ReadType.ZERO)
            {
                Value = IndexToEnum(Ar, tagData, 0);
            }
            else if (Ar.HasUnversionedProperties && type == ReadType.NORMAL)
            {
                var index = 0;
                if (tagData?.InnerType != null)
                {
                    var underlyingProp = ReadPropertyTagType(Ar, tagData.InnerType, tagData.InnerTypeData, ReadType.NORMAL)?.GenericValue;
                    if (underlyingProp != null && underlyingProp.IsNumericType())
                        index = Convert.ToInt32(underlyingProp);
                }
                else
                {
                    index = Ar.Read<byte>();
                }
                Value = IndexToEnum(Ar, tagData, index);
            }
            else
            {
                Value = new EnumValue() { SelectedName = Ar.ReadFName().Text };
            }
        }

        private static EnumValue IndexToEnum(FAssetArchive Ar, FPropertyTagData? tagData, int index)
        {
            var enumName = tagData?.EnumName;
            if (enumName == null)
                return new EnumValue()
                {
                    SelectedIndex = index,
                };

            if (tagData.Enum != null) // serialized
            {
                foreach (var (name, value) in tagData.Enum.Names)
                    if (value == index)
                        return new EnumValue()
                        {
                            EnumName = enumName,
                            SelectedIndex = index,
                            SelectedName = name.Text,
                        };

                return new EnumValue()
                {
                    EnumName = enumName,
                    SelectedIndex = index,
                };
            }

            if (Ar.Owner.Mappings != null)
            {
                if (Ar.Owner.Mappings.Enums.TryGetValue(enumName, out Dictionary<int, string> values))
                {
                    if (values.TryGetValue(index, out var member))
                    {
                        return new EnumValue()
                        {
                            EnumName = enumName,
                            SelectedIndex = index,
                            SelectedName = member,
                            Values = values.Values,
                        };
                    }
                }
            }
            return new EnumValue()
            {
                EnumName = enumName,
                SelectedIndex = index
            };
        }

        public PreviewOverrideData GetCustomData()
        {
            StackPanel stackPanel = new StackPanel();
            int Counter = 0;
            foreach (string item in Value.Values)
            {
                stackPanel.Children.Add(new ReadonlyText(item) { Foreground = Counter == Value.SelectedIndex ? HexBrush("#fa422a") : HexBrush("#bee757") });
                Counter++;
            }

            return new PreviewOverrideData() { OverrideTypeName = $"{Value.EnumName} (Enum)", OverrideElement = stackPanel };
        }
    }

    public class EnumValue
    {
        public ICollection<string>? Values { get; set; }
        public string? EnumName { get; set; }
        public string? SelectedName { get; set; }
        public int SelectedIndex { get; set; }

        public EnumValue() { }

        public override string ToString()
        {
            string ValuesString = "";
            if (Values != null)
                foreach (var item in Values)
                {
                    ValuesString += item + "\n";
                }
            else
                ValuesString = "Couldnt find values";

            return SelectedName;
        }
    }

    public class EnumPropertyConverter : JsonConverter<EnumProperty>
    {
        public override void WriteJson(JsonWriter writer, EnumProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override EnumProperty ReadJson(JsonReader reader, Type objectType, EnumProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}