using System;
using System.Collections.Generic;
using System.Data;
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

namespace ChemEngThermCal.View {
    /// <summary>
    /// DatabaseMngView.xaml 的交互逻辑
    /// </summary>
    public partial class DbMngView : Page {
        public DbMngView() {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            if (App.isDbExist == true) {
                lstChem.DataContext = CetcDALDisLayer.GetAllChemical();
                lstChem.Visibility = Visibility.Visible;
                lblNoDb.Visibility = Visibility.Collapsed;
            } else {
                lstChem.Visibility = Visibility.Collapsed;
                lblNoDb.Visibility = Visibility.Visible;
            }
        }
    }
}
