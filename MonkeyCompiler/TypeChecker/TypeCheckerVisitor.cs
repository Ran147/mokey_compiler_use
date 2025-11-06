// TypeChecker/TypeCheckerVisitor.cs

using Antlr4.Runtime.Misc;
using MonkeyCompiler.AST;
using static MonkeyParser;
using System;

namespace MonkeyCompiler.TypeChecker;

// Heredamos del AstBuilderVisitor para que el proceso de construcción y verificación
// ocurra en la misma pasada.
public class TypeCheckerVisitor : AstBuilderVisitor 
{
    // Mantenemos el ámbito de la tabla de símbolos
    private SymbolTable _currentScope;

    public TypeCheckerVisitor()
    {
        // Inicializa la tabla de símbolos global
        _currentScope = new SymbolTable(); 
        // Define funciones integradas:
        _currentScope.Define("print", "fn(any):void");
    }

    // Sobreescribe la lógica para verificar y registrar.
    public override Node VisitLetStatement([NotNull] LetStatementContext context)
    {
        // 1. Construir el nodo AST (usando la lógica de la clase base)
        LetStatement letNode = (LetStatement)base.VisitLetStatement(context);
        
        // 2. Comprobación de tipos: Se asume que la expresión ya fue visitada y anotada con su tipo.
        string valueType = letNode.Value.Type; 

        if (letNode.DeclaredType != valueType)
        {
            throw new TypeException(
                $"Tipos no coinciden. Declarado como '{letNode.DeclaredType}', pero la expresión es de tipo '{valueType}'.", 
                letNode
            );
        }
        
        // 3. Registrar en la Tabla de Símbolos
        bool isConst = context.CONST() != null; 
        _currentScope.Define(letNode.Name, letNode.DeclaredType, isConst); 
        
        Console.WriteLine($"[TYPE CHECK] Variable '{letNode.Name}' OK. Tipo final: {letNode.DeclaredType}");
        
        return letNode;
    }
    
    // NOTA: Implementar VisitIdentifier, VisitFunctionDeclaration, etc., para completar la verificación.
}