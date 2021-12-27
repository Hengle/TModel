using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TModel.MainFrame.Widgets;
using static TModel.ColorConverters;

namespace TModel.MainFrame.Modules
{
    public class SettingsModule : ModuleBase
    {
        public static Action ReadSettings;

        public override string ModuleName => "Settings";

        Grid SettingsPanel = new Grid() { Background = HexBrush("#242d76") };

        CoreTextBox GameDirectoryText = new CoreTextBox();

        public override void StartupModule()
        {
            SettingsPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            SettingsPanel.ColumnDefinitions.Add(new ColumnDefinition());

            Grid Root = new Grid() { Background = HexBrush("#010744") };
            Root.RowDefinitions.Add(new RowDefinition());
            Root.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(100) } );

            StackPanel ButtonPanel = new StackPanel() 
            { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            Grid.SetRow(ButtonPanel, 1);
            Root.Children.Add(ButtonPanel);

            CButton SaveButton = new CButton("Save", 60);

            SaveButton.Click += () =>
            {
                SaveButton.IsEnabled = false;

                Preferences.GameDirectory = GameDirectoryText.Text;

                Preferences.Save();

                SaveButton.IsEnabled = true;
            };

            ButtonPanel.Children.Add(SaveButton);

            Root.Children.Add(SettingsPanel);

            Content = Root;
            
            AddOption("Game Directory", GameDirectoryText);

            ReadSettings += () =>
            {
                Preferences.Read();
                GameDirectoryText.Text = string.IsNullOrWhiteSpace(Preferences.GameDirectory) ? null : Preferences.GameDirectory;
            };

            ReadSettings();
        }

        public void AddOption(string name, UIElement input)
        {
            SettingsPanel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });

            CoreTextBlock NameText = new CoreTextBlock(name, 20)
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0,0,40,0),
            };
            Grid.SetRow(NameText, SettingsPanel.RowDefinitions.Count);
            Grid.SetRow(input, SettingsPanel.RowDefinitions.Count);
            Grid.SetColumn(input, 1);

            SettingsPanel.Children.Add(NameText);
            SettingsPanel.Children.Add(input);
        }
    }
}
