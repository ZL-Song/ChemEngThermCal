using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using ChemEngThermCal.Controls;
using System.Windows.Media.Imaging;

namespace ChemEngThermCal.View {
    /// <summary>
    /// PfAntoineView.xaml 的交互逻辑
    /// </summary>
    public partial class PfAntoineView : Page {
        public PfAntoineView() {
            InitializeComponent();
            grdResult.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 单位为 kPa OC 时的 Antoine 方程形式时的 TextBox 类型成员的集合
        /// </summary>
        private List<NumberTextBox> ntbList
        {
            get
            {
                if (rbtKPaOcExp.IsChecked == true) {
                    return new List<NumberTextBox> { ntbKPaOcIndexA, ntbKPaOcIndexB, ntbKPaOcIndexC, ntbActualKPaOcTemperature };
                } else if (rbtMPaKExp.IsChecked == true) {
                    return new List<NumberTextBox> { ntbMPaKIndexA, ntbMPaKIndexB, ntbMPaKIndexC, ntbActualMPaKTemperature };
                } else if (rbtPaKExp.IsChecked == true) {
                    return new List<NumberTextBox> { ntbPaKIndexA, ntbPaKIndexB, ntbPaKIndexC, ntbActualPaKTemperature };
                } else {
                    return null;
                }
            }
        }
        /// <summary>
        /// 输入合法性检查
        /// </summary>
        /// <returns>true 表示输入合法，false 表示输入非法</returns>
        private bool IsInputLegal() {
            NumberTextBox ntb = DataChecker.CheckInputNumeric(ntbList);
            if (ntb != null) {
                ShowMsg.TextInputError(this);
                ntb.Focus();
                return false;
            }
            return true;
        }
        /// <summary>
        /// 进行计算
        /// </summary>
        private void btnCal_Click(object sender, RoutedEventArgs e) {
            Model.Material.Chemical chem = new Model.Material.Chemical(500, 3);
            Model.Antoine.AntoineModelBase calModel;
            Model.Antoine.AntoineMediator calProc;
            if (rbtKPaOcExp.IsChecked == true) {
                if (IsInputLegal()) {
                    grdResult.Visibility = Visibility.Collapsed;
                    chem.SetAntoinePara(Convert.ToDouble(ntbKPaOcIndexA.Text), Convert.ToDouble(ntbKPaOcIndexB.Text), Convert.ToDouble(ntbKPaOcIndexC.Text));
                    calModel = new Model.Antoine.KiloPascalCelsiusAntoineModel();
                    calProc = new Model.Antoine.AntoineMediator(calModel);
                    ntbKPaOcSaturatedPressure.Text = calProc.GetSaturatedPressure(chem, Convert.ToDouble(ntbActualKPaOcTemperature.Text)).ToString();
                    ShowResult();
                }
            } else if (rbtMPaKExp.IsChecked == true) {
                if (IsInputLegal()) {
                    grdResult.Visibility = Visibility.Collapsed;
                    chem.SetAntoinePara(Convert.ToDouble(ntbMPaKIndexA.Text), Convert.ToDouble(ntbMPaKIndexB.Text), Convert.ToDouble(ntbMPaKIndexC.Text));
                    calModel = new Model.Antoine.MegaPascalKevinAntoineModel();
                    calProc = new Model.Antoine.AntoineMediator(calModel);
                    ntbMPaKSaturatedPressure.Text = calProc.GetSaturatedPressure(chem, Convert.ToDouble(ntbActualMPaKTemperature.Text)).ToString();
                    ShowResult();
                }
            } else if (rbtPaKExp.IsChecked == true) {
                if (IsInputLegal()) {
                    grdResult.Visibility = Visibility.Collapsed;
                    chem.SetAntoinePara(Convert.ToDouble(ntbPaKIndexA.Text), Convert.ToDouble(ntbPaKIndexB.Text), Convert.ToDouble(ntbPaKIndexC.Text));
                    calModel = new Model.Antoine.MegaPascalKevinAntoineModel();
                    calProc = new Model.Antoine.AntoineMediator(calModel);
                    ntbPaKSaturatedPressure.Text = calProc.GetSaturatedPressure(chem, Convert.ToDouble(ntbActualPaKTemperature.Text)).ToString();
                    ShowResult();
                }
            }
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
        #region 在 rbtOmigaExp 或 rbtAlphaBetaExp 选定时不同的 txt 可见性调整，其中 txt 的 visibility,  属性已分别绑定到相应 conditionNtb 的相应属性
        //淡出效果
        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            string imageUri;
            if (sender == rbtKPaOcExp) {
                ntbActualMPaKTemperature.Visibility = Visibility.Collapsed;
                ntbActualKPaOcTemperature.Visibility = Visibility.Visible;
                ntbActualPaKTemperature.Visibility = Visibility.Collapsed;
                imageUri = "/Resx/Dictionary/Pic/AntoineEquationkPaC.png";
            } else if (sender == rbtMPaKExp) {
                ntbActualMPaKTemperature.Visibility = Visibility.Visible;
                ntbActualKPaOcTemperature.Visibility = Visibility.Collapsed;
                ntbActualPaKTemperature.Visibility = Visibility.Collapsed;
                imageUri = "/Resx/Dictionary/Pic/AntoineEquationMPaK.png";
            } else if (sender == rbtPaKExp) {
                ntbActualMPaKTemperature.Visibility = Visibility.Collapsed;
                ntbActualKPaOcTemperature.Visibility = Visibility.Collapsed;
                ntbActualPaKTemperature.Visibility = Visibility.Visible;
                imageUri = "/Resx/Dictionary/Pic/AntoineEquationPaK.png";

            } else {
                return;
            }
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(imageUri, UriKind.Relative);
            bi3.EndInit();
            img.Source = bi3;
        }
        #endregion
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
            rbtMPaKExp.IsChecked = true;
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
    }
}