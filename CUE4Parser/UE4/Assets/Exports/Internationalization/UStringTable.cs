﻿using CUE4Parse.UE4.Assets.Readers;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Exports.Internationalization
{
    public class UStringTable : UObject
    {
        public FStringTable StringTable { get; private set; }
        public int StringTableId { get; private set; } // Index of the string in the NameMap

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            StringTable = new FStringTable(Ar);
            StringTableId = Ar.Read<int>();
        }
    }
}