// AST/BlockStatement.cs (Necesario para el cuerpo de fn add y fn main)
using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST;

public class BlockStatement : Statement
{
    public List<Statement> Statements { get; } = new List<Statement>();

    public BlockStatement(List<Statement> statements)
    {
        Statements.AddRange(statements);
    }
    public override string GetAstRepresentation() => $"BlockStatement({Statements.Count} statements)";
    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}