using MonkeyCompiler.AST;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic; // <-- Asegúrate de tener este using
using System;  
using System.Linq;// <-- Asegúrate de tener este using
public static class EncoderHelpers
{
    // Este método será llamado por nuestro CIL
    public static string FormatObject(object obj)
    {
        if (obj == null) return "null";
        
        // Formatear Arrays
        if (obj is int[] intArray)
            return $"[{string.Join(", ", intArray)}]";
        if (obj is string[] stringArray)
            return $"[{string.Join(", ", stringArray.Select(s => $"\"{s}\""))}]";
        if (obj is bool[] boolArray)
            return $"[{string.Join(", ", boolArray)}]";
            
        // Formatear Hashes
        if (obj is Dictionary<string, string> ssDict)
            return $"{{{string.Join(", ", ssDict.Select(kv => $"\"{kv.Key}\": \"{kv.Value}\""))}}}";
        if (obj is Dictionary<string, int> siDict)
            return $"{{{string.Join(", ", siDict.Select(kv => $"\"{kv.Key}\": {kv.Value}"))}}}";
        if (obj is Dictionary<int, int> iiDict)
            return $"{{{string.Join(", ", iiDict.Select(kv => $"{kv.Key}: {kv.Value}"))}}}";

        // Fallback para cualquier otra cosa
        return obj.ToString();
    }
}

namespace MonkeyCompiler.Encoder
{
    public class EncoderVisitor : IAstVisitor
    {
        // --- Campos para Reflection.Emit ---
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly TypeBuilder typeBuilder; 
        private MethodBuilder currentMethod; 
        private ILGenerator il; 
        
        // --- FIX: Nuestro "mapa" de variables ---
        private Dictionary<string, LocalBuilder> locals; 
        // ... (justo después de "private Dictionary<string, LocalBuilder> locals;")
        private Dictionary<string, MethodInfo> functions;
        // ... (justo después de "private Dictionary<string, MethodInfo> functions;")
        private int lambdaCounter = 0;

        public EncoderVisitor()
        {
            AssemblyName asmName = new AssemblyName("MonkeyDynamicAssembly");
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(asmName.Name + ".dll");
            
            typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public);
            // --- AGREGA ESTA LÍNEA ---
            this.functions = new Dictionary<string, MethodInfo>();
        }

        // --- Método Auxiliar de Tipos ---
        private Type GetDotNetType(string monkeyType) {
            // Manejo de Arrays
            if (monkeyType.StartsWith("array<"))
            {
                string elementTypeStr = monkeyType.Substring(6, monkeyType.Length - 7);
                Type elementDotNetType = GetDotNetType(elementTypeStr);
                return elementDotNetType.MakeArrayType();
            }
            
            // Manejo de Hashes
            if (monkeyType.StartsWith("hash<"))
            {
                string innerTypesStr = monkeyType.Substring(5, monkeyType.Length - 6);
                int balance = 0;
                int splitIndex = -1;
                for(int i = 0; i < innerTypesStr.Length; i++)
                {
                    if (innerTypesStr[i] == '<') balance++;
                    if (innerTypesStr[i] == '>') balance--;
                    if (innerTypesStr[i] == ',' && balance == 0)
                    {
                        splitIndex = i;
                        break;
                    }
                }
                
                string keyTypeStr = innerTypesStr.Substring(0, splitIndex).Trim();
                string valueTypeStr = innerTypesStr.Substring(splitIndex + 1).Trim();
                Type keyDotNetType = GetDotNetType(keyTypeStr);
                Type valueDotNetType = GetDotNetType(valueTypeStr);
                return typeof(Dictionary<,>).MakeGenericType(keyDotNetType, valueDotNetType);
            }
            
            // --- NUEVO: Manejo de Tipos de Función (Delegados) ---
            if (monkeyType.StartsWith("fn("))
            {
                // 1. Extraer partes: "int,int" y "int" de "fn(int,int) : int"
                int paramsEnd = monkeyType.IndexOf(')');
                string paramsStr = monkeyType.Substring(3, paramsEnd - 3);
                string returnTypeStr = monkeyType.Substring(monkeyType.IndexOf(':') + 1).Trim();

                // 2. Obtener tipos .NET de los parámetros
                List<Type> paramTypes = new List<Type>();
                if (!string.IsNullOrEmpty(paramsStr))
                {
                    paramTypes = paramsStr.Split(',')
                        .Select(s => GetDotNetType(s.Trim()))
                        .ToList();
                }

                // 3. Obtener tipo .NET de retorno
                Type returnDotNetType = GetDotNetType(returnTypeStr);

                // 4. Elegir Func<> (con retorno) o Action<> (con void)
                if (returnDotNetType == typeof(void))
                {
                    // Es un Action<>
                    if (paramTypes.Count == 0) return typeof(Action);
                    // (Podríamos agregar más Actions, pero este es el más común)
                    Type genericAction = Type.GetType($"System.Action`{paramTypes.Count}");
                    if (genericAction != null)
                        return genericAction.MakeGenericType(paramTypes.ToArray());
                    
                    throw new NotImplementedException($"Delegados Action<> con {paramTypes.Count} parámetros no implementados.");
                }
                else
                {
                    // Es un Func<>. Agregar el tipo de retorno al final.
                    paramTypes.Add(returnDotNetType); 
                    
                    Type genericFunc = Type.GetType($"System.Func`{paramTypes.Count}");
                     if (genericFunc != null)
                        return genericFunc.MakeGenericType(paramTypes.ToArray());
                        
                    throw new NotImplementedException($"Delegados Func<> con {paramTypes.Count} parámetros no implementados.");
                }
            }
            // --- FIN DE LO NUEVO ---
            
            switch (monkeyType)
            {
                case "int": return typeof(int);
                case "string": return typeof(string);
                case "bool": return typeof(bool);
                case "char": return typeof(char);
                case "void": return typeof(void);
                default: return typeof(object); 
            }
        }

