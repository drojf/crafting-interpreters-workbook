using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class LoxInstance
    {
        private LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public override string ToString()
        {
            return $"object<{klass.name}>";
        }

        //Get property, given some token on this particular instance
        public object Get(Token name)
        {
            if(fields.TryGetValue(name.lexeme, out object value))
            {
                return value;
            }

            LoxFunction method = klass.FindMethod(name.lexeme);
            if (method != null)
                return method.Bind(this);

            //If instance doesn't have property, it's an error
            throw new RuntimeError(name, $"Undefined property '{name.lexeme}'");
        }

        public void Set(Token name, object value)
        {
            fields[name.lexeme] = value;
        }
    }
}
