//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using Sym;
using Sym.Nodes;
using System.Linq;

namespace SymAI.Regression
{
    public class Forecasting
    {
        public static double ForecastFast(List<double> independents, List<Node> modelBranches, List<Pointer> pointers)
        {
            Node.SetNumbers(pointers, independents, modelBranches);
            return Evaluation.Evaluate(modelBranches.Last());
        }

        public static double Forecast(double[] independents, Node model)
        {
            List<Node> branches = model.DescendantsAndSelf();
            List<Pointer> pointers = Node.BuildPointers("Independent", branches);
            return ForecastFast(independents.ToList(), branches, pointers);
        }

        public static double ScaleAndOffsetForecast(double forecast, double scale, double offset)
        {
            return forecast * scale + offset;
        }

    }
}
