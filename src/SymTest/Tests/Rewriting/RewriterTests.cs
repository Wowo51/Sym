//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Linq;
using System;
using System.Reflection;
using Sym.Core.Rewriters; // Added missing using directive

namespace SymTest
{
    [TestClass]
    public sealed class RewriterTests
    {
        // Helper method to create a simple rewrite rule for testing
        [TestMethod]
        public void TryMatch_MatchesSimpleAtom()
        {
            Number exp = new Number(5m);
            Number pattern = new Number(5m);
            MatchResult result = Rewriter.TryMatch(exp, pattern);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Bindings.IsEmpty);
        }

        [TestMethod]
        public void TryMatch_FailsOnDifferentAtomValue()
        {
            Number exp = new Number(5m);
            Number pattern = new Number(10m);
            MatchResult result = Rewriter.TryMatch(exp, pattern);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void TryMatch_MatchesWildAndBinds()
        {
            Number exp = new Number(10m);
            Wild pattern = new Wild("x");
            MatchResult result = Rewriter.TryMatch(exp, pattern);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Bindings.ContainsKey("x"));
            Assert.AreEqual((IExpression)exp, result.Bindings["x"]);
        }

        [TestMethod]
        public void TryMatch_MatchesWildWithExistingBinding()
        {
            Symbol exp1 = new Symbol("a");
            Symbol exp2 = new Symbol("a");
            Wild pattern1 = new Wild("x");
            Wild pattern2 = new Wild("x");
            // Build a manual initial binding for test purposes
            ImmutableDictionary<string, IExpression>.Builder bindingsBuilder = ImmutableDictionary.CreateBuilder<string, IExpression>();
            bindingsBuilder.Add("x", exp1);
            // Use the private TryMatchRecursive directly for this specific test
            bool success = InternalTryMatchRecursiveAccessor(exp2, pattern2, bindingsBuilder);
            Assert.IsTrue(success);
            Assert.AreEqual((IExpression)exp1, bindingsBuilder["x"]);
        }

        [TestMethod]
        public void TryMatch_FailsWildWithConflictingBinding()
        {
            Symbol exp1 = new Symbol("a");
            Symbol exp2 = new Symbol("b"); // Different value
            Wild pattern1 = new Wild("x");
            Wild pattern2 = new Wild("x");
            ImmutableDictionary<string, IExpression>.Builder bindingsBuilder = ImmutableDictionary.CreateBuilder<string, IExpression>();
            bindingsBuilder.Add("x", exp1);
            bool success = InternalTryMatchRecursiveAccessor(exp2, pattern2, bindingsBuilder);
            Assert.IsFalse(success);
            Assert.AreEqual((IExpression)exp1, bindingsBuilder["x"]); // Binding should remain unchanged if conflict
        }

