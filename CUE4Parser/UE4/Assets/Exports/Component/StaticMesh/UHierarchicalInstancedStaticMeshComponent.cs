using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Exports.Component.StaticMesh
{
    public class UHierarchicalInstancedStaticMeshComponent : UInstancedStaticMeshComponent
    {
        public FClusterNode_DEPRECATED[]? ClusterTree;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            ClusterTree = FReleaseObjectVersion.Get(Ar) < FReleaseObjectVersion.Type.HISMCClusterTreeMigration ? Ar.ReadBulkArray(() => new FClusterNode_DEPRECATED(Ar)) : Ar.ReadBulkArray(() => new FClusterNode(Ar));
        }
    }

    public class FClusterNode : FClusterNode_DEPRECATED
    {
        public FVector MinInstanceScale;
        public FVector MaxInstanceScale;

        public FClusterNode(FArchive Ar) : base(Ar)
        {
            MinInstanceScale = Ar.Read<FVector>();
            MaxInstanceScale = Ar.Read<FVector>();
        }
    }

    public class FClusterNode_DEPRECATED
    {
        public FVector BoundMin;
        public int FirstChild;
        public FVector BoundMax;
        public int LastChild;
        public int FirstInstance;
        public int LastInstance;

        public FClusterNode_DEPRECATED(FArchive Ar)
        {
            BoundMin = Ar.Read<FVector>();
            FirstChild = Ar.Read<int>();
            BoundMax = Ar.Read<FVector>();
            LastChild = Ar.Read<int>();
            FirstInstance = Ar.Read<int>();
            LastInstance = Ar.Read<int>();
        }
    }
}
