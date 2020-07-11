//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;

namespace SymAI.Regression
{
    public class RandomHelpers
    {
        static Random Random = new Random();

        public static int ExponentialSelector(int totalItems, double exponent)
        {
            //this function will select with a preference towards 0
            if (totalItems == 1)
            {
                return 0;
            }
            double r = Random.NextDouble();
            double cap = (double)(totalItems - 1);
            r = Math.Pow(r, exponent);
            r = r * cap;
            if (r > cap)
            {
                r = cap;
            }
            if (r < 0)
            {
                r = 0;
            }
            return (int)Math.Round(r);
        }

        public static double Exponential(double exponent)
        {
            double r = Random.NextDouble();
            r = Math.Pow(r, exponent);
            int sign = Random.Next(2);
            if (sign == 0)
            {
                return r;
            }
            else
            {
                return -r;
            }
        }

        //public static double[] CreateArray(int length)
        //{
        //    double[] outArray = new double[length];
        //    for (int i = 0; i < length; i++)
        //    {
        //        outArray[i] = Exponential();
        //    }
        //    return outArray;
        //}
    }
}
