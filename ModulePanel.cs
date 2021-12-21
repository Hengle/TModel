using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TModel.Modules;

namespace TModel
{
    /// <summary>
    /// Seperates multiple instances of <see cref="ModuleContainer"/> into resizable widgets
    /// <br/>
    /// Only works in one direction
    /// </summary>
    public sealed class ModulePanel : Grid
    {
        /// <summary>
        /// The direction of this panel
        /// </summary>
        private Orientation Direction;

        /// <summary>
        /// The number of modules that are contained inside of this widget
        /// </summary>
        private int ModuleCount => Direction == Orientation.Horizontal ? ColumnDefinitions.Count : RowDefinitions.Count;

        /// <summary>
        /// List of <see cref="ModuleContainer"/>s that are currently being shown.
        /// </summary>
        private List<ModuleContainer> Modules { get; } = new List<ModuleContainer>();

        public ModulePanel(Orientation direction = Orientation.Horizontal, bool Create = true)
        {
            Direction = direction;
        }

        public void ConvertToSeperator(ModuleInterface NewModule)
        {
            ModuleContainer MiddleModule = Modules[1];
            int Index = Children.IndexOf(MiddleModule);
            Children.Remove(MiddleModule);
            ModulePanel NewModulePanel = new ModulePanel(Orientation.Vertical);
            NewModulePanel.AddModule(MiddleModule);
            NewModulePanel.AddModule(NewModule);
            if (Direction == Orientation.Horizontal)
                Grid.SetColumn(NewModulePanel, Index);
            else
                Grid.SetRow(NewModulePanel, Index);
            Children.Insert(3, NewModulePanel);
        }

        public void AddModule(ModuleInterface module)
        {
            ModuleContainer moduleContainer = new ModuleContainer(module);
            AddModule(moduleContainer);
        }

        private void AddSplitter()
        {
            if (ModuleCount > 0)
            {
                GridSplitter gridSplitter = new GridSplitter()
                {
                    VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = Brushes.Black,
                    ResizeBehavior = GridResizeBehavior.PreviousAndNext
                };
                if (Direction == Orientation.Horizontal)
                {
                    gridSplitter.Width = 8;
                    gridSplitter.ResizeDirection = GridResizeDirection.Columns;
                }
                else
                {
                    gridSplitter.Height = 8;
                    gridSplitter.ResizeDirection = GridResizeDirection.Rows;
                }
                AddElement(gridSplitter, GridUnitType.Auto);
            }
        }

        public void AddModule(ModuleContainer moduleContainer)
        {
            AddSplitter();
            Modules.Add(moduleContainer);
            AddElement(moduleContainer, GridUnitType.Star);
        }

        private void AddElement(UIElement widget, GridUnitType type)
        {
            if (Direction == Orientation.Horizontal)
            {
                Grid.SetColumn(widget, ModuleCount);
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, type) });
            }
            else
            {
                Grid.SetRow(widget, ModuleCount);
                RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, type) });
            }
            Children.Add(widget);
        }
    }
}
