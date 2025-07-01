//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    public sealed class Multiply : Operation
    {
        public Multiply(ImmutableList<IExpression> arguments) : base(arguments)
        {
        }

        public override Shape Shape
        {
            get
            {
                // This Shape property now assumes an element-wise product if it remains a Multiply.
                // Canonicalization will transform to DotProduct or MatrixMultiply if appropriate.
                Shape currentCombinedShape = Shape.Scalar;
                foreach (IExpression arg in Arguments)
                {
                    if (!arg.Shape.IsValid)
                    {
                        return Shape.Error;
                    }

                    currentCombinedShape = currentCombinedShape.CombineForElementWise(arg.Shape);
                    if (!currentCombinedShape.IsValid)
                    {
                        return Shape.Error; // Incompatible shapes found during aggregation for element-wise product
                    }
                }
                return currentCombinedShape;
            }
        }

        public override IExpression Canonicalize()
        {
            ImmutableList<IExpression> canonicalArgs = Arguments.Select(arg => arg.Canonicalize()).ToImmutableList();
            ImmutableList<IExpression> flattenedArgs = ExpressionHelpers.FlattenArguments<Multiply>(canonicalArgs);
            
            System.Decimal numericProduct = 1m;
            ImmutableList<IExpression>.Builder nonNumericTermsBuilder = ImmutableList.CreateBuilder<IExpression>();

            foreach (IExpression arg in flattenedArgs)
            {
                if (arg is Number num)
                {
                    if (num.Value == 0m)
                    {
                        return new Number(0m); // Short-circuit: if any factor is 0, the product is 0.
                    }
                    numericProduct *= num.Value;
                }
                else
                {
                    nonNumericTermsBuilder.Add(arg);
                }
            }

            ImmutableList<IExpression> nonNumericTerms = nonNumericTermsBuilder.ToImmutable();

            // If all arguments were numeric (nonNumericTerms is empty), return a single Number.
            if (nonNumericTerms.IsEmpty)
            {
                return new Number(numericProduct);
            }

            // If there are non-numeric terms, and the numeric product is not 1, add it.
            // If numericProduct is 1 and there are other terms, Number(1) is redundant as a factor.
            if (numericProduct != 1m)
            {
                nonNumericTermsBuilder.Add(new Number(numericProduct));
            }

            ImmutableList<IExpression> collectedArgs = nonNumericTermsBuilder.ToImmutable();
            ImmutableList<IExpression> sortedArgs = ExpressionHelpers.SortArguments(collectedArgs);

            // If after canonicalization and combining, only one argument remains, return it.
            if (sortedArgs.Count == 1)
            {
                return sortedArgs[0];
            }

            // If sortedArgs is empty, it means no arguments, or all arguments simplified to factors of 1.
            // In multiplication, an empty product is typically 1 (multiplicative identity).
            if (sortedArgs.Count == 0)
            {
                return new Number(1m);
            }

            // Handle special products (DotProduct, MatrixMultiply) only if exactly two sorted arguments remain.
            if (sortedArgs.Count == 2)
            {
                IExpression operand1 = sortedArgs[0];
                IExpression operand2 = sortedArgs[1];

                // Attempt to promote to DotProduct: Vector . Vector or (1xN)Matrix . (Nx1)Matrix
                bool isVectorDotProduct = operand1.Shape.IsVector && operand2.Shape.IsVector && operand1.Shape.Dimensions[0] == operand2.Shape.Dimensions[0];
                bool isMatrixDotProductEquivalent = operand1.Shape.IsMatrix && operand2.Shape.IsMatrix &&
                                                    operand1.Shape.Dimensions.Length == 2 && operand2.Shape.Dimensions.Length == 2 &&
                                                    operand1.Shape.Dimensions[0] == 1 && operand1.Shape.Dimensions[1] == operand2.Shape.Dimensions[0] &&
                                                    operand2.Shape.Dimensions[1] == 1;

                if (isVectorDotProduct || isMatrixDotProductEquivalent)
                {
                    DotProduct result = new DotProduct(operand1, operand2);
                    if (!result.Shape.Equals(Shape.Error)) // Ensure the constructed DotProduct's shape is valid
                    {
                        return result.Canonicalize(); // Recursively canonicalize the new DotProduct
                    }
                }
                
                // Attempt to promote to MatrixMultiply: Matrix * Matrix OR Matrix * Vector OR Vector * Matrix
                bool isMatrixMatrixMultiply = operand1.Shape.IsMatrix && operand2.Shape.IsMatrix;
                bool isMatrixVectorMultiply = operand1.Shape.IsMatrix && operand2.Shape.IsVector;
                bool isVectorMatrixMultiply = operand1.Shape.IsVector && operand2.Shape.IsMatrix;

                if (isMatrixMatrixMultiply || isMatrixVectorMultiply || isVectorMatrixMultiply)
                {
                    MatrixMultiply result = new MatrixMultiply(operand1, operand2);
                    if (!result.Shape.Equals(Shape.Error)) // Ensure the constructed MatrixMultiply's shape is valid
                    {
                        return result.Canonicalize(); // Recursively canonicalize the new MatrixMultiply
                    }
                }
            }
            
            // If no special product applies, or if more than two arguments,
            // return a new Multiply operation with the sorted arguments.
            return new Multiply(sortedArgs);
        }

        public override string ToDisplayString()
        {
            return $"({string.Join(" * ", Arguments.Select(arg => arg.ToDisplayString()))})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is not Multiply otherMultiply)
            {
                return false;
            }

            if (Arguments.Count != otherMultiply.Arguments.Count)
            {
                return false;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].InternalEquals(otherMultiply.Arguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            foreach (IExpression arg in Arguments)
            {
                hash.Add(arg);
            }

            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Multiply(newArgs);
        }
    }
}