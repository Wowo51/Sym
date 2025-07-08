using SymIO;
namespace SymIOTest
{
    [TestClass]
    public sealed class NumericalSimplificationTests
    {
        private CSharpIO? _symIO;

        [TestInitialize]
        public void TestInitialize()
        {
            _symIO = new CSharpIO();
        }

        // --- Multiplication Rules ---
        [TestMethod]
        public void Simplify_MultiplyConstants()
        {
            string expression = "x = 2 * 3";
            string expected = "x = 6";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_MultiplyMultipleConstants()
        {
            string expression = "x = 2 * 3 * 4";
            string expected = "x = 24";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_MultiplyConstantWithVariable()
        {
            string expression = "x = 2 * 3 * y";
            string expected = "x = 6 * y";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        // --- Addition Rules ---
        [TestMethod]
        public void Simplify_AddConstants()
        {
            string expression = "x = 4 + 5";
            string expected = "x = 9";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_AddMultipleConstants()
        {
            string expression = "x = 1 + 2 + 3";
            string expected = "x = 6";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_AddConstantsWithVariable()
        {
            string expression = "x = 1 + 2 + y";
            string expected = "x = 3 + y";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        // --- Mixed Rules ---
        [TestMethod]
        public void Simplify_MixedAdditionAndMultiplication()
        {
            string expression = "x = 2 + 3 * 4";
            string expected = "x = 14"; // 3 * 4 = 12; 2 + 12 = 14
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_ParenthesesEvaluation()
        {
            string expression = "x = (2 + 3) * 4";
            string expected = "x = 20";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Simplify_ExponentiationOfConstants()
        {
            string expression = "x = 2**3";
            string expected = "x = 8";
            string actual = _symIO!.Simplify(expression);
            Assert.AreEqual(expected, actual);
        }
    }
}
