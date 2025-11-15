using System.Collections.Generic;
using System.Linq;
using MonkeyCompiler.Encoder;

namespace MonkeyCompiler.AST
{
    public class CallExpression : Expression
    {
        public Expression Function { get; set; } 
        public List<Expression> Arguments { get; set; } = new List<Expression>();

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation()
        {
            var args = string.Join(", ", Arguments.Select(a => a.GetAstRepresentation()));
            return $"{Function.GetAstRepresentation()}({args})";
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}