using System;
using System.Collections.Generic;
using System.IO;
using SysEnv = System.Environment;

namespace Lox;

public static class Lox
{
    private static readonly Interpreter Interpreter = new();
    
    private static bool HadError;
    private static bool HadRuntimeError;

    public static void Main(string[] args) {
        if (args.Length > 1) {
            Console.WriteLine("Usage: dotnet Lox.dll [script]");
            SysEnv.Exit(64);
        }

        if (args.Length == 1) {
            RunFile(args[0]);
        }
        else {
            RunPrompt();
        }
    }

    private static void RunFile(string path) {
        Run(File.ReadAllText(path));
        if (HadError) SysEnv.Exit(65);
        if (HadRuntimeError) SysEnv.Exit(75);
    }

    private static void RunPrompt() {
        while (true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line is null or "exit") break;
            Run(line);
            HadError = false;
        }
    }

    private static void Run(string source) {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new(tokens);
        Program program = parser.Parse();

        // Stop if there was a syntax error.
        if (HadError) return;

        Interpreter.Interpret(program);
    }

    public static void Error(int line, string message) {
        Report(line, "", message);
    }

    public static void Error(Token token, string message) {
        if (token.Type == EOF)
            Report(token.Line, " at end", message);
        else
            Report(token.Line, " at '" + token.Lexeme + "'", message);
    }

    private static void Report(int line, string where, string message) {
        Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
    }

    public static void RuntimeError(RuntimeException e) {
        Console.Error.WriteLine(e.Message + "\n[line " + e.Token.Line + ']');
        HadRuntimeError = true;
    }
}