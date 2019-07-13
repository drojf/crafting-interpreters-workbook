﻿//THIS FILE IS AUTOMATICALLY GENERATED - do not edit manually
using System;

using System.Collections.Generic;


namespace craftinginterpreters2
{
public abstract class Expr {

   public interface Visitor<R> {
    R VisitAssignExpr (Assign expr);
    R VisitBinaryExpr (Binary expr);
    R VisitGroupingExpr (Grouping expr);
    R VisitLiteralExpr (Literal expr);
    R VisitLogicalExpr (Logical expr);
    R VisitUnaryExpr (Unary expr);
    R VisitVariableExpr (Variable expr);
    }

    public abstract R Accept<R>(Visitor<R> visitor);

    public class Assign : Expr {
        public readonly Token name;
        public readonly Expr value;

        public Assign (Token name, Expr value) {
            this.name = name;
            this.value = value;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitAssignExpr(this);
        }
    }

    public class Binary : Expr {
        public readonly Expr left;
        public readonly Token op;
        public readonly Expr right;

        public Binary (Expr left, Token op, Expr right) {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitBinaryExpr(this);
        }
    }

    public class Grouping : Expr {
        public readonly Expr expression;

        public Grouping (Expr expression) {
            this.expression = expression;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitGroupingExpr(this);
        }
    }

    public class Literal : Expr {
        public readonly Object value;

        public Literal (Object value) {
            this.value = value;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class Logical : Expr {
        public readonly Expr left;
        public readonly Token op;
        public readonly Expr right;

        public Logical (Expr left, Token op, Expr right) {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitLogicalExpr(this);
        }
    }

    public class Unary : Expr {
        public readonly Token op;
        public readonly Expr right;

        public Unary (Token op, Expr right) {
            this.op = op;
            this.right = right;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitUnaryExpr(this);
        }
    }

    public class Variable : Expr {
        public readonly Token name;

        public Variable (Token name) {
            this.name = name;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitVariableExpr(this);
        }
    }

}
}
