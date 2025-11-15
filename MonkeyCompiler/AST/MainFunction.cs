using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class MainFunction : Statement
    {
        public BlockStatement Body { get; set; }
        
        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation() => "fn main() : void { ... }";
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}