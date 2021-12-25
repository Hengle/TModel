using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Ink;
using TModel.Modules;
using System.IO;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using System.Windows.Threading;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.Encryption.Aes;
using Newtonsoft.Json;
using System.Net;
using CUE4Parse.UE4.Assets.Exports;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace TModel
{
    public class App : Application
    {
        // Runs the given action on the UI thread.
        public static void Refresh(Action action)
        {
            Current.Dispatcher.Invoke(action, DispatcherPriority.Background);
        }

        public static DefaultFileProvider FileProvider = new DefaultFileProvider(Preferences.GameDirectory, SearchOption.TopDirectoryOnly, false, new VersionContainer(EGame.GAME_UE5_1));

        [STAThread]
        public static void Main()
        {
            App.FileProvider.Initialize();

#if false // Preload data
            FileManagerModule.LoadGame();
#endif
            // FileProvider.GetFilesInPath(@"FortniteGame/Content/Athena/Items");
            Window Window = new Window();

            var app = new App();
            Window.Title = "TModel";
            Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Window.ResizeMode = ResizeMode.CanResize;
            Window.MinWidth = 400;
            Window.MinHeight = 200;
            Window.Show();
            Window.Background = Brushes.DarkSlateGray;
            app.MainWindow = Window;
            ModulePanel ModulePanel = new ModulePanel();

            // Generates default window layout
            ModuleContainer Module_One = new ModuleContainer(new DirectoryModule(), ModulePanel);
            ModuleContainer Module_Two = new ModuleContainer(new FileManagerModule(), ModulePanel);

            Module_One.AddModule(new GameContentModule());
            Module_Two.AddModule(new ObjectViewerModule());
            Module_Two.AddModule(new ItemPreviewModule());

            ModulePanel.AddModule(Module_One);
            ModulePanel.AddModule(Module_Two);

            Window.Content = ModulePanel;
            app.Run();
        }
    }
}
