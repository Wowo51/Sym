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
    public sealed class MatrixTests
    {
        [TestMethod]
        public void Matrix_Constructor_DimensionsAndComponents()
        {
            Number n1 = new Number(1m);
            Number n2 = new Number(2m);
            Number n3 = new Number(3m);
            Number n4 = new Number(4m);
            ImmutableList<IExpression> components = ImmutableList.Create<IExpression>(n1, n2, n3, n4);
            ImmutableArray<int> dimensions = ImmutableArray.Create(2, 2);

            Matrix matrix = new Matrix(dimensions, components);

            Assert.AreEqual(dimensions[0], matrix.MatrixDimensions[0]);
            Assert.AreEqual(dimensions[1], matrix.MatrixDimensions[1]);
            CollectionAssert.AreEqual(components.ToList(), matrix.Arguments.ToList());
            Assert.IsTrue(matrix.IsOperation);
            Assert.IsFalse(matrix.IsAtom);
        }

        [TestMethod]
        public void Matrix_Constructor_FromRowsOfVectors()
        {
            Number n1 = new Number(1m);
            Number n2 = new Number(2m);
            Number n3 = new Number(3m);
            Number n4 = new Number(4m);

            Vector row1 = new Vector(ImmutableList.Create<IExpression>(n1, n2));
            Vector row2 = new Vector(ImmutableList.Create<IExpression>(n3, n4));
            ImmutableList<Vector> rows = ImmutableList.Create<Vector>(row1, row2);

            Matrix matrix = new Matrix(rows);

            Assert.AreEqual(2, matrix.MatrixDimensions[0]);
            Assert.AreEqual(2, matrix.MatrixDimensions[1]);
            CollectionAssert.AreEqual(ImmutableList.Create<IExpression>(n1, n2, n3, n4).ToList(), matrix.Arguments.ToList());
        }

        [TestMethod]
        public void Matrix_Shape_ReturnsCorrectMatrixShape()
        {
            ImmutableList<IExpression> components = ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m), new Number(4m));
            Matrix matrix = new Matrix(ImmutableArray.Create(2, 2), components);
            Assert.AreEqual(new Shape(ImmutableArray.Create(2, 2)), matrix.Shape);
        }

        [TestMethod]
        public void Matrix_Shape_ReturnsErrorForInvalidDimensionsMismatchComponents()
        {
            ImmutableList<IExpression> components = ImmutableList.Create<IExpression>(new Number(1m), new Number(2m), new Number(3m)); // 3 components
            Matrix matrix = new Matrix(ImmutableArray.Create(2, 2), components); // but 2x2 = 4 expected
            Assert.AreEqual(Shape.Error, matrix.Shape);
        }

        [TestMethod]
        public void Matrix_Shape_ReturnsErrorForNonScalarComponents()
        {
            // Use a true non-scalar here, such as a Vector with scalar components.
            Vector nonScalarComponent = new Vector(ImmutableList.Create<IExpression>(new Number(1m), new Number(2m)));
            ImmutableList<IExpression> components = ImmutableList.Create<IExpression>(new Number(1m), nonScalarComponent, new Number(3m), new Number(4m));
            Matrix matrix = new Matrix(ImmutableArray.Create(2, 2), components);
            Assert.AreEqual(Shape.Error, matrix.Shape);
        }
        
        [TestMethod]
        public void Matrix_Shape_ReturnsErrorForInvalidRowConstruction()
        {
            Vector row1 = new Vector(ImmutableList.Create<IExpression>(new Number(1m), new Number(2m)));
            Vector row2 = new Vector(ImmutableList.Create<IExpression>(new Number(3m))); // Uneven row
            ImmutableList<Vector> rows = ImmutableList.Create<Vector>(row1, row2);

            Matrix matrix = new Matrix(rows);
            Assert.AreEqual(Shape.Error, matrix.Shape);
            Assert.AreEqual(0, matrix.MatrixDimensions[0]); // Should be marked invalid
        }

        [TestMethod]
        public void Matrix_ToDisplayString_ReturnsCorrectFormat()
        {
            Number n1 = new Number(1m);
            Number n2 = new Number(2m);
            Number n3 = new Number(3m);
            Number n4 = new Number(4m);
            ImmutableList<IExpression> components = ImmutableList.Create<IExpression>(n1, n2, n3, n4);
            Matrix matrix = new Matrix(ImmutableArray.Create(2, 2), components);
            Assert.AreEqual("Matrix(2x2)<[1, 2]; [3, 4]>", matrix.ToDisplayString());

            Matrix invalidMatrix = new Matrix(ImmutableArray.Create(2, 2), ImmutableList.Create<IExpression>(n1, n2, n3));
            Assert.AreEqual("Matrix(Invalid)", invalidMatrix.ToDisplayString());
        }

        [TestMethod]
        public void Matrix_Canonicalize_CanonicalizesComponents()
        {
            Number n1 = new Number(1m);
            Number n2 = new Number(2m);
            Add complexNum = new Add(ImmutableList.Create<IExpression>(new Number(3m), new Number(0m))); // Will canonicalize to 3
            Symbol s = new Symbol("s");

            ImmutableList<IExpression> components = ImmutableList.Create<IExpression>(n1, n2, complexNum, s);
            ImmutableArray<int> dimensions = ImmutableArray.Create(2, 2);
            Matrix inputMatrix = new Matrix(dimensions, components);

            IExpression canonical = inputMatrix.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Matrix));
            Matrix canonicalMatrix = (Matrix)canonical;
            Assert.AreNotSame(inputMatrix, canonicalMatrix); // Should be new instance because of canonicalization of component
            
            CollectionAssert.AreEqual(
                ImmutableList.Create<IExpression>(n1, n2, new Number(3m), s).ToList(),
                canonicalMatrix.Arguments.ToList()
            );
        }

        [TestMethod]
        public void Matrix_Canonicalize_ReturnsSelfIfNoChange()
        {
            Number n1 = new Number(1m);
            Number n2 = new Number(2m);
            Number n3 = new Number(3m);
            Number n4 = new Number(4m);

            ImmutableList<IExpression> components = ImmutableList.Create<IExpression>(n1, n2, n3, n4);
            ImmutableArray<int> dimensions = ImmutableArray.Create(2, 2);
            Matrix inputMatrix = new Matrix(dimensions, components);

            IExpression canonical = inputMatrix.Canonicalize();
            Assert.AreSame(inputMatrix, canonical); // Should return same instance if no component changed
        }

        [TestMethod]
        public void Matrix_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Number n1 = new Number(1m); Number n2 = new Number(2m);
            Number n3 = new Number(3m); Number n4 = new Number(4m);

            ImmutableList<IExpression> comp1 = ImmutableList.Create<IExpression>(n1, n2, n3, n4);
            ImmutableList<IExpression> comp2 = ImmutableList.Create<IExpression>(n1, n2, n3, n4); // Same components
            ImmutableList<IExpression> comp3 = ImmutableList.Create<IExpression>(n1, n2, n4, n3); // Different order
            ImmutableList<IExpression> comp4 = ImmutableList.Create<IExpression>(n1, n2, n3); // Different count

            Matrix mx1 = new Matrix(ImmutableArray.Create(2, 2), comp1);
            Matrix mx2 = new Matrix(ImmutableArray.Create(2, 2), comp2);
            Matrix mx3 = new Matrix(ImmutableArray.Create(2, 2), comp3); // Should not be equal due to component order
            Matrix mx4 = new Matrix(ImmutableArray.Create(2, 2), comp4); // Invalid matrix, should compare as different

            Assert.IsTrue(mx1.Equals(mx2));
            Assert.AreEqual(mx1.GetHashCode(), mx2.GetHashCode());
            Assert.IsTrue(mx1.InternalEquals(mx2));
            Assert.AreEqual(mx1.InternalGetHashCode(), mx2.InternalGetHashCode());

            Assert.IsFalse(mx1.Equals(mx3));
            Assert.AreNotEqual(mx1.GetHashCode(), mx3.GetHashCode());
            Assert.IsFalse(mx1.InternalEquals(mx3));
            Assert.AreNotEqual(mx1.InternalGetHashCode(), mx3.InternalGetHashCode());

            Assert.IsFalse(mx1.Equals(mx4)); // Shapes are different
            // Hash codes *could* be same by accident, but shouldn't be equal conceptually
            Assert.IsFalse(mx1.InternalEquals(mx4));
            Assert.AreNotEqual(mx1.GetHashCode(), mx4.GetHashCode()); // Due to invalid internal state
        }
        
        [TestMethod]
        public void Matrix_WithArguments_CreatesNewInstanceWithNewArguments()
        {
             Number n1 = new Number(1m); Number n2 = new Number(2m);
             Number n3 = new Number(3m); Number n4 = new Number(4m);
             ImmutableArray<int> dimensions = ImmutableArray.Create(2, 2);
             ImmutableList<IExpression> originalComponents = ImmutableList.Create<IExpression>(n1, n2, n3, n4);
             Matrix originalMatrix = new Matrix(dimensions, originalComponents);

             Number n5 = new Number(5m);
             ImmutableList<IExpression> newComponents = ImmutableList.Create<IExpression>(n1, n2, n3, n5); // Change last component
            
             Operation newMatrix = originalMatrix.WithArguments(newComponents);

             Assert.IsInstanceOfType(newMatrix, typeof(Matrix));
             Assert.AreNotSame(originalMatrix, newMatrix);
             CollectionAssert.AreEqual(newComponents.ToList(), ((Matrix)newMatrix).Arguments.ToList());
             Assert.AreEqual(dimensions, ((Matrix)newMatrix).MatrixDimensions); // Dimensions should be preserved
        }
    }
}