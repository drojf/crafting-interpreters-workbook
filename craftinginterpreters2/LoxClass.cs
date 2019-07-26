using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class LoxClass : LoxCallable
    {
        private readonly bool isInitializer;
        public readonly string name;
        private readonly Dictionary<string, LoxFunction> methods;

        public LoxClass(string name)
        {
            this.name = name;
        }

        public LoxClass(string name, Dictionary<string, LoxFunction> methods) : this(name)
        {
            this.methods = methods;
        }

        public int Arity()
        {
            LoxFunction initializer = FindMethod("init");
            if (initializer == null)
            {
                return 0;
            }
            return initializer.Arity();
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initializer = FindMethod("init");
            if(initializer != null)
            {
                initializer.bind(instance).call(intepreter, arguments);
            }

            if (isInitializer)
            {
                return closure.getAt(0, "this")
            }

            return instance;
        }

        public LoxFunction FindMethod(string name)
        {
            if(methods.TryGetValue(name, out LoxFunction fn))
            {
                return fn;
            }

            return null;
        }


    }
}
