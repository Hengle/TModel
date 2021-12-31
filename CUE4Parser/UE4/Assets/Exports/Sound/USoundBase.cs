using System.Windows.Media;
using TModel;

namespace CUE4Parse.UE4.Assets.Exports.Sound
{
    public abstract class USoundBase : UObject
    {
        public override ImageSource GetPreviewIcon()
        {
            return ObjectIcons.Sound;
        }
    }
}