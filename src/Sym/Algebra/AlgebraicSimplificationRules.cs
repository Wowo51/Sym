// Copyright Warren Harding 2025
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Collections.Generic; // Required for ImmutableDictionary<string, IExpression> bindings

namespace Sym.Algebra
{
    /// <summary>
    /// Provides a collection of algebraic simplification rules for the Sym symbolic mathematics system.
    /// These rules cover fundamental arithmetic identities, combining like terms, and distributive properties.
    /// </summary>
    public static class AlgebraicSimplificationRules
    {
        /// <summary>
        /// Gets the immutable list of algebraic simplification rules.
        /// </summary>
        public static ImmutableList<Rule> SimplificationRules { get; }

        /// <summary>
        /// Initializes the <see cref="AlgebraicSimplificationRules"/> class, populating the simplification rules.
        /// </summary>
        static AlgebraicSimplificationRules()
        {
            // Define common wildcards for clarity and reusability
            Wild _wildX = new Wild("x");
            Wild _wildY = new Wild("y");
            Wild _wildZ = new Wild("z");
            Wild _wildA = new Wild("a");
            Wild _wildB = new Wild("b");
            Wild _wildC = new Wild("c"); // General wildcard for patterns
            Wild _wildN = new Wild("n");
            Wild _wildM = new Wild("m");

            SimplificationRules = ImmutableList.Create<Rule>(
                // --- Additive and Subtractive Rules ---
                // Additive Identity: X + 0 = X
                new Rule(new Add(_wildX, new Number(0m)), _wildX),
                // Subtraction Identity: X - 0 = X
                new Rule(new Subtract(_wildX, new Number(0m)), _wildX),
                // Subtraction of Self: X - X = 0
                new Rule(new Subtract(_wildX, _wildX), new Number(0m)),
                // Subtraction from Zero: 0 - X = -X
                new Rule(new Subtract(new Number(0m), _wildX), new Multiply(new Number(-1m), _wildX)),
                // Double Negative: -(-X) = X
                new Rule(new Multiply(new Number(-1m), new Multiply(new Number(-1m), _wildX)), _wildX),

                // --- Multiplicative and Divisive Rules ---
                // Multiplicative Identity: X * 1 = X
                new Rule(new Multiply(_wildX, new Number(1m)), _wildX),
                // Zero Multiplication: X * 0 = 0
                new Rule(new Multiply(_wildX, new Number(0m)), new Number(0m)),
                // Division by One: X / 1 = X
                new Rule(new Divide(_wildX, new Number(1m)), _wildX),
                // Division by Self: X / X = 1 (for X != 0)
                new Rule(new Divide(_wildX, _wildX), new Number(1m),
                    (bindings) => bindings.TryGetValue("x", out var x) && (x is not Number n || n.Value != 0m)),
                // Zero Divided by X: 0 / X = 0 (for X != 0)
                new Rule(new Divide(new Number(0m), _wildX), new Number(0m),
                    (bindings) => bindings.TryGetValue("x", out var x) && (x is not Number n || n.Value != 0m)),
                // Division by Negative One: X / -1 = -X
                new Rule(new Divide(_wildX, new Number(-1m)), new Multiply(new Number(-1m), _wildX)),
                // Reciprocal of a Reciprocal: 1 / (1 / X) -> X
                new Rule(new Divide(new Number(1m), new Divide(new Number(1m), _wildX)), _wildX),

                // --- Power Rules ---
                // Power by 1: X^1 = X
                new Rule(new Power(_wildX, new Number(1m)), _wildX),
                // Power by 0: X^0 = 1 (for X != 0)
                new Rule(new Power(_wildX, new Number(0m)), new Number(1m),
                    (bindings) => bindings.TryGetValue("x", out IExpression? matchedX) && (matchedX is not Number numX || numX.Value != 0m)),
                // Power of 1: 1^X = 1
                new Rule(new Power(new Number(1m), _wildX), new Number(1m)),
                // Power of 0: 0^X = 0 (for positive X)
                new Rule(new Power(new Number(0m), _wildX), new Number(0m),
                    (bindings) => bindings.TryGetValue("x", out var x) && (x is Number n && n.Value > 0m)),
                // Negative Exponent: X^(-N) = 1 / X^N
                new Rule(new Power(_wildX, new Multiply(new Number(-1m), _wildN)), new Divide(new Number(1m), new Power(_wildX, _wildN))),

                // ?? ADD THESE TWO NEW RULES
                // Product with Power: X * X^N = X^(1+N)
                new Rule(new Multiply(_wildX, new Power(_wildX, _wildN)), new Power(_wildX, new Add(new Number(1m), _wildN))),
                new Rule(new Multiply(new Power(_wildX, _wildN), _wildX), new Power(_wildX, new Add(_wildN, new Number(1m)))),

                // Product of Powers: X^N * X^M = X^(N+M)
                new Rule(new Multiply(new Power(_wildX, _wildN), new Power(_wildX, _wildM)), new Power(_wildX, new Add(_wildN, _wildM))),
                // Power of a Power: (X^N)^M = X^(N*M)
                new Rule(new Power(new Power(_wildX, _wildN), _wildM), new Power(_wildX, new Multiply(_wildN, _wildM))),
                // Power over Product: (X*Y)^N = X^N * Y^N
                new Rule(new Power(new Multiply(_wildX, _wildY), _wildN), new Multiply(new Power(_wildX, _wildN), new Power(_wildY, _wildN))),
                // Power over Quotient: (X/Y)^N = X^N / Y^N
                new Rule(new Power(new Divide(_wildX, _wildY), _wildN), new Divide(new Power(_wildX, _wildN), new Power(_wildY, _wildN))),

                // --- Combining and Factoring Rules ---
                // Direct combining of identical terms: x + x = 2 * x (Handled by canonicalization, but useful as a rule)
                new Rule(new Add(_wildX, _wildX), new Multiply(new Number(2m), _wildX)),
                // General Combining like terms: A*X + B*X = (A+B)*X
                new Rule(new Add(new Multiply(_wildA, _wildX), new Multiply(_wildB, _wildX)),
                         new Multiply(new Add(_wildA, _wildB), _wildX)),
                // Combining like terms with implicit 1: X + A*X = (1+A)*X
                new Rule(new Add(_wildX, new Multiply(_wildA, _wildX)), new Multiply(new Add(new Number(1m), _wildA), _wildX)),
                new Rule(new Add(new Multiply(_wildA, _wildX), _wildX), new Multiply(new Add(_wildA, new Number(1m)), _wildX)),

                // --- Distributive and Reverse-Distributive (Factoring) Rules ---
                // Distributive property (expand): A * (B + C) = A*B + A*C
                new Rule(new Multiply(_wildA, new Add(_wildX, _wildY)),
                         new Add(new Multiply(_wildA, _wildX), new Multiply(_wildA, _wildY))),
                // Distributive property (expand): (A + B) * C = A*C + B*C
                new Rule(new Multiply(new Add(_wildA, _wildB), _wildC),
                         new Add(new Multiply(_wildA, _wildC), new Multiply(_wildB, _wildC))),
                // Factoring out a common term (common multiplier on the right): A*C + B*C = (A+B)*C
                new Rule(new Add(new Multiply(_wildA, _wildC), new Multiply(_wildB, _wildC)),
                         new Multiply(new Add(_wildA, _wildB), _wildC)),
                // Factoring out a common term (common multiplier on the left): C*A + C*B = C*(A+B)
                new Rule(new Add(new Multiply(_wildC, _wildA), new Multiply(_wildC, _wildB)),
                         new Multiply(_wildC, new Add(_wildA, _wildB)))
            );
        }
    }
}





