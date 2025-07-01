//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    public sealed class Subtract : Operation
    {
        public IExpression LeftOperand { get; init; }
        public IExpression RightOperand { get; init; }

        public Subtract(IExpression left, IExpression right)
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
                return LeftOperand.Shape.CombineForElementWise(RightOperand.Shape);
            }
        }

        public override IExpression Canonicalize()
        {
            IExpression canonicalLeft = LeftOperand.Canonicalize();
            IExpression canonicalRight = RightOperand.Canonicalize();

            // Perform numerical evaluation if both operands are numbers
            if (canonicalLeft is Number leftNum && canonicalRight is Number rightNum)
            {
                return new Number(leftNum.Value - rightNum.Value);
            }

            // Convert A - B to A + (-1 * B) and canonicalize the Add operation.
            // This allows the Add operation's canonicalization to handle flattening, sorting, etc.
            Multiply negativeRight = new Multiply(new Number(-1m), canonicalRight);
            Add resultAdd = new Add(canonicalLeft, negativeRight.Canonicalize());

            return resultAdd.Canonicalize();
        }

        public override string ToDisplayString()
        {
            return $"({LeftOperand.ToDisplayString()} - {RightOperand.ToDisplayString()})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (other is not Subtract otherSubtract)
            {
                return false;
            }
            return LeftOperand.InternalEquals(otherSubtract.LeftOperand) &&
                   RightOperand.InternalEquals(otherSubtract.RightOperand);
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
            return new Subtract(newArgs[0], newArgs[1]);
        }
    }
}