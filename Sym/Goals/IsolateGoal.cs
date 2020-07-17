//Copyright 2020 Warren Harding, released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using Sym;
using Sym.Nodes;
using System.Linq;

namespace Sym.Goals
{
    public class IsolateGoal : Goal
    {
        public string VariableBeingSolvedFor;
        public double RightMatchScore = -1000d;
        public double LeftMatchScore = 0d;
        public double SingleVariableOnLeftScore = 10000;
        public double VariablesScore = 0;
        public double VariableCountScore = -100;
        public bool SystemMode = false;

        public IsolateGoal(string variableBeingSolvedFor, bool systemMode)
        {
            VariableBeingSolvedFor = variableBeingSolvedFor;
            SystemMode = systemMode;
        }

        public override double CalculateGoalFitness(Node potentialSolution)
        {
            if (potentialSolution is OperatorNode)
            {
                OperatorNode potentialSolutionOperator = (OperatorNode)potentialSolution;
                if (potentialSolutionOperator.Operator.OperatorString == "=" && potentialSolution.Children.Count == 2)
                {
                    Node rightTree = potentialSolution.Children[1];
                    List<Node> rightDescendants = rightTree.DescendantsAndSelf();
                    List<VariableNode> rightVariableNodes = rightDescendants.Where(x => x is VariableNode).Select(x => (VariableNode)x).ToList();
                    List<VariableNode> rightMatches = rightVariableNodes.Where(x => x.Variable == VariableBeingSolvedFor).ToList();
                    Node leftTree = potentialSolution.Children[0];
                    List<Node> leftDescendants = leftTree.DescendantsAndSelf();
                    List<VariableNode> leftVariableNodes = leftDescendants.Where(x => x is VariableNode).Select(x => (VariableNode)x).ToList();
                    List<VariableNode> leftMatches = leftVariableNodes.Where(x => x.Variable == VariableBeingSolvedFor).ToList();
                    double rightScorePenalty = (double)rightMatches.Count * RightMatchScore;
                    double leftScorePenalty = (double)leftMatches.Count * LeftMatchScore;
                    double score = rightScorePenalty + leftScorePenalty;
                    //string lStr = Node.Join(potentialSolution);
                    if (leftVariableNodes.Count == 1 && leftDescendants.Count == 1 && rightMatches.Count == 0)
                    {
                        score += SingleVariableOnLeftScore;
                    }
                    score += VariablesScore * (double)(leftVariableNodes.Count + rightVariableNodes.Count);
                    if (SystemMode)
                    {
                        int variableCount = potentialSolution.DescendantsAndSelf().Where(x => x is VariableNode).Select(x => (VariableNode)(x)).GroupBy(x => x.Variable).ToList().Count;
                        if (variableCount > 1)
                        {
                            score += variableCount * VariableCountScore;
                        }
                    }
                    //string lStr = Node.Join(potentialSolution);
                    return score;
                }
            }
            return double.NaN;
        }
    }
}
