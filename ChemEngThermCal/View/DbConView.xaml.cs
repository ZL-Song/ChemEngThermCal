using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChemEngThermCal.View {
    /// <summary>
    /// DbConView.xaml 的交互逻辑
    /// </summary>
    public partial class DbConView : Window {
        public DbConView() {
            InitializeComponent();
            IsChemGot = false;
            lstChem.DataContext = CetcDALDisLayer.GetAllChemical();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (lstChem.SelectedItem == null) {
                MessageBox.Show("blank selection !", "Error Input", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            } else {
                DataRowView drv = (lstChem.SelectedItem as DataRowView);
                Chem = new ChemicalInfo(Convert.ToDouble(drv["Tc"]), Convert.ToDouble(drv["Pc"]), Convert.ToDouble(drv["w"]), Convert.ToDouble(drv["Zc"]), Convert.ToDouble(drv["Vc"]));
                IsChemGot = true;
                DialogResult = true;
            }
        }

        public ChemicalInfo Chem { get; private set; }

        public bool IsChemGot { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            IsChemGot = false;
            DialogResult = false;
        }
    }
}
