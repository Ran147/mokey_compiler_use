using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class HashTypeNode : TypeNode
    {
        public TypeNode KeyType { get; set; }
        public TypeNode ValueType { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation() => $"hash<{KeyType.GetAstRepresentation()}, {ValueType.GetAstRepresentation()}>";
        public override void Accept(IAstVisitor visitor)
        {
            
        }
    }
}