// TypeChecker/TypeCheckerVisitor.cs
using Antlr4.Runtime.Misc;
using MonkeyCompiler.AST;
using static MonkeyParser;
using Antlr4.Runtime.Tree;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions; // Sigue siendo necesario

namespace MonkeyCompiler.TypeChecker;

public class TypeCheckerVisitor : AstBuilderVisitor 
{
    private SymbolTable _currentScope;

    public TypeCheckerVisitor()
    {
        _currentScope = new SymbolTable();
        // Definir funciones integradas (built-in)
        //_currentScope.DefineFunction("print", "void", new List<string> { "any" }); 
        //_currentScope.DefineFunction("len", "int", new List<string> { "string" }); 
        //_currentScope.DefineFunction("len", "int", new List<string> { "array" }); 
        //_currentScope.DefineFunction("first", "any", new List<string> { "array" }); 
        //_currentScope.DefineFunction("last", "any", new List<string> { "array" }); 
    }

    // --- 1. Gestión de Ámbitos (Scope) y Nodos Principales ---

    public override Node VisitProgram([NotNull] ProgramContext context)
    {
        _currentScope = _currentScope.EnterScope();
        var node = (ProgramNode)base.VisitProgram(context);
        _currentScope = _currentScope.ExitScope();
        return node;
    }

    public override Node VisitMainFunction([NotNull] MainFunctionContext context)
    {
        string fnName = "main";
        string returnType = "void";
        
        _currentScope.DefineFunction(fnName, returnType, new List<string>());
        _currentScope = _currentScope.EnterFunctionScope(fnName, returnType);
        var body = (BlockStatement)Visit(context.blockStatement());
        _currentScope = _currentScope.ExitScope();

        return new MainFunction { Body = body };
    }

    public override Node VisitBlockStatement([NotNull] BlockStatementContext context)
    {
        _currentScope = _currentScope.EnterScope();
        var node = (BlockStatement)base.VisitBlockStatement(context);
        _currentScope = _currentScope.ExitScope();
        return node;
    }

    // --- 2. Declaraciones (Let, Function) ---

    public override Node VisitLetStatement([NotNull] LetStatementContext context)
    {
        // 1. Visitar la expresión (derecha) PRIMERO.
        var value = (Expression)Visit(context.expression());
        
        string valueType = value.Type; 
        var typeNode = (TypeNode)Visit(context.type());
        string declaredType = typeNode.GetAstRepresentation();

        // 2. Inferir tipo de array vacío si es necesario
        if (string.IsNullOrEmpty(valueType) && value is ArrayLiteral arr && arr.Elements.Count == 0)
        {
            if (declaredType.StartsWith("array<"))
            {
                valueType = declaredType;
                value.Type = valueType; // Anotamos el nodo del array vacío
            }
        }
        // 3. Inferir tipo de hash vacío si es necesario
        if (string.IsNullOrEmpty(valueType) && value is HashLiteral hash && hash.Pairs.Count == 0)
        {
             if (declaredType.StartsWith("hash<"))
             {
                valueType = declaredType;
                value.Type = valueType;
             }
        }

        // 4. Comprobación de tipos
        if (declaredType != valueType)
        {
            throw new TypeException(
                $"Tipos no coinciden. Declarado como '{declaredType}', pero la expresión es de tipo '{valueType}'.",
                value
            );
        }
        
        // 5. Registrar en la Tabla de Símbolos
        var name = context.IDENTIFIER().GetText();
        bool isConst = context.CONST() != null;
        
        try
        {
            _currentScope.Define(name, declaredType, isConst);
        }
        catch (InvalidOperationException ex)
        {
            throw new TypeException(ex.Message, (Node)Visit(context.IDENTIFIER()));
        }

        // 6. Construir el nodo AST
        return new LetStatement(name, declaredType, value);
    }

