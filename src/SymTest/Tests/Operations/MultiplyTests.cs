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
    public sealed class MultiplyTests
    {
        [TestMethod]
        public void Multiply_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            ImmutableList<IExpression> args = ImmutableList.Create<IExpression>(x, y);
            Multiply mul = new Multiply(args);

            Assert.AreEqual(2, mul.Arguments.Count);
            CollectionAssert.AreEqual(args.ToList(), mul.Arguments.ToList());
            Assert.IsTrue(mul.IsOperation);
            Assert.IsFalse(mul.IsAtom);
        }

        [TestMethod]
        public void Multiply_Shape_ReturnsCorrectScalarShape()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Multiply mul = new Multiply(ImmutableList.Create<IExpression>(x, five));
            Assert.AreEqual(Shape.Scalar, mul.Shape);
        }

        [TestMethod]
        public void Multiply_Shape_ReturnsCorrectVectorShapeForElementWiseProduct()
        {
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x1"), new Symbol("y1")));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x2"), new Symbol("y2")));
            Multiply mul = new Multiply(ImmutableList.Create<IExpression>(vec1, vec2));
            Assert.AreEqual(new Shape(ImmutableArray.Create(2)), mul.Shape);
        }

        [TestMethod]
        public void Multiply_Shape_ReturnsErrorForIncompatibleShapes()
        {
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Vector vec3 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b"), new Symbol("c")));
            Multiply mul = new Multiply(ImmutableList.Create<IExpression>(vec2, vec3));
            Assert.AreEqual(Shape.Error, mul.Shape);
        }

        [TestMethod]
        public void Multiply_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Multiply mul = new Multiply(ImmutableList.Create<IExpression>(x, five));
            Assert.AreEqual("(x * 5)", mul.ToDisplayString());

            Multiply nestedMul = new Multiply(ImmutableList.Create<IExpression>(x, new Multiply(ImmutableList.Create<IExpression>(new Symbol("y"), new Symbol("z")))));
            Assert.AreEqual("(x * (y * z))", nestedMul.ToDisplayString());
        }

        [TestMethod]
        public void Multiply_Canonicalize_FlattensAndSortsArguments()
        {
            // (a * (c * b)) * d * 1 -> (a * b * c * d)
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Symbol c = new Symbol("c");
            Symbol d = new Symbol("d");
            Number one = new Number(1m);

            Multiply innerMul = new Multiply(ImmutableList.Create<IExpression>(c, b));
            Multiply outerMul1 = new Multiply(ImmutableList.Create<IExpression>(a, innerMul));
            Multiply input = new Multiply(ImmutableList.Create<IExpression>(outerMul1, d, one));

            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Multiply));
            Multiply canonicalMul = (Multiply)canonical;

            Assert.AreEqual(4, canonicalMul.Arguments.Count);
            // After flattening and sorting: a, b, c, d
            Assert.AreEqual((IExpression)a, canonicalMul.Arguments[0]);
            Assert.AreEqual((IExpression)b, canonicalMul.Arguments[1]);
            Assert.AreEqual((IExpression)c, canonicalMul.Arguments[2]);
            Assert.AreEqual((IExpression)d, canonicalMul.Arguments[3]);
        }

        [TestMethod]
        public void Multiply_Canonicalize_MultipliesNumericTerms()
        {
            Symbol x = new Symbol("x");
            Number num1 = new Number(2m);
            Number num2 = new Number(3m);
            Number num3 = new Number(4m);

            Multiply input = new Multiply(ImmutableList.Create<IExpression>(x, num1, num2, num3));
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Multiply));
            Multiply canonicalMul = (Multiply)canonical;

            // Arguments should be 24, x (after sorting, numbers before symbols)
            Assert.AreEqual(2, canonicalMul.Arguments.Count);
            Assert.AreEqual((IExpression)new Number(24m), canonicalMul.Arguments[0]);
            Assert.AreEqual((IExpression)x, canonicalMul.Arguments[1]);
        }
        
        [TestMethod]
        public void Multiply_Canonicalize_HandlesZeroProduct()
        {
            Symbol x = new Symbol("x");
            Number zero = new Number(0m);
            Number five = new Number(5m);
            Multiply input = new Multiply(ImmutableList.Create<IExpression>(x, zero, five));
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)new Number(0m), canonical);
        }

        [TestMethod]
        public void Multiply_Canonicalize_ReturnsSingleTermIfOnlyOneExists()
        {
            Symbol x = new Symbol("x");
            Multiply input = new Multiply(ImmutableList.Create<IExpression>(x));
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)x, canonical);
            
            Number five = new Number(5m);
            Multiply inputNum = new Multiply(ImmutableList.Create<IExpression>(five));
            IExpression canonicalNum = inputNum.Canonicalize();
            Assert.AreEqual((IExpression)five, canonicalNum);
        }

        [TestMethod]
        public void Multiply_Canonicalize_ReturnsOneIfNoTermsRemaining()
        {
            // If all terms simplify away to nothing or the product is 1
            Multiply input = new Multiply(ImmutableList.Create<IExpression>(new Number(1m)));
            Assert.AreEqual((IExpression)new Number(1m), input.Canonicalize());

            Multiply multipleOnes = new Multiply(ImmutableList.Create<IExpression>(new Number(1m), new Number(1m)));
            Assert.AreEqual((IExpression)new Number(1m), multipleOnes.Canonicalize());
        }

        [TestMethod]
        public void Multiply_Canonicalize_TransformsToDotProductWhenApplicable()
        {
            // Test case: vector * vector (DotProduct)
            Symbol s1 = new Symbol("s1");
            Symbol s2 = new Symbol("s2");
            Vector vecA = new Vector(ImmutableList.Create<IExpression>(s1, s2));
            Vector vecB = new Vector(ImmutableList.Create<IExpression>(s1, s2)); // Can be same for simplicity

            Multiply input = new Multiply(ImmutableList.Create<IExpression>(vecA, vecB));
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(DotProduct));
            DotProduct dotProd = (DotProduct)canonical;
            Assert.AreEqual((IExpression)vecA, dotProd.LeftOperand);
            Assert.AreEqual((IExpression)vecB, dotProd.RightOperand);
            Assert.AreEqual(Shape.Scalar, dotProd.Shape); // Dot product of two 2D vectors is scalar
        }

        [TestMethod]
        public void Multiply_Canonicalize_TransformsToMatrixMultiplyWhenApplicable()
        {
            // Test case: matrix * vector (MatrixMultiply)
            Number a = new Number(1m);
            Number b = new Number(2m);
            Number c = new Number(3m);
            Number d = new Number(4m);
            Number x = new Number(10m);
            Number y = new Number(20m);

            // A 2x2 matrix
            ImmutableList<IExpression> matrixComponents = ImmutableList.Create<IExpression>(a, b, c, d);
            Matrix matrix = new Matrix(ImmutableArray.Create(2, 2), matrixComponents);
            
            // A 2-component vector
            Vector vector = new Vector(ImmutableList.Create<IExpression>(x, y));

            Multiply input = new Multiply(ImmutableList.Create<IExpression>(matrix, vector));
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(MatrixMultiply));
            MatrixMultiply matMul = (MatrixMultiply)canonical;
            Assert.AreEqual((IExpression)matrix, matMul.LeftOperand);
            Assert.AreEqual((IExpression)vector, matMul.RightOperand);
            Assert.AreEqual(new Shape(ImmutableArray.Create(2)), matMul.Shape); // Product of 2x2 and 2x1 is 2x1 (represented as Shape(2))
        }

        [TestMethod]
        public void Multiply_Canonicalize_TransformsToDotProductWhenApplicableForMatrices()
        {
            // Test case: 1x2 matrix * 2x1 matrix (DotProduct - equivalent to vector dot product)
            Number a = new Number(1m);
            Number b = new Number(2m);
            Number x = new Number(10m);
            Number y = new Number(20m);

            Matrix matrix1x2 = new Matrix(ImmutableArray.Create(1, 2), ImmutableList.Create<IExpression>(a, b));
            Matrix matrix2x1 = new Matrix(ImmutableArray.Create(2, 1), ImmutableList.Create<IExpression>(x, y));

            Multiply input = new Multiply(ImmutableList.Create<IExpression>(matrix1x2, matrix2x1));
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(DotProduct));
            DotProduct dotProd = (DotProduct)canonical;
            Assert.AreEqual((IExpression)matrix1x2, dotProd.LeftOperand);
            Assert.AreEqual((IExpression)matrix2x1, dotProd.RightOperand);
            Assert.AreEqual(Shape.Scalar, dotProd.Shape);
        }

        [TestMethod]
        public void Multiply_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Multiply mul1 = new Multiply(ImmutableList.Create<IExpression>(a, b));
            Multiply mul2 = new Multiply(ImmutableList.Create<IExpression>(a, b));
            Multiply mul3 = new Multiply(ImmutableList.Create<IExpression>(b, a)); // Will canonicalize to same due to sorting
            Multiply mul4 = new Multiply(ImmutableList.Create<IExpression>(a, b, new Number(2m)));

            Assert.IsTrue(mul1.Equals(mul2));
            Assert.AreEqual(mul1.GetHashCode(), mul2.GetHashCode());
            Assert.IsTrue(mul1.InternalEquals(mul2)); // InternalEquals relies on sorted args after canonicalization
            Assert.AreEqual(mul1.InternalGetHashCode(), mul2.InternalGetHashCode());

            Assert.IsTrue(mul1.Equals(mul3)); // Canonical forms are equal
            Assert.AreEqual(mul1.GetHashCode(), mul3.GetHashCode());
            IExpression mul1Can = mul1.Canonicalize();
            IExpression mul3Can = mul3.Canonicalize();
            Assert.IsTrue(mul1Can.InternalEquals(mul3Can));
            Assert.AreEqual(mul1Can.InternalGetHashCode(), mul3Can.InternalGetHashCode());

            Assert.IsFalse(mul1.Equals(mul4));
            Assert.AreNotEqual(mul1.GetHashCode(), mul4.GetHashCode());
        }

        [TestMethod]
        public void Multiply_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Symbol z = new Symbol("z");
            Multiply originalMul = new Multiply(ImmutableList.Create<IExpression>(x, y));
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(x, y, z);
            
            Operation newMul = originalMul.WithArguments(newArgs);

            Assert.IsInstanceOfType(newMul, typeof(Multiply));
            Assert.AreNotSame(originalMul, newMul);
            CollectionAssert.AreEqual(newArgs.ToList(), ((Multiply)newMul).Arguments.ToList());
        }
    }
}