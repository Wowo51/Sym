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
    public sealed class ExpressionHelpersTests
    {
        [TestMethod]
        public void FlattenArguments_FlattensNestedOperations()
        {
            // (a + (b + c)) + d -> a + b + c + d
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Symbol c = new Symbol("c");
            Symbol d = new Symbol("d");

            Add nestedAdd = new Add(ImmutableList.Create<IExpression>(b, c));
            Add outerAdd1 = new Add(ImmutableList.Create<IExpression>(a, nestedAdd));
            Add outerAdd2 = new Add(ImmutableList.Create<IExpression>(outerAdd1, d));

            ImmutableList<IExpression> flattened = ExpressionHelpers.FlattenArguments<Add>(outerAdd2.Arguments);

            Assert.AreEqual(4, flattened.Count);
            CollectionAssert.Contains(flattened.ToList(), a);
            CollectionAssert.Contains(flattened.ToList(), b);
            CollectionAssert.Contains(flattened.ToList(), c);
            CollectionAssert.Contains(flattened.ToList(), d);
        }

        [TestMethod]
        public void FlattenArguments_DoesNotFlattenDifferentOperations()
        {
            // a + (b * c) + d (Multiply should not be flattened by Add)
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Symbol c = new Symbol("c");
            Symbol d = new Symbol("d");

            Multiply nestedMultiply = new Multiply(ImmutableList.Create<IExpression>(b, c));
            Add outerAdd = new Add(ImmutableList.Create<IExpression>(a, nestedMultiply, d));

            ImmutableList<IExpression> flattened = ExpressionHelpers.FlattenArguments<Add>(outerAdd.Arguments);

            Assert.AreEqual(3, flattened.Count);
            CollectionAssert.Contains(flattened.ToList(), a);
            CollectionAssert.Contains(flattened.ToList(), nestedMultiply);
            CollectionAssert.Contains(flattened.ToList(), d);
        }

        [TestMethod]
        public void FlattenArguments_HandlesNoNestedOperations()
        {
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Add simpleAdd = new Add(ImmutableList.Create<IExpression>(a, b));

            ImmutableList<IExpression> flattened = ExpressionHelpers.FlattenArguments<Add>(simpleAdd.Arguments);

            Assert.AreEqual(2, flattened.Count);
            CollectionAssert.Contains(flattened.ToList(), a);
            CollectionAssert.Contains(flattened.ToList(), b);
        }

        [TestMethod]
        public void SortArguments_SortsNumbersCorrectly()
        {
            Number num1 = new Number(3m);
            Number num2 = new Number(1m);
            Number num3 = new Number(2m);

            ImmutableList<IExpression> unsorted = ImmutableList.Create<IExpression>(num1, num2, num3);
            ImmutableList<IExpression> sorted = ExpressionHelpers.SortArguments(unsorted);

            Assert.AreEqual((IExpression)num2, sorted[0]); // 1
            Assert.AreEqual((IExpression)num3, sorted[1]); // 2
            Assert.AreEqual((IExpression)num1, sorted[2]); // 3
        }

        [TestMethod]
        public void SortArguments_SortsSymbolsAlphabetically()
        {
            Symbol sym1 = new Symbol("z");
            Symbol sym2 = new Symbol("a");
            Symbol sym3 = new Symbol("b");

            ImmutableList<IExpression> unsorted = ImmutableList.Create<IExpression>(sym1, sym2, sym3);
            ImmutableList<IExpression> sorted = ExpressionHelpers.SortArguments(unsorted);

            Assert.AreEqual((IExpression)sym2, sorted[0]); // a
            Assert.AreEqual((IExpression)sym3, sorted[1]); // b
            Assert.AreEqual((IExpression)sym1, sorted[2]); // z
        }

        [TestMethod]
        public void SortArguments_SortsSymbolsByShapeThenAlphabetically()
        {
            Symbol symA_scalar = new Symbol("a", Shape.Scalar);
            Symbol symB_scalar = new Symbol("b", Shape.Scalar);
            Symbol symA_vector = new Symbol("a", new Shape(ImmutableArray.Create(3)));
            Symbol symB_vector = new Symbol("b", new Shape(ImmutableArray.Create(2)));


            ImmutableList<IExpression> unsorted = ImmutableList.Create<IExpression>(symA_vector, symB_scalar, symA_scalar, symB_vector);
            ImmutableList<IExpression> sorted = ExpressionHelpers.SortArguments(unsorted);
            
            // Expected order based on current comparer:
            // Atoms first, then Operations. Within Atoms: Number then Symbol then Wild.
            // Symbol: Name then Shape.DisplayString
            // symA_scalar (a())
            // symB_scalar (b())
            // symA_vector (a(3))
            // symB_vector (b(2)) - NO, (2) < (3)
            // symB_vector (b(2)) vs symA_vector (a(3)) -> symA_vector comes after symB_vector because 'a' > 'b'.
            // The comparer specifically says if type is same, then Sym.Name.
            // `string.CompareOrdinal(symX.Name, symY.Name)`
            // `symX.Shape.ToDisplayString().CompareTo(symY.Shape.ToDisplayString())` if names are equal.
            // So if names are different, name comparison is primary.
            // Correct order should be:
            // symA_scalar (a)
            // symA_vector (a(3))
            // symB_scalar (b)
            // symB_vector (b(2))

            Assert.AreEqual((IExpression)symA_scalar, sorted[0]);
            Assert.AreEqual((IExpression)symA_vector, sorted[1]); 
            Assert.AreEqual((IExpression)symB_scalar, sorted[2]); 
            Assert.AreEqual((IExpression)symB_vector, sorted[3]);
        }

        [TestMethod]
        public void SortArguments_SortsMixedTypesCorrectly()
        {
            Number num1 = new Number(5m);
            Symbol symX = new Symbol("x");
            Wild wildF = new Wild("f");
            Add addOp = new Add(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b")));
            Multiply mulOp = new Multiply(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));

            ImmutableList<IExpression> unsorted = ImmutableList.Create<IExpression>(wildF, mulOp, symX, addOp, num1);
            ImmutableList<IExpression> sorted = ExpressionHelpers.SortArguments(unsorted);

            // Natural order: Numbers -> Symbols -> Wilds -> Operations (sorted by type name, then args)
            // Expected
            // 0: num1 (Number)
            // 1: symX (Symbol)
            // 2: wildF (Wild)
            // 3: addOp (Add, because "Add" < "Multiply")
            // 4: mulOp (Multiply)

            Assert.AreEqual((IExpression)num1, sorted[0]);
            Assert.AreEqual((IExpression)symX, sorted[1]);
            Assert.AreEqual((IExpression)wildF, sorted[2]);
            Assert.AreEqual((IExpression)addOp, sorted[3]);
            Assert.AreEqual((IExpression)mulOp, sorted[4]);
        }

        [TestMethod]
        public void SortArguments_SortsOperationsByArgumentCountThenArguments()
        {
            Symbol s1 = new Symbol("s1");
            Symbol s2 = new Symbol("s2");
            Symbol s3 = new Symbol("s3");

            Add add1 = new Add(ImmutableList.Create<IExpression>(s1)); // 1 arg
            Add add2 = new Add(ImmutableList.Create<IExpression>(s1, s2)); // 2 args (s1, s2)
            Add add3 = new Add(ImmutableList.Create<IExpression>(s2, s1)); // 2 args (s1, s2) - canonicalized to s1, s2
            Add add4 = new Add(ImmutableList.Create<IExpression>(s1, s2, s3)); // 3 args

            ImmutableList<IExpression> unsorted = ImmutableList.Create<IExpression>(add4, add2, add1, add3);
            ImmutableList<IExpression> sorted = ExpressionHelpers.SortArguments(unsorted);

            // Canonicalization happens during comparison, so (s1,s2) and (s2,s1) are treated as equal.
            // Order: add1 (1 arg), add2/add3 (2 args), add4 (3 args)
            // The order of add2 and add3 might be arbitrary if their canonical forms are identical and order isn't guaranteed beyond comparison returning 0.
            Assert.AreEqual((IExpression)add1, sorted[0]);
            Assert.IsTrue(sorted[1].Equals(add2)); // Either add2 or add3
            Assert.IsTrue(sorted[2].Equals(add3)); // The other one
            Assert.AreEqual((IExpression)add4, sorted[3]);
        }
    }
}