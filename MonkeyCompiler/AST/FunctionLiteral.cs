using System.Collections.Generic;
using System.Linq;

namespace MonkeyCompiler.AST
{
    public class FunctionLiteral : Expression
    {
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public TypeNode ReturnType { get; set; }
        public BlockStatement Body { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation()
        {
            var paramTypes = string.Join(", ", Parameters.Select(p => p.GetAstRepresentation()));
            return $"fn({paramTypes}) : {ReturnType.GetAstRepresentation()} {{ ... }}";
        }
    }
}