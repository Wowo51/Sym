//Copyright Warren Harding 2025.
using Sym.Core;

namespace Sym.Core.Rewriters
{
    /// <summary>
    /// Represents the result of a rewrite operation, indicating the rewritten expression
    /// and whether any change occurred.
    /// </summary>
    public sealed class RewriterResult
    {
        public IExpression RewrittenExpression { get; init; }
        public bool Changed { get; init; }

        public RewriterResult(IExpression rewrittenExpression, bool changed)
        {
            RewrittenExpression = rewrittenExpression;
            Changed = changed;
        }
    }
}