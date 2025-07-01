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
    public sealed class MatrixMultiplyTests
    {
        [TestMethod]
        public void MatrixMultiply_ConstructorAndProperties_SetsCorrectly()
        {
            Matrix matrix = new Matrix(ImmutableArray.Create(2,2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m)));
            Vector vector = new Vector(ImmutableList.Create<IExpression>(new Number(5m), new Number(6m)));
            MatrixMultiply matMul = new MatrixMultiply(matrix, vector);

            Assert.AreEqual((IExpression)matrix, matMul.LeftOperand);
            Assert.AreEqual((IExpression)vector, matMul.RightOperand);
            Assert.AreEqual(2, matMul.Arguments.Count);
            Assert.IsTrue(matMul.IsOperation);
            Assert.IsFalse(matMul.IsAtom);
        }
        
        [TestMethod]
        public void MatrixMultiply_Shape_ReturnsCorrectMatrixShapeForMatrixMatrixProduct()
        {
            Matrix A = new Matrix(ImmutableArray.Create(2, 3), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m), new Number(5m), new Number(6m)));
            Matrix B = new Matrix(ImmutableArray.Create(3, 2), ImmutableList.Create<IExpression>(new Number(7m), new Number(8m), new Number(9m), new Number(10m), new Number(11m), new Number(12m)));
            MatrixMultiply matMul = new MatrixMultiply(A, B);
            Assert.AreEqual(new Shape(ImmutableArray.Create(2, 2)), matMul.Shape); // (2x3) * (3x2) = (2x2)
        }

        [TestMethod]
        public void MatrixMultiply_Shape_ReturnsCorrectVectorShapeForMatrixVectorProduct()
        {
            Matrix matrix = new Matrix(ImmutableArray.Create(2, 2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m)));
            Vector vector = new Vector(ImmutableList.Create<IExpression>(new Number(5m), new Number(6m)));
            MatrixMultiply matMul = new MatrixMultiply(matrix, vector);
            Assert.AreEqual(new Shape(ImmutableArray.Create(2)), matMul.Shape); // (2x2) * (2,) = (2,)
        }

        [TestMethod]
        public void MatrixMultiply_Shape_ReturnsCorrectVectorShapeForVectorMatrixProduct()
        {
            Vector vector = new Vector(ImmutableList.Create<IExpression>(new Number(1m), new Number(2m))); // (2,) vector treated as (1x2)
            Matrix matrix = new Matrix(ImmutableArray.Create(2, 3), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m), new Number(5m), new Number(6m)));
            MatrixMultiply matMul = new MatrixMultiply(vector, matrix);
            Assert.AreEqual(new Shape(ImmutableArray.Create(3)), matMul.Shape); // (2,) * (2x3) ~= (1x2) * (2x3) = (1x3) -> Shape(3)
        }

        [TestMethod]
        public void MatrixMultiply_Shape_ReturnsOperandsShapeIfOneIsScalar()
        {
            Number scalar = new Number(5m);
            Matrix matrix = new Matrix(ImmutableArray.Create(2, 2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m)));
            
            MatrixMultiply scalarMatrix = new MatrixMultiply(scalar, matrix);
            Assert.AreEqual(matrix.Shape, scalarMatrix.Shape);

            MatrixMultiply matrixScalar = new MatrixMultiply(matrix, scalar);
            Assert.AreEqual(matrix.Shape, matrixScalar.Shape);
        }

        [TestMethod]
        public void MatrixMultiply_Shape_ReturnsErrorForIncompatibleDimensions()
        {
            Matrix A_2x2 = new Matrix(ImmutableArray.Create(2, 2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m)));
            Matrix B_3x2 = new Matrix(ImmutableArray.Create(3, 2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m), new Number(5m), new Number(6m)));
            MatrixMultiply matMul = new MatrixMultiply(A_2x2, B_3x2); // (2x2) * (3x2) - inner dimensions mismatch
            Assert.AreEqual(Shape.Error, matMul.Shape);

            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Number(1m), new Number(2m)));
            Vector vec3 = new Vector(ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m)));
            MatrixMultiply vecVec = new MatrixMultiply(vec2, vec3); // Vector-Vector should be DotProduct, not MatrixMultiply here
            Assert.AreEqual(Shape.Error, vecVec.Shape);
        }

        [TestMethod]
        public void MatrixMultiply_ToDisplayString_ReturnsCorrectFormat()
        {
            Matrix matrix = new Matrix(ImmutableArray.Create(2,2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m)));
            Vector vector = new Vector(ImmutableList.Create<IExpression>(new Number(5m), new Number(6m)));
            MatrixMultiply matMul = new MatrixMultiply(matrix, vector);
            Assert.AreEqual("MatrixMultiply(Matrix(2x2)<[1, 2]; [3, 4]>, Vector(5, 6))", matMul.ToDisplayString());
        }

        [TestMethod]
        public void MatrixMultiply_Canonicalize_CanonicalizesOperands()
        {
            Number n1 = new Number(1m);
            Number n2 = new Number(2m);
            Add complexN3 = new Add(ImmutableList.Create<IExpression>(new Number(3m), new Number(0m))); // Canonicalizes to 3
            Number n4 = new Number(4m);

            Matrix left = new Matrix(ImmutableArray.Create(2,2), ImmutableList.Create<IExpression>(n1, n2, complexN3, n4));
            Vector right = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Add(ImmutableList.Create<IExpression>(new Symbol("y"), new Number(0m)))));

            MatrixMultiply input = new MatrixMultiply(left, right);
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(MatrixMultiply));
            MatrixMultiply canonicalMatMul = (MatrixMultiply)canonical;
            
            Assert.AreEqual((IExpression)new Matrix(ImmutableArray.Create(2,2), ImmutableList.Create<IExpression>(n1, n2, new Number(3m), n4)), canonicalMatMul.LeftOperand);
            Assert.AreEqual((IExpression)new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y"))), canonicalMatMul.RightOperand);
        }

        [TestMethod]
        public void MatrixMultiply_Canonicalize_PerformsNumericalScalarProduct()
        {
            Number num1 = new Number(5m);
            Number num2 = new Number(10m);
            MatrixMultiply input = new MatrixMultiply(num1, num2); // Valid for scalar*scalar
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)new Number(50m), canonical);
        }

        [TestMethod]
        public void MatrixMultiply_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Matrix matrix1 = new Matrix(ImmutableArray.Create(1,1), ImmutableList.Create<IExpression>(new Number(1m)));
            Vector vector1 = new Vector(ImmutableList.Create<IExpression>(new Number(2m)));

            MatrixMultiply mm1 = new MatrixMultiply(matrix1, vector1);
            MatrixMultiply mm2 = new MatrixMultiply(matrix1, vector1);
            MatrixMultiply mm3 = new MatrixMultiply(vector1, matrix1); // Order matters
            MatrixMultiply mm4 = new MatrixMultiply(matrix1, new Vector(ImmutableList.Create<IExpression>(new Number(3m))));

            Assert.IsTrue(mm1.Equals(mm2));
            Assert.AreEqual(mm1.GetHashCode(), mm2.GetHashCode());
            Assert.IsTrue(mm1.InternalEquals(mm2));
            Assert.AreEqual(mm1.InternalGetHashCode(), mm2.InternalGetHashCode());

            Assert.IsFalse(mm1.Equals(mm3));
            Assert.AreNotEqual(mm1.GetHashCode(), mm3.GetHashCode());
            Assert.IsFalse(mm1.InternalEquals(mm3));
            Assert.AreNotEqual(mm1.InternalGetHashCode(), mm3.InternalGetHashCode());

            Assert.IsFalse(mm1.Equals(mm4));
            Assert.AreNotEqual(mm1.GetHashCode(), mm4.GetHashCode());
        }

        [TestMethod]
        public void MatrixMultiply_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Matrix matrix = new Matrix(ImmutableArray.Create(2,2), ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m)));
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(new Number(5m), new Number(6m)));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Number(7m), new Number(8m)));
            MatrixMultiply originalMatMul = new MatrixMultiply(matrix, vec1);
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(matrix, vec2);
            
            Operation newMatMul = originalMatMul.WithArguments(newArgs);

            Assert.IsInstanceOfType(newMatMul, typeof(MatrixMultiply));
            Assert.AreNotSame(originalMatMul, newMatMul);
            Assert.AreEqual((IExpression)matrix, ((MatrixMultiply)newMatMul).LeftOperand);
            Assert.AreEqual((IExpression)vec2, ((MatrixMultiply)newMatMul).RightOperand);
        }
    }
}