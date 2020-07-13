//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using ILGPU;
using ILGPU.IR;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.OpenCL;
using ILGPU.Runtime.Cuda;
using ILGPU.Algorithms;

namespace SymAI.Regression
{
    public class GPUManager
    {
        public AcceleratorId AcceleratorId;
        public Accelerator Accelerator;
        public Action<Index2, ArrayView2D<double>, ArrayView<double>, ArrayView<NodeGPU>, ArrayView<int>, ArrayView2D<double>> EvaluationKernel;
        public Action<Index1, ArrayView2D<double>, ArrayView<double>> ProcessResultsKernel;
        public MemoryBuffer2D<double> Independents;
        public MemoryBuffer<double> Dependants;
        public MemoryBuffer<NodeGPU> Nodes;
        public MemoryBuffer<int> NodeArrayStarts;
        public MemoryBuffer2D<double> Results2D;
        public MemoryBuffer<double> Results1D;
        public double[] Results;
        Index2 IndependentsTableSize;

        public void Initialize(Context context, AcceleratorId acceleratorId, double[,] independents, double[] dependants)
        {
            AcceleratorId = acceleratorId;
            AcceleratorType acceleratorType = AcceleratorId.AcceleratorType;
            if (acceleratorType == AcceleratorType.CPU)
            {
                Accelerator = Accelerator.Create(context, AcceleratorId);
            }
            else if (acceleratorType == AcceleratorType.OpenCL)
            {
                Accelerator = CLAccelerator.Create(context, AcceleratorId);
            }
            else if (acceleratorType == AcceleratorType.Cuda)
            {
                Accelerator = CudaAccelerator.Create(context, AcceleratorId);
            }
            EvaluationKernel = Accelerator.LoadAutoGroupedStreamKernel<Index2, ArrayView2D<double>, ArrayView<double>, ArrayView<NodeGPU>, ArrayView<int>, ArrayView2D<double>>(EvaluationKernelFunction);
            ProcessResultsKernel = Accelerator.LoadAutoGroupedStreamKernel<Index1, ArrayView2D<double>, ArrayView<double>>(ProcessResultsKernelFunction);
            IndependentsTableSize = new Index2(independents.GetUpperBound(0) + 1, independents.GetUpperBound(1) + 1);
            Independents = Accelerator.Allocate<double>(IndependentsTableSize);
            Independents.CopyFrom(independents, new Index2(), new Index2(), IndependentsTableSize);
            Dependants = Accelerator.Allocate<double>(dependants.Length);
            Dependants.CopyFrom(dependants, 0, 0, dependants.Length);
        }

        public void Run(NodeGPU[] nodes, int[] nodeArrayStarts)
        {
            Nodes = Accelerator.Allocate<NodeGPU>(nodes.Length);
            Nodes.CopyFrom(nodes, new Index1(), new Index1(), nodes.Length);
            NodeArrayStarts = Accelerator.Allocate<int>(nodeArrayStarts.Length);
            NodeArrayStarts.CopyFrom(nodeArrayStarts, 0, new Index1(), nodeArrayStarts.Length);
            Results2D = Accelerator.Allocate<double>(nodeArrayStarts.Length, IndependentsTableSize.X);
            Results1D = Accelerator.Allocate<double>(nodeArrayStarts.Length);
            Index2 index = new Index2(nodeArrayStarts.Length, IndependentsTableSize.X);
            EvaluationKernel(index, Independents, Dependants, Nodes, NodeArrayStarts, Results2D);
            Accelerator.Synchronize();
            ProcessResultsKernel(nodeArrayStarts.Length, Results2D, Results1D);
            Accelerator.Synchronize();
            Results = new double[nodeArrayStarts.Length];
            Results1D.CopyTo(Results, new Index1(), 0, Results1D.Extent);
            Nodes.Dispose();
            NodeArrayStarts.Dispose();
            Results2D.Dispose();
            Results1D.Dispose();
        }

        ~GPUManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            Independents.Dispose();
            Dependants.Dispose();
            Nodes.Dispose();
            Results2D.Dispose();
            Results1D.Dispose();
            Accelerator.Dispose();
        }

