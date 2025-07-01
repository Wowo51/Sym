//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System;

namespace Sym.Calculus
{
    /// <summary>
    /// Provides a collection of standard rewrite rules for basic calculus operations.
    /// </summary>
    public static class CalculusRules
    {
        // Define common wildcards for clarity and reusability
        private static readonly Wild _wildF = new Wild("f");
        private static readonly Wild _wildG = new Wild("g");
        private static readonly Wild _wildX = new Wild("x");
        private static readonly Wild _wildC = new Wild("c", WildConstraint.Constant);
        private static readonly Wild _wildN = new Wild("n");
        private static readonly Wild _wildV = new Wild("v");

        // New wildcards for Div/Curl vector components
        private static readonly Wild _wildFx = new Wild("fx");
        private static readonly Wild _wildFy = new Wild("fy");
        private static readonly Wild _wildFz = new Wild("fz");
        private static readonly Wild _wildVarX = new Wild("varX");
        private static readonly Wild _wildVarY = new Wild("varY");
        private static readonly Wild _wildVarZ = new Wild("varZ");

        // Specific symbols for common 3D Cartesian coordinates, as needed by some rules
        // These are only for _symX, _symY, _symZ, their usage eliminated from Grad rule as per plan.
        private static readonly Symbol _symX = new Symbol("x");
        private static readonly Symbol _symY = new Symbol("y");
        private static readonly Symbol _symZ = new Symbol("z");


        /// <summary>
        /// Rules for symbolic differentiation.
        /// </summary>
        public static ImmutableList<Rule> DifferentiationRules { get; }

        /// <summary>
        /// Rules for symbolic integration.
        /// </summary>
        public static ImmutableList<Rule> IntegrationRules { get; }

        /// <summary>
        /// Rules for vector calculus operations (Grad, Div, Curl).
        /// </summary>
        public static ImmutableList<Rule> VectorCalculusRules { get; }

