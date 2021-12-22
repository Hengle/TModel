using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using TModel.Modules;

namespace TModel
{
    /// <summary>
    /// Contains multiple instances of <see cref="ModuleBase"/> that can be switched using the tab bar
    /// </summary>
    public sealed class ModuleContainer : ContentControl
    {
        private TabControl TabBar = new TabControl();

        private Grid OverlayPanel = new Grid();

        public ModulePanel? ParentPanel;

        public List<ModuleBase> Modules { get; } = new List<ModuleBase>();

        /// <summary>
        /// Creates new instance of <see cref="ModuleContainer"/>.
        /// </summary>
        /// <param name="Base">The first tab module.</param>
        public ModuleContainer(ModuleBase Base, ModulePanel? parent = null)
        {
            ParentPanel = parent;
            AddModule(Base);
            Modules.Add(Base);
            OverlayPanel.Children.Add(TabBar);
            Content = OverlayPanel;
#if false // Splits container upon right clicking
            MouseRightButtonDown += (sender, args) =>
            {
                ModuleContainer NewContainer = (ModuleContainer)Activator.CreateInstance(typeof(ModuleContainer), new ItemPreviewModule(), null)!;
                if (ParentPanel != null)
                    ParentPanel.MakeSeperator(this, NewContainer);
            };
#endif
        }

        /// <summary>
        /// Creates a new tab with the <see cref="ModuleBase"/> widget.
        /// </summary>
        /// <param name="module"></param>
        public void AddModule(ModuleBase module)
        {
            module.StartupModule();
            TabBar.Items.Add(new TabItem() { Header = new TextBlock() { FontSize = 18, Foreground = Brushes.Black, Text = module.ModuleName },  Content = module  } );
        }

        /// <summary>
        /// Removes tab.
        /// </summary>
        /// <param name="module">Module to remove.</param>
        public void RemoveModule(ModuleBase module)
        {

        }
    }
}
