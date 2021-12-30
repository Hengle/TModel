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
using CUE4Parse.UE4.Assets;
using CUE4Parse.FN.Exports.FortniteGame;
using TModel.MainFrame.Modules;

namespace TModel
{
    public class App : Application
    {
        // Runs the given action on the UI thread.
        public static void Refresh(Action action)
        {
            Current.Dispatcher.Invoke(action, DispatcherPriority.Background);
        }

        static ModulePanel ModulePanel = new ModulePanel();

        public static DefaultFileProvider FileProvider { set; get; } = null;

        // Trys to find the type of module, if found then selects it
        public static void ShowModule<T>()
        {
            ModulePanel.TryShowModule<T>();
        }

        public static void LogMessage(string message, MessageLevel level = MessageLevel.Info) => LoggerModule.Message(message, level);

        public static bool IsValidGameDirectory(string? path)
        {
            return path != null && Directory.Exists(path);
        }

        [STAThread]
        public static void Main()
        {
            Preferences.Read();

            if (IsValidGameDirectory(Preferences.GameDirectory))
            {
                FileProvider = new DefaultFileProvider(Preferences.GameDirectory, SearchOption.TopDirectoryOnly, false, new VersionContainer(EGame.GAME_UE5_1));
                App.FileProvider.Initialize();
            }

            // Makes sure the storage folder exists
            Directory.CreateDirectory(Preferences.StorageFolder);

#if true
            ModuleContainer Module_Left = new ModuleContainer(new DirectoryModule(), ModulePanel);
#else
            ModuleContainer Module_Left = new ModuleContainer(new GameContentModule(), ModulePanel);
#endif
            ModuleContainer Module_Right = new ModuleContainer(new FileManagerModule(), ModulePanel);

            ObjectTypeRegistry.RegisterEngine(typeof(UFortItemDefinition).Assembly);

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
            Window.Background = Brushes.DarkSlateGray;
            app.MainWindow = Window;

            Window.Content = ModulePanel;

            Window.Show();
#if false
            Module_Right.AddModule(new ObjectViewerModule());
#endif
            Module_Left.AddModule(new SettingsModule());
            Module_Right.AddModule(new ObjectViewerModule());
            Module_Right.AddModule(new LoggerModule());

            ModulePanel.AddModule(Module_Left);
            ModulePanel.AddModule(Module_Right);

            app.Run();
        }
    }
}
