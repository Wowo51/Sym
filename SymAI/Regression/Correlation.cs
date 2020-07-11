//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace SymAI.Regression
{
    public class Correlation
    {
        public static CorrelationItem[] ComputeRankedCorrelationItems(double[,] independents, double[] dependants)
        {
            double[] correlations = ComputeCorrelationCoefficients(independents, dependants);
            List<CorrelationItem> correlationItems = new List<CorrelationItem>();
            for (int columnIndex = 0; columnIndex < correlations.Length; columnIndex++)
            {
                CorrelationItem item = new CorrelationItem();
                item.ColumnIndex = columnIndex;
                item.Correlation = correlations[columnIndex];
                correlationItems.Add(item);
            }
            return correlationItems.OrderByDescending(x => Math.Abs(x.Correlation)).ToArray();
        }

        public static double[] ComputeCorrelationCoefficients(double[,] independents, double[] dependants)
        {
            int totalRows = independents.GetUpperBound(0) + 1;
            int totalColumns = independents.GetUpperBound(1) + 1;
            double[] outCorrelations = new double[totalColumns];
            Parallel.For(0, totalColumns, columnIndex =>
            {
                double[] independentsColumn = GetColumn(independents, columnIndex);
                outCorrelations[columnIndex] = ComputeCorrelationCoefficient(independentsColumn, dependants);
            });
            return outCorrelations;
        }

        public static double[] GetColumn(double[,] table, int columnIndex)
        {
            int totalRows = table.GetUpperBound(0) + 1;
            double[] outColumn = new double[totalRows];
            Parallel.For(0, totalRows, rowIndex =>
            {
                outColumn[rowIndex] = table[rowIndex, columnIndex];
            });
            return outColumn;
        }

        public static double ComputeCorrelationCoefficient(double[] v1, double[] v2)
        {
            double avg1 = v1.Average();
            double avg2 = v2.Average();

            double sum1 = v1.Zip(v2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            double sumSqr1 = v1.Sum(x => Math.Pow((x - avg1), 2.0));
            double sumSqr2 = v2.Sum(y => Math.Pow((y - avg2), 2.0));

            double result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

            return result;
        }
    }
}
