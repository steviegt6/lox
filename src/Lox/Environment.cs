using System.Collections.Generic;

namespace Lox;

public class Environment
{
    private readonly Environment? Enclosing;
    private readonly Dictionary<string, object?> Values = new();

    public Environment(Environment? enclosing = null) {
        Enclosing = enclosing;
    }

    public object? Get(Token name) {
        if (Values.ContainsKey(name.Lexeme)) return Values[name.Lexeme];
        if (Enclosing is not null) return Enclosing.Get(name);
        throw new RuntimeException(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public void Define(string name, object? value) {
        Values.Add(name, value);
    }

    public void Assign(Token name, object? value) {
        if (Values.ContainsKey(name.Lexeme)) {
            Values[name.Lexeme] = value;
            return;
        }

        if (Enclosing is not null) {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeException(name, "Undefined variable '" + name.Lexeme + "'.");
    }
}