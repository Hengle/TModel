using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace TModel
{
    /// <summary>
    /// Interface for a module
    /// </summary>
    public class ModuleBase : ContentControl
    {
        /// <summary>
        /// The display name in the UI for this module
        /// </summary>
        /// <remarks>
        /// Don't confuse this with <see cref="FrameworkElement.Name"/>
        /// </remarks>
        public string ModuleName { get; protected set; } = "";

        public ModuleBase() : base()
        {

        }

        /// <summary>
        /// Loads UI elements for this module
        /// </summary>
        public virtual void StartupModule()
        {
#if DEBUG
            Content = new TextBlock()
            {
                Background = System.Windows.Media.Brushes.Black,
                Foreground = System.Windows.Media.Brushes.White,
                Text = $"{this.GetType().ToString()} does no override StartupModule()"
            };
#endif
        }

        /// <summary>
        /// Creates a new instance of <see cref="ModuleContainer"/> with the first tab being this module.
        /// </summary>
        /// <returns>The <see cref="ModuleContainer"/> containing this module.</returns>
        public ModuleContainer Container() => new ModuleContainer(this);
    }
}