    public override Node VisitFunctionDeclaration([NotNull] FunctionDeclarationContext context)
    {
        var fnName = context.IDENTIFIER().GetText();
        var returnType = (TypeNode)Visit(context.type());
        var returnTypeStr = returnType.GetAstRepresentation();
        
        var paramTypes = new List<string>();
        var paramNodes = new List<Parameter>();
        if (context.functionParameters() != null)
        {
            foreach (var paramCtx in context.functionParameters().parameter())
            {
                var paramNode = (Parameter)Visit(paramCtx);
                paramTypes.Add(paramNode.ValueType.GetAstRepresentation());
                paramNodes.Add(paramNode);
            }
        }

        try
        {
            _currentScope.DefineFunction(fnName, returnTypeStr, paramTypes);
        }
        catch (InvalidOperationException ex)
        {
            throw new TypeException(ex.Message, (Node)Visit(context.IDENTIFIER()));
        }

        _currentScope = _currentScope.EnterFunctionScope(fnName, returnTypeStr);

        foreach (var p in paramNodes)
        {
            _currentScope.Define(p.Name.Name, p.ValueType.GetAstRepresentation());
        }
        
        var body = (BlockStatement)Visit(context.blockStatement());
        _currentScope = _currentScope.ExitScope();

        var fnDeclNode = new FunctionDeclaration
        {
            Name = new Identifier(fnName),
            ReturnType = returnType,
            Body = body,
            Parameters = paramNodes
        };
        
        fnDeclNode.Type = $"fn({string.Join(",", paramTypes)}) : {returnTypeStr}";
        return fnDeclNode;
    }

    // --- 3. Sentencias (Statements) ---

    public override Node VisitReturnStatement([NotNull] ReturnStatementContext context)
    {
        if (_currentScope.CurrentFunction == null)
        {
            throw new TypeException("Sentencia 'return' encontrada fuera de una función.");
        }

        var node = (ReturnStatement)base.VisitReturnStatement(context);
        
        string expectedReturn = _currentScope.CurrentFunction.ReturnType;
        string actualReturn = node.ReturnValue?.Type ?? "void";
        
        if(node.ReturnValue == null && expectedReturn != "void")
        {
             throw new TypeException($"La función '{_currentScope.CurrentFunction.Name}' espera un valor de tipo '{expectedReturn}' pero se usó 'return' sin valor.", node);
        }

        if (expectedReturn != actualReturn)
        {
            throw new TypeException($"Tipo de retorno no coincide. La función '{_currentScope.CurrentFunction.Name}' espera '{expectedReturn}' pero se retornó '{actualReturn}'.", node);
        }

        return node;
    }

    public override Node VisitIfStatement([NotNull] IfStatementContext context)
    {
        var node = (IfStatement)base.VisitIfStatement(context);

        if (node.Condition.Type != "bool")
        {
            throw new TypeException($"La condición de 'if' debe ser de tipo 'bool', pero se encontró '{node.Condition.Type}'.", node.Condition);
        }
        
        return node;
    }

    // --- 4. Expresiones (Anotación de Tipos) ---

    // ESTA ES LA SECCIÓN CORREGIDA
    // Sobreescribimos los métodos de la gramática, no los que inventé.

    public override Node VisitExpression([NotNull] ExpressionContext context)
    {
        Node left = Visit(context.additionExpression());
        var comparisonNode = context.comparison();

        if (comparisonNode.children == null)
        {
            return left;
        }

        var ops = comparisonNode.children.OfType<ITerminalNode>().ToList();
        var rights = comparisonNode.children.OfType<AdditionExpressionContext>().ToList();

        for (int i = 0; i < ops.Count; i++)
        {
            Node right = Visit(rights[i]); 
            string op = ops[i].GetText();
            var infixNode = new InfixExpression { Left = (Expression)left, Operator = op, Right = (Expression)right };

            string leftType = left.Type;
            string rightType = right.Type; // <--- ¡AQUÍ ESTÁ LA CORRECCIÓN!
            switch (op)
            {
                case ">":
                case "<":
                case ">=":
                case "<=":
                    if (leftType != "int" || rightType != "int")
                    {
                        throw new TypeException($"Operador '{op}' solo aplicable a (int, int), pero se encontró ({leftType}, {rightType}).", infixNode);
                    }
                    infixNode.Type = "bool";
                    break;
                case "==":
                case "!=":
                    if (leftType != rightType)
                    {
                        throw new TypeException($"Operador '{op}' requiere tipos idénticos, pero se encontró '{leftType}' y '{rightType}'.", infixNode);
                    }
                    if (leftType != "int" && leftType != "string" && leftType != "bool" && leftType != "char")
                    {
                        throw new TypeException($"Operador '{op}' no aplicable al tipo '{leftType}'.", infixNode);
                    }
                    infixNode.Type = "bool";
                    break;
            }
        
            left = infixNode;
        }
        return left;
    }
    
