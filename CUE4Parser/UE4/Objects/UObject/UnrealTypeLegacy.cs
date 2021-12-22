using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Objects.UObject
{
    public class UProperty : UField
    {
        public int ArrayDim;
        public ulong PropertyFlags;
        public FName RepNotifyFunc;
        public ELifetimeCondition BlueprintReplicationCondition;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            ArrayDim = Ar.Read<int>();
            PropertyFlags = Ar.Read<ulong>();
            RepNotifyFunc = Ar.ReadFName();
            if (FReleaseObjectVersion.Get(Ar) >= FReleaseObjectVersion.Type.PropertiesSerializeRepCondition)
            {
                BlueprintReplicationCondition = (ELifetimeCondition) Ar.Read<byte>();
            }
        }
    }

    public class UNumericProperty : UProperty { }

    public class UByteProperty : UNumericProperty
    {
        public FPackageIndex Enum; // UEnum

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            Enum = new FPackageIndex(Ar);
        }
    }

    public class UInt8Property : UNumericProperty { }

    public class UInt16Property : UNumericProperty { }

    public class UIntProperty : UNumericProperty { }

    public class UInt64Property : UNumericProperty { }

    public class UUInt16Property : UNumericProperty { }

    public class UUInt32Property : UNumericProperty { }

    public class UUInt64Property : UNumericProperty { }

    public class UFloatProperty : UNumericProperty { }

    public class UDoubleProperty : UNumericProperty { }

    public class UBoolProperty : UProperty
    {
        public byte BoolSize;
        public bool bIsNativeBool;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            BoolSize = Ar.Read<byte>();
            bIsNativeBool = Ar.ReadFlag();
        }
    }

    public class UObjectPropertyBase : UProperty
    {
        public FPackageIndex PropertyClass; // UClass

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            PropertyClass = new FPackageIndex(Ar);
        }
    }

    public class UObjectProperty : UObjectPropertyBase { }

    public class UWeakObjectProperty : UObjectPropertyBase { }

    public class ULazyObjectProperty : UObjectPropertyBase { }

    public class USoftObjectProperty : UObjectPropertyBase { }

    public class UAssetObjectProperty : UObjectPropertyBase { }

    public class UAssetClassProperty : UObjectPropertyBase
    {
        public FPackageIndex MetaClass; // UClass

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            MetaClass = new FPackageIndex(Ar);
        }
    }

    public class UClassProperty : UObjectProperty
    {
        public FPackageIndex MetaClass; // UClass

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            MetaClass = new FPackageIndex(Ar);
        }
    }

    public class USoftClassProperty : UObjectPropertyBase
    {
        public FPackageIndex MetaClass; // UClass

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            MetaClass = new FPackageIndex(Ar);
        }
    }

    public class UInterfaceProperty : UProperty
    {
        public FPackageIndex InterfaceClass; // UClass

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            InterfaceClass = new FPackageIndex(Ar);
        }
    }

    public class UNameProperty : UProperty { }

    public class UStrProperty : UProperty { }

    public class UArrayProperty : UProperty
    {
        public FPackageIndex Inner; // UProperty

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            Inner = new FPackageIndex(Ar);
        }
    }

    public class UMapProperty : UProperty
    {
        public FPackageIndex KeyProp; // UProperty
        public FPackageIndex ValueProp; // UProperty

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            KeyProp = new FPackageIndex(Ar);
            ValueProp = new FPackageIndex(Ar);
        }
    }

    public class USetProperty : UProperty
    {
        public FPackageIndex ElementProp; // UProperty

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            ElementProp = new FPackageIndex(Ar);
        }
    }

    public class UStructProperty : UProperty
    {
        public FPackageIndex Struct; // UScriptStruct

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            Struct = new FPackageIndex(Ar);
        }
    }

    public class UDelegateProperty : UProperty
    {
        public FPackageIndex SignatureFunction; // UFunction

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            SignatureFunction = new FPackageIndex(Ar);
        }
    }

    public class UMulticastDelegateProperty : UProperty
    {
        public FPackageIndex SignatureFunction; // UFunction

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            SignatureFunction = new FPackageIndex(Ar);
        }
    }

    public class UMulticastInlineDelegateProperty : UMulticastDelegateProperty { }

    public class UMulticastSparseDelegateProperty : UMulticastDelegateProperty { }

    public class UEnumProperty : UProperty
    {
        public FPackageIndex UnderlyingProp; // UNumericProperty
        public FPackageIndex Enum; // UEnum

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            Enum = new FPackageIndex(Ar);
            UnderlyingProp = new FPackageIndex(Ar);
        }
    }
}