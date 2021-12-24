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
        private Orientation OppositeDirection => Direction == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;

        /// <summary>
        /// The number of modules that are contained inside of this widget
        /// </summary>
        private int ModuleCount => Direction == Orientation.Horizontal ? ColumnDefinitions.Count : RowDefinitions.Count;

        /// <summary>
        /// List of <see cref="ModuleContainer"/>s that are currently being shown.
        /// </summary>
        private List<ModuleContainer> Modules { get; } = new List<ModuleContainer>();

        public ModulePanel(Orientation direction = Orientation.Horizontal)
        {
            Direction = direction;
        }

        // Replaces the ContainerToReplace with a new ModulePanel of the OppositeDirection
        // containing the module that was there before and the NewModule.
        public void MakeSeperator(ModuleContainer ContainerToReplace, ModuleContainer NewModule)
        {
            // Module exists in this panel.
            if (Modules.IndexOf(ContainerToReplace) == -1) throw new ArgumentException("Module does not exist in panel", nameof(ContainerToReplace));
            // Gets index of current module for replacing it.
            int Index = Children.IndexOf(ContainerToReplace);
            // Removes original module.
            Children.Remove(ContainerToReplace);
            // Creates new module panel of OppositeDireciton.
            ModulePanel NewModulePanel = new ModulePanel(OppositeDirection);
            // Adds the module that was removed onto the new panel.
            NewModulePanel.AddModule(ContainerToReplace);
            // Adds the new module.
            NewModule.ParentPanel = NewModulePanel;
            NewModulePanel.AddModule(NewModule);
            // Sets the Grid Index for the new module
            if (Direction == Orientation.Horizontal)
                Grid.SetColumn(NewModulePanel, Index);
            else
                Grid.SetRow(NewModulePanel, Index);
            // Adds new module to panel
            Children.Add(NewModulePanel);
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
                AddElement(gridSplitter, true);
            }
        }

        public void AddModule(ModuleContainer moduleContainer)
        {
            AddSplitter();
            Modules.Add(moduleContainer);
            AddElement(moduleContainer, false);
        }

        private void AddElement(UIElement widget, bool IsSeperator)
        {
            GridUnitType GridType = IsSeperator ? GridUnitType.Auto : GridUnitType.Star;
            double MinSize = IsSeperator ? 0 : 100;
            if (Direction == Orientation.Horizontal)
            {
                Grid.SetColumn(widget, ModuleCount);
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridType), MinWidth = MinSize });
            }
            else
            {
                Grid.SetRow(widget, ModuleCount);
                RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridType), MinHeight = MinSize });
            }
            Children.Add(widget);
        }
    }
}
