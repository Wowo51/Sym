using System;
using System.Collections.Generic;
using System.Text;
using Sym.Nodes;
using System.Linq;

namespace Sym.Goals
{
    public class LengthGoal : Goal
    {
        public double Score = -1d;

        public LengthGoal(double score = -1d)
        {
            Score = score;
        }

        public override double CalculateGoalFitness(Node potentialSolution)
        {
            return Score * potentialSolution.DescendantsAndSelf().Count;
        }
    }
}
