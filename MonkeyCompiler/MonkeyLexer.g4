lexer grammar MonkeyLexer;
@namespace {MonkeyCompiler}
// -----------------------------
// PALABRAS CLAVE
// -----------------------------
FN:     'fn';
MAIN:   'main';
VOID:   'void';
RETURN: 'return';
IF:     'if';
ELSE:   'else';
LET:    'let';
CONST:  'const';
PRINT:  'print';
INT:    'int';
STRING: 'string';
BOOL:   'bool';
CHAR:   'char';
ARRAY:  'array';
HASH:   'hash';
TRUE:   'true';
FALSE:  'false';

// -----------------------------
// LITERALES
// -----------------------------
INTEGER_LITERAL: [0-9]+;
STRING_LITERAL:  '"' ( ~["\\] | '\\' . )*? '"';
CHAR_LITERAL:    '\'' ( ~['\\] | '\\' . ) '\'';

// -----------------------------
// IDENTIFICADORES
// -----------------------------
IDENTIFIER: [a-zA-Z_] [a-zA-Z0-9_]*;

// -----------------------------
// ESPACIOS Y COMENTARIOS
// -----------------------------
WHITESPACE: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' .*? '\n' -> skip;
BLOCK_COMMENT_START: '/*' -> pushMode(IN_COMMENT), skip; // <- ¡AÑADIDO `, skip`!

// -----------------------------
// OPERADORES Y PUNTUACIÓN
// -----------------------------
EQ:     '==';
NOT_EQ: '!=';
LTE:    '<=';
GTE:    '>=';
ASSIGN: '=';
LT:     '<';
GT:     '>';
PLUS:   '+';
MINUS:  '-';
MUL:    '*';
DIV:    '/';
LPAREN: '(';
RPAREN: ')';
LBRACE: '{';
RBRACE: '}';
LBRACKET: '[';
RBRACKET: ']';
COMMA:  ',';
COLON:  ':';

// -----------------------------
// MODO PARA COMENTARIOS ANIDADOS
// -----------------------------
mode IN_COMMENT;
    NESTED_COMMENT_START: '/*' -> pushMode(IN_COMMENT), skip; // <- ¡AÑADIDO `, skip`!
    BLOCK_COMMENT_END: '*/' -> popMode, skip;
    COMMENT_TEXT: . -> skip;
