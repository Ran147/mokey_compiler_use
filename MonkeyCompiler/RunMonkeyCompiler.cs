using Antlr4.Runtime;
using System.IO;
using System;
using System.Collections.Generic; 
using MonkeyCompiler.AST;        // Necesario para Node
using MonkeyCompiler.TypeChecker; // Necesario para TypeCheckerVisitor y TypeException
using MonkeyCompiler.AST; // Asegúrate de tener este using



namespace MonkeyCompiler
{
    class RunMonkeyCompiler
    {
        static void Main(string[] args)
        {
            string inputFile = "Codigo.monkey";
            
            // --- 1. Inicialización y Lectura ---
            string filePath = Path.Combine(AppContext.BaseDirectory, inputFile);
            string testInput = File.ReadAllText(filePath);
            
            Console.WriteLine($"--- Contenido de {inputFile} ---\n{testInput}\n-----------------------------");

            // --- 2. FASE LEXER ---
            AntlrInputStream inputStream = new AntlrInputStream(testInput);
            MonkeyLexer lexer = new MonkeyLexer(inputStream);
            
            lexer.Reset(); 
            
            // --- 3. FASE PARSER ---
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            MonkeyParser parser = new MonkeyParser(tokenStream);
            
            Console.WriteLine("\n--- Iniciando Parser (Análisis Sintáctico) ---");
            
            Antlr4.Runtime.Tree.IParseTree parseTree = parser.program(); 
            
            Console.WriteLine("--- Análisis Sintáctico Terminado ---");

            Node programAST = null;

            // --- 4. FASE AST BUILDER (Visitor) ---
            try
            {
                Console.WriteLine("\n--- Iniciando AST Builder ---");
                AstBuilderVisitor astVisitor = new AstBuilderVisitor();
                programAST = astVisitor.Visit(parseTree); 
                Console.WriteLine($"--- Construcción del AST Terminada. Raíz: {programAST.GetAstRepresentation()} ---");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR en la construcción del AST: {ex.Message}");
                Console.ResetColor();
                return;
            }

            // ==========================================================
            // --- 4.5 VERIFICACIÓN DEL AST ---
            // ==========================================================
            Console.WriteLine("\n--- Verificación del AST (Imprimiendo sentencias) ---");
            if (programAST is MonkeyCompiler.AST.ProgramNode programNode)
            {
                if (programNode.Statements.Count == 0)
                {
                    Console.WriteLine("El AST no tiene sentencias.");
                }
                else
                {
                    foreach (var statement in programNode.Statements)
                    {
                        Console.WriteLine(statement.GetAstRepresentation());
                    }
                }
            }
            else
            {
                Console.WriteLine("El nodo raíz del AST es nulo o no es un ProgramNode.");
            }
            Console.WriteLine("--- Fin de la Verificación del AST ---");
            // ==========================================================


            // --- 5. FASE TYPE CHECKER ---
            try
            {
                Console.WriteLine("\n--- Iniciando Type Checker ---");
                
                TypeCheckerVisitor typeChecker = new TypeCheckerVisitor();
                
                // ***** FIX *****
                // Revertimos el cambio. Tu TypeCheckerVisitor espera
                // el 'parseTree' de ANTLR, no tu 'programAST'.
                typeChecker.Visit(parseTree); 
                
                Console.WriteLine("=============================================");
                Console.WriteLine("✅ ¡VERIFICACIÓN DE TIPOS EXITOSA! ESTÁS LISTO PARA EL ENCODER.");
                Console.WriteLine("=============================================");
            }
            catch (TypeException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ FALLO EN TYPE CHECKER: {ex.Message}");
                Console.ResetColor();
                return;
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ FALLO SEMÁNTICO (Tabla de Símbolos): {ex.Message}");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("\n--- Proceso del Compilador Terminado ---");
        }
    }
}