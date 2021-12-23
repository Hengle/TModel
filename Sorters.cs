using CUE4Parse.UE4.Vfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel.Sorters
{
    public class NameSort : IComparer<IAesVfsReader>
    {
        public int Compare(IAesVfsReader? x, IAesVfsReader? y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
