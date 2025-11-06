// AST/ProgramNode.cs
namespace MonkeyCompiler.AST;

public class ProgramNode : Node
{
    public List<Node> Declarations { get; } = new List<Node>();

    public override string GetAstRepresentation() => 
        $"Program (Declarations: {Declarations.Count})";
}