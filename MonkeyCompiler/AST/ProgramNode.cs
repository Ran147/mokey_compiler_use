// AST/ProgramNode.cs
using System.Collections.Generic;

namespace MonkeyCompiler.AST
{
    public class ProgramNode : Node
    {
        // Esta es la lista que faltaba o que estaba mal escrita.
        // Debe ser pública y debe llamarse 'Statements'
        public List<Statement> Statements { get; } = new List<Statement>();

        // Constructor vacío (como vimos en el error anterior)
        public ProgramNode()
        {
        }

        // Constructor (Alternativo, por si lo prefieres, pero el de arriba es más simple)
        // public ProgramNode(List<Statement> statements)
        // {
        //     Statements.AddRange(statements);
        // }

        public override string GetAstRepresentation()
        {
            return $"ProgramNode ({Statements.Count} statements)";
        }
    }
}