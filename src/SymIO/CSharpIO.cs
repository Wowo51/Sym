// Copyright Warren Harding 2025
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using Sym.Calculus;
using Sym.Algebra;
using Sym.Core.Strategies;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System;
using SymIO.Parsing;
using Sym.Formatting;

namespace SymIO
{
    /// <summary>
    /// Provides a high-level interface for symbolic mathematics using C# expression strings.
    /// This class handles parsing of C# pseudo-language expressions, interaction with the Sym solver,
    /// and formatting of the results back into C# strings.
    /// </summary>
    public class CSharpIO
    {
        private readonly ExpressionParser _parser = new ExpressionParser();
        private static readonly ImmutableList<Rule> AllRules = CalculusRules.DifferentiationRules
            .AddRange(CalculusRules.IntegrationRules)
            .AddRange(CalculusRules.VectorCalculusRules)
            .AddRange(AlgebraicSimplificationRules.SimplificationRules);

        /// <summary>
        /// Simplifies a mathematical expression.
        /// </summary>
        /// <param name="expression">A string containing the C# mathematical expression to simplify.</param>
        /// <returns>A string representing the simplified expression.</returns>
        /// <example>
        /// CSharpIO sym = new CSharpIO();
        /// string result = sym.Simplify("x + x + y * 0"); // Returns "(2 * x)"
        /// </example>
        public string Simplify(string expression)
        {
            IExpression problem = _parser.Parse(expression);
            SolveContext context = new SolveContext(rules: AllRules, maxIterations: 100);
            FullSimplificationStrategy strategy = new FullSimplificationStrategy();

            SolveResult result = SymSolver.Solve(problem, strategy, context);

            if (result.ResultExpression != null)
            {
                return ParenthesisEliminationRules.Format(result.ResultExpression);
            }
           
            return "Error in simplification";
        }

        /// <summary>
        /// Solves an equation for a specified variable.
        /// </summary>
        /// <param name="equation">A string containing the equation (e.g., "2 * x + 5 = 15").</param>
        /// <param name="variable">The name of the variable to solve for (e.g., "x").</param>
        /// <returns>A string representing the solved equation (e.g., "x = 5").</returns>
        public string Solve(string equation, string variable)
        {
            IExpression problem = _parser.Parse(equation);
            Symbol targetVar = new Symbol(variable);

            SolveResult result = SymSolver.SolveEquation(problem, targetVar, AllRules);

            if (result.ResultExpression != null)
            {
                return ParenthesisEliminationRules.Format(result.ResultExpression);
            }

            return $"Error solving for {variable}";
        }

        /// <summary>
        /// Computes the derivative of an expression with respect to a variable.
        /// </summary>
        /// <param name="expression">The expression to differentiate (e.g., "Sin(x**2)").</param>
        /// <param name="variable">The variable to differentiate with respect to (e.g., "x").</param>
        /// <returns>A string representing the computed derivative.</returns>
        public string Differentiate(string expression, string variable)
        {
            return Simplify($"Derivative({expression}, {variable})");
        }

        /// <summary>
        /// Computes the indefinite integral of an expression with respect to a variable.
        /// </summary>
        /// <param name="expression">The expression to integrate (e.g., "x**2").</param>
        /// <param name="variable">The variable to integrate with respect to (e.g., "x").</param>
        /// <returns>A string representing the computed integral.</returns>
        public string Integrate(string expression, string variable)
        {
            return Simplify($"Integral({expression}, {variable})");
        }

        /// <summary>
        /// Computes the gradient of a scalar field.
        /// </summary>
        /// <param name="scalarExpression">The scalar field expression (e.g., "x*y*z").</param>
        /// <param name="vectorVariable">The vector of variables (e.g., "Vector(x, y, z)").</param>
        /// <returns>A string representing the gradient vector.</returns>
        public string Grad(string scalarExpression, string vectorVariable)
        {
            return Simplify($"Grad({scalarExpression}, {vectorVariable})");
        }

        /// <summary>
        /// Computes the divergence of a vector field.
        /// </summary>
        /// <param name="vectorExpression">The vector field expression (e.g., "Vector(x**2, y**2, z**2)").</param>
        /// <param name="vectorVariable">The vector of variables (e.g., "Vector(x, y, z)").</param>
        /// <returns>A string representing the scalar divergence.</returns>
        public string Div(string vectorExpression, string vectorVariable)
        {
            return Simplify($"Div({vectorExpression}, {vectorVariable})");
        }

        /// <summary>
        /// Computes the curl of a vector field.
        /// </summary>
        /// <param name="vectorExpression">The vector field expression (e.g., "Vector(-y, x, 0)").</param>
        /// <param name="vectorVariable">The vector of variables (e.g., "Vector(x, y, z)").</param>
        /// <returns>A string representing the curl vector.</returns>
        public string Curl(string vectorExpression, string vectorVariable)
        {
            return Simplify($"Curl({vectorExpression}, {vectorVariable})");
        }
    }
}