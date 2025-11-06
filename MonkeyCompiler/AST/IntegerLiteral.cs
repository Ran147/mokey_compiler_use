// AST/IntegerLiteral.cs
namespace MonkeyCompiler.AST;

public class IntegerLiteral : Expression
{
    public int Value { get; }

    public IntegerLiteral(int value)
    {
        Value = value;
        Type = "int";
    }
    public override string GetAstRepresentation() => $"INT({Value})";
}