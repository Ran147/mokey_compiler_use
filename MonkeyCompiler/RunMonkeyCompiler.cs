using Antlr4.Runtime;
using System.IO;
using System;
using System.Collections.Generic; 
using MonkeyCompiler.AST;        // Necesario para Node
using MonkeyCompiler.TypeChecker; // Necesario para TypeCheckerVisitor y TypeException

namespace MonkeyCompiler;

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
        
        // NOTA: Reiniciamos el Lexer si GetAllTokens() fue llamado.
        // Si no se llamó, el Lexer debe estar en la misma posición que al iniciar.
        lexer.Reset(); 
        
        // --- 3. FASE PARSER ---
        CommonTokenStream tokenStream = new CommonTokenStream(lexer);
        MonkeyParser parser = new MonkeyParser(tokenStream);
        
        Console.WriteLine("\n--- Iniciando Parser (Análisis Sintáctico) ---");
        
        // Ejecutamos la regla inicial
        Antlr4.Runtime.Tree.IParseTree parseTree = parser.program(); 
        
        Console.WriteLine("--- Análisis Sintáctico Terminado ---");

        // Variable para el nodo raíz del AST
        Node programAST = null;

        // --- 4. FASE AST BUILDER (Visitor) ---
        try
        {
            Console.WriteLine("\n--- Iniciando AST Builder ---");
            // El AstBuilderVisitor construye nuestro modelo de clases Node
            AstBuilderVisitor astVisitor = new AstBuilderVisitor();
            
            // El resultado es el nodo raíz del AST
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


        // --- 5. FASE TYPE CHECKER ---
        try
        {
            Console.WriteLine("\n--- Iniciando Type Checker ---");
            
            // El TypeCheckerVisitor verifica la semántica del AST
            TypeCheckerVisitor typeChecker = new TypeCheckerVisitor();
            
            // Llama a la lógica de verificación (el TypeCheckerVisitor debe
            // recorrer el árbol y lanzar TypeException si encuentra un error)
            typeChecker.Visit(parseTree); // Usamos el parseTree para generar y verificar en la misma pasada
            
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
             // Captura errores de la tabla de símbolos (ej. re-declaración)
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ FALLO SEMÁNTICO (Tabla de Símbolos): {ex.Message}");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("\n--- Proceso del Compilador Terminado ---");
        
        // Opcional: Imprimir el árbol de análisis de ANTLR (Parse Tree)
        // Console.WriteLine("\nÁrbol de Análisis (Parse Tree):\n" + parseTree.ToStringTree(parser) + "\n");
    }
}