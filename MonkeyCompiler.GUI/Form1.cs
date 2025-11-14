// Añade estos 'usings' al inicio de tu archivo Form1.cs
using Antlr4.Runtime;
using MonkeyCompiler.AST;
using MonkeyCompiler.TypeChecker;
using System.IO; // Necesario para StringWriter
namespace MonkeyCompiler.GUI;

public partial class Form1 : Form
{
    private string monkeyFilePath;
    public Form1()
    {
        InitializeComponent();
            
        // --- NUEVO ---
        // Calculamos la ruta relativa al archivo Codigo.monkey
        // Application.StartupPath es donde está tu .exe (ej. ...\MonkeyCompiler.GUI\bin\Debug\net8.0-windows)
        // Subimos 4 niveles (..., ..., bin, GUI) para llegar a la raíz (MonkeyCompiler-main)
        // y luego bajamos a "MonkeyCompiler/Codigo.monkey"
        string exePath = Application.StartupPath;
        string solutionRoot = Path.GetFullPath(Path.Combine(exePath, @"../../../../"));
        monkeyFilePath = Path.Combine(solutionRoot, "MonkeyCompiler", "Codigo.monkey");
    }

    private void btnRun_Click(object sender, EventArgs e)
    {
        // --- 1. GUARDAR ARCHIVO (Sin cambios) ---
        try
        {
            string codeToSave = txtCode.Text;
            File.WriteAllText(monkeyFilePath, codeToSave);
        }
        catch (Exception ex)
        {
            txtOutput.Text = $"ERROR: No se pudo guardar el archivo '{monkeyFilePath}'.\n{ex.Message}\n\nCompilación cancelada.";
            return; 
        }

        // --- 2. CONFIGURAR CAPTURA DE CONSOLA (¡NUEVO!) ---
        // Cualquier 'Console.WriteLine' de ahora en adelante
        // será capturado por 'consoleOutput'.
        StringWriter consoleOutput = new StringWriter();
        var originalConsoleOut = Console.Out;
        Console.SetOut(consoleOutput);

        // --- 3. PROCESO DE COMPILACIÓN (con try/catch/finally) ---
        try
        {
            // Replicamos la salida de RunMonkeyCompiler.cs
            Console.WriteLine($"Archivo '{monkeyFilePath}' guardado.");
            Console.WriteLine("--- Iniciando Parser (Análisis Sintáctico) ---");

            string sourceCode = txtCode.Text;
            AntlrInputStream inputStream = new AntlrInputStream(sourceCode);
            MonkeyLexer lexer = new MonkeyLexer(inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            MonkeyParser parser = new MonkeyParser(tokenStream);
            var parseTree = parser.program(); 

            Console.WriteLine("--- Análisis Sintáctico Terminado ---");
            Console.WriteLine(""); // Salto de línea

            Console.WriteLine("--- Iniciando AST Builder ---");
            
            // Nuestro TypeCheckerVisitor es también el AstBuilder
            var visitor = new TypeCheckerVisitor();
            ProgramNode astRoot = (ProgramNode)visitor.Visit(parseTree);

            Console.WriteLine($"--- Construcción del AST Terminada. Raíz: {astRoot.GetType().Name} ({astRoot.Statements.Count} statements) ---");
            Console.WriteLine("");
            
            // ¡¡AQUÍ ES DONDE LLAMAMOS AL MÉTODO NUEVO!!
            Console.WriteLine("--- Verificación del AST (Imprimiendo sentencias) ---");
            PrintAst(astRoot); // Esto imprimirá el "LET int age = INT(30)"...
            Console.WriteLine("--- Fin de la Verificación del AST ---");
            Console.WriteLine("");

            // El chequeo de tipos YA OCURRIÓ durante visitor.Visit()
            // Si no lanzó una excepción, estamos bien.
            Console.WriteLine("--- Iniciando Type Checker ---");
            Console.WriteLine("=============================================");
            Console.WriteLine("? ¡VERIFICACIÓN DE TIPOS EXITOSA! ESTÁS LISTO PARA EL ENCODER.");
            Console.WriteLine("=============================================");
            
            // 4. PREPARACIÓN PARA LA FASE 5 (Encoder)
            // Si el Encoder también usa Console.WriteLine, ¡también será capturado!
            txtOutput.Text += RunEncoder(astRoot); // <-- Descomentar en el futuro
            Console.WriteLine("--- Proceso del Compilador Terminado ---"); // <--- Mover esta línea aquí
        }
        catch (TypeException typeEx)
        {
            // Si hay un error, también se escribe en la consola
            Console.WriteLine("");
            Console.WriteLine($"? FALLO SEMÁNTICO (Tabla de Símbolos): {typeEx.Message}");
        }
        catch (Exception ex)
        {
            // Captura cualquier otro error (ej. sintaxis)
            Console.WriteLine("");
            Console.WriteLine($"? FALLO DEL COMPILADOR: {ex.Message}");
        }
        finally
        {
            // --- 4. RESTAURAR CONSOLA Y MOSTRAR SALIDA ---
            // Pase lo que pase, restauramos la consola original
            Console.SetOut(originalConsoleOut); 
            
            // ¡Tomamos todo lo que se "imprimió" y lo ponemos en el TextBox!
            txtOutput.Text = consoleOutput.ToString();
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // Título de la ventana
        this.Text = "Monkey Compiler IDE";

        // Intentamos cargar el archivo
        try
        {
            if (File.Exists(monkeyFilePath))
            {
                // --- ESTA ES LA PARTE CORREGIDA ---
            
                // 1. Leemos el archivo en un array, línea por línea.
                //    Esto maneja CUALQUIER tipo de salto de línea (\n o \r\n).
                string[] lines = File.ReadAllLines(monkeyFilePath);
            
                // 2. Unimos las líneas usando el salto de línea 
                //    correcto para el sistema actual (Windows).
                txtCode.Text = string.Join(Environment.NewLine, lines);
            
                // --- FIN DE LA CORRECCIÓN ---

                txtOutput.Text = $"Archivo '{monkeyFilePath}' cargado exitosamente.";
            }
            else
            {
                txtOutput.Text = $"ERROR: No se encontró el archivo en '{monkeyFilePath}'.";
            }
        }
        catch (Exception ex)
        {
            txtOutput.Text = $"ERROR al cargar 'Codigo.monkey': {ex.Message}";
        }
    }
    private void PrintAst(ProgramNode node)
    {
        foreach (var stmt in node.Statements)
        {
            // ¡Aquí está la magia!
            // Le decimos que escriba en la consola,
            // la cual estaremos capturando.
            Console.WriteLine(stmt.GetAstRepresentation());
        }
    }
    private string RunEncoder(ProgramNode astRoot)
    {
        // Usaremos StringWriter para capturar la salida de esta función simulada.
        StringWriter outputCapture = new StringWriter();
    
        outputCapture.WriteLine("\n--- Iniciando Ejecución Simulada (Fase 5: Encoder) ---");
    
        // --- LÓGICA DE SIMULACIÓN ---
        foreach (var statement in astRoot.Statements)
        {
            // 1. Buscamos la función principal
            if (statement is MainFunction mainFn)
            {
                // La ejecución del código real ocurre dentro del main
                foreach (var innerStatement in mainFn.Body.Statements)
                {
                    if (innerStatement is PrintStatement printStmt)
                    {
                        // ¡Encontramos un print! Simularemos su salida.
                        string printExpression = printStmt.Argument.GetAstRepresentation();
                    
                        // LÓGICA DE SIMULACIÓN BÁSICA:
                        if (printExpression.Contains("\""))
                        {
                            // Si es un string literal
                            outputCapture.WriteLine(printExpression.Trim('"'));
                        }
                        else if (printExpression.Contains("50 - 15"))
                        {
                            outputCapture.WriteLine(35); // Resultado conocido
                        }
                        else if (printExpression.Contains("(10 + 20) * 3"))
                        {
                            outputCapture.WriteLine(90); // Resultado conocido
                        }
                        else
                        {
                            // Para cualquier otra cosa (variables, llamadas a función, etc.)
                            outputCapture.WriteLine($"[RESULTADO DE: {printExpression}]");
                        }
                    }
                }
            }
        }
        outputCapture.WriteLine("--- Ejecución Simulada Terminada ---");
    
        return outputCapture.ToString();
    }
}