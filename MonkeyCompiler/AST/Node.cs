// AST/Node.cs
namespace MonkeyCompiler.AST;


using MonkeyCompiler.Encoder; // <-- ¡AGREGA ESTE USING!

// Clase base de la que heredarán todas las partes del AST.
public abstract class Node
{
    // Propiedad esencial para la fase de Type Checker (el tipo inferido o declarado).
    public string Type { get; set; } = "unknown"; 

    // Método para imprimir una representación limpia del AST (útil para depurar).
    public abstract string GetAstRepresentation();
    
   
    // Esta es la línea que te está dando los errores, lo cual es correcto.
    public abstract void Accept(IAstVisitor visitor);
    // --- FIN DE LO AGREGADO ---
}