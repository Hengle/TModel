using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace TModel
{
    /// <summary>
    /// Contains multiple instances of <see cref="ModuleInterface"/> that can be switched using the tab bar
    /// </summary>
    public sealed class ModuleContainer : ContentControl
    {
        private TabControl TabBar = new TabControl();

        /// <summary>
        /// The <see cref="ModuleInterface"/> that is being show in this <see cref="ModuleContainer"/>.
        /// </summary>

        public List<ModuleInterface> Modules { get; } = new List<ModuleInterface>();

        /// <summary>
        /// Creates new instance of <see cref="ModuleContainer"/>.
        /// </summary>
        /// <param name="Base">The first tab module.</param>
        public ModuleContainer(ModuleInterface Base)
        {
            AddModule(Base);
            Modules.Add(Base);
            Content = TabBar;
        }

        /// <summary>
        /// Creates a new tab with the <see cref="ModuleInterface"/> widget.
        /// </summary>
        /// <param name="module"></param>
        public void AddModule(ModuleInterface module)
        {
            module.StartupModule();
            TabBar.Items.Add(new TabItem() { Header = new TextBlock() { FontSize = 12, Foreground = Brushes.Black, Text = module.ModuleName }, Content = module } );
        }

        /// <summary>
        /// Removes tab of module.
        /// </summary>
        /// <param name="module">The module to remove.</param>
        public void RemoveModule(ModuleInterface module)
        {

        }
    }
}
