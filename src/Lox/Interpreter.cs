using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lox;

public class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    private Environment Environment = new();

    public void Interpret(Program program) {
        try {
            foreach (Stmt statement in program) {
                Execute(statement);
            }
        }
        catch (RuntimeException e) {
            Lox.RuntimeError(e);
        }
    }

    #region Expression Visiting

    public object? VisitAssignExpr(Expr.Assign expr) {
        object? value = Evaluate(expr.Value);
        Environment.Assign(expr.Name, value);
        return value;
    }

    public object? VisitBinaryExpr(Expr.Binary expr) {
        object? left = Evaluate(expr.Left);
        object? right = Evaluate(expr.Right);

        switch (expr.Operator.Type) {
            case GREATER:
                CheckNumberOperands(expr.Operator, left, right);
                return (double) left! > (double) right!;

            case GREATER_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double) left! >= (double) right!;

            case LESS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double) left! < (double) right!;

            case LESS_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double) left! <= (double) right!;

            case BANG_EQUAL:
                return !IsEqual(left, right);

            case EQUAL_EQUAL:
                return IsEqual(left, right);

            case MINUS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double) left! - (double) right!;

            case PLUS:
                switch (left) {
                    case double ld when right is double rd:
                        return ld + rd;
                    case string ls when right is string rs:
                        return ls + rs;
                }

                throw new RuntimeException(expr.Operator, "Operands must be two numbers or two strings.");

            case SLASH:
                return (double) left! / (double) right!;

            case STAR:
                CheckNumberOperands(expr.Operator, left, right);
                return (double) left! * (double) right!;
        }

        // Unreachable.
        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr) {
        return Evaluate(expr.Expression);
    }

    public object? VisitLiteralExpr(Expr.Literal expr) {
        return expr.Value;
    }

    public object? VisitLogicalExpr(Expr.Logical expr) {
        object? left = Evaluate(expr.Left);

        if (expr.Operator.Type == OR) {
            if (IsTruthy(left)) return left;
        }
        else {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    public object? VisitUnaryExpr(Expr.Unary expr) {
        object? right = Evaluate(expr.Right);

        switch (expr.Operator.Type) {
            case BANG:
                return !IsTruthy(right);

            case MINUS:
                return -(double) right!;
        }

        // Unreachable.
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr) {
        return Environment.Get(expr.Name);
    }

    #endregion

    #region Statement Visiting

    public object? VisitBlockStmt(Stmt.Block stmt) {
        ExecuteBlock(stmt.Statements, new Environment(Environment));
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt) {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt) {
        if (IsTruthy(Evaluate(stmt.Condition)))
            Execute(stmt.ThenBranch);
        else if (stmt.ElseBranch is not null) Execute(stmt.ElseBranch);

        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt) {
        Console.WriteLine(Stringify(Evaluate(stmt.Expr)));
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt) {
        object? value = null;
        if (stmt.Initializer is not null) value = Evaluate(stmt.Initializer);
        Environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    #endregion

    private void ExecuteBlock(List<Stmt> statements, Environment environment) {
        Environment previous = Environment;

        try {
            Environment = environment;

            foreach (Stmt statement in statements) {
                Execute(statement);
            }
        }
        finally {
            Environment = previous;
        }
    }

    private object? Evaluate(Expr expr) {
        return expr.Accept(this);
    }

    private void Execute(Stmt statement) {
        statement.Accept(this);
    }

    private static string Stringify(object? obj) {
        switch (obj) {
            case null:
                return "nil";
            case double d:
            {
                string text = d.ToString(CultureInfo.InvariantCulture);
                // TODO: Don't think this is needed in C#?
                // if (text.EndsWith(".0")) text = text[..^2];
                return text;
            }
            default:
                return obj.ToString() ?? "nil";
        }
    }

    private static bool IsTruthy(object? obj) {
        return obj switch
        {
            null => false,
            bool b => b,
            _ => true
        };
    }

    private static bool IsEqual(object? left, object? right) {
        if (left is null && right is null) return true;
        if (left is null) return false;
        return left.Equals(right);
    }

    private static void CheckNumberOperand(Token op, object? operand) {
        if (operand is double) return;
        throw new RuntimeException(op, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token op, object? left, object? right) {
        if (left is double && right is double) return;
        throw new RuntimeException(op, "Operands must be numbers.");
    }
}