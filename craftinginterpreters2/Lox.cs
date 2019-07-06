using System;
using System.Collections.Generic;
using System.IO;

namespace craftinginterpreters2
{
    class Lox
    {
        static bool hadError = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            if(args.Length > 1)
            {
                Console.WriteLine("Usage: jlog [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(String path)
        {
            Run(File.ReadAllText(path));

            if (hadError)
            {
                Environment.Exit(65);
            }
        }

        private static void RunPrompt()
        {
            while(true)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            Expr expression = parser.parse();

            if(hadError)
            {
                return;
            }

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void error(int line, String message)
        {
            report(line, "", message);
        }

        private static void report(int line, String where, String message)
        {
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }

        public static void error(Token token, string message)
        {
            if(token.type == TokenType.EOF)
            {
                report(token.line, " at end", message);
            }
            else
            {
                report(token.line, $" at '{token.lexeme}'", message);
            }
        }
    }
}
