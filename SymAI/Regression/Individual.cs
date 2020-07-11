//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;

namespace SymAI.Regression
{
    public class Individual
    {
        public Model Model;
        public int DimensionIndex;
        public int StepIndex;
        public double Fitness;
        public double LengthAdjustedFitness;
        public double[] Constants;
    }
}
