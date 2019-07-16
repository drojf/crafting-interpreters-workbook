using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    interface LoxCallable
    {
        int Arity();
        object Call(Interpreter interpreter, List<object> arguments);
    }
}
