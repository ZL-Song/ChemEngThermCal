using System;
using System.Windows;

namespace AntnCnvt {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            txb_result.Text = "";
            double a, b, c; // unconverted
            double cA, cB, cC; //converted
            if (double.TryParse(txtA.Text, out a) && double.TryParse(txtB.Text, out b) && double.TryParse(txtC.Text, out c)) {
                //valid 
                cA = (a-3.0) * Math.Log(10);
                cB = b * Math.Log(10);
                cC = c - 273.15;

                txb_result.Text = string.Format("Converting Done.\nA = {0}\nB = {1}\nC = {2}", cA, cB, cC);
                
            } else {
                //invalid
                MessageBox.Show("Please enter a valid number.");

                return;
            }
        }
    }
}
