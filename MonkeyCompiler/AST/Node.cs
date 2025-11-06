// AST/Node.cs
namespace MonkeyCompiler.AST;

using System.Collections.Generic;

// Clase base de la que heredarán todas las partes del AST.
public abstract class Node
{
    // Propiedad esencial para la fase de Type Checker (el tipo inferido o declarado).
    public string Type { get; set; } = "unknown"; 

    // Método para imprimir una representación limpia del AST (útil para depurar).
    public abstract string GetAstRepresentation();
}