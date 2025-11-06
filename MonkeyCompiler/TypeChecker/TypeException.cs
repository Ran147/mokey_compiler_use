// TypeChecker/TypeException.cs
using System;
using MonkeyCompiler.AST;

namespace MonkeyCompiler.TypeChecker;

public class TypeException : Exception
{
    public Node Node { get; } 
    
    // Lanza la excepción con el nodo para saber dónde ocurrió el error.
    public TypeException(string message, Node node) 
        : base($"[ERROR SEMÁNTICO DE TIPO] {message}. En el nodo: {node.GetAstRepresentation()}")
    {
        Node = node;
    }
}