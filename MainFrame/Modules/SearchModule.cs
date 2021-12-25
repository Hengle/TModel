using CUE4Parse.FileProvider;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static TModel.ColorConverters;
using static CUE4Parse.Utils.StringUtils;
using System.Windows.Media;
using System;
using System.Threading.Tasks;
using System.Windows.Documents;
using TModel.Modules;

namespace TModel.MainFrame.Modules
{
    public class SearchModule : ModuleBase
    {
        private static int MaxSize = 100;

        public SearchModule()
        {
            ModuleName = "Search";
        }

        public override void StartupModule()
        {
            Grid Root = new Grid() { Background = HexBrush("#3a3a3a") };
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100) });
            Root.RowDefinitions.Add(new RowDefinition());

            StackPanel ButtonPanel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            TextBox SearchBar = new TextBox()
            {
                Height = 40,
                FontSize = 16,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontFamily = CoreStyle.CoreFont,
                Background = HexBrush("#282828"),
                Foreground = Brushes.White,
                Padding = new Thickness(5),
            };

            ScrollViewer scrollViewer = new ScrollViewer() { Background = HexBrush("#000318") };

            StackPanel ResultsPanel = new StackPanel();

            scrollViewer.Content = ResultsPanel;

            SearchBar.TextChanged += (sender, args) =>
            {
                string SearchTerm = SearchBar.Text;

                List<GameFile> Results = new List<GameFile>();
                Task.Run(() =>
                {
                    var Files = App.FileProvider.Files;
                    foreach (var item in Files)
                    {
                        if (item.Key.Contains(SearchTerm, System.StringComparison.CurrentCultureIgnoreCase))
                        {
                            Results.Add(item.Value);
                        }
                    }
                }).GetAwaiter().OnCompleted(() => 
                {
                    ResultsPanel.Children.Clear();
                    for (int i = 0; i < (MaxSize < Results.Count ? MaxSize : Results.Count); i++)
                    {
                        GameFile item = Results[i];
                        ResultsPanel.Children.Add(new SearchResult(item));
                    }
                });
            };

            ButtonPanel.Children.Add(SearchBar);


            Root.Children.Add(ButtonPanel);
            Grid.SetRow(scrollViewer, 1);
            Root.Children.Add(scrollViewer);

            Content = Root;
        }

        class SearchResult : Grid
        {
            private static Brush Hover = HexBrush("#051c56");
            private static Brush NameBrush = HexBrush("#24c5e3");

            public SearchResult(GameFile file)
            {
                MouseEnter += (sender, args) =>
                {
                    Background = Hover;
                };

                MouseLeave += (sender, args) =>
                {
                    Background = Brushes.Transparent;
                };

                MouseLeftButtonDown += (sender, args) =>
                {
                    App.ShowModule<DirectoryModule>();
                    DirectoryModule.GoToPath(file.Path);
                };

                RichTextBox richTextBox = new RichTextBox()
                {
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent,
                    FontSize = 13,
                    IsReadOnly = true,
                };

                Children.Add(new TextBlock() { Text = file.Path, Style = new DefaultText(13) });
            }
        }
    }
}
