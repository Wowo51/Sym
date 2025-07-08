// Copyright Warren Harding 2025
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Collections.Generic; // Required for ImmutableDictionary<string, IExpression> bindings

namespace Sym.Calculus
{
    /// <summary>
    /// Provides a static collection of calculus rules (differentiation, integration, and vector calculus)
    /// for the Sym symbolic mathematics system.
    /// </summary>
    public static class CalculusRules
    {
        // Wildcard declarations
        private static readonly Wild _wildX = new Wild("x");
        private static readonly Wild _wildY = new Wild("y");
        private static readonly Wild _wildF = new Wild("f");
        private static readonly Wild _wildN = new Wild("n");
        private static readonly Wild _wildC = new Wild("c"); // For general constant values

        // New wildcards for general expressions as vector components, and for variables
        private static readonly Wild _wildExp1 = new Wild("exp1");
        private static readonly Wild _wildExp2 = new Wild("exp2");
        private static readonly Wild _wildExp3 = new Wild("exp3");
        private static readonly Wild _wildVar1 = new Wild("var1");
        private static readonly Wild _wildVar2 = new Wild("var2");
        private static readonly Wild _wildVar3 = new Wild("var3");

        /// <summary>
        /// Gets the immutable list of differentiation rules.
        /// </summary>
        public static ImmutableList<Rule> DifferentiationRules { get; }

        /// <summary>
        /// Gets the immutable list of integration rules.
        /// </summary>
        public static ImmutableList<Rule> IntegrationRules { get; }

        /// <summary>
        /// Gets the immutable list of vector calculus rules.
        /// </summary>
        public static ImmutableList<Rule> VectorCalculusRules { get; }

