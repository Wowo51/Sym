# Sym: A .NET Symbolic Mathematics Library - Code Guide

Welcome to the code guide for **Sym**, a symbolic computation library written in C\#. This library provides a framework for representing and manipulating mathematical expressions, performing calculus, and solving equations through a powerful pattern-matching and term-rewriting engine.

This guide will walk you through the core architecture, key components, and provide examples of how to use the library for various mathematical tasks.

## Table of Contents

1.  [Core Architecture: The Expression Tree](https://www.google.com/search?q=%231-core-architecture-the-expression-tree)
2.  [The Building Blocks: Atoms](https://www.google.com/search?q=%232-the-building-blocks-atoms)
3.  [Combining Expressions: Operations](https://www.google.com/search?q=%233-combining-expressions-operations)
4.  [Tensor Concepts: Shape, Vector, and Matrix](https://www.google.com/search?q=%234-tensor-concepts-shape-vector-and-matrix)
5.  [The Engine: Pattern Matching and Rewriting](https://www.google.com/search?q=%235-the-engine-pattern-matching-and-rewriting)
6.  [The Solver Framework](https://www.google.com/search?q=%236-the-solver-framework)
7.  [Putting It All Together: Usage Examples](https://www.google.com/search?q=%237-putting-it-all-together-usage-examples)

-----

## 1\. Core Architecture: The Expression Tree

At the heart of the `Sym` library is the concept of the **expression tree**. Every mathematical formula, no matter how complex, is represented as a tree structure. The foundation for this is the `IExpression` interface and its abstract base class `Expression`.

### `IExpression` and `Expression`

  - **`IExpression`**: Defines the essential contract for all symbolic expressions. Key members include:

      - `Shape Shape { get; }`: Describes the tensor shape (scalar, vector, etc.).
      - `IExpression Canonicalize()`: Returns the simplified, standard form of the expression. This is the most important method for ensuring consistency.
      - `ToDisplayString()`: Provides a human-readable string representation.
      - `Equals()` & `GetHashCode()`: Defines value equality based on the canonical form.

  - **`Expression`**: The abstract base class that implements `IExpression`. It provides a crucial, sealed implementation of `Equals()` and `GetHashCode()`. Two expressions are considered equal if and only if their **canonical forms** are equal.

<!-- end list -->

```csharp
// C:\Users\dual5\OneDrive\Desktop\Code2025\GithubRepos\Sym\src\Sym\Core\Expression.cs
public abstract class Expression : IExpression
{
    // ...
    public override sealed bool Equals(object? obj)
    {
        // ...
        // For comparison, always canonicalize both expressions FIRST
        IExpression thisCanonical = this.Canonicalize();
        IExpression otherCanonical = otherExpression.Canonicalize();

        // Now compare the canonical forms using InternalEquals
        return thisCanonical.InternalEquals(otherCanonical);
    }
    // ...
}
```

This design means that `new Add(x, y)` will correctly equal `new Add(y, x)` because they both simplify to the same canonical representation.

### Atoms vs. Operations

Every node in the expression tree is either an `Atom` (a leaf) or an `Operation` (an internal node).

  - **`Atom`**: Represents indivisible elements. They have no arguments and are the leaves of the expression tree. Examples include numbers (`5`), symbols (`x`), and wildcards used for pattern matching (`_a`).
  - **`Operation`**: Represents a function or operator applied to one or more other expressions (its `Arguments`). Examples include addition (`Add`), multiplication (`Multiply`), and differentiation (`Derivative`).

*The expression `x + 2 * y` represented as a tree.*

-----

## 2\. The Building Blocks: Atoms

Atoms are the fundamental, indivisible components of expressions.

### `Number`

Represents a numeric literal. Internally, it uses `System.Decimal` for high precision.

  - **File**: `Atoms/Number.cs`
  - **Usage**: `new Number(123.45m)`

### `Symbol`

Represents a variable, constant, or indeterminate. It has a `Name` (e.g., "x", "y", "pi") and a `Shape`.

  - **File**: `Atoms/Symbol.cs`
  - **Usage**: `new Symbol("x")` for a scalar, or `new Symbol("V", new Shape(ImmutableArray.Create(3)))` for a 3D vector symbol.

### `Wild`

A special type of atom used exclusively for pattern matching. It acts as a placeholder that can match any expression.

  - **File**: `Atoms/Wild.cs`
  - **Key Properties**:
      - `Name`: A string identifier (e.g., "a", "f") used to retrieve the expression it matched.
      - `Constraint`: An optional `WildConstraint` that restricts what the wild can match.
  - **`WildConstraint` Enum**:
      - `None`: Matches anything.
      - `Scalar`: Matches only scalar expressions.
      - `Constant`: Matches only `Number` atoms.
  - **Usage**: `new Wild("f")` or `new Wild("c", WildConstraint.Constant)`

-----

## 3\. Combining Expressions: Operations

Operations take one or more `IExpression` arguments and combine them. Their most important feature is the `Canonicalize()` method, which performs automatic simplification.

### Commutative Operations: `Add` and `Multiply`

`Add` and `Multiply` are special because the order of their arguments doesn't matter (e.g., $x+y = y+x$). The library enforces this by:

1.  **Flattening**: `Add(Add(a, b), c)` becomes `Add(a, b, c)`.
2.  **Constant Folding**: `Add(x, 2, 3)` becomes `Add(x, 5)`.
3.  **Sorting**: The arguments are sorted into a canonical order. `Add(y, x)` becomes `Add(x, y)`.

This ensures that any two expressions that are algebraically equivalent through commutation and association will have the exact same canonical representation.

```csharp
// C:\Users\dual5\OneDrive\Desktop\Code2025\GithubRepos\Sym\src\Sym\Operations\Add.cs
public override IExpression Canonicalize()
{
    // ...
    ImmutableList<IExpression> flattenedArgs = ExpressionHelpers.FlattenArguments<Add>(canonicalArgs);
    // ... fold numeric sum ...
    ImmutableList<IExpression> sortedArgs = ExpressionHelpers.SortArguments(nonNumericTermsBuilder.ToImmutable());
    // ...
    return new Add(sortedArgs);
}
```

### Binary Operations: `Subtract`, `Divide`, `Power`

For simplicity, binary operations are canonicalized into their more general forms:

  - `Subtract(a, b)` becomes `Add(a, Multiply(-1, b))`
  - `Divide(a, b)` becomes `Multiply(a, Power(b, -1))`

This drastically reduces the number of rules needed in other parts of the system, as they only need to handle `Add` and `Multiply`.

### Calculus and Function Operations

  - **`Derivative(expr, var)`**: Represents $\\frac{d}{d\\text{var}}(\\text{expr})$.
  - **`Integral(expr, var)`**: Represents $\\int \\text{expr} ,d\\text{var}$.
  - **`Function(name, args)`**: A general-purpose operation for functions like `sin(x)` or `log(x, 2)`. It's created like `new Function("sin", ImmutableList.Create<IExpression>(x))`.

-----

## 4\. Tensor Concepts: `Shape`, `Vector`, and `Matrix`

The library has first-class support for vectors and matrices, governed by the `Shape` class.

### `Shape`

A record that defines the dimensions of an expression.

  - **File**: `Core/Shape.cs`
  - **Key Static Instances**:
      - `Shape.Scalar`: For single values.
      - `Shape.Vector`: For 1D arrays, e.g., `new Shape(ImmutableArray.Create(3))` for a 3-vector.
      - `Shape.Matrix`: For 2D arrays, e.g., `new Shape(ImmutableArray.Create(2, 3))` for a 2x3 matrix.
  - The `Shape` class includes logic to determine compatibility for element-wise operations.

### `Vector` and `Matrix` Operations

  - **`Vector`**: An operation whose arguments are its scalar components. `new Vector(ImmutableList.Create(x, y, z))` represents the vector $(x, y, z)$.
  - **`Matrix`**: An operation storing matrix elements in a flat list, with dimensions stored separately.
  - **`DotProduct`**, **`MatrixMultiply`**: Specific operations for linear algebra. The `Multiply` operation's `Canonicalize` method will automatically promote a multiplication of two compatible vectors into a `DotProduct`.
  - **`Grad`**, **`Div`**, **`Curl`**: Vector calculus operations that take a field and a vector variable as arguments. For example, `new Grad(f, V)` where `f` is a scalar function and `V` is `Vector(x,y,z)`.

-----

## 5\. The Engine: Pattern Matching and Rewriting

The real power of `Sym` comes from its ability to transform expressions using a set of rules. This is a three-part system: `Rule`, `Rewriter`, and `MatchResult`.

### `Rule`

A `Rule` defines a single transformation.

  - **File**: `Core/Rule.cs`
  - **Components**:
    1.  `Pattern`: An `IExpression` containing `Wild`s that describes what to look for.
    2.  `Replacement`: An `IExpression` describing the output form. It can use `Wild`s from the pattern.
    3.  `Condition` (optional): A lambda function that can check the matched expressions to decide if the rule should apply.

Here's the rule for the derivative of a constant, $ \\frac{d}{dx}(c) = 0 $:

```csharp
// C:\Users\dual5\OneDrive\Desktop\Code2025\GithubRepos\Sym\src\Sym\Calculus\CalculusRules.cs

// Define wildcards
private static readonly Wild _wildX = new Wild("x");
private static readonly Wild _wildC = new Wild("c", WildConstraint.Constant);

// Define the rule
new Rule(
    // Pattern: Derivative of a constant 'c' w.r.t any 'x'
    new Derivative(_wildC, _wildX),
    // Replacement: The number 0
    new Number(0m)
),
```

Here's a more complex rule with a condition: the power rule for integration, $\\int x^n ,dx = \\frac{x^{n+1}}{n+1}$, which is only valid if $n \\neq -1$.

```csharp
// C:\Users\dual5\OneDrive\Desktop\Code2025\GithubRepos\Sym\src\Sym\Calculus\CalculusRules.cs
new Rule(
    // Pattern: Integral(x^n, x)
    new Integral(new Power(_wildX, _wildN), _wildX),
    // Replacement: x^(n+1) * (n+1)^-1
    new Multiply(ImmutableList.Create<IExpression>(
        new Power(_wildX, new Add(ImmutableList.Create<IExpression>(_wildN, new Number(1m)))),
        new Power(new Add(ImmutableList.Create<IExpression>(_wildN, new Number(1m))), new Number(-1m))
    )),
    // Condition: n must not be -1
    (bindings) => bindings.TryGetValue("n", out IExpression? matchedN) && matchedN is Number numN && numN.Value != -1m
)
```

### `Rewriter`

The `Rewriter` is a static class that applies rules to an expression.

  - **File**: `Core/Rewriter.cs`
  - **Core Logic**:
    1.  **`TryMatch(expression, pattern)`**: Attempts to match the `pattern` against the `expression`. If successful, it returns a `MatchResult` containing the `bindings` (a dictionary mapping `Wild` names to the expressions they matched).
    2.  **`Substitute(replacement, bindings)`**: Takes the `replacement` part of a rule and the `bindings` from a successful match, and builds the new expression.
    3.  **`RewriteSinglePass(expression, rules)`**: Traverses the expression tree once, attempting to apply the first matching rule at each node.
    4.  **`RewriteFully(expression, rules)`**: Repeatedly calls `RewriteSinglePass` until no more changes can be made (a "fixed point" is reached).

-----

## 6\. The Solver Framework

The solver framework provides a clean, high-level API for end-users. It orchestrates the rewriting engine to achieve a specific goal, like simplifying an expression or solving an equation.

### `SymSolver`

The main static entry point for users.

  - **File**: `Core/SymSolver.cs`
  - **Convenience Methods**:
      - `SymSolver.Simplify(...)`
      - `SymSolver.SolveEquation(...)`
  - **Core Method**: `Solve(problem, strategy, context)` which delegates the work to a chosen strategy.

### `ISolverStrategy`

An interface that defines a particular method for solving a problem. This allows the system to be extended with new solving techniques.

  - **File**: `Core/ISolverStrategy.cs`
  - **Implementations**:
      - **`FullSimplificationStrategy`**: The simplest strategy. It repeatedly applies rules using `Rewriter.Rewrite` until the expression is fully simplified.
      - **`EquationSolverStrategy`**: A more complex strategy for solving equations. It first simplifies both sides of an `Equality` expression. Then, it uses **isolation tactics** to rearrange the equation and solve for a `TargetVariable`. For example, if it sees `x + 5 = 10`, it will subtract `5` from both sides.

### `SolveContext` and `SolveResult`

  - **`SolveContext`**: A class that holds all the information for a solving job: the rules to use, the target variable (for equation solving), max iterations, and tracing options.
  - **`SolveResult`**: A class that encapsulates the outcome of a `Solve` call, including a success flag, the final expression, a message, and an optional trace of all intermediate steps.

-----

## 7\. Putting It All Together: Usage Examples

Here’s how you would use the library to perform common tasks.

### Example 1: Simplifying an Expression

Let's simplify the expression `(x + y) * (x - y)` using basic algebraic rules.

```csharp
using Sym.Core;
using Sym.Core.Strategies;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;

// 1. Define symbols
var x = new Symbol("x");
var y = new Symbol("y");

// 2. Define rules (e.g., a rule for distribution)
// Rule: a * (b + c) -> a*b + a*c
var a = new Wild("a");
var b = new Wild("b");
var c = new Wild("c");
var distributionRule = new Rule(
    new Multiply(a, new Add(b, c)),
    new Add(new Multiply(a, b), new Multiply(a, c))
);
var rules = ImmutableList.Create(distributionRule);

// 3. Create the expression
// Note: Subtract and Add/Multiply canonicalization handles a lot automatically
// (x + y) * (x - y) -> (x + y) * (x + -1*y)
var expr = new Multiply(new Add(x, y), new Subtract(x, y));

// 4. Use the solver to simplify
var context = new SolveContext(rules: rules, maxIterations: 10);
var strategy = new FullSimplificationStrategy();
var result = SymSolver.Solve(expr, strategy, context);

// The canonicalization of Add and Multiply will automatically group and cancel terms.
// The final result should be Power(x, 2) + -1 * Power(y, 2)
Console.WriteLine(result.ResultExpression.ToDisplayString());
// Expected output might be something like: ((x**2) + (-1 * (y**2)))
```

*Note: Full algebraic expansion would require more rules, but the built-in canonicalization of `Add` and `Multiply` does most of the heavy lifting like `x*y - y*x = 0`.*

### Example 2: Taking a Derivative

Let's find the derivative of $x^2 + \\sin(x)$ with respect to $x$.

```csharp
using Sym.Core;
using Sym.Calculus; // Where the differentiation rules live
using Sym.Atoms;
using Sym.Operations;

// 1. Define symbols and expression
var x = new Symbol("x");
var expr = new Add(
    new Power(x, new Number(2)),
    new Function("sin", ImmutableList.Create<IExpression>(x))
);

// 2. Create the Derivative operation
var derivative = new Derivative(expr, x);
Console.WriteLine($"Original: {derivative.ToDisplayString()}");

// 3. Use the Simplify convenience method with calculus rules
var result = SymSolver.Simplify(
    derivative,
    CalculusRules.DifferentiationRules,
    maxIterations: 20
);

// 4. Print the result
if (result.IsSuccess)
{
    Console.WriteLine($"Result: {result.ResultExpression.ToDisplayString()}");
    // Expected output will be the canonical form of:
    // (2 * x) + cos(x)
}
```

### Example 3: Solving an Equation

Let's solve the equation $2x - 4 = 6$ for $x$.

```csharp
using Sym.Core;
using Sym.Core.Strategies;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;

// 1. Define symbols and the equation
var x = new Symbol("x");
var two = new Number(2);
var four = new Number(4);
var six = new Number(6);

// An equation is an Equality operation
var equation = new Equality(
    new Subtract(new Multiply(two, x), four),
    six
);

// 2. Use the SolveEquation convenience method.
// No extra rules are needed; the EquationSolverStrategy has built-in
// logic for algebraic isolation.
var result = SymSolver.SolveEquation(
    equation,
    targetVariable: x,
    rules: ImmutableList<Rule>.Empty // No custom rules needed for this
);

// 3. Print the result
if (result.IsSuccess)
{
    // The result will be an Equality expression in the form 'x = ...'
    Console.WriteLine($"Solved: {result.ResultExpression.ToDisplayString()}");
    // Expected Output: (x = 5)
}
```