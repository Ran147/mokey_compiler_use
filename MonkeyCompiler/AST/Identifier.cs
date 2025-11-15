// AST/Identifier.cs
namespace MonkeyCompiler.AST;
using MonkeyCompiler.Encoder;

public class Identifier : Expression
{
    public string Name { get; }

    public Identifier(string name)
    {
        Name = name;
    }
    public override string GetAstRepresentation() => $"ID({Name})";
    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}