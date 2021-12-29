using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel.CUE4Parser.UE4.Assets.Exports.SkeletalMesh
{
    public class USkeletalMeshComponentBudgeted : UObject
    {
        FSoftObjectPath? SkeletalMesh;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            SkeletalMesh = GetOrDefault<FSoftObjectPath>(nameof(SkeletalMesh));
        }
    }
}
