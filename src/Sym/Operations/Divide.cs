//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    public sealed class Divide : Operation
    {
        public IExpression Numerator { get; init; }
        public IExpression Denominator { get; init; }

        public Divide(IExpression numerator, IExpression denominator)
            : base(ImmutableList.Create(numerator, denominator))
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public override Shape Shape
        {
            get
            {
                if (!Numerator.Shape.IsValid || !Denominator.Shape.IsValid)
                {
                    return Shape.Error;
                }
                return Numerator.Shape.CombineForElementWise(Denominator.Shape);
            }
        }

        public override IExpression Canonicalize()
        {
            IExpression canonicalNumerator = Numerator.Canonicalize();
            IExpression canonicalDenominator = Denominator.Canonicalize();

            // Handle division by zero
            if (canonicalDenominator is Number denNum && denNum.Value == 0m)
            {
                return this; // Return symbolic form, as it's undefined or leads to infinity
            }

            // Perform numerical evaluation if both operands are numbers
            if (canonicalNumerator is Number numVal && canonicalDenominator is Number denVal)
            {
                return new Number(numVal.Value / denVal.Value);
            }

            // Convert A / B to A * B^(-1) and canonicalize the Multiply operation.
            // This delegates to Multiply and Power for simplification rules.
            Power inverseDenominator = new Power(canonicalDenominator, new Number(-1m));
            Multiply resultMultiply = new Multiply(canonicalNumerator, inverseDenominator.Canonicalize());
            
            return resultMultiply.Canonicalize();
        }

        public override string ToDisplayString()
        {
            return $"({Numerator.ToDisplayString()} / {Denominator.ToDisplayString()})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (other is not Divide otherDivide)
            {
                return false;
            }
            return Numerator.InternalEquals(otherDivide.Numerator) &&
                   Denominator.InternalEquals(otherDivide.Denominator);
        }

        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(Numerator);
            hash.Add(Denominator);
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Divide(newArgs[0], newArgs[1]);
        }
    }
}