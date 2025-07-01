//Copyright Warren Harding 2025.
using Sym.Core;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    /// <summary>
    /// Represents a vector as an ordered list of scalar expressions (its components).
    /// </summary>
    public sealed class Vector : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector"/> class.
        /// </summary>
        /// <param name="components">The immutable list of scalar expressions representing the vector's components.</param>
        public Vector(ImmutableList<IExpression> components) : base(components) { }

        public override Shape Shape
        {
            get
            {
                // A vector's shape is (N,) where N is the number of components.
                // All components must be scalars for it to be a pure vector in this context.
                if (Arguments.Any(arg => !arg.Shape.IsScalar))
                {
                    return Shape.Error; // Components of a vector must be scalars
                }
                return new Shape(ImmutableArray.Create(Arguments.Count));
            }
        }

        public override IExpression Canonicalize()
        {
            ImmutableList<IExpression> canonicalArgs = Arguments.Select(arg => arg.Canonicalize()).ToImmutableList();

            bool changed = false;
            if (Arguments.Count != canonicalArgs.Count) // Should ideally not change count here
            {
                changed = true;
            }
            else
            {
                for (int i = 0; i < Arguments.Count; i++)
                {
                    // If the reference of any argument changes, the vector itself is considered changed.
                    if (!ReferenceEquals(Arguments[i], canonicalArgs[i])) 
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                // Vector component order is meaningful, so do not sort arguments here.
                return new Vector(canonicalArgs);
            }
            return this;
        }

        public override string ToDisplayString()
        {
            return $"Vector({string.Join(", ", Arguments.Select(arg => arg.ToDisplayString()))})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is not Vector otherVector || Arguments.Count != otherVector.Arguments.Count)
            {
                return false;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].InternalEquals(otherVector.Arguments[i]))
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
            foreach (IExpression arg in Arguments)
            {
                hash.Add(arg);
            }
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Vector(newArgs);
        }
    }
}