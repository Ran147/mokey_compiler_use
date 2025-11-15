using System.Text;
using MonkeyCompiler.Encoder;
namespace MonkeyCompiler.AST
{
    public class StringLiteral : Expression
    {
        public string Value { get; }

        public StringLiteral(string value)
        {
            Value = value;
            Type = "string"; // Set the type
        }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }

        public override string GetAstRepresentation()
        {
            return $"STRING({Value})";
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}