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
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sym;
using Sym.Nodes;
using SymApp.FileHelper;

namespace SymApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileIODataContract<AppData> FileIO = new FileIODataContract<AppData>();
        public Solvers Solvers = new Solvers();
        SolverDialog SolverDialog;
        public bool IsSolving = false;
        public int MaxRepetitions = 1000;
        public int MaxPopulationSize = 250;
        public bool PostSolverLog = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            Init();
            Solvers.FinishedRepetition += Solvers_FinishedRepetition;
        }

        private void Solvers_FinishedRepetition(List<Solvers.PotentialAnswer> potentialAnswers)
        {
            Dispatcher.Invoke(() => SolverDialog.lbl1.Content = "Best so far:  " + potentialAnswers[0].EquationString);
            if (PostSolverLog)
            {
                string[] rows = potentialAnswers.Select(x => x.EquationString + "   " + x.Fitness.ToString()).ToArray();
                string block = string.Join(Environment.NewLine, rows);
                Dispatcher.Invoke(() => PutTextInTextBox(block));
            }
        }

        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete everything and start a new project?", "New?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                AppDataToInterface(new AppData());
                Init();
            }
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            AppData appData = new AppData();
            bool result = FileIO.Open(ref appData);
            if (result)
            {
                AppDataToInterface(appData);
            }
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            AppData appData = InterfaceToAppData();
            FileIO.Save(ref appData);
        }

        private void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            AppData appData = InterfaceToAppData();
            FileIO.SaveAs(ref appData);
        }

        private void FileQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void AppDataToInterface(AppData inAppData)
        {
            int L;
            this.tabControl1.Items.Clear();
            this.tabControlAlgebra.Items.Clear();
            this.tabControlCalculus.Items.Clear();
            this.tabControlVector.Items.Clear();
            this.tabControlMatrix.Items.Clear();
            this.tabControlTensor.Items.Clear();
            this.tabControlLogic.Items.Clear();
            for (L = 0; L < inAppData.mainFiles.Count; L++)
            {
                NewTab(false, "");
                SelectedTab.Header = inAppData.mainFiles[L].Name;
                SelectedTextBox.Text = inAppData.mainFiles[L].Text;
            }
            ScFilesToInterface(inAppData.algebraTransforms, this.tabControlAlgebra);
            ScFilesToInterface(inAppData.calculusTransforms, this.tabControlCalculus);
            ScFilesToInterface(inAppData.vectorTransforms, this.tabControlVector);
            ScFilesToInterface(inAppData.matrixTransforms, this.tabControlMatrix);
            ScFilesToInterface(inAppData.tensorTransforms, this.tabControlTensor);
            ScFilesToInterface(inAppData.logicTransforms, this.tabControlLogic);
            if (this.tabControl1.Items.Count > 0)
            {
                this.tabControl1.SelectedIndex = 0;
            }
            if (this.tabControlAlgebra.Items.Count > 0)
            {
                this.tabControlAlgebra.SelectedIndex = 0;
            }
        }

        public void ScFilesToInterface(List<ScFile> files, TabControl tabControl)
        {
            int L;
            TabItem tabItem;
            TextBox textBox;
            for (L = 0; L < files.Count; L++)
            {
                NewTransformTab(false, "", tabControl);
                tabItem = (TabItem)tabControl.Items[L];
                tabItem.Header = files[L].Name;
                textBox = (TextBox)tabItem.Content;
                textBox.Text = files[L].Text;
            }
        }

        public AppData InterfaceToAppData()
        {
            AppData lOut = new AppData();
            lOut.mainFiles = InterfaceToScFiles(this.tabControl1);
            lOut.algebraTransforms = InterfaceToScFiles(this.tabControlAlgebra);
            lOut.calculusTransforms = InterfaceToScFiles(this.tabControlCalculus);
            lOut.vectorTransforms = InterfaceToScFiles(this.tabControlVector);
            lOut.matrixTransforms = InterfaceToScFiles(this.tabControlMatrix);
            lOut.tensorTransforms = InterfaceToScFiles(this.tabControlTensor);
            lOut.logicTransforms = InterfaceToScFiles(this.tabControlLogic);
            return lOut;
        }

        public List<ScFile> InterfaceToScFiles(TabControl tabControl)
        {
            int L;
            TextBox lBox;
            TabItem lTab;
            List<ScFile> files = new List<ScFile>();
            for (L = 0; L < tabControl.Items.Count; L++)
            {
                lTab = (TabItem)tabControl.Items[L];
                lBox = (TextBox)lTab.Content;
                files.Add(new ScFile());
                files[L].Text = lBox.Text;
                files[L].Name = (string)lTab.Header;
            }
            return files;
        }

        public void Init()
        {
            NewTab(false, "Tab 1");
            InitializeTransformTabs();
            FileIO.SetFilters("Sym Files", "sca");
        }

        public void NewTab(bool nameTab, string tabName)
        {
            NewTab(nameTab, this.tabControl1, tabName, true);
        }

        public void NewTransformTab(bool nameTab, string tabName, TabControl tabControl)
        {
            NewTab(nameTab, tabControl, tabName, false);
            TextBox lBox = SelectedTabsTextBox(tabControl);
            lBox.MouseDoubleClick += new MouseButtonEventHandler(lBox_MouseDoubleClick);
        }

        void lBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = SelectedTransformTabTextBox;
            string transform = GetLine(textBox);
            this.txtActiveTransform.Text = transform;
        }

        public string GetMainLine()
        {
            return GetLine(SelectedTextBox);
        }

        public string GetLine(TextBox inTextBox)
        {
            string text = inTextBox.Text;
            int cursorPosition = inTextBox.SelectionStart;
            int end = SearchForEndForwards(text, Environment.NewLine, cursorPosition);
            int begin = SearchForEndBackwards(text, Environment.NewLine, cursorPosition);
            string selectedLineText = text.Substring(begin, end - begin);
            return selectedLineText;
        }

        public static int SearchForEndBackwards(string SearchMe, string inOperator, int startLocation)
        {
            int L;
            string subString;
            for (L = startLocation - 1; L >= 0; L--)
            {
                if (L + inOperator.Length > SearchMe.Length)
                {
                    //fail!
                }
                else
                {
                    subString = SearchMe.Substring(L, inOperator.Length);
                    if (inOperator == subString)
                    {
                        return L + inOperator.Length;
                    }
                }
            }
            return 0;
        }

        public static int SearchForEndForwards(string SearchMe, string inOperator, int startLocation)
        {
            int L;
            string subString;
            for (L = startLocation; L < SearchMe.Length; L++)
            {
                if (L + inOperator.Length > SearchMe.Length)
                {
                    //fail!
                }
                else
                {
                    subString = SearchMe.Substring(L, inOperator.Length);
                    if (inOperator == subString)
                    {
                        return L;
                    }
                }
            }
            return SearchMe.Length;
        }

        public void NewTab(bool nameTab, TabControl tabControl, string tabName, bool wrapText)
        {
            if (nameTab == true)
            {
                InputBox inputBox = new InputBox();
                inputBox.Label1.Content = "Enter a name for the new tab.";
                inputBox.ShowDialog();
                tabName = inputBox.TextBox1.Text;
                //tabName = Interaction.InputBox("Enter a name for the new tab.", "Enter Name", "Tab " + Convert.ToString(tabControl.Items.Count + 1));
                if (tabName == "" | tabName == null)
                {
                    return;
                }
            }
            else if (tabName != "" & tabName != null)
            {
                //tabName = tabName;
            }
            else
            {
                tabName = "Tab " + Convert.ToString(tabControl.SelectedIndex + 1);
            }
            TabItem lTab = new TabItem();
            TextBox lBox = new TextBox();
            lBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            lBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            if (wrapText)
            {
                lBox.TextWrapping = TextWrapping.Wrap;
            }
            lBox.FontSize = 22;
            lBox.AcceptsReturn = true;
            lTab.Content = (object)lBox;
            tabControl.Items.Add(lTab);
            tabControl.SelectedIndex = tabControl.Items.Count - 1;
            lTab.Header = tabName;
        }

        public TabItem SelectedTransformTab
        {
            get
            {
                TabItem t = (TabItem)this.tabControlTransforms.SelectedItem;
                TabControl tc = (TabControl)t.Content;
                return (TabItem)tc.SelectedItem;
            }
        }

        public TabItem SelectedTab
        {
            get
            {
                return (TabItem)this.tabControl1.SelectedItem;
            }
        }

        public static TabItem SelectedTabItem(TabControl tabControl)
        {
            return (TabItem)tabControl.SelectedItem;
        }

        public static TextBox SelectedTabsTextBox(TabControl tabControl)
        {
            return (TextBox)SelectedTabItem(tabControl).Content;
        }

        public TextBox SelectedTransformTabTextBox
        {
            get
            {
                return (TextBox)SelectedTransformTab.Content;
            }
        }

        public TextBox TransformTabTextBox(int tabIndex, TabControl tabControl)
        {
            TabItem tab = (TabItem)tabControl.Items[tabIndex];
            return (TextBox)tab.Content;
        }

        public string SelectedTabString
        {
            get
            {
                return SelectedTextBox.Text;
            }
            set
            {
                SelectedTextBox.Text = value;
            }
        }

        public TextBox SelectedTextBox
        {
            get
            {
                return (TextBox)SelectedTab.Content;
            }
        }

        public void InitializeTransformTabs()
        {
            int L;
            Dictionary<string, string> transforms = TransformData.AllAlgebraAsNamedSets();
            for (L = 0; L < transforms.Count; L++)
            {
                NewTransformTab(false, transforms.ElementAt(L).Key, tabControlAlgebra);
                TransformTabTextBox(L, tabControlAlgebra).Text = transforms.ElementAt(L).Value;
            }
            transforms = TransformData.AllCalculusAsNamedSets();
            for (L = 0; L < transforms.Count; L++)
            {
                NewTransformTab(false, transforms.ElementAt(L).Key, tabControlCalculus);
                TransformTabTextBox(L, tabControlCalculus).Text = transforms.ElementAt(L).Value;
            }
            transforms = TransformData.AllVectorAsNamedSets();
            for (L = 0; L < transforms.Count; L++)
            {
                NewTransformTab(false, transforms.ElementAt(L).Key, tabControlVector);
                TransformTabTextBox(L, tabControlVector).Text = transforms.ElementAt(L).Value;
            }

            transforms = TransformData.AllLogicAsNamedSets();
            for (L = 0; L < transforms.Count; L++)
            {
                NewTransformTab(false, transforms.ElementAt(L).Key, tabControlLogic);
                TransformTabTextBox(L, tabControlLogic).Text = transforms.ElementAt(L).Value;
            }

            this.tabControlAlgebra.SelectedIndex = 0;
            this.tabControlCalculus.SelectedIndex = 0;
            this.tabControlVector.SelectedIndex = 0;

            this.tabControlLogic.SelectedIndex = 0;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to save the open file before closing?", "Save?", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                AppData appData = InterfaceToAppData();
                FileIO.SaveAs(ref appData);
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            App.Current.Shutdown();
        }

        private void mnuTabDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the current tab?", "Delete?", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                if (tabControl1.SelectedIndex > -1)
                {
                    tabControl1.Items.RemoveAt(tabControl1.SelectedIndex);
                }
            }
        }

        private void mnuTabNew_Click(object sender, RoutedEventArgs e)
        {
            NewTab(true, this.tabControl1, "", true);
        }

        private void mnuTransformTabNew_Click(object sender, RoutedEventArgs e)
        {
            TabItem t = (TabItem)this.tabControlTransforms.SelectedItem;
            TabControl tc = (TabControl)t.Content;
            NewTransformTab(true, "", tc);
        }

        private void mnuTransformTabDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the current transform tab?", "Delete?", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                TabItem t = (TabItem)this.tabControlTransforms.SelectedItem;
                TabControl tc = (TabControl)t.Content;
                if (tc.SelectedIndex > -1)
                {
                    tc.Items.RemoveAt(tc.SelectedIndex);
                }
            }
        }

        private void mnuRenameTab_Click(object sender, RoutedEventArgs e)
        {
            InputBox inputBox = new InputBox();
            inputBox.Label1.Content = "Enter the new name.";
            inputBox.ShowDialog();
            string newName = inputBox.TextBox1.Text;
            //string newName = Interaction.InputBox("Enter the new name", "Enter Name", "");
            if (newName == "" | newName == null)
            {
                return;
            }
            SelectedTab.Header = newName;
        }

        private void mnuRenameTransformTab_Click(object sender, RoutedEventArgs e)
        {
            InputBox inputBox = new InputBox();
            inputBox.Label1.Content = "Enter the new name.";
            inputBox.ShowDialog();
            string newName = inputBox.TextBox1.Text;
            //string newName = Interaction.InputBox("Enter the new name", "Enter Name", "");
            if (newName == "" | newName == null)
            {
                return;
            }
            SelectedTransformTab.Header = newName;
        }

        private void mnuSelectAll_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedTextBox.SelectAll();
        }

        private void HelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Copyright Warren Harding 2020.");
        }

        private void HelpHelp_Click(object sender, RoutedEventArgs e)
        {
            string path = Environment.CurrentDirectory + "\\symdocs.htm";
            //Process.Start(path);
        }

        private void SymHome_Click(object sender, RoutedEventArgs e)
        {
            //this will have to be swapped out when symboliccomputation.com is uploaded
            //Process.Start("http:\\\\symboliccomputation.com");
        }

        private void mnuTest_Click(object sender, RoutedEventArgs e)
        {
            TestSym testSym = new TestSym();
            testSym.Show();
        }

        public string GetSelectedText()
        {
            TextBox textBox = SelectedTextBox;
            string text = textBox.SelectedText;
            return text;
        }

        public void PutTextInTextBox(string addMe)
        {
            string cleaned = addMe.Replace("--", "+").Replace("+-", "-");
            SelectedTabString = SelectedTabString + Environment.NewLine + Environment.NewLine + cleaned;
            SelectedTextBox.Select(SelectedTextBox.Text.Length, 0);
            SelectedTextBox.SelectionStart = SelectedTextBox.Text.Length;
            SelectedTextBox.CaretIndex = SelectedTextBox.Text.Length;
            SelectedTextBox.Focus();
        }

        private void btnTransform_Click(object sender, RoutedEventArgs e)
        {
            string transformMeString = this.GetMainLine();
            if (transformMeString == "" | transformMeString == null)
            {
                return;
            }
            string transformString = this.txtActiveTransform.Text;
            if (transformString == "" | transformString == null)
            {
                return;
            }
            Transform transform = null;
            Node transformMe = null;
            List<Operator> operators = Operator.BuildOperators();
            try
            {
                transformMe = Node.Parse(transformMeString, operators);
                transform = Transform.StringToTransform(transformString, operators);
            }
            catch
            {
                MessageBox.Show("Parse fail. Did you an expression and transform in valid C# syntax?");
                return;
            }
            Node transformedNode = Transform.TransformNode(transformMe, transform, operators);
            if (transformedNode != null)
            {
                string transformedResult = Node.Join(transformedNode);
                if (transformedResult != "" & transformedResult != null)
                {
                    PutTextInTextBox(transformedResult);
                }
            }
        }

        private void btnEval_Click(object sender, RoutedEventArgs e)
        {
            string selectedText = this.GetMainLine();
            if (selectedText == "" | selectedText == null)
            {
                return;
            }
            double result;
            List<Operator> operators = Operator.BuildOperators();
            try
            {
                Node node = Node.Parse(selectedText, operators);
                //List<Node> branches = node.DescendantsAndSelf();
                result = Evaluation.Evaluate(node);
            }
            catch
            {
                MessageBox.Show("Couldn't evaluate, did you enter an expression with all numeric values and no variables?");
                return;
            }
            if (double.IsNaN(result))
            {
                MessageBox.Show("Couldn't evaluate, did you enter an expression with all numeric values and no variables?");
            }
            else
            {
                PutTextInTextBox(result.ToString());
            }
        }

        private async void btnIsolate_Click(object sender, RoutedEventArgs e)
        {
            if (IsSolving)
            {
                MessageBox.Show("The solver is busy.");
                return;
            }
            IsSolving = true;
            InputBox inputBox = new InputBox();
            inputBox.Label1.Content = "Enter the variable you want to isolate.";
            inputBox.ShowDialog();
            string isolateMe = inputBox.TextBox1.Text;
            if (isolateMe == null | isolateMe == "")
            {
                IsSolving = false;
                return;
            }
            //string equation = this.GetLine();
            string equation = this.GetMainLine();
            if (equation == "" | equation == null)
            {
                IsSolving = false;
                return;
            }
            if (equation.Contains("=") == false)
            {
                MessageBox.Show("You have to enter an equation.");
                IsSolving = false;
                return;
            }
            SolverDialog = new SolverDialog();
            SolverDialog.ParentWindow = this;
            SolverDialog.Show();
            List<Operator> operators = Operator.BuildOperators();
            string transformedResult = await Task.Run(() => Solvers.Isolate(equation, isolateMe, MaxRepetitions, MaxPopulationSize, operators));
            SolverDialog.Close();
            if (transformedResult != "" & transformedResult != null)
            {
                PutTextInTextBox(transformedResult);
            }
            IsSolving = false;
        }

        private void btnCopyLine_Click(object sender, RoutedEventArgs e)
        {
            string result = this.GetMainLine();
            PutTextInTextBox(result);
        }

        private async void btnDerivative_Click(object sender, RoutedEventArgs e)
        {
            if (IsSolving)
            {
                MessageBox.Show("The solver is busy.");
                return;
            }
            IsSolving = true;
            string transformMe = this.GetMainLine();
            if (transformMe == "" | transformMe == null)
            {
                IsSolving = false;
                return;
            }
            SolverDialog = new SolverDialog();
            SolverDialog.ParentWindow = this;
            SolverDialog.Show();
            List<Operator> operators = Operator.BuildOperators();
            string transformedResult = await Task.Run(() => Solvers.Derivative(transformMe, MaxRepetitions, MaxPopulationSize, operators));
            SolverDialog.Close();
            if (transformedResult != "" & transformedResult != null)
            {
                PutTextInTextBox(transformedResult);
            }
            IsSolving = false;
        }

        private async void btnPartialDerivative_Click(object sender, RoutedEventArgs e)
        {
            if (IsSolving)
            {
                MessageBox.Show("The solver is busy.");
                return;
            }
            IsSolving = true;
            string transformMe = this.GetMainLine();
            if (transformMe == "" | transformMe == null)
            {
                IsSolving = false;
                return;
            }
            SolverDialog = new SolverDialog();
            SolverDialog.ParentWindow = this;
            SolverDialog.Show();
            List<Operator> operators = Operator.BuildOperators();
            string transformedResult = await Task.Run(() => Solvers.PartialDerivative(transformMe, MaxRepetitions, MaxPopulationSize, operators));
            SolverDialog.Close();
            SolverDialog = null;
            if (transformedResult != "" & transformedResult != null)
            {
                PutTextInTextBox(transformedResult);
            }
            IsSolving = false;
        }

        private async void btnSimplify_Click(object sender, RoutedEventArgs e)
        {
            if (IsSolving)
            {
                MessageBox.Show("The solver is busy.");
                return;
            }
            IsSolving = true;
            string transformMe = this.GetMainLine();
            if (transformMe == "" | transformMe == null)
            {
                IsSolving = false;
                return;
            }
            SolverDialog = new SolverDialog();
            SolverDialog.ParentWindow = this;
            SolverDialog.Show();
            List<Operator> operators = Operator.BuildOperators();
            string transformedResult = await Task.Run(() => Solvers.Simplify(transformMe, MaxRepetitions, MaxPopulationSize, operators));
            SolverDialog.Close();
            if (transformedResult != "" & transformedResult != null)
            {
                PutTextInTextBox(transformedResult);
            }
            IsSolving = false;
        }

        private void btnSubstitute_Click(object sender, RoutedEventArgs e)
        {
            string transformMeString = this.GetMainLine();
            if (transformMeString == "" | transformMeString == null)
            {
                return;
            }
            InputBox inputBox = new InputBox();
            inputBox.Label1.Content = "Enter the transformation equation you want to use.";
            inputBox.ShowDialog();
            string transformString = inputBox.TextBox1.Text;
            if (transformString == null | transformString == "")
            {
                MessageBox.Show("Invalid transform.");
                return;
            }
            Node transformMe = null;
            Transform transform = null;
            List<Operator> operators = Operator.BuildOperators();
            try
            {
                transformMe = Node.Parse(transformMeString, operators);
                transform = Transform.StringToTransform(transformString, operators);
            }
            catch
            {
                MessageBox.Show("Parse fail. Did you enter an expression and a transform in valid C#?");
                return;
            }
            bool didTransform = false;
            Node transformedNode = TransformBranchFunctions.TransformBranchesWithTransformToOneResult(transformMe, transform, operators, ref didTransform);
            string transformedResult = Node.Join(transformedNode);
            if (transformedResult != "" && transformedResult != null && didTransform)
            {
                PutTextInTextBox(transformedResult);
            }
        }

        private void btnTransformBranches_Click(object sender, RoutedEventArgs e)
        {
            string transformMeString = this.GetMainLine();
            if (transformMeString == "" | transformMeString == null)
            {
                return;
            }
            string transformString = this.txtActiveTransform.Text;
            if (transformString == "" | transformString == null)
            {
                return;
            }
            Node transformMe = null;
            Transform transform = null;
            List<Operator> operators = Operator.BuildOperators();
            try
            {
                transformMe = Node.Parse(transformMeString, operators);
                transform = Transform.StringToTransform(transformString, operators);
            }
            catch
            {
                MessageBox.Show("Parse fail. Did you enter an expression and a transform in valid C#?");
                return;
            }
            bool didTransform = false;
            Node transformedNode = TransformBranchFunctions.TransformBranchesWithTransformToOneResult(transformMe, transform, operators, ref didTransform);
            string transformedResult = Node.Join(transformedNode);
            if (transformedResult != "" & transformedResult != null && didTransform)
            {
                PutTextInTextBox(transformedResult);
            }
        }

        private void btnStopSolving_Click(object sender, RoutedEventArgs e)
        {
            Solvers.StopSolving = true;
        }

        private void mnuSolverOptions_Click(object sender, RoutedEventArgs e)
        {
            SolverOptions solverOptions = new SolverOptions();
            solverOptions.txtMaxRepetitions.Text = Convert.ToString(MaxRepetitions);
            solverOptions.txtMaxPopulationSize.Text = Convert.ToString(MaxPopulationSize);
            solverOptions.chkParallel.IsChecked = Solvers.UseParallel;
            solverOptions.chkLog.IsChecked = PostSolverLog;
            solverOptions.ParentWindow = this;
            solverOptions.ShowDialog();
        }

        private async void btnSolveSystem_Click(object sender, RoutedEventArgs e)
        {
            if (IsSolving)
            {
                MessageBox.Show("The solver is busy.");
                return;
            }
            IsSolving = true;
            InputBox inputBox = new InputBox();
            inputBox.Label1.Content = "Enter the variable you want to isolate.";
            inputBox.ShowDialog();
            string isolateMe = inputBox.TextBox1.Text;
            if (isolateMe == null | isolateMe == "")
            {
                IsSolving = false;
                return;
            }
            string transformMe = this.GetSelectedText();
            if (transformMe == "" | transformMe == null)
            {
                IsSolving = false;
                return;
            }
            SolverDialog = new SolverDialog();
            SolverDialog.ParentWindow = this;
            SolverDialog.Show();
            List<Operator> operators = Operator.BuildOperators();
            string transformedResult = await Task.Run(() => Solvers.SolveSystem(transformMe, isolateMe, MaxRepetitions, MaxPopulationSize, operators));
            SolverDialog.Close();
            if (transformedResult != "" & transformedResult != null)
            {
                PutTextInTextBox(transformedResult);
            }
            IsSolving = false;
        }
    }
}
