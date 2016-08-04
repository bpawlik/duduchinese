using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC_CEDICT.Universal
{
    public abstract class ILine
    {
        public int Index;
        public abstract void Initialize(ref byte[] data);
    }
}
