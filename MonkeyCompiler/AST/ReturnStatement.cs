namespace MonkeyCompiler.AST
{
    public class ReturnStatement : Statement
    {
        public Expression ReturnValue { get; }

        public ReturnStatement(Expression returnValue)
        {
            ReturnValue = returnValue;
        }

        public override string ToString()
        {
            return $"return {ReturnValue}";
        }

        
        public override string GetAstRepresentation()
        {
            return $"return {ReturnValue?.GetAstRepresentation()}";
        }
    }
}