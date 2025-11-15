
using MonkeyCompiler.AST;

namespace MonkeyCompiler.Encoder
{
    /// <summary>
    /// Define el contrato para cualquier Visitor que quiera
    /// recorrer nuestro AST personalizado.
    /// </summary>
    public interface IAstVisitor
    {
        // Métodos 'Visit' para cada tipo de nodo CONCRETO en el AST
        
        // Statements
        void Visit(ProgramNode node);
        void Visit(BlockStatement node);
        void Visit(LetStatement node);
        void Visit(ReturnStatement node);
        void Visit(PrintStatement node);
        void Visit(ExpressionStatement node);
        void Visit(IfStatement node);
        void Visit(FunctionDeclaration node);
        void Visit(MainFunction node);

        // Expressions
        void Visit(Identifier node);
        void Visit(IntegerLiteral node);
        void Visit(StringLiteral node);
        void Visit(BooleanLiteral node);
        void Visit(CharLiteral node);
        void Visit(InfixExpression node);
        void Visit(CallExpression node);
        void Visit(ElementAccessExpression node);
        void Visit(ArrayLiteral node);
        void Visit(FunctionLiteral node);
        void Visit(HashLiteral node);
        void Visit(Parameter node);
        
        // (Ignoramos los TypeNodes por ahora, ya que el Encoder
        // probablemente no necesite visitarlos directamente)
    }
}