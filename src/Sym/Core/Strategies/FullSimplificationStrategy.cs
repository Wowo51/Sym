//Copyright Warren Harding 2025.
using System.Collections.Immutable;
using Sym.Core;
using Sym.Core.Rewriters;

namespace Sym.Core.Strategies
{
    /// <summary>
    /// An ISolverStrategy implementation that applies general simplification rules
    /// until the expression can no longer be simplified.
    /// </summary>
    public class FullSimplificationStrategy : ISolverStrategy
    {
        public SolveResult Solve(IExpression? problem, SolveContext context)
        {
            if (problem == null)
            {
                return SolveResult.Failure(null, "Problem expression cannot be null.");
            }

            IExpression currentExpression = problem;
            ImmutableList<IExpression>.Builder? traceBuilder = context.EnableTracing ? ImmutableList.CreateBuilder<IExpression>() : null;

            traceBuilder?.Add(currentExpression);

            // Iterate a maximum of 'MaxIterations' times, or until no further simplification occurs.
            for (int iterationCount = 0; iterationCount < context.MaxIterations; iterationCount++)
            {
                // Perform a single pass of simplification over the current expression.
                RewriterResult rewriteResult = Rewriter.Rewrite(currentExpression, context.Rules);
                
                // If the rewriter indicates no changes occurred in this pass,
                // then the expression is fully simplified.
                if (!rewriteResult.Changed)
                {
                    return SolveResult.Success(currentExpression, "Simplification completed successfully.", traceBuilder?.ToImmutable());
                }
                
                // Update the current expression with the result of this pass.
                currentExpression = rewriteResult.RewrittenExpression;
                
                // Add the new state to the trace if tracing is enabled.
                traceBuilder?.Add(currentExpression);
            }

            // If the loop completes, it means max iterations were reached before full simplification.
            return SolveResult.Failure(currentExpression, $"Max iterations ({context.MaxIterations}) reached before full simplification.", traceBuilder?.ToImmutable());
        }
    }
}