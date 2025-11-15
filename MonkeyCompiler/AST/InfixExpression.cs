using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST

{
    public class InfixExpression : Expression
    {
        public Expression Left { get; set; }
        public string Operator { get; set; } 
        public Expression Right { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation() => $"({Left.GetAstRepresentation()} {Operator} {Right.GetAstRepresentation()})";
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}