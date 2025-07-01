//Copyright Warren Harding 2025.
using System;

namespace Sym.Core
{
    /// <summary>
    /// Defines constraints for wildcard pattern matching.
    /// </summary>
    public enum WildConstraint
    {
        /// <summary>
        /// No specific constraint on the matched expression.
        /// </summary>
        None,
        /// <summary>
        /// The matched expression must be a scalar (Shape.IsScalar is true).
        /// </summary>
        Scalar,
        /// <summary>
        /// The matched expression must be a constant (currently, only Sym.Atoms.Number).
        /// This can be extended to include symbolic constants later.
        /// </summary>
        Constant
    }
}