using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace TModel.MainFrame.Modules
{
    public class LoggerModule : ModuleBase
    {
        public static Action<string> Write;

        public override string ModuleName => "Logger";


        ScrollViewer scrollViewer = new ScrollViewer();
        TextBlock textBlock = new TextBlock() { Style = new DefaultText(12), Background = Brushes.Black };

        public override void StartupModule()
        {
            Background = Brushes.Black;
            scrollViewer.Content = textBlock;
            Content = scrollViewer;
            
            Write += (message) => App.Refresh(() => { textBlock.Text += message + "\n"; scrollViewer.ScrollToVerticalOffset(1); } );
        }
    }
}
