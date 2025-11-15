using System.Collections.Generic;
using System.Linq; // Necesario para string.Join
using MonkeyCompiler.Encoder;

namespace MonkeyCompiler.AST
{
    public class FunctionTypeNode : TypeNode
    {
        public List<TypeNode> ParameterTypes { get; set; } = new List<TypeNode>();
        public TypeNode ReturnType { get; set; }

        // FIX: Implementación del método abstracto
        public override string GetAstRepresentation()
        {
            var paramTypes = string.Join(", ", ParameterTypes.Select(p => p.GetAstRepresentation()));
            return $"fn({paramTypes}) : {ReturnType.GetAstRepresentation()}";
        }
        public override void Accept(IAstVisitor visitor)
        {
            
        }
    }
}