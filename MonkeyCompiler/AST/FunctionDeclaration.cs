using System.Collections.Generic;
using System.Linq;
using MonkeyCompiler.Encoder;

namespace MonkeyCompiler.AST
{
    public class FunctionDeclaration : Statement
    {
        public Identifier Name { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public TypeNode ReturnType { get; set; }
        public BlockStatement Body { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation()
        {
            var paramTypes = string.Join(", ", Parameters.Select(p => p.GetAstRepresentation()));
            return $"fn {Name.GetAstRepresentation()}({paramTypes}) : {ReturnType.GetAstRepresentation()} {{ ... }}";
        }
        public override void Accept(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}