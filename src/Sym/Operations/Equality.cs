//Copyright Warren Harding 2025.
using Sym.Core;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    /// <summary>
    /// Represents a mathematical equality, typically used to define an equation.
    /// It has two operands: LeftOperand and RightOperand.
    /// </summary>
    public sealed class Equality : Operation
    {
        public IExpression LeftOperand { get; init; }
        public IExpression RightOperand { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Equality"/> class.
        /// </summary>
        /// <param name="left">The expression on the left side of the equality.</param>
        /// <param name="right">The expression on the right side of the equality.</param>
        public Equality(IExpression left, IExpression right)
            : base(ImmutableList.Create(left, right))
        {
            LeftOperand = left;
            RightOperand = right;
        }

        public override Shape Shape => Shape.Scalar;

        /// <summary>
        /// Canonicalizes the equality by canonicalizing its left and right operands.
        /// This method explicitly does NOT solve the equation.
        /// </summary>
        /// <returns>A canonicalized Equality expression.</returns>
        public override IExpression Canonicalize()
        {
            IExpression canonicalLeft = LeftOperand.Canonicalize();
            IExpression canonicalRight = RightOperand.Canonicalize();

            if (!ReferenceEquals(canonicalLeft, LeftOperand) || !ReferenceEquals(canonicalRight, RightOperand))
            {
                return new Equality(canonicalLeft, canonicalRight);
            }
            return this;
        }

        /// <summary>
        /// Provides a string representation of the equality.
        /// </summary>
        /// <returns>A string representing the equality in the format "(LeftOperand = RightOperand)".</returns>
        public override string ToDisplayString()
        {
            return $"({LeftOperand.ToDisplayString()} = {RightOperand.ToDisplayString()})";
        }

        /// <summary>
        /// Determines whether this equality expression is internally equal to another expression.
        /// </summary>
        /// <param name="other">The other expression to compare with.</param>
        /// <returns>True if expressions are internally equal, false otherwise.</returns>
        public override bool InternalEquals(IExpression other)
        {
            if (other is not Equality otherEquality)
            {
                return false;
            }
            return LeftOperand.InternalEquals(otherEquality.LeftOperand) &&
                   RightOperand.InternalEquals(otherEquality.RightOperand);
        }

        /// <summary>
        /// Returns the internal hash code for this equality expression.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(LeftOperand);
            hash.Add(RightOperand);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Creates a new instance of the Equality operation with the given new arguments.
        /// This is used internally during rewriting.
        /// </summary>
        /// <param name="newArgs">The new immutable list of arguments.</param>
        /// <returns>A new instance of the Equality type with the updated arguments.</returns>
        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            // Equality is a binary operation, so newArgs should always contain two elements.
            return new Equality(newArgs[0], newArgs[1]);
        }
    }
}