        // --- Métodos Principales ---
        
        public void Visit(ProgramNode node)
        {
            foreach (var stmt in node.Statements)
            {
                stmt.Accept(this); 
            }
            typeBuilder.CreateType();
        }

        public void Visit(MainFunction node)
        {
            currentMethod = typeBuilder.DefineMethod(
                "main",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(void), 
                Type.EmptyTypes 
            );
            
            // --- FIX: Inicializa el mapa de variables ---
            this.locals = new Dictionary<string, LocalBuilder>();
            
            il = currentMethod.GetILGenerator();
            node.Body.Accept(this); 
            il.Emit(OpCodes.Ret); 
        }
        
        public void Visit(BlockStatement node)
        {
            foreach (var stmt in node.Statements)
            {
                stmt.Accept(this); 
            }
        }
        
        // --- Implementaciones de Nodos (¡CON CIL!) ---

        public void Visit(LetStatement node)
        {
            // --- FIX CORREGIDO ---
            // Usamos las propiedades correctas de LetStatement.cs
            string varName = node.Name; 
            
            // El TypeChecker ya verificó que TypeName y Value.Type coinciden.
            // Usamos la propiedad .Type del nodo de la expresión (Value)
            // que fue anotada por el TypeChecker.
            string monkeyType = node.Value.Type; 
            Type varType = GetDotNetType(monkeyType);
            
            // 1. Visitar la expresión (ej. 123 o "hola")
            node.Value.Accept(this); // -> Pila: [123]

            // 2. Declarar la variable local en CIL
            LocalBuilder localBuilder = il.DeclareLocal(varType);

            // 3. Almacenar el valor de la pila en la variable local
            il.Emit(OpCodes.Stloc, localBuilder); // -> Pila: []

            // 4. Guardar la variable en nuestro mapa
            this.locals[varName] = localBuilder;
        }
        
        public void Visit(Identifier node)
        {
            // --- FIX CORREGIDO ---
            // La propiedad en Identifier.cs es 'Name', no 'Value'.
            string varName = node.Name;

            // 1. Buscar la variable en nuestro mapa
            if (!this.locals.TryGetValue(varName, out LocalBuilder localBuilder))
            {
                throw new Exception($"Error del Encoder: Variable '{varName}' no encontrada.");
            }
            
            // 2. Cargar el valor en la pila
            il.Emit(OpCodes.Ldloc, localBuilder); // -> Pila: [valor de la variable]
        }

