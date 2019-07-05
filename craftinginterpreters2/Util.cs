using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    public static class Util
    {
        public static string JavaSubString(this string str, int start, int end)
        {
            return str.Substring(start, end - start);
        }
    }
}
