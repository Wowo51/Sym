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
    public sealed class CurlTests
    {
        [TestMethod]
        public void Curl_ConstructorAndProperties_SetsCorrectly()
        {
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Symbol("f_y"), new Symbol("f_z")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Curl curlOp = new Curl(vecF, vecVar);

            Assert.AreEqual((IExpression)vecF, curlOp.VectorExpression);
            Assert.AreEqual((IExpression)vecVar, curlOp.VectorVariable);
            Assert.AreEqual(2, curlOp.Arguments.Count);
            Assert.IsTrue(curlOp.IsOperation);
            Assert.IsFalse(curlOp.IsAtom);
        }

        [TestMethod]
        public void Curl_Shape_ReturnsVectorVariableShapeForValidVectorVector()
        {
            Vector vecF_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Vector vecVar_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Curl curlOp = new Curl(vecF_3D, vecVar_3D);
            Assert.AreEqual(vecVar_3D.Shape, curlOp.Shape);
        }

        [TestMethod]
        public void Curl_Shape_ReturnsErrorForNonVectorExpression()
        {
            Number scalarF = new Number(5m);
            Vector vecVar_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Curl curlOp = new Curl(scalarF, vecVar_3D);
            Assert.AreEqual(Shape.Error, curlOp.Shape);
        }

        [TestMethod]
        public void Curl_Shape_ReturnsErrorForNonVectorVariable()
        {
            Vector vecF_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Number scalarVar = new Number(5m); // Not a vector
            Curl curlOp = new Curl(vecF_3D, scalarVar);
            Assert.AreEqual(Shape.Error, curlOp.Shape);
        }

        [TestMethod]
        public void Curl_Shape_ReturnsErrorForIncompatibleVectorDimensions()
        {
            Vector vecF_2D = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy")));
            Vector vecVar_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Curl curlOp = new Curl(vecF_2D, vecVar_3D);
            Assert.AreEqual(Shape.Error, curlOp.Shape);

            Vector vecF_3D = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Vector vecVar_2D = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Curl curlOp2 = new Curl(vecF_3D, vecVar_2D);
            Assert.AreEqual(Shape.Error, curlOp2.Shape);
        }

        [TestMethod]
        public void Curl_ToDisplayString_ReturnsCorrectFormat()
        {
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Symbol("f_y"), new Symbol("f_z")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Curl curlOp = new Curl(vecF, vecVar);
            Assert.AreEqual("Curl(Vector(f_x, f_y, f_z), Vector(x, y, z))", curlOp.ToDisplayString());
        }

        [TestMethod]
        public void Curl_Canonicalize_CanonicalizesOperands()
        {
            Add complexFx = new Add(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Number(0m)));
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(complexFx, new Symbol("fy"), new Symbol("fz")));
            Add complexX = new Add(ImmutableList.Create<IExpression>(new Symbol("x"), new Number(0m)));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(complexX, new Symbol("y"), new Symbol("z")));

            Curl input = new Curl(vecF, vecVar);
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Curl));
            Curl canonicalCurl = (Curl)canonical;

            Vector expectedVecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Symbol("fy"), new Symbol("fz")));
            Vector expectedVecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));

            Assert.AreEqual((IExpression)expectedVecF, canonicalCurl.VectorExpression);
            Assert.AreEqual((IExpression)expectedVecVar, canonicalCurl.VectorVariable);
            Assert.AreNotSame(input, canonicalCurl); // Should be new instance due to canonicalized elements
        }

        [TestMethod]
        public void Curl_Canonicalize_ReturnsSelfIfNoChange()
        {
            Vector vecF = new Vector(ImmutableList.Create<IExpression>(new Symbol("f_x"), new Symbol("f_y"), new Symbol("f_z")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Curl curlOp = new Curl(vecF, vecVar);

            IExpression canonical = curlOp.Canonicalize();
            Assert.AreSame(curlOp, canonical);
        }

        [TestMethod]
        public void Curl_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Vector vecF1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));

            Vector vecVar1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Vector vecVar2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b"), new Symbol("c")));

            Curl curl1 = new Curl(vecF1, vecVar1);
            Curl curl2 = new Curl(vecF1, vecVar1); // Same operands
            Curl curl3 = new Curl(vecF1, vecVar2); // Different variable
            Curl curl4 = new Curl(vecVar1, vecF1); // Swapped operands (though type checking would likely error)

            Assert.IsTrue(curl1.Equals(curl2));
            Assert.AreEqual(curl1.GetHashCode(), curl2.GetHashCode());
            Assert.IsTrue(curl1.InternalEquals(curl2));
            Assert.AreEqual(curl1.InternalGetHashCode(), curl2.InternalGetHashCode());

            Assert.IsFalse(curl1.Equals(curl3));
            Assert.AreNotEqual(curl1.GetHashCode(), curl3.GetHashCode());
            Assert.IsFalse(curl1.InternalEquals(curl3));
            Assert.AreNotEqual(curl1.InternalGetHashCode(), curl3.InternalGetHashCode());

            Assert.IsFalse(curl1.Equals(curl4)); // Not equal due to operand order.
            Assert.AreNotEqual(curl1.GetHashCode(), curl4.GetHashCode());
        }

        [TestMethod]
        public void Curl_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Vector vecF1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("fx"), new Symbol("fy"), new Symbol("fz")));
            Vector vecF2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("gx"), new Symbol("gy"), new Symbol("gz")));
            Vector vecVar = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"), new Symbol("z")));
            Curl originalCurl = new Curl(vecF1, vecVar);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(vecF2, vecVar);

            Operation newCurl = originalCurl.WithArguments(newArgs);

            Assert.IsInstanceOfType(newCurl, typeof(Curl));
            Assert.AreNotSame(originalCurl, newCurl);
            Assert.AreEqual((IExpression)vecF2, ((Curl)newCurl).VectorExpression);
            Assert.AreEqual((IExpression)vecVar, ((Curl)newCurl).VectorVariable);
        }
    }
}