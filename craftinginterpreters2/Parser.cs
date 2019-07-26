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

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while(!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if(Match(TokenType.CLASS))
                {
                    return ClassDeclaration();
                }

                if(Match(TokenType.FUN))
                {
                    return function("function");
                }

                if(Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch(ParseError error)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt ClassDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect class name");
            Consume(TokenType.LEFT_BRACE, "Expect '{' before class body");

            List<Stmt.Function> methods = new List<Stmt.Function>();
            while(!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                methods.Add(function("method"));
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

            return new Stmt.Class(name, methods);
        }

        // "kind" is used to differentiate a class-method from a standalone method
        private Stmt.Function function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");

            // Parse parameter list
            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
            List<Token> parameters = new List<Token>();
            if(!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 parameters.");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
            List<Stmt> body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if(Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration");
            return new Stmt.Var(name, initializer);
        }

        private Stmt Statement()
        {
            if(Match(TokenType.FOR))
            {
                return ForStatement();
            }

            if(Match(TokenType.IF))
            {
                return IfStatement();
            }

            if(Match(TokenType.PRINT))
            {
                return PrintStatement();
            }

            if(Match(TokenType.RETURN))
            {
                return returnStatement();
            }

            if(Match(TokenType.WHILE))
            {
                return WhileStatement();
            }

            if(Match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(Block());
            }

            return ExpressionStatement();
        }

        private Stmt returnStatement()
        {
            Token keyword = Previous();

            //Expect return value if next token is not ';'
            Expr value = null;
            if(!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after return value");

            return new Stmt.Return(keyword, value);
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            // This is the for loop body provided by the user
            Stmt forLoopBody = Statement();

            //////////////// The for loop is rewritten as a while loop. ///////////
            //Copy for loop body into a list of while loop statements
            List<Stmt> whileLoopStatements = new List<Stmt> { forLoopBody };

            //If an increment was given, append it to the list of while loop statements
            if (increment != null)
            {
                whileLoopStatements.Add(new Stmt.Expression(increment));
            }

            //Form the while loop, without the initializer
            //Condition is forced to "true" if the for loop had no condition expression
            Stmt whileLoop = new Stmt.While(
                condition ?? new Expr.Literal(true),
                new Stmt.Block(whileLoopStatements)
            );

            //If no initializer given, just return the while loop.
            //Otherwise, insert the initializer just above the while loop
            return initializer == null ? whileLoop : new Stmt.Block(new List<Stmt>
            {
                initializer,
                whileLoop,
            });
        }


        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect '(' after 'if'.");

            Stmt thenBranch = Statement();

            // Else statement is optional
            Stmt elseBranch = null;
            if(Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();

            while(!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value");
            return new Stmt.Print(value);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Or()
        {
            Expr expr = And();

            while(Match(TokenType.OR))
            {
                Token op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(TokenType.AND))
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if(Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                switch(expr)
                {
                    case Expr.Variable variableExpression:
                        return new Expr.Assign(variableExpression.name, value);
                    case Expr.Get getExpression:
                        return new Expr.Set(getExpression.obj, getExpression.name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Equality()
        {
            // The default returned value if while loop never executes is just the result of comp()
            Expr expr = Comp();

            while(Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comp();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comp()
        {
            Expr expr = Addition();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Addition();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token op = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if(Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();
            while(true)
            {
                if(Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else if(Match(TokenType.DOT))
                {
                    Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'");
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        // Normally the 'callee' argument is an identifier.
        // Then this function will return a Expr.Call() with the calee as first argument.
        // What happens if you do 1() ?
        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if(!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if(arguments.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if(Match(TokenType.FALSE))
            {
                return new Expr.Literal(false);
            }

            if(Match(TokenType.TRUE))
            {
                return new Expr.Literal(true);
            }

            if(Match(TokenType.NIL))
            {
                return new Expr.Literal(null);
            }

            if(Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().literal);
            }

            if(Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expresssion.");
                return new Expr.Grouping(expr);
            }

            if(Match(TokenType.THIS))
            {
                return new Expr.This(Previous());
            }

            if(Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            throw Error(Peek(), "Expect expression.");
        }

        //Try to consume a token, but error out if the type does not match
        private Token Consume(TokenType type, string message)
        {
            if(Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while(!IsAtEnd())
            {
                if(Previous().type == TokenType.SEMICOLON)
                {
                    return;
                }

                switch(Peek().type)
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

                Advance();
            }
        }

        //advance if current token is of any of the given types
        private bool Match(params TokenType[] types)
        {
            foreach(TokenType type in types)
            {
                if(Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        //Check current token is of given type
        private bool Check(TokenType type)
        {
            if(IsAtEnd())
            {
                return false;
            }

            return Peek().type == type;
        }

        private Token Advance()
        {
            if(!IsAtEnd())
            {
                current++;
            }

            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().type == TokenType.EOF;
        }

        private Token Peek()
        {
            return tokens[current];
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }
    }
}
