//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;

namespace SymRegressionApp.DataProcessing
{
    public class NumericTable
    {
        public string[] IndependentHeaders;
        public string DependantHeader;
        public double[,] Independents;
        public double[] Dependants;
    }
}
