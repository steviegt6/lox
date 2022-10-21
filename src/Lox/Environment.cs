using System.Collections.Generic;

namespace Lox;

public class Environment
{
    private readonly Dictionary<string, object?> Values = new();

    public object? Get(Token name) {
        if (Values.ContainsKey(name.Lexeme)) return Values[name.Lexeme];
        throw new RuntimeException(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public void Define(string name, object? value) {
        Values.Add(name, value);
    }

    public void Assign(Token name, object? value) {
        if (Values.ContainsKey(name.Lexeme)) {
            Values.Add(name.Lexeme, value);
            return;
        }

        throw new RuntimeException(name, "Undefined variable '" + name.Lexeme + "'.");
    }
}