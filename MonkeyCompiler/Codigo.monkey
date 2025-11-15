fn fibonacci(x: int) : int {
    if (x == 0) {
        return 0
    } else {
        // ¡AQUÍ ESTÁ EL ARREGLO!
        // Anidamos el 'if' dentro del 'else'
        if (x == 1) {
            return 1
        } else {
            return fibonacci(x - 1) + fibonacci(x - 2)
        }
    }
}

fn main() : void {
    let resultado: int = fibonacci(7)
    print(resultado)
}