//// Copyright Warren Harding 2025
//using Sym.Core;
//using Sym.Atoms;
//using Sym.Operations;
//using System.Collections.Immutable;
//using System.Collections.Generic; // Required for ImmutableDictionary<string, IExpression> bindings

//namespace Sym.Algebra
//{
//    /// <summary>
//    /// Provides a collection of algebraic simplification rules for the Sym symbolic mathematics system.
//    /// These rules cover fundamental arithmetic identities, combining like terms, and distributive properties.
//    /// </summary>
//    public static class AlgebraicSimplificationRules
//    {
//        /// <summary>
//        /// Gets the immutable list of algebraic simplification rules.
//        /// </summary>
//        public static ImmutableList<Rule> SimplificationRules { get; }

//        /// <summary>
//        /// Initializes the <see cref="AlgebraicSimplificationRules"/> class, populating the simplification rules.
//        /// </summary>
//        static AlgebraicSimplificationRules()
//        {
//            // Define common wildcards for clarity and reusability
//            Wild _wildX = new Wild("x");
//            Wild _wildY = new Wild("y");
//            Wild _wildZ = new Wild("z");
//            Wild _wildA = new Wild("a");
//            Wild _wildB = new Wild("b");
//            Wild _wildC = new Wild("c"); // General wildcard for patterns
//            Wild _wildN = new Wild("n");
//            Wild _wildM = new Wild("m");

//            SimplificationRules = ImmutableList.Create<Rule>(
//                // --- Additive and Subtractive Rules ---
//                // Additive Identity: X + 0 = X
//                new Rule(new Add(_wildX, new Number(0m)), _wildX),
//                // Subtraction Identity: X - 0 = X
//                new Rule(new Subtract(_wildX, new Number(0m)), _wildX),
//                // Subtraction of Self: X - X = 0
//                new Rule(new Subtract(_wildX, _wildX), new Number(0m)),
//                // Subtraction from Zero: 0 - X = -X
//                new Rule(new Subtract(new Number(0m), _wildX), new Multiply(new Number(-1m), _wildX)),
//                // Double Negative: -(-X) = X
//                new Rule(new Multiply(new Number(-1m), new Multiply(new Number(-1m), _wildX)), _wildX),

