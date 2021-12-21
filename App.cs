using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Ink;
using TModel.Modules;

namespace TModel
{
    public class MainApp : Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new MainApp();
            MainFrame.Window.Title = "TModel";
            MainFrame.Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MainFrame.Window.ResizeMode = ResizeMode.CanResize;
            MainFrame.Window.Show();
            MainFrame.Window.Background = Brushes.DarkSlateGray;
            app.MainWindow = MainFrame.Window;
            ModulePanel ModulePanel = new ModulePanel();

            ModulePanel.AddModule(new DirectoryModule());
            ModulePanel.AddModule(new ObjectViewerModule());
            ModulePanel.AddModule(new ObjectViewerModule());

            ModulePanel.ConvertToSeperator(new ItemPreviewModule());

            MainFrame.Window.Content = ModulePanel;
            app.Run();
        }
    }
}
