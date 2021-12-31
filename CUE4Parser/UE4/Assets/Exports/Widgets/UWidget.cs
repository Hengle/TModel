using CUE4Parse.UE4.Assets.Exports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TModel.CUE4Parser.UE4.Assets.Exports.Widgets
{
    public class UWidget : UObject
    {
        public override ImageSource GetPreviewIcon()
        {
            return ObjectIcons.Widget;
        }
    }
}
