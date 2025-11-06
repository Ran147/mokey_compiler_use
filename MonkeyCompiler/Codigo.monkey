/*
 * Este es un archivo de prueba de Monkey.
 * Contiene un comentario de bloque principal.
 *
 * /*
 * Y aquí... un comentario anidado.
 * El lexer debería ignorar todo esto.
 * */
 *
 * Volvemos al bloque principal.
*/

// Declaraciones de variables
let age: int = 30
let name: string = "Monkey" // Comentario de línea

// Una función simple
fn add(a: int, b: int) : int {
    return a + b
}
// Prueba de sintaxis extraña
let y: bool = (5 < 10) == true

// El punto de entrada principal
fn main() : void {
    let x: int = add(age, 5)
    print(x)
}



/* Comentario final */