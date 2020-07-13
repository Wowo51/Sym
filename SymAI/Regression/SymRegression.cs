//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using ILGPU;
//using ILGPU.IR;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.OpenCL;
using ILGPU.Runtime.Cuda;
using Sym;
using Sym.Nodes;
using System.Linq;
using SymAI.Optimization;
using System.Threading.Tasks;

namespace SymAI.Regression
{
    public class SymRegression
    {
        public ModelsManager ModelManager = new ModelsManager();
        public GPUsManager GPUsManager = new GPUsManager();
        public List<Individual> Individuals = new List<Individual>();
        public int MaxNodesPerExpression;
        public event Action FinishedRegressing;
        public event Action FinishedGeneration;
        public int IndividualsTestedCount = 0;
        public bool IsRunning = false;
        public bool AdjustForLength = true;
        public double PenalizeLengthFactor = .001d;
        public bool UseSymCalculate = false;
        public double[][] Independents;
        public double[] Dependants;

        public SymRegression()
        {
            ModelManager.PostIndividual += ModelManager_PostIndividual;
            ModelManager.FinishedGeneration += ModelManager_FinishedGeneration;
            ModelManager.FinishedRegressing += ModelManager_FinishedRegressing;
            ModelManager.Calculate += ModelManager_Calculate;
        }

        public void Run(double[,] independents, double[] dependants, int maxNodesPerExpression, AcceleratorType acceleratorType, List<string> startModels, List<Transform> transforms)
        {
            if (IsRunning)
            {
                return;
            }
            IsRunning = true;
            if (UseSymCalculate)
            {
                Independents = ToJagged(independents);
                Dependants = dependants;
            }
            else
            {
                GPUsManager.Initialize(acceleratorType, independents, dependants);
            }
            MaxNodesPerExpression = maxNodesPerExpression;
            ModelManager.MaxNodesPerExpression = maxNodesPerExpression;
            ModelManager.CorrelationItems = Correlation.ComputeRankedCorrelationItems(independents, dependants);
            ModelManager.Run(independents.GetUpperBound(1) + 1, startModels, transforms);
            IsRunning = false;
        }

        public double[][] ToJagged(double[,] independents)
        {
            int totalRows = independents.GetUpperBound(0) + 1;
            int totalColumns = independents.GetUpperBound(1) + 1;
            double[][] jagged = new double[totalRows][];
            Parallel.For(0, totalRows, rowIndex =>
            {
                jagged[rowIndex] = GetRow(independents, rowIndex);
            });
            return jagged;
        }

        public static double[] GetRow(double[,] table, int rowIndex)
        {
            int totalColumns = table.GetUpperBound(1) + 1;
            List<double> outRow = new List<double>();
            for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
            {
                outRow.Add(table[rowIndex, columnIndex]);
            }
            return outRow.ToArray();
        }

        private void ModelManager_PostIndividual(Individual individual)
        {
            Individuals.Add(individual);
        }

        private void ModelManager_Calculate()
        {
            if (Individuals.Count == 0)
            {
                return;
            }

            if (UseSymCalculate)
            {
                double[] symResults = SymCalculate.CalculateErrors(Independents, Dependants, Individuals);
                UpdateIndividuals(symResults, Individuals, AdjustForLength, PenalizeLengthFactor);
            }
            else
            {
                int[] nodeArrayStarts;
                NodeGPU[] nodeGrid = NodesToNodesGPU(Individuals, out nodeArrayStarts);
                double[] results = GPUsManager.Run(nodeGrid, nodeArrayStarts);
                UpdateIndividuals(results, Individuals, AdjustForLength, PenalizeLengthFactor);
            }

            List<Individual> bestIndividuals = GetBestIndividualForEachModelAndUpdateOptimizer(Individuals);
            PushBestResultsBackToModels(bestIndividuals);
            List<Model> models = Individuals.Select(x => x.Model).Distinct().ToList();
            IndividualsTestedCount += Individuals.Count;
            Individuals = new List<Individual>();
        }



        public static NodeGPU[] NodesToNodesGPU(List<Individual> individuals, out int[] nodeArrayStarts)
        {
            List<NodeGPU> nodeGrid = new List<NodeGPU>();
            List<int> nodeArrayStartsList = new List<int>();
            int nodeArrayStart = 0;
            foreach (Individual individual in individuals)
            {
                NodeGPU[] nodes = NodeToNodesGPU(individual);
                nodeGrid.AddRange(nodes);
                nodeArrayStartsList.Add(nodeArrayStart);
                nodeArrayStart += nodes.Length;
            }
            nodeArrayStarts = nodeArrayStartsList.ToArray();
            return nodeGrid.ToArray();
        }

