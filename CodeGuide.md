## `Sym`: A Guide to the Symbolic Mathematics Library

This document provides a comprehensive overview of the `Sym` library, a powerful C\# framework designed for symbolic mathematics. It allows for the representation and manipulation of mathematical expressions as expression trees, using a rule-based rewriting engine to perform operations like simplification, calculus, and more.

### \#\# Core Concepts

The entire library is built around a few core concepts that represent mathematical expressions as a tree structure.

#### 1\. `IExpression`: The Foundation

Everything in the `Sym` library, from a number to a complex integral, implements the `IExpression` interface. This provides a unified way to work with any mathematical object. The most important methods defined by an expression are:

  * **`Canonicalize()`**: This is the cornerstone of the library. It converts an expression into a standardized, simplified, and unambiguous "canonical" form. For example, `(x + 1 + y)` and `(y + x + 1)` would canonicalize to the same object, likely with sorted terms. All equality checks and hashing are based on this canonical form, ensuring that mathematically equivalent expressions are treated as equal.
  * **`Shape`**: Represents the tensor shape of the expression (e.g., scalar, vector, or matrix).
  * **`ToDisplayString()`**: Returns a human-readable string representation of the expression.

#### 2\. `Atom`: The Leaves of the Tree

An **`Atom`** is the simplest type of expression; it has no sub-expressions (arguments). These are the "leaves" of our expression tree. The primary atoms are:

  * **`Number`**: Represents a numeric constant. Uses `System.Decimal` for high precision.
  * **`Symbol`**: Represents a variable or a named constant, like 'x' or 'pi'.
  * **`Wild`**: A special-purpose atom used exclusively for pattern matching. A `Wild` object (e.g., `new Wild("f")`) acts as a placeholder that can match any expression during a rewrite operation.

#### 3\. `Operation`: The Branches of the Tree

An **`Operation`** represents a node in the expression tree that has arguments. These are the "branches" of the tree. Examples include:

  * **Basic Arithmetic**: `Add`, `Multiply`, `Power`
  * **Calculus**: `Derivative`, `Integral`
  * **Vector/Matrix**: `Vector`, `Matrix`, `Grad`, `Div`, `Curl`, `DotProduct`, `MatrixMultiply`
  * **General**: `Function` (e.g., to represent `sin(x)`)

Each `Operation` holds an `ImmutableList<IExpression>` of its arguments. For instance, `new Add(x, y)` is an `Add` operation with two `Symbol` arguments.

### \#\# The Rewriting Engine: Bringing Expressions to Life

The true power of `Sym` lies in its ability to transform expressions. This is handled by the `Rewriter` class, which uses a system of rules to simplify and evaluate expressions.

#### How it Works

The rewriting process involves three key components:

1.  **`Rule`**: A `Rule` defines a single transformation. It consists of a `Pattern`, a `Replacement`, and an optional `Condition`.

      * **Pattern**: An expression (often containing `Wild` objects) that the rewriter searches for.
      * **Replacement**: The expression to substitute in place of the matched pattern.
      * **Condition**: A function that can check the matched expressions to see if the rule should apply.

    For example, the rule for the derivative of a constant (`d/dx(c) = 0`) is defined like this:

    ```csharp
    // Pattern: Derivative(Wild("c", WildConstraint.Constant), Wild("x"))
    // Replacement: new Number(0)

    new Rule(
        new Derivative(_wildC, _wildX), // _wildC is a Wild with a Constant constraint
        new Number(0m)
    );
    ```

2.  **Pattern Matching (`TryMatch`)**: The rewriter traverses an expression and tries to match it against a rule's pattern. If `TryMatch` is called with the expression `Derivative(5, x)` and the pattern above, it succeeds. It returns a `MatchResult` containing the "bindings":

      * The wildcard `_wildC` successfully matched the `Number(5)`.
      * The wildcard `_wildX` successfully matched the `Symbol("x")`.

3.  **Substitution (`Substitute`)**: Once a match is found, the bindings are applied to the rule's `Replacement` expression. Since the replacement is `new Number(0m)` and contains no wildcards, the result is simply `new Number(0m)`.

4.  **Rewriting (`Rewrite`)**: The main `Rewrite` method orchestrates this process. It takes an expression and a list of rules, and repeatedly applies them to the expression and all its sub-expressions until no more changes can be made.

### \#\# How to Use the Library

Using the `Sym` library involves creating expressions and then applying rules to them using the `Rewriter`.

#### Practical Example: Symbolic Differentiation

Let's differentiate the expression $f(x) = x^2 + 2x$ with respect to $x$.

The process is:

1.  Define the necessary `Symbol` and `Number` atoms.
2.  Construct the expression tree for `Derivative(x**2 + 2*x, x)`.
3.  Load the predefined `DifferentiationRules` from `CalculusRules`.
4.  Call `Rewriter.Rewrite` to apply the rules.
5.  Display the result.

<!-- end list -->

```csharp
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using Sym.Calculus;
using System.Collections.Immutable;

public class Program
{
    public static void Main()
    {
        // 1. Define the symbol 'x'.
        var x = new Symbol("x");

        // 2. Construct the expression for d/dx (x^2 + 2*x).
        var expression = new Derivative(
            new Add(ImmutableList.Create<IExpression>(
                new Power(x, new Number(2)),
                new Multiply(ImmutableList.Create<IExpression>(new Number(2), x))
            )),
            x
        );

        Console.WriteLine($"Original Expression: {expression.ToDisplayString()}");

        // 3. Get the list of differentiation rules.
        var differentiationRules = CalculusRules.DifferentiationRules;

        // 4. Use the rewriter to apply the rules and simplify the expression.
        IExpression result = Rewriter.Rewrite(expression, differentiationRules);

        // 5. The result is another expression tree, which we can display.
        // The final canonicalization will sort and simplify the result.
        Console.WriteLine($"Differentiated Result: {result.Canonicalize().ToDisplayString()}");
        // Expected Output: (2 + (2 * x)) or a similar canonical form.
    }
}
```

#### How the Example Works Internally:

1.  `Rewrite` is called on the top-level `Derivative` expression.
2.  It doesn't match a simple rule at first, so it recursively calls `Rewrite` on its arguments.
3.  The `Add` expression is rewritten. The derivative sum rule `d/dx(f+g) = d/dx(f) + d/dx(g)` is applied.
4.  The expression becomes `Derivative(x**2, x) + Derivative(2*x, x)`.
5.  `Rewrite` continues on these new, simpler derivatives.
      * `Derivative(x**2, x)` matches the power rule, becoming `2 * x**(2-1) * Derivative(x,x)`. This is further rewritten to `2 * x * 1`, which canonicalizes to `(2 * x)`.
      * `Derivative(2*x, x)` matches the product rule, becoming `(d/dx(2)*x + 2*d/dx(x))`. This is rewritten to `(0*x + 2*1)`, which canonicalizes to `2`.
6.  The final result is an `Add` operation with arguments `2` and `(2 * x)`, which is then canonicalized for the final display.