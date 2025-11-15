using MonkeyCompiler.Encoder;

namespace MonkeyCompiler.AST
{
    public class ArrayTypeNode : TypeNode
    {
        // El tipo de los elementos, ej: un SimpleTypeNode para "int"
        public TypeNode ElementType { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation() => $"array<{ElementType.GetAstRepresentation()}>";
        public override void Accept(IAstVisitor visitor)
        {
            
        }
    }
}