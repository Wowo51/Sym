//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using Sym;
using Sym.Nodes;
using SymAI.Optimization;
using System.Linq;

namespace SymAI.Regression
{
    public class Model
    {
        public Node Expression;
        public string ExpressionString;
        //public int TotalConstants;
        public double Fitness = double.MinValue;
        public double LengthAdjustedFitness = double.MinValue;
        public MultiDimensional Optimizer = new MultiDimensional();
        public bool ModelOptimizerIsInitialized = false;
        public event Action<Individual> PostIndividual;

        public Model()
        {
            Optimizer.PostIndividual += Optimizer_PostIndividual;
        }

        private void Optimizer_PostIndividual(double[] independents, int dimensionIndex, int stepIndex)
        {
            Individual individual = new Individual();
            individual.Constants = independents;
            individual.DimensionIndex = dimensionIndex;
            individual.StepIndex = stepIndex;
            individual.Model = this;
            PostIndividual(individual);
        }

        public void Optimize()
        {
            if (ModelOptimizerIsInitialized == false)
            {
                double[] initialConstants = GetInitialConstants();
                Optimizer.Initialize(initialConstants);
                ModelOptimizerIsInitialized = true;
            }
            Optimizer.Run();
        }

        public double[] GetInitialConstants()
        {
            double[] constants = Expression.DescendantsAndSelf().Where(x => x is VariableNode)
                .Where(x => ((VariableNode)x).Variable.Substring(0, 8) == "Constant")
                .Select(x => ((VariableNode)x).Number).ToArray();
            for (int i = 0; i < constants.Length; i++)
            {
                if (double.IsNaN(constants[i]))
                {
                    constants[i] = RandomHelpers.Exponential(1);
                }
            }
            return constants;

            //int totalConstants = Expression.DescendantsAndSelf().Where(x => x is VariableNode)
            //    .Where(x => ((VariableNode)x).Variable.Substring(0, 8) == "Constant").ToList().Count;
            //double[] constants = new double[totalConstants];
            //for (int i = 0; i < constants.Length; i++)
            //{
            //    constants[i] = RandomHelpers.Exponential(1);
            //}
            //return constants;
        }

        public Model Clone()
        {
            Model clone = new Model();
            clone.Expression = Expression.Clone();
            clone.ExpressionString = Node.Join(Expression);
            //clone.t = TotalConstants;
            return clone;
        }
    }
}
