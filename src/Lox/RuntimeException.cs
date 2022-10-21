using System;
using System.Runtime.Serialization;

namespace Lox;

[Serializable]
public class RuntimeException : Exception
{
    public Token Token { get; }

    public RuntimeException(Token token) {
        Token = token;
    }

    public RuntimeException(Token token, string message) : base(message) {
        Token = token;
    }

    public RuntimeException(Token token, string message, Exception inner) : base(message, inner) {
        Token = token;
    }

    protected RuntimeException(
        SerializationInfo info,
        StreamingContext context
    ) : base(info, context) {
        Token = new Token(default, string.Empty, null, default);
    }
}