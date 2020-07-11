//Copyright 2020 Warren Harding, released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using Sym;
using Sym.Nodes;
using System.Linq;

namespace Sym.Goals
{
    public class Goal
    {
        //public double Score;

        public virtual double CalculateGoalFitness(Node potentialSolution)
        {
            return double.NaN;
        }

        public static double CalculateGoalFitness(Node potential, List<Goal> goals)
        {
            double goalFitness = 0;
            foreach (Goal goal in goals)
            {
                goalFitness += goal.CalculateGoalFitness(potential);
            }
            return goalFitness;
        }


    }
}
