using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class LoxFunction : LoxCallable
    {
        private readonly Stmt.Function declaration;
        private readonly VariableEnvironment closure;
        private readonly bool isInitializer;

        public LoxFunction(Stmt.Function declaration, VariableEnvironment closure, Boolean isInitializer)
        {
            this.closure = closure;
            this.declaration = declaration;
            this.isInitializer = isInitializer;
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
                // Allow an empty return ("return;") in a constructor. When this happens, return "this".
                if(isInitializer)
                {
                    return closure.GetAt(0, "this");
                }

                return returnValue.value;
            }

            return null;
        }

        public override string ToString()
        {
            return $"<fn {declaration.name.lexeme}>";
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            VariableEnvironment environment = new VariableEnvironment(closure);
            environment.Define("this", instance);
            return new LoxFunction(declaration, environment, isInitializer);
        }

    }
}