    public override Node VisitAdditionExpression([NotNull] AdditionExpressionContext context)
    {
        Node left = Visit(context.multiplicationExpression(0));
    
        if (context.children == null)
        {
            return left;
        }

        var ops = context.children.OfType<ITerminalNode>().ToList();
        var rights = context.multiplicationExpression().Skip(1).ToList();

        for (int i = 0; i < ops.Count; i++)
        {
            Node right = Visit(rights[i]);
            string op = ops[i].GetText();
            var infixNode = new InfixExpression { Left = (Expression)left, Operator = op, Right = (Expression)right };

            string leftType = left.Type;
            string rightType = right.Type; // <--- ¡AQUÍ ESTÁ LA CORRECCIÓN!
            switch (op)
            {
                case "+":
                    if (leftType == "int" && rightType == "int")
                    {
                        infixNode.Type = "int";
                    }
                    else if (leftType == "string" && rightType == "string")
                    {
                        infixNode.Type = "string"; 
                    }
                    else
                    {
                        throw new TypeException($"Operador '{op}' no aplicable a tipos '{leftType ?? "unknown"}' y '{rightType ?? "unknown"}'. Se esperaba (int, int) o (string, string).", infixNode);
                    }
                    break;
                case "-":
                    if (leftType == "int" && rightType == "int")
                    {
                        infixNode.Type = "int";
                    }
                    else
                    {
                        throw new TypeException($"Operador '{op}' solo aplicable a tipos (int, int), pero se encontró ({leftType ?? "unknown"}, {rightType ?? "unknown"}).", infixNode);
                    }
                    break;
            }
        
            left = infixNode;
        }
        return left;
    }

    public override Node VisitMultiplicationExpression([NotNull] MultiplicationExpressionContext context)
    {
        Node left = Visit(context.elementExpression(0));

        if (context.children == null)
        {
            return left;
        }
    
        var ops = context.children.OfType<ITerminalNode>().ToList();
        var rights = context.elementExpression().Skip(1).ToList();
    
        for (int i = 0; i < ops.Count; i++)
        {
            Node right = Visit(rights[i]);
            string op = ops[i].GetText();
            var infixNode = new InfixExpression { Left = (Expression)left, Operator = op, Right = (Expression)right };
        
            string leftType = left.Type;
            string rightType = right.Type; // <--- ¡AQUÍ ESTÁ LA CORRECCIÓN!
            if (leftType == "int" && rightType == "int")
            {
                infixNode.Type = "int";
            }
            else
            {
                throw new TypeException($"Operador '{op}' solo aplicable a tipos (int, int), pero se encontró ({leftType ?? "unknown"}, {rightType ?? "unknown"}).", infixNode);
            }
        
            left = infixNode;
        }
        return left;
    }
    
    public override Node VisitIdentifier([NotNull] IdentifierContext context)
    {
        var node = (Identifier)base.VisitIdentifier(context);
        var symbol = _currentScope.Resolve(node.Name);
        
        if (symbol == null)
        {
            throw new TypeException($"Error de uso: El identificador '{node.Name}' no ha sido declarado.", node);
        }

        node.Type = symbol.Type;
        return node;
    }

