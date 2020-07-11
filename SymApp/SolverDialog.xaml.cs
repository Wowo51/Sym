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
    /// Interaction logic for SolverDialog.xaml
    /// </summary>
    public partial class SolverDialog : Window
    {
        public MainWindow ParentWindow;

        public SolverDialog()
        {
            InitializeComponent();
            Closing += SolverDialog_Closing;
        }

        private void SolverDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ParentWindow.Solvers.StopSolving = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.Solvers.StopSolving = true;
            Close();
        }
    }
}
