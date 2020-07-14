//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SymAI.Optimization
{
    public delegate void PostIndividualDelegate(double[] independents, int dimensionIndex, int stepIndex);

    public class OneDimensional
    {
        public double StepSize = .1d;
        public event PostIndividualDelegate PostIndividual;
        public int DimensionIndex = 0;
        public double[] StepSizes = new double[7];
        public double[] Independents;
        public int StrikeCount = 0;

        public void Run()
        {

            StepSizes[0] = 0;
            StepSizes[1] = StepSize / 2;
            StepSizes[2] = StepSize;
            StepSizes[3] = StepSize * 2;
            StepSizes[4] = -StepSize / 2;
            StepSizes[5] = -StepSize;
            StepSizes[6] = -StepSize * 2;

            for (int stepSizeIndex = 0; stepSizeIndex < 7; stepSizeIndex++)
            {
                double[] independents = Independents.ToArray();                
                independents[DimensionIndex] = Independents[DimensionIndex] + StepSizes[stepSizeIndex];
                PostIndividual(independents, DimensionIndex, stepSizeIndex);
            }
        }

        public void FinishStep(int stepIndex)
        {
            //StepSize = StepSizes[stepIndex];
            if (stepIndex > 0)
            {
                StepSize *= Math.Abs(StepSizes[stepIndex]);
            }
            else
            {
                StepSize /= 10;
                StrikeCount++;
            }
        }

        public bool IsOptimized()
        {
            if (StrikeCount > 2)
            {
                return true;
            }
            return false;
        }

    }
}
