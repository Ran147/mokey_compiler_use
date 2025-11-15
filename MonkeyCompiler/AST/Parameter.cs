using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class Parameter : Node
    {
        public Identifier Name { get; set; }
        public TypeNode ValueType { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation() => $"{Name.GetAstRepresentation()} : {ValueType.GetAstRepresentation()}";
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}