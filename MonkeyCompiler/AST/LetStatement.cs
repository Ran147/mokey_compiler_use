// AST/LetStatement.cs
namespace MonkeyCompiler.AST;

public class LetStatement : Statement
{
    public string Name { get; }
    public string DeclaredType { get; }
    public Expression Value { get; }

    public LetStatement(string name, string declaredType, Expression value)
    {
        Name = name;
        DeclaredType = declaredType;
        Value = value;
    }
    public override string GetAstRepresentation() => 
        $"LET {DeclaredType} {Name} = {Value.GetAstRepresentation()}";
}