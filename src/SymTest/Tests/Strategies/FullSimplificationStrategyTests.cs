//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Core.Strategies;
using Sym.Operations;
using Sym.Core.Rewriters;
using System.Collections.Immutable;
using System.Linq;
using Sym.Atoms;

namespace Sym.Test.Strategies
{
    [TestClass]
    public class FullSimplificationStrategyTests
    {
        // Define some general simplification rules for testing
        private static ImmutableList<Rule> generalSimplificationRules = ImmutableList.Create<Rule>
        (
             // Basic arithmetic
             new Rule(new Add(new Wild("x"), new Number(0)), new Wild("x")), // x + 0 = x
             new Rule(new Multiply(new Wild("x"), new Number(1)), new Wild("x")), // x * 1 = x
             new Rule(new Multiply(new Wild("x"), new Number(0)), new Number(0)), // x * 0 = 0
             new Rule(new Power(new Wild("x"), new Number(0)), new Number(1)),   // x ^ 0 = 1 (for x != 0)
             new Rule(new Power(new Wild("x"), new Number(1)), new Wild("x")),   // x ^ 1 = x
             
             // Combine like terms (simple example)
             new Rule(new Add(new Multiply(new Wild("n1"), new Wild("x")), new Multiply(new Wild("n2"), new Wild("x"))), 
                      new Multiply(new Add(new Wild("n1"), new Wild("n2")), new Wild("x")))
        );

        [TestMethod]
        public void FullSimplificationStrategy_ImplementsISolverStrategy()
        {
            ISolverStrategy strategy = new FullSimplificationStrategy();
            Assert.IsNotNull(strategy);
        }

        [TestMethod]
        public void FullSimplificationStrategy_SimplifiesBasicExpressions()
        {
            var problem = new Add(new Symbol("x"), new Number(0)); // x + 0
            var context = new SolveContext(null, generalSimplificationRules, 100, false, null);
            var strategy = new FullSimplificationStrategy();

            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Symbol("x"), result.ResultExpression);
        }

        [TestMethod]
        public void FullSimplificationStrategy_SimplifiesWithMultipleRules()
        {
            var problem = new Add(new Add(new Symbol("x"), new Number(0)), new Multiply(new Symbol("y"), new Number(1))); // (x + 0) + (y * 1)
            var context = new SolveContext(null, generalSimplificationRules, 100, false, null);
            var strategy = new FullSimplificationStrategy();

            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Add(new Symbol("x"), new Symbol("y")), result.ResultExpression);
        }

        [TestMethod]
        public void FullSimplificationStrategy_HandlesAlreadySimplifiedExpressions()
        {
            var problem = new Add(new Symbol("x"), new Symbol("y"));
            var context = new SolveContext(null, generalSimplificationRules, 100, false, null);
            var strategy = new FullSimplificationStrategy();

            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(problem, result.ResultExpression);
        }

        [TestMethod]
        public void FullSimplificationStrategy_ReachesMaxIterationsWarning()
        {
            var complexProblem = new Multiply(new Add(new Symbol("x"), new Number(0)), new Number(1)); // (x + 0) * 1
            var minimalIterationContext = new SolveContext(null, generalSimplificationRules, 1, false, null); // Only 1 iteration allowed
            
            var complexStrategy = new FullSimplificationStrategy();
            var complexResult = complexStrategy.Solve(complexProblem, minimalIterationContext);

            Assert.IsFalse(complexResult.IsSuccess, "Should fail if max iterations reached.");
            StringAssert.Contains(complexResult.Message, "Max iterations (1) reached");
            Assert.AreEqual(new Multiply(new Symbol("x"), new Number(1)), complexResult.ResultExpression);
        }

        [TestMethod]
        public void FullSimplificationStrategy_EnablesTracingCorrectly()
        {
            var problem = new Multiply(new Add(new Symbol("x"), new Number(0)), new Number(1)); // (x + 0) * 1
            var context = new SolveContext(null, generalSimplificationRules, 100, true, null); // Enable tracing
            var strategy = new FullSimplificationStrategy();

            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsNotNull(result.Trace);
            Assert.AreEqual(3, result.Trace.Count); // Original + 2 steps
            Assert.AreEqual(problem, result.Trace[0]);
            Assert.AreEqual(new Multiply(new Symbol("x"), new Number(1)), result.Trace[1]);
            Assert.AreEqual(new Symbol("x"), result.Trace[2]);
        }

        [TestMethod]
        public void FullSimplificationStrategy_HandlesNullProblem()
        {
            var context = new SolveContext(null, generalSimplificationRules, 100, false, null);
            var strategy = new FullSimplificationStrategy();

            var result = strategy.Solve(null, context);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.ResultExpression);
            Assert.AreEqual("Problem expression cannot be null.", result.Message);
        }

        [TestMethod]
        public void FullSimplificationStrategy_HandlesEmptyRuleSet()
        {
            var problem = new Add(new Symbol("x"), new Number(0));
            var context = new SolveContext(null, ImmutableList<Rule>.Empty, 100, false, null);
            var strategy = new FullSimplificationStrategy();

            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(problem, result.ResultExpression, "Expression should remain unchanged with empty rule set.");
            Assert.AreEqual("Simplification completed successfully.", result.Message);
        }

        [TestMethod]
        public void FullSimplificationStrategy_CombinesLikeTerms()
        {
            var problem = new Add(new Multiply(new Number(2), new Symbol("x")), new Multiply(new Number(3), new Symbol("x"))); // 2x + 3x
            var context = new SolveContext(null, generalSimplificationRules, 100, false, null);
            var strategy = new FullSimplificationStrategy();

            var result = strategy.Solve(problem, context);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(new Multiply(new Number(5), new Symbol("x")), result.ResultExpression);
        }
    }
}