        public static NodeGPU[] NodeToNodesGPU(Individual individual)
        {
            List<Node> branches = individual.Model.Expression.DescendantsAndSelf();
            NodeGPU[] outNodes = new NodeGPU[branches.Count];
            for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
            {
                Node node = branches[branchIndex];
                double number = double.NaN;
                int operatorIndex = -1;
                int independentIndex = -1;
                byte isRoot = 0;
                int branch1 = -1;
                int branch2 = -1;
                if (branchIndex == branches.Count - 1)
                {
                    isRoot = 1;
                }
                if (node is NumericNode)
                {
                    NumericNode numericNode = (NumericNode)node;
                    number = (double)numericNode.Number;
                }
                if (node is OperatorNode)
                {
                    OperatorNode operatorNode = (OperatorNode)node;
                    operatorIndex = operatorNode.Operator.OperatorIndex;
                    branch1 = branches.IndexOf(node.Children[0]);
                    if (node.Children.Count == 2)
                    {
                        branch2 = branches.IndexOf(node.Children[1]);
                    }
                }
                if (node is VariableNode)
                {
                    VariableNode variableNode = (VariableNode)node;
                    if (variableNode.Variable.Substring(0, 11) == "Independent")
                    {
                        independentIndex = Convert.ToInt32(variableNode.Variable.Substring(12, variableNode.Variable.Length - 13));
                        //double a = 1;
                    }
                    else if (variableNode.Variable.Substring(0, 8) == "Constant")
                    {
                        int constantIndex = Convert.ToInt32(variableNode.Variable.Substring(9, variableNode.Variable.Length - 10));
                        number = individual.Constants[constantIndex];
                    }
                }
                outNodes[branchIndex] = new NodeGPU(number, operatorIndex, isRoot, branch1, branch2, independentIndex);
            }
            return outNodes;
        }

        public static void UpdateIndividuals(double[] results, List<Individual> individuals, bool adjustForLength, double penalizeLengthFactor)
        {
            for (int individualIndex = 0; individualIndex < individuals.Count; individualIndex++)
            {
                Individual individual = individuals[individualIndex];
                if (results[individualIndex] < 0)
                {
                    individual.Fitness = double.MinValue;
                }
                else
                {
                    individual.Fitness = -results[individualIndex];
                }
                double adjustedFitness = individual.Fitness;
                if (adjustForLength)
                {
                    double totalLength = (double)individual.Model.Expression.DescendantsAndSelf().Count;
                    adjustedFitness = individual.Fitness - totalLength * penalizeLengthFactor;
                    //if (individual.Model.Fitness > 0)
                    //{
                    //    adjustedFitness = individual.Fitness - totalLength / adjustForLengthFactor;
                    //}
                    //else
                    //{
                    //    adjustedFitness = -Math.Pow(Math.Abs(individual.Fitness), adjustForLengthFactor) * totalLength;
                    //}
                }
                individual.LengthAdjustedFitness = adjustedFitness;
            }
        }

        public static List<Individual> GetBestIndividualForEachModelAndUpdateOptimizer(List<Individual> individuals)
        {
            List<Individual> bestIndividuals = new List<Individual>();
            var groups = individuals.GroupBy(x => x.Model);
            foreach (var group in groups)
            {
                Individual bestIndividual = group.OrderByDescending(x => x.LengthAdjustedFitness).FirstOrDefault();
                if (bestIndividual != null)
                {
                    if (bestIndividual.LengthAdjustedFitness > bestIndividual.Model.LengthAdjustedFitness)
                    {
                        bestIndividuals.Add(bestIndividual);
                        bestIndividual.Model.Optimizer.OneDimensionals[bestIndividual.DimensionIndex].StrikeCount = 0;
                    }
                    bestIndividual.Model.Optimizer.OneDimensionals[bestIndividual.DimensionIndex].FinishStep(bestIndividual.StepIndex);
                }
            }
            return bestIndividuals;
        }

        public static void PushBestResultsBackToModels(List<Individual> bestIndividuals)
        {
            foreach (Individual bestIndividual in bestIndividuals)
            {
                bestIndividual.Model.Optimizer.SetIndependents(bestIndividual.Constants);
                bestIndividual.Model.Fitness = bestIndividual.Fitness;
                bestIndividual.Model.LengthAdjustedFitness = bestIndividual.LengthAdjustedFitness;
            }
        }

        private void ModelManager_FinishedRegressing()
        {
            if (UseSymCalculate == false)
            {
                GPUsManager.Dispose();
            }
            FinishedRegressing();
        }

        private void ModelManager_FinishedGeneration()
        {
            FinishedGeneration();
        }
    }
}
