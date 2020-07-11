//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;

namespace Sym.Nodes
{
    public enum VariableNodeType { RequireVariable, RequireNumber, NoRequirement }
    public class VariableNode : Node
    {
        public string Variable;
        public VariableNodeType VariableNodeType;
        public double Number = double.NaN;
    }
}
