parser grammar MonkeyParser;

options {tokenVocab = MonkeyLexer; }
@namespace {MonkeyCompiler}
// -----------------------------
// PROGRAMA Y DECLARACIONES
// -----------------------------
program
    : (functionDeclaration | statement)* mainFunction EOF
    ;

mainFunction
    : FN MAIN LPAREN RPAREN COLON VOID blockStatement
    ;

functionDeclaration
    : FN IDENTIFIER LPAREN functionParameters? RPAREN COLON type blockStatement
    ;

functionParameters 
    : parameter (COMMA parameter)*
    ;

parameter 
    : IDENTIFIER COLON type
    ;

type
    : INT
    | STRING
    | BOOL
    | CHAR
    | VOID
    | arrayType
    | hashType
    | functionType
    ;

arrayType 
    : ARRAY LT type GT
    ;

hashType 
    : HASH LT type COMMA type GT
    ;

functionType
    : FN LPAREN functionParameterTypes? RPAREN COLON type
    ;

functionParameterTypes 
    : type (COMMA type)*
    ;

// -----------------------------
// SENTENCIAS
// -----------------------------
statement
    : letStatement
    | returnStatement
    | expressionStatement
    | ifStatement
    | blockStatement
    | printStatement
    ;

letStatement 
    : LET CONST? IDENTIFIER COLON type ASSIGN expression
    ;

returnStatement 
    : RETURN expression?
    ;

expressionStatement 
    : expression
    ;

ifStatement 
    : IF expression blockStatement (ELSE blockStatement)?
    ;

blockStatement 
    : LBRACE statement* RBRACE
    ;

printStatement    
    : PRINT LPAREN expression RPAREN
    ;

// -----------------------------
// EXPRESIONES (CON PRECEDENCIA)
// -----------------------------
expression
    : additionExpression comparison
    ;

comparison 
    : ((LT | GT | LTE | GTE | EQ | NOT_EQ) additionExpression)*
    ;

additionExpression 
    : multiplicationExpression ((PLUS | MINUS) multiplicationExpression)*
    ;

multiplicationExpression 
    : elementExpression ((MUL | DIV) elementExpression)*
    ;

elementExpression
    : primitiveExpression (elementAccess | callExpression)?
    ;

elementAccess
    : LBRACKET expression RBRACKET
    ;

callExpression 
    : LPAREN expressionList? RPAREN
    ;

expressionList 
    : expression (COMMA expression)*
    ;

// -----------------------------
// EXPRESIONES PRIMITIVAS
// -----------------------------
primitiveExpression
    : INTEGER_LITERAL          # IntegerLiteral
    | STRING_LITERAL         # StringLiteral
    | CHAR_LITERAL           # CharLiteral
    | IDENTIFIER             # Identifier
    | TRUE                   # BooleanTrue
    | FALSE                  # BooleanFalse
    | LPAREN expression RPAREN # ParenthesizedExpression
    | arrayLiteral           # ArrayLiteralExpr
    | functionLiteral        # FunctionLiteralExpr
    | hashLiteral            # HashLiteralExpr
    ;

arrayLiteral
    : LBRACKET expressionList? RBRACKET
    ;

functionLiteral
    : FN LPAREN functionParameters? RPAREN COLON type blockStatement
    ;
    
hashLiteral 
    : LBRACE (expression COLON expression (COMMA expression COLON expression)*)? RBRACE
    ;
