using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Animation;
using ChemEngThermCal.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace ChemEngThermCal.View {
    /// <summary>
    /// RackettView.xaml 的交互逻辑
    /// </summary>
    public partial class PfRackettView : Page {
        public PfRackettView() {
            InitializeComponent();
            grdResult.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// ntb List
        /// </summary>
        private List<NumberTextBox> ntbList
        {
            get
            {
                if (rbtAlphaBetaExp.IsChecked == true) {
                    return new List<NumberTextBox> { ntbCriticalTemperature, ntbCriticalPressure, ntbAlpha, ntbBeta, ntbActualTemperature };
                } else if (rbtOmigaExp.IsChecked == true) {
                    return new List<NumberTextBox> { ntbCriticalTemperature, ntbCriticalPressure, ntbAcentricFactor, ntbActualTemperature };
                } else {
                    return new List<NumberTextBox> { ntbCriticalTemperature, ntbCriticalPressure, ntbActualTemperature };
                }
            }
        }
        /// <summary>
        /// rbt List
        /// </summary>
        private List<RadioButton> rbtList
        {
            get
            {
                return new List<RadioButton> { rbtAlphaBetaExp, rbtOmigaExp };
            }
        }
        /// <summary>
        /// 输入合法性检查
        /// </summary>
        /// <returns>true 表示输入合法，false 表示输入非法</returns>
        private bool IsInputLegal() {
            bool rbtCheck = DataChecker.CheckRadioSelection(rbtList);
            if (!rbtCheck) {
                ShowMsg.RadioSelectionError(this);
                return false;
            } else {
                NumberTextBox ntb = DataChecker.CheckInputNumeric(ntbList);
                if (ntb != null) {
                    ntb.Focus();
                    ShowMsg.TextInputError(this);
                    return false;
                } else {
                    return true;
                }
            }
        }
        /// <summary>
        /// 计算开始
        /// </summary>
        private void btnCal_Click(object sender, RoutedEventArgs e) {
            if (IsInputLegal()) {
                grdResult.Visibility = Visibility.Collapsed;
                if (rbtOmigaExp.IsChecked == true) {
                    ntbSaturatedLiquidVolume.Text = Model.Rackett.GetSaturateLiquidVolume(
                        Convert.ToDouble(ntbCriticalTemperature.Text),
                        Convert.ToDouble(ntbCriticalPressure.Text),
                        Convert.ToDouble(ntbAcentricFactor.Text),
                        Convert.ToDouble(ntbActualTemperature.Text)
                        ).ToString();
                } else if (rbtAlphaBetaExp.IsChecked == true) {
                    ntbSaturatedLiquidVolume.Text = Model.Rackett.GetSaturateLiquidVolume(
                        Convert.ToDouble(ntbCriticalTemperature.Text),
                        Convert.ToDouble(ntbCriticalPressure.Text),
                        Convert.ToDouble(ntbAlpha.Text),
                        Convert.ToDouble(ntbBeta.Text),
                        Convert.ToDouble(ntbActualTemperature.Text)
                        ).ToString();
                }
                ShowResult();
            }
        }
        /// <summary>
        /// 后退
        /// </summary> 
        private void btnBack_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new OtherFuncStartView());
        }
        #region 页面加载后续回退按钮动画
        /// <summary>
        /// 页面加载后续回退按钮动画
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            btnBack.IsHitTestVisible = false;
            rbtOmigaExp.IsChecked = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                ThicknessAnimation ta = ControlAnimations.FrameMoveOutAnimation;
                DoubleAnimation oa = ControlAnimations.FadeInAnimation;
                Storyboard sb = new Storyboard();
                sb.Duration = ControlAnimations.GeneralAniDuation;
                sb.Children.Add(oa);
                sb.Children.Add(ta);
                sb.BeginTime = TimeSpan.FromMilliseconds(500);
                Storyboard.SetTarget(oa, btnBack);
                Storyboard.SetTarget(ta, btnBack);
                Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
                Storyboard.SetTargetProperty(ta, new PropertyPath("Margin"));
                ta.Completed += Sb_Completed;
                sb.Begin();
            });
        }
        /// <summary>
        /// 重启 回退按钮命中测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sb_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= Sb_Completed;
            btnBack.IsHitTestVisible = true;
        }
        #endregion
        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            string imageUri;
            if (sender == rbtOmigaExp) {
                ntbAcentricFactor.Visibility = Visibility.Visible;
                ntbAlpha.Visibility = Visibility.Collapsed;
                imageUri = "/Resx/Dictionary/Pic/RackettEquationOmiga.png"; 
            } else if (sender == rbtAlphaBetaExp) {
                ntbAcentricFactor.Visibility = Visibility.Collapsed;
                ntbAlpha.Visibility = Visibility.Visible;
                imageUri = "/Resx/Dictionary/Pic/RackettEquationAlphaBeta.png";
            } else {
                return;
            }
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(imageUri, UriKind.Relative);
            bi3.EndInit();
            img.Source = bi3;
        }
        /// <summary>
        /// 展示结果
        /// </summary>
        private void ShowResult() {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                grdResult.Visibility = Visibility.Visible;
                ThicknessAnimation ta = ControlAnimations.FrameMoveInAnimation;
                DoubleAnimation oa = ControlAnimations.FadeInAnimation;
                Storyboard sb = new Storyboard();
                sb.Duration = ControlAnimations.GeneralAniDuation;
                sb.Children.Add(oa);
                sb.Children.Add(ta);
                Storyboard.SetTarget(oa, grdResult);
                Storyboard.SetTarget(ta, grdResult);
                Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
                Storyboard.SetTargetProperty(ta, new PropertyPath("Margin"));
                sb.Begin();
            });
        }
    }
}
