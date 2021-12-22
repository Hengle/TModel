using System;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Objects.UObject
{
    [SkipObjectRegistration]
    public class UStruct : UField
    {
        public FPackageIndex SuperStruct;
        public FPackageIndex[] Children;
        public FField[] ChildProperties;

        public byte[]? Script;

        public override string ToString() => Name;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            SuperStruct = new FPackageIndex(Ar);
            if (FFrameworkObjectVersion.Get(Ar) < FFrameworkObjectVersion.Type.RemoveUField_Next)
            {
                var firstChild = new FPackageIndex(Ar);
                Children = firstChild.IsNull ? Array.Empty<FPackageIndex>() : new[] { firstChild };
            }
            else
            {
                Children = Ar.ReadArray(() => new FPackageIndex(Ar));
            }

            if (FCoreObjectVersion.Get(Ar) >= FCoreObjectVersion.Type.FProperties)
            {
                DeserializeProperties(Ar);
            }

            var bytecodeBufferSize = Ar.Read<int>();
            var serializedScriptSize = Ar.Read<int>();
            Script = Ar.ReadBytes(serializedScriptSize);
        }

        protected void DeserializeProperties(FAssetArchive Ar)
        {
            ChildProperties = Ar.ReadArray(() =>
            {
                var propertyTypeName = Ar.ReadFName();
                var prop = FField.Construct(propertyTypeName);
                prop.Deserialize(Ar);
                return prop;
            });
        }
    }
}