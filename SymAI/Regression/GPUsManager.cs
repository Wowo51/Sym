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
using System.Linq;
using System.Threading.Tasks;

namespace SymAI.Regression
{
    public class GPUsManager
    {
        public List<GPUManager> GPUManagers = new List<GPUManager>();
        public Context Context;
        public double TotalIndependentsRows;

        public void Initialize(AcceleratorType acceleratorType, double[,] independents, double[] dependants)
        {
            Context = new Context();
            List<AcceleratorId> acceleratorIds = Accelerator.Accelerators.Where(x => x.AcceleratorType == acceleratorType).ToList();
            List<double[,]> splitIndependents = SplitArray(independents, acceleratorIds.Count);
            List<double[]> splitDependants = SplitArray(dependants, acceleratorIds.Count);
            for (int gpuIndex = 0; gpuIndex < acceleratorIds.Count; gpuIndex++)
            {
                GPUManager gpuManager = new GPUManager();
                gpuManager.Initialize(Context, acceleratorIds[gpuIndex], splitIndependents[gpuIndex], splitDependants[gpuIndex]);
                GPUManagers.Add(gpuManager);
            }
            TotalIndependentsRows = splitDependants.Select(x => x.Length).Sum();
        }

        public double[] Run(NodeGPU[] nodes, int[] nodeArrayStarts)
        {
            List<Task> tasks = new List<Task>();
            foreach (GPUManager gpuManager in GPUManagers)
            {
                tasks.Add(Task.Factory.StartNew(() => gpuManager.Run(nodes, nodeArrayStarts)));
            }
            Task.WaitAll(tasks.ToArray());
            int totalIndividuals = nodeArrayStarts.Length;
            double[] results = new double[totalIndividuals];
            double gpuCount = (double)GPUManagers.Count;
            for (int individualIndex = 0; individualIndex < totalIndividuals; individualIndex++)
            {
                for (int gpuIndex = 0; gpuIndex < GPUManagers.Count; gpuIndex++)
                {
                    results[individualIndex] += GPUManagers[gpuIndex].Results[individualIndex];
                }
                results[individualIndex] /= TotalIndependentsRows;
                results[individualIndex] = Math.Sqrt(results[individualIndex]);
            }
            return results;
        }

        public static List<double[,]> SplitArray(double[,] inArray, int totalSubArrays)
        {
            int totalX = inArray.GetUpperBound(0) + 1;
            int totalY = inArray.GetUpperBound(1) + 1;
            int totalSubX = (int)Math.Floor((double)totalX / (double)totalSubArrays);
            List<double[,]> outArrays = new List<double[,]>();
            for (int i = 0; i < totalSubArrays; i++)
            {
                outArrays.Add(new double[0, 0]);
            }
            Parallel.For(0, totalSubArrays, subArrayIndex =>
            {
                double[,] outArray = new double[totalSubX, totalY];
                for (int x = 0; x < totalSubX; x++)
                {
                    for (int y = 0; y < totalY; y++)
                    {
                        outArray[x, y] = inArray[subArrayIndex * totalSubX + x, y];
                    }
                }
                outArrays[subArrayIndex] = outArray;
            });
            return outArrays;
        }

        public static List<double[]> SplitArray(double[] inArray, int totalSubArrays)
        {
            int totalSub = (int)Math.Floor((double)inArray.Length / (double)totalSubArrays);
            List<double[]> outArrays = new List<double[]>();
            for (int subArrayIndex = 0; subArrayIndex < totalSubArrays; subArrayIndex++)
            {
                double[] outArray = new double[totalSub];
                for (int i = 0; i < totalSub; i++)
                {
                    outArray[i] = inArray[subArrayIndex * totalSub + i];
                }
                outArrays.Add(outArray);
            }
            return outArrays;
        }


        ~GPUsManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            Context.Dispose();
            foreach (GPUManager gpuManager in GPUManagers)
            {
                gpuManager.Dispose();
            }
        }
    }
}
