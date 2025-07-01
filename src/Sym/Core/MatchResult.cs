//Copyright Warren Harding 2025.
using System.Collections.Immutable;
using Sym.Atoms; // Required for WildConstraint check
using System.Linq; // For OrderBy in GetHashCode
using System.Collections.Generic; // For KeyValuePair

namespace Sym.Core
{
    /// <summary>
    /// Represents the result of a pattern matching attempt,
    /// including whether the match was successful and any bound wildcard values.
    /// </summary>
    public sealed class MatchResult
    {
        /// <summary>
        /// Indicates whether the pattern match was successful.
        /// </summary>
        public bool Success { get; init; }
        /// <summary>
        /// A dictionary of wildcard names to their matched expressions.
        /// </summary>
        public ImmutableDictionary<string, IExpression> Bindings { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchResult"/> class.
        /// </summary>
        /// <param name="success">Indicates if the match was successful.</param>
        /// <param name="bindings">The collected bindings from the match.</param>
        public MatchResult(bool success, ImmutableDictionary<string, IExpression> bindings)
        {
            Success = success;
            Bindings = bindings;
        }

        /// <summary>
        /// Creates a failed match result with empty bindings.
        /// </summary>
        /// <returns>A failed MatchResult.</returns>
        public static MatchResult Fail()
        {
            return new MatchResult(false, ImmutableDictionary<string, IExpression>.Empty);
        }

        /// <summary>
        /// Determines whether this MatchResult instance is equal to another object.
        /// Overridden to provide value equality for the Bindings dictionary.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not MatchResult other || Success != other.Success)
            {
                return false;
            }

            if (Bindings.Count != other.Bindings.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, IExpression> kvp in Bindings)
            {
                if (!other.Bindings.TryGetValue(kvp.Key, out IExpression? otherValue))
                {
                    return false; // Key not found in other dictionary
                }
                // Use IExpression's overridden Equals, which itself canonicalizes before internal comparison.
                if (!kvp.Value.Equals(otherValue))
                {
                    return false; // Values for the same key are not equal
                }
            }
            return true;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// Overridden to provide a hash code consistent with value equality for the Bindings dictionary.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Success);
            // Compute hash code for immutable dictionary elements.
            // OrderBy ensures consistent hash code regardless of dictionary iteration order.
            foreach (KeyValuePair<string, IExpression> kvp in Bindings.OrderBy(kv => kv.Key))
            {
                hash.Add(kvp.Key);
                hash.Add(kvp.Value); // This calls IExpression.GetHashCode, which canonicalizes.
            }
            return hash.ToHashCode();
        }
    }
}