//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace SymTest
{
    [TestClass]
    public sealed class EqualityTests
    {
        [TestMethod]
        public void Equality_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Equality equality = new Equality(x, five);

            Assert.AreEqual((IExpression)x, equality.LeftOperand);
            Assert.AreEqual((IExpression)five, equality.RightOperand);
            Assert.AreEqual(2, equality.Arguments.Count);
            Assert.IsTrue(equality.IsOperation);
            Assert.IsFalse(equality.IsAtom);
        }

        [TestMethod]
        public void Equality_Inheritance_IsOperation()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Equality equality = new Equality(x, five);

            Assert.IsInstanceOfType(equality, typeof(Operation));
        }

        [TestMethod]
        public void Equality_Shape_ReturnsScalar()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Equality equality = new Equality(x, five);

            Assert.AreEqual(Shape.Scalar, equality.Shape);
        }

        [TestMethod]
        public void Equality_Canonicalize_CanonicalizesOperandsButDoesNotSolve()
        {
            Symbol x = new Symbol("x");
            Number one = new Number(1m);
            Number zero = new Number(0m);

            // Left: x + 0 -> x
            // Right: (y * 1) + 1 -> y + 1
            // So: (x + 0 = (y * 1) + 1) should become (x = (y + 1)) after operand canonicalization.
            Add complexLeft = new Add(ImmutableList.Create<IExpression>(x, zero));
            Multiply complexRightInner = new Multiply(ImmutableList.Create<IExpression>(new Symbol("y"), one));
            Add complexRight = new Add(ImmutableList.Create<IExpression>(complexRightInner, one));

            Equality input = new Equality(complexLeft, complexRight);
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Equality));
            Equality canonicalEquality = (Equality)canonical;

            // Assert that operands were canonicalized
            Assert.AreEqual((IExpression)x, canonicalEquality.LeftOperand);
            Assert.AreEqual(new Add(ImmutableList.Create<IExpression>(new Symbol("y"), one)), canonicalEquality.RightOperand);
            
            // Crucially, assert that the equation was NOT solved or simplified based on equality
            // For example, if it were solved, (x+0 = x) should become (0=0) or True, but we expect it to remain (x = x) after canonicalizing operands.
            Equality selfEqualInput = new Equality(new Add(ImmutableList.Create<IExpression>(x, one)), new Add(ImmutableList.Create<IExpression>(x, one)));
            IExpression selfEqualCanonical = selfEqualInput.Canonicalize();
            
            Assert.IsInstanceOfType(selfEqualCanonical, typeof(Equality));
            Equality selfEqualCanonicalEquality = (Equality)selfEqualCanonical;
            Assert.AreEqual(selfEqualInput.LeftOperand.Canonicalize(), selfEqualCanonicalEquality.LeftOperand);
            Assert.AreEqual(selfEqualInput.RightOperand.Canonicalize(), selfEqualCanonicalEquality.RightOperand);
            
            Assert.AreNotSame(input, canonicalEquality); // Should be new instance due to canonicalized elements
        }

        [TestMethod]
        public void Equality_Canonicalize_ReturnsSelfIfNoChange()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Equality equality = new Equality(x, five);

            IExpression canonical = equality.Canonicalize();
            Assert.AreSame(equality, canonical);
        }

        [TestMethod]
        public void Equality_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Equality equality = new Equality(x, five);
            Assert.AreEqual("(x = 5)", equality.ToDisplayString());

            Add complexLeft = new Add(ImmutableList.Create<IExpression>(new Symbol("a"), new Number(2m)));
            Multiply complexRight = new Multiply(ImmutableList.Create<IExpression>(new Symbol("b"), new Symbol("c")));
            Equality complexEquality = new Equality(complexLeft, complexRight);
            Assert.AreEqual("((a + 2) = (b * c))", complexEquality.ToDisplayString());
        }

        [TestMethod]
        public void Equality_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Add xPlusOne = new Add(ImmutableList.Create<IExpression>(x, new Number(1m)));
            Add xPlusTwo = new Add(ImmutableList.Create<IExpression>(x, new Number(2m)));
            
            Equality eq1 = new Equality(x, five);
            Equality eq2 = new Equality(x, five); // Same operands
            Equality eq3 = new Equality(five, x); // Swapped operands, not equal for Equality
            Equality eq4 = new Equality(x, xPlusOne); // Different right operand
            Equality eq5 = new Equality(xPlusOne, xPlusOne); // Canonical forms are equal (x+1 = x+1)
            Equality eq6 = new Equality(xPlusOne, new Add(ImmutableList.Create<IExpression>(new Number(1m), x))); // Canonical forms are equal after sorting (x+1 = x+1)

            Assert.IsTrue(eq1.Equals(eq2));
            Assert.AreEqual(eq1.GetHashCode(), eq2.GetHashCode());
            Assert.IsTrue(eq1.InternalEquals(eq2));
            Assert.AreEqual(eq1.InternalGetHashCode(), eq2.InternalGetHashCode());

            Assert.IsFalse(eq1.Equals(eq3)); // Order matters for Equality
            Assert.AreNotEqual(eq1.GetHashCode(), eq3.GetHashCode());
            Assert.IsFalse(eq1.InternalEquals(eq3));
            Assert.AreNotEqual(eq1.InternalGetHashCode(), eq3.InternalGetHashCode());

            Assert.IsFalse(eq1.Equals(eq4));
            Assert.AreNotEqual(eq1.GetHashCode(), eq4.GetHashCode());
            Assert.IsFalse(eq1.InternalEquals(eq4));
            Assert.AreNotEqual(eq1.InternalGetHashCode(), eq4.InternalGetHashCode());

            Assert.IsTrue(eq5.Equals(eq6)); // Canonical forms (x+1 = x+1)
            Assert.AreEqual(eq5.GetHashCode(), eq6.GetHashCode());
            // InternalEquals operates on canonical forms for operations.
            // If the arguments of eq5 and eq6 canonicalize identically, then their InternalEquals will be true.
            Assert.IsTrue(eq5.Canonicalize().InternalEquals(eq6.Canonicalize())); 
            Assert.AreEqual(eq5.Canonicalize().InternalGetHashCode(), eq6.Canonicalize().InternalGetHashCode());
        }

        [TestMethod]
        public void Equality_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Number ten = new Number(10m);
            Equality originalEq = new Equality(x, five);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(x, ten);
            
            Operation newEq = originalEq.WithArguments(newArgs);

            Assert.IsInstanceOfType(newEq, typeof(Equality));
            Assert.AreNotSame(originalEq, newEq);
            Assert.AreEqual((IExpression)x, ((Equality)newEq).LeftOperand);
            Assert.AreEqual((IExpression)ten, ((Equality)newEq).RightOperand);
        }
    }
}