//Copyright Warren Harding 2025.
using Sym.Core;

namespace Sym.Core
{
    public interface IExpression
    {
        Shape Shape { get; }
        IExpression Canonicalize();
        bool IsAtom { get; }
        bool IsOperation { get; }
        string ToDisplayString();

        bool Equals(object? obj);
        int GetHashCode();

        public bool InternalEquals(IExpression other);
        public int InternalGetHashCode();
    }
}