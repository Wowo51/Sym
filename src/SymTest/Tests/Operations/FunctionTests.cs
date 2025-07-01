//Copyright Warren Harding 2025.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace SymTest
{
    [TestClass]
    public sealed class FunctionTests
    {
        [TestMethod]
        public void Function_ConstructorAndProperties_SetsCorrectly()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            ImmutableList<IExpression> args = ImmutableList.Create<IExpression>(x, five);
            Function func = new Function("sin", args);

            Assert.AreEqual("sin", func.Name);
            Assert.AreEqual(2, func.Arguments.Count);
            CollectionAssert.AreEqual(args.ToList(), func.Arguments.ToList());
            Assert.IsTrue(func.IsOperation);
            Assert.IsFalse(func.IsAtom);
        }

        [TestMethod]
        public void Function_Shape_ReturnsCombinedShape()
        {
            Symbol x = new Symbol("x"); // Scalar
            Vector vecY = new Vector(ImmutableList.Create<IExpression>(new Symbol("y1"), new Symbol("y2"))); // Vector of shape (2,)
            
            Function funcScalarArgs = new Function("f", ImmutableList.Create<IExpression>(x, new Number(10m)));
            Assert.AreEqual(Shape.Scalar, funcScalarArgs.Shape);

            Function funcMixedArgs = new Function("g", ImmutableList.Create<IExpression>(vecY, x));
            Assert.AreEqual(vecY.Shape, funcMixedArgs.Shape); // Scalar argument should promote to vector.
        }

        [TestMethod]
        public void Function_Shape_ReturnsErrorForIncompatibleShapes()
        {
            Vector vec2 = new Vector(ImmutableList.Create<IExpression>(new Symbol("x"), new Symbol("y")));
            Vector vec3 = new Vector(ImmutableList.Create<IExpression>(new Symbol("a"), new Symbol("b"), new Symbol("c")));
            Function funcIncompatible = new Function("h", ImmutableList.Create<IExpression>(vec2, vec3));
            Assert.AreEqual(Shape.Error, funcIncompatible.Shape);
        }

        [TestMethod]
        public void Function_ToDisplayString_ReturnsCorrectFormat()
        {
            Symbol x = new Symbol("x");
            Number five = new Number(5m);
            Function func = new Function("sum", ImmutableList.Create<IExpression>(x, five));
            Assert.AreEqual("sum(x, 5)", func.ToDisplayString());

            Function nestedFunc = new Function("outer", ImmutableList.Create<IExpression>(x, new Function("inner", ImmutableList.Create<IExpression>(new Symbol("y"), new Symbol("z")))));
            Assert.AreEqual("outer(x, inner(y, z))", nestedFunc.ToDisplayString());
        }

        [TestMethod]
        public void Function_Canonicalize_CanonicalizesArguments()
        {
            Symbol x = new Symbol("x");
            Number zero = new Number(0m);
            Symbol y = new Symbol("y");

            Add complexArg1 = new Add(ImmutableList.Create<IExpression>(x, zero)); // Canonicalizes to x
            Multiply complexArg2 = new Multiply(ImmutableList.Create<IExpression>(y, new Number(1m))); // Canonicalizes to y

            Function input = new Function("f", ImmutableList.Create<IExpression>(complexArg1, complexArg2));
            IExpression canonical = input.Canonicalize();

            Assert.IsInstanceOfType(canonical, typeof(Function));
            Function canonicalFunc = (Function)canonical;

            Assert.AreEqual("f", canonicalFunc.Name);
            Assert.AreEqual(2, canonicalFunc.Arguments.Count);
            Assert.AreEqual((IExpression)x, canonicalFunc.Arguments[0]);
            Assert.AreEqual((IExpression)y, canonicalFunc.Arguments[1]);
            Assert.AreNotSame(input, canonicalFunc); // Should be new instance due to canonicalized elements
        }

        [TestMethod]
        public void Function_Canonicalize_ReturnsSelfIfNoChange()
        {
            Symbol x = new Symbol("x");
            Function func = new Function("g", ImmutableList.Create<IExpression>(x, new Number(10m)));
            IExpression canonical = func.Canonicalize();
            Assert.AreSame(func, canonical);
        }

        [TestMethod]
        public void Function_EqualsAndGetHashCode_BehaveCorrectly()
        {
            Symbol a = new Symbol("a");
            Symbol b = new Symbol("b");
            Function func1 = new Function("myFunc", ImmutableList.Create<IExpression>(a, b));
            Function func2 = new Function("myFunc", ImmutableList.Create<IExpression>(a, b)); // Same name, same args
            Function func3 = new Function("otherFunc", ImmutableList.Create<IExpression>(a, b)); // Different name
            Function func4 = new Function("myFunc", ImmutableList.Create<IExpression>(b, a)); // Same args, different order after canonicalization (but func Args are ordered)
            Function func5 = new Function("myFunc", ImmutableList.Create<IExpression>(a)); // Different arg count

            Assert.IsTrue(func1.Equals(func2));
            Assert.AreEqual(func1.GetHashCode(), func2.GetHashCode());
            Assert.IsTrue(func1.InternalEquals(func2));
            Assert.AreEqual(func1.InternalGetHashCode(), func2.InternalGetHashCode());

            Assert.IsFalse(func1.Equals(func3));
            Assert.AreNotEqual(func1.GetHashCode(), func3.GetHashCode());
            Assert.IsFalse(func1.InternalEquals(func3));
            Assert.AreNotEqual(func1.InternalGetHashCode(), func3.InternalGetHashCode());

            // A Function's arguments are order significant, even if internally their components would sort.
            // So func1 and func4 should NOT be equal.
            Assert.IsFalse(func1.Equals(func4));
            Assert.AreNotEqual(func1.GetHashCode(), func4.GetHashCode());
            Assert.IsFalse(func1.InternalEquals(func4));
            Assert.AreNotEqual(func1.InternalGetHashCode(), func4.InternalGetHashCode());

            Assert.IsFalse(func1.Equals(func5));
            Assert.AreNotEqual(func1.GetHashCode(), func5.GetHashCode());
        }

        [TestMethod]
        public void Function_WithArguments_CreatesNewInstanceWithNewArguments()
        {
            Symbol x = new Symbol("x");
            Symbol y = new Symbol("y");
            Symbol z = new Symbol("z");
            Function originalFunc = new Function("foo", ImmutableList.Create<IExpression>(x, y));
            ImmutableList<IExpression> newArgs = ImmutableList.Create<IExpression>(x, y, z);
            
            Operation newFunc = originalFunc.WithArguments(newArgs);

            Assert.IsInstanceOfType(newFunc, typeof(Function));
            Assert.AreNotSame(originalFunc, newFunc);
            CollectionAssert.AreEqual(newArgs.ToList(), ((Function)newFunc).Arguments.ToList());
            Assert.AreEqual("foo", ((Function)newFunc).Name); // Name should be preserved
        }
    }
}