        static void EvaluationKernelFunction(Index2 index, ArrayView2D<double> independents, ArrayView<double> dependants, ArrayView<NodeGPU> nodes, ArrayView<int> nodeArrayStarts, ArrayView2D<double> results)
        {
            results[index.X, index.Y] = double.NaN;
            double forecast = Evaluate(index.X, index.Y, independents, nodes, nodeArrayStarts);
            double dependant = dependants[index.Y];
            //double logDependant = XMath.Log(dependant);
            //double logForecast = XMath.Log(forecast);
            //double diff = logDependant - logForecast;
            double diff = dependant - forecast;
            double error = diff * diff;
            results[index.X, index.Y] = error;
        }

        static double Evaluate(int individualIndex, int independentsRowIndex, ArrayView2D<double> independents, ArrayView<NodeGPU> nodes, ArrayView<int> nodeArrayStarts)
        {
            for (int nodeIndex = 0; nodeIndex < nodeArrayStarts.Length; nodeIndex++)
            {
                Index1 currentNodeIndex = new Index1(nodeArrayStarts[individualIndex] + nodeIndex);
                //NodeGPU currentNode = nodes[currentNodeIndex];
                if (nodes[currentNodeIndex].IndependentIndex >= 0)
                {
                    int independentIndex = nodes[currentNodeIndex].IndependentIndex;
                    nodes[currentNodeIndex].Number = independents[independentsRowIndex, independentIndex];
                }
                else if (nodes[currentNodeIndex].OperatorIndex >= 0)
                {
                    Index1 branchIndex1 = new Index1(nodeArrayStarts[individualIndex] + nodes[currentNodeIndex].Branch1);
                    Index1 branchIndex2 = new Index1(nodeArrayStarts[individualIndex] + nodes[currentNodeIndex].Branch2);
                    if (nodes[currentNodeIndex].OperatorIndex < 6)
                    {
                        if (nodes[currentNodeIndex].OperatorIndex < 4)
                        {
                            if (nodes[currentNodeIndex].OperatorIndex == 2)
                            {
                                nodes[currentNodeIndex].Number = nodes[branchIndex1].Number + nodes[branchIndex2].Number;
                            }
                            else if (nodes[currentNodeIndex].OperatorIndex == 3)
                            {
                                nodes[currentNodeIndex].Number = nodes[branchIndex1].Number - nodes[branchIndex2].Number;
                            }
                        }
                        else
                        {
                            if (nodes[currentNodeIndex].OperatorIndex == 4)
                            {
                                nodes[currentNodeIndex].Number = nodes[branchIndex1].Number * nodes[branchIndex2].Number;
                            }
                            else if (nodes[currentNodeIndex].OperatorIndex == 5)
                            {
                                nodes[currentNodeIndex].Number = nodes[branchIndex1].Number / nodes[branchIndex2].Number;
                            }
                        }
                    }
                    else // >= 6
                    {
                        if (nodes[currentNodeIndex].OperatorIndex == 6)
                        {
                            nodes[currentNodeIndex].Number = -nodes[branchIndex1].Number;
                        }
                        else if (nodes[currentNodeIndex].OperatorIndex == 8)
                        {
                            nodes[currentNodeIndex].Number = XMath.Sin(nodes[branchIndex1].Number);
                        }
                        else if (nodes[currentNodeIndex].OperatorIndex == 9)
                        {
                            nodes[currentNodeIndex].Number = XMath.Cos(nodes[branchIndex1].Number);
                        }
                        else if (nodes[currentNodeIndex].OperatorIndex == 14)
                        {
                            nodes[currentNodeIndex].Number = XMath.Pow(nodes[branchIndex1].Number, nodes[branchIndex2].Number);
                        }
                        else if (nodes[currentNodeIndex].OperatorIndex == 15)
                        {
                            nodes[currentNodeIndex].Number = XMath.Sign(nodes[branchIndex1].Number);
                        }
                    }
                    if (nodes[currentNodeIndex].Number == double.NaN)
                    {
                        return double.NaN;
                    }
                }

                if (nodes[currentNodeIndex].IsRoot == 1)
                {
                    return nodes[currentNodeIndex].Number;
                }
            }
            return double.NaN;
        }

        static void ProcessResultsKernelFunction(Index1 individualIndex, ArrayView2D<double> results2D, ArrayView<double> results1D)
        {
            for (int rowIndex = 0; rowIndex < results2D.Height; rowIndex++)
            {
                results1D[individualIndex] += results2D[individualIndex, rowIndex];
            }
        }
    }
}
