using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class Parser
    {
        private class ParseError : Exception { }

        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Expr parse()
        {
            try
            {
                return expression();
            }
            catch(ParseError error)
            {
                return null;
            }
        }

        private Expr expression()
        {
            return equality();
        }

        private Expr equality()
        {
            // The default returned value if while loop never executes is just the result of comp()
            Expr expr = comp();

            while(match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = previous();
                Expr right = comp();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr comp()
        {
            Expr expr = addition();

            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = previous();
                Expr right = addition();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr addition()
        {
            Expr expr = multiplication();

            while (match(TokenType.MINUS, TokenType.PLUS))
            {
                Token op = previous();
                Expr right = multiplication();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr multiplication()
        {
            Expr expr = unary();

            while (match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr unary()
        {
            if(match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = previous();
                Expr right = unary();
                return new Expr.Unary(op, right);
            }

            return primary();
        }

        private Expr primary()
        {
            if(match(TokenType.FALSE))
            {
                return new Expr.Literal(false);
            }

            if(match(TokenType.TRUE))
            {
                return new Expr.Literal(true);
            }

            if(match(TokenType.NIL))
            {
                return new Expr.Literal(null);
            }

            if(match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(previous().literal);
            }

            if(match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expresssion.");
                return new Expr.Grouping(expr);
            }

            throw error(peek(), "Expect expression.");
        }

        //Try to consume a token, but error out if the type does not match
        private Token consume(TokenType type, string message)
        {
            if(check(type))
            {
                return advance();
            }

            throw error(peek(), message);
        }

        private ParseError error(Token token, string message)
        {
            Lox.error(token, message);
            return new ParseError();
        }

        private void synchronize()
        {
            advance();

            while(!isAtEnd())
            {
                if(previous().type == TokenType.SEMICOLON)
                {
                    return;
                }

                switch(peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                advance();
            }
        }

        //advance if current token is of any of the given types
        private Boolean match(params TokenType[] types)
        {
            foreach(TokenType type in types)
            {
                if(check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        //Check current token is of given type
        private bool check(TokenType type)
        {
            if(isAtEnd())
            {
                return false;
            }

            return peek().type == type;
        }

        private Token advance()
        {
            if(!isAtEnd())
            {
                current++;
            }

            return previous();
        }

        private Boolean isAtEnd()
        {
            return peek().type == TokenType.EOF;
        }

        private Token peek()
        {
            return tokens[current];
        }

        private Token previous()
        {
            return tokens[current - 1];
        }


    }
}
