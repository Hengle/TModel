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
            MainFrame.Window.MinWidth = 400;
            MainFrame.Window.MinHeight = 200;
            MainFrame.Window.Show();
            MainFrame.Window.Background = Brushes.DarkSlateGray;
            app.MainWindow = MainFrame.Window;
            ModulePanel ModulePanel = new ModulePanel();

            ModuleContainer Module_One = new ModuleContainer(new DirectoryModule(), ModulePanel);
            ModuleContainer Module_Two = new ModuleContainer(new GameContentModule(), ModulePanel);

            Module_One.AddModule(new ItemPreviewModule());
            Module_Two.AddModule(new GameContentModule());

            ModulePanel.AddModule(Module_One);
            ModulePanel.AddModule(Module_Two);

            MainFrame.Window.Content = ModulePanel;
            app.Run();
        }
    }
}
