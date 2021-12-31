using System.Windows.Media;
using TModel;

namespace CUE4Parse.UE4.Objects.Engine.Animation
{
    public class UBlendSpace : UBlendSpaceBase
    {
        public override ImageSource GetPreviewIcon()
        {
            return ObjectIcons.BlendSpace;
        }
    }
}
