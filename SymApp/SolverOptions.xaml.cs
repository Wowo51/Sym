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

namespace SymApp
{
    /// <summary>
    /// Interaction logic for SolverOptions.xaml
    /// </summary>
    public partial class SolverOptions : Window
    {
        public MainWindow ParentWindow;

        public SolverOptions()
        {
            InitializeComponent();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentWindow.MaxRepetitions = Convert.ToInt32(txtMaxRepetitions.Text);
                ParentWindow.MaxPopulationSize = Convert.ToInt32(txtMaxPopulationSize.Text);
                ParentWindow.Solvers.UseParallel = (bool)chkParallel.IsChecked;
                ParentWindow.PostSolverLog = (bool)chkLog.IsChecked;
            }
            catch
            {
                MessageBox.Show("Operation failed. You need to enter integer numbers in the text boxes.");
            }
            Close();
        }
    }
}
