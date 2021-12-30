using CUE4Parse.UE4.Vfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TModel.Modules;

namespace TModel.Sorters
{
    public class NameSort : IComparer<IAesVfsReader>, IComparer<DirectoryModule.AssetItem>, IComparer<DirectoryModule.FolderItem>
    {
        public int Compare(IAesVfsReader? x, IAesVfsReader? y)
        {
            return x.Name.CompareTo(y.Name);
        }

        public int Compare(DirectoryModule.AssetItem? x, DirectoryModule.AssetItem? y)
        {
            return x.Name.CompareTo(y.Name);
        }

        public int Compare(DirectoryModule.FolderItem? x, DirectoryModule.FolderItem? y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }

    public class SizeSort : IComparer<IAesVfsReader>
    {
        public int Compare(IAesVfsReader? x, IAesVfsReader? y)
        {
            return x.FileCount.CompareTo(y.FileCount);
        }
    }
}
