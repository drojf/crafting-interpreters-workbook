using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class AstPrinter : Expr.Visitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return "Printing assign expression not implemeneted";
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if(expr.value == null)
            {
                return "nil";
            }
            return expr.value.ToString();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return "Printing variable expression not implemeneted";
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach(Expr expr in exprs) {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
