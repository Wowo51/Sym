//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    /// <summary>
    /// Represents a dot product operation. Typically applied to two vectors.
    /// The result is a scalar.
    /// </summary>
    public sealed class DotProduct : Operation
    {
        public IExpression LeftOperand { get; init; }
        public IExpression RightOperand { get; init; }

        public DotProduct(IExpression left, IExpression right)
            : base(ImmutableList.Create(left, right))
        {
            LeftOperand = left;
            RightOperand = right;
        }

        public override Shape Shape
        {
            get
            {
                // Dot product of two vectors (N,) * (N,) results in a Scalar.
                // Or a (1, N) matrix * (N, 1) matrix also results in a Scalar.
                // For now, only handle vector-vector dot product or 1xN * Nx1 matrix product.
                if (!LeftOperand.Shape.IsValid || !RightOperand.Shape.IsValid)
                {
                    return Shape.Error;
                }

                if (LeftOperand.Shape.IsVector && RightOperand.Shape.IsVector)
                {
                    if (LeftOperand.Shape.Dimensions[0] == RightOperand.Shape.Dimensions[0])
                    {
                        return Shape.Scalar;
                    }
                }
                // Handle 1xN matrix * Nx1 matrix (dot product equivalent)
                else if (LeftOperand.Shape.IsMatrix && RightOperand.Shape.IsMatrix &&
                         LeftOperand.Shape.Dimensions.Length == 2 && RightOperand.Shape.Dimensions.Length == 2 &&
                         LeftOperand.Shape.Dimensions[0] == 1 && LeftOperand.Shape.Dimensions[1] == RightOperand.Shape.Dimensions[0] &&
                         RightOperand.Shape.Dimensions[1] == 1)
                {
                    return Shape.Scalar;
                }
                
                return Shape.Error;
            }
        }

        public override IExpression Canonicalize()
        {
            IExpression canonicalLeft = LeftOperand.Canonicalize();
            IExpression canonicalRight = RightOperand.Canonicalize();

            // Perform numerical evaluation if both operands are numbers
            if (canonicalLeft is Number leftNum && canonicalRight is Number rightNum)
            {
                return new Number(leftNum.Value * rightNum.Value); // Standard scalar multiplication
            }

            // If no change to operands, return this instance for efficiency and identity preservation
            if (ReferenceEquals(canonicalLeft, LeftOperand) && ReferenceEquals(canonicalRight, RightOperand))
            {
                return this;
            }

            // Otherwise, return a new DotProduct instance with the canonicalized operands
            return new DotProduct(canonicalLeft, canonicalRight);
        }

        public override string ToDisplayString()
        {
            return $"DotProduct({LeftOperand.ToDisplayString()}, {RightOperand.ToDisplayString()})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (other is not DotProduct otherDotProduct)
            {
                return false;
            }
            return LeftOperand.InternalEquals(otherDotProduct.LeftOperand) &&
                   RightOperand.InternalEquals(otherDotProduct.RightOperand);
        }

        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(LeftOperand);
            hash.Add(RightOperand);
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            // WithArguments is called internally by the Rewriter after collecting arguments.
            // It should always provide the correct count for a binary operation.
            return new DotProduct(newArgs[0], newArgs[1]);
        }
    }
}