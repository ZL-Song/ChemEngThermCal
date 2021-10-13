using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChemEngThermCal.View {
    public static class ShowMsg {
        public static void RadioSelectionError(Page page) {
            Window main = Window.GetWindow(page);
            (main as MainWindowView).ShowError(1);
        }
        public static void ComboSelectionError(Page page) {
            Window main = Window.GetWindow(page);
            (main as MainWindowView).ShowError(2);
        }
        public static void TextInputError(Page page) {
            Window main = Window.GetWindow(page);
            (main as MainWindowView).ShowError(3);
        }
    }
}
