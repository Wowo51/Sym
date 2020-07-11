using System;
using System.Collections.Generic;
using System.Text;
using Sym.Nodes;
using System.Linq;

namespace Sym.Goals
{
    public class PreferOneVariableGoal : Goal
    {
        public double Score = 100d;

        public PreferOneVariableGoal(double score = 100d)
        {
            Score = score;
        }

        public override double CalculateGoalFitness(Node potentialSolution)
        {
            int variableCount = potentialSolution.DescendantsAndSelf().Where(x => x is VariableNode).Select(x => (VariableNode)(x)).GroupBy(x => x.Variable).ToList().Count;
            if (variableCount > 1)
            {
                return -variableCount * 5000;
            }
            return 0;
        }
    }
}
