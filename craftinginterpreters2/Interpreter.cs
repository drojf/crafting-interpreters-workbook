using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class MyVoid
    {
        public MyVoid()
        {
            throw new InvalidOperationException("Nothing isn't supposed to be instantiated");
        }
    }

    class Interpreter : Expr.Visitor<object>, Stmt.Visitor<MyVoid>
    {
        public readonly VariableEnvironment globals;
        private VariableEnvironment environment;
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        private class ClockFn : LoxCallable
        {
            public int Arity()
            {
                return 0;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return ((double)DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond;
            }

            public override string ToString()
            {
                return "<native fn>";
            }
        }

        public Interpreter()
        {
            this.globals = new VariableEnvironment();
            this.environment = globals;

            //Define built-in function to get the time in milliseconds
            globals.Define("clock", new ClockFn());
        }

        public void Intepret(List<Stmt> statements)
        {
            try
            {
                foreach(Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch(RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch(expr.op.type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.MINUS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    
                    if(left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.op, "Operands must be two numbers or two strings.");
                case TokenType.SLASH:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
            }

            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch(expr.op.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.op, right);
                    return -(double)right;
            }

            return null;
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        } 

        private string Stringify(object obj)
        {
            if(obj == null)
            {
                return "nil";
            }

            return obj.ToString();
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if(left is double && right is double)
            {
                return;
            }

            throw new RuntimeError(op, "Operands must be numbers");
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if(operand is Double)
            {
                return;
            }

            throw new RuntimeError(op, "Operand must be a number.");
        }

        private bool IsEqual(Object a, Object b)
        {
            return Equals(a, b);
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public MyVoid VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public MyVoid VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public MyVoid VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if(stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            //return environment.Get(expr.name);
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if(locals.TryGetValue(expr, out int distance))
            {
                return environment.GetAt(distance, name.lexeme);
            }
            else
            {
                return globals.Get(name);
            }
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);

            if(locals.TryGetValue(expr, out int distance))
            {
                environment.AssignAt(distance, expr.name, value);
            }
            else
            {
                globals.Assign(expr.name, value);
            }

            return value;
        }

        public MyVoid VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new VariableEnvironment(environment));
            return null;
        }

        public void ExecuteBlock(List<Stmt> statements, VariableEnvironment environment)
        {
            //save the last environment (to be restored later)
            VariableEnvironment previous = this.environment;

            try
            {
                //change environment to the given one, and execute statments with this new environment
                this.environment = environment;
                foreach(Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                //even if there's an exception, we need to restore the last environment before continuing
                this.environment = previous;
            }
        }

        private bool isTruthy(object o)
        {
            switch(o) {
                case bool b:
                    return b;

                default:
                    return o == null ? false : true;
            }
        }

        public MyVoid VisitIfStmt(Stmt.If stmt)
        {
            if(isTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            } 
            else if(stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }

            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.op.type == TokenType.OR)
            {
                if (isTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if (!isTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(expr.right);
        }

        public MyVoid VisitWhileStmt(Stmt.While stmt)
        {
            while(isTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }

            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach(Expr argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if(!(callee is LoxCallable))
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            LoxCallable function = (LoxCallable)callee;

            //check number of arguments before calling
            if(arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} but got {arguments.Count}");
            }

            return function.Call(this, arguments);
        }

        public MyVoid VisitFunctionStmt(Stmt.Function stmt)
        {
            // Capture the environment when the function is declared, for closures
            LoxFunction function = new LoxFunction(stmt, environment);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public MyVoid VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if(stmt.value != null)
            {
                value = Evaluate(stmt.value);
            }

            throw new Return(value);
        }
    }
}
