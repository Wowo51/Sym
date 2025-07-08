# Sym Symbolic Manipulation Engine: A Developer's Guide

This guide provides a comprehensive overview of the **Sym** library, a powerful symbolic mathematics engine written in C\#, and its companion library, **SymIO**, which provides easy-to-use input/output capabilities. We will explore the core architecture, from the fundamental expression tree to the rule-based solver, and demonstrate how to use the high-level API to perform complex symbolic operations.

## 1\. Introduction to the Sym Ecosystem

The Sym ecosystem is composed of two primary projects:

  * **`Sym`**: The core library that defines all symbolic entities and logic. It contains the building blocks for expressions (like numbers, symbols, and operations), a powerful rule-based rewriting engine, and a flexible solver system designed around a strategy pattern. It has no knowledge of how expressions are represented as text; it works purely with the object model.
  * **`SymIO`**: A user-facing library that acts as a bridge between human-readable strings and the `Sym` core. It includes a parser to convert C\#-like mathematical strings into `Sym` expression trees and a formatter to convert those trees back into clean, readable strings.

Together, they provide a complete solution for parsing, solving, and formatting symbolic math problems.

-----

## 2\. Core Concepts of the `Sym` Library

The entire `Sym` engine is built around a central interface: `IExpression`. Every mathematical object, from a simple number to a complex equation, is a type of `IExpression`. This creates a unified, composable system.

### 2.1. Expressions: `Atom` and `Operation`

`IExpression` has two abstract implementations, forming the basis of our expression tree:

  * **`Atom`**: Represents the "leaves" of the expression tree. These are indivisible objects.

      * `Number`: Represents a numerical value, stored internally as a `System.Decimal`.
      * `Symbol`: Represents a variable or constant, like 'x' or 'pi'. It has a `Name` and a `Shape`.
      * `Wild`: A special atom used only for pattern matching in rules. It acts as a placeholder that can match any expression.

  * **`Operation`**: Represents the "nodes" of the expression tree. These are objects that contain other `IExpression`s as arguments.

      * **Arithmetic**: `Add`, `Subtract`, `Multiply`, `Divide`, `Power`.
      * **Calculus**: `Derivative`, `Integral`, `Grad`, `Div`, `Curl`.
      * **Structural**: `Function` (for generic functions like `sin` or `log`), `Vector`, `Matrix`.
      * **Logical**: `Equality` (to represent equations like `lhs = rhs`).

For example, the mathematical expression `2*x + 5` would be represented as an `Add` operation containing two arguments:

1.  A `Multiply` operation with a `Number(2)` and a `Symbol("x")` as arguments.
2.  A `Number(5)`.

### 2.2. The Principle of Canonicalization

One of the most important concepts in the `Sym` library is **canonicalization**. Every `IExpression` has a `Canonicalize()` method that returns a "standard" or "normal" form of that expression.

**Why is this important?**
Consider the expressions `x + 0` and `x`. Mathematically, they are identical. Likewise, `a + b` is the same as `b + a`. For a computer to recognize these equivalences, we need to reduce them to a single, consistent representation.

The `Canonicalize()` method performs several key tasks:

1.  **Simplification**: It performs basic, universally true simplifications.

      * `x + 0` becomes `x`.
      * `y * 1` becomes `y`.
      * `z * 0` becomes `0`.
      * `2 * 3` becomes `6`.

2.  **Normalization**: It converts expressions into a standard structure.

      * `Subtract(a, b)` becomes `Add(a, Multiply(-1, b))`.
      * `Divide(a, b)` becomes `Multiply(a, Power(b, -1))`.
      * This reduces the number of operation types the `Rewriter` needs to handle directly.

3.  **Ordering**: For commutative operations like `Add` and `Multiply`, it sorts the arguments into a consistent order.

      * `Add(c, b, a)` becomes `Add(a, b, c)`.

4.  **Flattening**: It flattens nested operations of the same type.

      * `Add(a, Add(b, c))` becomes `Add(a, b, c)`.

Crucially, the `Equals()` and `GetHashCode()` methods on any `IExpression` are implemented to work on the *canonical form*. This means that `new Add(x, y)` will be considered equal to `new Add(y, x)` because they both canonicalize to the same sorted and flattened `Add` operation.

### 2.3. The Rewriting System

While canonicalization handles fundamental simplifications, more complex transformations require a rule-based rewriting system. This system is composed of `Rule` objects and a `Rewriter` that applies them.

#### `Rule`

