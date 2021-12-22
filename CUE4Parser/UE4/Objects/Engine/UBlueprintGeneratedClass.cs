using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Objects.Engine
{
    public class UBlueprintGeneratedClass : UClass
    {
        public Dictionary<FName, string>? EditorTags;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            if (FFortniteMainBranchObjectVersion.Get(Ar) >= FFortniteMainBranchObjectVersion.Type.BPGCCookedEditorTags)
            {
                if (validPos - Ar.Position > 4)
                {
                    var size = Ar.Read<int>();
                    EditorTags = new Dictionary<FName, string>();
                    for (var i = 0; i < size; i++)
                    {
                        EditorTags[Ar.ReadFName()] = Ar.ReadFString();
                    }
                }
            }
        }
    }
}