using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class Scanner
    {
        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            ["and"] = TokenType.AND,
            ["class"] = TokenType.CLASS,
            ["else"] = TokenType.ELSE,
            ["false"] = TokenType.FALSE,
            ["for"] = TokenType.FOR,
            ["fun"] = TokenType.FUN,
            ["if"] = TokenType.IF,
            ["nil"] = TokenType.NIL,
            ["or"] = TokenType.OR,
            ["print"] = TokenType.PRINT,
            ["return"] = TokenType.RETURN,
            ["super"] = TokenType.SUPER,
            ["this"] = TokenType.THIS,
            ["true"] = TokenType.TRUE,
            ["var"] = TokenType.VAR,
            ["while"] = TokenType.WHILE,
        };

        private readonly String source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(String source)
        {
            this.source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;

                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case '"': ConsumeString(); break;

                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    line++;
                    break;

                default:
                    if(char.IsDigit(c))
                    {
                        Number();
                    }
                    else if(char.IsLetter(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void Identifier()
        {
            while(char.IsLetter(Peek()))
            {
                Advance();
            }

            String text = source.JavaSubString(start, current);

            if(keywords.TryGetValue(text, out TokenType identifierTokenType))
            {
                AddToken(identifierTokenType);
            }
            else
            {
                AddToken(TokenType.IDENTIFIER);
            }            
        }

        private void Number()
        {
            while(char.IsDigit(Peek()))
            {
                Advance();
            }

            if(Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance();
                while(char.IsDigit(Peek()))
                {
                    Advance();
                }
            }

            AddToken(TokenType.NUMBER, Double.Parse(source.JavaSubString(start, current)));
        }

        private char PeekNext()
        {
            if(current + 1 >= source.Length)
            {
                return '\0';
            }

            return source[current + 1];
        }

        private void ConsumeString() {
            while(Peek() != '"' && !IsAtEnd())
            {
                if(Peek() == '\n')
                {
                    line++;
                }
                Advance();
            }

            if(IsAtEnd())
            {
                Lox.Error(line, "Unterminated string.");
                return;
            }

            // The closing "
            Advance();

            // Trim the surrounding quotes
            String value = source.JavaSubString(start + 1, current - 1);
            AddToken(TokenType.STRING, value);
        }

        private char Peek()
        {
            if(IsAtEnd())
            {
                return '\0';
            }

            return source[current];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            string text = source.JavaSubString(start, current);
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool IsAtEnd()
        {
            return current >= source.Length;
        }

    }
}