        static CalculusRules()
        {
            // Initialize Differentiation Rules
            DifferentiationRules = ImmutableList.Create(
                // Rule: d/dx (c) = 0
                new Rule(
                    new Derivative(_wildC, _wildX),
                    new Number(0m)
                ),
                // Rule: d/dx (x) = 1
                new Rule(
                    new Derivative(_wildX, _wildX),
                    new Number(1m)
                ),
                // Rule: d/dx (f + g) = d/dx (f) + d/dx (g) (Sum Rule)
                new Rule(
                    new Derivative(new Add(ImmutableList.Create<IExpression>(_wildF, _wildG)), _wildX),
                    new Add(ImmutableList.Create<IExpression>(new Derivative(_wildF, _wildX), new Derivative(_wildG, _wildX)))
                ),
                // Rule: d/dx (f * g) = f'g + fg' (Product Rule)
                new Rule(
                    new Derivative(new Multiply(ImmutableList.Create<IExpression>(_wildF, _wildG)), _wildX),
                    new Add(ImmutableList.Create<IExpression>(
                        new Multiply(ImmutableList.Create<IExpression>(new Derivative(_wildF, _wildX), _wildG)),
                        new Multiply(ImmutableList.Create<IExpression>(_wildF, new Derivative(_wildG, _wildX)))
                    ))
                ),
                // Rule: d/dx (f^n) = n * f^(n-1) * f' (Power Rule with Chain Rule for f)
                new Rule(
                    new Derivative(new Power(_wildF, _wildN), _wildX), // d/dx(f^n)
                    new Multiply(ImmutableList.Create<IExpression>(
                        _wildN,
                        new Power(_wildF, new Add(ImmutableList.Create<IExpression>(_wildN, new Number(-1m)))), // n * f^(n-1)
                        new Derivative(_wildF, _wildX) // * f' (chain rule)
                    )),
                    // Condition: n must be a number for this particular rule, but f can be anything.
                    (bindings) => bindings.TryGetValue("n", out IExpression? matchedN) && matchedN is Number
                ),
                // Rule: d/dx (sin(f)) = cos(f) * f' (Chain Rule for Sin)
                new Rule(
                    new Derivative(new Function("sin", ImmutableList.Create<IExpression>(_wildF)), _wildX),
                    new Multiply(ImmutableList.Create<IExpression>(
                        new Function("cos", ImmutableList.Create<IExpression>(_wildF)),
                        new Derivative(_wildF, _wildX)
                    ))
                )
            );

            // Initialize Integration Rules
            IntegrationRules = ImmutableList.Create(
                // Rule: Integral(0, x) = 0
                new Rule(
                    new Integral(new Number(0m), _wildX),
                    new Number(0m)
                ),
                // Rule: Integral(x^n, x) = x^(n+1) / (n+1), for n != -1
                new Rule(
                    new Integral(new Power(_wildX, _wildN), _wildX),
                    new Multiply(ImmutableList.Create<IExpression>(
                        new Power(_wildX, new Add(ImmutableList.Create<IExpression>(_wildN, new Number(1m)))),
                        new Power(new Add(ImmutableList.Create<IExpression>(_wildN, new Number(1m))), new Number(-1m))
                    )),
                    // Condition: n must not be -1
                    (bindings) => bindings.TryGetValue("n", out IExpression? matchedN) && matchedN is Number numN && numN.Value != -1m
                ),
                // Rule: Integral(cos(x), x) = sin(x)
                new Rule(
                    new Integral(new Function("cos", ImmutableList.Create<IExpression>(_wildX)), _wildX),
                    new Function("sin", ImmutableList.Create<IExpression>(_wildX))
                )
            );

            // Initialize Vector Calculus Rules
            VectorCalculusRules = ImmutableList.Create(
                // Rule: Grad(f, v) -> Vector(d/dx, d/dy, d/dz) for v = (x, y, z) dynamically matched
                new Rule(
                    new Grad(_wildF,
                        new Vector(ImmutableList.Create<IExpression>( // Pattern for a vector variable with wildcard components
                            _wildVarX,
                            _wildVarY,
                            _wildVarZ
                        ))
                    ),
                    new Vector(ImmutableList.Create<IExpression>(
                        new Derivative(_wildF, _wildVarX), // Use the wildcard-bound components in replacement
                        new Derivative(_wildF, _wildVarY),
                        new Derivative(_wildF, _wildVarZ)
                    ))
                ),
                // Rule: Div(Vector(fx, fy, fz), Vector(varX, varY, varZ)) = dfx/dvarX + dfy/dvarY + dfz/dvarZ (3D)
                new Rule(
                    new Div(
                        new Vector(ImmutableList.Create<IExpression>(_wildFx, _wildFy, _wildFz)),
                        new Vector(ImmutableList.Create<IExpression>(_wildVarX, _wildVarY, _wildVarZ))
                    ),
                    new Add(ImmutableList.Create<IExpression>(
                        new Derivative(_wildFx, _wildVarX),
                        new Derivative(_wildFy, _wildVarY),
                        new Derivative(_wildFz, _wildVarZ)
                    ))
                ),
                // Rule: Curl(Vector(fx, fy, fz), Vector(varX, varY, varZ)) (3D)
                // = Vector(dfz/dvarY - dfy/dvarZ, dfx/dvarZ - dfz/dvarX, dfy/dvarX - dfx/dvarY)
                new Rule(
                    new Curl(
                        new Vector(ImmutableList.Create<IExpression>(_wildFx, _wildFy, _wildFz)),
                        new Vector(ImmutableList.Create<IExpression>(_wildVarX, _wildVarY, _wildVarZ))
                    ),
                    new Vector(ImmutableList.Create<IExpression>(
                        // Component 1: dfz/dvarY - dfy/dvarZ
                        new Add(ImmutableList.Create<IExpression>(new Derivative(_wildFz, _wildVarY), new Multiply(ImmutableList.Create<IExpression>(new Number(-1m), new Derivative(_wildFy, _wildVarZ))))),
                        // Component 2: dfx/dvarZ - dfz/dvarX
                        new Add(ImmutableList.Create<IExpression>(new Derivative(_wildFx, _wildVarZ), new Multiply(ImmutableList.Create<IExpression>(new Number(-1m), new Derivative(_wildFz, _wildVarX))))),
                        // Component 3: dfy/dvarX - dfx/dvarY
                        new Add(ImmutableList.Create<IExpression>(new Derivative(_wildFy, _wildVarX), new Multiply(ImmutableList.Create<IExpression>(new Number(-1m), new Derivative(_wildFx, _wildVarY)))))
                    ))
                )
            );
        }
    }
}