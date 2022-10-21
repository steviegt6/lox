namespace Lox;

public abstract partial record Stmt
{
    public partial interface IVisitor<out T>
    {
    }

    public abstract T Accept<T>(IVisitor<T> visit);
}