//                // --- Multiplicative and Divisive Rules ---
//                // Multiplicative Identity: X * 1 = X
//                new Rule(new Multiply(_wildX, new Number(1m)), _wildX),
//                // Zero Multiplication: X * 0 = 0
//                new Rule(new Multiply(_wildX, new Number(0m)), new Number(0m)),
//                // Division by One: X / 1 = X
//                new Rule(new Divide(_wildX, new Number(1m)), _wildX),
//                // Division by Self: X / X = 1 (for X != 0)
//                new Rule(new Divide(_wildX, _wildX), new Number(1m),
//                    (bindings) => bindings.TryGetValue("x", out var x) && (x is not Number n || n.Value != 0m)),
//                // Zero Divided by X: 0 / X = 0 (for X != 0)
//                new Rule(new Divide(new Number(0m), _wildX), new Number(0m),
//                    (bindings) => bindings.TryGetValue("x", out var x) && (x is not Number n || n.Value != 0m)),
//                // Division by Negative One: X / -1 = -X
//                new Rule(new Divide(_wildX, new Number(-1m)), new Multiply(new Number(-1m), _wildX)),
//                // Reciprocal of a Reciprocal: 1 / (1 / X) -> X
//                new Rule(new Divide(new Number(1m), new Divide(new Number(1m), _wildX)), _wildX),

//                // --- Power Rules ---
//                // Power by 1: X^1 = X
//                new Rule(new Power(_wildX, new Number(1m)), _wildX),
//                // Power by 0: X^0 = 1 (for X != 0)
//                new Rule(new Power(_wildX, new Number(0m)), new Number(1m),
//                    (bindings) => bindings.TryGetValue("x", out IExpression? matchedX) && (matchedX is not Number numX || numX.Value != 0m)),
//                // Power of 1: 1^X = 1
//                new Rule(new Power(new Number(1m), _wildX), new Number(1m)),
//                // Power of 0: 0^X = 0 (for positive X)
//                new Rule(new Power(new Number(0m), _wildX), new Number(0m),
//                    (bindings) => bindings.TryGetValue("x", out var x) && (x is Number n && n.Value > 0m)),
//                // Negative Exponent: X^(-N) = 1 / X^N
//                new Rule(new Power(_wildX, new Multiply(new Number(-1m), _wildN)), new Divide(new Number(1m), new Power(_wildX, _wildN))),
//                // Product of Powers: X^N * X^M = X^(N+M)
//                new Rule(new Multiply(new Power(_wildX, _wildN), new Power(_wildX, _wildM)), new Power(_wildX, new Add(_wildN, _wildM))),
//                // Power of a Power: (X^N)^M = X^(N*M)
//                new Rule(new Power(new Power(_wildX, _wildN), _wildM), new Power(_wildX, new Multiply(_wildN, _wildM))),
//                // Power over Product: (X*Y)^N = X^N * Y^N
//                new Rule(new Power(new Multiply(_wildX, _wildY), _wildN), new Multiply(new Power(_wildX, _wildN), new Power(_wildY, _wildN))),
//                // Power over Quotient: (X/Y)^N = X^N / Y^N
//                new Rule(new Power(new Divide(_wildX, _wildY), _wildN), new Divide(new Power(_wildX, _wildN), new Power(_wildY, _wildN))),

//                // --- Combining and Factoring Rules ---
//                // Direct combining of identical terms: x + x = 2 * x (Handled by canonicalization, but useful as a rule)
//                new Rule(new Add(_wildX, _wildX), new Multiply(new Number(2m), _wildX)),
//                // General Combining like terms: A*X + B*X = (A+B)*X
//                new Rule(new Add(new Multiply(_wildA, _wildX), new Multiply(_wildB, _wildX)),
//                         new Multiply(new Add(_wildA, _wildB), _wildX)),
//                // Combining like terms with implicit 1: X + A*X = (1+A)*X
//                new Rule(new Add(_wildX, new Multiply(_wildA, _wildX)), new Multiply(new Add(new Number(1m), _wildA), _wildX)),
//                new Rule(new Add(new Multiply(_wildA, _wildX), _wildX), new Multiply(new Add(_wildA, new Number(1m)), _wildX)),

//                // --- Distributive and Reverse-Distributive (Factoring) Rules ---
//                // Distributive property (expand): A * (B + C) = A*B + A*C
//                new Rule(new Multiply(_wildA, new Add(_wildX, _wildY)),
//                         new Add(new Multiply(_wildA, _wildX), new Multiply(_wildA, _wildY))),
//                // Distributive property (expand): (A + B) * C = A*C + B*C
//                new Rule(new Multiply(new Add(_wildA, _wildB), _wildC),
//                         new Add(new Multiply(_wildA, _wildC), new Multiply(_wildB, _wildC))),
//                // Factoring out a common term (common multiplier on the right): A*C + B*C = (A+B)*C
//                new Rule(new Add(new Multiply(_wildA, _wildC), new Multiply(_wildB, _wildC)),
//                         new Multiply(new Add(_wildA, _wildB), _wildC)),
//                // Factoring out a common term (common multiplier on the left): C*A + C*B = C*(A+B)
//                new Rule(new Add(new Multiply(_wildC, _wildA), new Multiply(_wildC, _wildB)),
//                         new Multiply(_wildC, new Add(_wildA, _wildB)))
//            );
//        }
//    }
//}