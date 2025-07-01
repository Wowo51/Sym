//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace SymTest
{
    [TestClass]
    public sealed class PowerTests
    {
        [TestMethod]
        public void Power_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol x = new Symbol("x");
            Number two = new Number(2m);
            Power powerOp = new Power(x, two);

            Assert.AreEqual((IExpression)x, powerOp.Base);
            Assert.AreEqual((IExpression)two, powerOp.Exponent);
            Assert.AreEqual(2, powerOp.Arguments.Count);
            Assert.IsTrue(powerOp.IsOperation);
            Assert.IsFalse(powerOp.IsAtom);
        }

        [TestMethod]
        public void Power_Shape_ReturnsCorrectScalarShape()
        {
            Number baseNum = new Number(5m);
            Number expNum = new Number(2m);
            Power powerScalarScalar = new Power(baseNum, expNum);
            Assert.AreEqual(Shape.Scalar, powerScalarScalar.Shape);
        }

        [TestMethod]
        public void Power_Shape_ReturnsCorrectBaseShapeForScalarExponent()
        {
            Vector vecBase = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Number expNum = new Number(2m);
            Power powerVecScalar = new Power(vecBase, expNum);
            Assert.AreEqual(vecBase.Shape, powerVecScalar.Shape); // (2,)
        }

        [TestMethod]
        public void Power_Shape_ReturnsCorrectShapeForCompatibleNonScalarExponent()
        {
            Vector vecBase = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Vector vecExp = new Vector(ImmutableList.Create<IExpression>(new Number(2m), new Number(3m)));
            Power powerVecVec = new Power(vecBase, vecExp);
            Assert.AreEqual(vecBase.Shape, powerVecVec.Shape); // (2,)
        }

        [TestMethod]
        public void Power_Shape_ReturnsErrorForIncompatibleShapes()
        {
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Vector vec3 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b"), new Symbol("c")));
            Power powerIncompatible = new Power(vec2, vec3);
            Assert.AreEqual(Shape.Error, powerIncompatible.Shape);

            Matrix matrix = new Matrix(ImmutableArray.Create(2,2),ImmutableList.Create<IExpression>(new Number(1m),new Number(2m),new Number(3m),new Number(4m)));
            Power powerMatrixVector = new Power(matrix, vec2);
            Assert.AreEqual(Shape.Error, powerMatrixVector.Shape);
        }

        [TestMethod]
        public void Power_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol x = new Symbol("x");
            Number two = new Number(2m);
            Power powerOp = new Power(x, two);
            Assert.AreEqual("x**2", powerOp.ToDisplayString());

            Add baseAdd = new Add(ImmutableList.Create<IExpression>(x, new Symbol("y")));
            Multiply expMul = new Multiply(ImmutableList.Create<IExpression>(new Number(3m), new Symbol("z")));
            Power complicatedPower = new Power(baseAdd, expMul);
            Assert.AreEqual("(x + y)**(3 * z)", complicatedPower.ToDisplayString());
        }

        [TestMethod]
        public void Power_Canonicalize_HandlesZeroExponent()
        {
            Symbol x = new Symbol("x");
            Number zero = new Number(0m);
            Power input = new Power(x, zero);
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)new Number(1m), canonical); // x^0 = 1
        }

        [TestMethod]
        public void Power_Canonicalize_HandlesOneExponent()
        {
            Symbol x = new Symbol("x");
            Number one = new Number(1m);
            Power input = new Power(x, one);
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)x, canonical); // x^1 = x
        }

        [TestMethod]
        public void Power_Canonicalize_HandlesZeroBasePositiveExponent()
        {
            Number zero = new Number(0m);
            Number two = new Number(2m);
            Power input = new Power(zero, two);
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)new Number(0m), canonical); // 0^2 = 0
        }

        [TestMethod]
        public void Power_Canonicalize_HandlesOneBase()
        {
            Number one = new Number(1m);
            Symbol x = new Symbol("x");
            Power input = new Power(one, x);
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)new Number(1m), canonical); // 1^x = 1
        }
        
        [TestMethod]
        public void Power_Canonicalize_FlattensNestedPower()
        {
            // ( (a^b)^c ) should become a^(b*c)
            Symbol a = new Symbol("a");
            Number b = new Number(2m);
            Number c = new Number(3m);
            Power innerPower = new Power(a, b); // a^2
            Power outerPower = new Power(innerPower, c); // (a^2)^3

            IExpression canonical = outerPower.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Power));
            Power canonicalPower = (Power)canonical;

            Assert.AreEqual((IExpression)a, canonicalPower.Base);
            // The exponent should be (2*3) = 6
            Assert.AreEqual((IExpression)new Number(6m), canonicalPower.Exponent.Canonicalize());
        }

        [TestMethod]
        public void Power_Canonicalize_PerformsNumericalEvaluation()
        {
            Number num2 = new Number(2m);
            Number num3 = new Number(3m);
            Power twoToPowerOfThree = new Power(num2, num3);
            Assert.AreEqual((IExpression)new Number(8m), twoToPowerOfThree.Canonicalize());

            Number num27 = new Number(27m);
            Number oneThird = new Number(1m/3m);
            Power numToFractionalPower = new Power(num27, oneThird);
            Assert.AreEqual((IExpression)new Number(3m), numToFractionalPower.Canonicalize());

            Number num4 = new Number(4m);
            Number point5 = new Number(0.5m);
            Power numToPoint5Power = new Power(num4, point5);
            Assert.AreEqual((IExpression)new Number(2m), numToPoint5Power.Canonicalize());

            Number numBase = new Number(2m);
            Number negOne = new Number(-1m);
            Power negativeExponent = new Power(numBase, negOne);
            Assert.AreEqual((IExpression)new Number(0.5m), negativeExponent.Canonicalize());

            // Test nested numerical evaluation: ( (2^2)^3 ) = 2^6 = 64
            Power innerNumPower = new Power(new Number(2m), new Number(2m));
            Power outerNumPower = new Power(innerNumPower, new Number(3m));
            Assert.AreEqual((IExpression)new Number(64m), outerNumPower.Canonicalize());
        }

        [TestMethod]
        public void Power_Canonicalize_HandlesZeroBaseZeroExponentSymbolic()
        {
            Number zero = new Number(0m);
            Power input = new Power(zero, zero);
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)new Number(1m), canonical);
        }

        [TestMethod]
        public void Power_Canonicalize_HandlesZeroBaseNegativeExponentSymbolic()
        {
            Number zero = new Number(0m);
            Number negTwo = new Number(-2m);
            Power input = new Power(zero, negTwo);
            IExpression canonical = input.Canonicalize();
            // Current implementation keeps 0^-n as symbolic, matching the `this` return.
            Assert.AreSame(input, canonical);
            Assert.IsInstanceOfType(canonical, typeof(Power));
        }

        [TestMethod]
        public void Power_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol x = new Symbol("x");
            Number two = new Number(2m);
            Power pow1 = new Power(x, two);
            Power pow2 = new Power(x, two); // Same operands
            Power pow3 = new Power(two, x); // Swapped operands
            Power pow4 = new Power(x, new Number(3m)); // Different exponent

            Assert.IsTrue(pow1.Equals(pow2));
            Assert.AreEqual(pow1.GetHashCode(), pow2.GetHashCode());
            Assert.IsTrue(pow1.InternalEquals(pow2));
            Assert.AreEqual(pow1.InternalGetHashCode(), pow2.InternalGetHashCode());

            Assert.IsFalse(pow1.Equals(pow3));
            Assert.AreNotEqual(pow1.GetHashCode(), pow3.GetHashCode());
            Assert.IsFalse(pow1.InternalEquals(pow3));
            Assert.AreNotEqual(pow1.InternalGetHashCode(), pow3.InternalGetHashCode());

            Assert.IsFalse(pow1.Equals(pow4));
            Assert.AreNotEqual(pow1.GetHashCode(), pow4.GetHashCode());
        }

        [TestMethod]
        public void Power_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Symbol x = new Symbol("x");
            Number originalExp = new Number(2m);
            Number newExp = new Number(3m);
            Power originalPower = new Power(x, originalExp);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(x, newExp);
            
            Operation newPower = originalPower.WithArguments(newArgs);

            Assert.IsInstanceOfType(newPower, typeof(Power));
            Assert.AreNotSame(originalPower, newPower);
            Assert.AreEqual((IExpression)x, ((Power)newPower).Base);
            Assert.AreEqual((IExpression)newExp, ((Power)newPower).Exponent);
        }
    }
}