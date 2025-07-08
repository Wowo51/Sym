//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Core.Strategies;
using Sym.Operations;
using Sym.Core.Rewriters;
using System.Collections.Immutable;
using Sym.Atoms;

namespace Sym.Test.Strategies
{
    [TestClass]
    public class EquationSolverStrategyTests
    {
        private static ImmutableList<Rule>? generalAndEquationSolvingRules;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            // The LLM should define a comprehensive set of rules here, including general simplification
            // and rules/tactics necessary for solving linear algebraic equations (e.g., inverse operations).
            generalAndEquationSolvingRules = ImmutableList.Create<Rule>
            (
                 // Basic arithmetic simplification rules
                 new Rule(new Add(new Wild("x"), new Number(0)), new Wild("x")),
                 new Rule(new Multiply(new Wild("x"), new Number(1)), new Wild("x")),
                 new Rule(new Multiply(new Wild("x"), new Number(0)), new Number(0)),
                 new Rule(new Power(new Wild("x"), new Number(1)), new Wild("x")),
                 
                 // Combining like terms (example)
                 new Rule(new Add(new Multiply(new Wild("n1"), new Wild("x")), new Multiply(new Wild("n2"), new Wild("x"))),
                          new Multiply(new Add(new Wild("n1"), new Wild("n2")), new Wild("x"))),
                 
                 // Example of rule for isolating. LLM needs to generate appropriate rules here.
                 // This could be expressed as rules or as specific logic within the strategy.
                 // e.g., if (Add (Wild x) (Wild n)) matches LeftOperand, then RightOperand becomes (Subtract currentRight (Wild n))
                 // This is a placeholder; actual rules for solving will be more advanced.
                
                 // A*X + B = C  =>  A*X = C - B
                 new Rule(new Equality(new Add(new Multiply(new Wild("A"), new Wild("X")), new Wild("B")), new Wild("C")),
                          new Equality(new Multiply(new Wild("A"), new Wild("X")), new Subtract(new Wild("C"), new Wild("B")))),
                 
                 // A*X - B = C  =>  A*X = C + B
                 new Rule(new Equality(new Subtract(new Multiply(new Wild("A"), new Wild("X")), new Wild("B")), new Wild("C")),
                          new Equality(new Multiply(new Wild("A"), new Wild("X")), new Add(new Wild("C"), new Wild("B")))),

                 // A*X = B  => X = B / A
                 new Rule(new Equality(new Multiply(new Wild("A"), new Wild("X")), new Wild("B")),
                          new Equality(new Wild("X"), new Divide(new Wild("B"), new Wild("A"))))

            );
        }

        [TestMethod]
        public void EquationSolverStrategy_SolvesBasicLinearAddition()
        {
            var problem = new Equality(new Add(new Symbol("x"), new Number(2)), new Number(5)); // x + 2 = 5
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Equality(new Symbol("x"), new Number(3)), result.ResultExpression);
        }

        [TestMethod]
        public void EquationSolverStrategy_SolvesBasicLinearMultiplication()
        {
            var problem = new Equality(new Multiply(new Number(2), new Symbol("x")), new Number(6)); // 2 * x = 6
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Equality(new Symbol("x"), new Number(3)), result.ResultExpression);
        }

        [TestMethod]
        public void EquationSolverStrategy_SolvesLinearEquationMultiStep()
        {
            var problem = new Equality(new Add(new Multiply(new Number(2), new Symbol("x")), new Number(1)), new Number(7)); // 2x + 1 = 7
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Equality(new Symbol("x"), new Number(3)), result.ResultExpression);
        }

        [TestMethod]
        public void EquationSolverStrategy_HandlesVariableOnRightSide()
        {
            var problem = new Equality(new Number(5), new Add(new Symbol("x"), new Number(2))); // 5 = x + 2
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Equality(new Symbol("x"), new Number(3)), result.ResultExpression);
        }

        [TestMethod]
        public void EquationSolverStrategy_HandlesNoSolution()
        {
            var problem = new Equality(new Symbol("x"), new Add(new Symbol("x"), new Number(1))); // x = x + 1
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message, "No further progress achievable.");
        }

        [TestMethod]
        public void EquationSolverStrategy_HandlesNonEqualityProblem()
        {
            var problem = new Add(new Symbol("x"), new Number(2)); // Not an Equality
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("EquationSolverStrategy requires an Equality expression as input.", result.Message);
        }

        [TestMethod]
        public void EquationSolverStrategy_RequiresTargetVariable()
        {
            var problem = new Equality(new Add(new Symbol("x"), new Number(2)), new Number(5));
            var context = new SolveContext(null, generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message, "Target variable must be specified");
        }

        [TestMethod]
        public void EquationSolverStrategy_HandlesTargetVariableNotInProblem()
        {
            var problem = new Equality(new Symbol("y"), new Number(5));
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message, "Target variable 'x' could not be isolated");
        }

        [TestMethod]
        public void EquationSolverStrategy_ReachesMaxIterations()
        {
            var problem = new Equality(new Add(new Multiply(new Number(2), new Symbol("x")), new Number(1)), new Number(7));
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 1, false, null); // Limited iterations
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message, "Max iterations");
        }

        [TestMethod]
        public void EquationSolverStrategy_EnablesTracingCorrectly()
        {
            var problem = new Equality(new Add(new Multiply(new Number(2), new Symbol("x")), new Number(1)), new Number(7));
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, true, null); // Enable tracing
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsNotNull(result.Trace);
            Assert.IsTrue(result.Trace.Count >= 3, $"Expected at least 3 trace steps, but got {result.Trace.Count}.");
            Assert.AreEqual(problem, result.Trace[0]);
            Assert.AreEqual(new Equality(new Symbol("x"), new Number(3)), result.ResultExpression);
        }

        [TestMethod]
        public void EquationSolverStrategy_HandlesNullProblem()
        {
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();

            var result = strategy.Solve(null, context);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.ResultExpression);
            Assert.AreEqual("EquationSolverStrategy requires an Equality expression as input.", result.Message);
        }

        [TestMethod]
        public void EquationSolverStrategy_HandlesComplexExpressionRequiresSimplificationBeforeIsolation()
        {
            // Test: 3 * (x + 2) + 4 = 25  =>  3x + 6 + 4 = 25  =>  3x + 10 = 25  =>  3x = 15  => x = 5
            var problem = new Equality(
                new Add(new Multiply(new Number(3), new Add(new Symbol("x"), new Number(2))), new Number(4)),
                new Number(25)
            );
            var context = new SolveContext(new Symbol("x"), generalAndEquationSolvingRules, 100, false, null);
            var strategy = new EquationSolverStrategy();
            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Equality(new Symbol("x"), new Number(5)), result.ResultExpression);
        }
    }
}