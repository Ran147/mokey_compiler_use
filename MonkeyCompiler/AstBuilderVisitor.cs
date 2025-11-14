// AstBuilderVisitor.cs
using Antlr4.Runtime.Tree;
using MonkeyCompiler.AST;



using Antlr4.Runtime.Tree;
using MonkeyCompiler.AST;
using System.Linq; // Necesario para .OfType<>




using System; // Necesario para Exception e InvalidOperationException


namespace MonkeyCompiler
{
    public class AstBuilderVisitor : MonkeyParserBaseVisitor<Node>
    {
        // --- MÉTODO AUXILIAR PARA TRAZABILIDAD ---
        
        private InvalidOperationException CreateTraceException(
            Antlr4.Runtime.ParserRuleContext context, 
            Exception innerException)
        {
            string ruleName = context.GetType().Name.Replace("Context", "");
            int line = context.Start.Line;
            int col = context.Start.Column;
            
            string message = 
                $"Error de trazabilidad en AST builder (regla: {ruleName}, línea: {line}, col: {col}): {innerException.Message}";
            
            return new InvalidOperationException(message, innerException);
        }

        // --- 1. Nodos Principales (Programa y Bloques) ---

        public override Node VisitProgram(MonkeyParser.ProgramContext context)
        {
            try
            {
                var program = new ProgramNode(); 
                
                // FIX: Iterar por 'context.children' para preservar el orden
                // en que las sentencias y funciones fueron declaradas.
                foreach (var child in context.children)
                {
                    // Solo visitamos los nodos que son Sentencias o Funciones
                    if (child is MonkeyParser.FunctionDeclarationContext funcCtx)
                    {
                        program.Statements.Add((Statement)Visit(funcCtx));
                    }
                    else if (child is MonkeyParser.StatementContext stmtCtx)
                    {
                        program.Statements.Add((Statement)Visit(stmtCtx));
                    }
                    else if (child is MonkeyParser.MainFunctionContext mainCtx)
                    {
                        program.Statements.Add((Statement)Visit(mainCtx));
                    }
                    // Ignoramos otros nodos (como EOF)
                }
                
                return program;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }


        public override Node VisitMainFunction(MonkeyParser.MainFunctionContext context)
        {
            try
            {
                return new MainFunction
                {
                    Body = (BlockStatement)Visit(context.blockStatement())
                };
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitBlockStatement(MonkeyParser.BlockStatementContext context)
        {
            try
            {
                var statements = new List<Statement>();
                // context.statement() devuelve la lista en el orden correcto
                foreach (var stmtContext in context.statement())
                {
                    statements.Add((Statement)Visit(stmtContext));
                }
                return new BlockStatement(statements); 
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        // --- 2. Sentencias (Statements) ---

        public override Node VisitLetStatement(MonkeyParser.LetStatementContext context)
        {
            try
            {
                var name = context.IDENTIFIER().GetText();
                var typeNode = (TypeNode)Visit(context.type());
                var value = (Expression)Visit(context.expression());
                string typeName = typeNode.GetAstRepresentation();
                return new LetStatement(name, typeName, value);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitReturnStatement(MonkeyParser.ReturnStatementContext context)
        {
            try
            {
                Expression returnValue = null;
                if (context.expression() != null)
                {
                    returnValue = (Expression)Visit(context.expression());
                }
                return new ReturnStatement(returnValue);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitPrintStatement(MonkeyParser.PrintStatementContext context)
        {
            try
            {
                var argument = (Expression)Visit(context.expression());
                return new PrintStatement(argument);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitIfStatement(MonkeyParser.IfStatementContext context)
        {
            try
            {
                var ifStmt = new IfStatement
                {
                    Condition = (Expression)Visit(context.expression()),
                    Consequence = (BlockStatement)Visit(context.blockStatement(0))
                };

                if (context.ELSE() != null && context.blockStatement(1) != null)
                {
                    ifStmt.Alternative = (BlockStatement)Visit(context.blockStatement(1));
                }
                return ifStmt;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitExpressionStatement(MonkeyParser.ExpressionStatementContext context)
        {
            try
            {
                var expression = (Expression)Visit(context.expression());
                return new ExpressionStatement(expression);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitFunctionDeclaration(MonkeyParser.FunctionDeclarationContext context)
        {
            try
            {
                var fnDecl = new FunctionDeclaration
                {
                    Name = new Identifier(context.IDENTIFIER().GetText()),
                    ReturnType = (TypeNode)Visit(context.type()),
                    Body = (BlockStatement)Visit(context.blockStatement())
                };

                if (context.functionParameters() != null)
                {
                    foreach (var paramCtx in context.functionParameters().parameter())
                    {
                        fnDecl.Parameters.Add((Parameter)Visit(paramCtx));
                    }
                }
                return fnDecl;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        // --- 3. Nodos de Tipo ---

        public override Node VisitType(MonkeyParser.TypeContext context)
        {
            try
            {
                if (context.INT() != null) return new SimpleTypeNode { TypeName = "int" };
                if (context.STRING() != null) return new SimpleTypeNode { TypeName = "string" };
                if (context.BOOL() != null) return new SimpleTypeNode { TypeName = "bool" };
                if (context.CHAR() != null) return new SimpleTypeNode { TypeName = "char" };
                if (context.VOID() != null) return new SimpleTypeNode { TypeName = "void" };
                
                if (context.arrayType() != null) return Visit(context.arrayType());
                if (context.hashType() != null) return Visit(context.hashType());
                if (context.functionType() != null) return Visit(context.functionType());
                
                throw new System.Exception("Tipo desconocido en la gramática");
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitArrayType(MonkeyParser.ArrayTypeContext context)
        {
            try
            {
                return new ArrayTypeNode
                {
                    ElementType = (TypeNode)Visit(context.type())
                };
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitHashType(MonkeyParser.HashTypeContext context)
        {
            try
            {
                return new HashTypeNode
                {
                    KeyType = (TypeNode)Visit(context.type(0)),
                    ValueType = (TypeNode)Visit(context.type(1))
                };
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitFunctionType(MonkeyParser.FunctionTypeContext context)
        {
            try
            {
                var fnType = new FunctionTypeNode
                {
                    ReturnType = (TypeNode)Visit(context.type())
                };

                if (context.functionParameterTypes() != null)
                {
                    foreach (var typeCtx in context.functionParameterTypes().type())
                    {
                        fnType.ParameterTypes.Add((TypeNode)Visit(typeCtx));
                    }
                }
                return fnType;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        // --- 4. Expresiones (Manejo de Precedencia) ---

        public override Node VisitExpression(MonkeyParser.ExpressionContext context)
        {
            try
            {
                Node left = Visit(context.additionExpression());
                var compCtx = context.comparison();
                int addExprIndex = 0;
                
                if (compCtx.children != null)
                {
                    foreach (var child in compCtx.children)
                    {
                        if (child is ITerminalNode op)
                        {
                            left = new InfixExpression
                            {
                                Left = (Expression)left,
                                Operator = op.GetText(),
                                Right = (Expression)Visit(compCtx.additionExpression(addExprIndex++))
                            };
                        }
                    }
                }
                return left;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitAdditionExpression(MonkeyParser.AdditionExpressionContext context)
        {
            try
            {
                Node left = Visit(context.multiplicationExpression(0));
                int multExprIndex = 1;
                
                if (context.children != null)
                {
                    foreach (var op in context.children.OfType<ITerminalNode>())
                    {
                        if (op.Symbol.Type == MonkeyParser.PLUS || op.Symbol.Type == MonkeyParser.MINUS)
                        {
                            left = new InfixExpression
                            {
                                Left = (Expression)left,
                                Operator = op.GetText(),
                                Right = (Expression)Visit(context.multiplicationExpression(multExprIndex++))
                            };
                        }
                    }
                }
                return left;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitMultiplicationExpression(MonkeyParser.MultiplicationExpressionContext context)
        {
            try
            {
                Node left = Visit(context.elementExpression(0));
                int elemExprIndex = 1;

                if (context.children != null)
                {
                    foreach (var op in context.children.OfType<ITerminalNode>())
                    {
                        if (op.Symbol.Type == MonkeyParser.MUL || op.Symbol.Type == MonkeyParser.DIV)
                        {
                            left = new InfixExpression
                            {
                                Left = (Expression)left,
                                Operator = op.GetText(),
                                Right = (Expression)Visit(context.elementExpression(elemExprIndex++))
                            };
                        }
                    }
                }
                return left;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitElementExpression(MonkeyParser.ElementExpressionContext context)
        {
            try
            {
                Node left = Visit(context.primitiveExpression());

                if (context.elementAccess() != null)
                {
                    return new ElementAccessExpression
                    {
                        Left = (Expression)left,
                        Index = (Expression)Visit(context.elementAccess().expression())
                    };
                }
                else if (context.callExpression() != null)
                {
                    var call = new CallExpression
                    {
                        Function = (Expression)left
                    };
                    
                    if (context.callExpression().expressionList() != null)
                    {
                        foreach (var exprCtx in context.callExpression().expressionList().expression())
                        {
                            call.Arguments.Add((Expression)Visit(exprCtx));
                        }
                    }
                    return call;
                }
                
                return left;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        // --- 5. Expresiones Primitivas (CON ETIQUETAS) ---

        public override Node VisitIntegerLiteral(MonkeyParser.IntegerLiteralContext context)
        {
            try
            {
                var value = int.Parse(context.INTEGER_LITERAL().GetText());
                return new IntegerLiteral(value);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitStringLiteral(MonkeyParser.StringLiteralContext context)
        {
            try
            {
                string value = context.STRING_LITERAL().GetText().Trim('"');
                return new StringLiteral(value);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitCharLiteral(MonkeyParser.CharLiteralContext context)
        {
            try
            {
                char value = char.Parse(context.CHAR_LITERAL().GetText().Trim('\''));
                return new CharLiteral(value);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitIdentifier(MonkeyParser.IdentifierContext context)
        {
            try
            {
                return new Identifier(context.IDENTIFIER().GetText());
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitBooleanTrue(MonkeyParser.BooleanTrueContext context)
        {
            try
            {
                return new BooleanLiteral(true);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitBooleanFalse(MonkeyParser.BooleanFalseContext context)
        {
            try
            {
                return new BooleanLiteral(false);
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitParenthesizedExpression(MonkeyParser.ParenthesizedExpressionContext context)
        {
            try
            {
                return Visit(context.expression());
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitArrayLiteralExpr(MonkeyParser.ArrayLiteralExprContext context)
        {
            try
            {
                return Visit(context.arrayLiteral());
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitFunctionLiteralExpr(MonkeyParser.FunctionLiteralExprContext context)
        {
            try
            {
                return Visit(context.functionLiteral());
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitHashLiteralExpr(MonkeyParser.HashLiteralExprContext context)
        {
            try
            {
                return Visit(context.hashLiteral());
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }


        // --- 6. Literales Complejos y Auxiliares ---

        public override Node VisitArrayLiteral(MonkeyParser.ArrayLiteralContext context)
        {
            try
            {
                var arr = new ArrayLiteral();
                if (context.expressionList() != null)
                {
                    foreach (var exprCtx in context.expressionList().expression())
                    {
                        arr.Elements.Add((Expression)Visit(exprCtx));
                    }
                }
                return arr;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitFunctionLiteral(MonkeyParser.FunctionLiteralContext context)
        {
            try
            {
                var fnLit = new FunctionLiteral
                {
                    ReturnType = (TypeNode)Visit(context.type()),
                    Body = (BlockStatement)Visit(context.blockStatement())
                };
                
                if (context.functionParameters() != null)
                {
                    foreach (var paramCtx in context.functionParameters().parameter())
                    {
                        fnLit.Parameters.Add((Parameter)Visit(paramCtx));
                    }
                }
                return fnLit;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }

        public override Node VisitHashLiteral(MonkeyParser.HashLiteralContext context)
        {
            try
            {
                var hash = new HashLiteral();
                var expressions = context.expression(); 

                for (int i = 0; i < expressions.Length; i += 2)
                {
                    var key = (Expression)Visit(expressions[i]);
                    var value = (Expression)Visit(expressions[i + 1]);
                    hash.Pairs.Add(key, value);
                }
                return hash;
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }
        
        public override Node VisitParameter(MonkeyParser.ParameterContext context)
        {
            try
            {
                return new Parameter
                {
                    Name = new Identifier(context.IDENTIFIER().GetText()),
                    ValueType = (TypeNode)Visit(context.type())
                };
            }
            catch (Exception ex)
            {
                throw CreateTraceException(context, ex);
            }
        }
    }
}