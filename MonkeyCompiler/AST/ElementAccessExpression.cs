using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class ElementAccessExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Index { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation() => $"({Left.GetAstRepresentation()}[{Index.GetAstRepresentation()}])";
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    
}