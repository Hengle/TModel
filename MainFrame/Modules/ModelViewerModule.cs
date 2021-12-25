using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TModel.MainFrame.Modules
{
    public class ModelViewerModule : ModuleBase
    {
        public ModelViewerModule()
        {
            ModuleName = "Model Viewer";
        }

        public override void StartupModule()
        {
            Content = new ReadonlyText("ModelViewer", 60) { Background = Brushes.Black };
        }
    }
}
