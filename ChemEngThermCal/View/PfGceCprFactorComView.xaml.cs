using ChemEngThermCal.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ChemEngThermCal.View {
    /// <summary>
    /// PfGceCprFactorComView.xaml 的交互逻辑
    /// </summary>
    public partial class PfGceCprDiagView : Window {
        public PfGceCprDiagView(double relativeTemperature, double relativePressure) {
            InitializeComponent();
            ntbTr.Text = relativeTemperature.ToString();
            ntbPr.Text = relativePressure.ToString();
        }
        //Z0
        public double Base
            => System.Convert.ToDouble(ntbZ0.Text);
        //Z1
        public double Crec
            => System.Convert.ToDouble(ntbZ1.Text);
        //输入数字的文本框
        private List<NumberTextBox> ntbList
            => new List<NumberTextBox> { ntbZ0, ntbZ1 };
        //输入检查
        private bool IsInputLegal() {
            NumberTextBox ntb = DataChecker.CheckInputNumeric(ntbList);
            if (ntb != null) {
                ntb.Focus();
                ntb.Text += " <-- Error";
                return false;
            } else {
                return true;
            }
        }
        //继续
        private void btnCtn_Click(object sender, RoutedEventArgs e) {
            if (IsInputLegal()) {
                DialogResult = true;
            }
        }
        //放弃
        private void btnAbrt_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
