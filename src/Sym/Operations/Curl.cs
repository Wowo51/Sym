//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System; // Alias Sys for System namespace

namespace Sym.Operations
{
    /// <summary>
    /// Represents the curl operation: Curl(vector_expression, vector_variable).
    /// The result of curl is typically a vector.
    /// </summary>
    public sealed class Curl : Operation
    {
        /// <summary>
        /// The vector expression for which the curl is to be computed.
        /// </summary>
        public IExpression VectorExpression { get; init; }
        /// <summary>
        /// The vector variable with respect to which the curl is computed.
        /// </summary>
        public IExpression VectorVariable { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Curl"/> class.
        /// </summary>
        /// <param name="vectorExpression">The vector expression.</param>
        /// <param name="vectorVariable">The vector variable.</param>
        public Curl(IExpression vectorExpression, IExpression vectorVariable) 
            : base(ImmutableList.Create(vectorExpression, vectorVariable))
        {
            VectorExpression = vectorExpression;
            VectorVariable = vectorVariable;
        }

        public override Shape Shape
        {
            get
            {
                // The curl of a vector field is a vector whose dimensions match the input variable,
                // provided the vector expression is indeed a vector of matching dimensions.
                if (!VectorExpression.Shape.IsVector || !VectorVariable.Shape.IsVector || !VectorExpression.Shape.AreDimensionsCompatibleForElementWise(VectorVariable.Shape))
                {
                    return Shape.Error; // Curl requires vector expression and vector variable of compatible dimension
                }
                return VectorVariable.Shape;
            }
        }

        /// <summary>
        /// Canonicalizes the curl operation by canonicalizing its constituent expressions.
        /// </summary>
        /// <returns>A canonicalized Curl expression.</returns>
        public override IExpression Canonicalize()
        {
            IExpression canonicalVectorExp = VectorExpression.Canonicalize();
            IExpression canonicalVectorVar = VectorVariable.Canonicalize();

            if (!ReferenceEquals(canonicalVectorExp, VectorExpression) || !ReferenceEquals(canonicalVectorVar, VectorVariable))
            {
                return new Curl(canonicalVectorExp, canonicalVectorVar);
            }
            return this;
        }

        /// <summary>
        /// Provides a string representation of the curl operation.
        /// </summary>
        /// <returns>A string representing the curl.</returns>
        public override string ToDisplayString()
        {
            return $"Curl({VectorExpression.ToDisplayString()}, {VectorVariable.ToDisplayString()})";
        }
        
        /// <summary>
        /// Determines whether this curl expression is internally equal to another expression.
        /// </summary>
        /// <param name="other">The other expression to compare with.</param>
        /// <returns>True if expressions are internally equal, false otherwise.</returns>
        public override bool InternalEquals(IExpression other)
        {
            if (other is not Curl otherCurl)
            {
                return false;
            }
            return VectorExpression.InternalEquals(otherCurl.VectorExpression) &&
                   VectorVariable.InternalEquals(otherCurl.VectorVariable);
        }

        /// <summary>
        /// Returns the internal hash code for this curl expression.
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
            return new Curl(newArgs[0], newArgs[1]);
        }
    }
}