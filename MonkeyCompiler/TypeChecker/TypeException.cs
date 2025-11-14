// TypeChecker/TypeException.cs
using System;
using MonkeyCompiler.AST; // Asegúrate de tener esto

namespace MonkeyCompiler.TypeChecker;

public class TypeException : Exception
{
    // Almacena el nodo del AST que causó el error
    public Node AstNode { get; } 

    // Constructor que ahora acepta el nodo
    public TypeException(string message, Node node) : base(message)
    {
        AstNode = node;
    }
    
    // Dejamos el constructor antiguo por si acaso,
    // pero lo marcamos como obsoleto.
    [Obsolete("Use constructor that includes the AST Node")]
    public TypeException(string message) : base(message)
    {
        // Si se llama a este, asignamos un nodo "vacío"
        AstNode = new ProgramNode(); 
    }
}