A `Rule` defines a single transformation. It consists of three parts:

  * **`Pattern`**: An `IExpression` that may contain `Wild` atoms. This is what the rule looks for.
  * **`Replacement`**: An `IExpression` that defines the output. It can use the `Wild` atoms that were captured in the pattern.
  * **`Condition` (Optional)**: A C\# function (`Func`) that receives the results of a successful match and returns `true` if the rule should be applied.

**Example: The rule for `x / x = 1`**

```csharp
// Wild _wildX = new Wild("x");
new Rule(
    // Pattern: X / X
    new Divide(_wildX, _wildX), 
    
    // Replacement: 1
    new Number(1m),
    
    // Condition: The rule only applies if the matched value for 'x' is not zero.
    (bindings) => bindings.TryGetValue("x", out var x) && (x is not Number n || n.Value != 0m)
);
```

#### `Rewriter`

The static `Rewriter` class is the engine that applies these rules. Its core job is to traverse an expression tree and attempt to apply a list of rules.

  * `TryMatch(expression, pattern)`: This method checks if an `expression` matches a given `pattern`. If it does, it returns a `MatchResult` containing the sub-expressions that were bound to the `Wild`s in the pattern.
  * `Substitute(replacement, bindings)`: This takes a `replacement` pattern and a dictionary of `bindings` (from a `MatchResult`) and builds a new expression by swapping the `Wild`s with their bound values.
  * `Rewrite(expression, rules)`: This method performs a single, top-down pass over the expression tree. It checks for a rule match at the current node. If found, it applies the rule and stops. If not, it recursively calls itself on the node's children.
  * `RewriteFully(expression, rules)`: This method repeatedly calls `Rewrite` on an expression until no more changes can be made, ensuring that all possible rule applications have been performed.

### 2.4. The Solver Framework

To provide a structured way to perform complex tasks like solving equations or simplifying expressions, `Sym` uses a strategy pattern.

  * **`SymSolver`**: A high-level static class that acts as the main entry point for all solving operations. You don't call the `Rewriter` directly; you go through the `SymSolver`.

  * **`ISolverStrategy`**: An interface defining a contract for a solving process. It has a single method: `Solve`.

      * **`FullSimplificationStrategy`**: The simplest strategy. Its goal is to fully simplify an expression. It does this by repeatedly canonicalizing and applying a given set of rules until the expression stops changing.
      * **`EquationSolverStrategy`**: A more complex strategy designed to solve an `Equality` for a specific variable. It uses a combination of full simplification and "isolation tactics"—applying inverse operations to both sides of the equation to isolate the target variable (e.g., if it sees `x + 5 = 10`, it will subtract `5` from both sides).

  * **`SolveContext`**: This object holds all the configuration for a single call to the solver. It specifies which `Rules` to use, the `TargetVariable` to solve for (if any), a `MaxIterations` limit, and whether to enable tracing.

  * **`SolveResult`**: This object encapsulates the result of a `Solve` operation. It tells you if the operation was a `Success`, what the final `ResultExpression` is, and provides a `Message`.

-----

## 3\. High-Level API: `SymIO`

While the `Sym` library is powerful, creating expression trees manually in C\# can be verbose. The `SymIO` library provides a simple, string-based interface for common tasks.

### `CSharpIO` Class

This is the main class you will interact with. It hides all the complexity of parsing and solving.

**Usage:**

```csharp
using SymIO;

// Create an instance of the I/O handler
CSharpIO sym = new CSharpIO();

// Use its methods to perform operations
string result = sym.Simplify("(x + x) * (y - y + 1)"); 
// result will be "2 * x"
```

### Core Methods

The `CSharpIO` class offers several convenient methods that handle parsing the input string, setting up the correct solver strategy and context, executing the solver, and formatting the result back into a clean string.

#### `Simplify(string expression)`

Applies the `FullSimplificationStrategy` using a combined list of all algebraic and calculus rules.

```csharp
string simplified = sym.Simplify("2 * (x + 0) - (x - x)");
// simplified -> "2 * x"
```

#### `Differentiate(string expression, string variable)`

Builds a `Derivative` expression and then simplifies it.

```csharp
string derivative = sym.Differentiate("x**3 + 2*x", "x");
// derivative -> "3 * x ** 2 + 2" 
```

#### `Integrate(string expression, string variable)`

Builds an `Integral` expression and then simplifies it.

```csharp
string integral = sym.Integrate("3*x**2 + cos(x)", "x");
// integral -> "x ** 3 + sin(x)"
```

#### `Solve(string equation, string variable)`

Parses an equation string, identifies the target variable, and uses the `EquationSolverStrategy` to find a solution.

