//Copyright Warren Harding 2025.
using Sym.Core;

namespace Sym.Core
{
    public abstract class Atom : Expression
    {
        public override bool IsAtom => true;
        public override bool IsOperation => false;
    }
}
