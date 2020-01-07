What command do I use to generate Daedalus Lexer, Parser and Visitor from grammar file?
 
Compiler:
```sh
antlr4 -Dlanguage=CSharp -encoding utf8 -o Output -visitor -no-listener Daedalus.g4
```

Transpiler:
```sh
antlr4 -Dlanguage=CSharp -encoding utf8 -o Output -visitor -no-listener LegacyDaedalus.g4
```
