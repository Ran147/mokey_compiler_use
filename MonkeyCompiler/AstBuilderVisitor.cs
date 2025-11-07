// AstBuilderVisitor.cs

using Antlr4.Runtime.Misc;
using MonkeyCompiler.AST; 
using MonkeyCompiler.AST;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MonkeyCompiler;

public class AstBuilderVisitor : MonkeyParserBaseVisitor<Node>
{
    // ====================================================================
    // Métodos 1-5 (VisitProgram, VisitLetStatement, VisitIntegerLiteral, 
    // VisitIdentifier, VisitBlockStatement) ... déjalos como están.
    // ...
    // ====================================================================
    
    // (Pega tus métodos 1-5 aquí)

    public override Node VisitProgram([NotNull] MonkeyParser.ProgramContext context)
    {
        // (Tu código existente)
        var programNode = new ProgramNode();
        foreach (var childContext in context.children)
        {
            var node = Visit(childContext); 
            if (node is Node astNode)
            {
                programNode.Declarations.Add(astNode);
            }
        }
        return programNode;
    }
    
    public override Node VisitLetStatement([NotNull] MonkeyParser.LetStatementContext context)
    {
        // (Tu código existente)
        string name = context.IDENTIFIER().GetText(); 
        string declaredType = context.type().GetText(); 
        Expression value = Visit(context.expression()) as Expression ?? 
                           throw new InvalidOperationException("La sentencia let requiere una expresión de valor."); 
        return new LetStatement(name, declaredType, value);
    }

    
    public override Node VisitIntegerLiteral([NotNull] MonkeyParser.IntegerLiteralContext context)
    {
        // (Tu código existente)
        string text = context.INTEGER_LITERAL().GetText();
        if (int.TryParse(text, out int value))
        {
            return new IntegerLiteral(value);
        }
        return new IntegerLiteral(0); 
    }
    public override Node VisitIdentifier([NotNull] MonkeyParser.IdentifierContext context)
    {
        // (Tu código existente)
        string name = context.IDENTIFIER().GetText();
        return new Identifier(name);
    }
    
    public override Node VisitBlockStatement([NotNull] MonkeyParser.BlockStatementContext context)
    {
        // (Tu código existente)
        var statements = new List<Statement>();
        foreach(var stmtContext in context.statement())
        {
            if(Visit(stmtContext) is Statement stmtNode)
            {
                statements.Add(stmtNode);
            }
        }
        return new BlockStatement(statements);
    }

    // ====================================================================
    // 6. ¡NUEVOS MÉTODOS PARA CORREGIR EL 'null'!
    // Estos métodos arreglan la visita de expresiones simples (literales).
    // ====================================================================

    // expression: additionExpression comparison;
    // Visitamos 'additionExpression' y (por ahora) ignoramos 'comparison'.
    public override Node VisitExpression([NotNull] MonkeyParser.ExpressionContext context)
    {
        // Devolvemos lo que sea que 'additionExpression' nos dé.
        return Visit(context.additionExpression());
    }

    // additionExpression: multiplicationExpression ((PLUS | MINUS) multiplicationExpression)*;
    // Visitamos el primer 'multiplicationExpression' e ignoramos el resto (por ahora).
    public override Node VisitAdditionExpression([NotNull] MonkeyParser.AdditionExpressionContext context)
    {
        // Devolvemos lo que sea que 'multiplicationExpression' nos dé.
        return Visit(context.multiplicationExpression(0)); // Visita el primero
    }

    // multiplicationExpression: elementExpression ((MUL | DIV) elementExpression)*;
    // Visitamos el primer 'elementExpression' e ignoramos el resto (por ahora).
    public override Node VisitMultiplicationExpression([NotNull] MonkeyParser.MultiplicationExpressionContext context)
    {
        // Devolvemos lo que sea que 'elementExpression' nos dé.
        return Visit(context.elementExpression(0)); // Visita el primero
    }

    // elementExpression: primitiveExpression (elementAccess | callExpression)?;
    // Visitamos 'primitiveExpression' e ignoramos el acceso o llamada (por ahora).
    public override Node VisitElementExpression([NotNull] MonkeyParser.ElementExpressionContext context)
    {
        // Devolvemos lo que sea que 'primitiveExpression' nos dé.
        return Visit(context.primitiveExpression());
    }
    // ====================================================================
    // 7. NEW METHODS TO BUILD THE MISSING STATEMENTS AND LITERALS
    //    (Building upon your classmate's original file)
    // ====================================================================

    // Handles: return expression?
    public override Node VisitReturnStatement([NotNull] MonkeyParser.ReturnStatementContext context)
    {
        Expression returnValue = null;
        if (context.expression() != null)
        {
            returnValue = (Expression)Visit(context.expression());
        }
        
        return new ReturnStatement(returnValue);
    }

    // Handles: expression
    public override Node VisitExpressionStatement([NotNull] MonkeyParser.ExpressionStatementContext context)
    {
        var expression = (Expression)Visit(context.expression());
        return new ExpressionStatement(expression);
    }

    // Handles: print ( expression )
    public override Node VisitPrintStatement([NotNull] MonkeyParser.PrintStatementContext context)
    {
        var argument = (Expression)Visit(context.expression());
        return new PrintStatement(argument);
    }

    // Handles: integerLiteral
    // (This was missing from your last file but was in the original)
    
    
    // Handles: stringLiteral
    public override Node VisitStringLiteral([NotNull] MonkeyParser.StringLiteralContext context)
    {
        string text = context.STRING_LITERAL().GetText();
        // Remove the surrounding quotes
        return new StringLiteral(text.Substring(1, text.Length - 2));
    }

    // Handles: charLiteral
    public override Node VisitCharLiteral([NotNull] MonkeyParser.CharLiteralContext context)
    {
        string text = context.CHAR_LITERAL().GetText();
        // Get the character inside the quotes
        return new CharLiteral(text[1]);
    }

    // Handles: booleanLiteral
    // THIS IS THE KEY WE WERE MISSING
    public override Node VisitBooleanLiteral([NotNull] MonkeyParser.BooleanLiteralContext context)
    {
        if (context.TRUE() != null)
        {
            return new BooleanLiteral(true);
        }

        return new BooleanLiteral(false);
    }
}