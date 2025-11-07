namespace MonkeyCompiler.AST
{
    public class BooleanLiteral : Expression
    {
        public bool Value { get; }

        public BooleanLiteral(bool value)
        {
            Value = value;
            Type = "bool"; // Set the type
        }

        public override string ToString()
        {
            return Value.ToString().ToLower();
        }

        public override string GetAstRepresentation()
        {
            return $"BOOL({Value.ToString().ToUpper()})";
        }
    }
}