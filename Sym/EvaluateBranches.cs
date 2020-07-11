//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using Sym.Nodes;

namespace Sym
{
    public class EvaluateBranches
    {

        public static Node Evaluate(Node inNode)
        {
            bool isCalculatable = IsCalculatable(inNode);
            if (isCalculatable)
            {
                double evaluation = Evaluation.Evaluate(inNode);
                NumericNode newNode = new NumericNode();
                newNode.Number = evaluation;
                return newNode;
            }
            else
            {
                List<Node> newChildren = new List<Node>();
                foreach (Node child in inNode.Children)
                {
                    Node newChild = Evaluate(child);
                    newChildren.Add(newChild);
                }
                inNode.Children = newChildren;
                return inNode;
            }
        }

        public static bool IsCalculatable(Node inNode)
        {
            if (inNode is NumericNode)
            {
                return true;
            }
            if (inNode is OperatorNode)
            {
                OperatorNode operatorNode = (OperatorNode)inNode;
                if (operatorNode.Operator.Function != null)
                {
                    foreach (Node child in operatorNode.Children)
                    {
                        bool isCalculatable = IsCalculatable(child);
                        if (isCalculatable == false)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