        public void Visit(PrintStatement node)
        {
            // 1. Visita la expresión interna
            // (Pone el valor, ej. 123 o un arrayRef, en la pila)
            node.Argument.Accept(this);

            string argumentType = node.Argument.Type;

            // 2. Determinar el método de impresión
            MethodInfo printMethod;

            switch (argumentType)
            {
                // --- CASOS SIMPLES ---
                // (Llaman a Console.WriteLine(T) directamente)
                case "int":
                    printMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(int) });
                    break;
                case "string":
                    printMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
                    break;
                case "bool":
                    printMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(bool) });
                    break;
                case "char":
                    printMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(char) });
                    break;
        
                // --- CASOS COMPLEJOS (Array/Hash) ---
                // (Llaman a nuestro helper FormatObject y luego a Console.WriteLine(string))
                default:
                    // 1. Boxear el valor (convertir array/hash a 'object')
                    // (Si es un tipo de valor)
                    Type netType = GetDotNetType(argumentType);
                    if (netType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, netType);
                    }
            
                    // 2. Llamar a nuestro helper estático: EncoderHelpers.FormatObject(object)
                    MethodInfo formatMethod = typeof(EncoderHelpers).GetMethod("FormatObject", new[] { typeof(object) });
                    il.Emit(OpCodes.Call, formatMethod);
            
                    // 3. El resultado (un string) está ahora en la pila.
                    // Llamar a Console.WriteLine(string)
                    printMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
                    break;
            }
    
            // 3. Emitir la llamada final
            il.Emit(OpCodes.Call, printMethod);
        }

        public void Visit(IntegerLiteral node)
        {
            il.Emit(OpCodes.Ldc_I4, node.Value);
        }

        public void Visit(StringLiteral node)
        {
            il.Emit(OpCodes.Ldstr, node.Value);
        }

        public void Visit(BooleanLiteral node)
        {
            if (node.Value)
            {
                il.Emit(OpCodes.Ldc_I4_1); 
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0); 
            }
        }

        public void Visit(CharLiteral node)
        {
            il.Emit(OpCodes.Ldc_I4, (int)node.Value);
        }
        
        public void Visit(ExpressionStatement node)
        {
            node.Expression.Accept(this);
            if (node.Expression.Type != "void")
            {
                il.Emit(OpCodes.Pop); 
            }
        }

        // --- Otros Nodos (Vacíos por ahora) ---
        
        public void Visit(ReturnStatement node)
        {
            if (node.ReturnValue != null)
            {
                // Visita la expresión. El resultado queda en la pila.
                node.ReturnValue.Accept(this);
            }
    
            // Emite la instrucción de retorno
            il.Emit(OpCodes.Ret);
        }
        public void Visit(IfStatement node)
        {
            // 1. Definir las etiquetas (puntos de salto)
            Label elseLabel = il.DefineLabel();
            Label endIfLabel = il.DefineLabel();

            // 2. Visitar la condición
            // Esto deja 'true' (1) o 'false' (0) en la pila
            node.Condition.Accept(this);

            // 3. Emitir el salto condicional
            // Brfalse = "Branch if False" (Salta a la etiqueta si el valor en la pila es 0)
            il.Emit(OpCodes.Brfalse, elseLabel);

            // 4. Bloque 'then' (Consequence)
            // Si la condición fue 'true', se ejecuta este bloque
            node.Consequence.Accept(this);

            // 5. Salto incondicional al final
            // (Para evitar ejecutar el bloque 'else' si el 'then' se ejecutó)
            il.Emit(OpCodes.Br, endIfLabel);

            // 6. Marcar el inicio del bloque 'else'
            il.MarkLabel(elseLabel);

            // 7. Bloque 'else' (Alternative)
            // Se ejecuta si la condición fue 'false'
            if (node.Alternative != null)
            {
                node.Alternative.Accept(this);
            }
            // Si no hay 'else', esta sección simplemente se salta

            // 8. Marcar el final del 'if'
            il.MarkLabel(endIfLabel);
    
            // La pila queda limpia.
        }
        public void Visit(FunctionDeclaration node) {
            // 1. Obtener nombre y tipos
            string fnName = node.Name.Name;
            Type returnType = GetDotNetType(node.ReturnType.GetAstRepresentation());
            
            Type[] paramTypes = node.Parameters
                .Select(p => GetDotNetType(p.ValueType.GetAstRepresentation()))
                .ToArray();

            // 2. Definir el nuevo método CIL
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                fnName,
                MethodAttributes.Public | MethodAttributes.Static, // Todas las funciones de Monkey son estáticas
                returnType,
                paramTypes
            );

            // 3. Guardar el método ANTES de visitarlo (para permitir recursión)
            this.functions[fnName] = methodBuilder;

            // 4. --- CAMBIO DE CONTEXTO ---
            // Guardamos el generador de CIL y el mapa de variables
            // del método anterior (ej. "main")
            var oldIL = this.il;
            var oldLocals = this.locals;
            var oldMethod = this.currentMethod;

            // Creamos un nuevo generador y mapa de variables para esta función
            this.currentMethod = methodBuilder;
            this.il = methodBuilder.GetILGenerator();
            this.locals = new Dictionary<string, LocalBuilder>();

            // 5. Mapear argumentos CIL (Ldarg) a variables locales (Stloc)
            // En CIL, los argumentos se acceden por índice (0, 1, 2...)
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                Parameter param = node.Parameters[i];
                string paramName = param.Name.Name;
                Type paramType = GetDotNetType(param.ValueType.GetAstRepresentation());
                
                // Declara una variable local (ej. 'a')
                LocalBuilder local = il.DeclareLocal(paramType);
                
                // Carga el argumento de la función (ej. argumento en índice 0)
                il.Emit(OpCodes.Ldarg, i);
                
                // Almacena el argumento en la variable local 'a'
                il.Emit(OpCodes.Stloc, local);
                
                // Guarda la variable local 'a' en nuestro mapa
                this.locals[paramName] = local;
            }

            // 6. Visitar el cuerpo de la función
            node.Body.Accept(this);
            
            // 7. (Seguro) Agregar un 'ret' implícito si es una función void
            // y no tiene un 'return' explícito al final.
            if (returnType == typeof(void))
            {
                il.Emit(OpCodes.Ret);
            }

            // 8. --- RESTAURAR CONTEXTO ---
            // Volvemos a generar CIL para el método anterior (ej. "main")
            this.il = oldIL;
            this.locals = oldLocals;
            this.currentMethod = oldMethod;
        }
        
        public void Visit(InfixExpression node) {
            // 1. Visitar el lado izquierdo (pone el valor en la pila)
            node.Left.Accept(this);
            
            // 2. Visitar el lado derecho (pone el valor en la pila)
            node.Right.Accept(this);

            // 3. Obtener el tipo de los operandos
            // (El TypeChecker ya se aseguró de que sean compatibles)
            string leftType = node.Left.Type;

            // 4. Emitir la instrucción CIL basada en el operador
            // La pila tiene [valor_izq, valor_der]
            
            switch (node.Operator)
            {
                // --- Operaciones Aritméticas (int) ---
                case "+":
                    // Manejo especial: '+' puede ser int+int o string+string
                    if (leftType == "string")
                    {
                        var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                        il.Emit(OpCodes.Call, concatMethod);
                    }
                    else
                    {
                        il.Emit(OpCodes.Add);
                    }
                    break;
                case "-":
                    il.Emit(OpCodes.Sub);
                    break;
                case "*":
                    il.Emit(OpCodes.Mul);
                    break;
                case "/":
                    il.Emit(OpCodes.Div);
                    break;

                // --- Operaciones de Comparación ---
                
                // Ceq = "Comparar si son Iguales"
                // Pone 1 (true) o 0 (false) en la pila
                case "==":
                    il.Emit(OpCodes.Ceq);
                    break;
                    
                // Clt = "Comparar si es Menor Que" (Less Than)
                case "<":
                    il.Emit(OpCodes.Clt);
                    break;
                    
                // Cgt = "Comparar si es Mayor Que" (Greater Than)
                case ">":
                    il.Emit(OpCodes.Cgt);
                    break;
                    
                // Para '!=' (No Igual), hacemos:
                // 1. Comparamos si son iguales (Ceq) -> [resultado]
                // 2. Cargamos un 0 (false) -> [resultado, 0]
                // 3. Comparamos si son iguales (Ceq) -> [resultado_final]
                // (Si el resultado era 'true' (1), 1 == 0 es 'false' (0))
                // (Si el resultado era 'false' (0), 0 == 0 es 'true' (1))
                case "!=":
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;

                // Para '>=' (Mayor o Igual), usamos "No Menor Que"
                // 1. Comparamos si es menor que (Clt) -> [resultado]
                // 2. Comparamos si el resultado es 0 (false) -> [resultado_final]
                case ">=":
                    il.Emit(OpCodes.Clt);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;

                // Para '<=' (Menor o Igual), usamos "No Mayor Que"
                // 1. Comparamos si es mayor que (Cgt) -> [resultado]
                // 2. Comparamos si el resultado es 0 (false) -> [resultado_final]
                case "<=":
                    il.Emit(OpCodes.Cgt);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                    
                default:
                    throw new Exception($"Error del Encoder: Operador infijo desconocido '{node.Operator}'.");
            }
            
            // Al final, el resultado de la operación (ej. 15)
            // queda en la cima de la pila, listo para ser usado.
        }
        public void Visit(CallExpression node)
        {
            // Primero, revisamos si es un Identificador (el caso más común)
            if (node.Function is Identifier fnIdentifier)
            {
                string fnName = fnIdentifier.Name;

                // --- INICIO DE LA CORRECCIÓN ---

                // CASO 1: Es una función estática (ej. 'fn fibonacci...' o 'fn add...')
                // (Buscamos en el mapa de 'functions' PRIMERO)
                if (this.functions.TryGetValue(fnName, out MethodInfo staticMethodInfo))
                {
                    // 1. Visitar todos los argumentos PRIMERO
                    foreach (var arg in node.Arguments)
                    {
                        arg.Accept(this);
                    } // -> Pila: [arg1, arg2]

                    // 2. Emitir la llamada estática
                    il.Emit(OpCodes.Call, staticMethodInfo);
                }
                // CASO 2: Es un delegado (ej. 'let miFunc = fn...' o un parámetro de función)
                else
                {
                    // 1. Cargar el objeto delegado en la pila PRIMERO
                    // (Visit(Identifier) lo buscará en 'locals')
                    node.Function.Accept(this); // -> Pila: [delegateObj]
                    
                    // 2. Cargar todos los argumentos
                    foreach (var arg in node.Arguments)
                    {
                        arg.Accept(this);
                    } // -> Pila: [delegateObj, arg1, arg2]

                    // 3. Obtener el tipo y llamar a 'Invoke'
                    string functionType = node.Function.Type;
                    Type delegateType = GetDotNetType(functionType);
                    MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
                    
                    il.Emit(OpCodes.Callvirt, invokeMethod);
                }
                // --- FIN DE LA CORRECCIÓN ---
            }
            else
            {
                // Esto maneja casos extraños (ej. (fn() { ... })())
                // (Lógica original de delegado)
                
                // 1. Cargar el delegado (el FunctionLiteral) en la pila
                node.Function.Accept(this); // -> Pila: [delegateObj]
                
                // 2. Cargar argumentos
                foreach (var arg in node.Arguments)
                {
                    arg.Accept(this);
                } // -> Pila: [delegateObj, arg1, arg2]

                // 3. Llamar a Invoke
                string functionType = node.Function.Type;
                Type delegateType = GetDotNetType(functionType);
                MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
                il.Emit(OpCodes.Callvirt, invokeMethod);
            }
            
            // El valor de retorno (si lo hay) queda en la pila.
        }
        
        public void Visit(ElementAccessExpression node)
        {
            string collectionType = node.Left.Type; // ej. "array<int>" o "hash<string, int>"
    
            // --- LÓGICA PARA ARRAYS (Ya implementada) ---
            if (collectionType.StartsWith("array<"))
            {
                node.Left.Accept(this);  // -> Pila: [arrayRef]
                node.Index.Accept(this); // -> Pila: [arrayRef, indice]
                string elementTypeStr = collectionType.Substring(6, collectionType.Length - 7);

                if (elementTypeStr == "int" || elementTypeStr == "bool")
                {
                    il.Emit(OpCodes.Ldelem_I4); 
                }
                else if (elementTypeStr == "string")
                {
                    il.Emit(OpCodes.Ldelem_Ref); 
                }
                else
                {
                    il.Emit(OpCodes.Ldelem_Ref);
                }
                return;
            }
    
            // --- NUEVO: LÓGICA PARA HASHES ---
            if (collectionType.StartsWith("hash<"))
            {
                // 1. Obtener el tipo .NET del hash y su clave
                Type dictDotNetType = GetDotNetType(collectionType);
                Type keyDotNetType = dictDotNetType.GetGenericArguments()[0];

                // 2. Encontrar el método get_Item(TKey) (el indexador '[]')
                MethodInfo getItemMethod = dictDotNetType.GetMethod("get_Item", new[] { keyDotNetType });

                // 3. Cargar la referencia al diccionario
                node.Left.Accept(this); // -> Pila: [dictRef]
        
                // 4. Cargar la clave (el índice)
                node.Index.Accept(this); // -> Pila: [dictRef, clave]
        
                // 5. Llamar al método get_Item(clave)
                // (callvirt) saca [dictRef, clave] y pone [valor] en la pila
                il.Emit(OpCodes.Callvirt, getItemMethod);
        
                // Pila al final: [valorElemento]
                return;
            }
            // --- FIN DE LO NUEVO ---

            throw new Exception($"Error del Encoder: No se puede acceder a elementos del tipo '{collectionType}'");
        }
        
        public void Visit(ArrayLiteral node) {
            // 1. Obtener el tipo de los elementos (ej. "int" de "array<int>")
            string fullType = node.Type; // "array<int>"
            string elementTypeStr = fullType.Substring(6, fullType.Length - 7);
            Type elementDotNetType = GetDotNetType(elementTypeStr);
            
            // 2. Cargar el tamaño del array en la pila
            int size = node.Elements.Count;
            il.Emit(OpCodes.Ldc_I4, size);
            
            // 3. Crear el nuevo array de ese tipo y tamaño
            // (Newarr) saca el tamaño de la pila, crea el array
            // y deja la referencia al array en la pila.
            il.Emit(OpCodes.Newarr, elementDotNetType); // -> Pila: [arrayRef]

            // 4. Iterar y llenar el array
            for (int i = 0; i < node.Elements.Count; i++)
            {
                // Pila actual: [arrayRef]
                
                // Duplicar la referencia del array en la pila
                il.Emit(OpCodes.Dup); // -> Pila: [arrayRef, arrayRef]
                
                // Cargar el índice 'i' en la pila
                il.Emit(OpCodes.Ldc_I4, i); // -> Pila: [arrayRef, arrayRef, i]
                
                // Visitar la expresión del elemento (ej. 10)
                node.Elements[i].Accept(this); // -> Pila: [arrayRef, arrayRef, i, 10]
                
                // Almacenar el elemento en el array:
                // Stelem (Store Element) saca [arrayRef, i, valor]
                // y guarda el valor en el array.
                // Usamos el OpCode específico para el tipo de dato.
                if (elementTypeStr == "int" || elementTypeStr == "bool")
                {
                    il.Emit(OpCodes.Stelem_I4); // Almacena int/bool
                }
                else if (elementTypeStr == "string")
                {
                    il.Emit(OpCodes.Stelem_Ref); // Almacena string (referencia)
                }
                else
                {
                    // TODO: Manejar otros tipos (arrays de arrays, etc.)
                    il.Emit(OpCodes.Stelem_Ref); // Asumir referencia por defecto
                }
                
                // Pila al final del bucle: [arrayRef]
            }
            
            // Al final, la referencia al array lleno queda en la pila,
            // lista para ser usada por 'let' o 'print'.
        }
        
        public void Visit(FunctionLiteral node)
        {
            // 1. Obtener tipo del delegado (ej. Func<int, int, int>)
            Type delegateType = GetDotNetType(node.Type);
            
            // 2. Obtener tipos de parámetros y retorno
            Type returnType = GetDotNetType(node.ReturnType.GetAstRepresentation());
            Type[] paramTypes = node.Parameters
                .Select(p => GetDotNetType(p.ValueType.GetAstRepresentation()))
                .ToArray();

            // 3. Crear un nombre único para el método estático que contendrá el CIL
            string lambdaName = $"_lambda_{lambdaCounter++}";
            
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                lambdaName,
                MethodAttributes.Public | MethodAttributes.Static,
                returnType,
                paramTypes
            );
            
            // 4. --- CAMBIO DE CONTEXTO (Igual que en FunctionDeclaration) ---
            var oldIL = this.il;
            var oldLocals = this.locals;
            var oldMethod = this.currentMethod;

            this.currentMethod = methodBuilder;
            this.il = methodBuilder.GetILGenerator();
            this.locals = new Dictionary<string, LocalBuilder>();

            // 5. Mapear argumentos a variables locales
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                Parameter param = node.Parameters[i];
                LocalBuilder local = il.DeclareLocal(paramTypes[i]);
                il.Emit(OpCodes.Ldarg, i);
                il.Emit(OpCodes.Stloc, local);
                this.locals[param.Name.Name] = local;
            }

            // 6. Visitar el cuerpo
            node.Body.Accept(this);
            
            // 7. Retorno implícito (si es void)
            if (returnType == typeof(void))
            {
                il.Emit(OpCodes.Ret);
            }

            // 8. --- RESTAURAR CONTEXTO ---
            this.il = oldIL;
            this.locals = oldLocals;
            this.currentMethod = oldMethod;

            // 9. --- CREAR EL DELEGADO ---
            // Ahora, de vuelta en el método original (ej. 'main'),
            // creamos el objeto delegado que apunta al método que acabamos de crear.
            
            // 9.1. Obtener el constructor del delegado
            ConstructorInfo ctor = delegateType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            
            // 9.2. Cargar 'null' (para el 'target' de un método estático)
            il.Emit(OpCodes.Ldnull);
            
            // 9.3. Cargar el puntero a nuestro método estático _lambda_X
            il.Emit(OpCodes.Ldftn, methodBuilder);
            
            // 9.4. Crear el nuevo objeto delegado
            il.Emit(OpCodes.Newobj, ctor);
            
            // El objeto delegado (ej. Func<int,int,int>) queda en la pila,
            // listo para ser almacenado por 'let'.
        }
        
        public void Visit(HashLiteral node)
        {
            // 1. Obtener los tipos .NET (ej. typeof(string), typeof(int))
            Type dictDotNetType = GetDotNetType(node.Type); // ej. Dictionary<string, int>
            Type keyDotNetType = dictDotNetType.GetGenericArguments()[0];
            Type valueDotNetType = dictDotNetType.GetGenericArguments()[1];

            // 2. Encontrar el constructor del Diccionario
            ConstructorInfo ctor = dictDotNetType.GetConstructor(Type.EmptyTypes);
    
            // 3. Crear una nueva instancia del Diccionario
            // (newobj) crea el objeto y deja la referencia en la pila.
            il.Emit(OpCodes.Newobj, ctor); // -> Pila: [dictRef]
    
            // 4. Encontrar el método .Add(TKey, TValue)
            MethodInfo addMethod = dictDotNetType.GetMethod("Add", new[] { keyDotNetType, valueDotNetType });

            // 5. Iterar y llenar el diccionario
            foreach (var pair in node.Pairs)
            {
                // Pila actual: [dictRef]
        
                // Duplicar la referencia del diccionario
                il.Emit(OpCodes.Dup); // -> Pila: [dictRef, dictRef]
        
                // Visitar la CLAVE (ej. "uno")
                pair.Key.Accept(this); // -> Pila: [dictRef, dictRef, "uno"]
        
                // Visitar el VALOR (ej. 1)
                pair.Value.Accept(this); // -> Pila: [dictRef, dictRef, "uno", 1]
        
                // Llamar a .Add(clave, valor)
                // (callvirt) saca [dictRef, clave, valor] de la pila
                il.Emit(OpCodes.Callvirt, addMethod);
        
                // Pila al final del bucle: [dictRef]
            }
    
            // Al final, la referencia al diccionario lleno queda en la pila.
        }
        public void Visit(Parameter node) { /* TODO */ }

        public Assembly GetAssembly()
        {
            return assemblyBuilder;
        }
    }
}