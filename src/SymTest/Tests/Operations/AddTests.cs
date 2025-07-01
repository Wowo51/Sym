//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace SymTest
{
    [TestClass]
    public sealed class AddTests
    {
        [TestMethod]
        public void Add_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            ImmutableList<IExpression> args = ImmutableList.Create<IExpression>(x, y);
            Add add = new Add(args);

            Assert.AreEqual(2, add.Arguments.Count);
            CollectionAssert.AreEqual(args.ToList(), add.Arguments.ToList());
            Assert.IsTrue(add.IsOperation);
            Assert.IsFalse(add.IsAtom);
        }

        [TestMethod]
        public void Add_Shape_ReturnsCorrectScalarShape()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Add add = new Add(ImmutableList.Create<IExpression>(x, five));
            Assert.AreEqual(Shape.Scalar, add.Shape);
        }

        [TestMethod]
        public void Add_Shape_ReturnsCorrectVectorShape()
        {
            Vector vec1 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x1"), new Symbol("y1")));
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x2"), new Symbol("y2")));
            Add add = new Add(ImmutableList.Create<IExpression>(vec1, vec2));
            Assert.AreEqual(new Shape(ImmutableArray.Create(2)), add.Shape);
        }

        [TestMethod]
        public void Add_Shape_HandlesDifferentDimensionVectorsReturnsError()
        {
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Vector vec3 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b"), new Symbol("c")));
            Add add = new Add(ImmutableList.Create<IExpression>(vec2, vec3));
            Assert.AreEqual(Shape.Error, add.Shape);
        }

        [TestMethod]
        public void Add_Shape_HandlesScalarAndVectorReturnsVectorShape()
        {
            Number scalar = new Number(5m);
            Vector vector = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Add add = new Add(ImmutableList.Create<IExpression>(scalar, vector));
            Assert.AreEqual(vector.Shape, add.Shape);
        }

        [TestMethod]
        public void Add_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Add add = new Add(ImmutableList.Create<IExpression>(x, five));
            Assert.AreEqual("(x + 5)", add.ToDisplayString());

            Add nestedAdd = new Add(ImmutableList.Create<IExpression>(x, new Add(ImmutableList.Create<IExpression>(new Symbol("y"), new Symbol("z")))));
            Assert.AreEqual("(x + (y + z))", nestedAdd.ToDisplayString());
        }

        [TestMethod]
        public void Add_Canonicalize_FlattensAndSortsArguments()
        {
            // (a + (c + b)) + d + 0 -> (a + b + c + d)
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Symbol c = new Symbol("c");
            Symbol d = new Symbol("d");
            Number zero = new Number(0m);

            Add innerAdd = new Add(ImmutableList.Create<IExpression>(c, b));
            Add outerAdd1 = new Add(ImmutableList.Create<IExpression>(a, innerAdd));
            Add input = new Add(ImmutableList.Create<IExpression>(outerAdd1, d, zero));

            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Add));
            Add canonicalAdd = (Add)canonical;

            Assert.AreEqual(4, canonicalAdd.Arguments.Count);
            // After flattening and sorting: a, b, c, d
            Assert.AreEqual((IExpression)a, canonicalAdd.Arguments[0]);
            Assert.AreEqual((IExpression)b, canonicalAdd.Arguments[1]);
            Assert.AreEqual((IExpression)c, canonicalAdd.Arguments[2]);
            Assert.AreEqual((IExpression)d, canonicalAdd.Arguments[3]);
        }

        [TestMethod]
        public void Add_Canonicalize_SumsNumericTerms()
        {
            Symbol x = new Symbol("x");
            Number num1 = new Number(2m);
            Number num2 = new Number(3m);
            Number num3 = new Number(4m);

            Add input = new Add(ImmutableList.Create<IExpression>(x, num1, num2, num3));
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Add));
            Add canonicalAdd = (Add)canonical;

            // Arguments should be 9, x (after sorting, numbers before symbols)
            Assert.AreEqual(2, canonicalAdd.Arguments.Count);
            Assert.AreEqual((IExpression)new Number(9m), canonicalAdd.Arguments[0]);
            Assert.AreEqual((IExpression)x, canonicalAdd.Arguments[1]);
        }

        [TestMethod]
        public void Add_Canonicalize_HandlesZeroSum()
        {
            Number num1 = new Number(5m);
            Number num2 = new Number(-5m);
            Add input = new Add(ImmutableList.Create<IExpression>(num1, num2));
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)new Number(0m), canonical);

            Symbol x = new Symbol("x");
            Add inputWithZeroSum = new Add(ImmutableList.Create<IExpression>(x, num1, num2));
            IExpression canonicalWithZeroSum = inputWithZeroSum.Canonicalize();
            Assert.IsInstanceOfType(canonicalWithZeroSum, typeof(Symbol));
            Assert.AreEqual((IExpression)x, canonicalWithZeroSum);
        }

        [TestMethod]
        public void Add_Canonicalize_ReturnsSingleTermIfOnlyOneExists()
        {
            Symbol x = new Symbol("x");
            Add input = new Add(ImmutableList.Create<IExpression>(x));
            IExpression canonical = input.Canonicalize();
            Assert.AreEqual((IExpression)x, canonical);

            Number five = new Number(5m);
            Add inputNum = new Add(ImmutableList.Create<IExpression>(five));
            IExpression canonicalNum = inputNum.Canonicalize();
            Assert.AreEqual((IExpression)five, canonicalNum);
        }

        [TestMethod]
        public void Add_Canonicalize_HandlesEmptyAddResultingInZero()
        {
            // This scenario should primarily be unreachable with current constructors,
            // but if flattening or summing removed all terms, this should result in 0.
            // Example: Add(0) might become 0, Add(x, -x) might become 0.
            Add emptyAdd = new Add(ImmutableList.Create<IExpression>(new Number(0m)));
            Assert.AreEqual((IExpression)new Number(0m), emptyAdd.Canonicalize());

            // A bit contrived, but if arg list becomes empty after processing (e.g. all terms cancel)
            Add cancellingAdd = new Add(ImmutableList.Create<IExpression>(new Number(5m), new Number(-5m)));
            Assert.AreEqual((IExpression)new Number(0m), cancellingAdd.Canonicalize());
        }

        [TestMethod]
        public void Add_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Add add1 = new Add(ImmutableList.Create<IExpression>(a, b));
            Add add2 = new Add(ImmutableList.Create<IExpression>(a, b));
            Add add3 = new Add(ImmutableList.Create<IExpression>(b, a)); // Will canonicalize to same due to sorting
            Add add4 = new Add(ImmutableList.Create<IExpression>(a, b, new Number(1m)));

            Assert.IsTrue(add1.Equals(add2));
            Assert.AreEqual(add1.GetHashCode(), add2.GetHashCode());
            Assert.IsTrue(add1.InternalEquals(add2)); // InternalEquals relies on sorted args after canonicalization
            Assert.AreEqual(add1.InternalGetHashCode(), add2.InternalGetHashCode());

            Assert.IsTrue(add1.Equals(add3)); // Canonical forms are equal
            Assert.AreEqual(add1.GetHashCode(), add3.GetHashCode());
            // InternalEquals for operations compares already cannonicalized args.
            // So if add1 has (a,b) and add3 has (b,a), their canonical forms are same (A,B), so internal equals true.
            IExpression add1Can = add1.Canonicalize();
            IExpression add3Can = add3.Canonicalize();
            Assert.IsTrue(add1Can.InternalEquals(add3Can));
            Assert.AreEqual(add1Can.InternalGetHashCode(), add3Can.InternalGetHashCode());

            Assert.IsFalse(add1.Equals(add4));
            Assert.AreNotEqual(add1.GetHashCode(), add4.GetHashCode());
        }

        [TestMethod]
        public void Add_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Symbol z = new Symbol("z");
            Add originalAdd = new Add(ImmutableList.Create<IExpression>(x, y));
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(x, y, z);
            
            Operation newAdd = originalAdd.WithArguments(newArgs);

            Assert.IsInstanceOfType(newAdd, typeof(Add));
            Assert.AreNotSame(originalAdd, newAdd);
            CollectionAssert.AreEqual(newArgs.ToList(), ((Add)newAdd).Arguments.ToList());
        }
    }
}