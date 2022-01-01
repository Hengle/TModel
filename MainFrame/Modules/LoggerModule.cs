using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;
using static TModel.ColorConverters;
using Serilog.Core;
using Serilog.Events;
using TModel.MainFrame.Widgets;

namespace TModel.MainFrame.Modules
{
    public class LoggerModule : ModuleBase
    {
        public static List<string> MessageLog;

        public override string ModuleName => "Logger";

        public static Grid Root = new Grid();
        public static CScrollViewer scrollViewer = new CScrollViewer()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        public static StackPanel LogPanel = new StackPanel()
        {
            VerticalAlignment = VerticalAlignment.Bottom
        };

        public LoggerModule() : base()
        {
            Background = HexBrush("#092041");
            scrollViewer.Content = LogPanel;
            Root.Children.Add(scrollViewer);
            Root.Children.Add(new CTextBlock("LOGGER")
            {
                Opacity = .2,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                FontSize = 40,
                FontFamily = new FontFamily("Segoe UI Bold"),
                Margin = new Thickness(5,5,10,5),
            });
            Content = Root;
        }
    }

    public class LoggerSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public LoggerSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);
            LoggerModule.LogPanel.Children.Add(new ReadonlyText(message));
            LoggerModule.scrollViewer.ScrollToEnd();
        }
    }
}

public enum MessageLevel
{
    Info,
    Error
}
