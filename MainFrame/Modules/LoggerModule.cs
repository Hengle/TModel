using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;
using static TModel.ColorConverters;

namespace TModel.MainFrame.Modules
{
    public class LoggerModule : ModuleBase
    {
        // DON'T use this, use App.LogMessage(string) 
        public static Action<string, MessageLevel> Message;

        public static List<string> MessageLog;

        public override string ModuleName => "Logger";

        ScrollViewer scrollViewer = new ScrollViewer();
        CTextBlock textBlock = new CTextBlock("", 14) 
        { 
            VerticalAlignment = VerticalAlignment.Bottom,
            TextWrapping = TextWrapping.Wrap,
        };

        public LoggerModule() : base()
        {
            Background = HexBrush("#092041");
            scrollViewer.Content = textBlock;
            Content = scrollViewer;

            Message += (message, level) => App.Refresh(() =>
            {
                Brush FinalColor = Brushes.White;
                if (level == MessageLevel.Error)
                {
                    FinalColor = HexBrush("#ff5e5e");
                    App.ShowModule<LoggerModule>();
                }

                textBlock.Inlines.Add(new Run("\n" + message) { Foreground = FinalColor });
                scrollViewer.ScrollToEnd();
            });
        }
    }
}

public enum MessageLevel
{
    Info,
    Error
}
