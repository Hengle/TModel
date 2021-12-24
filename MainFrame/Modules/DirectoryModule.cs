using CUE4Parse.FileProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static TModel.ColorConverters;
using static CUE4Parse.Utils.StringUtils;
using System.IO;
using System.Threading;

namespace TModel.Modules
{
    internal class DirectoryModule : ModuleBase
    {
        public static Action<string> SelectedItemChanged;

        public DirectoryModule() : base()
        {
            ModuleName = "Directory";
        }

        ScrollViewer FolderScrollViewer = new ScrollViewer();
        ScrollViewer AssetScrollViewer = new ScrollViewer();

        StackPanel FoldersPanel = new StackPanel();
        StackPanel AssetsPanel = new StackPanel();

        string CurrentPath = "FortniteGame/Content";

        TextBlock PathPanel = new TextBlock() { Style = CoreStyle.STextBlock.DefaultMedium, Foreground = Brushes.Black };

        public override void StartupModule()
        {
            FileManagerModule.ContextChanged += RefreshStuff;

            void RefreshStuff() => LoadPath(CurrentPath);

            Grid grid = new Grid();
            grid.Children.Add(PathPanel);
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });

            TextBlock PathText = new TextBlock() { Style = CoreStyle.STextBlock.DefaultMedium };



            Grid ItemsGrid = new Grid();
            Grid.SetRow(ItemsGrid, 1);
            ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 200 });
            ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            
            FolderScrollViewer.Content = FoldersPanel;
            AssetScrollViewer.Content = AssetsPanel;

            grid.Children.Add(ItemsGrid);
            Grid.SetColumn(FolderScrollViewer, 1);
            ItemsGrid.Children.Add(FolderScrollViewer);
            Grid.SetColumn(AssetScrollViewer, 0);
            ItemsGrid.Children.Add(AssetScrollViewer);

            StackPanel ButtonPanel = new StackPanel();
            Grid.SetRow(ButtonPanel, 2);
            grid.Children.Add(ButtonPanel);
            ButtonPanel.Orientation = Orientation.Horizontal;




            Button RefreshButton = new Button() { Style = CoreStyle.SButton.Default };
            RefreshButton.Content = new TextBlock() { Style = CoreStyle.STextBlock.DefaultMedium, Text = "Refresh" };
            RefreshButton.Padding = new Thickness(4);
            RefreshButton.Click += (sender, args) =>
            {
                LoadPath(CurrentPath);
            };

            Button BackButton = new Button() { Style = CoreStyle.SButton.Default };
            BackButton.Content = new TextBlock() { Style = CoreStyle.STextBlock.DefaultMedium, Text = "Back" };
            BackButton.Padding = new Thickness(4);
            BackButton.Click += (sender, args) =>
            {
                if (CurrentPath.Contains('/'))
                {
                    string NEwString = CurrentPath.SubstringBeforeWithLast('/');
                    LoadPath(CurrentPath = NEwString.Substring(0, NEwString.Length - 1));
                }
            };

            ButtonPanel.Children.Add(RefreshButton);
            ButtonPanel.Children.Add(BackButton);

            Content = grid;
        }
        void LoadPath(string path)
        {
            List<DirectoryItem> Folders = new List<DirectoryItem>();
            List<DirectoryItem> Assets = new List<DirectoryItem>();

            IEnumerable<string> Names = Enumerable.Empty<string>();

            Task.Run(() =>
            {
                Names = App.FileProvider.GetFilesInPath(path);


            }).GetAwaiter().OnCompleted(() => 
            {
                foreach (var item in Names)
                    if (item.Contains('.'))
                        Folders.Add(new DirectoryItem(item, this, true));
                    else
                        Assets.Add(new DirectoryItem(item, this));

                // ERROR: failed to sort items in array
                Folders.Sort();
                Assets.Sort();

                PathPanel.Text = CurrentPath;

                FoldersPanel.Children.Clear();
                AssetsPanel.Children.Clear();
                FolderScrollViewer.ScrollToVerticalOffset(0);
                AssetScrollViewer.ScrollToVerticalOffset(0);

                foreach (var folder in Folders)
                    FoldersPanel.Children.Add(folder);
                foreach (var asset in Assets)
                    AssetsPanel.Children.Add(asset);

            });
        }

        DirectoryItem? SelectedAsset = null;

        class DirectoryItem : ContentControl, IComparable<DirectoryItem>
        {
            bool Selected;

            string ItemName = "";

            Border border = new Border();

            public DirectoryItem(string name, DirectoryModule Owner, bool IsAsset = false)
            {
                ItemName = name;

                MinWidth = 120;

                Content = border;
                border.Background = HexBrush("#000073");
                border.BorderBrush = HexBrush("#0001a9");
                border.BorderThickness = new Thickness(1);
                border.Padding = new Thickness(5);

                MouseEnter += (s, a) =>
                {
                    border.Background = Selected ? HexBrush("#544afd") : HexBrush("#1b1b9e");
                };

                MouseLeave += (s, a) =>
                {
                    border.Background = Selected ? HexBrush("#544afd") : HexBrush("#000073");
                };

                MouseLeftButtonDown += (s, a) =>
                {
                    if (!IsAsset)
                    {
                        Owner.CurrentPath = Owner.CurrentPath.EndsWith('/') ? Owner.CurrentPath : Owner.CurrentPath += '/';
                        Owner.LoadPath(Owner.CurrentPath += name);
                    }
                    else
                    {
                        if (Owner.SelectedAsset is DirectoryItem item)
                            item.Deselect();
                        Selected = true;
                        border.Background = HexBrush("#544afd");
                        Owner.SelectedAsset = this;
                        if (SelectedItemChanged != null)
                            SelectedItemChanged(Owner.CurrentPath + '/' + name);
                    }

                };

                TextBlock textBlock = new TextBlock() { Style = CoreStyle.STextBlock.DefaultSmall };
                textBlock.Text = name;

                border.Child = textBlock;
            }

            public int CompareTo(DirectoryItem? other)
            {
                if (other is DirectoryItem item)
                    return ItemName.CompareTo(item.ItemName);
                else
                    return 0;
            }

            void Deselect()
            {
                Selected = false;
                border.Background = HexBrush("#000073");
            }
        }
    }
}
