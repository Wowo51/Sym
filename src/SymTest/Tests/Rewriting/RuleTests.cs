//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System;

namespace SymTest
{
    [TestClass]
    public sealed class RuleTests
    {
        [TestMethod]
        public void Rule_Constructor_SetsPropertiesCorrectly()
        {
            // d/dx (x) = 1
            Wild wildX = new Wild("x");
            IExpression pattern = new Derivative(wildX, wildX);
            IExpression replacement = new Number(1m);

            Rule rule = new Rule(pattern, replacement);

            Assert.AreSame(pattern, rule.Pattern);
            Assert.AreSame(replacement, rule.Replacement);
            Assert.IsNull(rule.Condition);

            Func<ImmutableDictionary<string, IExpression>, bool> condition = (b) => true;
            Rule ruleWithCondition = new Rule(pattern, replacement, condition);
            Assert.AreSame(condition, ruleWithCondition.Condition);
        }

        [TestMethod]
        public void Rule_Apply_AppliesWhenMatchSuccessfulAndNoCondition()
        {
            // d/dx (x) = 1
            Wild wildX = new Wild("x");
            IExpression pattern = new Derivative(wildX, wildX);
            IExpression replacement = new Number(1m);
            Rule rule = new Rule(pattern, replacement);

            IExpression input = new Derivative(new Symbol("y"), new Symbol("y"));
            IExpression expectedOutput = new Number(1m);

            IExpression actualOutput = rule.Apply(input);
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [TestMethod]
        public void Rule_Apply_DoesNotApplyWhenNoMatch()
        {
            // d/dx (x) = 1
            Wild wildX = new Wild("x");
            IExpression pattern = new Derivative(wildX, wildX);
            IExpression replacement = new Number(1m);
            Rule rule = new Rule(pattern, replacement);

            IExpression input = new Add(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b"))); // Not a derivative
            IExpression expectedOutput = input;

            IExpression actualOutput = rule.Apply(input);
            Assert.AreSame(expectedOutput, actualOutput); // Should return the same instance if no change
        }

        [TestMethod]
        public void Rule_Apply_AppliesWhenMatchSuccessfulAndConditionTrue()
        {
            // d/dx (x^n) = n * x^(n-1) if n is a Number
            Wild wildX = new Wild("x");
            Wild wildN = new Wild("n");
            IExpression pattern = new Derivative(new Power(wildX, wildN), wildX);
            IExpression replacement = new Multiply(ImmutableList.Create<IExpression>(
                wildN,
                new Power(wildX, new Add(ImmutableList.Create<IExpression>(wildN, new Number(-1m))))
            ));
            Func<ImmutableDictionary<string, IExpression>, bool> condition = (bindings) =>
                bindings.TryGetValue("n", out IExpression? matchedN) && matchedN is Number;

            Rule rule = new Rule(pattern, replacement, condition);

            // Test case: d/dx (x^2) -> 2 * x^(2-1) = 2*x
            IExpression input = new Derivative(new Power(new Symbol("z"), new Number(2m)), new Symbol("z"));
            IExpression expectedOutput = new Multiply(ImmutableList.Create<IExpression>(
                new Number(2m),
                new Power(new Symbol("z"), new Add(ImmutableList.Create<IExpression>(new Number(2m), new Number(-1m))))
            )).Canonicalize(); // Canonicalize the expected output for fair comparison due to sorting in Mul/Add

            IExpression actualOutput = rule.Apply(input);
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [TestMethod]
        public void Rule_Apply_DoesNotApplyWhenConditionFalse()
        {
            // d/dx (x^n) only if n is a Number
            Wild wildX = new Wild("x");
            Wild wildN = new Wild("n");
            IExpression pattern = new Derivative(new Power(wildX, wildN), wildX);
            IExpression replacement = new Multiply(ImmutableList.Create<IExpression>(
                wildN,
                new Power(wildX, new Add(ImmutableList.Create<IExpression>(wildN, new Number(-1m))))
            ));
            Func<ImmutableDictionary<string, IExpression>, bool> condition = (bindings) =>
                bindings.TryGetValue("n", out IExpression? matchedN) && matchedN is Number;

            Rule rule = new Rule(pattern, replacement, condition);

            // Test case: d/dx (x^y) - 'y' is a Symbol, not a Number
            IExpression input = new Derivative(new Power(new Symbol("a"), new Symbol("b")), new Symbol("a"));
            IExpression expectedOutput = input;

            IExpression actualOutput = rule.Apply(input);
            Assert.AreSame(expectedOutput, actualOutput); // Should return original if condition false
        }
    }
}