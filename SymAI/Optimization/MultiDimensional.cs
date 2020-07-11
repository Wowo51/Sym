//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SymAI.Regression;

namespace SymAI.Optimization
{
    public class MultiDimensional
    {
        public List<OneDimensional> OneDimensionals = new List<OneDimensional>();
        public event PostIndividualDelegate PostIndividual;
        public int CurrentDimensionIndex = -1;
        public double[] Independents;

        public void Initialize(double[] independents)
        {
            //Independents = RandomHelpers.CreateArray(totalDimensions);
            Independents = independents;
            foreach (OneDimensional oneDimensional in OneDimensionals)
            {
                oneDimensional.PostIndividual -= OneDimensional_PostIndividual;
            }
            OneDimensionals = new List<OneDimensional>();
            for (int i = 0; i < Independents.Length; i++)
            {
                OneDimensional oneDimensional = new OneDimensional();
                oneDimensional.DimensionIndex = i;
                oneDimensional.PostIndividual += OneDimensional_PostIndividual;
                oneDimensional.Independents = Independents;
                OneDimensionals.Add(oneDimensional);
            }
        }

        private void OneDimensional_PostIndividual(double[] independents, int dimensionIndex, int stepIndex)
        {
            PostIndividual(independents, dimensionIndex, stepIndex);
        }

        public void Run()
        {
            if (IsOptimized())
            {
                return;
            }

            CurrentDimensionIndex++;
            if (CurrentDimensionIndex >= OneDimensionals.Count)
            {
                CurrentDimensionIndex = 0;
            }
            if (CurrentDimensionIndex < OneDimensionals.Count)
            {
                OneDimensionals[CurrentDimensionIndex].Run();
            }
        }

        public bool IsOptimized()
        {
            return OneDimensionals.All(x => x.IsOptimized());
        }

        public void SetIndependents(double[] independents)
        {
            Independents = independents;
            foreach (OneDimensional oneDimensional in OneDimensionals)
            {
                oneDimensional.Independents = independents;
            }
        }
    }
}
