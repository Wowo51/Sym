//Copyright Warren Harding 2025.
using System.Collections.Immutable;
using Sym.Atoms; // Required for Symbol
using Sym.Core; // Required for Rule

namespace Sym.Core
{
    /// <summary>
    /// Represents the context and settings for a symbolic solver session.
    /// </summary>
    public sealed class SolveContext
    {
        /// <summary>
        /// An optional property to specify the variable to solve for.
        /// </summary>
        public Symbol? TargetVariable { get; }
        
        /// <summary>
        /// A property to hold the set of rewrite rules to be used. This should be an immutable list.
        /// </summary>
        public ImmutableList<Rule> Rules { get; }
        
        /// <summary>
        /// A property for the maximum number of rewrite iterations. It should have a default value of 100 if not explicitly provided.
        /// </summary>
        public int MaxIterations { get; }
        
        /// <summary>
        /// A property to indicate if a history of transformation steps should be recorded. It should have a default value of false.
        /// </summary>
        public bool EnableTracing { get; }
        
        /// <summary>
        /// An optional flexible container for strategy-specific data, stored as an immutable dictionary.
        /// </summary>
        public ImmutableDictionary<string, object>? AdditionalData { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolveContext"/> class.
        /// </summary>
        /// <param name="targetVariable">An optional variable to solve for.</param>
        /// <param name="rules">The set of rewrite rules to be used. If null, ImmutableList<Rule>.Empty will be used.</param>
        /// <param name="maxIterations">The maximum number of rewrite iterations. Defaults to 100 if not provided.</param>
        /// <param name="enableTracing">Indicates if a history of transformation steps should be recorded. Defaults to false if not provided.</param>
        /// <param name="additionalData">An optional flexible container for strategy-specific data.</param>
        public SolveContext(
            Symbol? targetVariable = null,
            ImmutableList<Rule>? rules = null,
            int maxIterations = 100,
            bool enableTracing = false,
            ImmutableDictionary<string, object>? additionalData = null)
        {
            TargetVariable = targetVariable;
            Rules = rules ?? ImmutableList<Rule>.Empty; // Ensure Rules is never null, default to empty list.
            MaxIterations = maxIterations;
            EnableTracing = enableTracing;
            AdditionalData = additionalData; // Allows null as per property type.
        }
    }
}