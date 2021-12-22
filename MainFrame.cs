using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TModel
{
    /// <summary>
    /// Window that displays one/mutiple <see cref="ModuleBase"/>(s).
    /// </summary>
    public static class MainFrame
    {
        public static Window Window { get; } = new Window();
    }
}
