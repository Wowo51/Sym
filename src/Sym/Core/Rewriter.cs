//Copyright Warren Harding 2025.
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System; 
// Removed using System.Threading.Tasks; // For Parallel.For

namespace Sym.Core
{
    /// <summary>
    /// Provides functionality for rewriting symbolic expressions based on a set of rules.
    /// </summary>
    public static class Rewriter
    {
        /// <summary>
        /// Rewrites an expression by repeatedly applying a set of rules until no more rules can be applied.
        /// This method traverses the expression tree and applies rules both at the top level and to sub-expressions.
        /// </summary>
        /// <param name="expression">The expression to rewrite.</param>
        /// <param name="rules">The set of rules to apply.</param>
        /// <returns>The rewritten expression.</returns>
        public static IExpression Rewrite(IExpression expression, ImmutableList<Rule> rules)
        {
            IExpression currentExpression = expression;
            bool changedLocallyThisIteration;
            int maxIterations = 100; // Prevent infinite loops
            int iteration = 0;

            do
            {
                changedLocallyThisIteration = false; // Reset for each iteration
                // ApplyRulesRecursively now returns a tuple (newExpression, changedFlag)
                (IExpression newExpression, bool changedInSubtree) = ApplyRulesRecursively(currentExpression, rules);
                
                // If the top-level expression reference has changed, or if anything changed in a subtree
                if (!ReferenceEquals(newExpression, currentExpression) || changedInSubtree)
                {
                    changedLocallyThisIteration = true;
                }
                currentExpression = newExpression;
                iteration++;
            } while (changedLocallyThisIteration && iteration < maxIterations);
            
            return currentExpression;
        }

        /// <summary>
        /// Recursively applies rules to an expression and its sub-expressions.
        /// </summary>
        /// <param name="expression">The expression to apply rules to.</param>
        /// <param name="rules">The set of rules to apply.</param>
        /// <returns>A tuple containing the rewritten expression and a boolean indicating if any part of the expression (or its sub-expressions) was changed.</returns>
        private static (IExpression result, bool changed) ApplyRulesRecursively(IExpression expression, ImmutableList<Rule> rules)
        {
            // First, try to apply a rule directly to the current expression
            foreach (Rule rule in rules)
            {
                MatchResult match = TryMatch(expression, rule.Pattern);
                if (match.Success)
                {
                    if (rule.Condition is null || rule.Condition(match.Bindings))
                    {
                        return (Substitute(rule.Replacement, match.Bindings), true); // Rule successfully applied, so expression changed
                    }
                }
            }

            // If no rule applied directly, recursively apply to arguments/sub-expressions if it's an Operation
            if (expression is Operation operation)
            {
                IExpression[] newArgumentsArray = new IExpression[operation.Arguments.Count];
                bool anyArgumentsChanged = false;

                // Process arguments sequentially
                for (int i = 0; i < operation.Arguments.Count; i++)
                {
                    IExpression originalArg = operation.Arguments[i];
                    (IExpression newArg, bool argChanged) = ApplyRulesRecursively(originalArg, rules); // Recursive call
                    newArgumentsArray[i] = newArg; // Store the (potentially new) argument
                    if (argChanged)
                    {
                        anyArgumentsChanged = true;
                    }
                }

                if (anyArgumentsChanged)
                {
                    ImmutableList<IExpression> updatedArgs = newArgumentsArray.ToImmutableList();
                    // Reconstruct the operation using the WithArguments abstract method
                    return (((Operation)operation).WithArguments(updatedArgs), true);
                }
            }
            // If no rule applied and no arguments changed, return the original expression
            return (expression, false);
        }

        /// <summary>
        /// Attempts to match an expression against a pattern and collect bindings.
        /// This is the entry point for pattern matching.
        /// </summary>
        /// <param name="expression">The expression to match.</param>
        /// <param name="pattern">The pattern to match against.</param>
        /// <returns>A MatchResult indicating success and collected bindings.</returns>
        public static MatchResult TryMatch(IExpression expression, IExpression pattern)
        {
            ImmutableDictionary<string, IExpression>.Builder bindings = ImmutableDictionary.CreateBuilder<string, IExpression>();
            bool success = TryMatchRecursive(expression, pattern, bindings);
            return new MatchResult(success, bindings.ToImmutable());
        }

        /// <summary>
        /// Recursive helper for matching an expression against a pattern and collecting bindings.
        /// </summary>
        /// <param name="expression">The expression being matched.</param>
        /// <param name="pattern">The pattern being used for matching.</param>
        /// <param name="bindings">The mutable dictionary to store collected wildcard bindings.</param>
        /// <returns>True if the expression matches the pattern, false otherwise.</returns>
        private static bool TryMatchRecursive(IExpression expression, IExpression pattern, ImmutableDictionary<string, IExpression>.Builder bindings)
        {
            if (pattern is Wild wildPattern)
            {
                if (bindings.ContainsKey(wildPattern.Name))
                {
                    // If this wildcard name is already bound, the current expression must match the existing binding.
                    return bindings[wildPattern.Name].InternalEquals(expression);
                }
                else
                {
                    // Check constraints (Scalar, Constant) inherited from WildConstraint
                    if (wildPattern.Constraint == WildConstraint.Scalar && !expression.Shape.IsScalar)
                    {
                        return false;
                    }
                    if (wildPattern.Constraint == WildConstraint.Constant && expression is not Number)
                    {
                        // Future: Could be extended to check if an expression evaluates to a constant via simplification
                        return false;
                    }
                    
                    bindings.Add(wildPattern.Name, expression);
                    return true;
                }
            }

            if (expression.GetType() != pattern.GetType())
            {
                return false;
            }

            if (expression is Atom atomExpression && pattern is Atom atomPattern)
            {
                return atomExpression.InternalEquals(atomPattern);
            }
            else if (expression is Operation opExpression && pattern is Operation opPattern)
            {
                if (opExpression.Arguments.Count != opPattern.Arguments.Count)
                {
                    return false;
                }
                for (int i = 0; i < opExpression.Arguments.Count; i++)
                {
                    if (!TryMatchRecursive(opExpression.Arguments[i], opPattern.Arguments[i], bindings))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Substitutes wildcards in a replacement expression with their bound values.
        /// This creates a new expression based on the replacement pattern and the collected bindings.
        /// </summary>
        /// <param name="replacementExpression">The expression template (likely containing wildcards).</param>
        /// <param name="bindings">The dictionary of wildcard name to bound expression mappings.</param>
        /// <returns>A new expression with wildcards substituted, or the original if no wildcards are present or unbound.</returns>
        public static IExpression Substitute(IExpression replacementExpression, ImmutableDictionary<string, IExpression> bindings)
        {
            if (replacementExpression is Wild wild)
            {
                if (bindings.TryGetValue(wild.Name, out IExpression? boundExpression))
                {
                    return boundExpression;
                }
                return wild;
            }

            if (replacementExpression is Atom)
            {
                return replacementExpression;
            }

            if (replacementExpression is Operation operation)
            {
                ImmutableList<IExpression>.Builder newArgsBuilder = ImmutableList.CreateBuilder<IExpression>();
                foreach (IExpression arg in operation.Arguments)
                {
                    newArgsBuilder.Add(Substitute(arg, bindings)); // Recursively substitute in arguments
                }
                ImmutableList<IExpression> newArgs = newArgsBuilder.ToImmutable();

                // Call the virtual method WithArguments on the base operation class
                return operation.WithArguments(newArgs);
            }
            return replacementExpression; // Should not happen for valid IExpression types
        }
    }
}