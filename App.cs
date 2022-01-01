// #define GENERATE_MODULES

using System;
using System.Windows;
using TModel.Modules;
using System.IO;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using System.Windows.Threading;
using CUE4Parse.UE4.Assets;
using CUE4Parse.FN.Exports.FortniteGame;
using TModel.MainFrame.Modules;
using static TModel.ColorConverters;
using Serilog;
using Serilog.Configuration;

namespace TModel
{
    public class App : Application
    {
        // Runs the given action on the UI thread.
        public static void Refresh(Action action)
        {
            Current.Dispatcher.Invoke(action, DispatcherPriority.Background);
        }
#if GENERATE_MODULES
        static ModulePanel modulePanel = new ModulePanel();
#endif

        public static Window Window { get; } = new Window();

        public static DefaultFileProvider FileProvider { set; get; } = null;

        // Trys to find the type of module, if found then selects it
        public static void ShowModule<T>()
        {
#if GENERATE_MODULES
            modulePanel.TryShowModule<T>();
#endif
        }
        public static bool IsValidGameDirectory(string? path)
        {
            return path != null && Directory.Exists(path);
        }

        [STAThread]
        public static void Main()
        {
#if GENERATE_MODULES
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.MySink()
                .CreateLogger();

            Preferences.Read();

            if (IsValidGameDirectory(Preferences.GameDirectory))
            {
                FileProvider = new DefaultFileProvider(Preferences.GameDirectory, SearchOption.TopDirectoryOnly, false, new VersionContainer(EGame.GAME_UE5_1));
                App.FileProvider.Initialize();
            }
#endif
            // Makes sure the storage folder exists
            Directory.CreateDirectory(Preferences.StorageFolder);
#if GENERATE_MODULES
#if false
            ModuleContainer Module_Left = new ModuleContainer(new DirectoryModule(), ModulePanel);
#else
            ModuleContainer Module_Left = new ModuleContainer(new GameContentModule(), modulePanel);
#endif
            ModuleContainer Module_Right = new ModuleContainer(new FileManagerModule(), modulePanel);
#endif
            ObjectTypeRegistry.RegisterEngine(typeof(UFortItemDefinition).Assembly);

#if false // Preload data
            FileManagerModule.LoadGame();
#endif
            // FileProvider.GetFilesInPath(@"FortniteGame/Content/Athena/Items");

            var app = new App();
            Window.Title = "TModel";
            Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Window.ResizeMode = ResizeMode.CanResize;
            Window.MinWidth = 400;
            Window.MinHeight = 200;
            Window.Background = HexBrush("#15162a");
#if GENERATE_MODULES
            Window.Content = modulePanel;
#else
            Window.Content = new ItemPreviewModule();
#endif
            app.MainWindow = Window;
            Window.Show();
#if GENERATE_MODULES
#if false
            Module_Right.AddModule(new ObjectViewerModule());
#endif
            Module_Left.AddModule(new SettingsModule());
            // Module_Right.AddModule(new SearchModule());
            Module_Right.AddModule(new HexEditorModule());
            Module_Right.AddModule(new ItemPreviewModule());


            modulePanel.AddModule(Module_Left);
            modulePanel.AddModule(Module_Right);

            modulePanel.MakeSeperator(Module_Right, new ModuleContainer(new LoggerModule(), false), new GridLength(100, GridUnitType.Pixel));
#endif
            app.Run();
        }
    }


    public static class MySinkExtensions
    {
        public static LoggerConfiguration MySink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new LoggerSink(formatProvider));
        }
    }
}
