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
using TModel.MainFrame.Widgets;

namespace TModel.Modules
{
    internal class DirectoryModule : ModuleBase
    {
        public static Action<string> SelectedItemChanged;

        public DirectoryModule() : base()
        {
            ModuleName = "Directory";
        }

        // The path currently being shown.
        string CurrentPath = "FortniteGame/Content";

        // Left side. usaully smaller than asset panel
        ScrollViewer FolderScrollViewer = new ScrollViewer();
        StackPanel FoldersPanel = new StackPanel();

        // Right side.
        ScrollViewer AssetScrollViewer = new ScrollViewer();
        StackPanel AssetsPanel = new StackPanel();


        // Very top bar. Shows CurrentPath.
        TextBlock PathText = new TextBlock() 
        { 
            Style = new DefaultText(), 
            VerticalAlignment = VerticalAlignment.Center 
        };

        public override void StartupModule()
        {
            FileManagerModule.ContextChanged += RefreshStuff;

            void RefreshStuff() => LoadPath(CurrentPath);

            Grid Root = new Grid() { Background = HexBrush("#12161b") };
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            Root.RowDefinitions.Add(new RowDefinition());
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });


            Grid ItemsGrid = new Grid();
            Grid.SetRow(ItemsGrid, 1);
            ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 200 });
            ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            
            AssetScrollViewer.Content = AssetsPanel;
            FolderScrollViewer.Content = FoldersPanel;




            Grid.SetColumn(AssetScrollViewer, 1);
            Grid.SetColumn(FolderScrollViewer, 0);

            ItemsGrid.Children.Add(AssetScrollViewer);
            ItemsGrid.Children.Add(FolderScrollViewer);



            StackPanel ButtonPanel = new StackPanel();
            Grid.SetRow(ButtonPanel, 2);
            ButtonPanel.Orientation = Orientation.Horizontal;

#if false
            for (int i = 0; i < 40; i++)
            {
                AssetsPanel.Children.Add(new DirectoryItem("TestItemDir/TestItem.debug", this, true));
            }
#endif

            // Reloads the current path.
            CButton RefreshButton = new CButton("Refresh");
            RefreshButton.Padding = new Thickness(4);
            RefreshButton.Click += () => LoadPath(CurrentPath);

            // Goes back a path
            CButton BackButton = new CButton("Back");
            BackButton.Padding = new Thickness(4);
            BackButton.Click += () =>
            {
                if (CurrentPath.Contains('/'))
                {
                    string NEwString = CurrentPath.SubstringBeforeWithLast('/');
                    LoadPath(CurrentPath = NEwString.Substring(0, NEwString.Length - 1));
                }
            };

            ButtonPanel.Children.Add(RefreshButton);
            ButtonPanel.Children.Add(BackButton);



            Root.Children.Add(PathText);
            Root.Children.Add(ItemsGrid);
            Root.Children.Add(ButtonPanel);

            Content = Root;
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
                        Folders.Add(new DirectoryItem(CurrentPath + '/' + item, this, true));
                    else
                        Assets.Add(new DirectoryItem(CurrentPath + '/' + item, this));

                // ERROR: failed to sort items in array
                Folders.Sort();
                Assets.Sort();

                PathText.Text = CurrentPath;

                AssetsPanel.Children.Clear();
                FoldersPanel.Children.Clear();
                AssetScrollViewer.ScrollToVerticalOffset(0);
                FolderScrollViewer.ScrollToVerticalOffset(0);

                foreach (var folder in Folders)
                    AssetsPanel.Children.Add(folder);
                foreach (var asset in Assets)
                    FoldersPanel.Children.Add(asset);

            });
        }

        DirectoryItem? SelectedAsset = null;

        class DirectoryItem : ContentControl, IComparable<DirectoryItem>
        {
            bool IsSelected;

            string FullPath = "";

            string FileName => Path.GetFileName(FullPath);

            // Background colors for the following states:
            static Brush Normal = HexBrush("#1e2936");
            static Brush Hover = HexBrush("#1e64a5");
            static Brush Selected = HexBrush("#448af6");

            static Brush Border = HexBrush("#2a3d53");

            // The root of this element
            Border border = new Border() 
            {
                Background = Normal,
                BorderBrush = Border,
                BorderThickness = new Thickness(1.8),
                Padding = new Thickness(5)
            };

            public DirectoryItem(string path, DirectoryModule Owner, bool IsAsset = false)
            {

                FullPath = path;
                MinWidth = 120;
                Content = border;

                // Shows the name of item
                TextBlock textBlock = new TextBlock() 
                { 
                    Style = new DefaultText(),
                    Text = FileName
                };
                border.Child = textBlock;

                // Hover effects
                MouseEnter += (s, a) =>
                {
                    border.Background = IsSelected ? Selected : Hover;
                };
                MouseLeave += (s, a) =>
                {
                    border.Background = IsSelected ? Selected : Normal;
                };

                // Clicking on item
                MouseLeftButtonDown += (s, a) =>
                {

                    if (!IsAsset) // Folder
                    {
                        // Sets the CurrentPath making sure that is ends with a '/'
                        Owner.CurrentPath = Owner.CurrentPath.EndsWith('/') ? Owner.CurrentPath : Owner.CurrentPath += '/';
                        Owner.LoadPath(Owner.CurrentPath = FullPath);
                    }
                    else // Asset
                    {
                        // Deselects the current selected asset
                        if (Owner.SelectedAsset is DirectoryItem item)
                            item.Deselect();
                        // Selects this asset
                        IsSelected = true;
                        border.Background = Selected;
                        Owner.SelectedAsset = this;

                        // Calls a Action to let the ObjectViewer know to update its contents.
                        if (SelectedItemChanged != null)
                            SelectedItemChanged(Owner.CurrentPath + '/' + FileName);
                    }
                };
            }

            public int CompareTo(DirectoryItem? other)
            {
                if (other is DirectoryItem item)
                    return FullPath.CompareTo(item.FullPath);
                else
                    return 0;
            }

            void Deselect()
            {
                IsSelected = false;
                border.Background = Normal;
            }
        }
    }
}
