//Copyright Warren Harding 2025.
using Sym.Core;
using System;
using System.Globalization;

namespace Sym.Atoms
{
    public sealed class Number : Atom
    {
        public System.Decimal Value { get; init; }
        private readonly Shape _shape;
        public override Shape Shape { get { return _shape; } }

        public Number(System.Decimal value)
        {
            Value = value;
            _shape = Sym.Core.Shape.Scalar;
        }

        public override IExpression Canonicalize()
        {
            return this;
        }

        public override string ToDisplayString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
        
        public override bool InternalEquals(IExpression other)
        {
            if (other is not Number otherNumber)
            {
                return false;
            }
            return Value.Equals(otherNumber.Value);
        }

        public override int InternalGetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}