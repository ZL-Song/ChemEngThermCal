using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ChemEngThermCal.View {
    /// <summary>
    /// Page1.xaml 的交互逻辑
    /// </summary>
    public partial class MfVleStartView : Page {
        public MfVleStartView() {
            InitializeComponent();
            lblReal.Opacity = 0;
            lblSemiIdeal.Opacity = 0;
            lblIdeal.Opacity = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (sender == btnReal) {
                NavigationService.Navigate(new MfVleView(1));
            } else if (sender == btnSemiIdeal) {
                NavigationService.Navigate(new MfVleView(2));
            } else if (sender == btnIdeal) {
                NavigationService.Navigate(new MfVleView(3));
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e) {
            Label lbl;
            if (sender == btnReal) {
                lbl = lblReal;
            } else if (sender == btnSemiIdeal) {
                lbl = lblSemiIdeal;
            } else if (sender == btnIdeal) {
                lbl = lblIdeal;
            } else {
                return;
            }
            DoubleAnimation da = ControlAnimations.FadeInAnimation; 
            Storyboard sb = new Storyboard();
            sb.Duration = ControlAnimations.GeneralAniDuation;
            sb.Children.Add(da); 
            Storyboard.SetTarget(da, lbl); 
            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity")); 
            sb.Begin();
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e) {
            Label lbl;
            if (sender == btnReal) {
                lbl = lblReal;
            } else if (sender == btnSemiIdeal) {
                lbl = lblSemiIdeal;
            } else if (sender == btnIdeal) {
                lbl = lblIdeal;
            } else {
                return;
            }
            DoubleAnimation da = ControlAnimations.FadeOutAnimation; 
            Storyboard sb = new Storyboard();
            sb.Duration = ControlAnimations.GeneralAniDuation;
            sb.Children.Add(da);  
            Storyboard.SetTarget(da, lbl); 
            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity")); 
            sb.Begin();
        }
    }
}
