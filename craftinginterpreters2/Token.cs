using System;
using System.Collections.Generic;
using System.Text;

namespace craftinginterpreters2
{
    class Token
    {
        readonly TokenType type;
        readonly String lexeme;
        readonly Object literal;
        readonly int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public override String ToString()
        {
            return type + " " + lexeme + " " + literal;
        }
    }
}
