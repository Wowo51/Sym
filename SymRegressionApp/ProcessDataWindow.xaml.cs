//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using SymRegressionApp.DataProcessing;
using System.Linq;
using System.Diagnostics;
using SymRegressionApp.FileHelper;

namespace SymRegressionApp
{
    /// <summary>
    /// Interaction logic for ProcessDataWindow.xaml
    /// </summary>
    public partial class ProcessDataWindow : Window
    {
        string RawText;
        int DependantColumnIndex;
        List<ColumnProcessDefinition> ColumnProcessDefinitions;
        ProcessDefinition ProcessDefinition;
        StringTable StringTable;
        FileIODataContract<ProcessDefinition> FileIOProcessDefinition = new FileIODataContract<ProcessDefinition>();        

        public ProcessDataWindow()
        {
            InitializeComponent();
            FileIOProcessDefinition.SetFilters("pd", "pd");
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = FileHelper.FileIO.BuildFilter("csv", "csv");
            if (openFileDialog.ShowDialog() == true)
            {
                RawText = File.ReadAllText(openFileDialog.FileName);
                string[,] rawTable = ProcessData.ParseCSV(RawText, ",");
                string[] headers = ProcessData.GetRow(rawTable, 0);
                InputBox inputBox = new InputBox();
                inputBox.Label1.Text = "Enter the header name you want to use as the dependant column.";
                inputBox.TextBox1.Text = headers.Last();
                inputBox.ShowDialog();
                string selectedHeaderName = inputBox.TextBox1.Text;
                //tabName = Interaction.InputBox("Enter a name for the new tab.", "Enter Name", "Tab " + Convert.ToString(tabControl.Items.Count + 1));
                if (selectedHeaderName == "" | selectedHeaderName == null)
                {
                    return;
                }
                if (headers.Contains(selectedHeaderName) == false)
                {
                    MessageBox.Show("Header name does not exist, aborted process.");
                    return;
                }
                DependantColumnIndex = headers.ToList().IndexOf(selectedHeaderName);
                ColumnProcessDefinitions = new List<ColumnProcessDefinition>();
                foreach (string header in headers)
                {
                    ColumnProcessDefinition definition = new ColumnProcessDefinition();
                    definition.Header = header;
                    ColumnProcessDefinitions.Add(definition);
                }
                StringTable = ProcessData.RawTableToStringTable(rawTable, DependantColumnIndex);
                gridMain.ItemsSource = null;
                gridMain.ItemsSource = ColumnProcessDefinitions;
            }
            MessageBox.Show("Done.");
        }

        private void btnFindCategories_Click(object sender, RoutedEventArgs e)
        {
            bool[] categoryColumns = ProcessData.FindCategories(StringTable, 10);
            for (int columnIndex = 0; columnIndex < categoryColumns.Length; columnIndex++)
            {
                ColumnProcessDefinitions[columnIndex].ConvertToIntegers = categoryColumns[columnIndex];
            }
            gridMain.ItemsSource = null;
            gridMain.ItemsSource = ColumnProcessDefinitions;
        }

        private void btnExpandCategories_Click(object sender, RoutedEventArgs e)
        {
            for (int columnIndex = 0; columnIndex < StringTable.IndependentHeaders.Length; columnIndex++)
            {
                ColumnProcessDefinitions[columnIndex].ExpandIntegers = ColumnProcessDefinitions[columnIndex].ConvertToIntegers;
            }
            gridMain.ItemsSource = null;
            gridMain.ItemsSource = ColumnProcessDefinitions;
        }

        private void btnProcessDataAndSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = FileHelper.FileIO.BuildFilter("csv", "csv");
            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }
            ProcessDefinition = ProcessDefinition.ToProcessDefinition(ColumnProcessDefinitions);
            NumericTable numericTable = ProcessDefinition.ToNumericTableUpdateProcessDefinition(RawText, ",", DependantColumnIndex);
            string csv = ProcessData.NumericTableToCSV(numericTable);
            File.WriteAllText(saveFileDialog.FileName, csv);
        }

        private void btnSaveProcessDefinition_Click(object sender, RoutedEventArgs e)
        {
            ProcessDefinition = ProcessDefinition.ToProcessDefinition(ColumnProcessDefinitions);
            NumericTable numericTable = ProcessDefinition.ToNumericTableUpdateProcessDefinition(RawText, ",", DependantColumnIndex);

            if (ProcessDefinition != null)
            {
                FileIOProcessDefinition.SaveAs(ref ProcessDefinition);
            }
        }
    }
}
