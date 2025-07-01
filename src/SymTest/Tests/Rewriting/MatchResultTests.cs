//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using System.Collections.Immutable;
using Sym.Atoms;

namespace SymTest
{
    [TestClass]
    public sealed class MatchResultTests
    {
        [TestMethod]
        public void MatchResult_SuccessConstructor_SetsPropertiesCorrectly()
        {
            ImmutableDictionary<string, IExpression> bindings = ImmutableDictionary.Create<string, IExpression>().Add("x", new Number(5m));
            MatchResult result = new MatchResult(true, bindings);

            Assert.IsTrue(result.Success);
            Assert.AreSame(bindings, result.Bindings);
            Assert.AreEqual(1, result.Bindings.Count);
            Assert.AreEqual((IExpression)new Number(5m), result.Bindings["x"]);
        }

        [TestMethod]
        public void MatchResult_FailMethod_CreatesFailedResultWithEmptyBindings()
        {
            MatchResult result = MatchResult.Fail();

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Bindings.IsEmpty);
        }

        [TestMethod]
        public void MatchResult_RecordEquality_BehavesCorrectly()
        {
            ImmutableDictionary<string, IExpression> bindings1 = ImmutableDictionary.Create<string, IExpression>().Add("x", new Number(5m));
            ImmutableDictionary<string, IExpression> bindings2 = ImmutableDictionary.Create<string, IExpression>().Add("x", new Number(5m));
            ImmutableDictionary<string, IExpression> bindings3 = ImmutableDictionary.Create<string, IExpression>().Add("y", new Number(10m));

            MatchResult result1 = new MatchResult(true, bindings1);
            MatchResult result2 = new MatchResult(true, bindings2);
            MatchResult result3 = new MatchResult(false, ImmutableDictionary<string, IExpression>.Empty);
            MatchResult result4 = new MatchResult(true, bindings3);

            // Structural equality for records
            Assert.AreEqual(result1, result2);
            Assert.AreNotEqual(result1, result3);
            Assert.AreNotEqual(result1, result4);

            Assert.AreEqual(result1.GetHashCode(), result2.GetHashCode());
            Assert.AreNotEqual(result1.GetHashCode(), result3.GetHashCode());
            Assert.AreNotEqual(result1.GetHashCode(), result4.GetHashCode());
        }
    }
}