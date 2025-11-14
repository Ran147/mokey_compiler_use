/*
 * Archivo de prueba de Monkey para el Encoder.
 * (Versión CORREGIDA sin punto y coma)
*/

// Una función de utilidad para probar llamadas
fn add(a: int, b: int) : int {
    return a + b
}

// Punto de entrada principal
fn main() : void {
    
    // 1. Imprimir literales de string
    print("--- Prueba de Hola Mundo ---")
    print("Hola mundo! Bienvenido al compilador Monkey.")
    print(" ") // Para un salto de línea en la salida
    
    // 2. Imprimir literales de enteros y operaciones
    print("--- Prueba Aritmética (int) ---")
    print(100)
    print(50 - 15) // Prueba de resta. Debería ser 35
    let resultado: int = (10 + 20) * 3
    print("El resultado de (10+20)*3 es:")
    print(resultado) // Debería ser 90
    
    // 3. Imprimir literales de booleanos y operaciones
    print(" ")
    print("--- Prueba Lógica (bool) ---")
    print(true)
    print(false)
    print("5 es menor que 10?")
    print(5 < 10) // Debería ser true
    print("(1 == 1) es igual a (2 == 2)?")
    print((1 == 1) == (2 == 2)) // Debería ser true
    
    // 4. Imprimir variables
    print(" ")
    print("--- Prueba de Variables ---")
    let saludo: string = "Saludos desde una variable."
    print(saludo)
    
    // 5. Concatenación de strings (El TypeChecker lo soporta)
    print(" ")
    print("--- Prueba de Concatenación ---")
    let str1: string = "Hola"
    let str2: string = "Mundo"
    print(str1 + " " + str2 + "!") // Debería ser "Hola Mundo!"
    
    // 6. Prueba de llamada a función
    print(" ")
    print("--- Prueba de Llamada a Función ---")
    print("Llamando a add(25, 25)...")
    let fn_res: int = add(25, 25)
    print(fn_res) // Debería ser 50
    
    print(" ")
    print("--- Pruebas Terminadas ---")
}
/* Comentario final */