using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TModel
{
    public interface HexPosRef
    {
        public int HexPosition { get; protected set; }

        public string HexFileName { get; protected set; }
    }
}
