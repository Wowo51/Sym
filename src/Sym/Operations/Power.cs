//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    public sealed class Power : Operation
    {
        public IExpression Base { get; init; }
        public IExpression Exponent { get; init; }

        public Power(IExpression @base, IExpression exponent) : base(ImmutableList.Create(@base, exponent))
        {
            Base = @base;
            Exponent = exponent;
        }

        public override Shape Shape
        {
            get
            {
                // The shape of A**B:
                // 1. If A is Scalar, Result is Scalar.
                // 2. If B is Scalar, Result shape is Base.Shape (element-wise scalar exponentiation).
                // 3. If B is not Scalar and B's shape doesn't match A's shape, it's an error for now.
                //    (e.g., matrix exponentiation where Exp is also a matrix, not typically element-wise,
                //     or other complex tensor power rules are not yet implemented beyond scalar exponentiation).

                if (!Base.Shape.IsValid || !Exponent.Shape.IsValid)
                {
                    return Shape.Error;
                }

                if (Base.Shape.IsScalar)
                {
                    return Shape.Scalar;
                }
                
                if (Exponent.Shape.IsScalar)
                {
                    return Base.Shape; // Element-wise scalar exponentiation
                }

                // If exponent is not scalar, and shapes don't match, or it's a tensor power rule not implemented
                if (!Base.Shape.Dimensions.SequenceEqual(Exponent.Shape.Dimensions))
                {
                    return Shape.Error; // Incompatible shapes for non-scalar exponentiation
                }

                // If shapes are identical and non-scalar (e.g., element-wise vector power by vector exponent)
                return Base.Shape;
            }
        }

        public override IExpression Canonicalize()
        {
            IExpression canonicalBase = Base.Canonicalize();
            IExpression canonicalExponent = Exponent.Canonicalize();

            // Handle X^0 = 1 including 0^0 = 1 per hint
            if (canonicalExponent is Number expNum && expNum.Value == 0m)
            {
                return new Number(1m);
            }

            if (canonicalExponent is Number expNumOne && expNumOne.Value == 1m)
            {
                return canonicalBase;
            }

            if (canonicalBase is Number baseNumZero && baseNumZero.Value == 0m)
            {
                if (canonicalExponent is Number expNumPositive && expNumPositive.Value > 0m)
                {
                    // 0^positive = 0
                    return new Number(0m);
                }
                // Case 0^negative is undefined (division by zero), leave as Power for now.
            }

            if (canonicalBase is Number baseNumOne && baseNumOne.Value == 1m)
            {
                return new Number(1m);
            }
            
            if (canonicalBase is Power nestedPower)
            {
                // (a^b)^c = a^(b*c)
                IExpression newExponent = new Multiply(ImmutableList.Create(nestedPower.Exponent, canonicalExponent)).Canonicalize();
                if (newExponent is Number combinedExpNum && nestedPower.Base is Number nestedBaseNum) // Check nestedPower.Base as Number
                {
                    // Handle numerical evaluation of (number^number)^number
                    try
                    {
                        double doubleBase = (double)nestedBaseNum.Value;
                        double doubleExp = (double)combinedExpNum.Value;
                        double resDouble = System.Math.Pow(doubleBase, doubleExp);

                        if (double.IsNaN(resDouble) || double.IsInfinity(resDouble))
                        {
                            return new Power(canonicalBase, canonicalExponent); // Return original symbolic if numeric result is bad
                        }
                        
                        try
                        {
                            return new Number((System.Decimal)resDouble);
                        }
                        catch (OverflowException)
                        {
                            return new Power(canonicalBase, canonicalExponent); // Result too large for decimal.
                        }
                    }
                    catch (System.Exception) // Catch any other unexpected conversion/calc issues
                    {
                        return new Power(canonicalBase, canonicalExponent); // Return symbolic if exception occurs
                    }
                }
                return new Power(nestedPower.Base, newExponent).Canonicalize();
            }

            // Numerical evaluation if both base and exponent are numbers
            if (canonicalBase is Number baseVal && canonicalExponent is Number expVal)
            {
                // Handle 0^negative case: undefined (division by zero), return symbolic Power.
                // 0^0 already handled above to be 1.
                if (baseVal.Value == 0m && expVal.Value < 0m)
                {
                    return this; 
                }

                long integerExponent;
                if (expVal.Value == System.Math.Floor(expVal.Value) && expVal.Value >= long.MinValue && expVal.Value <= long.MaxValue)
                {
                    integerExponent = (long)expVal.Value;
                    if (integerExponent >= 0)
                    {
                        System.Decimal result = 1m;
                        System.Decimal currentBase = baseVal.Value;
                        for (long i = 0; i < integerExponent; i++)
                        {
                            // Check for potential overflow before multiplication
                            if (currentBase != 0m && System.Math.Abs(result) > System.Decimal.MaxValue / System.Math.Abs(currentBase) && currentBase != 1m && currentBase != -1m) return this; 
                            result *= currentBase;
                        }
                        return new Number(result);
                    }
                    else // Negative integer exponent: 1 / base^(-exp)
                    {
                        if (baseVal.Value == 0m)
                        {
                            return this; // Already handled 0^negative.
                        }
                        System.Decimal result = 1m;
                        System.Decimal currentBase = baseVal.Value;
                        for (long i = 0; i < -integerExponent; i++)
                        {
                            // Check for potential overflow
                            if (currentBase != 0m && System.Math.Abs(result) > System.Decimal.MaxValue / System.Math.Abs(currentBase) && currentBase != 1m && currentBase != -1m) return this;
                            result *= currentBase;
                        }
                        if (result == 0m)
                        {
                             // Should not happen unless currentBase was 0 originally (handled above).
                             return this; 
                        }
                        return new Number(1m / result);
                    }
                }
                else
                {
                    // Fallback to double if exponent is not integer for power calculation
                    // This might lose precision, but fulfills the numeric evaluation
                    // System.Math.Pow can handle NaN, Infinity for doubles, which Decimal can't.
                    // If result from Math.Pow is NaN/Infinity, we return original expression.
                    double doubleBase = (double)baseVal.Value;
                    double doubleExp = (double)expVal.Value;
                    double resDouble = System.Math.Pow(doubleBase, doubleExp);

                    if (double.IsNaN(resDouble) || double.IsInfinity(resDouble))
                    {
                        return this; // Cannot represent in decimal/symbolic for now
                    }
                    
                    try
                    {
                        return new Number((System.Decimal)resDouble);
                    }
                    catch (OverflowException)
                    {
                        return this; // Result too large for decimal.
                    }
                    catch (System.Exception) // Catch any other unexpected conversion/calc issues (e.g., negative base with non-integer exponent)
                    {
                        return this; // Return original symbolic if exception occurs
                    }
                }
            }


            if (!ReferenceEquals(canonicalBase, Base) || !ReferenceEquals(canonicalExponent, Exponent))
            {
                return new Power(canonicalBase, canonicalExponent);
            }

            return this;
        }

        public override string ToDisplayString()
        {
            return $"{Base.ToDisplayString()}**{Exponent.ToDisplayString()}";
        }

        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(Base);
            hash.Add(Exponent);
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Power(newArgs[0], newArgs[1]);
        }
    }
}