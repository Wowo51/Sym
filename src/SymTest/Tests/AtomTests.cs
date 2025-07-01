//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Atoms;
using Sym.Core;
using System.Collections.Immutable;
using System;

namespace SymTest
{
    [TestClass]
    public sealed class AtomTests
    {
        [TestMethod]
        public void Number_ConstructorAndProperties_SetsCorrectly()
        {
            System.Decimal value = 123.45m;
            Number number = new Number(value);
            Assert.AreEqual(value, number.Value);
            Assert.AreEqual(Shape.Scalar, number.Shape);
            Assert.IsTrue(number.IsAtom);
            Assert.IsFalse(number.IsOperation);
        }

        [TestMethod]
        public void Number_ToDisplayString_ReturnsCorrectFormat()
        {
            Number number = new Number(123.45m);
            Assert.AreEqual("123.45", number.ToDisplayString());

            Number integerNumber = new Number(10m);
            Assert.AreEqual("10", integerNumber.ToDisplayString());

            Number zeroNumber = new Number(0m);
            Assert.AreEqual("0", zeroNumber.ToDisplayString());
        }

        [TestMethod]
        public void Number_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Number num1 = new Number(1.23m);
            Number num2 = new Number(1.23m);
            Number num3 = new Number(4.56m);
            Symbol sym = new Symbol("x");

            Assert.IsTrue(num1.Equals(num2));
            Assert.AreEqual(num1.GetHashCode(), num2.GetHashCode());
            Assert.IsTrue(num1.InternalEquals(num2));
            Assert.AreEqual(num1.InternalGetHashCode(), num2.InternalGetHashCode());

            Assert.IsFalse(num1.Equals(num3));
            Assert.AreNotEqual(num1.GetHashCode(), num3.GetHashCode());
            Assert.IsFalse(num1.InternalEquals(num3));
            Assert.AreNotEqual(num1.InternalGetHashCode(), num3.InternalGetHashCode());

            Assert.IsFalse(num1.Equals(sym));
            Assert.IsFalse(num1.InternalEquals(sym));

            Assert.IsTrue(num1.Equals(num1)); // Self-equality
            Assert.IsFalse(num1.Equals(null)); // Null comparison
        }

        [TestMethod]
        public void Symbol_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol symbol1 = new Symbol("x");
            Assert.AreEqual("x", symbol1.Name);
            Assert.AreEqual(Shape.Scalar, symbol1.Shape);
            Assert.IsTrue(symbol1.IsAtom);
            Assert.IsFalse(symbol1.IsOperation);

