//Copyright Warren Harding 2025.
using Sym.Core;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    public sealed class Function : Operation
    {
        public string Name { get; init; }

        public Function(string name, ImmutableList<IExpression> arguments) : base(arguments)
        {
            Name = name;
        }
        
        public override Shape Shape
        {
            get
            {
                Shape resultShape = Shape.Scalar;
                foreach (IExpression arg in Arguments)
                {
                    if (!arg.Shape.IsValid)
                    {
                        return Shape.Error;
                    }

                    resultShape = resultShape.CombineForElementWise(arg.Shape);
                    if (!resultShape.IsValid)
                    {
                        return Shape.Error; // Incompatible shapes found during aggregation
                    }
                }
                return resultShape;
            }
        }

        public override IExpression Canonicalize()
        {
            ImmutableList<IExpression> canonicalArgs = Arguments.Select(arg => arg.Canonicalize()).ToImmutableList();

            bool argumentsChanged = false;
            if (Arguments.Count != canonicalArgs.Count)
            {
                argumentsChanged = true;
            }
            else
            {
                for (int i = 0; i < Arguments.Count; i++)
                {
                    // If the reference has changed, then the argument list effectively changed.
                    if (!ReferenceEquals(Arguments[i], canonicalArgs[i])) 
                    {
                        argumentsChanged = true;
                        break;
                    }
                }
            }

            if (argumentsChanged)
            {
                return new Function(Name, canonicalArgs);
            }

            return this;
        }

        public override string ToDisplayString()
        {
            return $"{Name}({string.Join(", ", Arguments.Select(arg => arg.ToDisplayString()))})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (other is not Function otherFunc || !Name.Equals(otherFunc.Name, StringComparison.Ordinal))
            {
                return false;
            }
            if (Arguments.Count != otherFunc.Arguments.Count)
            {
                return false;
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].InternalEquals(otherFunc.Arguments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(Name);
            foreach (IExpression arg in Arguments)
            {
                hash.Add(arg);
            }
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Function(Name, newArgs);
        }
    }
}