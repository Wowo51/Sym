//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace SymRegressionApp.DataProcessing
{
    public class ProcessDefinition
    {
        public List<string> IgnoreColumnHeaders;
        public List<string> ConvertToIntegersColumnHeaders;
        public List<string> ExpandColumnHeaders;
        public ExpandColumnsPointers ExpandColumnsPointers;
        public double Scale = 1;
        public double Offset = 0;


        public static ProcessDefinition ToProcessDefinition(List<ColumnProcessDefinition> columnProcessDefinitions)
        {
            ProcessDefinition processDefinition = new ProcessDefinition();
            processDefinition.IgnoreColumnHeaders = columnProcessDefinitions.Where(x => x.Ignore).Select(x => x.Header).ToList();
            processDefinition.ConvertToIntegersColumnHeaders = columnProcessDefinitions.Where(x => x.ConvertToIntegers).Select(x => x.Header).ToList();
            processDefinition.ExpandColumnHeaders = columnProcessDefinitions.Where(x => x.ExpandIntegers).Select(x => x.Header).ToList();
            return processDefinition;
        }

        public NumericTable ToNumericTableUpdateProcessDefinition(string table, string columnDelimiter, int dependantColumnIndex)
        {
            string[,] rawTable = ProcessData.ParseCSV(table, columnDelimiter);
            StringTable stringTable = ProcessData.RawTableToStringTable(rawTable, dependantColumnIndex);
            stringTable = ProcessData.RemoveUnwantedColumns(stringTable, IgnoreColumnHeaders.ToArray());
            ProcessData.ConvertCategoriesToIntegers(stringTable, ConvertToIntegersColumnHeaders.ToArray());
            stringTable = ProcessData.ExpandIntegerColumnsUpdateProcessDefinition(stringTable, ExpandColumnHeaders.ToArray(), this);
            NumericTable numericTable = ProcessData.ToNumericTable(stringTable);
            stringTable = null;
            ProcessData.Normalize(numericTable, this);
            return numericTable;
        }

        public NumericTable ToNumericTableUseProcessDefinition(string table, string columnDelimiter, int dependantColumnIndex)
        {
            string[,] rawTable = ProcessData.ParseCSV(table, columnDelimiter);
            StringTable stringTable = ProcessData.RawTableToStringTable(rawTable, dependantColumnIndex);
            stringTable = ProcessData.RemoveUnwantedColumns(stringTable, IgnoreColumnHeaders.ToArray());
            ProcessData.ConvertCategoriesToIntegers(stringTable, ConvertToIntegersColumnHeaders.ToArray());
            stringTable = ProcessData.ExpandIntegerColumnsUseProcessDefinition(stringTable, ExpandColumnHeaders.ToArray(), this);
            NumericTable numericTable = ProcessData.ToNumericTable(stringTable);
            stringTable = null;
            ProcessData.Normalize(numericTable, null);
            return numericTable;
        }
    }
}
