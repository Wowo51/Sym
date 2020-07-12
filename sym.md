## Sym, a free, open source, symbolic computation library.

Sym is in alpha, watch here for updates.

Sym is written in C# and compiles with the free edition of Visual Studio 2019.

The Sym library is a .NET library that you can integrate into your own projects. All of the important symbolic computation functions are in the Sym library. The project also includes a GUI, the SymApp.

Sym allows the definition of user defined algebraic laws that can be used to transform equations and solve algebra problems.

Consider the following transform:

a\*b~b\*a

Read this as a times b transforms to b times a. I use the ~ symbol to indicate a transformation.

a and b can be entire functions, e.g:

(2+3)\*8 transforms to 8\*(2+3) when transformed by the transform above.

Sym can read and use transforms that use the above syntax, e.g:

a/c+b/c~(a+b)/c  
(a+b)/c~a/c+b/c

Sym uses C# notation to define mathematical equations. The notation is similar to the notation used by many computer programming languages. If you are not familiar with the notation used by C# to write mathematical expressions, you can Google C# for further information. A single equals sign is used.

You can use the provided transforms to manually rearrange an equation, but that can be tedious. A solver is provided that uses many different transforms, applied at all relevant points in an equation, to combinatorially explore different valid permutations of your equation as it seeks an optimal solution.

Sym is designed from the ground up to be fast. It can perform thousands of elementary transforms per second with a single core. The solver is multithreaded. With large mathematical expressions you can run into a combinatorial explosion problem as the number of different permutations of a mathematical expression increases very rapidly with the size of the expression. Sym is designed to alleviate that to some extent with speed. As time goes on I'll be attempting to solve some truly difficult mathematical and physics problems.

[Docs](symdocs.md)

Keywords: Computer algebra system.