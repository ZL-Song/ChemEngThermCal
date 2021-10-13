using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Documents;
using ChemEngThermCal.Model.Material;
using ChemEngThermCal.Model.Vle.FugCoe;
using ChemEngThermCal.Model.Vle.ActCoe;
using ChemEngThermCal.Model.Vle.ActCoe.Models;
using System.Threading.Tasks;

namespace ChemEngThermCal.View {
    /// <summary>
    /// LowPressureMixtureVleView.xaml 的交互逻辑
    /// </summary>
    public partial class MfVleView : Page {
        /// <summary>
        /// 1-真实系，2 半理想系，3-理想系
        /// </summary>
        private readonly int _inilType;
        /// <summary>
        /// 二元混合物系 VLE
        /// </summary>
        /// <param name="inilType">1 - Realistic,2 - SemiIdeal,3 - Ideal</param>
        public MfVleView(int inilType) {
            InitializeComponent();
            if (inilType == 1) {
                hcbActivity.Visibility = Visibility.Visible;
                hcbFugacity.Visibility = Visibility.Visible;
            } else if (inilType == 2) {
                hcbActivity.Visibility = Visibility.Visible;
                hcbFugacity.Visibility = Visibility.Collapsed;
            } else {
                hcbActivity.Visibility = Visibility.Collapsed;
                hcbFugacity.Visibility = Visibility.Collapsed;
            }
            _inilType = inilType;
            grdResult.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 和 = 1
        /// </summary>
        private void ntbChemicalMoleFrac_LostFocus(object sender, RoutedEventArgs e) {
            try {
                if (sender as Controls.NumberTextBox == ntbChemical_1_x) {
                    double x_1 = Convert.ToDouble(ntbChemical_1_x.Text);
                    if (x_1 >= 0.0 && x_1 <= 1.0) {
                        ntbChemical_2_x.Text = Convert.ToString(1 - x_1);
                    } else {
                        ntbChemical_1_x.Text = "Input Error";
                    }
                } else if (sender as Controls.NumberTextBox == ntbChemical_2_x) {
                    double x_2 = Convert.ToDouble(ntbChemical_2_x.Text);
                    if (x_2 >= 0.0 && x_2 <= 1.0) {
                        ntbChemical_1_x.Text = Convert.ToString(1 - x_2);
                    } else {
                        ntbChemical_2_x.Text = "Input Error";
                    }
                } else if (sender as Controls.NumberTextBox == ntbChemical_1_y) {
                    double y_1 = Convert.ToDouble(ntbChemical_1_y.Text);
                    if (y_1 >= 0.0 && y_1 <= 1.0) {
                        ntbChemical_2_y.Text = Convert.ToString(1 - y_1);
                    } else {
                        ntbChemical_1_y.Text = "Input Error";
                    }
                } else if (sender as Controls.NumberTextBox == ntbChemical_2_y) {
                    double y_2 = Convert.ToDouble(ntbChemical_2_y.Text);
                    if (y_2 >= 0.0 && y_2 <= 1.0) {
                        ntbChemical_1_y.Text = Convert.ToString(1 - y_2);
                    } else {
                        ntbChemical_2_y.Text = "Input Error";
                    }
                }
            } catch (Exception) {
                if (sender as Controls.NumberTextBox == ntbChemical_1_x) {
                    ntbChemical_1_x.Text = "Input Error";
                } else if (sender as Controls.NumberTextBox == ntbChemical_2_x) {
                    ntbChemical_2_x.Text = "Input Error";
                } else if (sender as Controls.NumberTextBox == ntbChemical_1_y) {
                    ntbChemical_1_y.Text = "Input Error";
                } else if (sender as Controls.NumberTextBox == ntbChemical_2_y) {
                    ntbChemical_2_y.Text = "Input Error";
                }
            }
        }
        /// <summary>
        /// 进行计算
        /// </summary>
        private void btnCal_Click(object sender, RoutedEventArgs e) {
            //#region 测试用
            //NewMethod();
            //#endregion
            grdResult.Visibility = Visibility.Collapsed;
            (sender as Button).IsEnabled = false;
            if (IsInputLegal()) {
                if (_inilType == 1 || _inilType == 2 || _inilType == 3) {
                    MixVleBaseMediator.CalType type;
                    double independentVar;
                    if (rbtIsobaricBubble.IsChecked == true) {
                        type = MixVleBaseMediator.CalType.IsobaricBubblePt;
                        independentVar = Convert.ToDouble(ntbP.Text);
                    } else if (rbtIsobaricDew.IsChecked == true) {
                        type = MixVleBaseMediator.CalType.IsobaricDewPt;
                        independentVar = Convert.ToDouble(ntbP.Text);
                    } else if (rbtIsothermalBubble.IsChecked == true) {
                        type = MixVleBaseMediator.CalType.IsothemalBubblePt;
                        independentVar = Convert.ToDouble(ntbT.Text);
                    } else {
                        type = MixVleBaseMediator.CalType.IsothemalDewPt;
                        independentVar = Convert.ToDouble(ntbT.Text);
                    }
                    Chemical fst = new Chemical(
                        Convert.ToDouble(ntbChemical_1_Tc.Text),
                        Convert.ToDouble(ntbChemical_1_Pc.Text),
                        Convert.ToDouble(ntbChemical_1_w.Text)
                        );
                    fst.SetAntoinePara(Convert.ToDouble(ntbChemical_1_A.Text), Convert.ToDouble(ntbChemical_1_B.Text), Convert.ToDouble(ntbChemical_1_C.Text));
                    Chemical sec = new Chemical(
                        Convert.ToDouble(ntbChemical_2_Tc.Text),
                        Convert.ToDouble(ntbChemical_2_Pc.Text),
                        Convert.ToDouble(ntbChemical_2_w.Text)
                        );
                    sec.SetAntoinePara(Convert.ToDouble(ntbChemical_2_A.Text), Convert.ToDouble(ntbChemical_2_B.Text), Convert.ToDouble(ntbChemical_2_C.Text));
                    BinaryComplex binComplex;
                    if (rbtIsobaricBubble.IsChecked == true || rbtIsothermalBubble.IsChecked == true) {
                        double x1 = Convert.ToDouble(ntbChemical_1_x.Text);
                        double x2 = Convert.ToDouble(ntbChemical_2_x.Text);
                        binComplex = new BinaryComplex(fst, sec, x1 / x2);
                    } else if (rbtIsobaricDew.IsChecked == true || rbtIsothermalDew.IsChecked == true) {
                        double y1 = Convert.ToDouble(ntbChemical_1_y.Text);
                        double y2 = Convert.ToDouble(ntbChemical_2_y.Text);
                        binComplex = new BinaryComplex(fst, sec, y1 / y2);
                    } else {
                        binComplex = new BinaryComplex(fst, sec, 1);
                    }
                    MixVleBaseMediator med;
                    if (_inilType == 1 || _inilType == 2) {
                        ActivityBaseModel actModel;
                        if (hcbActivity.SelectedIndex == 0) {
                            actModel = new MargulesModel(Convert.ToDouble(ntbChemical_1_Margules.Text), Convert.ToDouble(ntbChemical_2_Margules.Text));
                        } else if (hcbActivity.SelectedIndex == 1) {
                            actModel = new VanLaarModel(Convert.ToDouble(ntbChemical_1_Vanlaar.Text), Convert.ToDouble(ntbChemical_2_Vanlaar.Text));
                        } else if (hcbActivity.SelectedIndex == 2) {
                            actModel = new WilsonModel(Convert.ToDouble(ntbChemical_1_WilsonModel.Text), Convert.ToDouble(ntbChemical_2_WilsonModel.Text));
                        } else if (hcbActivity.SelectedIndex == 3) {
                            actModel = new WilsonEnergyParaModel(Convert.ToDouble(ntbChemical_1_WilsonEnergy.Text), Convert.ToDouble(ntbChemical_2_WilsonEnergy.Text));
                        } else if (hcbActivity.SelectedIndex == 4) {
                            actModel = new NrtlModel(Convert.ToDouble(ntbChemical_1_Ntrl.Text), Convert.ToDouble(ntbChemical_2_Ntrl.Text), Convert.ToDouble(ntbAlpha_Ntrl.Text));
                        } else {
                            actModel = new MargulesModel(1000, 10000);
                        }
                        ActivityMediator actMed = new ActivityMediator(actModel);
                        if (_inilType == 1) {//真实系
                            IGasApplibleFugAdapter fugAda;
                            if (hcbFugacity.SelectedIndex == 0) {
                                fugAda = new CeosFugAdapter(new Model.Ceos.Models.RedlichKwongModel());
                            } else if (hcbFugacity.SelectedIndex == 1) {
                                fugAda = new CeosFugAdapter(new Model.Ceos.Models.SoaveRedlichKwongModel());
                            } else if (hcbFugacity.SelectedIndex == 2) {
                                fugAda = new CeosFugAdapter(new Model.Ceos.Models.PengRobinsonModel());
                            } else if (hcbFugacity.SelectedIndex == 3) {
                                fugAda = new SecVirialFugacityyAdapter(new Model.CorStt.Models.SecVirial.ClassicPureModel());
                            } else {
                                fugAda = new CeosFugAdapter(new Model.Ceos.Models.PengRobinsonModel());
                            }
                            GasFugacityMediator fugMed = new GasFugacityMediator(fugAda);
                            med = new MixActRealisticVleMediator(type, fugMed, actMed,
                               new Model.Antoine.AntoineMediator(new Model.Antoine.MegaPascalKevinAntoineModel()), binComplex, independentVar);
                        } else {  //半理想系
                            med = new MixActSemiIdealMediator(type, actMed,
                                new Model.Antoine.AntoineMediator(new Model.Antoine.MegaPascalKevinAntoineModel()), binComplex, independentVar);
                        }
                    } else { //理想系
                        med = new MixActIdealMediator(type,
                            new Model.Antoine.AntoineMediator(new Model.Antoine.MegaPascalKevinAntoineModel()), binComplex, independentVar);
                    }
                    OutPutResult(DoCal(med));
                }
            }
            (sender as Button).IsEnabled = true;
        }
        //用于测试
        //private void NewMethod() {
        //    if (rbtIsobaricBubble.IsChecked == true) {
        //        //苯
        //        ntbChemical_1_A.Text = "20.7936";
        //        ntbChemical_1_B.Text = "2788.51";
        //        ntbChemical_1_C.Text = "-52.36";
        //        ntbChemical_1_x.Text = "0.665";
        //        ntbChemical_1_Tc.Text = "562.1";
        //        ntbChemical_1_Pc.Text = "4.894";
        //        ntbChemical_1_w.Text = "0.212";
        //        //环己烷
        //        ntbChemical_2_A.Text = "20.6455";
        //        ntbChemical_2_B.Text = "2766.63";
        //        ntbChemical_2_C.Text = "-50.50";
        //        ntbChemical_2_x.Text = "0.335";
        //        ntbChemical_2_Tc.Text = "553.4";
        //        ntbChemical_2_Pc.Text = "4.073";
        //        ntbChemical_2_w.Text = "0.213";
        //        //苯&环己烷
        //        ntbChemical_1_Margules.Text = "0.312251";
        //        ntbChemical_1_Vanlaar.Text = "0.315559";
        //        ntbChemical_2_Margules.Text = "0.398646";
        //        ntbChemical_2_Vanlaar.Text = "0.402295";

        //        ntbP.Text = "0.101325";
        //    } else if (rbtIsothermalBubble.IsChecked == true) {
        //        //反算
        //        //苯
        //        ntbChemical_1_A.Text = "20.7936";
        //        ntbChemical_1_B.Text = "2788.51";
        //        ntbChemical_1_C.Text = "-52.36";
        //        ntbChemical_1_x.Text = "0.665";
        //        ntbChemical_1_Tc.Text = "562.1";
        //        ntbChemical_1_Pc.Text = "4.894";
        //        ntbChemical_1_w.Text = "0.212";
        //        //环己烷
        //        ntbChemical_2_A.Text = "20.6455";
        //        ntbChemical_2_B.Text = "2766.63";
        //        ntbChemical_2_C.Text = "-50.50";
        //        ntbChemical_2_x.Text = "0.335";
        //        ntbChemical_2_Tc.Text = "553.4";
        //        ntbChemical_2_Pc.Text = "4.073";
        //        ntbChemical_2_w.Text = "0.213";
        //        //苯&环己烷
        //        ntbChemical_1_Margules.Text = "0.312251";
        //        ntbChemical_1_Vanlaar.Text = "0.315559";
        //        ntbChemical_2_Margules.Text = "0.398646";
        //        ntbChemical_2_Vanlaar.Text = "0.402295";

        //        ntbT.Text = "350";
        //    } else if (rbtIsobaricDew.IsChecked == true) {

        //    }
        //}
        /// <summary>
        /// 显示结果
        /// </summary> 
        private void OutPutResult(MixVleBaseMediator.Result result) {
            runMF1.Text = result.TargetComplex.FstComponentMoleFraction.ToString();
            runTc1.Text = result.TargetComplex.FstComponent.CriticalTemperature.ToString();
            runPc1.Text = result.TargetComplex.FstComponent.CriticalPressure.ToString();
            runW1.Text = result.TargetComplex.FstComponent.AcentricFactor.ToString();
            runMF2.Text = result.TargetComplex.SecComponentMoleFraction.ToString();
            runTc2.Text = result.TargetComplex.SecComponent.CriticalTemperature.ToString();
            runPc2.Text = result.TargetComplex.SecComponent.CriticalPressure.ToString();
            runW2.Text = result.TargetComplex.SecComponent.AcentricFactor.ToString();

            switch (result.CalculateType) {
                case MixVleBaseMediator.CalType.IsobaricBubblePt:
                    runMF.Text = "x";
                    runIV.Text = $"P = {result.Pressure.ToString()} MPa";
                    break;
                case MixVleBaseMediator.CalType.IsobaricDewPt:
                    runMF.Text = "y";
                    runIV.Text = $"P = {result.Pressure.ToString()} MPa";
                    break;
                case MixVleBaseMediator.CalType.IsothemalBubblePt:
                    runMF.Text = "x";
                    runIV.Text = $"T = {result.Temperature.ToString()} K";
                    break;
                case MixVleBaseMediator.CalType.IsothemalDewPt:
                    runMF.Text = "y";
                    runIV.Text = $"T = {result.Temperature.ToString()} K";
                    break;
                default:
                    runIV.Text = "Unknown";
                    break;
            }

            runActModel.Text = ShowAct(_inilType);
            runFugModel.Text = ShowAct(_inilType);

            secItrProc.Blocks.Clear();

            MixVleBaseMediator.Step[] fstList = result.FstStepList.ToArray();
            MixVleBaseMediator.Step[] secList = result.SecStepList.ToArray();
            for (int i = 0; i < fstList.Length; i++) {
                Paragraph paraStep = new Paragraph();
                Run runMid = new Run($"Component - 1 : \nP(s) = {fstList[i].SaturatedPressure} MPa\nφ(s) = {fstList[i].SaturatedFugacity}\nγ = {fstList[i].Activity}\nφ = {fstList[i].Fugacity}\n\nComponent - 2 : \nP(s) = {secList[i].SaturatedPressure} MPa\nφ(s) = {secList[i].SaturatedFugacity}\nγ = {secList[i].Activity}\nφ = {secList[i].Fugacity}\n\n");
                Run runInil, runLast;
                switch (result.CalculateType) {
                    case MixVleBaseMediator.CalType.IsobaricBubblePt:
                        runInil = new Run($"Iteration step {i} :\n\n Assumed temperature = {fstList[i].Temperature} K\n\n");
                        runLast = new Run($"Mole Fractions :\n\ny(1) = {fstList[i].MoleFraction}\ny(2) = {secList[i].MoleFraction}\n\nTemperature = {fstList[i].Temperature} K");
                        break;
                    case MixVleBaseMediator.CalType.IsobaricDewPt:
                        runInil = new Run($"Iteration step {i} :\n\n Assumed temperature = {fstList[i].Temperature} K\n\n");
                        runLast = new Run($"Mole Fractions :\n\nx(1) = {fstList[i].MoleFraction}\nx(2) = {secList[i].MoleFraction}\n\nTemperature = {fstList[i].Temperature} K");
                        break;
                    case MixVleBaseMediator.CalType.IsothemalBubblePt:
                        runInil = new Run($"Iteration step {i} :\n\n Assumed pressure = {fstList[i].Pressure} MPa\n\n");
                        runLast = new Run($"Mole Fractions :\n\ny(1) = {fstList[i].MoleFraction}\ny(2) = {secList[i].MoleFraction}\n\nPressure = {fstList[i].Pressure} MPa");
                        break;
                    case MixVleBaseMediator.CalType.IsothemalDewPt:
                        runInil = new Run($"Iteration step {i} :\n\n Assumed pressure = {fstList[i].Pressure} MPa\n\n");
                        runLast = new Run($"Mole Fractions :\n\nx(1) = {fstList[i].MoleFraction}\nx(2) = {secList[i].MoleFraction}\n\nPressure = {fstList[i].Pressure} MPa");
                        break;
                    default:
                        runInil = new Run();
                        runMid = new Run();
                        runLast = new Run();
                        break;
                }
                paraStep.Inlines.Add(runInil);
                paraStep.Inlines.Add(runMid);
                paraStep.Inlines.Add(runLast);
                secItrProc.Blocks.Add(paraStep);
            }

            secResult.Blocks.Clear();

            if (result.IsAborted == true) {
                secResult.Blocks.Add(new Paragraph(new Run("The calculate is cancelled.")));
            } else if (result.IsConverged == false) {
                secResult.Blocks.Add(new Paragraph(new Run("The iteration diverged.")));
            } else {
                Run runC1, runC2, runResult;
                Run runC1Next = new Run($"Saturated Pressure = {result.FstLastStep.SaturatedPressure} MPa\nSaturated Fugacity = {result.FstLastStep.SaturatedFugacity}\nFugacity = {result.FstLastStep.Fugacity}\nActivity = {result.FstLastStep.Activity}\n");
                Run runC2Next = new Run($"Saturated Pressure = {result.SecLastStep.SaturatedPressure} MPa\nSaturated Fugacity = {result.SecLastStep.SaturatedFugacity}\nFugacity = {result.SecLastStep.Fugacity}\nActivity = {result.SecLastStep.Activity}\n");
                switch (result.CalculateType) {
                    case MixVleBaseMediator.CalType.IsobaricBubblePt:
                        runC1 = new Run($"Component - 1:\nMole Fraction in gas phase = {result.FstLastStep.MoleFraction}\n");
                        runC2 = new Run($"\nComponent - 2:\nMole Fraction in gas phase = {result.SecLastStep.MoleFraction}\n");
                        runResult = new Run($"\nTemperature = {result.Temperature} K");
                        ntbResultTemperature.Text = result.Temperature.ToString();
                        ntbResultGasMF1.Text = result.FstLastStep.MoleFraction.ToString();
                        ntbResultGasMF2.Text = result.SecLastStep.MoleFraction.ToString();
                        break;
                    case MixVleBaseMediator.CalType.IsobaricDewPt:
                        runC1 = new Run($"Component - 1:\nMole Fraction in liquid phase = {result.FstLastStep.MoleFraction}\n");
                        runC2 = new Run($"\nComponent - 2:\nMole Fraction in liquid phase = {result.SecLastStep.MoleFraction}\n");
                        runResult = new Run($"\nTemperature = {result.Temperature} K");
                        ntbResultTemperature.Text = result.Temperature.ToString();
                        ntbResultLiqMF1.Text = result.FstLastStep.MoleFraction.ToString();
                        ntbResultLiqMF2.Text = result.SecLastStep.MoleFraction.ToString();
                        break;
                    case MixVleBaseMediator.CalType.IsothemalBubblePt:
                        runC1 = new Run($"Component - 1:\nMole Fraction in gas phase = {result.FstLastStep.MoleFraction}\n");
                        runC2 = new Run($"\nComponent - 2:\nMole Fraction in gas phase = {result.SecLastStep.MoleFraction}\n");
                        runResult = new Run($"\nPressure = {result.Pressure} MPa");
                        ntbResultPressure.Text = result.Pressure.ToString();
                        ntbResultGasMF1.Text = result.FstLastStep.MoleFraction.ToString();
                        ntbResultGasMF2.Text = result.SecLastStep.MoleFraction.ToString();
                        break;
                    case MixVleBaseMediator.CalType.IsothemalDewPt:
                        runC1 = new Run($"Component - 1:\nMole Fraction in liquid phase = {result.FstLastStep.MoleFraction}\n");
                        runC2 = new Run($"\nComponent - 2:\nMole Fraction in liquid phase = {result.SecLastStep.MoleFraction}\n");
                        runResult = new Run($"\nPressure = {result.Pressure} MPa");
                        ntbResultPressure.Text = result.Pressure.ToString();
                        ntbResultLiqMF1.Text = result.FstLastStep.MoleFraction.ToString();
                        ntbResultLiqMF2.Text = result.SecLastStep.MoleFraction.ToString();
                        break;
                    default:
                        runC1 = new Run($"Component - 1:\nMole Fraction in gas phase = {result.FstLastStep.MoleFraction}\n");
                        runC2 = new Run($"\nComponent - 2:\nMole Fraction in gas phase = {result.SecLastStep.MoleFraction}\n");
                        runResult = new Run($"\nTemperature = {result.Temperature} K");
                        ntbResultTemperature.Text = result.Temperature.ToString();
                        ntbResultGasMF1.Text = result.FstLastStep.MoleFraction.ToString();
                        ntbResultGasMF2.Text = result.SecLastStep.MoleFraction.ToString();
                        break;
                }
                Paragraph paraResult = new Paragraph();
                paraResult.Inlines.Add(runC1);
                paraResult.Inlines.Add(runC1Next);
                paraResult.Inlines.Add(runC2);
                paraResult.Inlines.Add(runC2Next);
                paraResult.Inlines.Add(runResult);
                secResult.Blocks.Add(paraResult);
            }
            ShowResult();
        }
        /// <summary>
        /// 逸度组合框动画
        /// </summary>
        private string ShowFug(int inilType) {
            string fug;
            if (inilType == 1) {
                if (hcbFugacity.SelectedIndex == 0) {
                    fug = "Redlich - Kwong";
                } else if (hcbFugacity.SelectedIndex == 1) {
                    fug = "Soave - Redlich - Kwong";
                } else if (hcbFugacity.SelectedIndex == 2) {
                    fug = "Peng - Robinson";
                } else if (hcbFugacity.SelectedIndex == 3) {
                    fug = "Secondary Virial";
                } else {
                    fug = "Unknown";
                }
            } else {
                fug = "Not needed";
            }
            return fug;
        }
        /// <summary>
        /// 活度组合框动画
        /// </summary>
        private string ShowAct(int inilType) {
            string act;
            if (inilType == 1 || inilType == 2) {
                if (hcbActivity.SelectedIndex == 0) {
                    act = "Margules";
                    runActIndex_1.Text = ntbChemical_1_Margules.Text;
                    runActIndex_2.Text = ntbChemical_2_Margules.Text;
                } else if (hcbActivity.SelectedIndex == 1) {
                    act = "Van Laar";
                    runActIndex_1.Text = ntbChemical_1_Vanlaar.Text;
                    runActIndex_2.Text = ntbChemical_2_Vanlaar.Text;
                } else if (hcbActivity.SelectedIndex == 2) {
                    act = "Wilson";
                } else if (hcbActivity.SelectedIndex == 3) {
                    act = "Wilson";
                } else if (hcbActivity.SelectedIndex == 4) {
                    act = "NRTL";
                } else {
                    act = "Unknown";
                }
            } else {
                act = "Not needed";
            }
            return act;
        }
        /// <summary>
        /// 进行计算 获取结果
        /// </summary> 
        private MixVleBaseMediator.Result DoCal(MixVleBaseMediator calModel)
            => calModel.GetResult();
        /// <summary>
        /// 显示结果
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
        /// 输入验证
        /// </summary>
        private bool IsInputLegal() {
            if (!DataChecker.CheckRadioSelection(rbtList)) {
                ShowMsg.RadioSelectionError(this);
                return false;
            } else {
                ComboBox hcb = DataChecker.CheckComboSelection(hcbList);
                if (hcb != null) {
                    hcb.IsDropDownOpen = true;
                    ShowMsg.ComboSelectionError(this);
                    return false;
                } else {
                    Controls.NumberTextBox ntb = DataChecker.CheckInputNumeric(ntbList);
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
        /// <summary>
        /// rbt List
        /// </summary>
        private List<RadioButton> rbtList
            => new List<RadioButton> { rbtIsobaricBubble, rbtIsobaricDew, rbtIsothermalBubble, rbtIsothermalDew };
        /// <summary>
        /// hcb List
        /// </summary>  
        private List<Controls.HeaderComboBox> hcbList
        {
            get
            {
                if (_inilType == 1) {
                    return new List<Controls.HeaderComboBox> { hcbActivity, hcbFugacity };
                } else if (_inilType == 2) {
                    return new List<Controls.HeaderComboBox> { hcbActivity };
                } else {
                    return null;
                }
            }
        }
        /// <summary>
        /// ntb List
        /// </summary>
        private List<Controls.NumberTextBox> ntbList
        {
            get
            {
                List<Controls.NumberTextBox> ntbGather = new List<Controls.NumberTextBox> { };
                List<Controls.NumberTextBox> ntbAct;
                if (hcbActivity.SelectedIndex == 0) {
                    ntbAct = new List<Controls.NumberTextBox> { ntbChemical_1_Margules, ntbChemical_2_Margules };
                } else if (hcbActivity.SelectedIndex == 1) {
                    ntbAct = new List<Controls.NumberTextBox> { ntbChemical_1_Vanlaar, ntbChemical_2_Vanlaar };
                } else if (hcbActivity.SelectedIndex == 2) {
                    ntbAct = new List<Controls.NumberTextBox> { ntbChemical_1_WilsonModel, ntbChemical_2_WilsonModel };
                } else if (hcbActivity.SelectedIndex == 3) {
                    ntbAct = new List<Controls.NumberTextBox> { ntbChemical_1_WilsonEnergy, ntbChemical_2_WilsonEnergy };
                } else if (hcbActivity.SelectedIndex == 4) {
                    ntbAct = new List<Controls.NumberTextBox> { ntbChemical_1_Ntrl, ntbChemical_2_Ntrl, ntbAlpha_Ntrl };
                } else {
                    ntbAct = new List<Controls.NumberTextBox> { };
                }
                List<Controls.NumberTextBox> ntbCommon = new List<Controls.NumberTextBox> {
                    ntbChemical_1_Tc, ntbChemical_1_Pc, ntbChemical_1_w,
                    ntbChemical_2_Tc, ntbChemical_2_Pc, ntbChemical_2_w,
                    ntbChemical_1_A, ntbChemical_1_B, ntbChemical_1_C,
                    ntbChemical_2_A, ntbChemical_2_B, ntbChemical_2_C
                };
                List<Controls.NumberTextBox> ntbXY;
                if (rbtIsobaricBubble.IsChecked == true) {
                    ntbXY = new List<Controls.NumberTextBox> { ntbChemical_1_x, ntbChemical_2_x };
                } else if (rbtIsobaricDew.IsChecked == true) {
                    ntbXY = new List<Controls.NumberTextBox> { ntbChemical_1_y, ntbChemical_2_y };
                } else if (rbtIsothermalBubble.IsChecked == true) {
                    ntbXY = new List<Controls.NumberTextBox> { ntbChemical_1_x, ntbChemical_2_x };
                } else if (rbtIsothermalDew.IsChecked == true) {
                    ntbXY = new List<Controls.NumberTextBox> { ntbChemical_1_y, ntbChemical_2_y };
                } else {
                    ntbXY = new List<Controls.NumberTextBox> { };
                }
                List<Controls.NumberTextBox> ntbCondition;
                if (rbtIsobaricBubble.IsChecked == true) {
                    ntbCondition = new List<Controls.NumberTextBox> { ntbP };
                } else if (rbtIsobaricDew.IsChecked == true) {
                    ntbCondition = new List<Controls.NumberTextBox> { ntbP };
                } else if (rbtIsothermalBubble.IsChecked == true) {
                    ntbCondition = new List<Controls.NumberTextBox> { ntbT };
                } else if (rbtIsothermalDew.IsChecked == true) {
                    ntbCondition = new List<Controls.NumberTextBox> { ntbT };
                } else {
                    ntbCondition = new List<Controls.NumberTextBox> { };
                }
                ntbGather.AddRange(ntbAct);
                ntbGather.AddRange(ntbXY);
                ntbGather.AddRange(ntbCommon);
                ntbGather.AddRange(ntbCondition);
                return ntbGather;
            }
        }
        #region 在 hcbActivity 选定项时不同的 txt 动画调整，多个属性绑定
        private void hcbActivity_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (hcbActivity.SelectedIndex == -1) {
                return;
            }
            Controls.NumberTextBox currentNtb;
            if (ntbChemical_1_Vanlaar.Visibility == Visibility.Visible) {
                currentNtb = ntbChemical_1_Vanlaar;
            } else if (ntbChemical_1_Margules.Visibility == Visibility.Visible) {
                currentNtb = ntbChemical_1_Margules;
            } else if (ntbChemical_1_WilsonEnergy.Visibility == Visibility.Visible) {
                currentNtb = ntbChemical_1_WilsonEnergy;
            } else if (ntbChemical_1_WilsonModel.Visibility == Visibility.Visible) {
                currentNtb = ntbChemical_1_WilsonModel;
            } else if (ntbChemical_1_Ntrl.Visibility == Visibility.Visible) {
                currentNtb = ntbChemical_1_Ntrl;
            } else {
                currentNtb = null;
            }
            ntbActivityAnimation(currentNtb);
        }
        private void ntbActivityAnimation(Controls.NumberTextBox currentNtb) {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                if (currentNtb != null) {
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightDownAnimation;
                    da.Completed += activityNtbCollapse_Completed;
                    Storyboard sb = new Storyboard();
                    sb.Duration = View.ControlAnimations.GeneralAniDuation;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, currentNtb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                } else {
                    Controls.NumberTextBox toVisibleNtb;
                    switch (hcbActivity.SelectedIndex) {
                        case 0:
                            toVisibleNtb = ntbChemical_1_Margules;
                            break;
                        case 1:
                            toVisibleNtb = ntbChemical_1_Vanlaar;
                            break;
                        case 2:
                            toVisibleNtb = ntbChemical_1_WilsonModel;
                            break;
                        case 3:
                            toVisibleNtb = ntbChemical_1_WilsonEnergy;
                            break;
                        case 4:
                            toVisibleNtb = ntbChemical_1_Ntrl;
                            break;
                        default:
                            return;
                    }
                    toVisibleNtb.Visibility = Visibility.Visible;
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
                    Storyboard sb = new Storyboard();
                    sb.Duration = View.ControlAnimations.GeneralAniDuation;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, toVisibleNtb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                }
            });
        }
        private void activityNtbCollapse_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= activityNtbCollapse_Completed;
            ntbChemical_1_Margules.Visibility = Visibility.Collapsed;
            ntbChemical_1_Vanlaar.Visibility = Visibility.Collapsed;
            ntbChemical_1_WilsonModel.Visibility = Visibility.Collapsed;
            ntbChemical_1_WilsonEnergy.Visibility = Visibility.Collapsed;
            ntbChemical_1_Ntrl.Visibility = Visibility.Collapsed;
            Controls.NumberTextBox toVisibleNtb;
            switch (hcbActivity.SelectedIndex) {
                case 0:
                    toVisibleNtb = ntbChemical_1_Margules;
                    break;
                case 1:
                    toVisibleNtb = ntbChemical_1_Vanlaar;
                    break;
                case 2:
                    toVisibleNtb = ntbChemical_1_WilsonModel;
                    break;
                case 3:
                    toVisibleNtb = ntbChemical_1_WilsonEnergy;
                    break;
                case 4:
                    toVisibleNtb = ntbChemical_1_Ntrl;
                    break;
                default:
                    return;
            }
            toVisibleNtb.Visibility = Visibility.Visible;
            DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
            Storyboard sb = new Storyboard();
            sb.Duration = View.ControlAnimations.GeneralAniDuation;
            sb.Children.Add(da);
            Storyboard.SetTarget(da, toVisibleNtb);
            Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
            sb.Begin();
        }
        #endregion
        #region 计算模式变化时不同的 txt 动画调整，多个属性绑定
        /// <summary>
        /// 选定选项
        /// </summary>
        private void RadioButtonChecked(object sender, RoutedEventArgs e) {
            Controls.NumberTextBox currentFractionNtb;
            if (ntbChemical_1_x.Visibility == Visibility.Visible) {
                currentFractionNtb = ntbChemical_1_x;
            } else if (ntbChemical_1_y.Visibility == Visibility.Visible) {
                currentFractionNtb = ntbChemical_1_y;
            } else {
                currentFractionNtb = null;
            }
            ntbFractionAnimation(currentFractionNtb);
            Controls.NumberTextBox currentConditionNtb;
            if (ntbT.Visibility == Visibility.Visible) {
                currentConditionNtb = ntbT;
            } else if (ntbP.Visibility == Visibility.Visible) {
                currentConditionNtb = ntbP;
            } else {
                currentConditionNtb = null;
            }
            ntbConditionAnimation(currentConditionNtb);
        }
        /// <summary>
        /// mole fraction: x or y
        /// </summary>
        private void ntbFractionAnimation(Controls.NumberTextBox currentFractionNtb) {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                if (currentFractionNtb != null) {
                    if (((rbtIsobaricBubble.IsChecked == true || rbtIsothermalBubble.IsChecked == true) && (ntbChemical_1_x == currentFractionNtb)) ||
                        ((rbtIsobaricDew.IsChecked == true || rbtIsothermalDew.IsChecked == true) && (ntbChemical_1_y == currentFractionNtb))) {
                        return;
                    }
                    Storyboard sb = new Storyboard();
                    sb.Duration = View.ControlAnimations.GeneralAniDuation;
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightDownAnimation;
                    da.Completed += fractionNtbCollapse_Completed;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, currentFractionNtb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                } else {
                    Controls.NumberTextBox toVisibleNtb;
                    if (rbtIsobaricBubble.IsChecked == true || rbtIsothermalBubble.IsChecked == true) {
                        toVisibleNtb = ntbChemical_1_x;
                    } else if (rbtIsobaricDew.IsChecked == true || rbtIsothermalDew.IsChecked == true) {
                        toVisibleNtb = ntbChemical_1_y;
                    } else {
                        return;
                    }
                    ntbToVisibleAnimation(toVisibleNtb);
                }
            });
        }
        private static void ntbToVisibleAnimation(Controls.NumberTextBox toVisibleNtb) {
            Storyboard sb = new Storyboard();
            DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
            toVisibleNtb.Visibility = Visibility.Visible;
            sb.Duration = View.ControlAnimations.GeneralAniDuation;
            sb.Children.Add(da);
            Storyboard.SetTarget(da, toVisibleNtb);
            Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
            sb.Begin();
        }
        private void fractionNtbCollapse_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= fractionNtbCollapse_Completed;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                ntbChemical_1_x.Visibility = Visibility.Collapsed;
                ntbChemical_1_y.Visibility = Visibility.Collapsed;
                Controls.NumberTextBox toVisibleNtb;
                if (rbtIsobaricBubble.IsChecked == true || rbtIsothermalBubble.IsChecked == true) {
                    toVisibleNtb = ntbChemical_1_x;
                } else if (rbtIsobaricDew.IsChecked == true || rbtIsothermalDew.IsChecked == true) {
                    toVisibleNtb = ntbChemical_1_y;
                } else {
                    return;
                }
                ntbToVisibleAnimation(toVisibleNtb);
            });
        }
        /// <summary>
        /// condition: T or P
        /// </summary>
        private void ntbConditionAnimation(Controls.NumberTextBox currentConditionNtb) {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                Storyboard sb = new Storyboard();
                if (currentConditionNtb != null) {
                    if (((rbtIsobaricBubble.IsChecked == true || rbtIsobaricDew.IsChecked == true) && (ntbP == currentConditionNtb)) ||
                        ((rbtIsothermalBubble.IsChecked == true || rbtIsothermalDew.IsChecked == true) && (ntbT == currentConditionNtb))) {
                        return;
                    }
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightDownAnimation;
                    da.Completed += conditionNtbCollapse_Completed;
                    sb.Duration = View.ControlAnimations.GeneralAniDuation;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, currentConditionNtb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                } else {
                    Controls.NumberTextBox toVisibleNtb;
                    if (rbtIsobaricBubble.IsChecked == true || rbtIsobaricDew.IsChecked == true) {
                        toVisibleNtb = ntbP;
                    } else if (rbtIsothermalBubble.IsChecked == true || rbtIsothermalDew.IsChecked == true) {
                        toVisibleNtb = ntbT;
                    } else {
                        return;
                    }
                    toVisibleNtb.Visibility = Visibility.Visible;
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
                    sb.Duration = View.ControlAnimations.GeneralAniDuation;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, toVisibleNtb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                }
            });
        }
        private void conditionNtbCollapse_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= conditionNtbCollapse_Completed;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                ntbP.Visibility = Visibility.Collapsed;
                ntbT.Visibility = Visibility.Collapsed;
                Storyboard sb = new Storyboard();
                Controls.NumberTextBox toVisibleNtb;
                if (rbtIsobaricBubble.IsChecked == true || rbtIsobaricDew.IsChecked == true) {
                    toVisibleNtb = ntbP;
                } else if (rbtIsothermalBubble.IsChecked == true || rbtIsothermalDew.IsChecked == true) {
                    toVisibleNtb = ntbT;
                } else {
                    return;
                }
                toVisibleNtb.Visibility = Visibility.Visible;
                DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
                sb.Duration = View.ControlAnimations.GeneralAniDuation;
                sb.Children.Add(da);
                Storyboard.SetTarget(da, toVisibleNtb);
                Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                sb.Begin();
            });
        }
        #endregion
        /// <summary>
        /// 后退
        /// </summary> 
        private void btnBack_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new MfVleStartView());
        }
        #region 页面加载后续回退按钮动画
        /// <summary>
        /// 页面加载后续回退按钮动画
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            btnBack.IsHitTestVisible = false;
            if (App.isDbExist == true) {
                btnDbCon_1.Visibility = Visibility.Visible;
                btnDbCon_2.Visibility = Visibility.Visible;
            } else {
                btnDbCon_1.Visibility = Visibility.Collapsed;
                btnDbCon_2.Visibility = Visibility.Collapsed;
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
        private void DbBtnCon_Click(object sender, RoutedEventArgs e) {
            DbConView db = new DbConView();
            db.Owner = Window.GetWindow(this);
            db.ShowDialog();
            if (db.IsChemGot == true) {
                if (sender == btnDbCon_1) {
                    ntbChemical_1_Tc.Text = db.Chem.Tc.ToString();
                    ntbChemical_1_Pc.Text = db.Chem.Pc.ToString();
                    ntbChemical_1_w.Text = db.Chem.W.ToString(); 
                } else if (sender == btnDbCon_2) {
                    ntbChemical_2_Tc.Text = db.Chem.Tc.ToString();
                    ntbChemical_2_Pc.Text = db.Chem.Pc.ToString();
                    ntbChemical_2_w.Text = db.Chem.W.ToString(); 
                }
            } else {
                return;
            }
        }
    }
}
