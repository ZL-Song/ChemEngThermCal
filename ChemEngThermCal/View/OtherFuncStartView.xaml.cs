using System;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChemEngThermCal.View {
    /// <summary>
    /// OtherFuncStartView.xaml 的交互逻辑
    /// </summary>
    public partial class OtherFuncStartView : Page {
        public OtherFuncStartView() {
            InitializeComponent();
            lblAntoine.Opacity = 0;
            lblGceCalP.Opacity = 0;
            lblRackett.Opacity = 0;
        }
        /// <summary>
        /// 不同的附属功能 RadioButtons 被选定时，进行不同位置的导航
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e) {
            if (sender == btnAntoine) {
                NavigationService.Navigate(new PfAntoineView());
            } else if (sender == btnRackett) {
                NavigationService.Navigate(new PfRackettView());
            } else if (sender == btnGceCalP) {
                NavigationService.Navigate(new PfGceCalPView());
            }
        }
        private void Button_MouseEnter(object sender, MouseEventArgs e) {
            Label lbl;
            if (sender == btnAntoine) {
                lbl = lblAntoine;
            } else if (sender == btnRackett) {
                lbl = lblRackett;
            } else if (sender == btnGceCalP) {
                lbl = lblGceCalP;
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
            if (sender == btnAntoine) {
                lbl = lblAntoine;
            } else if (sender == btnRackett) {
                lbl = lblRackett;
            } else if (sender == btnGceCalP) {
                lbl = lblGceCalP;
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
