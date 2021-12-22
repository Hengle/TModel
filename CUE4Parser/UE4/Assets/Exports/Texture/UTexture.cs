using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Engine;

namespace CUE4Parse.UE4.Assets.Exports.Texture
{
    public abstract class UTexture : UStreamableRenderAsset
    {
        public FStripDataFlags StripFlags;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            StripFlags = Ar.Read<FStripDataFlags>();
        }
    }
}