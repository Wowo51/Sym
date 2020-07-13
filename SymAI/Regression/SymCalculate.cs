using System;
using System.Collections.Generic;
using System.Text;
using Sym;
using Sym.Nodes;
using System.Linq;
using System.Threading.Tasks;

namespace SymAI.Regression
{
    public class SymCalculate
    {
        public static double[] CalculateErrors(double[][] independents, double[] dependants, List<Individual> individuals)
        {
            List<Node> nodes = new List<Node>();
            foreach (Individual individual in individuals)
            {
                Node node = individual.Model.Expression.Clone();
                List<Node> branches = node.DescendantsAndSelf();
                List<Pointer> pointers = Node.BuildPointers("Constant", branches);
                List<double> numbers = individual.Model.Optimizer.Independents.ToList();
                Node.SetNumbers(pointers, numbers, branches);
                nodes.Add(node);
            }
            return CalculateErrors(independents, dependants, nodes);
        }

        public static double[] CalculateErrors(double[][] independents, double[] dependants, List<Node> nodes)
        {
            double[] errors = new double[nodes.Count];
            Parallel.For(0, nodes.Count, nodeIndex =>
            {
                errors[nodeIndex] = CalculateError(independents, dependants, nodes[nodeIndex]);
            });
            return errors;
        }

        public static double CalculateError(double[][] independents, double[] dependants, Node node)
        {
            int totalRows = independents.Length;
            //int totalColumns = independents[0].Length;
            List<Node> branches = node.DescendantsAndSelf();
            List<Pointer> pointers = Node.BuildPointers("Independent", branches);
            double error = 0;
            for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
            {
                double[] numbers = independents[rowIndex];
                Node.SetNumbers(pointers, numbers, branches);
                double forecast = Evaluation.Evaluate(node);
                double dependant = dependants[rowIndex];
                double diff = dependant - forecast;
                error += diff * diff;
            }
            error = Math.Sqrt(error / (double)totalRows);

            return error;
        }
    }
}
