using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class Resolver : Expr.Visitor<MyVoid>, Stmt.Visitor<MyVoid>
    {
        private enum FunctionType
        {
            None,
            Function,
            Method,
            Initializer,
        }

        private enum ClassType
        {
            None,
            Class,
        }

        private ClassType currentClass = ClassType.None;
        private readonly Interpreter intepreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.None;

        public Resolver(Interpreter intepreter)
        {
            this.intepreter = intepreter;
        }

        public MyVoid VisitAssignExpr(Expr.Assign expr)
        {
            resolve(expr.value);
            resolveLocal(expr, expr.name);
            return null;
        }

        public MyVoid VisitBinaryExpr(Expr.Binary expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public MyVoid VisitBlockStmt(Stmt.Block stmt)
        {
            beginScope();
            resolve(stmt.statements);
            endScope();
            return null;
        }

        public void resolve(List<Stmt> statements)
        {
            foreach(Stmt statement in statements)
            {
                resolve(statement);
            }
        }

        void resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void beginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void endScope()
        {
            scopes.Pop();
        }

        public MyVoid VisitCallExpr(Expr.Call expr)
        {
            resolve(expr.callee);

            foreach(Expr argument in expr.arguments)
            {
                resolve(argument);
            }

            return null;
        }

        public MyVoid VisitExpressionStmt(Stmt.Expression stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public MyVoid VisitFunctionStmt(Stmt.Function stmt)
        {
            declare(stmt.name);
            define(stmt.name);

            resolveFunction(stmt, FunctionType.Function);
            return null;
        }

        private void resolveFunction(Stmt.Function function, FunctionType functionType)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = functionType;

            beginScope();
            foreach(Token param in function.parameters) {
                declare(param);
                define(param);
            }
            resolve(function.body);
            endScope();

            currentFunction = enclosingFunction;
        }

        public MyVoid VisitGroupingExpr(Expr.Grouping expr)
        {
            resolve(expr.expression);
            return null;
        }

        public MyVoid VisitIfStmt(Stmt.If stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.thenBranch);
            if(stmt.elseBranch != null)
            {
                resolve(stmt.elseBranch);
            }

            return null;
        }

        public MyVoid VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public MyVoid VisitLogicalExpr(Expr.Logical expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public MyVoid VisitPrintStmt(Stmt.Print stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public MyVoid VisitReturnStmt(Stmt.Return stmt)
        {
            if(currentFunction == FunctionType.None)
            {
                Lox.Error(stmt.keyword, "Cannot return from top-level code");
            }

            if(stmt.value != null)
            {
                if(currentFunction == FunctionType.Initializer)
                {
                    Lox.Error(stmt.keyword, "Cannot return a value from an initializer");
                }

                resolve(stmt.value);
            }

            return null;
        }

        public MyVoid VisitUnaryExpr(Expr.Unary expr)
        {
            resolve(expr.right);
            return null;
        }

        public MyVoid VisitVariableExpr(Expr.Variable expr)
        {
            if(!(scopes.Count == 0))
            {
                if(scopes.Peek().TryGetValue(expr.name.lexeme, out bool value))
                {
                    if(value == false)
                    {
                        //If you do `var a = a;` then it is an error
                        Lox.Error(expr.name, "Cannot read local variable in its own initializer.");
                    }
                }
                else
                {
                    Lox.Error(expr.name, $"Lookup of {expr.name.lexeme} failed");
                }
            }

            resolveLocal(expr, expr.name);
            return null;
        }

        private void resolveLocal(Expr expr, Token name)
        {
            //TODO: this is probably ineffecient
            var scopesArray = scopes.ToArray();
            for(int i = scopesArray.Length - 1; i >= 0; i--)
            {
                if(scopesArray[i].ContainsKey(name.lexeme)) {
                    intepreter.Resolve(expr, scopesArray.Length - 1 - i);
                    return;
                }
            }

            // Not found. Assume global
        }

        public MyVoid VisitVarStmt(Stmt.Var stmt)
        {
            declare(stmt.name);
            if(stmt.initializer != null) {
                resolve(stmt.initializer);
            }
            define(stmt.name);
            return null;
        }

        private void resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void declare(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }

            Dictionary<string, bool> scope = scopes.Peek();
            if(scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name, "Variable with this name already declared in this scope");
            }

            scope[name.lexeme] = false;
        }

        private void define(Token name)
        {
            if(scopes.Count == 0)
            {
                return;
            }

            scopes.Peek()[name.lexeme] = true;
        }

        public MyVoid VisitWhileStmt(Stmt.While stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.body);
            return null;
        }

        public MyVoid VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.Class;

            declare(stmt.name);
            define(stmt.name);

            beginScope();
            scopes.Peek()["this"] = true;

            foreach (Stmt.Function method in stmt.methods)
            {
                FunctionType declaration = FunctionType.Method;
                if(method.name.lexeme == "init")
                {
                    declaration = FunctionType.Initializer;
                }

                resolveFunction(method, declaration);
            }

            endScope();

            currentClass = enclosingClass;

            return null;
        }

        public MyVoid VisitGetExpr(Expr.Get expr)
        {
            resolve(expr.obj);
            return null;
        }

        public MyVoid VisitSetExpr(Expr.Set expr)
        {
            resolve(expr.value);
            resolve(expr.obj);
            return null;
        }

        public MyVoid VisitThisExpr(Expr.This expr)
        {
            if(currentClass == ClassType.None)
            {
                Lox.Error(expr.keyword, "Cannot use 'this' outside of a class.");
                return null;
            }

            resolveLocal(expr, expr.keyword);
            return null;
        }
    }
}
