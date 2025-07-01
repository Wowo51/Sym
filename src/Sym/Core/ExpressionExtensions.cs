//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace Sym.Core
{
    /// <summary>
    /// Provides extension methods for IExpression, useful for analysis operations.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Determines if the expression or any of its sub-expressions contain the specified symbol.
        /// </summary>
        /// <param name="expression">The expression to search within.</param>
        /// <param name="targetSymbol">The symbol to search for.</param>
        /// <returns>True if the symbol is found, false otherwise.</returns>
        public static bool ContainsSymbol(this IExpression expression, Symbol targetSymbol)
        {
            // If the expression itself is the target symbol
            if (expression.InternalEquals(targetSymbol))
            {
                return true;
            }

            // If it's an operation, recursively check its arguments
            if (expression is Operation operation)
            {
                foreach (IExpression arg in operation.Arguments)
                {
                    if (arg.ContainsSymbol(targetSymbol))
                    {
                        return true;
                    }
                }
            }
            // For other Atoms (Number, Wild), they do not contain other symbols unless they are the symbol themselves (already checked).
            return false;
        }
    }
}