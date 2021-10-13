using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ChemEngThermCal.View {
    /// <summary>
    /// ConfigurationView.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigView : Page {
        public ConfigView() {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            if (hcbLang.SelectedIndex == 0) {
                Properties.Settings.Default.UICulture = "en-US";
            } else if (hcbLang.SelectedIndex == 1) {
                Properties.Settings.Default.UICulture = "zh-CN";
            }
            if (MessageBox.Show("Will restart ChemEngThermCal to update new settings.\nProceed?", "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes) {
                Properties.Settings.Default.Save();
                AppAssist.RefreshSettings();
                Application.Current.Shutdown();
                System.Reflection.Assembly.GetEntryAssembly();
                string startpath = System.IO.Directory.GetCurrentDirectory();
                Process.Start(startpath + "/ChemEngThermCal.exe");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            string uiculture = Properties.Settings.Default.UICulture;
            if (uiculture == "en-US") {
                hcbLang.SelectedIndex = 0;
            } else if (uiculture == "zh-CN") {
                hcbLang.SelectedIndex = 1;
            } else {
                hcbLang.SelectedIndex = 0;
            }
        }
    }
}
