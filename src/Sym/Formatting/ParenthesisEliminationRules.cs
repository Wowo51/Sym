// Copyright Warren Harding 2025
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Sym.Formatting
{
    /// <summary>
    /// Provides rules and logic for eliminating unnecessary parentheses when converting an expression to a string.
    /// This class handles operator precedence to create a more readable infix notation,
    /// and recognizes common patterns like subtraction and division from their canonical forms.
    /// </summary>
    public static class ParenthesisEliminationRules
    {
        // Define operator precedence. Higher numbers mean higher precedence.
        private enum Precedence
        {
            None = 0, // Used for equation roots and other low-precedence contexts
            Add = 10,
            Multiply = 20,
            Power = 30,
            Atom = 100 // Atoms and functions have the highest precedence
        }

        private static Precedence GetPrecedence(IExpression expr)
        {
            return expr switch
            {
                Equality => Precedence.None, // 👈 ADD THIS LINE
                Add => Precedence.Add,
                Multiply => Precedence.Multiply,
                Power => Precedence.Power,
                Atom => Precedence.Atom,
                Function => Precedence.Atom,
                Vector => Precedence.Atom,
                Matrix => Precedence.Atom,
                _ => Precedence.None
            };
        }

        /// <summary>
        /// Converts an expression to its string representation, removing unnecessary parentheses.
        /// This method first canonicalizes the expression to ensure it is in a standard form.
        /// </summary>
        /// <param name="expression">The expression to format.</param>
        /// <returns>A string with optimized parentheses.</returns>
        public static string Format(IExpression expression)
        {
            // Start with a canonical form and the lowest precedence context.
            return ToInfixRecursive(expression.Canonicalize(), Precedence.None);
        }

        private static string ToInfixRecursive(IExpression expression, Precedence parentPrecedence)
        {
            var currentPrecedence = GetPrecedence(expression);

            string content = expression switch
            {
                // 👇 ADD THIS CASE
                Equality eqOp => $"{ToInfixRecursive(eqOp.LeftOperand, Precedence.None)} = {ToInfixRecursive(eqOp.RightOperand, Precedence.None)}",
                Add addOp => FormatAdd(addOp, currentPrecedence),
                Multiply mulOp => FormatMultiply(mulOp, currentPrecedence),
                Power powOp => FormatPower(powOp, currentPrecedence),
                Atom atom => atom.ToDisplayString(),
                Function func => func.ToDisplayString(),
                _ => expression.ToDisplayString() // Fallback for other operations
            };

            // Add parentheses if the current operator has lower precedence than its parent.
            if (currentPrecedence != Precedence.None && currentPrecedence < parentPrecedence)
            {
                return $"({content})";
            }
            return content;
        }

        private static string FormatAdd(Add addOp, Precedence currentPrecedence)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < addOp.Arguments.Count; i++)
            {
                var term = addOp.Arguments[i];
                var isSubtraction = false;

                // Check for the pattern `(-1 * term)` which represents subtraction.
                if (term is Multiply mulTerm && mulTerm.Arguments.Count == 2 && mulTerm.Arguments[0] is Number n && n.Value == -1m)
                {
                    term = mulTerm.Arguments[1];
                    isSubtraction = true;
                }

                if (i > 0)
                {
                    builder.Append(isSubtraction ? " - " : " + ");
                }
                else if (isSubtraction)
                {
                    builder.Append('-'); // Unary minus for the first term
                }

                // For a - (b+c), the sub-expression (b+c) needs parentheses.
                // We achieve this by passing a higher precedence to the recursive call for the right-hand side of a subtraction.
                builder.Append(ToInfixRecursive(term, isSubtraction ? Precedence.Add + 1 : currentPrecedence));
            }
            return builder.ToString();
        }

        private static string FormatMultiply(Multiply mulOp, Precedence currentPrecedence)
        {
            var numeratorTerms = new List<IExpression>();
            var denominatorTerms = new List<IExpression>();

            // Separate terms into numerator and denominator based on power
            foreach (var factor in mulOp.Arguments)
            {
                if (factor is Power p && p.Exponent is Number n && n.Value < 0)
                {
                    // If exponent is -1, the base is the denominator term.
                    // If exponent is -2, then base^2 is the denominator term.
                    IExpression denTerm = n.Value == -1m ? p.Base : new Power(p.Base, new Number(-n.Value));
                    denominatorTerms.Add(denTerm);
                }
                else
                {
                    numeratorTerms.Add(factor);
                }
            }

            // If there are no denominator terms, format as a standard multiplication.
            if (denominatorTerms.Count == 0)
            {
                return string.Join(" * ", mulOp.Arguments.Select(arg => ToInfixRecursive(arg, currentPrecedence)));
            }

            // Build numerator and denominator expressions
            IExpression numerator = numeratorTerms.Count switch
            {
                0 => new Number(1m),
                1 => numeratorTerms[0],
                _ => new Multiply(numeratorTerms.ToImmutableList())
            };

            IExpression denominator = denominatorTerms.Count switch
            {
                1 => denominatorTerms[0],
                _ => new Multiply(denominatorTerms.ToImmutableList())
            };

            // The denominator needs parentheses if it's an operation.
            string numString = ToInfixRecursive(numerator, currentPrecedence);
            string denString = ToInfixRecursive(denominator, Precedence.Multiply + 1);

            return $"{numString} / {denString}";
        }

        private static string FormatPower(Power powOp, Precedence currentPrecedence)
        {
            // Power is right-associative: a**b**c should be formatted unambiguously.
            // The base needs parentheses for lower-precedence ops, e.g., (a+b)**c.
            // The exponent also needs them for lower-precedence ops, e.g., a**(b+c).
            var baseStr = ToInfixRecursive(powOp.Base, currentPrecedence);
            var expStr = ToInfixRecursive(powOp.Exponent, currentPrecedence);
            return $"{baseStr} ** {expStr}";
        }
    }
}