            Shape vectorShape = new Shape(ImmutableArray.Create(3));
            Symbol symbol2 = new Symbol("vec", vectorShape);
            Assert.AreEqual("vec", symbol2.Name);
            Assert.AreEqual(vectorShape, symbol2.Shape);
        }

        [TestMethod]
        public void Symbol_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol symbol1 = new Symbol("x");
            Assert.AreEqual("x", symbol1.ToDisplayString());

            Shape vectorShape = new Shape(ImmutableArray.Create(3));
            Symbol symbol2 = new Symbol("vec", vectorShape);
            Assert.AreEqual("vec(3)", symbol2.ToDisplayString());
        }

        [TestMethod]
        public void Symbol_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol sym1 = new Symbol("x");
            Symbol sym2 = new Symbol("x");
            Symbol sym3 = new Symbol("y");
            Shape vectorShape = new Shape(ImmutableArray.Create(3));
            Symbol sym4 = new Symbol("x", vectorShape);
            Number num = new Number(5m);

            Assert.IsTrue(sym1.Equals(sym2));
            Assert.AreEqual(sym1.GetHashCode(), sym2.GetHashCode());
            Assert.IsTrue(sym1.InternalEquals(sym2));
            Assert.AreEqual(sym1.InternalGetHashCode(), sym2.InternalGetHashCode());

            Assert.IsFalse(sym1.Equals(sym3));
            Assert.AreNotEqual(sym1.GetHashCode(), sym3.GetHashCode());
            Assert.IsFalse(sym1.InternalEquals(sym3));
            Assert.AreNotEqual(sym1.InternalGetHashCode(), sym3.InternalGetHashCode());

            Assert.IsFalse(sym1.Equals(sym4)); // Different shape
            Assert.AreNotEqual(sym1.GetHashCode(), sym4.GetHashCode());
            Assert.IsFalse(sym1.InternalEquals(sym4));
            Assert.AreNotEqual(sym1.InternalGetHashCode(), sym4.InternalGetHashCode());

            Assert.IsFalse(sym1.Equals(num));
            Assert.IsFalse(sym1.InternalEquals(num));
        }

        [TestMethod]
        public void Wild_ConstructorAndProperties_SetsCorrectly()
        {
            Wild wild1 = new Wild("f");
            Assert.AreEqual("f", wild1.Name);
            Assert.AreEqual(WildConstraint.None, wild1.Constraint);
            Assert.AreEqual(Shape.Wildcard, wild1.Shape); // Wildcards are always scalars conceptually until bound
            Assert.IsTrue(wild1.IsAtom);
            Assert.IsFalse(wild1.IsOperation);

            Wild wild2 = new Wild("c", WildConstraint.Constant);
            Assert.AreEqual("c", wild2.Name);
            Assert.AreEqual(WildConstraint.Constant, wild2.Constraint);

            Wild wild3 = new Wild("s", WildConstraint.Scalar);
            Assert.AreEqual("s", wild3.Name);
            Assert.AreEqual(WildConstraint.Scalar, wild3.Constraint);
        }

        [TestMethod]
        public void Wild_ToDisplayString_ReturnsCorrectFormat()
        {
            Wild wild1 = new Wild("f");
            Assert.AreEqual("Wild('f')", wild1.ToDisplayString());

            Wild wild2 = new Wild("c", WildConstraint.Constant);
            Assert.AreEqual("Wild('c', Constant)", wild2.ToDisplayString());
        }

        [TestMethod]
        public void Wild_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Wild wild1 = new Wild("f");
            Wild wild2 = new Wild("f");
            Wild wild3 = new Wild("g");
            Wild wild4 = new Wild("f", WildConstraint.Constant);
            Number num = new Number(5m);

            Assert.IsTrue(wild1.Equals(wild2));
            Assert.AreEqual(wild1.GetHashCode(), wild2.GetHashCode());
            Assert.IsTrue(wild1.InternalEquals(wild2));
            Assert.AreEqual(wild1.InternalGetHashCode(), wild2.InternalGetHashCode());

            Assert.IsFalse(wild1.Equals(wild3));
            Assert.AreNotEqual(wild1.GetHashCode(), wild3.GetHashCode());
            Assert.IsFalse(wild1.InternalEquals(wild3));
            Assert.AreNotEqual(wild1.InternalGetHashCode(), wild3.InternalGetHashCode());

            Assert.IsFalse(wild1.Equals(wild4)); // Different constraint
            Assert.AreNotEqual(wild1.GetHashCode(), wild4.GetHashCode());
            Assert.IsFalse(wild1.InternalEquals(wild4));
            Assert.AreNotEqual(wild1.InternalGetHashCode(), wild4.InternalGetHashCode());

            Assert.IsFalse(wild1.Equals(num));
            Assert.IsFalse(wild1.InternalEquals(num));
        }

        [TestMethod]
        public void Canonicalize_ReturnsSelfForAtoms()
        {
            Number num = new Number(10m);
            Symbol sym = new Symbol("y");
            Wild wild = new Wild("z");

            Assert.AreSame(num, num.Canonicalize());
            Assert.AreSame(sym, sym.Canonicalize());
            Assert.AreSame(wild, wild.Canonicalize());
        }
    }
}