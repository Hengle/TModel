using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using Newtonsoft.Json;
using TModel.MainFrame.Widgets;
using static CUE4Parse.Utils.StringUtils;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    class ObjectViewerModule : ModuleBase
    {
        public override string ModuleName => "Object Viewer";

        public ObjectViewerModule() : base()
        {

        }

        public override void StartupModule()
        {

        }
    }
}
