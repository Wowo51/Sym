//Copyright Warren Harding 2025.
using System.Collections.Immutable;
using Sym.Core;

namespace Sym.Core
{
    /// <summary>
    /// Encapsulates the outcome of a solver operation.
    /// </summary>
    public sealed class SolveResult
    {
        /// <summary>
        /// Indicates whether the goal was achieved.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The final expression after the solver runs.
        /// </summary>
        public IExpression? ResultExpression { get; init; }

        /// <summary>
        /// A human-readable message, such as "Solved successfully" or "Max iterations reached."
        /// </summary>
        public string Message { get; init; }

        /// <summary>
        /// An optional list of intermediate expressions if tracing was enabled.
        /// </summary>
        public ImmutableList<IExpression>? Trace { get; init; }

        /// <summary>
        /// Private constructor to enforce the use of static factory methods.
        /// </summary>
        /// <param name="success">Indicates if the operation was successful.</param>
        /// <param name="resultExpression">The resulting expression.</param>
        /// <param name="message">A descriptive message.</param>
        /// <param name="trace">Optional trace of intermediate expressions.</param>
        private SolveResult(bool success, IExpression? resultExpression, string message, ImmutableList<IExpression>? trace)
        {
            IsSuccess = success;
            ResultExpression = resultExpression;
            Message = message;
            Trace = trace;
        }

        /// <summary>
        /// Creates a successful <see cref="SolveResult"/> instance.
        /// </summary>
        /// <param name="resultExpression">The resulting expression.</param>
        /// <param name="message">A success message.</param>
        /// <param name="trace">Optional trace of intermediate expressions.</param>
        /// <returns>A successful SolveResult.</returns>
        public static SolveResult Success(IExpression? resultExpression, string message, ImmutableList<IExpression>? trace = null)
        {
            return new SolveResult(true, resultExpression, message, trace);
        }

        /// <summary>
        /// Creates a failed <see cref="SolveResult"/> instance.
        /// </summary>
        /// <param name="resultExpression">The resulting expression (can be the last known state or an error expression).</param>
        /// <param name="message">A failure message.</param>
        /// <param name="trace">Optional trace of intermediate expressions.</param>
        /// <returns>A failed SolveResult.</returns>
        public static SolveResult Failure(IExpression? resultExpression, string message, ImmutableList<IExpression>? trace = null)
        {
            return new SolveResult(false, resultExpression, message, trace);
        }
    }
}