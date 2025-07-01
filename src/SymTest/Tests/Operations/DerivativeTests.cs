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
    public sealed class DerivativeTests
    {
        [TestMethod]
        public void Derivative_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Derivative deriv = new Derivative(x, y);

            Assert.AreEqual((IExpression)x, deriv.TargetExpression);
            Assert.AreEqual((IExpression)y, deriv.Variable);
            Assert.AreEqual(2, deriv.Arguments.Count);
            Assert.IsTrue(deriv.IsOperation);
            Assert.IsFalse(deriv.IsAtom);
        }

        [TestMethod]
        public void Derivative_Shape_ReturnsTargetShape()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Derivative derivScalar = new Derivative(x, y);
            Assert.AreEqual(Shape.Scalar, derivScalar.Shape);

            Vector vec = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Derivative derivVector = new Derivative(vec, new Symbol("t"));
            Assert.AreEqual(vec.Shape, derivVector.Shape);
        }

        [TestMethod]
        public void Derivative_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Derivative deriv = new Derivative(x, y);
            Assert.AreEqual("Derivative(x, y)", deriv.ToDisplayString());

            Add addExp = new Add(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b")));
            Derivative complexDeriv = new Derivative(addExp, new Symbol("c"));
            Assert.AreEqual("Derivative((a + b), c)", complexDeriv.ToDisplayString());
        }

        [TestMethod]
        public void Derivative_Canonicalize_CanonicalizesOperands()
        {
            Symbol x = new Symbol("x");
            Number zero = new Number(0m);
            Add complexTarget = new Add(ImmutableList.Create<IExpression>(x, zero)); // Canonicalizes to x
            Add complexVariable = new Add(ImmutableList.Create<IExpression>(new Symbol("y"), new Number(0m))); // Canonicalizes to y

            Derivative input = new Derivative(complexTarget, complexVariable);
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Derivative));
            Derivative canonicalDeriv = (Derivative)canonical;

            Assert.AreEqual((IExpression)x, canonicalDeriv.TargetExpression);
            Assert.AreEqual((IExpression)new Symbol("y"), canonicalDeriv.Variable);
            Assert.AreNotSame(input, canonicalDeriv); // Should be new instance due to canonicalized elements
        }

        [TestMethod]
        public void Derivative_Canonicalize_ReturnsSelfIfNoChange()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Derivative deriv = new Derivative(x, y);

            IExpression canonical = deriv.Canonicalize();
            Assert.AreSame(deriv, canonical);
        }

        [TestMethod]
        public void Derivative_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Symbol z = new Symbol("z");

            Derivative deriv1 = new Derivative(x, y);
            Derivative deriv2 = new Derivative(x, y); // Same operands
            Derivative deriv3 = new Derivative(x, z); // Different variable
            Derivative deriv4 = new Derivative(y, x); // Swapped operands

            Assert.IsTrue(deriv1.Equals(deriv2));
            Assert.AreEqual(deriv1.GetHashCode(), deriv2.GetHashCode());
            Assert.IsTrue(deriv1.InternalEquals(deriv2));
            Assert.AreEqual(deriv1.InternalGetHashCode(), deriv2.InternalGetHashCode());

            Assert.IsFalse(deriv1.Equals(deriv3));
            Assert.AreNotEqual(deriv1.GetHashCode(), deriv3.GetHashCode());
            Assert.IsFalse(deriv1.InternalEquals(deriv3));
            Assert.AreNotEqual(deriv1.InternalGetHashCode(), deriv3.InternalGetHashCode());

            Assert.IsFalse(deriv1.Equals(deriv4)); // Not equal due to operand order.
            Assert.AreNotEqual(deriv1.GetHashCode(), deriv4.GetHashCode());
        }

        [TestMethod]
        public void Derivative_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Symbol z = new Symbol("z");
            Derivative originalDeriv = new Derivative(x, y);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(x, z);
            
            Operation newDeriv = originalDeriv.WithArguments(newArgs);

            Assert.IsInstanceOfType(newDeriv, typeof(Derivative));
            Assert.AreNotSame(originalDeriv, newDeriv);
            Assert.AreEqual((IExpression)x, ((Derivative)newDeriv).TargetExpression);
            Assert.AreEqual((IExpression)z, ((Derivative)newDeriv).Variable);
        }
    }
}