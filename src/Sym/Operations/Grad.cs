//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    /// <summary>
    /// Represents the gradient operation: Grad(scalar_expression, vector_variable).
    /// The resulting shape is typically a vector derived from the dimensions of the vector_variable.
    /// </summary>
    public sealed class Grad : Operation
    {
        /// <summary>
        /// The scalar expression for which the gradient is to be computed.
        /// </summary>
        public IExpression ScalarExpression { get; init; }
        /// <summary>
        /// The vector variable with respect to which the gradient is computed.
        /// </summary>
        public IExpression VectorVariable { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grad"/> class.
        /// </summary>
        /// <param name="scalarExpression">The scalar expression.</param>
        /// <param name="vectorVariable">The vector variable.</param>
        public Grad(IExpression scalarExpression, IExpression vectorVariable) 
            : base(ImmutableList.Create(scalarExpression, vectorVariable))
        {
            ScalarExpression = scalarExpression;
            VectorVariable = vectorVariable;
        }

        public override Shape Shape
        {
            get
            {
                // The gradient of a scalar field with respect to a vector variable results in a vector
                // whose dimensions match the vector variable, provided the scalar expression is indeed scalar.
                if (!ScalarExpression.Shape.IsScalar || !VectorVariable.Shape.IsVector)
                {
                    return Shape.Error; // Grad requires scalar expression and vector variable
                }
                return VectorVariable.Shape;
            }
        }

        /// <summary>
        /// Canonicalizes the gradient operation by canonicalizing its constituent expressions.
        /// </summary>
        /// <returns>A canonicalized Grad expression.</returns>
        public override IExpression Canonicalize()
        {
            IExpression canonicalScalar = ScalarExpression.Canonicalize();
            IExpression canonicalVector = VectorVariable.Canonicalize();

            if (!ReferenceEquals(canonicalScalar, ScalarExpression) || !ReferenceEquals(canonicalVector, VectorVariable))
            {
                return new Grad(canonicalScalar, canonicalVector);
            }
            return this;
        }

        /// <summary>
        /// Provides a string representation of the gradient operation.
        /// </summary>
        /// <returns>A string representing the gradient.</returns>
        public override string ToDisplayString()
        {
            return $"Grad({ScalarExpression.ToDisplayString()}, {VectorVariable.ToDisplayString()})";
        }
        
        /// <summary>
        /// Determines whether this gradient expression is internally equal to another expression.
        /// </summary>
        /// <param name="other">The other expression to compare with.</param>
        /// <returns>True if expressions are internally equal, false otherwise.</returns>
        public override bool InternalEquals(IExpression other)
        {
            if (other is not Grad otherGrad)
            {
                return false;
            }
            return ScalarExpression.InternalEquals(otherGrad.ScalarExpression) &&
                   VectorVariable.InternalEquals(otherGrad.VectorVariable);
        }

        /// <summary>
        /// Returns the internal hash code for this gradient expression.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(ScalarExpression);
            hash.Add(VectorVariable);
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Grad(newArgs[0], newArgs[1]);
        }
    }
}