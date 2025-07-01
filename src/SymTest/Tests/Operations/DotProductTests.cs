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
    public sealed class DotProductTests
    {
        [TestMethod]
        public void DotProduct_ConstructorAndProperties_SetsCorrectly()
        {
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b")));
            DotProduct dotProd = new DotProduct(vec1, vec2);

            Assert.AreEqual((IExpression)vec1, dotProd.LeftOperand);
            Assert.AreEqual((IExpression)vec2, dotProd.RightOperand);
            Assert.AreEqual(2, dotProd.Arguments.Count);
            Assert.IsTrue(dotProd.IsOperation);
            Assert.IsFalse(dotProd.IsAtom);
        }

        [TestMethod]
        public void DotProduct_Shape_ReturnsScalarForValidVectorVectorProduct()
        {
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(new Number(1m), new Number(2m)));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Number(3m), new Number(4m)));
            DotProduct dotProd = new DotProduct(vec1, vec2);
            Assert.AreEqual(Shape.Scalar, dotProd.Shape);
        }

        [TestMethod]
        public void DotProduct_Shape_ReturnsScalarForValidMatrixMatrixProduct1xN_Nx1()
        {
            Matrix mat1x2 = new Matrix(ImmutableArray.Create(1, 2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m)));
            Matrix mat2x1 = new Matrix(ImmutableArray.Create(2, 1), ImmutableList.Create<IExpression>(new Number(3m), new Number(4m)));
            DotProduct dotProd = new DotProduct(mat1x2, mat2x1);
            Assert.AreEqual(Shape.Scalar, dotProd.Shape);
        }

        [TestMethod]
        public void DotProduct_Shape_ReturnsErrorForIncompatibleVectorDimensions()
        {
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Number(1m), new Number(2m)));
            Vector vec3 = new Vector(ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m)));
            DotProduct dotProd = new DotProduct(vec2, vec3);
            Assert.AreEqual(Shape.Error, dotProd.Shape);
        }

        [TestMethod]
        public void DotProduct_Shape_ReturnsErrorForInvalidMatrixDimensions()
        {
            Matrix mat2x2 = new Matrix(ImmutableArray.Create(2, 2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m)));
            Matrix mat2x1 = new Matrix(ImmutableArray.Create(2, 1), ImmutableList.Create<IExpression>(new Number(5m), new Number(6m)));
            DotProduct dotProd = new DotProduct(mat2x2, mat2x1); // Should be matrix multiply not dot product
            Assert.AreEqual(Shape.Error, dotProd.Shape);

            Matrix mat1x2 = new Matrix(ImmutableArray.Create(1, 2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m)));
            Matrix mat1x2_2 = new Matrix(ImmutableArray.Create(1, 2), ImmutableList.Create<IExpression>(new Number(3m), new Number(4m)));
            DotProduct dotProd2 = new DotProduct(mat1x2, mat1x2_2); // 1x2 * 1x2 invalid for dotproduct
            Assert.AreEqual(Shape.Error, dotProd2.Shape);
        }

        [TestMethod]
        public void DotProduct_ToDisplayString_ReturnsCorrectFormat()
        {
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b")));
            DotProduct dotProd = new DotProduct(vec1, vec2);
            Assert.AreEqual("DotProduct(Vector(x, y), Vector(a, b))", dotProd.ToDisplayString());
        }

        [TestMethod]
        public void DotProduct_Canonicalize_CanonicalizesOperands()
        {
            Add complexX = new Add(ImmutableList.Create<IExpression>(new Symbol("x"), new Number(0m))); // Canonicalizes to "x"
            Number three = new Number(3m);
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(complexX, new Number(1m)));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Number(2m), three));

            DotProduct input = new DotProduct(vec1, vec2);
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(DotProduct));
            DotProduct canonicalDotProd = (DotProduct)canonical;
            // `vec1` contains `complexX` which canonicalizes from Add to Symbol, thus changing its identity.
            // So the LeftOperand of canonicalDotProd will be a new instance.
            Assert.AreNotSame(vec1, canonicalDotProd.LeftOperand);

            // `vec2` contains `Number(2m)` and `Number(3m)` (where 3m is `three` reference).
            // Both `Number.Canonicalize()` methods return `this`, meaning their identities don't change.
            // Therefore, `vec2.Canonicalize()` will return `vec2` (the same instance).
            // So the RightOperand of canonicalDotProd *should* be the same instance as vec2.
            Assert.AreSame(vec2, canonicalDotProd.RightOperand);

            Vector expectedLeft = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Number(1m)));
            Vector expectedRight = new Vector(ImmutableList.Create<IExpression>(new Number(2m), new Number(3m)));
            Assert.AreEqual((IExpression)expectedLeft, canonicalDotProd.LeftOperand);
            Assert.AreEqual((IExpression)expectedRight, canonicalDotProd.RightOperand);
        }
        
        [TestMethod]
        public void DotProduct_Canonicalize_PerformsNumericalScalarProduct()
        {
            Number num1 = new Number(5m);
            Number num2 = new Number(10m);
            DotProduct input = new DotProduct(num1, num2); // Validates scalar*scalar as dot product
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)new Number(50m), canonical);
        }

        [TestMethod]
        public void DotProduct_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x")));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("y")));

            DotProduct dp1 = new DotProduct(vec1, vec2);
            DotProduct dp2 = new DotProduct(vec1, vec2); // Same operands
            DotProduct dp3 = new DotProduct(vec2, vec1); // Different order (DotProduct is commutative if operands are scalars/vectors, but typically not for symbolic)
            DotProduct dp4 = new DotProduct(vec1, new Vector(ImmutableList.Create<IExpression>(new Symbol("z"))));

            Assert.IsTrue(dp1.Equals(dp2));
            Assert.AreEqual(dp1.GetHashCode(), dp2.GetHashCode());
            Assert.IsTrue(dp1.InternalEquals(dp2));
            Assert.AreEqual(dp1.InternalGetHashCode(), dp2.InternalGetHashCode());

            Assert.IsFalse(dp1.Equals(dp3)); // Order matters for internal representation
            Assert.AreNotEqual(dp1.GetHashCode(), dp3.GetHashCode());
            Assert.IsFalse(dp1.InternalEquals(dp3));
            Assert.AreNotEqual(dp1.InternalGetHashCode(), dp3.InternalGetHashCode());

            Assert.IsFalse(dp1.Equals(dp4));
            Assert.AreNotEqual(dp1.GetHashCode(), dp4.GetHashCode());
        }

        [TestMethod]
        public void DotProduct_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x")));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("y")));
            Vector vec3 = new Vector(ImmutableList.Create<IExpression>(new Symbol("z")));
            DotProduct originalDotP = new DotProduct(vec1, vec2);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(vec1, vec3);
            
            Operation newDotP = originalDotP.WithArguments(newArgs);

            Assert.IsInstanceOfType(newDotP, typeof(DotProduct));
            Assert.AreNotSame(originalDotP, newDotP);
            Assert.AreEqual((IExpression)vec1, ((DotProduct)newDotP).LeftOperand);
            Assert.AreEqual((IExpression)vec3, ((DotProduct)newDotP).RightOperand);
        }
    }
}