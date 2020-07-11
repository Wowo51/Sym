//Copyright 2020 Warren Harding, released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using Sym;
using Sym.Nodes;
using System.Linq;

namespace Sym.Goals
{
    public class TransformGoal : Goal
    {
        public Transform Transform;
        public double Score;
        public List<Operator> Operators;

        public TransformGoal(double score, string transform, List<Operator> operators)
        {
            Score = score;
            Transform = Transform.StringToTransform(transform, operators);
            Operators = operators;
        }

        public override double CalculateGoalFitness(Node potentialSolution)
        {
            int goalAchievedCount = TransformBranchFunctions.TransformBranchesWithTransform(potentialSolution, Transform, Operators).Count;
            return (double)goalAchievedCount * Score;
        }

        public static List<Goal> StandardFormTransformGoals(List<Operator> operators)
        {
            List<Goal> goals = new List<Goal>();
            goals.Add(new TransformGoal(1d, "C*V~0", operators));
            goals.Add(new TransformGoal(-1d, "V*C~0", operators));
            return goals;
        }


    }
}
