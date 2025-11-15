using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class CharLiteral : Expression
    {
        public char Value { get; }

        public CharLiteral(char value)
        {
            Value = value;
            Type = "char"; // Set the type
        }

        public override string ToString()
        {
            return $"'{Value}'";
        }

        public override string GetAstRepresentation()
        {
            return $"CHAR('{Value}')";
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}