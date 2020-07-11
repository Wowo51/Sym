//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using SymRegressionApp.DataProcessing;
using SymAI.Regression;
using Sym.Nodes;
using Sym;
using ILGPU;
using ILGPU.Runtime;

namespace SymRegressionApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public NumericTable NumericTable;
        public SymRegression SymRegression;
        public int MaxNodesPerExpression = 15;
        public AcceleratorType AcceleratorType = AcceleratorType.OpenCL;
        public DateTime LastTimeOfTextRefresh = DateTime.Now;
        public MainWindow()
        {
            InitializeComponent();
            //SymRegression.FinishedGeneration += SymRegression_FinishedGeneration;
            //SymRegression.FinishedRegressing += SymRegression_FinishedRegressing;
        }

        private void SymRegression_FinishedRegressing()
        {
            MessageBox.Show("Finished Regressing.");
        }

        private void SymRegression_FinishedGeneration()
        {
            if (DateTime.Now > LastTimeOfTextRefresh + new TimeSpan(0, 0, 2))
            {
                List<string> rows = new List<string>();
                rows.Add("Total Individuals Tested=" + SymRegression.IndividualsTestedCount.ToString());
                rows.Add("LengthAdjustedFitness,Fitness,Model");
                int modelCount = SymRegression.ModelManager.SurvivingPopulationSize;
                for(int modelIndex = 0; modelIndex < modelCount; modelIndex++)
                {
                    Model model = SymRegression.ModelManager.Models[modelIndex];
                    Node root = model.Expression.Clone();
                    List<double> numbers = model.Optimizer.Independents.ToList();
                    List<Node> branches = root.DescendantsAndSelf();
                    List<Pointer> constantPointers = Node.BuildPointers("Constant", branches);
                    Node.SetNumbers(constantPointers, numbers, branches);
                    string expression = Node.Join(root, "Constant");
                    string row = model.LengthAdjustedFitness.ToString() + "   " + model.Fitness.ToString() + "   " + expression;
                    rows.Add(row);
                }
                string log = string.Join(Environment.NewLine, rows.ToArray());
                Dispatcher.Invoke(() =>
                {
                    txtMain.Text = log;
                });
                LastTimeOfTextRefresh = DateTime.Now;
            }
        }

        private void mnuProcessData_Click(object sender, RoutedEventArgs e)
        {
            ProcessDataWindow window = new ProcessDataWindow();
            window.Show();
        }

        private void mnuImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string rawText = File.ReadAllText(openFileDialog.FileName);
                string[,] rawTable = ProcessData.ParseCSV(rawText, ",");
                StringTable stringTable = ProcessData.RawTableToStringTable(rawTable, rawTable.GetUpperBound(1));
                NumericTable = ProcessData.ToNumericTable(stringTable);
            }
            MessageBox.Show("Done.");
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (SymRegression != null && SymRegression.IsRunning)
            {
                MessageBox.Show("You are already running a regression, aborted.");
                return;
            }
            if (NumericTable == null || NumericTable.Independents == null || NumericTable.Dependants == null)
            {
                MessageBox.Show("You don't have any valid data loaded, try the functions in the File menu.");
                return;
            }
            if (SymRegression != null)
            {
                SymRegression.FinishedGeneration -= SymRegression_FinishedGeneration;
                SymRegression.FinishedRegressing -= SymRegression_FinishedRegressing;
            }
            SymRegression = new SymRegression();
            SymRegression.FinishedGeneration += SymRegression_FinishedGeneration;
            SymRegression.FinishedRegressing += SymRegression_FinishedRegressing;
            Task.Run(() => SymRegression.Run(NumericTable.Independents, NumericTable.Dependants, MaxNodesPerExpression, AcceleratorType, new List<string>(), SymRegression.ModelManager.GenerationTransforms()));
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            SymRegression.ModelManager.StopRegressing = true;
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtMain.Text);
        }

        private void mnuOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionsWindow = new OptionsWindow();
            optionsWindow.ParentMainWindow = this;
            optionsWindow.txtMaxNodes.Text = MaxNodesPerExpression.ToString();
            optionsWindow.cmbAcceleratorType.SelectedIndex = (int)AcceleratorType;
            optionsWindow.ShowDialog();
        }

        private void mnuForecasting_Click(object sender, RoutedEventArgs e)
        {
            ForecastingWindow forecastingWindow = new ForecastingWindow();
            forecastingWindow.ParentMainWindow = this;
            forecastingWindow.Show();
        }
    }
}
