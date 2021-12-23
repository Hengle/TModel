using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.Engine;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Vfs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TModel.Sorters;
using static CUE4Parse.Utils.StringUtils;

namespace TModel.Modules
{
    /// <summary>
    /// Module for managing the loading and unloading of pak files.
    /// </summary>
    public class FileManagerModule : ModuleBase
    {
        StackPanel FilesPanel = new StackPanel();

        public FileManagerModule() : base()
        {
            ModuleName = "File Manager";
        }

        public override void StartupModule()
        {
            Grid grid = new Grid();
            Content = grid;
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(120) });
            grid.RowDefinitions.Add(new RowDefinition());

            StackPanel ButtonPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            ButtonPanel.HorizontalAlignment = HorizontalAlignment.Center;
            ButtonPanel.VerticalAlignment = VerticalAlignment.Center;
            
            ScrollViewer FilePanelScroller = new ScrollViewer();
            FilePanelScroller.Content = FilesPanel;
            Grid.SetRow(ButtonPanel, 0);
            Grid.SetRow(FilePanelScroller, 1);

            grid.Children.Add(ButtonPanel);
            grid.Children.Add(FilePanelScroller);


            grid.Background = Brushes.DarkSlateBlue;

            Button LoadButton = new Button();
            TextBlock ButtonText = new TextBlock();
            ButtonText.Text = "Load";
            ButtonText.Foreground = Brushes.Black;
            ButtonText.FontSize = 40;
            LoadButton.Content = ButtonText;
            LoadButton.Click += (sender, args) =>
            {
                ButtonText.Text = "Unpacking";
                LoadButton.IsEnabled = false;
                Task.Run(() =>
                {
                    FileProvider.SubmitKey(new FGuid(), new FAesKey("DAE1418B289573D4148C72F3C76ABC7E2DB9CAA618A3EAF2D8580EB3A1BB7A63"));
                }).GetAwaiter().OnCompleted(() =>
                {
                    ButtonText.Text = "Finished";
                    var Results = FileProvider.AesKeys;
                });
            };

            LoadButton.Height = 90;
            LoadButton.Width = 180;

            ButtonPanel.Children.Add(LoadButton);

            Button SelectAllButton = new Button();
            Button DeselectAllButton = new Button();


            SelectAllButton.Click += (sender, args) =>
            {
                foreach (var item in FilesPanel.Children)
                {
                    ((FileManagerItem)item).Select();
                }
            };

            DeselectAllButton.Click += (sender, args) =>
            {
                foreach (var item in FilesPanel.Children)
                {
                    ((FileManagerItem)item).Deselect();
                }
            };



            TextBlock SelectButtonText = new TextBlock();
            SelectButtonText.Text = "Select All";
            SelectButtonText.FontSize = 20;
            SelectButtonText.TextWrapping = TextWrapping.NoWrap;
            SelectButtonText.VerticalAlignment = VerticalAlignment.Center;
            SelectButtonText.HorizontalAlignment = HorizontalAlignment.Center;

            SelectAllButton.Content = SelectButtonText;

            Grid.SetRow(SelectAllButton, 0);

            TextBlock DeselectAllButtonText = new TextBlock();
            DeselectAllButtonText.Text = "Deselect All";
            DeselectAllButtonText.FontSize = 20;
            DeselectAllButtonText.TextWrapping = TextWrapping.NoWrap;
            DeselectAllButtonText.VerticalAlignment = VerticalAlignment.Center;
            DeselectAllButtonText.HorizontalAlignment = HorizontalAlignment.Center;

            DeselectAllButton.Content = DeselectAllButtonText;

            Grid.SetRow(DeselectAllButton, 1);

            Grid SelectionGrid = new Grid() { Height = 90, Width = 130 };
            SelectionGrid.RowDefinitions.Add(new RowDefinition());
            SelectionGrid.RowDefinitions.Add(new RowDefinition());

            SelectionGrid.Children.Add(SelectAllButton);
            SelectionGrid.Children.Add(DeselectAllButton);

            ButtonPanel.Children.Add(SelectionGrid);

            LoadFiles();
        }

        private void LoadFiles()
        {
            FileProvider.UnloadedVfs.Sort(new NameSort());

            foreach (var item in FileProvider.UnloadedVfs)
            {
                if (item.Name.EndsWith(".pak"))
                    FilesPanel.Children.Add(new FileManagerItem(item));
            }
        }
    }

    public class FileManagerItem : ContentControl
    {
        private static bool Switcher = false;
        CheckBox checkBox = new CheckBox() { IsEnabled = false };
        public bool IsSelected => checkBox.IsChecked == false ? false : true;


        public void Select()
        {
            checkBox.IsChecked = true;
        }

        public void Deselect()
        {
            checkBox.IsChecked = false;
        }

        // Needs name (and file size probs)
        public FileManagerItem(IAesVfsReader reader)
        {
            Brush BackgroundBrush = Switcher ? Brushes.Peru : Brushes.Orange;
            Switcher = !Switcher;
            Border SingleFileBorder = new Border();



            MouseEnter += (sender, args) =>
            {
                SingleFileBorder.Background = Brushes.OrangeRed;
            };

            MouseLeave += (sender, args) =>
            {
                SingleFileBorder.Background = BackgroundBrush;
            };

            MouseLeftButtonDown += (sender, args) =>
            {
                SingleFileBorder.Background = Brushes.Orchid;
                if (IsSelected)
                    Deselect();
                else
                    Select();
            };

            MouseLeftButtonUp += (sender, args) =>
            {
                SingleFileBorder.Background = Brushes.OrangeRed;
            };



            Grid SingleFilePanel = new Grid();
            SingleFilePanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            SingleFilePanel.ColumnDefinitions.Add(new ColumnDefinition());
            SingleFileBorder.Child = SingleFilePanel;

            TextBlock SingleFileName = new TextBlock();
            SingleFileName.Text = reader.Name; // Name of file
            SingleFileName.FontSize = 20;
            SingleFileName.TextWrapping = TextWrapping.Wrap;
            SingleFileName.VerticalAlignment = VerticalAlignment.Center;
            SingleFileName.HorizontalAlignment = HorizontalAlignment.Left;

            checkBox.LayoutTransform = new ScaleTransform(1.7, 1.7);
            // checkBox.Width = 80;
            // checkBox.Height = 80;
            checkBox.Margin = new Thickness(0);
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.HorizontalAlignment = HorizontalAlignment.Right;

            WrapPanel DeteailsPanel = new WrapPanel() { Orientation = Orientation.Horizontal };
            DeteailsPanel.VerticalAlignment = VerticalAlignment.Center;
            DeteailsPanel.HorizontalAlignment = HorizontalAlignment.Right;
            SingleFilePanel.Children.Add(SingleFileName);

            DeteailsPanel.Children.Add(checkBox); // Last

            Grid.SetColumn(DeteailsPanel, 1);

            SingleFilePanel.Children.Add(DeteailsPanel);


            SingleFileBorder.Height = 40;
            SingleFileBorder.Background = BackgroundBrush;
            SingleFileBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            SingleFileBorder.BorderThickness = new Thickness(1.8);
            SingleFileBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            SingleFileBorder.VerticalAlignment = VerticalAlignment.Top;

            Content = SingleFileBorder;
        }
    }
}
