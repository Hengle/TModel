using CUE4Parse.UE4.Assets.Readers;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Exports.Component.StaticMesh
{
    public class UStaticMeshComponent : UObject
    {
        public FStaticMeshComponentLODInfo[]? LODData;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            LODData = Ar.ReadArray(() => new FStaticMeshComponentLODInfo(Ar));
        }
    }
}