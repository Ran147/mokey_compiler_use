// 1. Define la función recursiva
fn fibonacci(x: int) : int {
    if (x == 0) {
        return 0;
    } else if (x == 1) {
        return 1;
    } else {
        return fibonacci(x - 1) + fibonacci(x - 2);
    }
}

// 2. Define el punto de entrada (main) que la llama
fn main() : void {
    let resultado: int = fibonacci(7); // Llama a la función
    print(resultado); // Imprime el resultado
}