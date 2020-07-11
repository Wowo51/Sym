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
using ILGPU.Runtime;

namespace SymRegressionApp
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public MainWindow ParentMainWindow;
        public OptionsWindow()
        {
            InitializeComponent();
            cmbAcceleratorType.Items.Add("CPU");
            cmbAcceleratorType.Items.Add("Cuda");
            cmbAcceleratorType.Items.Add("OpenCL");
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            int maxNodes = 0;
            try
            {
                maxNodes = Convert.ToInt32(txtMaxNodes.Text);
            }
            catch
            {
                MessageBox.Show("Enter a valid number in the maximum nodes box, aborted.");
                return;
            }
            if (maxNodes < 0)
            {
                MessageBox.Show("Enter a valid number in the maximum nodes box, aborted.");
                return;
            }
            ParentMainWindow.MaxNodesPerExpression = maxNodes;
            ParentMainWindow.AcceleratorType = (AcceleratorType)cmbAcceleratorType.SelectedIndex;
        }
    }
}
