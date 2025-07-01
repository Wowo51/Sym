//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using System.Collections.Immutable;
using System.Linq;

namespace SymTest
{
    [TestClass]
    public sealed class ShapeTests
    {
        [TestMethod]
        public void ScalarShape_PropertiesAreCorrect()
        {
            Shape scalar = Shape.Scalar;
            Assert.IsTrue(scalar.IsScalar);
            Assert.IsFalse(scalar.IsVector);
            Assert.IsFalse(scalar.IsMatrix);
            Assert.IsFalse(scalar.IsTensor);
            Assert.IsTrue(scalar.IsValid);
            Assert.IsTrue(scalar.Dimensions.IsEmpty);
            Assert.AreEqual("()", scalar.ToDisplayString());
        }

        [TestMethod]
        public void ErrorShape_PropertiesAreCorrect()
        {
            Shape error = Shape.Error;
            Assert.IsFalse(error.IsScalar);
            Assert.IsFalse(error.IsVector);
            Assert.IsFalse(error.IsMatrix);
            Assert.IsFalse(error.IsTensor);
            Assert.IsFalse(error.IsValid);
            Assert.IsTrue(error.Dimensions.IsEmpty);
            Assert.AreEqual("ErrorShape", error.ToDisplayString());
        }

        [TestMethod]
        public void VectorShape_PropertiesAreCorrect()
        {
            Shape vector = new Shape(ImmutableArray.Create(5));
            Assert.IsFalse(vector.IsScalar);
            Assert.IsTrue(vector.IsVector);
            Assert.IsFalse(vector.IsMatrix);
            Assert.IsFalse(vector.IsTensor);
            Assert.IsTrue(vector.IsValid);
            Assert.AreEqual(1, vector.Dimensions.Length);
            Assert.AreEqual(5, vector.Dimensions[0]);
            Assert.AreEqual("(5)", vector.ToDisplayString());
        }

        [TestMethod]
        public void MatrixShape_PropertiesAreCorrect()
        {
            Shape matrix = new Shape(ImmutableArray.Create(2, 3));
            Assert.IsFalse(matrix.IsScalar);
            Assert.IsFalse(matrix.IsVector);
            Assert.IsTrue(matrix.IsMatrix);
            Assert.IsFalse(matrix.IsTensor);
            Assert.IsTrue(matrix.IsValid);
            Assert.AreEqual(2, matrix.Dimensions.Length);
            Assert.AreEqual(2, matrix.Dimensions[0]);
            Assert.AreEqual(3, matrix.Dimensions[1]);
            Assert.AreEqual("(2, 3)", matrix.ToDisplayString());
        }

        [TestMethod]
        public void TensorShape_PropertiesAreCorrect()
        {
            Shape tensor = new Shape(ImmutableArray.Create(2, 3, 4));
            Assert.IsFalse(tensor.IsScalar);
            Assert.IsFalse(tensor.IsVector);
            Assert.IsFalse(tensor.IsMatrix);
            Assert.IsTrue(tensor.IsTensor);
            Assert.IsTrue(tensor.IsValid);
            Assert.AreEqual(3, tensor.Dimensions.Length);
            Assert.AreEqual("(2, 3, 4)", tensor.ToDisplayString());
        }

        [TestMethod]
        public void AreDimensionsCompatibleForElementWise_ScalarAndOther_AreCompatible()
        {
            Shape scalar = Shape.Scalar;
            Shape vector = new Shape(ImmutableArray.Create(3));
            Shape matrix = new Shape(ImmutableArray.Create(2, 2));

            Assert.IsTrue(scalar.AreDimensionsCompatibleForElementWise(vector));
            Assert.IsTrue(vector.AreDimensionsCompatibleForElementWise(scalar));
            Assert.IsTrue(scalar.AreDimensionsCompatibleForElementWise(matrix));
            Assert.IsTrue(matrix.AreDimensionsCompatibleForElementWise(scalar));
        }

