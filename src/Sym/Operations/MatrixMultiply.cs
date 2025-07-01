//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    /// <summary>
    /// Represents a matrix multiplication operation.
    /// Supports matrix-matrix, matrix-vector, and vector-matrix products.
    /// </summary>
    public sealed class MatrixMultiply : Operation
    {
        public IExpression LeftOperand { get; init; }
        public IExpression RightOperand { get; init; }

        public MatrixMultiply(IExpression left, IExpression right)
            : base(ImmutableList.Create(left, right))
        {
            LeftOperand = left;
            RightOperand = right;
        }

        public override Shape Shape
        {
            get
            {
                if (!LeftOperand.Shape.IsValid || !RightOperand.Shape.IsValid)
                {
                    return Shape.Error;
                }

                Shape leftShape = LeftOperand.Shape;
                Shape rightShape = RightOperand.Shape;

                // Scalar * Any: Scalar multiplication, shape is 'Any'
                if (leftShape.IsScalar)
                {
                    return rightShape;
                }
                if (rightShape.IsScalar)
                {
                    return leftShape;
                }

                // Matrix-Matrix Multiplication (M,N) * (N,P) = (M,P)
                if (leftShape.IsMatrix && rightShape.IsMatrix)
                {
                    if (leftShape.Dimensions[1] == rightShape.Dimensions[0])
                    {
                        return new Shape(ImmutableArray.Create(leftShape.Dimensions[0], rightShape.Dimensions[1]));
                    }
                }
                // Matrix-Vector Multiplication (M,N) * (N,) = (M,)
                else if (leftShape.IsMatrix && rightShape.IsVector)
                {
                    if (leftShape.Dimensions[1] == rightShape.Dimensions[0])
                    {
                        return new Shape(ImmutableArray.Create(leftShape.Dimensions[0]));
                    }
                }
                // Vector-Matrix Multiplication (N,) * (N,P) is typically not defined or requires (1,N) * (N,P)
                // Assuming (1,N) for vector-matrix product in this case:
                // (1,N) * (N,P) = (1,P)
                else if (leftShape.IsVector && rightShape.IsMatrix)
                {
                    if (leftShape.Dimensions[0] == rightShape.Dimensions[0])
                    {
                         return new Shape(ImmutableArray.Create(rightShape.Dimensions[1])); // Result is 1xP, so (P,) for vector
                    }
                }

                return Shape.Error; // Invalid shapes for matrix multiplication
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

            if (!ReferenceEquals(canonicalLeft, LeftOperand) || !ReferenceEquals(canonicalRight, RightOperand))
            {
                return new MatrixMultiply(canonicalLeft, canonicalRight);
            }
            return this;
        }

        public override string ToDisplayString()
        {
            return $"MatrixMultiply({LeftOperand.ToDisplayString()}, {RightOperand.ToDisplayString()})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (other is not MatrixMultiply otherMatrixMultiply)
            {
                return false;
            }
            return LeftOperand.InternalEquals(otherMatrixMultiply.LeftOperand) &&
                   RightOperand.InternalEquals(otherMatrixMultiply.RightOperand);
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
            // See comments in DotProduct.cs for discussion about this scenario.
            return new MatrixMultiply(newArgs[0], newArgs[1]);
        }
    }
}