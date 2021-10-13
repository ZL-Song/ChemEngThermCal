using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using ChemEngThermCal.Controls;

namespace ChemEngThermCal.View {
    /// <summary>
    /// GeneralizedCorrelationEquationView.xaml 的交互逻辑
    /// </summary>
    public partial class PfGceCalPView : Page {
        public PfGceCalPView() {
            InitializeComponent();
            grdResult.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 所有文本框的集合
        /// </summary>
        private List<NumberTextBox> ntbList
            => new List<NumberTextBox> { ntbTc, ntbPc, ntbW, ntbT, ntbV };
        /// <summary>
        /// 组合框的集合
        /// </summary>
        private List<HeaderComboBox> hcbList
            => new List<HeaderComboBox> { hcbCalMethod };
        /// <summary>
        /// 输入合法性检查
        /// </summary>
        /// <returns>true：输入合法可以继续；false ：输入非法</returns>
        private bool IsInputLegal() {
            HeaderComboBox hcb = DataChecker.CheckComboSelection(hcbList);
            if (hcb != null) {
                hcbCalMethod.IsDropDownOpen = true;
                ShowMsg.ComboSelectionError(this);
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
        /// 计算开始的事件，secVirial 异步执行，其他同步执行
        /// </summary>
        private void btnCal_Click(object sender, RoutedEventArgs e) {
            grdResult.Visibility = Visibility.Collapsed;
            (sender as Button).IsEnabled = false;
            if (IsInputLegal()) {
                Model.CorStt.CalPressure.CalPressurePureBaseMediator calProc;
                if (hcbCalMethod.SelectedIndex == 0) {
                    calProc = new Model.CorStt.CalPressure.UsingSecVirial.SecVirialPureMediator(
                        new Model.CorStt.Models.SecVirial.ClassicPureModel(),
                        new Model.Material.Chemical(
                            Convert.ToDouble(ntbTc.Text),
                            Convert.ToDouble(ntbPc.Text),
                            Convert.ToDouble(ntbW.Text)
                            ),
                        Convert.ToDouble(ntbT.Text),
                        Convert.ToDouble(ntbV.Text)
                        );
                    OutPutResult(DoCal(calProc as Model.CorStt.CalPressure.UsingSecVirial.SecVirialPureMediator));
                } else if (hcbCalMethod.SelectedIndex == 1) {
                    calProc = new Model.CorStt.CalPressure.UsingCprDiagram.CprDiagPureMediator(
                      new Model.CorStt.Models.Diagram.CprModel(new CprDiagInterator()),
                      new Model.Material.Chemical(
                          Convert.ToDouble(ntbTc.Text),
                          Convert.ToDouble(ntbPc.Text),
                          Convert.ToDouble(ntbW.Text)
                      ),
                      Convert.ToDouble(ntbT.Text),
                      Convert.ToDouble(ntbV.Text)
                      );
                    OutPutResult((calProc as Model.CorStt.CalPressure.UsingCprDiagram.CprDiagPureMediator).GetResult());
                } 
                ShowResult();
            }
            (sender as Button).IsEnabled = true;
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
        /// <summary>
        /// 异步执行的计算过程 cubicCalOrder.Inilialize()，至少进行2000ms
        /// </summary>
        /// <param name="calProc">要进行计算的视图模型</param>
        /// <returns></returns>
        private Model.CorStt.CalPressure.CalPressurePureBaseMediator.CalPressureResultBase DoCal(Model.CorStt.CalPressure.UsingSecVirial.SecVirialPureMediator calProc)
            => calProc.GetResult();
        /// <summary>
        /// 输出计算结果
        /// </summary>
        /// <param name="result">计算过程字符串记录器的泛型数据</param>
        private void OutPutResult(Model.CorStt.CalPressure.CalPressurePureBaseMediator.CalPressureResultBase result) {
            ntbResultVP.Text = "";
            ntbResultVZ.Text = "";
            //输入条件
            runT.Text = result.GasResult.Temperature.ToString();
            runV.Text = result.GasResult.MoleVol.ToString();
            runTc.Text = result.TargetChemical.CriticalTemperature.ToString();
            runPc.Text = result.TargetChemical.CriticalPressure.ToString();
            runW.Text = result.TargetChemical.AcentricFactor.ToString();
            //方程参数
            runTr.Text = result.RelativeTemperature.ToString();
            secCalModeInfoText.Blocks.Clear();

            string calMode = "";
            if (result is Model.CorStt.CalPressure.UsingSecVirial.SecVirialPureMediator.Result) {
                calMode = $"Calculate model : Using second virial coefficient.";
            } else if (result is Model.CorStt.CalPressure.UsingCprDiagram.CprDiagPureMediator.Result) {
                calMode = $"Calculate model : Using compressibility factor diagram.";
            }
            secCalModeInfoText.Blocks.Add(new Paragraph(new Run(calMode)));

            //迭代过程
            secItrProc.Blocks.Clear();
            int gasStepCount = 1;
            foreach (Model.CorStt.CalPressure.GceStepInfo stepInfo in result.GasStepInfo) {
                string step = $"Iteration Step {gasStepCount} : \n\nZ({gasStepCount}) = {stepInfo.CprFactor}\nPr = {stepInfo.RelativePressure}\nZº = {stepInfo.CprFactorBase} \nZ¹ = {stepInfo.CprFactorCrec} \nZ({gasStepCount + 1}) = {stepInfo.NextCprFactor}\n|Z({gasStepCount + 1}) - Z({gasStepCount})| = {stepInfo.DiffFlag}";
                secItrProc.Blocks.Add(new Paragraph(new Run(step)));
                gasStepCount++;
            }
            //计算结果
            secResult.Blocks.Clear();
            Paragraph paraResult = new Paragraph();
            if (result.GasResult.IsConverged) {
                ntbResultVP.Text = result.GasResult.Pressure.ToString();
                ntbResultVZ.Text = result.GasResult.CprFactor.ToString();
                paraResult.Inlines.Add(new Run($"Compressibility Factor Z = {result.GasResult.CprFactor} \nPressure of Chemical = {result.GasResult.Pressure} MPa"));
            } else {
                ntbResultVP.Text = "null";
                ntbResultVZ.Text = "null";
                if (result.GasResult.CprFactor == -1.0) {
                    paraResult.Inlines.Add(new Run("The iteration diverges,\nthis is probably because such model is not applicable while Pr > 8,\nplease try other thermdynamic model (Cubic E.O.S. is recommended)."));
                    ntbResultVP.Text = "null";
                    ntbResultVZ.Text = "null";
                } else if (result.GasResult.CprFactor == -2.0) {
                    paraResult.Inlines.Add(new Run("The iteration could not converge in 100 steps,\nthis is probably because that the choosen model is not applicable while Pr > 8,\nplease try other thermdynamic model (Cubic E.O.S. is recommended)."));
                    ntbResultVP.Text = "null";
                    ntbResultVZ.Text = "null";
                } else {
                    if (result is Model.CorStt.CalPressure.UsingCprDiagram.CprDiagPureMediator.Result && result.GasResult.CprFactor == -3.0) {
                        paraResult.Inlines.Add(new Run("The calculate process is cancelled."));
                    }
                }
            }
            secResult.Blocks.Add(paraResult);
        }
        #region 页面加载后续回退按钮动画
        /// <summary>
        /// 页面加载后续回退按钮动画
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            btnBack.IsHitTestVisible = false;
            if (App.isDbExist == true) {
                DbBtnCon.Visibility = Visibility.Visible;
            } else {
                DbBtnCon.Visibility = Visibility.Collapsed;
            }
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
        private void btnBack_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new OtherFuncStartView());
        }

        private void DbBtnCon_Click(object sender, RoutedEventArgs e) {
            DbConView db = new DbConView();
            db.Owner = Window.GetWindow(this);
            db.ShowDialog();
            if (db.IsChemGot == true) {
                ntbTc.Text = db.Chem.Tc.ToString();
                ntbPc.Text = db.Chem.Pc.ToString();
                ntbW.Text = db.Chem.W.ToString();
            } else {
                return;
            }
        }
    }
}