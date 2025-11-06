// TypeChecker/SymbolTable.cs
using System.Collections.Generic;
using System;

namespace MonkeyCompiler.TypeChecker;

public class SymbolTable
{
    private readonly Dictionary<string, Symbol> _symbols = new();
    private readonly SymbolTable? _parent;

    public SymbolTable(SymbolTable? parent = null)
    {
        _parent = parent;
    }

    public void Define(string name, string type, bool isConst = false)
    {
        if (_symbols.ContainsKey(name))
        {
            throw new InvalidOperationException($"Error de declaración: La variable '{name}' ya está definida en este ámbito.");
        }
        _symbols[name] = new Symbol(name, type, isConst);
    }

    public Symbol? Resolve(string name)
    {
        if (_symbols.TryGetValue(name, out var symbol))
        {
            return symbol;
        }
        return _parent?.Resolve(name);
    }
}