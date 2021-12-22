using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.Versions;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using UExport = CUE4Parse.UE4.Assets.Exports.UObject;

namespace CUE4Parse.UE4.Objects.UObject
{
    [JsonConverter(typeof(FSoftObjectPathConverter))]

    public readonly struct FSoftObjectPath : IUStruct
    {
        /** Asset path, patch to a top level object in a package. This is /package/path.assetname */
        public readonly FName AssetPathName;
        /** Optional FString for subobject within an asset. This is the sub path after the : */
        public readonly string SubPathString;

        public readonly string Normalized => StringUtils.NormalizePath(AssetPathName);

        public readonly AbstractUePackage? Owner;

        public FSoftObjectPath(FAssetArchive Ar)
        {
            if (Ar.Ver < EUnrealEngineObjectUE4Version.ADDED_SOFT_OBJECT_PATH)
            {
                var path = Ar.ReadFString();
                throw new ParserException(Ar, $"Asset path \"{path}\" is in short form and is not supported, nor recommended");
            }
            
            AssetPathName = Ar.ReadFName();
            SubPathString = Ar.ReadFString();
            Owner = Ar.Owner;
        }

        public FSoftObjectPath(FName assetPathName, string subPathString, AbstractUePackage? owner = null)
        {
            AssetPathName = assetPathName;
            SubPathString = subPathString;
            Owner = owner;
        }

        #region Loading Methods


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Trys to load the object as the specified type
        public T Load<T>() where T : UExport => FileProvider.FileProvider.LoadObject(AssetPathName.Text) as T;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Loads the object
        public UExport Load() => FileProvider.FileProvider.LoadObject(AssetPathName.Text);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(out UExport export) =>
            FileProvider.FileProvider.TryLoadObject(AssetPathName.Text, out export);
        
        #endregion

        public static implicit operator string(FSoftObjectPath value) => value.Normalized;

        public static bool operator ==(FSoftObjectPath a, FSoftObjectPath b) => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase) == 0;

        public static bool operator !=(FSoftObjectPath a, FSoftObjectPath b) => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase) != 0;

        public static bool operator ==(string a, FSoftObjectPath b) => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase) == 0;

        public static bool operator !=(string a, FSoftObjectPath b) => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase) != 0;

        public override string ToString() => Normalized;
    }
    
    public class FSoftObjectPathConverter : JsonConverter<FSoftObjectPath>
    {
        public override void WriteJson(JsonWriter writer, FSoftObjectPath value, JsonSerializer serializer)
        {
            /*var path = value.ToString();
            writer.WriteValue(path.Length > 0 ? path : "None");*/
            writer.WriteStartObject();
            
            writer.WritePropertyName("AssetPathName");
            serializer.Serialize(writer, value.AssetPathName);
            
            writer.WritePropertyName("SubPathString");
            writer.WriteValue(value.SubPathString);
            
            writer.WriteEndObject();
        }

        public override FSoftObjectPath ReadJson(JsonReader reader, Type objectType, FSoftObjectPath existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
