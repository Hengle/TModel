using System.Windows.Media;
using TModel;

namespace CUE4Parse.UE4.Assets.Exports.Animation
{
    public class UAnimMontage : UAnimCompositeBase
    {
        public override ImageSource GetPreviewIcon()
        {
            return ObjectIcons.AnimMontage;
        }
    }
}