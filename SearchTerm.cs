using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel
{
    // Specifies filter options for files
    public class SearchTerm
    {
        // ************Search Terms***********

        // The string the the name must start with
        // Examples are 'CID' or 'WID'
        public string[] FileNameStart { set; get; }


        // Empty construct for SearchTerm
        public SearchTerm() { }

        // Checks if the given path is valid according to the search terms.
        // The path must be the full path and must include the extension
        public bool CheckName(string path)
        {
            string FileName = Path.GetFileName(path);

            foreach (var item in FileNameStart)
                if (FileName.StartsWith(item))
                    return true;

            return false;
        }
    }
}
