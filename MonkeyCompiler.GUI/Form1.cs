// --- AGREGA ESTOS USINGS AL INICIO ---
using Antlr4.Runtime;
using MonkeyCompiler.AST;
using MonkeyCompiler.TypeChecker;
using MonkeyCompiler.Encoder; // <-- Nuevo
using System.IO; 
using System.Reflection;     // <-- Nuevo
using System.Reflection.Emit; // <-- Nuevo
// --- FIN DE USINGS ---

namespace MonkeyCompiler.GUI
{
    public partial class Form1 : Form
    {
        private string monkeyFilePath;
        public Form1()
        {
            InitializeComponent();
            
            // Lógica para encontrar el archivo Codigo.monkey
            string exePath = Application.StartupPath;
            string solutionRoot = Path.GetFullPath(Path.Combine(exePath, @"../../../../"));
            monkeyFilePath = Path.Combine(solutionRoot, "MonkeyCompiler", "Codigo.monkey");
        }

        // --- ¡MÉTODO btnRun_Click REEMPLAZADO! ---
        private void btnRun_Click(object sender, EventArgs e)
        {
            // --- 1. GUARDAR Y PREPARAR ---
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
            
            txtOutput.ForeColor = Color.White; 
            txtOutput.Text = string.Empty;

            // --- 2. CAPTURAR CONSOLA ---
            StringWriter consoleOutput = new StringWriter();
            var originalConsoleOut = Console.Out;
            Console.SetOut(consoleOutput);

            ProgramNode astRoot = null; 

            // --- 3. PROCESO DE COMPILACIÓN ---
            try
            {
                // --- FASES 1-4 (Lexer, Parser, AST Builder, TypeChecker) ---
                Console.WriteLine($"Archivo '{monkeyFilePath}' guardado.");
                Console.WriteLine("--- Iniciando Parser (Análisis Sintáctico) ---");

                string sourceCode = txtCode.Text;
                AntlrInputStream inputStream = new AntlrInputStream(sourceCode);
                MonkeyLexer lexer = new MonkeyLexer(inputStream);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                MonkeyParser parser = new MonkeyParser(tokenStream);
                var parseTree = parser.program(); 

                Console.WriteLine("--- Análisis Sintáctico Terminado ---\r\n");
                Console.WriteLine("--- Iniciando AST Builder & Type Checker ---");
                
                var visitor = new TypeCheckerVisitor();
                astRoot = (ProgramNode)visitor.Visit(parseTree); 

                Console.WriteLine("✅ ¡AST construido y tipos verificados exitosamente!\r\n");
                
                // (Opcional) Imprimir AST
                Console.WriteLine("--- Verificación del AST (Imprimiendo sentencias) ---");
                PrintAst(astRoot); 
                Console.WriteLine("--- Fin de la Verificación del AST ---\r\n");

                // --- FASE 5: ENCODER ---
                Console.WriteLine("--- Iniciando Encoder (Generación CIL) ---");
                EncoderVisitor encoder = new EncoderVisitor();
                
                astRoot.Accept(encoder);
                
                Console.WriteLine("--- Generación CIL Terminada ---");

                // --- FASE 6: EJECUCIÓN ---
                Console.WriteLine("\n--- Ejecutando código compilado... ---");

                Assembly asm = encoder.GetAssembly(); // <-- ¡Esta línea ahora funciona!
                Type programType = asm.GetType("Program");
                MethodInfo mainMethod = programType.GetMethod("main");
                
                if (mainMethod == null)
                {
                    throw new InvalidOperationException("Fallo del Encoder: No se encontró el método 'main'.");
                }
                
                mainMethod.Invoke(null, null);

                Console.WriteLine("--- Ejecución Terminada ---");
                Console.WriteLine("\n--- Proceso del Compilador Terminado ---");
            }
            catch (TypeException typeEx)
            {
                Console.WriteLine("");
                Console.WriteLine($"❌ FALLO SEMÁNTICO (Tabla de Símbolos): {typeEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine($"❌ FALLO DEL COMPILADOR: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                // --- 4. RESTAURAR CONSOLA Y MOSTRAR SALIDA ---
                Console.SetOut(originalConsoleOut); 
                string output = consoleOutput.ToString();
                
                if (output.Contains("❌"))
                {
                    txtOutput.ForeColor = Color.Red;
                }
                else
                {
                    txtOutput.ForeColor = Color.LimeGreen; 
                }
                txtOutput.Text = output;
            }
        }

        // --- (Método Form1_Load sin cambios) ---
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Monkey Compiler IDE";
            try
            {
                if (File.Exists(monkeyFilePath))
                {
                    string[] lines = File.ReadAllLines(monkeyFilePath);
                    txtCode.Text = string.Join(Environment.NewLine, lines);
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

        // --- (Método PrintAst sin cambios) ---
        private void PrintAst(ProgramNode node)
        {
            foreach (var stmt in node.Statements)
            {
                Console.WriteLine(stmt.GetAstRepresentation());
            }
        }
        
        // --- (ELIMINAMOS EL MÉTODO 'RunEncoder' SIMULADO) ---
    }
}