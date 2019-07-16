using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class Return : Exception
    {
        public readonly object value;

        public Return(object value)
        {
            this.value = value;
        }
    }
}
