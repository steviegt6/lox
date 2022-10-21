using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Lox.AstGenerator;

[Generator]
public class Generator : ISourceGenerator
{
    private readonly record struct TypeProperty(string Type, string Name, string? DefaultValue)
    {
        public override string ToString() {
            StringBuilder sb = new();

            sb.Append(Type);
            sb.Append(' ');
            sb.Append(Name);

            if (DefaultValue is not null) {
                sb.Append(" = ");
                sb.Append(DefaultValue);
            }

            return sb.ToString();
        }
    }

    private readonly record struct AppendableType(string Name, params TypeProperty[] Properties);

    private static readonly AppendableType[] ExprTypes =
    {
        new(
            Name: "Binary",
            Properties: new TypeProperty[]
            {
                new("Expr", "Left", null),
                new("Token", "Operator", null),
                new("Expr", "Right", null)
            }
        ),

        new(
            Name: "Grouping",
            Properties: new TypeProperty[]
            {
                new("Expr", "Expression", null)
            }
        ),

        new(
            Name: "Literal",
            Properties: new TypeProperty[]
            {
                new("object?", "Value", null)
            }
        ),

        new(
            Name: "Unary",
            Properties: new TypeProperty[]
            {
                new("Token", "Operator", null),
                new("Expr", "Right", null)
            }
        ),

        new(
            Name: "Variable",
            Properties: new TypeProperty[]
            {
                new("Token", "Name", null),
            }
        ),

        new(
            Name: "Assign",
            Properties: new TypeProperty[]
            {
                new("Token", "Name", null),
                new("Expr", "Value", null)
            }
        ),

        new(
            Name: "Logical",
            Properties: new TypeProperty[]
            {
                new("Expr", "Left", null),
                new("Token", "Operator", null),
                new("Expr", "Right", null)
            }
        )
    };

    private static readonly AppendableType[] StmtTypes =
    {
        new(
            Name: "Expression",
            Properties: new TypeProperty[]
            {
                new("Expr", "Expr", null)
            }
        ),

        new(
            Name: "Print",
            Properties: new TypeProperty[]
            {
                new("Expr", "Expr", null)
            }
        ),

        new(
            Name: "Var",
            Properties: new TypeProperty[]
            {
                new("Token", "Name", null),
                new("Expr?", "Initializer", null)
            }
        ),

        new(
            Name: "Block",
            Properties: new TypeProperty[]
            {
                new("System.Collections.Generic.List<Stmt>", "Statements", null)
            }
        ),

        new(
            Name: "If",
            Properties: new TypeProperty[]
            {
                new("Expr", "Condition", null),
                new("Stmt", "ThenBranch", null),
                new("Stmt?", "ElseBranch", null)
            }
        ),

        new(
            Name: "While",
            Properties: new TypeProperty[]
            {
                new("Expr", "Condition", null),
                new("Stmt", "Body", null)
            }
        )
    };

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context) {
        Generate(context, "Expr", "expr", ExprTypes);
        Generate(context, "Stmt", "stmt", StmtTypes);
    }

    private void Generate(GeneratorExecutionContext context, string className, string paramName, AppendableType[] types) {
        StringBuilder sb = new();

        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Lox;");
        sb.AppendLine();

        GenerateClass(sb, className, paramName, types);

        context.AddSource($"{className}.g.cs", sb.ToString());
    }

    private void GenerateClass(StringBuilder sb, string className, string paramName, params AppendableType[] types) {
        types = types.OrderBy(x => x.Name).ToArray();

        sb.AppendLine($"public abstract partial record {className} {{");

        GenerateVisitorInterface(sb, className, paramName, types);

        foreach (AppendableType type in types) {
            const int spaces = 4;

            sb.AppendLineWithSpaces(spaces, $"public record {type.Name}({string.Join(", ", type.Properties)}) : {className} {{");
            sb.AppendLineWithSpaces(spaces * 2, $"public override T Accept<T>(IVisitor<T> visitor) {{ return visitor.Visit{type.Name}{className}(this); }}");
            sb.AppendLineWithSpaces(spaces, "}");
        }

        sb.AppendLine("}");
    }

    private void GenerateVisitorInterface(StringBuilder sb, string className, string paramName, params AppendableType[] types) {
        const int spaces = 4;

        sb.AppendLineWithSpaces(spaces, "public partial interface IVisitor<out T> {");

        foreach (AppendableType type in types) {
            sb.AppendLineWithSpaces(spaces * 2, $"T Visit{type.Name}{className}({type.Name} {paramName});");
        }

        sb.AppendLineWithSpaces(spaces, "}");
    }
}

public static class StringBuilderExtensions
{
    public static void AppendLineWithSpaces(this StringBuilder sb, int spaces, string? value = null) {
        sb.Append(' ', spaces);
        sb.AppendLine(value);
    }
}