//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System; // Alias Sys for System namespace

namespace Sym.Operations
{
    /// <summary>
    /// Represents the divergence operation: Div(vector_expression, vector_variable).
    /// The result of divergence is a scalar.
    /// </summary>
    public sealed class Div : Operation
    {
        /// <summary>
        /// The vector expression for which the divergence is to be computed.
        /// </summary>
        public IExpression VectorExpression { get; init; }
        /// <summary>
        /// The vector variable with respect to which the divergence is computed.
        /// </summary>
        public IExpression VectorVariable { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Div"/> class.
        /// </summary>
        /// <param name="vectorExpression">The vector expression.</param>
        /// <param name="vectorVariable">The vector variable.</param>
        public Div(IExpression vectorExpression, IExpression vectorVariable) 
            : base(ImmutableList.Create(vectorExpression, vectorVariable))
        {
            VectorExpression = vectorExpression;
            VectorVariable = vectorVariable;
        }

        public override Shape Shape
        {
            get
            {
                // Divergence requires a vector expression and a vector variable of compatible dimensions.
                // The result is a scalar.
                if (!VectorExpression.Shape.IsVector || !VectorVariable.Shape.IsVector || !VectorExpression.Shape.AreDimensionsCompatibleForElementWise(VectorVariable.Shape))
                {
                    return Shape.Error; // Div requires vector expression and vector variable of compatible dimension
                }
                return Shape.Scalar;
            }
        } // Divergence of a vector field results in a scalar field

        /// <summary>
        /// Canonicalizes the divergence operation by canonicalizing its constituent expressions.
        /// </summary>
        /// <returns>A canonicalized Div expression.</returns>
        public override IExpression Canonicalize()
        {
            IExpression canonicalVectorExp = VectorExpression.Canonicalize();
            IExpression canonicalVectorVar = VectorVariable.Canonicalize();

            if (!ReferenceEquals(canonicalVectorExp, VectorExpression) || !ReferenceEquals(canonicalVectorVar, VectorVariable))
            {
                return new Div(canonicalVectorExp, canonicalVectorVar);
            }
            return this;
        }

        /// <summary>
        /// Provides a string representation of the divergence operation.
        /// </summary>
        /// <returns>A string representing the divergence.</returns>
        public override string ToDisplayString()
        {
            return $"Div({VectorExpression.ToDisplayString()}, {VectorVariable.ToDisplayString()})";
        }
        
        /// <summary>
        /// Determines whether this divergence expression is internally equal to another expression.
        /// </summary>
        /// <param name="other">The other expression to compare with.</param>
        /// <returns>True if expressions are internally equal, false otherwise.</returns>
        public override bool InternalEquals(IExpression other)
        {
            if (other is not Div otherDiv)
            {
                return false;
            }
            return VectorExpression.InternalEquals(otherDiv.VectorExpression) &&
                   VectorVariable.InternalEquals(otherDiv.VectorVariable);
        }

        /// <summary>
        /// Returns the internal hash code for this divergence expression.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(VectorExpression);
            hash.Add(VectorVariable);
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Div(newArgs[0], newArgs[1]);
        }
    }
}