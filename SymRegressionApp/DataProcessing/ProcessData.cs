//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace SymRegressionApp.DataProcessing
{
    public class ProcessData
    {
        public static string[,] ParseCSV(string csv, string columnDelimiter)
        {
            string[] rows = csv.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Length == 1)
            {
                rows = csv.Split(new string[1] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (rows.Length == 1)
            {
                rows = csv.Split(new string[1] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            }
            int totalColumns = rows[0].Split(new string[1] { columnDelimiter }, StringSplitOptions.None).Length;
            string[,] outTable = new string[rows.Length, totalColumns];
            Parallel.For(0, rows.Length, rowIndex =>
            {
                string[] cells = rows[rowIndex].Split(new string[1] { columnDelimiter }, StringSplitOptions.None);
                for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                {
                    outTable[rowIndex, cellIndex] = cells[cellIndex];
                }
            });
            return outTable;
        }

        public static StringTable RawTableToStringTable(string[,] inTable, int dependantColumnIndex)
        {
            StringTable outTable = new StringTable();
            int totalRows = inTable.GetUpperBound(0) + 1;
            int totalColumns = inTable.GetUpperBound(1) + 1;
            
            if (dependantColumnIndex >= 0)
            {
                outTable.IndependentHeaders = new string[totalColumns - 1];
                outTable.DependantHeader = inTable[0, dependantColumnIndex];
                outTable.Independents = new string[totalRows - 1, totalColumns - 1];
            }
            else
            {
                outTable.IndependentHeaders = new string[totalColumns];
                outTable.Independents = new string[totalRows - 1, totalColumns];
            }
            int columnIndex = 0;
            for (int rawColumnIndex = 0; rawColumnIndex < totalColumns; rawColumnIndex++)
            {
                if (rawColumnIndex != dependantColumnIndex)
                {
                    outTable.IndependentHeaders[columnIndex] = inTable[0, rawColumnIndex];
                    columnIndex++;
                }
            }
            
            outTable.Dependants = new string[totalRows - 1];
            Parallel.For(1, totalRows, rawRowIndex =>
            {
                int rowIndex = rawRowIndex - 1;
                int columnIndex = 0;
                for (int rawColumnIndex = 0; rawColumnIndex < totalColumns; rawColumnIndex++)
                {
                    if (rawColumnIndex != dependantColumnIndex)
                    {
                        outTable.Independents[rowIndex, columnIndex] = inTable[rawRowIndex, rawColumnIndex];
                        columnIndex++;
                    }
                    else
                    {
                        outTable.Dependants[rowIndex] = inTable[rawRowIndex, rawColumnIndex];
                    }
                }
            });
            return outTable;
        }

        public static bool[] FindCategories(StringTable table, int maxCategories)
        {
            int totalColumns = table.IndependentHeaders.Length;
            bool[] categoryColumns = new bool[totalColumns];
            for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
            {
                string[] column = GetColumn(table.Independents, columnIndex);
                int distinctEntries = column.Distinct().ToList().Count;
                if (distinctEntries <= maxCategories)
                {
                    categoryColumns[columnIndex] = true;
                }
            }
            return categoryColumns;
        }

        public static void ConvertCategoriesToIntegers(StringTable table, string[] columnNames)
        {
            bool[] columnsToConvert = FindColumns(table.IndependentHeaders.ToList(), columnNames);
            int totalRows = table.Dependants.Length;
            for (int columnIndex = 0; columnIndex < columnsToConvert.Length; columnIndex++)
            {
                if (columnsToConvert[columnIndex] == true)
                {
                    string[] columnStrings = GetColumn(table.Independents, columnIndex);
                    List<string> distinctEntries = columnStrings.Distinct().ToList();
                    distinctEntries.Sort();
                    Parallel.For(0, totalRows, rowIndex =>
                    {
                        table.Independents[rowIndex, columnIndex] = Convert.ToString(distinctEntries.IndexOf(table.Independents[rowIndex, columnIndex]));
                    });
                }
            }
        }

        public static bool[] FindColumns(List<string> allHeaders, string[] namesToFind)
        {
            bool[] outBools = new bool[allHeaders.Count];
            foreach (string nameToFind in namesToFind)
            {
                if (allHeaders.Contains(nameToFind))
                {
                    int i = allHeaders.IndexOf(nameToFind);
                    outBools[i] = true;
                }
            }
            return outBools;
        }

        public static StringTable ExpandIntegerColumnsUpdateProcessDefinition(StringTable inStringTable, string[] columnNames, ProcessDefinition processDefinition)
        {
            processDefinition.ExpandColumnsPointers = new ExpandColumnsPointers();
            bool[] columnsToExpand = FindColumns(inStringTable.IndependentHeaders.ToList(), columnNames);
            int countAddedColumns = 0;
            for (int inColumnIndex = 0; inColumnIndex < columnsToExpand.Length; inColumnIndex++)
            {
                if (columnsToExpand[inColumnIndex] == true)
                {
                    string[] column = GetColumn(inStringTable.Independents, inColumnIndex);
                    countAddedColumns += column.Distinct().ToList().Count - 1;
                }
            }
            StringTable outStringTable = new StringTable();
            outStringTable.DependantHeader = inStringTable.DependantHeader;
            outStringTable.Dependants = inStringTable.Dependants;
            int totalRows = inStringTable.Dependants.Length;
            int totalInColumns = inStringTable.Independents.GetUpperBound(1) + 1;
            int totalOutColumns = totalInColumns + countAddedColumns;
            outStringTable.IndependentHeaders = new string[totalOutColumns];
            outStringTable.Independents = new string[totalRows, totalOutColumns];
            int outColumnIndex = 0;
            for (int inColumnIndex = 0; inColumnIndex < columnsToExpand.Length; inColumnIndex++)
            {
                ExpandColumnPointers expandColumnPointers = new ExpandColumnPointers();
                string[] inColumn = GetColumn(inStringTable.Independents, inColumnIndex);
                if (columnsToExpand[inColumnIndex] == false)
                {
                    outStringTable.IndependentHeaders[outColumnIndex] = inStringTable.IndependentHeaders[inColumnIndex];
                    PutColumn(outStringTable.Independents, inColumn, outColumnIndex);
                    outColumnIndex++;
                }
                else
                {
                    List<string> distinctCategories = inColumn.Distinct().ToList();
                    int totalNewColumns = distinctCategories.Count;
                    for (int newColumnIndex = 0; newColumnIndex < totalNewColumns; newColumnIndex++)
                    {
                        ExpandColumnPointer expandColumnPointer = new ExpandColumnPointer();
                        expandColumnPointer.NewColumnIndex = newColumnIndex;
                        expandColumnPointer.CategoryName = distinctCategories[newColumnIndex];
                        outStringTable.IndependentHeaders[outColumnIndex] = inStringTable.IndependentHeaders[inColumnIndex] + "-" + newColumnIndex.ToString().Trim();
                        for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
                        {
                            if (newColumnIndex == Convert.ToInt32(inColumn[rowIndex]))
                            {
                                outStringTable.Independents[rowIndex, outColumnIndex] = "1";
                            }
                            else
                            {
                                outStringTable.Independents[rowIndex, outColumnIndex] = "0";
                            }
                        }
                        outColumnIndex++;
                        expandColumnPointers.Pointers.Add(expandColumnPointer);
                    }
                }
                if (expandColumnPointers.Pointers.Count > 0)
                {
                    processDefinition.ExpandColumnsPointers.Pointers.Add(expandColumnPointers);
                }
            }
            return outStringTable;
        }

        public static StringTable ExpandIntegerColumnsUseProcessDefinition(StringTable inStringTable, string[] columnNames, ProcessDefinition processDefinition)
        {
            bool[] columnsToExpand = FindColumns(inStringTable.IndependentHeaders.ToList(), columnNames);
            int countAddedColumns = 0;
            for (int inColumnIndex = 0; inColumnIndex < processDefinition.ExpandColumnsPointers.Pointers.Count; inColumnIndex++)
            {
                ExpandColumnPointers expandColumnPointers = processDefinition.ExpandColumnsPointers.Pointers[inColumnIndex];
                if (expandColumnPointers.Pointers.Count > 0)
                {
                    countAddedColumns += expandColumnPointers.Pointers.Count - 1;
                }
            }
            StringTable outStringTable = new StringTable();
            int totalRows = inStringTable.Independents.GetUpperBound(0) + 1;
            int totalInColumns = inStringTable.Independents.GetUpperBound(1) + 1;
            int totalOutColumns = totalInColumns + countAddedColumns;
            outStringTable.IndependentHeaders = new string[totalOutColumns];
            outStringTable.Independents = new string[totalRows, totalOutColumns];
            int outColumnIndex = 0;
            int countExpandedInColumns = 0;
            for (int inColumnIndex = 0; inColumnIndex < columnsToExpand.Length; inColumnIndex++)
            {
                string[] inColumn = GetColumn(inStringTable.Independents, inColumnIndex);
                if (columnsToExpand[inColumnIndex] == false)
                {
                    outStringTable.IndependentHeaders[outColumnIndex] = inStringTable.IndependentHeaders[inColumnIndex];
                    PutColumn(outStringTable.Independents, inColumn, outColumnIndex);
                    outColumnIndex++;
                }
                else
                {
                    ExpandColumnPointers expandColumnPointers = processDefinition.ExpandColumnsPointers.Pointers[countExpandedInColumns];
                    int totalNewColumns = expandColumnPointers.Pointers.Count;
                    for (int newColumnIndex = 0; newColumnIndex < totalNewColumns; newColumnIndex++)
                    {
                        ExpandColumnPointer expandColumnPointer = expandColumnPointers.Pointers[newColumnIndex];
                        outStringTable.IndependentHeaders[outColumnIndex] = inStringTable.IndependentHeaders[inColumnIndex] + "-" + newColumnIndex.ToString().Trim();
                        for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
                        {
                            if (inStringTable.Independents[rowIndex, inColumnIndex] == expandColumnPointer.CategoryName & newColumnIndex == Convert.ToInt32(expandColumnPointer.NewColumnIndex))
                            {
                                outStringTable.Independents[rowIndex, outColumnIndex] = "1";
                            }
                            else
                            {
                                outStringTable.Independents[rowIndex, outColumnIndex] = "0";
                            }
                        }
                        outColumnIndex++;
                    }
                    countExpandedInColumns++;
                }
            }
            return outStringTable;
        }

        public static string[] GetColumn(string[,] table, int columnIndex)
        {
            int totalRows = table.GetUpperBound(0) + 1;
            string[] outColumn = new string[totalRows];
            Parallel.For(0, totalRows, rowIndex =>
            {
                outColumn[rowIndex] = table[rowIndex, columnIndex];
            });
            return outColumn;
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

        public static void PutColumn(double[,] table, double[] column, int columnIndex)
        {
            int totalRows = table.GetUpperBound(0) + 1;
            Parallel.For(0, totalRows, rowIndex =>
            {
                table[rowIndex, columnIndex] = column[rowIndex];
            });
        }

        public static void PutColumn(string[,] table, string[] column, int columnIndex)
        {
            int totalRows = table.GetUpperBound(0) + 1;
            Parallel.For(0, totalRows, rowIndex =>
            {
                table[rowIndex, columnIndex] = column[rowIndex];
            });
        }

        public static string[] GetRow(string[,] table, int rowIndex)
        {
            int totalColumns = table.GetUpperBound(1) + 1;
            string[] outRow = new string[totalColumns];
            Parallel.For(0, totalColumns, columnIndex =>
            {
                outRow[columnIndex] = table[rowIndex, columnIndex];
            });
            return outRow;
        }

        public static double[] GetRow(double[,] table, int rowIndex)
        {
            int totalColumns = table.GetUpperBound(1) + 1;
            double[] outRow = new double[totalColumns];
            Parallel.For(0, totalColumns, columnIndex =>
            {
                outRow[columnIndex] = table[rowIndex, columnIndex];
            });
            return outRow;
        }


        public static StringTable RemoveUnwantedColumns(StringTable inTable, string[] removeColumnNames)
        {
            bool[] removeColumns = FindColumns(inTable.IndependentHeaders.ToList(), removeColumnNames);
            bool[] keepColumns = new bool[removeColumns.Length];
            for (int columnIndex = 0; columnIndex < removeColumns.Length; columnIndex++)
            {
                keepColumns[columnIndex] = !removeColumns[columnIndex];
            }
            int columnsToKeepCount = keepColumns.Count(x => x);
            int totalRows = inTable.Independents.GetUpperBound(0) + 1;
            StringTable outTable = new StringTable();
            outTable.Dependants = inTable.Dependants;
            outTable.DependantHeader = inTable.DependantHeader;
            outTable.IndependentHeaders = new string[columnsToKeepCount];
            outTable.Independents = new string[totalRows, columnsToKeepCount];
            int outColumnIndex = 0;
            for (int inColumnIndex = 0; inColumnIndex < keepColumns.Length; inColumnIndex++)
            {
                if (keepColumns[inColumnIndex])
                {
                    outTable.IndependentHeaders[outColumnIndex] = inTable.IndependentHeaders[inColumnIndex];
                    Parallel.For(0, totalRows, rowIndex =>
                    {
                        outTable.Independents[rowIndex, outColumnIndex] = inTable.Independents[rowIndex, inColumnIndex];
                    });
                    outColumnIndex++;
                }
            }
            return outTable;
        }

        public static void Normalize(NumericTable numericTable, ProcessDefinition processDefinition)
        {
            int totalRows = numericTable.Independents.GetUpperBound(0) + 1;
            int totalColumns = numericTable.Independents.GetUpperBound(1) + 1;
            //Parallel.For(0, totalColumns, columnIndex =>
            //{
            //    double[] column = GetColumn(numericTable.Independents, columnIndex);
            //    double min = column.Min();
            //    double max = column.Max();
            //    double range = max - min;
            //    for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
            //    {
            //        if (range != 0)
            //        {
            //            column[rowIndex] = (column[rowIndex] - min) / range;
            //        }
            //        else
            //        {
            //            column[rowIndex] = 0;
            //        }
            //    }
            //    PutColumn(numericTable.Independents, column, columnIndex);
            //});
            for(int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
            {
                double[] column = GetColumn(numericTable.Independents, columnIndex);
                double min = column.Min();
                double max = column.Max();
                double range = max - min;
                for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
                {
                    if (range != 0)
                    {
                        column[rowIndex] = (column[rowIndex] - min) / range;
                    }
                    else
                    {
                        column[rowIndex] = 0;
                    }
                }
                PutColumn(numericTable.Independents, column, columnIndex);
            }
            if (processDefinition != null)
            {
                double min = numericTable.Dependants.Min();
                double max = numericTable.Dependants.Max();
                double range = max - min;
                for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
                {
                    if (range != 0)
                    {
                        numericTable.Dependants[rowIndex] = (numericTable.Dependants[rowIndex] - min) / range;
                    }
                    else
                    {
                        numericTable.Dependants[rowIndex] = 0;
                    }
                }
                processDefinition.Scale = range;
                processDefinition.Offset = min;
            }
        }

        public static NumericTable ToNumericTable(StringTable stringTable)
        {
            NumericTable numericTable = new NumericTable();
            numericTable.IndependentHeaders = stringTable.IndependentHeaders;
            numericTable.DependantHeader = stringTable.DependantHeader;
            int totalRows = stringTable.Independents.GetUpperBound(0) + 1;
            int totalColumns = stringTable.Independents.GetUpperBound(1) + 1;
            numericTable.Independents = new double[totalRows, totalColumns];
            Parallel.For(0, totalRows, rowIndex =>
            {
                for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
                {
                    double result = 0;
                    bool succeeded = double.TryParse(stringTable.Independents[rowIndex, columnIndex], out result);
                    if (succeeded & double.IsNaN(result) == false)
                    {
                        numericTable.Independents[rowIndex, columnIndex] = result;
                    }
                    else
                    {
                        numericTable.Independents[rowIndex, columnIndex] = 0;
                    }
                }
            });
            numericTable.Dependants = new double[totalRows];
            if (stringTable.Dependants != null)
            {
                for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
                {
                    double result = 0;
                    bool succeeded = double.TryParse(stringTable.Dependants[rowIndex], out result);
                    if (succeeded & double.IsNaN(result) == false)
                    {
                        numericTable.Dependants[rowIndex] = result;
                    }
                    else
                    {
                        numericTable.Dependants[rowIndex] = 0;
                    }
                }
            }
            return numericTable;
        }

        public static string NumericTableToCSV(NumericTable numericTable)
        {
            int totalIndependentsRows = numericTable.Dependants.Length;
            int totalIndependentsColumns = numericTable.IndependentHeaders.Length;

            string[] rows = new string[totalIndependentsRows + 1];
            rows[0] = string.Join(",", numericTable.IndependentHeaders) + "," + numericTable.DependantHeader;
            Parallel.For(0, totalIndependentsRows, independentRowIndex =>
            {
                double[] rowDoubles = GetRow(numericTable.Independents, independentRowIndex);
                List<string> rowStrings = new List<string>();
                foreach (double rowDouble in rowDoubles)
                {
                    string rowString = rowDouble.ToString();
                    if (rowString == "NaN")
                    {
                        rowString = "0";
                    }
                    rowStrings.Add(rowString);
                }
                rowStrings.Add(numericTable.Dependants[independentRowIndex].ToString());
                string row = string.Join(",", rowStrings);
                rows[independentRowIndex + 1] = row;
            });
            return string.Join(Environment.NewLine, rows);
        }
    }
}
