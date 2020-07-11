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
using SymRegressionApp.FileHelper;
using System.Linq;
using Sym;
using Sym.Nodes;
using SymAI.Regression;

namespace SymRegressionApp
{
    /// <summary>
    /// Interaction logic for ForecastingWindow.xaml
    /// </summary>
    public partial class ForecastingWindow : Window
    {
        string RawData;
        ProcessDefinition ProcessDefinition = new ProcessDefinition();
        FileIODataContract<ProcessDefinition> FileIOProcessDefinition = new FileIODataContract<ProcessDefinition>();
        public MainWindow ParentMainWindow;

        public ForecastingWindow()
        {
            InitializeComponent();
        }

        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = FileHelper.FileIO.BuildFilter("csv", "csv");
            if (openFileDialog.ShowDialog() == true)
            {
                RawData = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void btnLoadProcessDefinition_Click(object sender, RoutedEventArgs e)
        {
            FileIOProcessDefinition.Open(ref ProcessDefinition);
        }

        private void btnForecast_Click(object sender, RoutedEventArgs e)
        {
            InputBox inputBox = new InputBox();
            inputBox.Label1.Text = "Enter the header name you want to use as the id column.";
            inputBox.ShowDialog();
            string selectedHeaderName = inputBox.TextBox1.Text;
            //tabName = Interaction.InputBox("Enter a name for the new tab.", "Enter Name", "Tab " + Convert.ToString(tabControl.Items.Count + 1));
            if (selectedHeaderName == "" | selectedHeaderName == null)
            {
                return;
            }
            NumericTable numericTable = ProcessDefinition.ToNumericTableUseProcessDefinition(RawData, ",", -1);
            StringTable stringTable = ProcessData.RawTableToStringTable(ProcessData.ParseCSV(RawData, ","), -1);
            int idHeaderIndex = numericTable.IndependentHeaders.ToList().IndexOf(selectedHeaderName);
            if (idHeaderIndex == -1)
            {
                MessageBox.Show("Id header not found, aborted.");
                return;
            }
            int totalRows = numericTable.Independents.GetUpperBound(0) + 1;
            int totalColumns = numericTable.Independents.GetUpperBound(1) + 1;
            List<string> rows = new List<string>();
            rows.Add(selectedHeaderName + "," + "Forecast");
            List<Operator> operators = Operator.BuildOperators();
            Node modelExpression = Node.Parse(txtModelExpression.Text, operators);
            List<Node> branches = modelExpression.DescendantsAndSelf();
            List<Pointer> pointers = Node.BuildPointers("Independent", branches);
            for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
            {
                string id = stringTable.Independents[rowIndex, idHeaderIndex];
                List<double> numbers = new List<double>();
                for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
                {
                    double number = numericTable.Independents[rowIndex, columnIndex];
                    numbers.Add(number);
                }
                //Node.SetNumbers(pointers, numbers, branches);
                //double forecast = Evaluation.Evaluate(modelExpression) * ProcessDefinition.Scale + ProcessDefinition.Offset;
                double forecast = Forecasting.ForecastFast(numbers, branches, pointers);
                forecast = Forecasting.ScaleAndOffsetForecast(forecast, ProcessDefinition.Scale, ProcessDefinition.Offset);
                string row = id + "," + forecast.ToString();
                rows.Add(row);
            }
            txtForecasts.Text = string.Join(Environment.NewLine, rows.ToArray());
            //txtForecasts.Text = ProcessData.NumericTableToCSV(numericTable);
        }

        private void btnSaveForecasts_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = FileHelper.FileIO.BuildFilter("csv", "csv");
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, txtForecasts.Text);
            }
        }

        private void btnGetBestModel_Click(object sender, RoutedEventArgs e)
        {
            if (ParentMainWindow.SymRegression.ModelManager.Models != null && ParentMainWindow.SymRegression.ModelManager.Models.Count > 0)
            {
                Model model = ParentMainWindow.SymRegression.ModelManager.Models[0];
                Node root = model.Expression;
                List<Node> branches = root.DescendantsAndSelf();
                List<Pointer> pointers = Node.BuildPointers("Constant", branches);
                List<double> numbers = model.Optimizer.Independents.ToList();
                Node.SetNumbers(pointers, numbers, branches);
                txtModelExpression.Text = Node.Join(root, "Constant");
            }
        }
    }
}