        /// <summary>
        /// Initializes the <see cref="CalculusRules"/> class, populating the calculus rules.
        /// </summary>
        static CalculusRules()
        {
            DifferentiationRules = ImmutableList.Create<Rule>(
                // Rule: d/d(x) (f) = 0 if f does not contain x. This handles symbols (e.g., d/dx(y)=0) and numbers correctly.
                new Rule(
                    new Derivative(_wildF, _wildX),
                    new Number(0m),
                    (ImmutableDictionary<string, IExpression> bindings) =>
                    {
                        if (bindings.TryGetValue("f", out IExpression? matchedF) &&
                            bindings.TryGetValue("x", out IExpression? matchedX) && matchedX is Symbol targetVar)
                        {
                            // Assuming IExpression has a ContainsSymbol method.
                            return !matchedF.ContainsSymbol(targetVar);
                        }
                        return false;
                    }
                ),
                // Rule: d/dx (x) = 1
                new Rule(
                    new Derivative(_wildX, _wildX),
                    new Number(1m)
                ),
                // Sum rule: d/dx (f + g) = d/dx(f) + d/dx(g)
                new Rule(
                    new Derivative(new Add(_wildF, _wildY), _wildX),
                    new Add(new Derivative(_wildF, _wildX), new Derivative(_wildY, _wildX))
                ),
                // Product rule: d/dx (f * g) = g*d/dx(f) + f*d/dx(g)
                new Rule(
                    new Derivative(new Multiply(_wildF, _wildY), _wildX),
                    new Add(
                        new Multiply(_wildY, new Derivative(_wildF, _wildX)),
                        new Multiply(_wildF, new Derivative(_wildY, _wildX))
                    )
                ),
                // Power rule (with chain rule): d/dx(f^n) = n * f^(n-1) * d/dx(f), for n that is constant w.r.t x.
                new Rule(
                    new Derivative(new Power(_wildF, _wildN), _wildX),
                    new Multiply(
                        new Multiply(_wildN, new Power(_wildF, new Add(_wildN, new Number(-1m)))),
                        new Derivative(_wildF, _wildX)
                    ),
                    (ImmutableDictionary<string, IExpression> bindings) =>
                    {
                        if (bindings.TryGetValue("n", out var n) &&
                            bindings.TryGetValue("x", out var x) && x is Symbol var)
                        {
                            return !n.ContainsSymbol(var);
                        }
                        return false;
                    }
                )
            );

            IntegrationRules = ImmutableList.Create<Rule>(
                // Rule: Integral(0, x) = 0
                new Rule(
                    new Integral(new Number(0m), _wildX),
                    new Number(0m)
                ),
                // Rule: Integral(c, x) = c * x (for constant c, typically a Number)
                new Rule(
                    new Integral(_wildC, _wildX),
                    new Multiply(_wildC, _wildX),
                    (ImmutableDictionary<string, IExpression> bindings) => bindings.TryGetValue("c", out IExpression? mc) && mc is Number
                ),
                // Rule: Integral(c * f, x) = c * Integral(f, x) (Constant Multiple Rule)
                new Rule(
                    new Integral(new Multiply(_wildC, _wildF), _wildX),
                    new Multiply(_wildC, new Integral(_wildF, _wildX)),
                    (ImmutableDictionary<string, IExpression> bindings) => bindings.TryGetValue("c", out IExpression? matchedC) && matchedC is Number &&
                                  bindings.TryGetValue("x", out IExpression? matchedX) && matchedX is Symbol targetVar &&
                                  !matchedC.ContainsSymbol(targetVar) // Ensure constant does not contain integration variable
                ),
                // Rule: Integral(x^n, x) = x^(n+1) / (n+1), for n != -1
                new Rule(
                    new Integral(new Power(_wildX, _wildN), _wildX),
                    new Multiply(new Power(_wildX, new Add(_wildN, new Number(1m))),
                                 new Power(new Add(_wildN, new Number(1m)), new Number(-1m))),
                    (ImmutableDictionary<string, IExpression> bindings) => bindings.TryGetValue("n", out IExpression? mn) && mn is Number numN && numN.Value != -1m
                ),
                // Rule: Integral(cos(x), x) = sin(x)
                new Rule(
                    new Integral(new Function("cos", ImmutableList.Create<IExpression>(_wildX)), _wildX),
                    new Function("sin", ImmutableList.Create<IExpression>(_wildX))
                )
            );

            VectorCalculusRules = ImmutableList.Create<Rule>(
                // Updated: Grad(f, Vector(var1, var2, var3)) - original 3D rule, using new wildcards
                new Rule(
                    new Grad(_wildF,
                        new Vector(ImmutableList.Create<IExpression>(_wildVar1, _wildVar2, _wildVar3))
                    ),
                    new Vector(ImmutableList.Create<IExpression>(
                        new Derivative(_wildF, _wildVar1),
                        new Derivative(_wildF, _wildVar2),
                        new Derivative(_wildF, _wildVar3)
                    ))
                ),
                // NEW: Grad(f, Vector(var1, var2)) - 2D version
                new Rule(
                    new Grad(_wildF,
                        new Vector(ImmutableList.Create<IExpression>(_wildVar1, _wildVar2))
                    ),
                    new Vector(ImmutableList.Create<IExpression>(
                        new Derivative(_wildF, _wildVar1),
                        new Derivative(_wildF, _wildVar2)
                    ))
                ),
                // Updated: Div(Vector(exp1, exp2, exp3), Vector(var1, var2, var3)) - original 3D rule, using new wildcards
                new Rule(
                    new Div(
                        new Vector(ImmutableList.Create<IExpression>(_wildExp1, _wildExp2, _wildExp3)),
                        new Vector(ImmutableList.Create<IExpression>(_wildVar1, _wildVar2, _wildVar3))
                    ),
                    new Add(ImmutableList.Create<IExpression>(
                        new Derivative(_wildExp1, _wildVar1),
                        new Derivative(_wildExp2, _wildVar2),
                        new Derivative(_wildExp3, _wildVar3)
                    ))
                ),
                // NEW: Div(Vector(exp1, exp2), Vector(var1, var2)) - 2D version
                new Rule(
                    new Div(
                        new Vector(ImmutableList.Create<IExpression>(_wildExp1, _wildExp2)),
                        new Vector(ImmutableList.Create<IExpression>(_wildVar1, _wildVar2))
                    ),
                    new Add(ImmutableList.Create<IExpression>(
                        new Derivative(_wildExp1, _wildVar1),
                        new Derivative(_wildExp2, _wildVar2)
                    ))
                ),
                // Updated: Curl(Vector(exp1, exp2, exp3), Vector(var1, var2, var3)) - original 3D rule, using new wildcards
                new Rule(
                    new Curl(
                        new Vector(ImmutableList.Create<IExpression>(_wildExp1, _wildExp2, _wildExp3)),
                        new Vector(ImmutableList.Create<IExpression>(_wildVar1, _wildVar2, _wildVar3))
                    ),
                    new Vector(ImmutableList.Create<IExpression>(
                        new Add(new Derivative(_wildExp3, _wildVar2), new Multiply(new Number(-1m), new Derivative(_wildExp2, _wildVar3))),
                        new Add(new Derivative(_wildExp1, _wildVar3), new Multiply(new Number(-1m), new Derivative(_wildExp3, _wildVar1))),
                        new Add(new Derivative(_wildExp2, _wildVar1), new Multiply(new Number(-1m), new Derivative(_wildExp1, _wildVar2)))
                    ))
                )
            );
        }
    }
}