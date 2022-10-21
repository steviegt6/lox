using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lox;

public class Parser
{
    [Serializable]
    public class ParseException : Exception
    {
        public ParseException() { }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, Exception inner) : base(message, inner) { }

        protected ParseException(
            SerializationInfo info,
            StreamingContext context
        ) : base(info, context) { }
    }

    private readonly List<Token> Tokens;
    private int Current = 0;

    public Parser(List<Token> tokens) {
        Tokens = tokens;
    }

    public Program Parse() {
        Program statements = new();
        while (!IsAtEnd()) statements.Add(Declaration());
        return statements;
    }

    private Expr Expression() {
        return Assignment();
    }

    private Expr Assignment() {
        Expr expr = Or();

        if (Match(EQUAL)) {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable variable) {
                Token name = variable.Name;
                return new Expr.Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or() {
        Expr expr = And();

        while (Match(OR)) {
            Token op = Previous();
            Expr right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And() {
        Expr expr = Equality();

        while (Match(AND)) {
            Token op = Previous();
            Expr right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr Equality() {
        Expr expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL)) {
            Token op = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison() {
        Expr expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL)) {
            Token op = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term() {
        Expr expr = Factor();

        while (Match(MINUS, PLUS)) {
            Token op = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor() {
        Expr expr = Unary();

        while (Match(SLASH, STAR)) {
            Token op = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary() {
        if (Match(BANG, MINUS)) {
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        return Primary();
    }

    private Expr Primary() {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING)) {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(IDENTIFIER)) {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN)) {
            Expr expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Stmt? Declaration() {
        try {
            if (Match(VAR)) return VarDeclaration();
            return Statement();
        }
        catch (ParseException e) {
            Synchronize();
            return null;
        }
    }

    private Stmt VarDeclaration() {
        Token name = Consume(IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (Match(EQUAL)) {
            initializer = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private Stmt Statement() {
        if (Match(IF)) return IfStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(LEFT_BRACE)) return new Stmt.Block(Block());
        return ExpressionStatement();
    }

    private Stmt IfStatement() {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(ELSE)) {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement() {
        Expr value = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt ExpressionStatement() {
        Expr expr = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Expression(expr);
    }

    private List<Stmt?> Block() {
        List<Stmt?> statements = new();

        while (!Check(RIGHT_BRACE) && !IsAtEnd()) {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Token Consume(TokenType type, string message) {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private bool Match(params TokenType[] types) {
        foreach (TokenType type in types) {
            if (Check(type)) {
                Advance();
                return true;
            }
        }

        return false;
    }

    private void Synchronize() {
        Advance();

        while (!IsAtEnd()) {
            if (Previous().Type == SEMICOLON) return;

            switch (Peek().Type) {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }

    private bool Check(TokenType type) {
        return !IsAtEnd() && Peek().Type == type;
    }

    private Token Advance() {
        if (!IsAtEnd()) Current++;
        return Previous();
    }

    private bool IsAtEnd() {
        return Peek().Type == EOF;
    }

    private Token Peek() {
        return Tokens[Current];
    }

    private Token Previous() {
        return Tokens[Current - 1];
    }

    private ParseException Error(Token token, string message) {
        Lox.Error(token, message);
        return new ParseException();
    }
}