```csharp
string solution = sym.Solve("2*x + 10 = 20", "x");
// solution -> "x = 5"
```

#### Vector Calculus Methods

The `Grad`, `Div`, and `Curl` methods work similarly, constructing the appropriate vector calculus operation and simplifying the result.

```csharp
// Gradient
string grad = sym.Grad("x**2 * y", "Vector(x, y)");
// grad -> "Vector(2 * x * y, x ** 2)"

// Divergence
string div = sym.Div("Vector(x**2, y**2)", "Vector(x, y)");
// div -> "2 * x + 2 * y"

// Curl (2D case for simplicity)
string curl = sym.Curl("Vector(-y, x)", "Vector(x, y)");
// curl -> "2" (Note: Simplified from Vector(0, 0, 2), depending on full 3D rules)
```

-----

## 4\. How It Works: A Complete Walkthrough

Let's trace the execution of `sym.Solve("2 * x + 5 = 15", "x")`.

1.  **`SymIO.CSharpIO.Solve` is called.**

      * The input strings `"2 * x + 5 = 15"` and `"x"` are received.

2.  **Parsing (`ExpressionParser`)**

      * The `Tokenizer` splits the input string into a stream of tokens: `NUMBER(2)`, `STAR`, `SYMBOL(x)`, `PLUS`, `NUMBER(5)`, `EQUALS`, `NUMBER(15)`.
      * The `ExpressionParser` consumes these tokens using recursive descent. It recognizes the `EQUALS` token and creates an `Equality` operation.
      * The left-hand side is parsed into `Add(Multiply(Number(2), Symbol("x")), Number(5))`.
      * The right-hand side is parsed into `Number(15)`.
      * A `Symbol("x")` is created for the target variable.
      * The final parsed object is `problem = Equality(lhs, rhs)`.

3.  **Solver Setup (`SymSolver.SolveEquation`)**

      * An `EquationSolverStrategy` is instantiated.
      * A `SolveContext` is created, containing the target `Symbol("x")` and the complete list of algebraic and calculus rules.

4.  **Execution (`EquationSolverStrategy.Solve`)**

      * The strategy receives the `Equality` expression and the context.
      * It enters a loop, up to `MaxIterations`.
      * **Iteration 1:**
          * **Simplification:** The strategy first calls `Rewriter.RewriteFully` on `2*x + 5 = 15`. No standard simplification rules apply, so the expression remains unchanged.
          * **Goal Check:** The goal `x = solution` is not met.
          * **Isolation:** The strategy identifies that `x` is on the left side. It examines the outermost operation on that side: `Add`.
          * It sees `(2*x) + 5`. To isolate the term with `x`, it needs to remove the `+ 5`. It does this by creating a new equation where `5` is subtracted from both sides.
          * New LHS: `2*x`.
          * New RHS: `15 - 5`.
          * The new equation is `Equality(Multiply(Number(2), Symbol("x")), Subtract(Number(15), Number(5)))`.
          * This new equation is immediately canonicalized: `Subtract(15, 5)` becomes `Number(10)`.
          * The result of the first isolation step is `Equality(Multiply(Number(2), Symbol("x")), Number(10))`, which we can write as `2*x = 10`.
      * **Iteration 2:**
          * **Simplification:** No rules apply to `2*x = 10`.
          * **Goal Check:** The goal is not met.
          * **Isolation:** The strategy sees `2*x = 10`. The outermost operation on the left is `Multiply`.
          * To isolate `x`, it must divide both sides by `2`.
          * New LHS: `x`.
          * New RHS: `10 / 2`.
          * The new equation is `Equality(Symbol("x"), Divide(Number(10), Number(2)))`.
          * This is canonicalized: `Divide(10, 2)` becomes `Number(5)`.
          * The result is the equation `Equality(Symbol("x"), Number(5))`, or `x = 5`.
      * **Iteration 3:**
          * **Simplification:** No rules apply.
          * **Goal Check:** The strategy calls `CheckGoal`. It sees that the left side is the `targetVariable` (`x`) and the right side (`5`) does not contain the target variable. The check returns `true`.
          * The strategy packages `x = 5` into a `SolveResult.Success` and returns.

5.  **Formatting (`ParenthesisEliminationRules.Format`)**

      * The `CSharpIO` class receives the successful `SolveResult`.
      * It passes the `ResultExpression` (`Equality(Symbol("x"), Number(5))`) to the `Format` method.
      * The formatter traverses the expression, building the string "x = 5" without any unnecessary parentheses.

6.  **Return Value**

      * The final formatted string `"x = 5"` is returned to the user.