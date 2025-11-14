// TypeChecker/Symbol.cs
namespace MonkeyCompiler.TypeChecker;

public class Symbol
{
    public string Name { get; }
    public string Type { get; } // ej: "int", "string", "fn(int,int):int"
    public bool IsConst { get; }

    public Symbol(string name, string type, bool isConst = false)
    {
        Name = name;
        Type = type;
        IsConst = isConst;
    }
}

// Clase especializada para símbolos de función
public class FunctionSymbol : Symbol
{
    public string ReturnType { get; }
    public List<string> ParameterTypes { get; }

    public FunctionSymbol(string name, string type, string returnType, List<string> parameterTypes) 
        : base(name, type, true) // Las funciones son constantes
    {
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
    }
}