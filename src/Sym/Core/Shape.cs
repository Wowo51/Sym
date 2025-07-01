//Copyright Warren Harding 2025.
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Sym.Core
{
    public sealed record Shape : IEquatable<Shape>
    {
        public static readonly Shape Scalar = new Shape(ImmutableArray<int>.Empty, true, false);
        public static readonly Shape Error = new Shape(ImmutableArray<int>.Empty, false, false);
        public static readonly Shape Wildcard = new Shape(ImmutableArray<int>.Empty, true, true); // Represents a wildcard shape

        public ImmutableArray<int> Dimensions { get; init; }
        public bool IsValid { get; init; }
        public bool IsWildcardShape { get; init; }

        public Shape(ImmutableArray<int> dimensions, bool isValid = true) : this(dimensions, isValid, false) { }

        private Shape(ImmutableArray<int> dimensions, bool isValid, bool isWildcardShape)
        {
            Dimensions = dimensions;
            IsValid = isValid;
            IsWildcardShape = isWildcardShape;
        }

        public bool IsScalar => Dimensions.IsEmpty && IsValid && !IsWildcardShape;
        public bool IsVector => Dimensions.Length == 1 && IsValid && !IsWildcardShape;
        public bool IsMatrix => Dimensions.Length == 2 && IsValid && !IsWildcardShape;
        public bool IsTensor => Dimensions.Length > 2 && IsValid && !IsWildcardShape;

        /// <summary>
        /// Checks if two shapes are compatible for element-wise operations (e.g., addition, element-wise multiplication).
        /// Compatibility means:
        /// 1. Both shapes are valid and not wildcard shapes.
        /// 2. If either is scalar, they are compatible.
        /// 3. If both are non-scalar, their dimensions must be identical.
        /// </summary>
        /// <param name="other">The other shape to compare with.</param>
        /// <returns>True if compatible, false otherwise.</returns>
        public bool AreDimensionsCompatibleForElementWise(Shape other)
        {
            if (!this.IsValid || this.IsWildcardShape || !other.IsValid || other.IsWildcardShape)
            {
                return false;
            }
            if (this.IsScalar || other.IsScalar)
            {
                return true;
            }
            return this.Dimensions.SequenceEqual(other.Dimensions);
        }

        /// <summary>
        /// Attempts to combine two shapes for an element-wise operation.
        /// Returns the resulting shape if compatible, otherwise returns <see cref="Shape.Error"/>.
        /// </summary>
        /// <param name="other">The other shape to combine with.</param>
        /// <returns>The combined shape or <see cref="Shape.Error"/>.</returns>
        public Shape CombineForElementWise(Shape other)
        {
            if (!this.AreDimensionsCompatibleForElementWise(other))
            {
                return Shape.Error;
            }

            if (this.IsScalar)
            {
                return other;
            }
            if (other.IsScalar)
            {
                return this;
            }
            return this; // Both non-scalar and identical dimensions, so return either
        }

        public string ToDisplayString()
        {
            if (IsWildcardShape)
            {
                return "WildcardShape";
            }
            if (!IsValid)
            {
                return "ErrorShape";
            }
            if (IsScalar)
            {
                return "()";
            }
            return $"({(string.Join(", ", Dimensions.Select(d => $"{d}")))})";
        }

        // Explicitly implement IEquatable<Shape> and override Equals/GetHashCode
        public bool Equals(Shape? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (other is null)
            {
                return false;
            }
            // Include IsWildcardShape in comparison
            return IsValid.Equals(other.IsValid) && IsWildcardShape.Equals(other.IsWildcardShape) && Dimensions.SequenceEqual(other.Dimensions);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(IsValid);
            hash.Add(IsWildcardShape); // Include IsWildcardShape in hash calculation
            foreach (int dim in Dimensions)
            {
                hash.Add(dim);
            }
            return hash.ToHashCode();
        }
    }
}