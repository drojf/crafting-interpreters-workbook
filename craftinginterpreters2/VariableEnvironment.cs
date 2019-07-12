using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class VariableEnvironment
    {
        readonly VariableEnvironment enclosing;

        public VariableEnvironment()
        {
            enclosing = null;
        }

        public VariableEnvironment(VariableEnvironment enclosing)
        {
            this.enclosing = enclosing;
        }

        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public void Define(string name, object value)
        {
            values[name] = value;
        }

        public object Get(Token name)
        {
            if(values.TryGetValue(name.lexeme, out object value))
            {
                return value;
            }

            if(enclosing != null)
            {
                return enclosing.Get(name);
            }

            throw new RuntimeError(name, $"Undefined variable {name.lexeme}.");
        }

        public void Assign(Token name, Object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if(enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable'{name.lexeme}'.");
        }
    }
}