    public override Node VisitElementExpression([NotNull] ElementExpressionContext context)
    {
        // 1. Dejar que AstBuilderVisitor construya el nodo (será CallExpression o ElementAccess)
        var node = base.VisitElementExpression(context);

        // 2. Chequear si fue una LLAMADA DE FUNCIÓN (ej. miFunc(1, 2))
        if (node is CallExpression callNode)
        {
            if (callNode.Function is not Identifier fnIdentifier)
            {
                throw new TypeException($"Expresión no es una función llamable.", callNode.Function);
            }
            string fnName = fnIdentifier.Name;
            /*
            // Caso Especial: print()
            if (fnName == "print")
            {
                if (callNode.Arguments.Count != 1)
                {
                    throw new TypeException($"Error de aridad. Función 'print' espera 1 argumento, pero recibió {callNode.Arguments.Count}.", callNode);
                }
                if (string.IsNullOrEmpty(callNode.Arguments[0].Type))
                {
                    throw new TypeException($"Argumento inválido para 'print'. La expresión no tiene tipo.", callNode.Arguments[0]);
                }
                callNode.Type = "void"; // print() retorna void
                return callNode;
            }*/
            
            // Caso Especial: len() (Sobrecargada)
            if (fnName == "len")
            {
                if (callNode.Arguments.Count != 1)
                {
                     throw new TypeException($"Error de aridad. Función 'len' espera 1 argumento, pero recibió {callNode.Arguments.Count}.", callNode);
                }
                string argType = callNode.Arguments[0].Type;
                if (argType != "string" && !argType.StartsWith("array<"))
                {
                     throw new TypeException($"Argumento inválido para 'len'. Se esperaba 'string' o 'array<T>', pero se recibió '{argType}'.", callNode);
                }
                callNode.Type = "int"; // len() siempre retorna int
                return callNode;
            }

            // Funciones normales
            var symbol = _currentScope.Resolve(fnName);
            if (symbol is not FunctionSymbol fnSymbol)
            {
                throw new TypeException($"No se puede llamar a '{fnName}', no es una función.", callNode.Function);
            }

            if (callNode.Arguments.Count != fnSymbol.ParameterTypes.Count)
            {
                throw new TypeException($"Error de aridad. Función '{fnSymbol.Name}' espera {fnSymbol.ParameterTypes.Count} argumentos, pero recibió {callNode.Arguments.Count}.", callNode);
            }

            for (int i = 0; i < callNode.Arguments.Count; i++)
            {
                string expectedType = fnSymbol.ParameterTypes[i];
                string actualType = callNode.Arguments[i].Type; 
                if (expectedType != actualType)
                {
                    throw new TypeException($"Argumento {i+1} inválido para '{fnSymbol.Name}'. Se esperaba '{expectedType}' pero se recibió '{actualType}'.", callNode.Arguments[i]);
                }
            }
            callNode.Type = fnSymbol.ReturnType;
        }
        
        // 3. Chequear si fue un ACCESO A ELEMENTO (ej. miArray[0])
        else if (node is ElementAccessExpression accessNode)
        {
            string leftType = accessNode.Left.Type;
            string indexType = accessNode.Index.Type;

            if (leftType.StartsWith("array<"))
            {
                if (indexType != "int")
                {
                    throw new TypeException($"Índice de array debe ser 'int', pero se encontró '{indexType}'.", accessNode.Index);
                }
                string elementType = leftType.Substring(6, leftType.Length - 7);
                accessNode.Type = elementType;
            }
            else if (leftType.StartsWith("hash<"))
            {
                // ATENCIÓN: Esta Regex es mejor que la anterior, maneja tipos anidados.
                Match m = Regex.Match(leftType, @"hash<(.+),(.+)>");
                if (!m.Success)
                {
                     throw new TypeException($"Tipo hash malformado: '{leftType}'.", accessNode.Left);
                }
                
                // Regex voraz (greedy) no funciona. Necesitamos balancear los '< >'.
                // Solución simple (PERO FRÁGIL): encontrar la primera coma que no esté anidada.
                string inner = leftType.Substring(5, leftType.Length - 6);
                int balance = 0;
                int splitIndex = -1;
                for(int i = 0; i < inner.Length; i++)
                {
                    if (inner[i] == '<') balance++;
                    if (inner[i] == '>') balance--;
                    if (inner[i] == ',' && balance == 0)
                    {
                        splitIndex = i;
                        break;
                    }
                }

                if (splitIndex == -1)
                {
                     throw new TypeException($"Tipo hash malformado, no se encontró coma principal en: '{inner}'.", accessNode.Left);
                }

                string keyType = inner.Substring(0, splitIndex).Trim();
                string valueType = inner.Substring(splitIndex + 1).Trim();

                if (indexType != keyType)
                {
                    throw new TypeException($"Índice de hash no coincide. El hash espera '{keyType}' pero se encontró '{indexType}'.", accessNode.Index);
                }
                accessNode.Type = valueType;
            }
            else
            {
                throw new TypeException($"Expresión no es un array o hash. No se puede indexar el tipo '{leftType}'.", accessNode.Left);
            }
        }
        
        return node;
    }


