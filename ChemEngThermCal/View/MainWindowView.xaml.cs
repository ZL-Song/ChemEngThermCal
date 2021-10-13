using System;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;

namespace ChemEngThermCal.View {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowView : Window {
        public MainWindowView() {
            AppAssist.RefreshSettings();
            InitializeComponent();
            grdMain.Visibility = Visibility.Hidden;
            lblError1.Height = 0;
            lblError2.Height = 0;
            lblError3.Height = 0;
            lblError4.Height = 0;

        }
        #region   frmContent 的渐变切换效果 
        /// <summary>
        /// 当 frmContent 导航开始时，先取消事件，开始 frmContent 右移淡出
        /// </summary>
        private void frmContent_Navigating(object sender, NavigatingCancelEventArgs e) {
            frmContent.IsHitTestVisible = false;                           //关闭控件交互
            frmContent.Visibility = Visibility.Hidden;
            /* 新建线程开始左滑淡入的切换效果 */
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                frmContent.Visibility = Visibility.Visible;
                ThicknessAnimation ta = ControlAnimations.FrameMoveInAnimation;
                DoubleAnimation oa = ControlAnimations.FadeInAnimation;
                Storyboard sb = new Storyboard();
                sb.Duration = ControlAnimations.GeneralAniDuation;
                sb.Children.Add(oa);
                sb.Children.Add(ta);
                Storyboard.SetTarget(oa, frmContent);
                Storyboard.SetTarget(ta, frmContent);
                Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
                Storyboard.SetTargetProperty(ta, new PropertyPath("Margin"));
                oa.Completed += oaIn_Completed;
                sb.Begin();
            });
            /*  frmContent 的渐出效果 End */
        }
        /// <summary>
        /// 完成淡入后，开启控件交互
        /// </summary>
        void oaIn_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= oaIn_Completed;
            frmContent.IsHitTestVisible = true;                                   //开启控件交互
        }
        #endregion   用于 frmContent 的渐变切换效果 End  ========================
        /// <summary>
        /// 不同的主 RadioButton 被选定时，进行不同位置的导航
        /// </summary>
        private void MainWindowRadioButtonChecked(object sender, RoutedEventArgs e) {
            if (sender == vhrPfCubicEoS) {
                frmContent.Navigate(new PfCubicEosView());
            } else if (sender == vhrPfGce) {
                frmContent.Navigate(new PfGceCalvView());
            } else if (sender == vhrMfGasMix) {
                frmContent.Navigate(new MfGasMixView());
            } else if (sender == vhrPfVle) {
                frmContent.Navigate(new PfVleView());
            } else if (sender == vhrMfVle) {
                frmContent.Navigate(new MfVleStartView());
            } else if (sender == vhrOtherFunc) {
                frmContent.Navigate(new OtherFuncStartView());
            } else if (sender == vhrDataBaseMng) {
                frmContent.Navigate(new DbMngView());
            } else if (sender == vhrConfig) {
                frmContent.Navigate(new ConfigView());
            } else if (sender == vhrAbout) {
                frmContent.Navigate(new AboutView());
            }
        }
        /// <summary>
        /// 窗体启动动画
        /// </summary> 
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (!System.IO.File.Exists(System.IO.Directory.GetCurrentDirectory() + @"\cetcdb.mdb")) {
                lblError1_db.Content += "Not Connected";
            } else {
                lblError1_db.Content += "Connected";
                App.isDbExist = true;
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                Thread.Sleep(500);
                grdMain.Visibility = Visibility.Visible;
                ShowError(0);
                ThicknessAnimation ta = ControlAnimations.FrameMoveInAnimation;
                DoubleAnimation oa = ControlAnimations.FadeInAnimation;
                Storyboard sb = new Storyboard();
                sb.Duration = ControlAnimations.GeneralAniDuation;
                sb.Children.Add(oa);
                sb.Children.Add(ta);
                Storyboard.SetTarget(oa, grdMain);
                Storyboard.SetTarget(ta, grdMain);
                Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
                Storyboard.SetTargetProperty(ta, new PropertyPath("Margin"));
                oa.Completed += loadingAnimation_Completed;
                sb.Begin();
            });
        }
        /// <summary>
        /// 窗体启动后进行内容初始化
        /// </summary> 
        private void loadingAnimation_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= loadingAnimation_Completed;
            frmContent.Navigate(new StartView());
        }
        private int lastError;
        public void ShowError(int i) {
            Label lblError;
            lastError = i;
            if (i == 0) {
                lblError = lblError1;
            } else if (i == 1) {
                lblError = lblError2;
            } else if (i == 2) {
                lblError = lblError3;
            } else if (i == 3) {
                lblError = lblError4;
            } else {
                return;
            }
            if (lblError.Height == 0) {
                DoubleAnimation ta = ControlAnimations.TextBoxHeightUpAnimation;
                DoubleAnimation oa = ControlAnimations.FadeInAnimation;
                Storyboard sb = new Storyboard();
                sb.Duration = ControlAnimations.GeneralAniDuation;
                sb.Children.Add(oa);
                sb.Children.Add(ta);
                Storyboard.SetTarget(oa, lblError);
                Storyboard.SetTarget(ta, lblError);
                Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
                Storyboard.SetTargetProperty(ta, new PropertyPath("Height"));
                sb.Begin();
                InfoTimer timer = new InfoTimer(3000, lblError);
                timer.Elapsed += errorHide;
                timer.Start();
            }
        }
        private void errorHide(object sender, EventArgs e) {
            (sender as InfoTimer).Elapsed -= errorHide;
            (sender as InfoTimer).Stop();
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                DoubleAnimation ta = ControlAnimations.TextBoxHeightDownAnimation;
                DoubleAnimation oa = ControlAnimations.FadeOutAnimation;
                Storyboard sb = new Storyboard();
                sb.Duration = ControlAnimations.GeneralAniDuation;
                sb.Children.Add(oa);
                sb.Children.Add(ta);
                Storyboard.SetTarget(oa, (sender as InfoTimer).RelativeLabel);
                Storyboard.SetTarget(ta, (sender as InfoTimer).RelativeLabel);
                Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
                Storyboard.SetTargetProperty(ta, new PropertyPath("Height"));
                sb.Begin();
            });
        }
    }
    class InfoTimer : System.Timers.Timer {
        public InfoTimer(double interval, Label lbl) :
            base(interval) {
            RelativeLabel = lbl;
        }
        internal readonly Label RelativeLabel;
    }
}