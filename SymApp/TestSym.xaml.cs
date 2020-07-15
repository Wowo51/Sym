using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using System.Threading.Tasks;
using Sym;
using Sym.Nodes;

namespace SymApp
{
    /// <summary>
    /// Interaction logic for TestSym.xaml
    /// </summary>
    public partial class TestSym : Window
    {
        bool StopTesting = false;
        bool IsTesting = false;

        public TestSym()
        {
            InitializeComponent();
            this.Closing += TestSym_Closing;
        }

        private void TestSym_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopTesting = true;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Test());
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopTesting = true;
        }

        public void Test()
        {
            if (IsTesting)
            {
                MessageBox.Show("Is already testing.");
                return;
            }
            List<Operator> operators = Operator.BuildOperators();
            IsTesting = true;
            Dispatcher.Invoke(() => txt1.Text = "Started testing...");
            string[] variableSet = new string[3] { "x", "y", "z" };
            List<Transform> generationTransforms = GenerationTransforms().Select(x => Transform.StringToTransform(x, operators)).ToList();
            List<Transform> transformsToTest = TransformData.AllAlgebraTransformsAsTransforms(operators);
            //transformsToTest.Add(Transform.StringToTransform("x~x+1d"));
            List<TestEquation> testEquations = new List<TestEquation>();
            TestEquation seedX = new TestEquation();
            seedX.LHSExpression = "x";
            testEquations.Add(seedX);
            TestEquation seedY = new TestEquation();
            seedY.LHSExpression = "y";
            testEquations.Add(seedY);
            TestEquation seedZ = new TestEquation();
            seedZ.LHSExpression = "z";
            testEquations.Add(seedZ);
            Random random = new Random(1);
            List<ErrorReport> errorReports = new List<ErrorReport>();
            int countEquationsTested = 0;
            //EvaluateBranches evaluate = new EvaluateBranches();
            StopTesting = false;
            int countEquationsToReport = 0;
            while (StopTesting == false)
            {
                List<TestEquation> unTransformedTestEquations = testEquations.Where(x => x.Transformed == false).ToList();
                foreach (TestEquation testEquation in unTransformedTestEquations)
                {
                    Node node = Node.Parse(testEquation.LHSExpression, operators);
                    List<Node> newExpressions = TransformBranchFunctions.TransformBranchesWithTransforms(node, generationTransforms, operators);
                    testEquation.Transformed = true;
                    foreach (Node newExpression in newExpressions)
                    {
                        List<string> existingExpressions = testEquations.Select(x => x.LHSExpression).ToList();
                        string newExpressionString = Node.Join(newExpression);
                        if (existingExpressions.Contains(newExpressionString) == false)
                        {
                            TestEquation newEquation = new TestEquation();
                            newEquation.LHSExpression = newExpressionString;
                            string[] variableValueTransformStrings = variableSet.Select(x => "V" + x + "~" + Convert.ToString(random.NextDouble() * 2d - 1d).Trim()).ToArray();
                            newEquation.VariableValueTransforms = variableValueTransformStrings.Select(x => Transform.StringToTransform(x, operators)).ToList();
                            Node workNode1 = Node.Parse(newEquation.LHSExpression, operators);
                            string temp1 = Node.Join(workNode1);
                            workNode1 = TransformBranchFunctions.TransformBranchesWithTransformsToOneResult(workNode1, newEquation.VariableValueTransforms, operators);
                            string temp2 = Node.Join(workNode1);
                            newEquation.Value = Evaluation.Evaluate(workNode1);
                            if (double.IsInfinity(newEquation.Value) == false & double.IsNaN(newEquation.Value) == false)
                            {
                                newEquation.FullEquation = newEquation.LHSExpression + "=" + newEquation.Value.ToString();

                                Node fullEquationNode = Node.Parse(newEquation.FullEquation, operators);
                                string lStr3 = Node.Join(fullEquationNode);
                                int countTestTransform = 0;
                                foreach (Transform transformToTest in transformsToTest)
                                {
                                    countTestTransform++;

                                    List<Node> testInstances = TransformBranchFunctions.TransformBranchesWithTransform(fullEquationNode, transformToTest, operators);
                                    int countTestInstances = 0;
                                    foreach (Node testInstance in testInstances)
                                    {
                                        countEquationsTested++;
                                        countTestInstances++;
                                        string lStr = Node.Join(testInstance);
                                        Node workNode2 = testInstance.Clone();
                                        string lStr2 = Node.Join(workNode2);
                                        workNode2 = TransformBranchFunctions.TransformBranchesWithTransformsToOneResult(workNode2, newEquation.VariableValueTransforms, operators);
                                        if (workNode2 != null)
                                        {
                                            string[] sides = Node.Join(workNode2).Split(new string[1] { "=" }, StringSplitOptions.None);
                                            Node leftNode = Node.Parse(sides[0], operators);
                                            Node rightNode = Node.Parse(sides[1], operators);
                                            double lhsValue = Evaluation.Evaluate(leftNode);
                                            double rhsValue = Evaluation.Evaluate(rightNode);
                                            double lhsLeftOfDecimal = lhsValue.ToString().Split(".")[0].Length;
                                            double rhsLeftOfDecimal = rhsValue.ToString().Split(".")[0].Length;
                                            double adjustedLhsValue = lhsValue / Math.Pow(10 , lhsLeftOfDecimal);
                                            double adjustedRhsValue = rhsValue / Math.Pow(10, rhsLeftOfDecimal);
                                            if (Math.Abs(adjustedLhsValue - adjustedRhsValue) > .0001d)
                                            {
                                                ErrorReport errorReport = new ErrorReport();
                                                errorReport.InitialExpression = newEquation.FullEquation;
                                                errorReport.FinalExpression = Node.Join(testInstance);
                                                errorReport.TransformUsed = Transform.TransformToString(transformToTest);
                                                errorReports.Add(errorReport);
                                            }
                                            countEquationsToReport++;
                                            if (countEquationsToReport > 99)
                                            {
                                                countEquationsToReport = 0;
                                                string reportString = countEquationsTested.ToString() + " equations tested." + Environment.NewLine;
                                                if (errorReports.Count == 0)
                                                {
                                                    string[] instanceStrings = testInstances.Select(x => Node.Join(x)).ToArray();
                                                    reportString = reportString + "No errors found." + Environment.NewLine + string.Join(Environment.NewLine, instanceStrings);
                                                }
                                                else
                                                {
                                                    string[] errorStrings = errorReports.Select(x => x.InitialExpression + "," + x.FinalExpression + "," + x.TransformUsed).ToArray();
                                                    reportString = reportString + errorReports.Count.ToString() + " errors found." + Environment.NewLine + string.Join(Environment.NewLine, errorStrings);
                                                }
                                                Dispatcher.Invoke(() => txt1.Text = reportString);
                                            }
                                        }
                                        if (StopTesting)
                                        {
                                            IsTesting = false;
                                            return;
                                        }
                                    }
                                }
                            }
                            testEquations.Add(newEquation);
                        }
                    }
                }
            }
            IsTesting = false;
        }

        public class ErrorReport
        {
            public string InitialExpression;
            public string FinalExpression;
            public string TransformUsed;
        }

        public class TestEquation
        {
            public string FullEquation;
            public string LHSExpression;
            public bool Transformed = false;
            public List<Transform> VariableValueTransforms;
            public double Value;
        }

        public static List<string> GenerationTransforms()
        {
            List<string> lOut = new List<string>();
            lOut.Add("a~Sin(x)");
            lOut.Add("a~Cos(x)");
            lOut.Add("a~Tan(x)");
            lOut.Add("a~Sin(y)");
            lOut.Add("a~Cos(y)");
            lOut.Add("a~Tan(y)");
            lOut.Add("a~Sin(z)");
            lOut.Add("a~Cos(z)");
            lOut.Add("a~Tan(z)");
            lOut.Add("a~(x)");
            lOut.Add("a~(y)");
            lOut.Add("a~(z)");
            lOut.Add("a~Pow(x,y)");
            lOut.Add("a~Pow(x,z)");
            lOut.Add("a~Pow(y,z)");
            lOut.Add("a~x+y");
            lOut.Add("a~x-y");
            lOut.Add("a~x*y");
            lOut.Add("a~x/y");
            lOut.Add("a~x+z");
            lOut.Add("a~x-z");
            lOut.Add("a~x*z");
            lOut.Add("a~x/z");
            lOut.Add("a~y+z");
            lOut.Add("a~y-z");
            lOut.Add("a~y*z");
            lOut.Add("a~y/z");
            lOut.Add("a~x");
            lOut.Add("a~y");
            lOut.Add("a~z");
            return lOut;
        }
    }
}
