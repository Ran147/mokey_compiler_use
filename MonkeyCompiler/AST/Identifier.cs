// AST/Identifier.cs
namespace MonkeyCompiler.AST;

public class Identifier : Expression
{
    public string Name { get; }

    public Identifier(string name)
    {
        Name = name;
    }
    public override string GetAstRepresentation() => $"ID({Name})";
}