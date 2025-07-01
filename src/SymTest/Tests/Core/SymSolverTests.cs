//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Core.Strategies;
using Sym.Operations;
using System.Collections.Immutable;
using Sym.Atoms;

namespace Sym.Test.Core
{
    [TestClass]
    public class SymSolverTests
    {
        // Dummy strategy for testing delegation, now correctly implements ISolverStrategy
        private class DummyStrategy : ISolverStrategy
        {
            private SolveResult _testResult;

            public DummyStrategy(SolveResult result)
            {
                _testResult = result;
            }

            public SolveResult Solve(IExpression? problem, SolveContext context)
            {
                return _testResult;
            }
        }

        private static ImmutableList<Rule> _testRules;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _testRules = ImmutableList.Create<Rule>
            (
                 // Basic arithmetic simplification rules
                 new Rule(new Add(new Wild("x"), new Number(0)), new Wild("x")), // x + 0 = x
                 new Rule(new Multiply(new Wild("x"), new Number(1)), new Wild("x")), // x * 1 = x
                 new Rule(new Multiply(new Wild("x"), new Number(0)), new Number(0)), // x * 0 = 0
                 new Rule(new Power(new Wild("x"), new Number(1)), new Wild("x")),   // x ^ 1 = x
                 
                 // Equation solving rules for testing convenience methods
                 new Rule(new Equality(new Add(new Multiply(new Wild("A"), new Wild("X")), new Wild("B")), new Wild("C")),
                          new Equality(new Multiply(new Wild("A"), new Wild("X")), new Subtract(new Wild("C"), new Wild("B")))),
                 new Rule(new Equality(new Multiply(new Wild("A"), new Wild("X")), new Wild("B")),
                          new Equality(new Wild("X"), new Divide(new Wild("B"), new Wild("A"))))
            );
        }

        [TestMethod]
        public void SymSolver_Solve_DelegatesCorrectly()
        {
            Symbol problem = new Symbol("a");
            Symbol expectedResultExpression = new Symbol("b");
            SolveResult mockResult = SolveResult.Success(expectedResultExpression, "Mock Success");
            ISolverStrategy mockStrategy = new DummyStrategy(mockResult);
            SolveContext context = new SolveContext(null, _testRules, 100, false, null);

            SolveResult result = SymSolver.Solve(problem, mockStrategy, context);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(expectedResultExpression, result.ResultExpression);
            Assert.AreEqual("Mock Success", result.Message);
        }

        [TestMethod]
        public void SymSolver_Solve_HandlesNullStrategy()
        {
            Symbol problem = new Symbol("a");
            SolveContext context = new SolveContext(null, _testRules, 100, false, null);

            SolveResult result = SymSolver.Solve(problem, null, context);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("ISolverStrategy cannot be null.", result.Message);
            Assert.AreEqual(problem, result.ResultExpression);
        }

        [TestMethod]
        public void SymSolver_Solve_HandlesNullContext()
        {
            Symbol problem = new Symbol("a");
            ISolverStrategy mockStrategy = new DummyStrategy(SolveResult.Success(new Symbol("b"), "Mock Success"));

            SolveResult result = SymSolver.Solve(problem, mockStrategy, null);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("SolveContext cannot be null.", result.Message);
            Assert.AreEqual(problem, result.ResultExpression);
        }

        [TestMethod]
        public void SymSolver_SolveEquation_DelegatesCorrectly()
        {
            Equality problem = new Equality(new Add(new Symbol("x"), new Number(2)), new Number(5)); // x + 2 = 5
            Symbol targetVar = new Symbol("x");
            // The actual EquationSolverStrategy is used here
            SolveResult result = SymSolver.SolveEquation(problem, targetVar, _testRules);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Equality(new Symbol("x"), new Number(3)), result.ResultExpression);
        }

        [TestMethod]
        public void SymSolver_SolveEquation_HandlesNullTargetVariable()
        {
            Equality problem = new Equality(new Symbol("x"), new Number(5));
            SolveResult result = SymSolver.SolveEquation(problem, null, _testRules);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Target variable cannot be null for SolveEquation.", result.Message);
            Assert.AreEqual(problem, result.ResultExpression);
        }

        [TestMethod]
        public void SymSolver_Simplify_DelegatesCorrectly()
        {
            Add problem = new Add(new Symbol("x"), new Number(0)); // x + 0
            // The actual FullSimplificationStrategy is used here
            SolveResult result = SymSolver.Simplify(problem, _testRules);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Symbol("x"), result.ResultExpression);
        }

        [TestMethod]
        public void SymSolver_Simplify_HandlesAlreadySimplifiedExpression()
        {
            Add problem = new Add(new Symbol("x"), new Symbol("y"));
            SolveResult result = SymSolver.Simplify(problem, _testRules);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(problem, result.ResultExpression);
        }
    }
}