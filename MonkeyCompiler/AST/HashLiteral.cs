using System.Collections.Generic;
using System.Linq;
using MonkeyCompiler.Encoder;

namespace MonkeyCompiler.AST
{
    public class HashLiteral : Expression
    {
        public Dictionary<Expression, Expression> Pairs { get; set; } = new Dictionary<Expression, Expression>();

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation()
        {
            var pairs = string.Join(", ", Pairs.Select(p => $"{p.Key.GetAstRepresentation()}:{p.Value.GetAstRepresentation()}"));
            return $"{{{pairs}}}";
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}