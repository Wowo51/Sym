//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Operations;
using System.Collections.Immutable;
using System;
using Sym.Atoms;

namespace Sym.Test
{
    [TestClass]
    public class ISolverStrategyTests
    {
        // Dummy implementation for testing the interface contract and its method signature.
        // This dummy assumes IExpression and SolveResult exist and can be instantiated.
        private class DummySolverStrategy : ISolverStrategy
        {
            public SolveResult Solve(IExpression? problem, SolveContext context)
            {
                if (problem == null)
                {
                    return SolveResult.Failure(null, "Problem cannot be null", null);
                }

                // Simulate a successful solve for a specific input, otherwise failure.
                if (problem is Symbol symbol && symbol.Name == "x" && context.MaxIterations > 0)
                {
                    // Assuming a simple solvable case: solve x to 5
                    return SolveResult.Success(new Number(5), "Solved 'x' to '5'", null);
                }
                else if (context.MaxIterations == 0)
                {
                    return SolveResult.Failure(problem, "Max iterations reached (dummy)", null);
                }

                return SolveResult.Failure(problem, "Dummy solver could not solve this problem.", null);
            }
        }

        [TestMethod]
        public void ISolverStrategy_Interface_CanBeImplemented()
        {
            // Verify that a class can implement the interface without compilation errors
            ISolverStrategy strategy = new DummySolverStrategy();
            Assert.IsNotNull(strategy, "ISolverStrategy implementation should not be null after instantiation.");
        }

        [TestMethod]
        public void ISolverStrategy_SolveMethod_CanBeCalledAndReturnsSolveResult_SuccessCase()
        {
            ISolverStrategy strategy = new DummySolverStrategy();
            
            // Arrange: Create dummy IExpression and SolveContext instances
            IExpression problem = new Symbol("x"); // Simulate a problem to solve
            SolveContext context = new SolveContext(null, ImmutableList<Rule>.Empty, 100, false, null);
            
            // Act
            SolveResult result = strategy.Solve(problem, context);
            
            // Assert
            Assert.IsNotNull(result, "Solve method must return a SolveResult.");
            Assert.IsTrue(result.IsSuccess, "Result should indicate success for solvable problem.");
            Assert.IsInstanceOfType(result.ResultExpression, typeof(Number), "Result expression should be a Number.");
            Assert.AreEqual(5, ((Number)result.ResultExpression).Value, "Result expression value should be 5.");
            Assert.AreEqual("Solved 'x' to '5'", result.Message, "Success message should match.");
            Assert.IsNull(result.Trace, "Trace should be null if not enabled/recorded.");
        }

        [TestMethod]
        public void ISolverStrategy_SolveMethod_CanBeCalledAndReturnsSolveResult_FailureCase()
        {
            ISolverStrategy strategy = new DummySolverStrategy();
            
            // Arrange: Create a problem that the dummy solver cannot solve
            IExpression problem = new Symbol("unsolvable"); 
            SolveContext context = new SolveContext(null, ImmutableList<Rule>.Empty, 100, false, null);
            
            // Act
            SolveResult result = strategy.Solve(problem, context);
            
            // Assert
            Assert.IsNotNull(result, "Solve method must return a SolveResult.");
            Assert.IsFalse(result.IsSuccess, "Result should indicate failure for unsolvable problem.");
            Assert.AreEqual(problem, result.ResultExpression, "Result expression should be the original problem on failure.");
            Assert.AreEqual("Dummy solver could not solve this problem.", result.Message, "Failure message should match.");
            Assert.IsNull(result.Trace, "Trace should be null if not enabled/recorded.");
        }

        [TestMethod]
        public void ISolverStrategy_SolveMethod_HandlesNullProblem()
        {
            ISolverStrategy strategy = new DummySolverStrategy();
            SolveContext context = new SolveContext(null, ImmutableList<Rule>.Empty, 100, false, null);

            // Act
            SolveResult result = strategy.Solve(null, context);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess, "Should fail if problem is null.");
            Assert.IsNull(result.ResultExpression, "ResultExpression should be null when problem is null.");
            Assert.AreEqual("Problem cannot be null", result.Message);
        }

        [TestMethod]
        public void ISolverStrategy_SolveMethod_HandlesMaxIterationsReached()
        {
            ISolverStrategy strategy = new DummySolverStrategy();
            IExpression problem = new Symbol("x");
            // Arrange context with 0 max iterations to trigger a simulated failure
            SolveContext context = new SolveContext(null, ImmutableList<Rule>.Empty, 0, false, null);

            // Act
            SolveResult result = strategy.Solve(problem, context);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess, "Should fail if MaxIterations is 0.");
            Assert.AreEqual("Max iterations reached (dummy)", result.Message);
            Assert.AreEqual(problem, result.ResultExpression);
        }
    }
}