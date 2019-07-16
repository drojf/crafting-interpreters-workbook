using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class LoxFunction : LoxCallable
    {
        private readonly Stmt.Function declaration;
        private readonly VariableEnvironment closure;

        public LoxFunction(Stmt.Function declaration, VariableEnvironment closure)
        {
            this.closure = closure;
            this.declaration = declaration;
        }

        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            VariableEnvironment environment = new VariableEnvironment(closure);

            for (int i = 0; i < declaration.parameters.Count; i++)
            {
                environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch(Return returnValue)
            {
                return returnValue.value;
            }

            return null;
        }

        public override string ToString()
        {
            return $"<fn {declaration.name.lexeme}>";
        }
    }
}
