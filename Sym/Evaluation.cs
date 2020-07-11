//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using Sym.Nodes;

namespace Sym
{
    public class Evaluation
    {
        public static double Evaluate(Node inNode)
        {
            if (inNode is OperatorNode)
            {
                OperatorNode operatorNode = (OperatorNode)inNode;
                List<double> operands = new List<double>();
                foreach (Node child in inNode.Children)
                {
                    double operand = Evaluate(child);
                    operands.Add(operand);
                }
                if (operatorNode.Operator.Function != null)
                {
                    return operatorNode.Operator.Function(operands);
                }
                return double.NaN;
            }
            else if (inNode is NumericNode)
            {
                NumericNode numericNode = (NumericNode)inNode;
                return numericNode.Number;
            }
            else if (inNode is VariableNode)
            {
                VariableNode variableNode = (VariableNode)inNode;
                return variableNode.Number;
            }
            return double.NaN;
        }
    }
}
