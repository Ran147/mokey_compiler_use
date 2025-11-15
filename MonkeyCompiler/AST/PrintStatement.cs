using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class PrintStatement : Statement
    {
        public Expression Argument { get; }

        public PrintStatement(Expression argument)
        {
            Argument = argument;
        }

        
        public override string GetAstRepresentation()
        {
            return $"print({Argument.GetAstRepresentation()})";
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}