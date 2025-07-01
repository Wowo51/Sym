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
    public sealed class IntegralTests
    {
        [TestMethod]
        public void Integral_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Integral intgrl = new Integral(x, y);

            Assert.AreEqual((IExpression)x, intgrl.TargetExpression);
            Assert.AreEqual((IExpression)y, intgrl.Variable);
            Assert.AreEqual(2, intgrl.Arguments.Count);
            Assert.IsTrue(intgrl.IsOperation);
            Assert.IsFalse(intgrl.IsAtom);
        }

        [TestMethod]
        public void Integral_Shape_ReturnsTargetShape()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Integral intgrlScalar = new Integral(x, y);
            Assert.AreEqual(Shape.Scalar, intgrlScalar.Shape);

            Vector vec = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Integral intgrlVector = new Integral(vec, new Symbol("t"));
            Assert.AreEqual(vec.Shape, intgrlVector.Shape);
        }

        [TestMethod]
        public void Integral_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Integral intgrl = new Integral(x, y);
            Assert.AreEqual("Integral(x, y)", intgrl.ToDisplayString());

            Add addExp = new Add(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b")));
            Integral complexIntgrl = new Integral(addExp, new Symbol("c"));
            Assert.AreEqual("Integral((a + b), c)", complexIntgrl.ToDisplayString());
        }

        [TestMethod]
        public void Integral_Canonicalize_CanonicalizesOperands()
        {
            Symbol x = new Symbol("x");
            Number zero = new Number(0m);
            Add complexTarget = new Add(ImmutableList.Create<IExpression>(x, zero)); // Canonicalizes to x
            Add complexVariable = new Add(ImmutableList.Create<IExpression>(new Symbol("y"), new Number(0m))); // Canonicalizes to y

            Integral input = new Integral(complexTarget, complexVariable);
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Integral));
            Integral canonicalIntgrl = (Integral)canonical;

            Assert.AreEqual((IExpression)x, canonicalIntgrl.TargetExpression);
            Assert.AreEqual((IExpression)new Symbol("y"), canonicalIntgrl.Variable);
            Assert.AreNotSame(input, canonicalIntgrl); // Should be new instance due to canonicalized elements
        }

        [TestMethod]
        public void Integral_Canonicalize_ReturnsSelfIfNoChange()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Integral intgrl = new Integral(x, y);

            IExpression canonical = intgrl.Canonicalize();
            Assert.AreSame(intgrl, canonical);
        }

        [TestMethod]
        public void Integral_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Symbol z = new Symbol("z");

            Integral intgrl1 = new Integral(x, y);
            Integral intgrl2 = new Integral(x, y); // Same operands
            Integral intgrl3 = new Integral(x, z); // Different variable
            Integral intgrl4 = new Integral(y, x); // Swapped operands

            Assert.IsTrue(intgrl1.Equals(intgrl2));
            Assert.AreEqual(intgrl1.GetHashCode(), intgrl2.GetHashCode());
            Assert.IsTrue(intgrl1.InternalEquals(intgrl2));
            Assert.AreEqual(intgrl1.InternalGetHashCode(), intgrl2.InternalGetHashCode());

            Assert.IsFalse(intgrl1.Equals(intgrl3));
            Assert.AreNotEqual(intgrl1.GetHashCode(), intgrl3.GetHashCode());
            Assert.IsFalse(intgrl1.InternalEquals(intgrl3));
            Assert.AreNotEqual(intgrl1.InternalGetHashCode(), intgrl3.InternalGetHashCode());

            Assert.IsFalse(intgrl1.Equals(intgrl4)); // Not equal due to operand order.
            Assert.AreNotEqual(intgrl1.GetHashCode(), intgrl4.GetHashCode());
        }

        [TestMethod]
        public void Integral_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Symbol z = new Symbol("z");
            Integral originalIntgrl = new Integral(x, y);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(x, z);
            
            Operation newIntgrl = originalIntgrl.WithArguments(newArgs);

            Assert.IsInstanceOfType(newIntgrl, typeof(Integral));
            Assert.AreNotSame(originalIntgrl, newIntgrl);
            Assert.AreEqual((IExpression)x, ((Integral)newIntgrl).TargetExpression);
            Assert.AreEqual((IExpression)z, ((Integral)newIntgrl).Variable);
        }
    }
}