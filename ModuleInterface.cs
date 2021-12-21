using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TModel
{
    /// <summary>
    /// Interface for a module
    /// </summary>
    public abstract class ModuleInterface : ContentControl
    {
        /// <summary>
        /// The display name in the UI for this module
        /// </summary>
        public abstract string ModuleName { get; }

        /// <summary>
        /// Loads UI elements for this module
        /// </summary>
        public abstract void StartupModule();
    }
}
