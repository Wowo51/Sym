//Copyright Warren Harding 2025.
using System.Collections.Immutable;
using Sym.Core.Rewriters;
using Sym.Core.Strategies;
using Sym.Atoms;
using Sym.Operations;

namespace Sym.Core
{
    /// <summary>
    /// **`SymSolver`** (The Orchestrator):
    /// This static class will be the main entry entry point for users to interact with the new solver system.
    /// </summary>
    public static class SymSolver
    {
        /// <summary>
        /// Solves a given problem expression using a specified strategy and context.
        /// </summary>
        /// <param name="problem">The expression to be solved.</param>
        /// <param name="strategy">The solver strategy to apply (e.g., FullSimplificationStrategy, EquationSolverStrategy).</param>
        /// <param name="context">The solve context containing rules, max iterations, tracing settings, etc.</param>
        /// <returns>A SolveResult indicating success or failure, the resulting expression, and an optional trace.</returns>
        public static SolveResult Solve(IExpression? problem, ISolverStrategy? strategy, SolveContext? context)
        {
            if (strategy is null)
            {
                return SolveResult.Failure(problem, "ISolverStrategy cannot be null.");
            }
            if (context is null)
            {
                return SolveResult.Failure(problem, "SolveContext cannot be null.");
            }
            // The actual solving logic is delegated to the strategy
            return strategy.Solve(problem, context);
        }

        /// <summary>
        /// Convenience method to solve an equation for a target variable.
        /// </summary>
        public static SolveResult SolveEquation(IExpression? problem, Symbol targetVariable,
                                                ImmutableList<Rule> rules, int maxIterations = 100, bool enableTracing = false)
        {
            if (targetVariable is null)
            {
                return SolveResult.Failure(problem, "Target variable cannot be null for SolveEquation.");
            }
            SolveContext context = new SolveContext(targetVariable, rules, maxIterations, enableTracing, null);
            ISolverStrategy strategy = new EquationSolverStrategy();
            return Solve(problem, strategy, context);
        }

        /// <summary>
        /// Convenience method to simplify an expression.
        /// </summary>
        public static SolveResult Simplify(IExpression? problem, ImmutableList<Rule> rules, int maxIterations = 100, bool enableTracing = false)
        {
            SolveContext context = new SolveContext(null, rules, maxIterations, enableTracing, null);
            ISolverStrategy strategy = new FullSimplificationStrategy();
            return Solve(problem, strategy, context);
        }
    }
}