    // --- 5. Literales (Simples y Complejos) ---

    public override Node VisitArrayLiteral([NotNull] ArrayLiteralContext context)
    {
        var node = (ArrayLiteral)base.VisitArrayLiteral(context);

        if (node.Elements.Count == 0)
        {
            // Se resolverá en 'let'
            node.Type = null; 
            return node;
        }

        string firstType = node.Elements[0].Type;
        if (string.IsNullOrEmpty(firstType))
        {
             throw new TypeException($"No se puede inferir el tipo del primer elemento del array.", node.Elements[0]);
        }
        
        for (int i = 1; i < node.Elements.Count; i++)
        {
            if (node.Elements[i].Type != firstType)
            {
                throw new TypeException($"Todos los elementos de un array deben ser del mismo tipo. Se encontró '{node.Elements[i].Type}' donde se esperaba '{firstType}'.", node.Elements[i]);
            }
        }

        node.Type = $"array<{firstType}>";
        return node;
    }

    public override Node VisitHashLiteral([NotNull] HashLiteralContext context)
    {
        var node = (HashLiteral)base.VisitHashLiteral(context);

        if (node.Pairs.Count == 0)
        {
            // Se resolverá en 'let'
            node.Type = null;
            return node;
        }

        string keyType = node.Pairs.Keys.First().Type;
        string valueType = node.Pairs.Values.First().Type;

        if (keyType != "int" && keyType != "string")
        {
            throw new TypeException($"Claves de Hash deben ser 'int' o 'string', pero se encontró '{keyType}'.", node.Pairs.Keys.First());
        }

        foreach (var key in node.Pairs.Keys)
        {
            if (key.Type != keyType)
            {
                throw new TypeException($"Todas las claves de un hash deben ser del mismo tipo. Se encontró '{key.Type}' donde se esperaba '{keyType}'.", key);
            }
        }
        foreach (var val in node.Pairs.Values)
        {
            if (val.Type != valueType)
            {
                throw new TypeException($"Todos los valores de un hash deben ser del mismo tipo. Se encontró '{val.Type}' donde se esperaba '{valueType}'.", val);
            }
        }

        node.Type = $"hash<{keyType},{valueType}>";
        return node;
    }

    public override Node VisitFunctionLiteral([NotNull] FunctionLiteralContext context)
    {
        var returnType = (TypeNode)Visit(context.type());
        var returnTypeStr = returnType.GetAstRepresentation();

        var paramTypes = new List<string>();
        var paramNodes = new List<Parameter>();
        if (context.functionParameters() != null)
        {
            foreach (var paramCtx in context.functionParameters().parameter())
            {
                var paramNode = (Parameter)Visit(paramCtx);
                paramTypes.Add(paramNode.ValueType.GetAstRepresentation());
                paramNodes.Add(paramNode);
            }
        }
        
        // Creamos un "símbolo de función" temporal para el chequeo de 'return'
        var tempFnSymbol = new FunctionSymbol("_literal", "temp", returnTypeStr, paramTypes);
        _currentScope = _currentScope.EnterFunctionScope(tempFnSymbol);

        foreach (var p in paramNodes)
        {
            _currentScope.Define(p.Name.Name, p.ValueType.GetAstRepresentation());
        }
        
        var body = (BlockStatement)Visit(context.blockStatement());
        _currentScope = _currentScope.ExitScope();

        var fnLitNode = new FunctionLiteral
        {
            ReturnType = returnType,
            Body = body,
            Parameters = paramNodes
        };
        
        fnLitNode.Type = $"fn({string.Join(",", paramTypes)}) : {returnTypeStr}";
        return fnLitNode;
    }
}