using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChemEngThermCal.Controls;
using ChemEngThermCal.Model.Vle.Eos;

namespace ChemEngThermCal.View {
    /// <summary>
    /// VleForPureFluid.xaml 的交互逻辑
    /// </summary>
    public partial class PfVleView : Page {
        public PfVleView() {
            InitializeComponent();
            grdResult.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 开始异步计算
        /// </summary>
        private void btnCal_Click(object sender, RoutedEventArgs e) {
            grdResult.Visibility = Visibility.Collapsed;
            (sender as Button).IsEnabled = false;
            if (IsInputLegal()) {//输入合法，则初始化计算模型，进行计算过程，等待结果返回
                double criticalTemperature = Convert.ToDouble(ntbTc.Text);
                double criticalPressure = Convert.ToDouble(ntbPc.Text);
                double acentricFactor = Convert.ToDouble(ntbW.Text);
                Model.Material.Chemical chem = new Model.Material.Chemical(criticalTemperature, criticalPressure, acentricFactor);
                Model.Vle.FugCoe.CeosFugAdapter fugAdapter;
                if (hcbFugacity.SelectedIndex == 0) {
                    fugAdapter = new Model.Vle.FugCoe.CeosFugAdapter(new Model.Ceos.Models.RedlichKwongModel());
                } else if (hcbFugacity.SelectedIndex == 1) {
                    fugAdapter = new Model.Vle.FugCoe.CeosFugAdapter(new Model.Ceos.Models.SoaveRedlichKwongModel());
                } else {
                    fugAdapter = new Model.Vle.FugCoe.CeosFugAdapter(new Model.Ceos.Models.PengRobinsonModel());
                }
                PureMediator calMng;
                if (rbtIsobaric.IsChecked == true) {
                    double pressure = Convert.ToDouble(ntbP.Text);
                    calMng = new PureMediator(PureMediator.CalType.Isobaric, pressure, fugAdapter, chem);
                } else {
                    double temperature = Convert.ToDouble(ntbT.Text);
                    calMng = new PureMediator(PureMediator.CalType.Isothermal, temperature, fugAdapter, chem);
                }
                OutPutResult( DoCal(calMng));
                ShowResult();
            }
            (sender as Button).IsEnabled = true;
            #region MyRegion
            //Model.PureFluid.Vle.TemperatureKnownVleModel model = new Model.PureFluid.Vle.TemperatureKnownVleModel(new Model.Chemical(425.4, 3.797, 0.193), 273.15);
            //model.Calculate();
            //Model.PureFluid.Vle.PressureKnownVleModel model = new Model.PureFluid.Vle.PressureKnownVleModel(new Model.Chemical(408.1, 3.648, 0.176), 3.0);
            //ntbP.Text = "3.0";
            //ntbT.Text = "273.15";
            //ntbTc.Text = "425.4";
            //ntbPc.Text = "3.797";
            //ntbW.Text = "0.193";
            //fdsvOutPut.Visibility = Visibility.Collapsed;
            //pgbCalProcess.IsIndeterminate = true;
            //(sender as Button).IsEnabled = false;
            //if (IsInputLegal()) {
            //    ViewModel.PfVleViewModel calModel;
            //    if (rbtObjP.IsChecked == true) {
            //        calModel = new ViewModel.PfVleViewModel(ViewModel.PfVleViewModel.IndependentVariableType.Pressure, ntbP.Text, ntbTc.Text, ntbPc.Text, ntbW.Text);
            //    } else {
            //        calModel = new ViewModel.PfVleViewModel(ViewModel.PfVleViewModel.IndependentVariableType.Temperature, ntbT.Text, ntbTc.Text, ntbPc.Text, ntbW.Text);
            //    }
            //    OutPutResult(await DoCal(calModel));
            //    fdsvOutPut.Visibility = Visibility.Visible;
            //}
            //pgbCalProcess.IsIndeterminate = false;
            //(sender as Button).IsEnabled = true;
            //Model.Vle.Isobaric.PureCeosVleMediator med
            //    = new Model.Vle.Isobaric.PureCeosVleMediator(
            //        new Model.Vle.FugCoe.CeosFugacityAdapter(new Model.Material.Chemical(600, 3, 0.5), new Model.Ceos.Models.PengRobinsonModel()),
            //        0.2);
            //Model.Vle.Isobaric.PureCeosVleMediator.Result result = med.GetResult();
            //Console.WriteLine(result.InilTemperature);
            //Console.WriteLine(result.Temperature );
            //Console.WriteLine(result.Pressure);
            //Console.WriteLine(Math.Log(result.GasFugacity));
            //Console.WriteLine(result.GasResidualEnthalpy);
            //Console.WriteLine(result.GasResidualEntropy);
            //Console.WriteLine(Math.Log(result.LiqFugacity));
            //Console.WriteLine(result.LiqResidualEnthalpy);
            //Console.WriteLine(result.LiqResidualEntropy);
            //Console.WriteLine(result.VaporizationEnthalpy);
            //Console.WriteLine(result.Vaporizationentropy);
            //foreach (Model.Vle.Isobaric.PureCeosVleMediator.StepInfo step in result.ItrStepInfoList) {
            //    Console.WriteLine("------+-++-+-+-+-+-+-+-+-+");
            //    foreach (Model.Ceos.CalMoleVolume.CeosStepInfo ceosStep in step.FugResult.EosResult.GasStep) {
            //        Console.WriteLine(ceosStep.StepFunc);
            //    }
            //    Console.WriteLine(step.DiffValue);
            //}
            #endregion
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
        /// 等压异步计算
        /// </summary>
        private PureMediator.Result DoCal(PureMediator calModel)
            => calModel.GetResult();
        /// <summary>
        /// 输出 等压计算 结果
        /// </summary>
        /// <param name="calResult">包含计算有关的所有信息</param>
        private void OutPutResult(PureMediator.Result calResult) {

            ntbResultVV.Text = "";
            ntbResultLV.Text = "";
            secItrProc.Blocks.Clear();
            secResult.Blocks.Clear();

            runTc.Text = calResult.Chemical.CriticalTemperature.ToString();
            runPc.Text = calResult.Chemical.CriticalPressure.ToString();
            runOmiga.Text = calResult.Chemical.AcentricFactor.ToString();
            if (calResult.CalculateType == PureMediator.CalType.Isobaric) {
                runIV.Text = $"P = {calResult.GasResult.Pressure} MPa";
            } else if (calResult.CalculateType == PureMediator.CalType.Isothermal) {
                runIV.Text = $"T = {calResult.GasResult.Temperature} K";
            } else {
                runIV.Text = $"Unknown Calculate.";
            }
            switch (calResult.ItrStepInfoList.ToArray()[0].FugResult.EosResult.ModelType) {
                case Model.Ceos.Models.CubicEosType.RedlichKwong:
                    runEosType.Text = " Redlich - Kwong";
                    break;
                case Model.Ceos.Models.CubicEosType.SoaveRedlichKwong:
                    runEosType.Text = " Soave - Redlich - Kwong";
                    break;
                case Model.Ceos.Models.CubicEosType.PengRobinson:
                    runEosType.Text = " Peng - Robinson";
                    break;
                default:
                    runEosType.Text = " Unknown";
                    break;
            }
            int outterStepFlag = 1;
            foreach (PureMediator.StepInfo vleStep in calResult.ItrStepInfoList) {
                string inilStr1;
                if (calResult.CalculateType == PureMediator.CalType.Isobaric) {
                    inilStr1 = $"Iteration Step {outterStepFlag} : \n\nAssumeed Temperature = {vleStep.DependentVar} K\n\n";
                } else if (calResult.CalculateType == PureMediator.CalType.Isothermal) {
                    inilStr1 = $"Iteration Step {outterStepFlag} : \n\nAssumeed Pressure = {vleStep.DependentVar} MPa\n\n";
                } else {
                    inilStr1 = $"No Info can be obtained.";
                }
                string inilStr2 = $"Key parameters of E.o.S : \na(T) = {vleStep.FugResult.EosResult.Ceos_At} MPa·(cm³/mol)²\nA(Z) = {vleStep.FugResult.EosResult.Ceos_AZ}\nb(T) = {vleStep.FugResult.EosResult.Ceos_Bt}cm³/mol\nB(Z) = {vleStep.FugResult.EosResult.Ceos_BZ}\n\n";
                string inilStr3 = $"/* -------- Iteration for gas phase -------- */";
                Paragraph paraInil = new Paragraph(new Run(inilStr1));
                paraInil.Inlines.Add(new Run(inilStr2));
                paraInil.Inlines.Add(new Run(inilStr3));
                secItrProc.Blocks.Add(paraInil);
                int gasStepCount = 1;
                foreach (Model.Ceos.CalMoleVolume.CeosStepInfo eosStep in vleStep.FugResult.EosResult.GasStep) {
                    string step = $"Iteration step {outterStepFlag}-{gasStepCount}-G :\n\nZ({gasStepCount}) = {eosStep.StepCprFactor}\nf(Z) = {eosStep.StepFunc}\nf'(Z) = {eosStep.StepDerivFunc}\nZ({gasStepCount + 1}) = {eosStep.StepNextCprFactor}\n|Z({gasStepCount}) - Z({gasStepCount + 1})| = {eosStep.StepDiff}";
                    secItrProc.Blocks.Add(new Paragraph(new Run(step)));
                    gasStepCount++;
                }
                string gasRes = $"/* -------- Iteration for gas phase finished -------- */\n\nMole volume of gas phase = {vleStep.FugResult.EosResult.GasResult.MoleVol} cm³/mol\nCompressibility factor of gas phase = {vleStep.FugResult.EosResult.GasResult.CprFactor}\nFugacity of gas phase = {vleStep.FugResult.GasFugacity}\n\n/* -------- Iteration for liquid phase -------- */";
                secItrProc.Blocks.Add(new Paragraph(new Run(gasRes)));
                int liqStepCount = 1;
                foreach (Model.Ceos.CalMoleVolume.CeosStepInfo eosStep in vleStep.FugResult.EosResult.LiqStep) {
                    string step = $"Iteration Step {outterStepFlag}-{liqStepCount}-L :\n\nZ({liqStepCount}) = {eosStep.StepCprFactor}\nf(Z) = {eosStep.StepFunc}\nf'(Z) = {eosStep.StepDerivFunc}\nZ({liqStepCount + 1}) = {eosStep.StepNextCprFactor}\n|Z({liqStepCount}) - Z({liqStepCount + 1})| = {eosStep.StepDiff}";
                    secItrProc.Blocks.Add(new Paragraph(new Run(step)));
                    liqStepCount++;
                }
                string liqRes = $"/* -------- Iteration for liquid phase finished -------- */\n\nMole volume of liquid phase = {vleStep.FugResult.EosResult.LiqResult.MoleVol} cm³/mol\nCompressibility factor of gas phase = {vleStep.FugResult.EosResult.LiqResult.CprFactor}\nFugacity of gas phase = {vleStep.FugResult.LiqFugacity}";
                secItrProc.Blocks.Add(new Paragraph(new Run(liqRes)));
                string fugRes = $"|φ(gas) - φ(liquid)| = {vleStep.DiffValue}";
                secItrProc.Blocks.Add(new Paragraph(new Run(fugRes)));
                outterStepFlag++;
            }

            if (calResult.GasResult.IsConverged == true && calResult.LiqResult.IsConverged == true) {
                Run runGas = new Run($"Gas phase :\n\nMole Volume = {calResult.GasResult.MoleVol} cm³/mol\nCompressibility factor Z = {calResult.GasResult.CprFactor}\n");
                Run runGas2 = new Run($"Fugacity coefficient lnφ = {Math.Log(calResult.GasResult.Fugacity)}\nDeviation enthalpy (H - H(ig)) / RT = {calResult.GasResult.ResidualEnthalpy}\nDeviation entropy (S - S(ig)) / R ={calResult.GasResult.ResidualEntropy}\n\n");

                Run runLiq = new Run($"Liq phase :\n\nMole Volume = {calResult.LiqResult.MoleVol} cm³/mol\nCompressibility factor Z = {calResult.LiqResult.CprFactor}\n");
                Run runLiq2 = new Run($"Fugacity coefficient lnφ = {Math.Log(calResult.LiqResult.Fugacity)}\nDeviation enthalpy (H - H(ig)) / RT = {calResult.LiqResult.ResidualEnthalpy}\nDeviation entropy (S - S(ig)) / R ={calResult.LiqResult.ResidualEntropy}\n");

                Run runDelta = new Run($"\nVaporization enthalpy ΔH(vap) = {calResult.VaporizationEnthalpy} J/mol\nVaporization entropy ΔS(vap) = {calResult.Vaporizationentropy} J/(mol·K)\n\n");

                Run runDV;
                if (calResult.CalculateType == PureMediator.CalType.Isobaric) {
                    runDV = new Run($"Temperature = {calResult.GasResult.Temperature.ToString()} K");
                    ntbResultT.Visibility = Visibility.Visible;
                    ntbResultP.Visibility = Visibility.Collapsed;
                    ntbResultT.Text = calResult.GasResult.Temperature.ToString();
                    ntbResultLV.Text = calResult.LiqResult.MoleVol.ToString();
                    ntbResultVV.Text = calResult.GasResult.MoleVol.ToString();
                } else if (calResult.CalculateType == PureMediator.CalType.Isothermal) {
                    runDV = new Run($"Pressure = {calResult.GasResult.Pressure.ToString()} MPa");
                    ntbResultP.Visibility = Visibility.Visible;
                    ntbResultT.Visibility = Visibility.Collapsed;
                    ntbResultP.Text = calResult.GasResult.Pressure.ToString();
                    ntbResultLV.Text = calResult.LiqResult.MoleVol.ToString();
                    ntbResultVV.Text = calResult.GasResult.MoleVol.ToString();
                } else {
                    runDV = new Run($"Unknown Calculate");
                    ntbResultP.Visibility = Visibility.Collapsed;
                    ntbResultT.Visibility = Visibility.Collapsed;
                }
                Paragraph paraResult = new Paragraph();
                paraResult.Inlines.Add(runGas);
                paraResult.Inlines.Add(runGas2);
                paraResult.Inlines.Add(runLiq);
                paraResult.Inlines.Add(runLiq2);
                paraResult.Inlines.Add(runDelta);
                paraResult.Inlines.Add(runDV);
                secResult.Blocks.Add(paraResult);
            } else {
                Paragraph paraResult = new Paragraph();
                double liqCpr = calResult.LiqResult.CprFactor;
                if (liqCpr == -1.0) {
                    paraResult.Inlines.Add(new Run("The iteration diverges."));
                } else if (liqCpr == -2.0) {
                    paraResult.Inlines.Add(new Run("The iteration could not converge in 100 steps."));
                } else if (liqCpr == -3.0) {
                    paraResult.Inlines.Add(new Run("Liquid phase does not exist,,\n please make sure that T < Tc or P < Pc."));
                }
                secResult.Blocks.Add(paraResult);
                ntbResultP.Visibility = Visibility.Collapsed;
                ntbResultT.Visibility = Visibility.Collapsed;
            }

        }
        /// <summary>
        /// 单选按钮的集合
        /// </summary>
        private List<RadioButton> rbtList
               => new List<RadioButton> { rbtIsobaric, rbtIsothemal };
        /// <summary>
        /// 文本框的集合
        /// </summary>
        private List<NumberTextBox> ntbList
        {
            get
            {
                if (rbtIsothemal.IsChecked == true) {
                    return new List<NumberTextBox> { ntbTc, ntbPc, ntbW, ntbT };
                } else if (rbtIsobaric.IsChecked == true) {
                    return new List<NumberTextBox> { ntbTc, ntbPc, ntbW, ntbP };
                } else {
                    return new List<NumberTextBox> { ntbTc, ntbPc, ntbW };
                }
            }
        }
        /// <summary>
        /// hcb的集合
        /// </summary>
        private List<HeaderComboBox> hcbList
            => new List<HeaderComboBox> { hcbFugacity };
        /// <summary>
        /// 输入检查
        /// </summary>
        /// <returns>true：输入合法可以继续；false ：输入非法</returns>
        private bool IsInputLegal() {
            bool rbt = DataChecker.CheckRadioSelection(rbtList);
            if (!rbt) {
                ShowMsg.RadioSelectionError(this);
                return false;
            } else {
                HeaderComboBox hcb = DataChecker.CheckComboSelection(hcbList);
                if (hcb != null) {
                    hcb.IsDropDownOpen = true;
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
        }
        #region 在 rbtObjP 或 rbtObjT 选定时不同的 txt 可见性调整
        private void RadioButtonChecked(object sender, RoutedEventArgs e) {
            Controls.NumberTextBox currentNtb;
            if (ntbP.Visibility == Visibility.Visible) {
                currentNtb = ntbP;
            } else if (ntbT.Visibility == Visibility.Visible) {
                currentNtb = ntbT;
            } else {
                currentNtb = null;
            }
            conditionNtbAnimation(currentNtb);
        }
        private void conditionNtbAnimation(Controls.NumberTextBox currentNtb) {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                Storyboard sb = new Storyboard();
                if (currentNtb != null) {
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightDownAnimation;
                    da.Completed += da_Completed;
                    sb.Duration = View.ControlAnimations.GeneralAniDuation;
                    sb.Children.Add(da);
                    if (rbtIsobaric.IsChecked == true) {
                        Storyboard.SetTarget(da, ntbT);
                        Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                        sb.Begin();
                    } else if (rbtIsothemal.IsChecked == true) {
                        Storyboard.SetTarget(da, ntbP);
                        Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                        sb.Begin();
                    }
                } else {
                    Controls.NumberTextBox toVisibleNtb;
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
                    sb.Duration = View.ControlAnimations.GeneralAniDuation;
                    sb.Children.Add(da);
                    if (rbtIsobaric.IsChecked == true) {
                        toVisibleNtb = ntbP;
                    } else if (rbtIsothemal.IsChecked == true) {
                        toVisibleNtb = ntbT;
                    } else {
                        return;
                    }
                    toVisibleNtb.Visibility = Visibility.Visible;
                    Storyboard.SetTarget(da, toVisibleNtb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                }
            });
        }
        void da_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= da_Completed;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                ntbT.Visibility = Visibility.Collapsed;
                ntbP.Visibility = Visibility.Collapsed;
                Storyboard sb = new Storyboard();
                Controls.NumberTextBox toVisibleNtb;
                DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
                sb.Duration = View.ControlAnimations.GeneralAniDuation;
                sb.Children.Add(da);
                if (rbtIsobaric.IsChecked == true) {
                    toVisibleNtb = ntbP;
                } else if (rbtIsothemal.IsChecked == true) {
                    toVisibleNtb = ntbT;
                } else {
                    return;
                }
                toVisibleNtb.Visibility = Visibility.Visible;
                Storyboard.SetTarget(da, toVisibleNtb);
                Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                sb.Begin();
            });
        }
        #endregion

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
