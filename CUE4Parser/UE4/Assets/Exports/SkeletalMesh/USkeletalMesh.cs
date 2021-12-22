using CUE4Parse.UE4.Assets.Exports.Animation;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.Engine;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Exports.SkeletalMesh
{
    /// <summary>
    /// A skeletal mesh is basicly a static mesh and skeleton combined together
    /// with weights to connect them together.
    /// </summary>
    public class USkeletalMesh : UObject
    {
        public FBoxSphereBounds ImportedBounds { get; private set; }

        /// <summary>
        /// How the skeletal mesh looks.
        /// </summary>
        public FSkeletalMaterial[] Materials { get; private set; }

        /// <summary>
        /// The skeleton that controls this mesh.
        /// </summary>
        public FReferenceSkeleton ReferenceSkeleton { get; private set; }

        /// <summary>
        /// Skeletal meshes can have many different resolutions, LOD models defines those.
        /// <br/>
        /// There are usually 4 and the first one is the highest res.
        /// </summary>
        public FStaticLODModel[]? LODModels { get; private set; }
        public bool bHasVertexColors { get; private set; }
        public byte NumVertexColorChannels { get; private set; }

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            bHasVertexColors = GetOrDefault<bool>(nameof(bHasVertexColors));
            NumVertexColorChannels = GetOrDefault<byte>(nameof(NumVertexColorChannels));

            var stripDataFlags = Ar.Read<FStripDataFlags>();
            ImportedBounds = Ar.Read<FBoxSphereBounds>();
            Materials = Ar.ReadArray(() => new FSkeletalMaterial(Ar));
            ReferenceSkeleton = new FReferenceSkeleton(Ar);

            if (FSkeletalMeshCustomVersion.Get(Ar) < FSkeletalMeshCustomVersion.Type.SplitModelAndRenderData)
            {
                LODModels = Ar.ReadArray(() => new FStaticLODModel(Ar, bHasVertexColors));
            }
            else
            {
                if (!stripDataFlags.IsEditorDataStripped())
                {
                    LODModels = Ar.ReadArray(() => new FStaticLODModel(Ar, bHasVertexColors));
                }

                var bCooked = Ar.ReadBoolean();
                if (Ar.Versions["SkeletalMesh.KeepMobileMinLODSettingOnDesktop"])
                {
                    var minMobileLODIdx = Ar.Read<int>();
                }

                if (bCooked && LODModels == null)
                {
                    var useNewCookedFormat = Ar.Versions["SkeletalMesh.UseNewCookedFormat"];
                    LODModels = new FStaticLODModel[Ar.Read<int>()];
                    for (var i = 0; i < LODModels.Length; i++)
                    {
                        LODModels[i] = new FStaticLODModel();
                        if (useNewCookedFormat)
                        {
                            LODModels[i].SerializeRenderItem(Ar, bHasVertexColors, NumVertexColorChannels);
                        }
                        else
                        {
                            LODModels[i].SerializeRenderItem_Legacy(Ar, bHasVertexColors, NumVertexColorChannels);
                        }
                    }

                    if (useNewCookedFormat)
                    {
                        var numInlinedLODs = Ar.Read<byte>();
                        var numNonOptionalLODs = Ar.Read<byte>();
                    }
                }
            }

            if (Ar.Ver < EUnrealEngineObjectUE4Version.REFERENCE_SKELETON_REFACTOR)
            {
                var length = Ar.Read<int>();
                Ar.Position += 12 * length; // TMap<FName, int32> DummyNameIndexMap
            }

            var dummyObjs = Ar.ReadArray(() => new FPackageIndex(Ar));
        }
    }
}