using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    internal class RuntimeError : Exception
    {
        public readonly Token token;

        public RuntimeError(Token token, String message) : base(message)
        {
            this.token = token;
        }
    }
}
