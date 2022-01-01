﻿using CUE4Parse.UE4.Versions;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TModel.MainFrame.Widgets;
using TModel.Modules;
using static TModel.ColorConverters;

namespace TModel.MainFrame.Modules
{
    public class SettingsModule : ModuleBase
    {
        public static Action ReadSettings;

        public override string ModuleName => "Settings";

        Grid SettingsPanel = new Grid() { Background = HexBrush("#242d76") };

        // Options
        CTextBox GameDirectoryText = new CTextBox();
        CheckBox AutoLoadOnStartup = new CheckBox()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransform = new ScaleTransform(1.8,1.8),
        };

        CButton UpdateData = new CButton("Update");

        public SettingsModule() : base()
        {
            SettingsPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            SettingsPanel.ColumnDefinitions.Add(new ColumnDefinition());

            Grid Root = new Grid() { Background = HexBrush("#010744") };
            Root.RowDefinitions.Add(new RowDefinition());
            Root.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(100) });

            StackPanel ButtonPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            Grid.SetRow(ButtonPanel, 1);
            Root.Children.Add(ButtonPanel);

            CButton SaveButton = new CButton("Save", 60, () =>
            {
                Preferences.GameDirectory = GameDirectoryText.Text;
                Preferences.AutoLoad = AutoLoadOnStartup.IsChecked ?? false;
                Preferences.Save();
            });

            ButtonPanel.Children.Add(SaveButton);

            AddOption("Game Directory", GameDirectoryText);
            AddOption("Auto Load Upon Startup", AutoLoadOnStartup);
            AddOption("Update AES and Mappings", UpdateData);

            UpdateData.Click += () =>
            {
                Log.Information("Loading Mappings");
                App.FileProvider.LoadMappings();
                Log.Information("Loading AES keys");
                FileManagerModule.LoadAES(true);
                Log.Information("Finished Updating");
            };

            ReadSettings += () =>
            {
                Preferences.Read();
                GameDirectoryText.Text = string.IsNullOrWhiteSpace(Preferences.GameDirectory) ? null : Preferences.GameDirectory;
                AutoLoadOnStartup.IsChecked = Preferences.AutoLoad;
            };

            Root.Children.Add(SettingsPanel);
            Content = Root;
            ReadSettings();
        }

        public void AddOption(string name, UIElement input)
        {

            CTextBlock NameText = new CTextBlock(name, 20)
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0,0,40,0),
            };
            Grid.SetRow(NameText, SettingsPanel.RowDefinitions.Count);
            Grid.SetRow(input, SettingsPanel.RowDefinitions.Count);
            Grid.SetColumn(input, 1);

            SettingsPanel.Children.Add(NameText);
            SettingsPanel.Children.Add(input);
            SettingsPanel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
        }
    }
}
