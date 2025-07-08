using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SymIO;

namespace SymIOTest
{
    [TestClass]
    public sealed class CSharpIOTests
    {
        private CSharpIO? _symIO;

        [TestInitialize]
        public void TestInitialize()
        {
            _symIO = new CSharpIO();
        }

        // --- Simplify Tests ---
        [TestMethod]
        public void Simplify_BasicAddition()
        {
            string expression = "x + x";
            string expected = "2 * x";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_ZeroMultiplication()
        {
            string expression = "y * 0";
            string expected = "0";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_IdentityMultiplication()
        {
            string expression = "(x + y) * 1";
            string expected = "x + y";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_FunctionCall_NoIntrinsicSimplification()
        {
            string expression = "Sin(Pi)";
            string expected = "Sin(Pi)"; // Expect it to remain as is, not simplified by Simplify method itself.
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_DivisionByZero_RemainsUnchanged()
        {
            string expression = "5 / 0";
            string expected = "(5 / 0)";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        // --- Solve Tests ---
        [TestMethod]
        public void Solve_LinearEquation()
        {
            string equation = "2 * x + 5 = 15";
            string variable = "x";
            string expected = "x = 5.0";
            string actual = _symIO!.Solve(equation, variable);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Solve_EquationWithOtherVariables()
        {
            string equation = "x + 2 * y = 10";
            string variable = "x";
            string expected = "x = 10 + -2 * y";
            string actual = _symIO!.Solve(equation, variable);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Differentiate_Constant()
        {
            string expression = "5";
            string variable = "x";
            string expected = "0";
            string actual = _symIO!.Differentiate(expression, variable);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Differentiate_MultiVariable()
        {
            string expression = "x * y + y * z";
            string variable = "x";
            string expected = "y";
            string actual = _symIO!.Differentiate(expression, variable);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Differentiate_ExpressionWithoutVariable()
        {
            string expression = "x**2 + y**2";
            string variable = "z";
            string expected = "0";
            string actual = _symIO!.Differentiate(expression, variable);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Integrate_Constant()
        {
            string expression = "5";
            string variable = "x";
            string expected = "5 * x";
            string actual = _symIO!.Integrate(expression, variable);
            Assert.AreEqual(expected, actual);
        }

        // --- Grad Tests --

        [TestMethod]
        public void Grad_ScalarField_Constant()
        {
            string scalarExpression = "5";
            string vectorVariable = "Vector(x,y)";
            string expected = "Vector(0, 0)";
            string actual = _symIO!.Grad(scalarExpression, vectorVariable);
            Assert.AreEqual(expected, actual);
        }

        // --- Curl Tests ---

        [TestMethod]
        public void Curl_ZeroVectorField()
        {
            string vectorExpression = "Vector(0, 0, 0)";
            string vectorVariable = "Vector(x,y,z)";
            string expected = "Vector(0, 0, 0)";
            string actual = _symIO!.Curl(vectorExpression, vectorVariable);
            Assert.AreEqual(expected, actual);
        }

        // --- Error/Invalid Input Tests (Parsing Errors) ---
        [TestMethod]
        public void Simplify_MalformedExpression_ThrowsArgumentException()
        {
            string expression = "x + "; // Incomplete expression
            Assert.ThrowsException<ArgumentException>(() => _symIO!.Simplify(expression));
        }

        [TestMethod]
        public void Solve_MalformedEquation_ThrowsArgumentException()
        {
            string equation = "2 * x + = 15";
            string variable = "x";
            Assert.ThrowsException<ArgumentException>(() => _symIO!.Solve(equation, variable));
        }

        [TestMethod]
        public void Differentiate_MalformedExpression_ThrowsArgumentException()
        {
            string expression = "Sin("; // Incomplete function call
            string variable = "x";
            Assert.ThrowsException<ArgumentException>(() => _symIO!.Differentiate(expression, variable));
        }

        [TestMethod]
        public void Integrate_MalformedExpression_ThrowsArgumentException()
        {
            string expression = "* x"; // Starts with operator
            string variable = "x";
            Assert.ThrowsException<ArgumentException>(() => _symIO!.Integrate(expression, variable));
        }

        [TestMethod]
        public void Grad_MalformedScalarExpression_ThrowsArgumentException()
        {
            string scalarExpression = "x * "; // Incomplete
            string vectorVariable = "Vector(x,y,z)";
            Assert.ThrowsException<ArgumentException>(() => _symIO!.Grad(scalarExpression, vectorVariable));
        }

        [TestMethod]
        public void Div_MalformedVectorExpression_ThrowsArgumentException()
        {
            string vectorExpression = "Vector(x, y,"; // Incomplete vector
            string vectorVariable = "Vector(x,y,z)";
            Assert.ThrowsException<ArgumentException>(() => _symIO!.Div(vectorExpression, vectorVariable));
        }

        [TestMethod]
        public void Curl_MalformedVectorExpression_ThrowsArgumentException()
        {
            string vectorExpression = "Vector(x, "; // Incomplete vector
            string vectorVariable = "Vector(x,y,z)";
            Assert.ThrowsException<ArgumentException>(() => _symIO!.Curl(vectorExpression, vectorVariable));
        }
    }
}