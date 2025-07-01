//Copyright Warren Harding 2025.
using System.Collections.Immutable;
using System.Linq;
using Sym.Core;

namespace Sym.Core
{
    public abstract class Operation : Expression
    {
        public ImmutableList<IExpression> Arguments { get; init; }

        public override bool IsAtom => false;
        public override bool IsOperation => true;
        public abstract override Shape Shape { get; }

        protected Operation(ImmutableList<IExpression> arguments)
        {
            Arguments = arguments;
        }

        /// <summary>
        /// Creates a new instance of the operation with the given new arguments.
        /// This is used during rewriting when arguments are transformed and the operation needs to be reconstructed.
        /// </summary>
        /// <param name="newArgs">The new immutable list of arguments.</param>
        /// <returns>A new instance of the specific operation type with the updated arguments.</returns>
        public abstract Operation WithArguments(ImmutableList<IExpression> newArgs);

        public override bool InternalEquals(IExpression other)
        {
            if (other is not Operation otherOp || GetType() != otherOp.GetType())
            {
                return false;
            }

            if (Arguments.Count != otherOp.Arguments.Count)
            {
                return false;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                // InternalEquals comparison for arguments must be used here
                if (!Arguments[i].InternalEquals(otherOp.Arguments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public abstract override int InternalGetHashCode();
    }
}
