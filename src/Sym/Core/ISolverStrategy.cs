//Copyright Warren Harding 2025.
using Sym.Core;

namespace Sym.Core
{
    /// <summary>
    /// Defines the contract for all solver approaches within the Sym library.
    /// </summary>
    public interface ISolverStrategy
    {
        /// <summary>
        /// Solves a given problem expression using the provided context.
        /// </summary>
        /// <param name="problem">The expression to solve.</param>
        /// <param name="context">The context containing solver settings like rules, target variable, and tracing options.</param>
        /// <returns>A SolveResult indicating success/failure, the result expression, and a message.</returns>
        SolveResult Solve(IExpression? problem, SolveContext context);
    }
}