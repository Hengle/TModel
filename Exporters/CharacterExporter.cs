using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.UObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TModel.Modules;

namespace TModel.Exporters
{
    public class CharacterExporter : ExporterBase
    {
        public override SearchTerm SearchTerm => new SearchTerm() { FileNameStart = new string[] { "CID" } };

        public override Lazy<ItemPreviewInfo>? GetPreviewInfo(IPackage package)
        {
            UObject Base = package.ExportsLazy[0].Value;
            if (Base is UAthenaCharacterItemDefinition Character)
            {
                UFortHeroType HeroDefinition = Character.HeroDefinition.Value;
                var SmallImagePath = HeroDefinition.SmallPreviewImage.Value;
                UTexture2D SmallImage = SmallImagePath.Load<UTexture2D>();
                TextureRef SmallImageRef = new TextureRef(SmallImage);

                return new Lazy<ItemPreviewInfo>(new ItemPreviewInfo() { PreviewIcon = SmallImageRef });
            }

            return null;
        }
    }
}
