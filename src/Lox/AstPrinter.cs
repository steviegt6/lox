using System.Text;

namespace Lox;

public class AstPrinter : Expr.IVisitor<string>
{
    public string VisitAssignExpr(Expr.Assign expr) {
        throw new System.NotImplementedException();
    }

    public string VisitBinaryExpr(Expr.Binary expr) {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitGroupingExpr(Expr.Grouping expr) {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr) {
        return expr.Value is null ? "nil" : expr.Value.ToString()!;
    }

    public string VisitUnaryExpr(Expr.Unary expr) {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    public string VisitVariableExpr(Expr.Variable expr) {
        throw new System.NotImplementedException();
    }

    public string Print(Expr expr) {
        return expr.Accept(this);
    }

    private string Parenthesize(string name, params Expr[] expressions) {
        StringBuilder sb = new();

        sb.Append('(');
        sb.Append(name);

        foreach (Expr expression in expressions) {
            sb.Append(' ');
            sb.Append(expression.Accept(this));
        }

        sb.Append(')');

        return sb.ToString();
    }
}