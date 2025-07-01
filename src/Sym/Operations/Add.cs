//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    public sealed class Add : Operation
    {
        public Add(ImmutableList<IExpression> arguments) : base(arguments)
        {
        }

        public override Shape Shape
        {
            get
            {
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
                        return Shape.Error; // Incompatible shapes found during aggregation
                    }
                }
                return currentCombinedShape;
            }
        }

        public override IExpression Canonicalize()
        {
            ImmutableList<IExpression> canonicalArgs = Arguments.Select(arg => arg.Canonicalize()).ToImmutableList();
            ImmutableList<IExpression> flattenedArgs = ExpressionHelpers.FlattenArguments<Add>(canonicalArgs);
            
            System.Decimal numericSum = 0m;
            ImmutableList<IExpression>.Builder nonNumericTermsBuilder = ImmutableList.CreateBuilder<IExpression>();

            foreach (IExpression arg in flattenedArgs)
            {
                if (arg is Number num)
                {
                    numericSum += num.Value;
                }
                else
                {
                    nonNumericTermsBuilder.Add(arg);
                }
            }

            ImmutableList<IExpression> nonNumericTerms = nonNumericTermsBuilder.ToImmutable();

            // If all arguments were numeric, return a single Number.
            if (nonNumericTerms.IsEmpty)
            {
                return new Number(numericSum);
            }
            
            // If there are non-numeric terms, and the numeric sum is not zero, add it.
            // If numericSum is 0 and there are other terms, Number(0) is redundant.
            if (numericSum != 0m)
            {
                nonNumericTermsBuilder.Add(new Number(numericSum));
            }

            ImmutableList<IExpression> sortedArgs = ExpressionHelpers.SortArguments(nonNumericTermsBuilder.ToImmutable());
            
            if (sortedArgs.Count == 1)
            {
                return sortedArgs[0];
            }
            
            // This case should ideally not be reached if initial nonNumericTerms.IsEmpty handled
            // and numericSum != 0m adds the Number. But as a defensive check for empty list after processing.
            if (sortedArgs.Count == 0)
            {
                return new Number(0m); // e.g. if Add() was called with no arguments
            }

            // Return a new Add operation with the canonicalized and sorted arguments.
            return new Add(sortedArgs);
        }

        public override string ToDisplayString()
        {
            return $"({string.Join(" + ", Arguments.Select(arg => arg.ToDisplayString()))})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is not Add otherAdd)
            {
                return false;
            }

            if (Arguments.Count != otherAdd.Arguments.Count)
            {
                return false;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].InternalEquals(otherAdd.Arguments[i]))
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
            return new Add(newArgs);
        }
    }
}