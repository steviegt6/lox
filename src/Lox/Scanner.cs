using System;
using System.Collections.Generic;

namespace Lox;

public class Scanner
{
    public static readonly Dictionary<string, TokenType> Keywords = new()
    {
        {"and", AND},
        {"class", CLASS},
        {"else", ELSE},
        {"false", FALSE},
        {"for", FOR},
        {"fun", FUN},
        {"if", IF},
        {"nil", NIL},
        {"or", OR},
        {"print", PRINT},
        {"return", RETURN},
        {"super", SUPER},
        {"this", THIS},
        {"true", TRUE},
        {"var", VAR},
        {"while", WHILE}
    };

    private readonly char[] Source;
    private readonly List<Token> Tokens = new();
    private int Start = 0;
    private int Current = 0;
    private int Line = 1;

    public Scanner(string source) {
        Source = source.ToCharArray();
    }

    public List<Token> ScanTokens() {
        while (!IsAtEnd()) {
            Start = Current;
            ScanToken();
        }

        Tokens.Add(new Token(EOF, "", null, Line));
        return Tokens;
    }

    private void ScanToken() {
        char c = Advance();

        switch (c) {
            #region Single-character tokens

            case '(':
                AddToken(LEFT_PAREN);
                break;

            case ')':
                AddToken(RIGHT_PAREN);
                break;

            case '{':
                AddToken(LEFT_BRACE);
                break;

            case '}':
                AddToken(RIGHT_BRACE);
                break;

            case ',':
                AddToken(COMMA);
                break;

            case '.':
                AddToken(DOT);
                break;

            case '-':
                AddToken(MINUS);
                break;

            case '+':
                AddToken(PLUS);
                break;

            case ';':
                AddToken(SEMICOLON);
                break;

            case '*':
                AddToken(STAR);
                break;

            #endregion

            #region Tokens with potentially two characters

            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;

            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;

            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;

            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;

            #endregion

            #region Whitespace

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace
                break;
            
            case '\n':
                Line++;
                break;

            #endregion

            #region Special cases / Literals

            case '/':
                if (Match('/')) {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                    AddToken(SLASH);

                break;

            case '"':
                String();
                break;

            default:
                if (IsDigit(c))
                    Number();
                else if (IsAlpha(c))
                    Identifier();
                else
                    Lox.Error(Line, "Unexpected character: " + c);

                break;

            #endregion
        }
    }

    private void AddToken(TokenType type, object? literal = null) {
        string text = new(Source[Start..Current]);
        Tokens.Add(new Token(type, text, literal, Line));
    }

    private bool Match(char expected) {
        if (IsAtEnd()) return false;
        if (Source[Current] != expected) return false;

        Current++;
        return true;
    }

    private void String() {
        // TODO: support escape sequences?
        while (Peek() != '"' && !IsAtEnd()) {
            // TODO: lol option to disallow newlines?
            if (Peek() == '\n') Line++;
            Advance();
        }

        if (IsAtEnd()) {
            Lox.Error(Line, "Unterminated string.");
            return;
        }

        // The closing '"'.
        Advance();

        string value = new(Source[(Start + 1)..(Current - 1)]);
        AddToken(STRING, value);
    }

    private void Number() {
        while (IsDigit(Peek())) Advance();

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext()))
            // Consume the '.'.
            Advance();

        while (IsDigit(Peek())) Advance();

        AddToken(NUMBER, double.Parse(Source[Start..Current].AsSpan()));
    }

    private void Identifier() {
        while (IsAlphaNumeric(Peek())) Advance();
        string text = new(Source[Start..Current]);
        if (!Keywords.TryGetValue(text, out TokenType type)) type = IDENTIFIER;
        AddToken(type);
    }

    private char Advance() {
        return Source[Current++];
    }

    private char Peek() {
        return IsAtEnd() ? '\0' : Source[Current];
    }

    private char PeekNext() {
        return Current + 1 >= Source.Length ? '\0' : Source[Current + 1];
    }

    private bool IsAtEnd() {
        return Current >= Source.Length;
    }

    private static bool IsDigit(char c) {
        return c is >= '0' and <= '9';
    }

    private static bool IsAlpha(char c) {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    private static bool IsAlphaNumeric(char c) {
        return IsAlpha(c) || IsDigit(c);
    }
}