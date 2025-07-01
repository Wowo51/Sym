//Copyright Warren Harding 2025.
using System.Collections.Immutable;
using System;

namespace Sym.Core
{
    /// <summary>
    /// Represents a single rewrite rule, consisting of a pattern, a replacement, and an optional condition.
    /// </summary>
    public sealed class Rule
    {
        /// <summary>
        /// The pattern expression to match against. Can contain Wild objects.
        /// </summary>
        public IExpression Pattern { get; init; }
        /// <summary>
        /// The replacement expression. Can contain Wild objects that will be substituted with bound values from the pattern.
        /// </summary>
        public IExpression Replacement { get; init; }
        /// <summary>
        /// An optional predicate that must evaluate to true for the rule to apply.
        /// It operates on the collected bindings.
        /// </summary>
        public Func<ImmutableDictionary<string, IExpression>, bool>? Condition { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        /// <param name="pattern">The pattern expression.</param>
        /// <param name="replacement">The replacement expression.</param>
        /// <param name="condition">An optional condition function for applying the rule.</param>
        public Rule(IExpression pattern, IExpression replacement, Func<ImmutableDictionary<string, IExpression>, bool>? condition = null)
        {
            Pattern = pattern;
            Replacement = replacement;
            Condition = condition;
        }

        /// <summary>
        /// Attempts to apply this rule to a given expression.
        /// </summary>
        /// <param name="expression">The expression to apply the rule to.</param>
        /// <returns>A new expression if the rule was applied successfully, otherwise the original expression.</returns>
        public IExpression Apply(IExpression expression)
        {
            MatchResult match = Rewriter.TryMatch(expression, Pattern);
            if (match.Success)
            {
                if (Condition is null || Condition(match.Bindings))
                {
                    return Rewriter.Substitute(Replacement, match.Bindings);
                }
            }
            return expression;
        }
    }
}