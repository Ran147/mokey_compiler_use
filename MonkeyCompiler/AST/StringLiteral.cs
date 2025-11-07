using System.Text;
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
    }
}