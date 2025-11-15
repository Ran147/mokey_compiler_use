using System.Collections.Generic;
using System.Linq;
using MonkeyCompiler.Encoder;

namespace MonkeyCompiler.AST
{
    public class ArrayLiteral : Expression
    {
        public List<Expression> Elements { get; set; } = new List<Expression>();

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation()
        {
            var elems = string.Join(", ", Elements.Select(e => e.GetAstRepresentation()));
            return $"[{elems}]";
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
   
}