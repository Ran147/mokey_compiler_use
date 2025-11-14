namespace MonkeyCompiler.AST
{
    public class SimpleTypeNode : TypeNode
    {
        // El nombre del tipo, ej: "int", "string"
        public string TypeName { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation() => TypeName;
    }
}