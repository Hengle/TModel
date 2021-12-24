using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Vfs;
using CUE4Parse.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TModel.Sorters;
using static TModel.ColorConverters;

namespace TModel.Modules
{
    /// <summary>
    /// Module for managing the loading and unloading of pak files.
    /// </summary>
    public class FileManagerModule : ModuleBase
    {
        StackPanel FilesPanel = new StackPanel();

        public static Action ContextChanged;

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

            grid.Background = HexBrush("#2e3d54");

            Button LoadButton = new Button() { Style = CoreStyle.SButton.Default };

            TextBlock LoadButtonText = new TextBlock() { Style = CoreStyle.STextBlock.DefaultLarge };
            LoadButtonText.Text = "Load";
            LoadButton.Content = LoadButtonText;
            LoadButton.Click += (sender, args) =>
            {
                var Before = App.FileProvider.MountedVfs;
                LoadButtonText.Text = "Unpacking";
                Task.Run(() =>
                {
                    App.FileProvider.SubmitKey(new FGuid(), new FAesKey("DAE1418B289573D4148C72F3C76ABC7E2DB9CAA618A3EAF2D8580EB3A1BB7A63"));
                    App.FileProvider.LoadMappings();
                }).GetAwaiter().OnCompleted(() =>
                {
                    ContextChanged();
                    LoadButtonText.Text = "Finished";
                    var After = App.FileProvider.MountedVfs;
                    List<IAesVfsReader> AllVFS = new List<IAesVfsReader>();
                    AllVFS.AddRange(App.FileProvider.MountedVfs);
                    AllVFS.AddRange(App.FileProvider.UnloadedVfs);
                    AllVFS.Sort(new NameSort());
                    FilesPanel.Children.Clear();
                    LoadFiles(AllVFS, true);
                });
            };

            LoadButton.Height = 90;
            LoadButton.Width = 180;

            ButtonPanel.Children.Add(LoadButton);

            Button SelectAllButton = new Button() { Style = CoreStyle.SButton.Default };
            Button DeselectAllButton = new Button() { Style = CoreStyle.SButton.Default };


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



            TextBlock SelectButtonText = new TextBlock() { Style = CoreStyle.STextBlock.DefaultMedium };
            SelectButtonText.Text = "Select All";
            SelectButtonText.TextWrapping = TextWrapping.NoWrap;
            SelectButtonText.VerticalAlignment = VerticalAlignment.Center;
            SelectButtonText.HorizontalAlignment = HorizontalAlignment.Center;

            SelectAllButton.Content = SelectButtonText;

            Grid.SetRow(SelectAllButton, 0);

            TextBlock DeselectAllButtonText = new TextBlock() { Style = CoreStyle.STextBlock.DefaultMedium };
            DeselectAllButtonText.Text = "Deselect All";
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

            // App.FileProvider.UnloadedVfs.Sort(new NameSort());
            LoadFiles(App.FileProvider.UnloadedVfs);
        }

        private void LoadFiles(ICollection<IAesVfsReader> files, bool tryload = false)
        {
            foreach (var item in files)
            {
                if (item.Name.EndsWith(".utoc"))
                    FilesPanel.Children.Add(new FileManagerItem(item, tryload));
            }
        }
    }

    public class FileManagerItem : ContentControl
    {
        CheckBox checkBox = new CheckBox()
        {
            IsEnabled = false,
            LayoutTransform = new ScaleTransform(1.3, 1.3),
            Margin = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };

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
        public FileManagerItem(IAesVfsReader reader, bool tryload = false)
        {
            Brush BackgroundBrush = reader.FileCount > 0 ? HexBrush("073880") : HexBrush("031c40");
            Border RootBorder = new Border()
            {
                Padding = new Thickness(5),
                Background = BackgroundBrush,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            MouseEnter += (sender, args) =>
            {
                RootBorder.Background = HexBrush("#0f00b2");
            };

            MouseLeave += (sender, args) =>
            {
                RootBorder.Background = BackgroundBrush;
            };

            MouseLeftButtonDown += (sender, args) =>
            {
                RootBorder.Background = HexBrush("#1300d9");
                if (IsSelected)
                    Deselect();
                else
                    Select();
            };

            MouseLeftButtonUp += (sender, args) =>
            {
                RootBorder.Background = HexBrush("#0f00b2");
            };

            TextBlock FilesCountText = new TextBlock()
            {
                Style = CoreStyle.STextBlock.DefaultSmall,
                Text = reader.FileCount.ToString(),
                TextAlignment = TextAlignment.Right,
                Width = 60,
            };

            TextBlock MountPointText = new TextBlock()
            {
                Style = CoreStyle.STextBlock.DefaultSmall,
                Text = reader.MountPoint,
                ToolTip = reader.MountPoint,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextAlignment = TextAlignment.Right,
                TextWrapping = TextWrapping.NoWrap,
                Width = 300,
            };

            Grid MainPanel = new Grid();
            MainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            MainPanel.ColumnDefinitions.Add(new ColumnDefinition());
            RootBorder.Child = MainPanel;

            TextBlock FileNameText = new TextBlock()
            {
                Style = CoreStyle.STextBlock.DefaultSmall,
                Text = reader.Name,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            StackPanel DetailsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            Grid.SetColumn(DetailsPanel, 1);
            MainPanel.Children.Add(FileNameText);

            DetailsPanel.Children.Add(MountPointText);
            DetailsPanel.Children.Add(FilesCountText);
            DetailsPanel.Children.Add(checkBox);


            MainPanel.Children.Add(DetailsPanel);

            Content = RootBorder;
        }
    }
}
