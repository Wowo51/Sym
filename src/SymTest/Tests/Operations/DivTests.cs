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
    public sealed class DivTests
    {
        [TestMethod]
        public void Div_ConstructorAndProperties_SetsCorrectly()
        {
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Symbol("f_y"), new Symbol("f_z")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Div divOp = new Div(vecF, vecVar);

            Assert.AreEqual((IExpression)vecF, divOp.VectorExpression);
            Assert.AreEqual((IExpression)vecVar, divOp.VectorVariable);
            Assert.AreEqual(2, divOp.Arguments.Count);
            Assert.IsTrue(divOp.IsOperation);
            Assert.IsFalse(divOp.IsAtom);
        }

        [TestMethod]
        public void Div_Shape_ReturnsScalarForValidVectorVector()
        {
            Vector vecF_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Vector vecVar_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Div divOp = new Div(vecF_3D, vecVar_3D);
            Assert.AreEqual(Shape.Scalar, divOp.Shape);
        }

        [TestMethod]
        public void Div_Shape_ReturnsErrorForNonVectorExpression()
        {
            Number scalarF = new Number(5m);
            Vector vecVar_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Div divOp = new Div(scalarF, vecVar_3D);
            Assert.AreEqual(Shape.Error, divOp.Shape);
        }

        [TestMethod]
        public void Div_Shape_ReturnsErrorForNonVectorVariable()
        {
            Vector vecF_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Number scalarVar = new Number(5m); // Not a vector
            Div divOp = new Div(vecF_3D, scalarVar);
            Assert.AreEqual(Shape.Error, divOp.Shape);
        }

        [TestMethod]
        public void Div_Shape_ReturnsErrorForIncompatibleVectorDimensions()
        {
            Vector vecF_2D = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy")));
            Vector vecVar_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Div divOp = new Div(vecF_2D, vecVar_3D);
            Assert.AreEqual(Shape.Error, divOp.Shape);

            Vector vecF_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Vector vecVar_2D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Div divOp2 = new Div(vecF_3D, vecVar_2D);
            Assert.AreEqual(Shape.Error, divOp2.Shape);
        }

        [TestMethod]
        public void Div_ToDisplayString_ReturnsCorrectFormat()
        {
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Symbol("f_y"), new Symbol("f_z")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Div divOp = new Div(vecF, vecVar);
            Assert.AreEqual("Div(Vector(f_x, f_y, f_z), Vector(x, y, z))", divOp.ToDisplayString());
        }

        [TestMethod]
        public void Div_Canonicalize_CanonicalizesOperands()
        {
            Add complexFx = new Add(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Number(0m)));
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(complexFx, new Symbol("fy"), new Symbol("fz")));
            Add complexX = new Add(ImmutableList.Create<IExpression>(new Symbol("x"), new Number(0m)));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(complexX, new Symbol("y"), new Symbol("z")));

            Div input = new Div(vecF, vecVar);
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Div));
            Div canonicalDiv = (Div)canonical;

            Vector expectedVecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Symbol("fy"), new Symbol("fz")));
            Vector expectedVecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));

            Assert.AreEqual((IExpression)expectedVecF, canonicalDiv.VectorExpression);
            Assert.AreEqual((IExpression)expectedVecVar, canonicalDiv.VectorVariable);
            Assert.AreNotSame(input, canonicalDiv); // Should be new instance due to canonicalized elements
        }

        [TestMethod]
        public void Div_Canonicalize_ReturnsSelfIfNoChange()
        {
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Symbol("f_y"), new Symbol("f_z")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Div divOp = new Div(vecF, vecVar);

            IExpression canonical = divOp.Canonicalize();
            Assert.AreSame(divOp, canonical);
        }

        [TestMethod]
        public void Div_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Vector vecF1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Vector vecVar1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Vector vecVar2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b"), new Symbol("c")));

            Div div1 = new Div(vecF1, vecVar1);
            Div div2 = new Div(vecF1, vecVar1); // Same operands
            Div div3 = new Div(vecF1, vecVar2); // Different variable
            Div div4 = new Div(vecVar1, vecF1); // Swapped operands

            Assert.IsTrue(div1.Equals(div2));
            Assert.AreEqual(div1.GetHashCode(), div2.GetHashCode());
            Assert.IsTrue(div1.InternalEquals(div2));
            Assert.AreEqual(div1.InternalGetHashCode(), div2.InternalGetHashCode());

            Assert.IsFalse(div1.Equals(div3));
            Assert.AreNotEqual(div1.GetHashCode(), div3.GetHashCode());
            Assert.IsFalse(div1.InternalEquals(div3));
            Assert.AreNotEqual(div1.InternalGetHashCode(), div3.InternalGetHashCode());

            Assert.IsFalse(div1.Equals(div4)); // Not equal due to operand order.
            Assert.AreNotEqual(div1.GetHashCode(), div4.GetHashCode());
        }

        [TestMethod]
        public void Div_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Vector vecF1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Vector vecF2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("gx"), new Symbol("gy"), new Symbol("gz")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Div originalDiv = new Div(vecF1, vecVar);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(vecF2, vecVar);

            Operation newDiv = originalDiv.WithArguments(newArgs);

            Assert.IsInstanceOfType(newDiv, typeof(Div));
            Assert.AreNotSame(originalDiv, newDiv);
            Assert.AreEqual((IExpression)vecF2, ((Div)newDiv).VectorExpression);
            Assert.AreEqual((IExpression)vecVar, ((Div)newDiv).VectorVariable);
        }
    }
}