using System;
using System.Collections.Generic;
using System.Linq;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Versions;

namespace CUE4Parse.UE4.Assets.Exports.Material
{
    public class UMaterial : UMaterialInterface
    {
        public bool TwoSided;
        public bool bDisableDepthTest;
        public bool bIsMasked;
        public FStructFallback? CachedExpressionData;
        public EBlendMode BlendMode = EBlendMode.BLEND_Opaque;
        public float OpacityMaskClipValue = 0.333f;
        public List<UTexture> ReferencedTextures = new();

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            TwoSided = GetOrDefault<bool>(nameof(TwoSided));
            bDisableDepthTest = GetOrDefault<bool>(nameof(bDisableDepthTest));
            bIsMasked = GetOrDefault<bool>(nameof(bIsMasked));
            BlendMode = GetOrDefault<EBlendMode>(nameof(EBlendMode));
            OpacityMaskClipValue = GetOrDefault(nameof(OpacityMaskClipValue), 0.333f);

            // 4.25+
            if (Ar.Game >= EGame.GAME_UE4_25)
            {
                CachedExpressionData = GetOrDefault<FStructFallback>(nameof(CachedExpressionData));
                if (CachedExpressionData != null && CachedExpressionData.TryGetValue(out UTexture[] referencedTextures, "ReferencedTextures"))
                    ReferencedTextures.AddRange(referencedTextures);

                if (TryGetValue(out referencedTextures, "ReferencedTextures")) // is this a thing ?
                    ReferencedTextures.AddRange(referencedTextures);
            }

            // UE4 has complex FMaterialResource format, so avoid reading anything here, but
            // scan package's imports for UTexture objects instead
            ScanForTextures(Ar);

            if (Ar.Ver >= EUnrealEngineObjectUE4Version.PURGED_FMATERIAL_COMPILE_OUTPUTS)
            {
#if READ_SHADER_MAPS
                DeserializeInlineShaderMaps(Ar, LoadedMaterialResources);
#else
                Ar.Position = validPos;
#endif
            }
        }

        private void ScanForTextures(FAssetArchive Ar)
        {
            //!! NOTE: this code will not work when textures are located in the same package - they don't present in import table
            //!! but could be found in export table. That's true for Simplygon-generated materials.
            /*foreach (var import in Ar.Owner.ImportMap)
            {
                if (import.ClassName.Text.StartsWith("Texture", StringComparison.OrdinalIgnoreCase) && import.TryLoad<UTexture>(out var tex))
                    ReferencedTextures.Add(tex);
            }*/
        }
    }
}
