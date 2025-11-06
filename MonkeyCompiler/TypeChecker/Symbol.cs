// TypeChecker/Symbol.cs
namespace MonkeyCompiler.TypeChecker;

public class Symbol
{
    public string Name { get; }
    public string Type { get; }
    public bool IsConst { get; }

    public Symbol(string name, string type, bool isConst = false)
    {
        Name = name;
        Type = type;
        IsConst = isConst;
    }
}