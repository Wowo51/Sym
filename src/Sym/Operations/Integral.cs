//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    /// <summary>
    /// Represents a symbolic integration operation: Integral(expression, variable).
    /// </summary>
    public sealed class Integral : Operation
    {
        /// <summary>
        /// The expression to be integrated.
        /// </summary>
        public IExpression TargetExpression { get; init; }
        /// <summary>
        /// The variable with respect to which the integration is performed.
        /// </summary>
        public IExpression Variable { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Integral"/> class.
        /// </summary>
        /// <param name="targetExpression">The expression to be integrated.</param>
        /// <param name="variable">The variable of integration.</param>
        public Integral(IExpression targetExpression, IExpression variable) 
            : base(ImmutableList.Create(targetExpression, variable))
        {
            TargetExpression = targetExpression;
            Variable = variable;
        }

        public override Shape Shape => TargetExpression.Shape; // The shape of the integral is the same as the original expression

        /// <summary>
        /// Canonicalizes the integral expression by canonicalizing its target expression and variable.
        /// </summary>
        /// <returns>A canonicalized Integral expression.</returns>
        public override IExpression Canonicalize()
        {
            IExpression canonicalTarget = TargetExpression.Canonicalize();
            IExpression canonicalVariable = Variable.Canonicalize();

            if (!ReferenceEquals(canonicalTarget, TargetExpression) || !ReferenceEquals(canonicalVariable, Variable))
            {
                return new Integral(canonicalTarget, canonicalVariable);
            }
            return this;
        }

        /// <summary>
        /// Provides a string representation of the integral operation.
        /// </summary>
        /// <returns>A string representing the integral.</returns>
        public override string ToDisplayString()
        {
            return $"Integral({TargetExpression.ToDisplayString()}, {Variable.ToDisplayString()})";
        }
        
        /// <summary>
        /// Determines whether this integral expression is internally equal to another expression.
        /// </summary>
        /// <param name="other">The other expression to compare with.</param>
        /// <returns>True if expressions are internally equal, false otherwise.</returns>
        public override bool InternalEquals(IExpression other)
        {
            if (other is not Integral otherIntegral)
            {
                return false;
            }
            return TargetExpression.InternalEquals(otherIntegral.TargetExpression) &&
                   Variable.InternalEquals(otherIntegral.Variable);
        }

        /// <summary>
        /// Returns the internal hash code for this integral expression.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(TargetExpression);
            hash.Add(Variable);
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Integral(newArgs[0], newArgs[1]);
        }
    }
}