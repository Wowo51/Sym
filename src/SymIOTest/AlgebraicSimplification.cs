using SymIO;
namespace SymIOTest
{
    [TestClass]
    public sealed class AlgebraicSimplificationRuleTests
    {
        private CSharpIO? _symIO;


        [TestInitialize]
        public void TestInitialize()
        {
            _symIO = new CSharpIO();
        }

        // --- Subtraction Rules ---
        [TestMethod]
        public void Simplify_SubtractionIdentity()
        {
            string expression = "x - 0";
            string expected = "x";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_SubtractSelf()
        {
            string expression = "y - y";
            string expected = "0";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_SubtractFromZero()
        {
            string expression = "0 - z";
            string expected = "-1 * z";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_DoubleNegative()
        {
            // Tests -(-x) = x
            string expression = "-(-x)";
            string expected = "x";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        // --- Division Rules ---
        [TestMethod]
        public void Simplify_DivisionByIdentity()
        {
            string expression = "x / 1";
            string expected = "x";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_DivisionBySelf()
        {
            string expression = "y / y";
            string expected = "1";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_ZeroDividedByX()
        {
            string expression = "0 / x";
            string expected = "0";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_DivisionByNegativeOne()
        {
            string expression = "z / -1";
            string expected = "-1 * z";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_ReciprocalOfReciprocal()
        {
            string expression = "1 / (1 / x)";
            string expected = "x";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        // --- Power Rules ---
        [TestMethod]
        public void Simplify_PowerOfOne()
        {
            string expression = "1**x";
            string expected = "1";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_PowerOfZero()
        {
            string expression = "0**5";
            string expected = "0";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_PowerOfProduct()
        {
            string expression = "(x*y)**n";
            string expected = "x ** n * y ** n";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }
    }
}