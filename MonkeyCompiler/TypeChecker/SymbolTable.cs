// TypeChecker/SymbolTable.cs
using System.Collections.Generic;
using System;
using MonkeyCompiler.AST; // Necesario para TypeException

namespace MonkeyCompiler.TypeChecker;

public class SymbolTable
{
    private readonly Dictionary<string, Symbol> _symbols = new();
    private readonly SymbolTable? _parent; 
    
    public FunctionSymbol? CurrentFunction { get; private set; }

    public SymbolTable(SymbolTable? parent = null)
    {
        _parent = parent;
        if (parent != null)
        {
            CurrentFunction = parent.CurrentFunction;
        }
    }

    public void Define(string name, string type, bool isConst = false)
    {
        if (_symbols.ContainsKey(name))
        {
            // Lanzamos una excepción que será atrapada por el Visitor
            throw new InvalidOperationException($"Error de declaración: La variable '{name}' ya está definida en este ámbito.");
        }
        _symbols[name] = new Symbol(name, type, isConst);
    }
    
    public FunctionSymbol DefineFunction(string name, string returnType, List<string> paramTypes)
    {
        if (_symbols.ContainsKey(name))
        {
            throw new InvalidOperationException($"Error de declaración: La función '{name}' ya está definida en este ámbito.");
        }
        var functionType = $"fn({string.Join(",", paramTypes)}) : {returnType}";
        var fnSymbol = new FunctionSymbol(name, functionType, returnType, paramTypes);
        _symbols[name] = fnSymbol;
        return fnSymbol;
    }

    public Symbol? Resolve(string name)
    {
        if (_symbols.TryGetValue(name, out var symbol))
        {
            return symbol;
        }
        return _parent?.Resolve(name);
    }

    public SymbolTable EnterScope()
    {
        return new SymbolTable(this);
    }

    // Esta es la versión que usa el FunctionSymbol,
    // que coincide con el TypeCheckerVisitor
    public SymbolTable EnterFunctionScope(FunctionSymbol functionSymbol)
    {
        var newScope = new SymbolTable(this);
        newScope.CurrentFunction = functionSymbol; 
        return newScope;
    }
    
    // Dejamos esta versión por si acaso (para 'main')
    public SymbolTable EnterFunctionScope(string name, string returnType)
    {
         var newScope = new SymbolTable(this);
         // Buscamos el símbolo que acabamos de definir
         var symbol = Resolve(name);
         if (symbol is FunctionSymbol fnSymbol)
         {
            newScope.CurrentFunction = fnSymbol;
         }
         return newScope;
    }

    public SymbolTable ExitScope()
    {
        return _parent ?? this;
    }
}