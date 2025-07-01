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
    public sealed class GradTests
    {
        [TestMethod]
        public void Grad_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol scalarF = new Symbol("f");
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Grad gradOp = new Grad(scalarF, vecVar);

            Assert.AreEqual((IExpression)scalarF, gradOp.ScalarExpression);
            Assert.AreEqual((IExpression)vecVar, gradOp.VectorVariable);
            Assert.AreEqual(2, gradOp.Arguments.Count);
            Assert.IsTrue(gradOp.IsOperation);
            Assert.IsFalse(gradOp.IsAtom);
        }

        [TestMethod]
        public void Grad_Shape_ReturnsVectorVariableShapeForScalar()
        {
            Symbol scalarF = new Symbol("f");
            Vector vecVar_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Grad gradOp = new Grad(scalarF, vecVar_3D);
            Assert.AreEqual(vecVar_3D.Shape, gradOp.Shape);

            Vector vecVar_2D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Grad gradOp2 = new Grad(scalarF, vecVar_2D);
            Assert.AreEqual(vecVar_2D.Shape, gradOp2.Shape);
        }

        [TestMethod]
        public void Grad_Shape_ReturnsErrorForNonScalarExpression()
        {
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Grad gradOp = new Grad(vecF, vecVar);
            Assert.AreEqual(Shape.Error, gradOp.Shape);

            Matrix matrixF = new Matrix(ImmutableArray.Create(2,2),ImmutableList.Create<IExpression>(new Number(1m),new Number(2m),new Number(3m),new Number(4m)));
            Grad gradOp2 = new Grad(matrixF, vecVar);
            Assert.AreEqual(Shape.Error, gradOp2.Shape);
        }

        [TestMethod]
        public void Grad_Shape_ReturnsErrorForNonVectorVariable()
        {
            Symbol scalarF = new Symbol("f");
            Number scalarVar = new Number(5m); // Not a vector
            Grad gradOp = new Grad(scalarF, scalarVar);
            Assert.AreEqual(Shape.Error, gradOp.Shape);

            Matrix matrixVar = new Matrix(ImmutableArray.Create(2,2),ImmutableList.Create<IExpression>(new Number(1m),new Number(2m),new Number(3m),new Number(4m)));
            Grad gradOp2 = new Grad(scalarF, matrixVar);
            Assert.AreEqual(Shape.Error, gradOp2.Shape);
        }

        [TestMethod]
        public void Grad_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol scalarF = new Symbol("f");
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Grad gradOp = new Grad(scalarF, vecVar);
            Assert.AreEqual("Grad(f, Vector(x, y, z))", gradOp.ToDisplayString());
        }

        [TestMethod]
        public void Grad_Canonicalize_CanonicalizesOperands()
        {
            Symbol f = new Symbol("f");
            Number zero = new Number(0m);
            Add complexF = new Add(ImmutableList.Create<IExpression>(f, zero)); // Canonicalizes to f
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Add complexX = new Add(ImmutableList.Create<IExpression>(new Symbol("x"), zero));
            Vector complexVecVar = new Vector(ImmutableList.Create<IExpression>(complexX, new Symbol("y")));
            
            Grad input = new Grad(complexF, complexVecVar);
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Grad));
            Grad canonicalGrad = (Grad)canonical;

            Vector expectedVecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));

            Assert.AreEqual((IExpression)f, canonicalGrad.ScalarExpression);
            Assert.AreEqual((IExpression)expectedVecVar, canonicalGrad.VectorVariable);
            Assert.AreNotSame(input, canonicalGrad); // Should be new instance due to canonicalized elements
        }
        
        [TestMethod]
        public void Grad_Canonicalize_ReturnsSelfIfNoChange()
        {
            Symbol f = new Symbol("f");
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Grad gradOp = new Grad(f, vecVar);

            IExpression canonical = gradOp.Canonicalize();
            Assert.AreSame(gradOp, canonical);
        }

        [TestMethod]
        public void Grad_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol f1 = new Symbol("f");
            Symbol f2 = new Symbol("g");
            Vector vecVar1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Vector vecVar2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b"), new Symbol("c")));

            Grad grad1 = new Grad(f1, vecVar1);
            Grad grad2 = new Grad(f1, vecVar1); // Same operands
            Grad grad3 = new Grad(f1, vecVar2); // Different variable
            Grad grad4 = new Grad(f2, vecVar1); // Different scalar expression
            Grad grad5 = new Grad(vecVar1, f1); // Swapped operands (though type checking would likely error)

            Assert.IsTrue(grad1.Equals(grad2));
            Assert.AreEqual(grad1.GetHashCode(), grad2.GetHashCode());
            Assert.IsTrue(grad1.InternalEquals(grad2));
            Assert.AreEqual(grad1.InternalGetHashCode(), grad2.InternalGetHashCode());

            Assert.IsFalse(grad1.Equals(grad3));
            Assert.AreNotEqual(grad1.GetHashCode(), grad3.GetHashCode());
            Assert.IsFalse(grad1.InternalEquals(grad3));
            Assert.AreNotEqual(grad1.InternalGetHashCode(), grad3.InternalGetHashCode());
            
            Assert.IsFalse(grad1.Equals(grad4));
            Assert.AreNotEqual(grad1.GetHashCode(), grad4.GetHashCode());

            Assert.IsFalse(grad1.Equals(grad5));
            Assert.AreNotEqual(grad1.GetHashCode(), grad5.GetHashCode());
        }

        [TestMethod]
        public void Grad_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Symbol f1 = new Symbol("f");
            Symbol f2 = new Symbol("g");
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Grad originalGrad = new Grad(f1, vecVar);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(f2, vecVar);
            
            Operation newGrad = originalGrad.WithArguments(newArgs);

            Assert.IsInstanceOfType(newGrad, typeof(Grad));
            Assert.AreNotSame(originalGrad, newGrad);
            Assert.AreEqual((IExpression)f2, ((Grad)newGrad).ScalarExpression);
            Assert.AreEqual((IExpression)vecVar, ((Grad)newGrad).VectorVariable);
        }
    }
}