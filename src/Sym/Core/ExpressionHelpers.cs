//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Core
{
    public static class ExpressionHelpers
    {
        public static ImmutableList<IExpression> FlattenArguments<T>(ImmutableList<IExpression> arguments) where T : Operation
        {
            ImmutableList<IExpression>.Builder flattenedArgs = ImmutableList.CreateBuilder<IExpression>();
            foreach (IExpression arg in arguments)
            {
                if (arg is T nestedOp)
                {
                    flattenedArgs.AddRange(FlattenArguments<T>(nestedOp.Arguments));
                }
                else
                {
                    flattenedArgs.Add(arg);
                }
            }
            return flattenedArgs.ToImmutable();
        }

        public static ImmutableList<IExpression> SortArguments(ImmutableList<IExpression> arguments)
        {
            return arguments
                .OrderBy(arg => arg, new ExpressionComparer())
                .ToImmutableList();
        }

        private sealed class ExpressionComparer : System.Collections.Generic.IComparer<IExpression>
        {
            public int Compare(IExpression? x, IExpression? y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x is null) return -1;
                if (y is null) return 1;

                IExpression canonicalX = x.Canonicalize();
                IExpression canonicalY = y.Canonicalize();

                // Define precedence: Number < Symbol < Wild < other Atom types < Operation types
                int typePrecedenceX = GetTypePrecedence(canonicalX);
                int typePrecedenceY = GetTypePrecedence(canonicalY);

                if (typePrecedenceX != typePrecedenceY)
                {
                    return typePrecedenceX.CompareTo(typePrecedenceY);
                }

                // If same type precedence, compare based on specific type rules
                if (canonicalX is Number numX && canonicalY is Number numY)
                {
                    return numX.Value.CompareTo(numY.Value);
                }
                else if (canonicalX is Symbol symX && canonicalY is Symbol symY)
                {
                    int nameComparison = string.CompareOrdinal(symX.Name, symY.Name);
                    if (nameComparison != 0) return nameComparison;
                    return symX.Shape.ToDisplayString().CompareTo(symY.Shape.ToDisplayString());
                }
                else if (canonicalX is Wild wildX && canonicalY is Wild wildY)
                {
                    int nameComparison = string.CompareOrdinal(wildX.Name, wildY.Name);
                    if (nameComparison != 0) return nameComparison;
                    return wildX.Constraint.CompareTo(wildY.Constraint);
                }
                else if (canonicalX is Atom && canonicalY is Atom) // Other Atoms by type name
                {
                    return string.CompareOrdinal(canonicalX.GetType().Name, canonicalY.GetType().Name);
                }
                else if (canonicalX is Operation opX && canonicalY is Operation opY)
                {
                    int typeNameComparison = string.CompareOrdinal(opX.GetType().Name, opY.GetType().Name);
                    if (typeNameComparison != 0) return typeNameComparison;

                    for (int i = 0; i < opX.Arguments.Count && i < opY.Arguments.Count; i++)
                    {
                        int argComparison = Compare(opX.Arguments[i], opY.Arguments[i]); // Recursive call
                        if (argComparison != 0) return argComparison;
                    }
                    return opX.Arguments.Count.CompareTo(opY.Arguments.Count);
                }
                
                // Fallback for types not explicitly handled above, usually by display string or type name
                return string.CompareOrdinal(canonicalX.ToDisplayString(), canonicalY.ToDisplayString());
            }

            private static int GetTypePrecedence(IExpression expr)
            {
                if (expr is Number) return 0;
                if (expr is Symbol) return 1;
                if (expr is Wild) return 2;
                if (expr is Atom) return 3; // Any other atom
                if (expr is Operation) return 4;
                return 5; // Default for unexpected types
            }
        }
    }
}