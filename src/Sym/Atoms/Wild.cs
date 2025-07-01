//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System;

namespace Sym.Atoms
{
    /// <summary>
    /// Represents a wildcard symbol used in pattern matching for rewriting.
    /// A wildcard can match any expression and optionally has a type constraint.
    /// </summary>
    public sealed class Wild : Atom
    {
        /// <summary>
        /// The name of the wildcard, used to bind the matched expression.
        /// </summary>
        public string Name { get; init; }
        /// <summary>
        /// The constraint that the matched expression must satisfy.
        /// </summary>
        public WildConstraint Constraint { get; init; }

        private readonly Shape _shape;
        public override Shape Shape { get { return _shape; } } // Wildcards do not have a fixed shape until bound

        /// <summary>
        /// Initializes a new instance of the <see cref="Wild"/> class.
        /// </summary>
        /// <param name="name">The name of the wildcard.</param>
        /// <param name="constraint">The type constraint for the wildcard (defaults to None).</param>
        public Wild(string name, WildConstraint constraint = WildConstraint.None)
        {
            Name = name;
            Constraint = constraint;
            _shape = Sym.Core.Shape.Wildcard; // A wildcard's shape is semantic 'Wildcard', not a concrete scalar.
        }

        /// <summary>
        /// Wildcards are canonical themselves, as they are not subject to reorganization like operations.
        /// </summary>
        /// <returns>This instance of the wildcard.</returns>
        public override IExpression Canonicalize()
        {
            return this;
        }

        /// <summary>
        /// Provides a string representation of the wildcard for display purposes.
        /// </summary>
        /// <returns>A string representing the wildcard.</returns>
        public override string ToDisplayString()
        {
            if (Constraint == WildConstraint.None)
            {
                return $"Wild('{Name}')";
            }
            return $"Wild('{Name}', {Constraint})";
        }

        /// <summary>
        /// Determines whether this wildcard expression is internally equal to another expression.
        /// Used for deep comparison after canonicalization.
        /// </summary>
        /// <param name="other">The other expression to compare with.</param>
        /// <returns>True if expressions are internally equal, false otherwise.</returns>
        public override bool InternalEquals(IExpression other)
        {
            if (other is not Wild otherWild)
            {
                return false;
            }
            return Name.Equals(otherWild.Name, StringComparison.Ordinal) && Constraint.Equals(otherWild.Constraint);
        }

        /// <summary>
        /// Returns the internal hash code for this wildcard expression.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Name);
            hash.Add(Constraint);
            return hash.ToHashCode();
        }
    }
}