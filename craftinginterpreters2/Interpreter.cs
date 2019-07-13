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
        private VariableEnvironment environment = new VariableEnvironment();

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
            return environment.Get(expr.name);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

        public MyVoid VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new VariableEnvironment(environment));
            return null;
        }

        private void ExecuteBlock(List<Stmt> statements, VariableEnvironment environment)
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
    }
}