        [TestMethod]
        public void TryMatch_MatchesOperationStructureAndBindsWilds()
        {
            // Test: (A + B) against (x + y)
            Symbol a = new Symbol("A");
            Symbol b = new Symbol("B");
            Add exp = new Add(ImmutableList.Create<IExpression>(a, b));
            Wild wildX = new Wild("x");
            Wild wildY = new Wild("y");
            Add pattern = new Add(ImmutableList.Create<IExpression>(wildX, wildY));
            MatchResult result = Rewriter.TryMatch(exp, pattern);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Bindings.Count);
            Assert.AreEqual((IExpression)a, result.Bindings["x"]);
            Assert.AreEqual((IExpression)b, result.Bindings["y"]);
        }

        [TestMethod]
        public void TryMatch_FailsOnOperationWithDifferentArgumentCount()
        {
            // Test: (A + B + C) against (x + y)
            Symbol a = new Symbol("A");
            Symbol b = new Symbol("B");
            Symbol c = new Symbol("C");
            Add exp = new Add(ImmutableList.Create<IExpression>(a, b, c));
            Wild wildX = new Wild("x");
            Wild wildY = new Wild("y");
            Add pattern = new Add(ImmutableList.Create<IExpression>(wildX, wildY));
            MatchResult result = Rewriter.TryMatch(exp, pattern);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void TryMatch_WildConstraint_Scalar()
        {
            Symbol scalarSym = new Symbol("x");
            Symbol vectorSym = new Symbol("v", new Shape(ImmutableArray.Create(3)));
            Number numExp = new Number(10m);
            Wild scalarWild = new Wild("s", WildConstraint.Scalar);
            Assert.IsTrue(Rewriter.TryMatch(scalarSym, scalarWild).Success);
            Assert.IsTrue(Rewriter.TryMatch(numExp, scalarWild).Success);
            Assert.IsFalse(Rewriter.TryMatch(vectorSym, scalarWild).Success); // Vector should not match scalar constraint
        }

        [TestMethod]
        public void TryMatch_WildConstraint_Constant()
        {
            Number numExp = new Number(10m);
            Symbol symExp = new Symbol("x");
            Wild constantWild = new Wild("c", WildConstraint.Constant);
            Assert.IsTrue(Rewriter.TryMatch(numExp, constantWild).Success);
            Assert.IsFalse(Rewriter.TryMatch(symExp, constantWild).Success); // Symbol is not a constant Atom
        }

        [TestMethod]
        public void Substitute_ReplacesWildsWithBindings()
        {
            // Original: x * y
            // Bindings: x -> A, y -> B
            // Expected: A * B
            Wild wildX = new Wild("x");
            Wild wildY = new Wild("y");
            Multiply replacementTemplate = new Multiply(ImmutableList.Create<IExpression>(wildX, wildY));
            ImmutableDictionary<string, IExpression> bindings = ImmutableDictionary.Create<string, IExpression>().Add("x", new Symbol("A")).Add("y", new Symbol("B"));
            IExpression substituted = Rewriter.Substitute(replacementTemplate, bindings);
            Assert.IsInstanceOfType(substituted, typeof(Multiply));
            Multiply resultMul = (Multiply)substituted;
            Assert.AreEqual(2, resultMul.Arguments.Count);
            Assert.AreEqual((IExpression)new Symbol("A"), resultMul.Arguments[0]);
            Assert.AreEqual((IExpression)new Symbol("B"), resultMul.Arguments[1]);
        }

        [TestMethod]
        public void Substitute_HandlesUnboundWilds_ReturnsOriginalWild()
        {
            // Original: x * y
            // Bindings: x -> A
            // Expected: A * y (y remains a wild)
            Wild wildX = new Wild("x");
            Wild wildY = new Wild("y");
            Multiply replacementTemplate = new Multiply(ImmutableList.Create<IExpression>(wildX, wildY));
            ImmutableDictionary<string, IExpression> bindings = ImmutableDictionary.Create<string, IExpression>().Add("x", new Symbol("A"));
            IExpression substituted = Rewriter.Substitute(replacementTemplate, bindings);
            Assert.IsInstanceOfType(substituted, typeof(Multiply));
            Multiply resultMul = (Multiply)substituted;
            Assert.AreEqual(2, resultMul.Arguments.Count);
            Assert.AreEqual((IExpression)new Symbol("A"), resultMul.Arguments[0]);
            Assert.AreEqual((IExpression)wildY, resultMul.Arguments[1]); // wildY should remain
        }

        [TestMethod]
        public void Substitute_HandlesNestedExpressions()
        {
            // f(x, g(y)) -> f(A, g(B))
            Wild wildX = new Wild("x");
            Wild wildY = new Wild("y");
            Function innerFunc = new Function("g", ImmutableList.Create<IExpression>(wildY));
            Function outerFunc = new Function("f", ImmutableList.Create<IExpression>(wildX, innerFunc));
            ImmutableDictionary<string, IExpression> bindings = ImmutableDictionary.Create<string, IExpression>().Add("x", new Symbol("A")).Add("y", new Symbol("B"));
            IExpression substituted = Rewriter.Substitute(outerFunc, bindings);
            Assert.IsInstanceOfType(substituted, typeof(Function));
            Function resultOuter = (Function)substituted;
            Assert.AreEqual("f", resultOuter.Name);
            Assert.AreEqual(2, resultOuter.Arguments.Count);
            Assert.AreEqual((IExpression)new Symbol("A"), resultOuter.Arguments[0]);
            Assert.IsInstanceOfType(resultOuter.Arguments[1], typeof(Function));
            Function resultInner = (Function)resultOuter.Arguments[1];
            Assert.AreEqual("g", resultInner.Name);
            Assert.AreEqual(1, resultInner.Arguments.Count);
            Assert.AreEqual((IExpression)new Symbol("B"), resultInner.Arguments[0]);
        }

        [TestMethod]
        public void Rewrite_AppliesSingleRuleAtTopLevel()
        {
            Rule rule = CreateAddZeroRule(); // Add(exp, 0) -> exp
            IExpression input = new Add(ImmutableList.Create<IExpression>(new Symbol("x"), new Number(0m)));
            IExpression expected = new Symbol("x");
            RewriterResult rewriteResult = Rewriter.RewriteFully(input, ImmutableList.Create<Rule>(rule));
            IExpression result = rewriteResult.RewrittenExpression;
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Rewrite_AppliesRuleRecursivelyIntoArguments()
        {
            // Input: (a + (b + 0)) * 1
            // Rules: Add(exp, 0) -> exp; Multiply(exp, 1) -> exp
            Rule addZeroRule = CreateAddZeroRule();
            Rule mulOneRule = CreateMulOneRule();
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Number zero = new Number(0m);
            Number one = new Number(1m);
            IExpression innerAdd = new Add(ImmutableList.Create<IExpression>(b, zero)); // b + 0
            IExpression outerAdd = new Add(ImmutableList.Create<IExpression>(a, innerAdd)); // a + (b + 0)
            IExpression input = new Multiply(ImmutableList.Create<IExpression>(outerAdd, one)); // (a + (b + 0)) * 1
            IExpression expected = new Add(ImmutableList.Create<IExpression>(a, b)).Canonicalize(); // Should simplify to (a + b)
            RewriterResult rewriteResult = Rewriter.RewriteFully(input, ImmutableList.Create<Rule>(addZeroRule, mulOneRule));
            IExpression result = rewriteResult.RewrittenExpression;
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Rewrite_AppliesMultipleRulesUntilNoChange()
        {
            // Input: x * (y + 0)
            // Rules: Add(exp, 0) -> exp; Distribute: a * (b + c) -> a*b + a*c;
            Rule addZeroRule = CreateAddZeroRule();
            Rule distributeRule = CreateDistributeMultiplyOverAddRule();
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Number zero = new Number(0m);
            IExpression input = new Multiply(ImmutableList.Create<IExpression>(x, new Add(ImmutableList.Create<IExpression>(y, zero))));
            // Step 1 (addZeroRule): x * (y + 0) -> x * y
            // Step 2 (distributeRule): won't apply to x * y as it's not x * (y+z)
            IExpression expected = new Multiply(ImmutableList.Create<IExpression>(x, y)).Canonicalize();
            RewriterResult rewriteResult = Rewriter.RewriteFully(input, ImmutableList.Create<Rule>(addZeroRule, distributeRule));
            IExpression result = rewriteResult.RewrittenExpression;
            Assert.AreEqual(expected, result);
            // Another case: x * (y+z+w) - distribute rule cannot handle more than two arguments for Add
            // This tests that canonicalization and pattern matching for binary operations is precise.
            Symbol z = new Symbol("z");
            Symbol w = new Symbol("w");
            IExpression longerAdd = new Add(ImmutableList.Create<IExpression>(y, z, w));
            IExpression inputLongerAdd = new Multiply(ImmutableList.Create<IExpression>(x, longerAdd));
            // Distribute rule is specifically for two arguments of Add.
            // Canonicalization of (y,z,w) will make it a 3-arg Add.
            // The pattern a*(b+c) should not match a*(b+c+d).
            RewriterResult rewriteResultLongerAdd = Rewriter.RewriteFully(inputLongerAdd, ImmutableList.Create<Rule>(distributeRule));
            IExpression resultLongerAdd = rewriteResultLongerAdd.RewrittenExpression;
            Assert.AreEqual(inputLongerAdd.Canonicalize(), resultLongerAdd); // Should remain unchanged
            // To ensure the test passes, the longerAdd must not be canonicalized to a 2-arg add
            Assert.AreEqual(3, ((Add)((Multiply)inputLongerAdd).Arguments[1]).Arguments.Count);
        }

        [TestMethod]
        public void Rewrite_TerminatesOnStableExpression()
        {
            // If no rule applies, it should terminate without infinite loop
            Symbol a = new Symbol("a");
            IExpression input = new Add(ImmutableList.Create<IExpression>(a, a)); // Simplification not covered by current rules
            RewriterResult rewriteResult = Rewriter.RewriteFully(input, ImmutableList<Rule>.Empty);
            IExpression result = rewriteResult.RewrittenExpression;
            Assert.AreSame(input, result); // Should return the same instance if no rules apply and no changes occur
        }

        // Accessor for the private TryMatchRecursive method
        private static bool InternalTryMatchRecursiveAccessor(IExpression expression, IExpression pattern, ImmutableDictionary<string, IExpression>.Builder bindings)
        {
            // Using reflection for testing private methods is generally discouraged but can be useful for isolated, low-level behavioral tests.
            // In a real project, one might expose this functionality via a public API or refactor.
            // For this autonomous agent context, simple reflection is a practical way to test the underlying logic.
            MethodInfo? method = typeof(Rewriter).GetMethod("TryMatchRecursive", BindingFlags.Static | BindingFlags.NonPublic);
            if (method is null)
            {
                Assert.Fail("TryMatchRecursive method not found.");
                return false;
            }

            // The parameters array itself must be mutable for the 'bindings' parameter which is a ref/out-like parameter.
            Object[] parameters = new Object[]
            {
                expression,
                pattern,
                bindings
            };
            Object? result = method.Invoke(null, parameters);
            if (result is null)
            {
                Assert.Fail("TryMatchRecursive returned null.");
                return false;
            }

            return (bool)result;
        }

        // Helper rule definitions for RewriterTests.cs
        private static Rule CreateAddZeroRule()
        {
            return new Rule(new Add(new Wild("x"), new Number(0m)), new Wild("x"));
        }

        private static Rule CreateMulOneRule()
        {
            return new Rule(new Multiply(new Wild("x"), new Number(1m)), new Wild("x"));
        }

        private static Rule CreateDistributeMultiplyOverAddRule()
        {
            return new Rule(
                new Multiply(new Wild("a"), new Add(new Wild("b"), new Wild("c"))),
                new Add(
                    new Multiply(new Wild("a"), new Wild("b")),
                    new Multiply(new Wild("a"), new Wild("c"))
                )
            );
        }
    }
}