using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; }

        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }

        public override string ToString()
        {
            return Expression.ToString();
        }

        public override string GetAstRepresentation()
        {
            return Expression.GetAstRepresentation();
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}