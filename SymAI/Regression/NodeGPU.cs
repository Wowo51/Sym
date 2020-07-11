//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;

namespace SymAI.Regression
{
    public struct NodeGPU
    {
        public NodeGPU(double number, int operatorIndex, byte isRoot, int branch1, int branch2, int independentIndex)
        {
            Number = number;
            OperatorIndex = operatorIndex;
            IsRoot = isRoot;
            Branch1 = branch1;
            Branch2 = branch2;
            IndependentIndex = independentIndex;
        }

        public double Number { get; set; }
        public int OperatorIndex { get; }
        public byte IsRoot { get; }
        public int Branch1 { get; }
        public int Branch2 { get; }
        public int IndependentIndex { get; }
    }
}