        [TestMethod]
        public void AreDimensionsCompatibleForElementWise_SameNonScalarShapes_AreCompatible()
        {
            Shape vec3a = new Shape(ImmutableArray.Create(3));
            Shape vec3b = new Shape(ImmutableArray.Create(3));
            Shape mat2x2a = new Shape(ImmutableArray.Create(2, 2));
            Shape mat2x2b = new Shape(ImmutableArray.Create(2, 2));

            Assert.IsTrue(vec3a.AreDimensionsCompatibleForElementWise(vec3b));
            Assert.IsTrue(mat2x2a.AreDimensionsCompatibleForElementWise(mat2x2b));
        }

        [TestMethod]
        public void AreDimensionsCompatibleForElementWise_DifferentNonScalarShapes_AreNotCompatible()
        {
            Shape vec3 = new Shape(ImmutableArray.Create(3));
            Shape vec2 = new Shape(ImmutableArray.Create(2));
            Shape mat2x2 = new Shape(ImmutableArray.Create(2, 2));
            Shape mat2x3 = new Shape(ImmutableArray.Create(2, 3));

            Assert.IsFalse(vec3.AreDimensionsCompatibleForElementWise(vec2));
            Assert.IsFalse(vec3.AreDimensionsCompatibleForElementWise(mat2x2));
            Assert.IsFalse(mat2x2.AreDimensionsCompatibleForElementWise(mat2x3));
        }

        [TestMethod]
        public void AreDimensionsCompatibleForElementWise_ErrorShapeInvolved_AreNotCompatible()
        {
            Shape error = Shape.Error;
            Shape scalar = Shape.Scalar;
            Shape vector = new Shape(ImmutableArray.Create(3));

            Assert.IsFalse(error.AreDimensionsCompatibleForElementWise(scalar));
            Assert.IsFalse(scalar.AreDimensionsCompatibleForElementWise(error));
            Assert.IsFalse(error.AreDimensionsCompatibleForElementWise(vector));
            Assert.IsFalse(vector.AreDimensionsCompatibleForElementWise(error));
        }

        [TestMethod]
        public void CombineForElementWise_ScalarAndOther_ReturnsOtherShape()
        {
            Shape scalar = Shape.Scalar;
            Shape vector = new Shape(ImmutableArray.Create(3));
            Shape matrix = new Shape(ImmutableArray.Create(2, 2));

            Assert.AreEqual(vector, scalar.CombineForElementWise(vector));
            Assert.AreEqual(vector, vector.CombineForElementWise(scalar));
            Assert.AreEqual(matrix, scalar.CombineForElementWise(matrix));
            Assert.AreEqual(matrix, matrix.CombineForElementWise(scalar));
        }

        [TestMethod]
        public void CombineForElementWise_SameNonScalarShapes_ReturnsEitherShape()
        {
            Shape vec3a = new Shape(ImmutableArray.Create(3));
            Shape vec3b = new Shape(ImmutableArray.Create(3));
            Shape mat2x2a = new Shape(ImmutableArray.Create(2, 2));
            Shape mat2x2b = new Shape(ImmutableArray.Create(2, 2));

            Assert.AreEqual(vec3a, vec3a.CombineForElementWise(vec3b));
            Assert.AreEqual(mat2x2a, mat2x2a.CombineForElementWise(mat2x2b));
        }

        [TestMethod]
        public void CombineForElementWise_DifferentNonScalarShapes_ReturnsError()
        {
            Shape vec3 = new Shape(ImmutableArray.Create(3));
            Shape vec2 = new Shape(ImmutableArray.Create(2));
            Shape mat2x2 = new Shape(ImmutableArray.Create(2, 2));

            Assert.AreEqual(Shape.Error, vec3.CombineForElementWise(vec2));
            Assert.AreEqual(Shape.Error, vec3.CombineForElementWise(mat2x2));
        }

        [TestMethod]
        public void CombineForElementWise_ErrorShapeInvolved_ReturnsError()
        {
            Shape error = Shape.Error;
            Shape scalar = Shape.Scalar;
            Shape vector = new Shape(ImmutableArray.Create(3));

            Assert.AreEqual(Shape.Error, error.CombineForElementWise(scalar));
            Assert.AreEqual(Shape.Error, scalar.CombineForElementWise(error));
            Assert.AreEqual(Shape.Error, error.CombineForElementWise(vector));
            Assert.AreEqual(Shape.Error, vector.CombineForElementWise(error));
        }
    }
}