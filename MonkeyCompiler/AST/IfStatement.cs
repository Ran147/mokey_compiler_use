using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class IfStatement : Statement
    {
        public Expression Condition { get; set; }
        public BlockStatement Consequence { get; set; }
        public BlockStatement? Alternative { get; set; }

        // FIX: Implementación del método abstracto (puedes hacerlo más detallado si quieres)
        public override string GetAstRepresentation()
        {
            string alt = Alternative != null ? " ELSE ..." : "";
            return $"IF ({Condition.GetAstRepresentation()}) {{ ... }}{alt}";
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}