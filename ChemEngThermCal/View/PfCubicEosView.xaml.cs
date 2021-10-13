using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Documents;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChemEngThermCal.Controls;

namespace ChemEngThermCal.View {
    /// <summary>
    /// CubicEosView.xaml 的交互逻辑
    /// </summary>
    public partial class PfCubicEosView : Page {
        public PfCubicEosView() {
            InitializeComponent();
            grdResult.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 计算开始，异步执行计算
        /// </summary>
        private void btnCal_Click(object sender, RoutedEventArgs e) {
            grdResult.Visibility = Visibility.Collapsed;
            (sender as Button).IsEnabled = false;
            if (IsInputLegal()) {//输入合法，则初始化计算模型，进行计算过程，等待结果返回
                double temperature = Convert.ToDouble(ntbT.Text);
                double pressure = Convert.ToDouble(ntbP.Text);
                double criticalTemperature = Convert.ToDouble(ntbTc.Text);
                double criticalPressure = Convert.ToDouble(ntbPc.Text);
                Model.Ceos.CalMoleVolume.CeosPureMediator calMng;
                if (rbtVdW.IsChecked == true) {//vdw
                    calMng = new Model.Ceos.CalMoleVolume.CeosPureMediator(
                        new Model.Ceos.Models.VanderWaalsModel(),
                        new Model.Material.Chemical(criticalTemperature, criticalPressure),
                        temperature,
                        pressure);
                } else if (rbtRK.IsChecked == true) {//rk
                    calMng = new Model.Ceos.CalMoleVolume.CeosPureMediator(
                        new Model.Ceos.Models.RedlichKwongModel(),
                        new Model.Material.Chemical(criticalTemperature, criticalPressure),
                        temperature,
                        pressure);
                } else if (rbtSRK.IsChecked == true) {//srk
                    double acentricFactor = Convert.ToDouble(ntbW.Text);
                    calMng = new Model.Ceos.CalMoleVolume.CeosPureMediator(
                        new Model.Ceos.Models.SoaveRedlichKwongModel(),
                        new Model.Material.Chemical(criticalTemperature, criticalPressure, acentricFactor),
                        temperature,
                        pressure);
                } else {//pr
                    double acentricFactor = Convert.ToDouble(ntbW.Text);
                    calMng = new Model.Ceos.CalMoleVolume.CeosPureMediator(
                        new Model.Ceos.Models.PengRobinsonModel(),
                        new Model.Material.Chemical(criticalTemperature, criticalPressure, acentricFactor),
                        temperature,
                        pressure);
                }
                OutPutResult(DoCal(calMng));
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
                ThicknessAnimation ta = View.ControlAnimations.FrameMoveInAnimation;
                DoubleAnimation oa = View.ControlAnimations.FadeInAnimation;
                Storyboard sb = new Storyboard();
                sb.Duration = View.ControlAnimations.GeneralAniDuation;
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
        /// 异步计算开始
        /// </summary>
        private Model.Ceos.CalMoleVolume.CeosPureMediator.Result DoCal(Model.Ceos.CalMoleVolume.CeosPureMediator calProc)
            => calProc.GetResult();
        /// <summary>
        /// 显示结果
        /// </summary> 
        private void OutPutResult(Model.Ceos.CalMoleVolume.CeosPureMediator.Result calResult) {
            //输入条件
            runT.Text = calResult.GasResult.Temperature.ToString();
            runP.Text = calResult.GasResult.Pressure.ToString();
            runTc.Text = calResult.TargetChemical.CriticalTemperature.ToString();
            runPc.Text = calResult.TargetChemical.CriticalPressure.ToString();
            runOmiga.Text = calResult.TargetChemical.AcentricFactor.ToString();
            //方程类型
            switch (calResult.ModelType) {
                case Model.Ceos.Models.CubicEosType.VanDerWaals:
                    runCubicEosType.Text = "Van der Waals";
                    break;
                case Model.Ceos.Models.CubicEosType.RedlichKwong:
                    runCubicEosType.Text = "Redlich Kwong";
                    break;
                case Model.Ceos.Models.CubicEosType.SoaveRedlichKwong:
                    runCubicEosType.Text = "Soave Redlich Kwong";
                    break;
                case Model.Ceos.Models.CubicEosType.PengRobinson:
                    runCubicEosType.Text = "Peng Robinson";
                    break;
                default:
                    runCubicEosType.Text = "Unkonwn";
                    break;
            }
            //方程参数 a(T)/b(T)/A(T)/B(T)
            runAT.Text = calResult.Ceos_At.ToString();
            runBT.Text = calResult.Ceos_Bt.ToString();
            runAZ.Text = calResult.Ceos_AZ.ToString();
            runBZ.Text = calResult.Ceos_BZ.ToString();
            //迭代过程
            //清空 Section
            secGasItrProc.Blocks.Clear();
            secLiquidItrProc.Blocks.Clear();
            secResult.Blocks.Clear();
            Paragraph paraGasInil = new Paragraph();
            paraGasInil.Inlines.Add(new Run("Gas phase :"));
            secGasItrProc.Blocks.Add(paraGasInil);
            int gasStepCount = 1;
            foreach (Model.Ceos.CalMoleVolume.CeosStepInfo stepInfo in calResult.GasStep) {
                string step = $"Iteration Step {gasStepCount} :\n\nZ({gasStepCount}) = {stepInfo.StepCprFactor}\nf(Z) = {stepInfo.StepFunc}\nf'(Z) = {stepInfo.StepDerivFunc}\nZ({gasStepCount + 1}) = {stepInfo.StepNextCprFactor}\n|Z({gasStepCount}) - Z({gasStepCount + 1})| = {stepInfo.StepDiff}";
                secGasItrProc.Blocks.Add(new Paragraph(new Run(step)));
                gasStepCount++;
            }
            Paragraph paraGasFin = new Paragraph();
            paraGasFin.Inlines.Add(new Run("Iteration of gas phase finished."));
            secGasItrProc.Blocks.Add(paraGasFin);
            if (!calResult.IsLiquidPhaseExist) {
                secGasItrProc.Blocks.Add(new Paragraph(new Run("Tc > or = T, liquid phase does NOT exist.")));
            } else {
                if (!calResult.isLiquidPhaseApplicable) {
                    secGasItrProc.Blocks.Add(new Paragraph(new Run("Selected model is not applicable to liquid phase.")));
                } else {
                    Paragraph paraLiqInil = new Paragraph();
                    paraLiqInil.Inlines.Add(new Run("Liquid phase :"));
                    secLiquidItrProc.Blocks.Add(paraLiqInil);
                    int liqStepCount = 1;
                    foreach (Model.Ceos.CalMoleVolume.CeosStepInfo stepInfo in calResult.LiqStep) {
                        string step = $"Iteration Step {liqStepCount} :\n\nZ({liqStepCount}) = {stepInfo.StepCprFactor}\nf(Z) = {stepInfo.StepFunc}\nf'(Z) = {stepInfo.StepDerivFunc}\nZ({liqStepCount + 1}) = {stepInfo.StepNextCprFactor}\n|Z({liqStepCount}) - Z({liqStepCount + 1})| = {stepInfo.StepDiff}";
                        secLiquidItrProc.Blocks.Add(new Paragraph(new Run(step)));
                        liqStepCount++;
                    }
                    Paragraph paraLiqFin = new Paragraph();
                    paraLiqFin.Inlines.Add(new Run("Iteration of liquid phase finished."));
                    secLiquidItrProc.Blocks.Add(paraLiqFin);
                }
            }
            Paragraph paraGasResult = new Paragraph();
            paraGasResult.Inlines.Add(new Run("Gas phase :\n\n"));
            if (calResult.GasResult.IsConverged) {
                paraGasResult.Inlines.Add(new Run($"Mole volume V = {calResult.GasResult.MoleVol} cm³/mol\nCompressibility factor Z ={calResult.GasResult.CprFactor}"));
                ntbResultVV.Text = calResult.GasResult.MoleVol.ToString();
                ntbResultVZ.Text = calResult.GasResult.CprFactor.ToString();
            } else {
                double gasCprFactor = calResult.GasResult.CprFactor;
                if (gasCprFactor == -1.0) {
                    paraGasResult.Inlines.Add(new Run("The iteration for gas phase diverges,\n please try other cubic equation of state."));
                } else if (gasCprFactor == -2.0) {
                    paraGasResult.Inlines.Add(new Run("The iteration for gas phase could not converge in 100 steps,\n please try other cubic equation of state."));
                }
                ntbResultVV.Text = "null";
                ntbResultVZ.Text = "null";
            }
            secResult.Blocks.Add(paraGasResult);
            Paragraph paraLiqResult = new Paragraph();
            paraLiqResult.Inlines.Add(new Run("Liquid phase :\n\n"));
            double liqCprFactor = calResult.LiqResult.CprFactor;
            if (calResult.LiqResult.IsConverged) {
                paraLiqResult.Inlines.Add(new Run($"Mole volume V = {calResult.LiqResult.MoleVol} cm³/mol\nCompressibility factor Z ={calResult.LiqResult.CprFactor}"));
                ntbResultLV.Visibility = Visibility.Visible;
                ntbResultLV.Text = calResult.LiqResult.MoleVol.ToString();
                ntbResultLZ.Text = calResult.LiqResult.CprFactor.ToString();
            } else {
                if (!calResult.IsLiquidPhaseExist) {
                    paraLiqResult.Inlines.Add("Liquid phase does not exist as T > or = Tc.");
                    ntbResultLV.Visibility = Visibility.Collapsed;
                } else {
                    if (!calResult.isLiquidPhaseApplicable) {
                        paraLiqResult.Inlines.Add("Selected model is not applicable to liquid phase.");
                        ntbResultLV.Visibility = Visibility.Collapsed;
                    } else {
                        if (liqCprFactor == -1.0) {
                            paraLiqResult.Inlines.Add(new Run("The iteration for liquid phase diverges,\n please try other cubic equation of state."));
                        } else if (liqCprFactor == -2.0) {
                            paraLiqResult.Inlines.Add(new Run("The iteration for liquid phase could not converge in 100 steps,\n please try other cubic equation of state."));
                        }
                    }
                }
                ntbResultLV.Text = "null";
                ntbResultLZ.Text = "null";
            }
            secResult.Blocks.Add(paraLiqResult);
        }
        /// <summary>
        /// 全部 TextBox 类型成员的集合
        /// </summary>
        private List<NumberTextBox> ntbList
        {
            get
            {
                if (rbtVdW.IsChecked == true || rbtRK.IsChecked == true) {
                    return new List<NumberTextBox> { ntbTc, ntbPc, ntbT, ntbP };
                } else {
                    return new List<NumberTextBox> { ntbTc, ntbPc, ntbW, ntbT, ntbP };
                }
            }
        }
        /// <summary>
        /// 全部 RadioButton 类型成员的集合
        /// </summary>
        private List<RadioButton> rbtList
            => new List<RadioButton> { rbtVdW, rbtRK, rbtSRK, rbtPR };
        /// <summary>
        /// 输入检测
        /// </summary>
        /// <returns>true：输入合法可以继续；false ：输入非法</returns>
        private bool IsInputLegal() {
            bool isEquationSelected = DataChecker.CheckRadioSelection(rbtList);
            if (!isEquationSelected) {
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
        #region SCP 中 txtW 的可见性调整效果
        /// <summary>
        /// SCP 的尺寸调整及 txtW 可见性调整，由 RadioButton 调用
        /// </summary>
        private void RadioButtonChecked(object sender, RoutedEventArgs e) {
            //新建线程开始动画
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                Storyboard sb = new Storyboard();                           //新故事版
                sb.Duration = View.ControlAnimations.GeneralAniDuation;
                if (rbtVdW.IsChecked == true || rbtRK.IsChecked == true) {
                    if (ntbW.Visibility == Visibility.Visible) {
                        IsHitTestVisible = false;
                        //当 VdW 或 RK 方程时的动画目标
                        DoubleAnimation da = View.ControlAnimations.TextBoxHeightDownAnimation;
                        //动画执行完毕时的委托
                        da.Completed += oa_Completed;
                        sb.Children.Add(da);
                        Storyboard.SetTarget(da, ntbW);
                        Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                        sb.Begin();
                    }
                } else if (rbtSRK.IsChecked == true || rbtPR.IsChecked == true) {
                    if (ntbW.Visibility == Visibility.Collapsed) {
                        IsHitTestVisible = false;
                        ntbW.Visibility = Visibility.Visible;
                        //当 SRK 或 PR 方程时的动画目标
                        DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
                        //动画执行完毕时的委托
                        da.Completed += oa_Completed;
                        sb.Children.Add(da);
                        Storyboard.SetTarget(da, ntbW);
                        Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                        sb.Begin();
                    }
                }
            });
        }
        /// <summary>
        /// 在 rdTxtW 高度变换完毕后执行 txtW 的渐入动画，并发执行
        /// </summary>
        void oa_Completed(object sender, EventArgs e) {
            IsHitTestVisible = true;
            (sender as AnimationClock).Completed -= oa_Completed;
            if (rbtVdW.IsChecked == true) {
                ntbW.Visibility = Visibility.Collapsed;
            } else if (rbtRK.IsChecked == true) {
                ntbW.Visibility = Visibility.Collapsed;
            } else if (rbtSRK.IsChecked == true || rbtPR.IsChecked == true) {
                ntbW.Visibility = Visibility.Visible;
            }
        }
        #endregion SCP 中 txtW 的可见性调整效果 结束 
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

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            if (App.isDbExist == true) {
                DbBtnCon.Visibility = Visibility.Visible;
            } else {
                DbBtnCon.Visibility = Visibility.Collapsed;
            }
        }
    }
}