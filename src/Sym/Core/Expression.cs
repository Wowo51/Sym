//Copyright Warren Harding 2025.
using System.Linq;
using System.Collections.Immutable;

namespace Sym.Core
{
    public abstract class Expression : IExpression
    {
        public abstract Shape Shape { get; }

        public abstract IExpression Canonicalize();
        public abstract bool IsAtom { get; }
        public abstract bool IsOperation { get; }

        public abstract string ToDisplayString();

        // Make Equals method sealed and implement it universally
        public override sealed bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not IExpression otherExpression)
            {
                return false;
            }

            // For comparison, always canonicalize both expressions FIRST
            // then compare their internal, canonical forms.
            IExpression thisCanonical = this.Canonicalize();
            IExpression otherCanonical = otherExpression.Canonicalize();

            // Now compare the canonical forms using InternalEquals
            return thisCanonical.InternalEquals(otherCanonical);
        }

        // Make GetHashCode sealed and implement it universally
        public override sealed int GetHashCode()
        {
            // The hash code must be based on the canonical form of the expression.
            // This ensures that two expressions that canonicalize to the same form
            // will have the same hash code (consistent with Equals).
            return this.Canonicalize().InternalGetHashCode();
        }

        // InternalEquals compares the internal state of the expression (useful after canonicalization)
        public abstract bool InternalEquals(IExpression other);
        // InternalGetHashCode computes a hash based on the internal state (useful after canonicalization)
        public abstract int InternalGetHashCode();
    }
}