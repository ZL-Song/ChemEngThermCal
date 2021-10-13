using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using ChemEngThermCal.Controls;
using System.Collections.Generic;

namespace ChemEngThermCal.View {
    /// <summary>
    /// MfGasMixView.xaml 的交互逻辑
    /// </summary>
    public partial class MfGasMixView : Page {
        public MfGasMixView() {
            InitializeComponent();
            grdResult.Visibility = Visibility.Collapsed;
        }
        #region 不同 混合模型 时的动画切换效果，所涉及的控件之间存在多个属性绑定
        /// <summary>
        /// HcbCeos & HcbGce Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HcbSelectionChanged(object sender, SelectionChangedEventArgs e) {
            // 如果空选项后为空，则不执行
            if ((sender as HeaderComboBox).SelectedIndex == -1) {
                return;
            }
            if ((sender == hcbCeosType && rbtCeos.IsChecked != true) || (sender == hcbGceMethod && rbtKay.IsChecked != true)) {
                return;
            }
            NumberTextBox currentWNtb;
            if (ntbChemical_1_w.Visibility == Visibility.Visible) {
                currentWNtb = ntbChemical_1_w;
            } else {
                currentWNtb = null;
            }
            NtbAcentricAnimation(currentWNtb);
            NumberTextBox currentVcNtb;
            if (ntbChemical_1_Vc.Visibility == Visibility.Visible) {
                currentVcNtb = ntbChemical_1_Vc;
            } else {
                currentVcNtb = null;
            }
            NtbCriticalMoleVolumeAnimation(currentVcNtb);
        }
        /// <summary>
        /// RadioButton Check
        /// </summary>
        private void RadioButtonChecked(object sender, RoutedEventArgs e) {
            HeaderComboBox currentHcb;
            if (hcbGceMethod.Visibility == Visibility.Visible) {
                currentHcb = hcbGceMethod;
            } else if (hcbCeosType.Visibility == Visibility.Visible) {
                currentHcb = hcbCeosType;
            } else {
                currentHcb = null;
            }
            HcbAnimation(currentHcb);
        }
        private void NtbAcentricAnimation(NumberTextBox currentWNtb) {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                Storyboard sb = new Storyboard();
                sb.Duration = View.ControlAnimations.GeneralAniDuation;
                if (currentWNtb != null) {
                    if (rbtSecVirial.IsChecked == true
                    || (rbtCeos.IsChecked == true && (hcbCeosType.SelectedIndex == 2 || hcbCeosType.SelectedIndex == 3 || hcbCeosType.SelectedIndex == -1))
                    || rbtKay.IsChecked == true) {
                        return;
                    }
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightDownAnimation;
                    da.Completed += ntbWCollapsed_Completed;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, currentWNtb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                } else {
                    if (rbtCeos.IsChecked == true && (hcbCeosType.SelectedIndex == 0 || hcbCeosType.SelectedIndex == 1)) {
                        return;
                    }
                    ntbChemical_1_w.Visibility = Visibility.Visible;
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, ntbChemical_1_w);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                }
            });
        }
        private void ntbWCollapsed_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= ntbWCollapsed_Completed;
            ntbChemical_1_w.Visibility = Visibility.Collapsed;
        }
        private void NtbCriticalMoleVolumeAnimation(NumberTextBox currentVcNtb) {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                Storyboard sb = new Storyboard();
                sb.Duration = View.ControlAnimations.GeneralAniDuation;
                if (currentVcNtb != null) {
                    if (rbtSecVirial.IsChecked == true) {
                        return;
                    }
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightDownAnimation;
                    da.Completed += ntbVcCollapsed_Completed;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, currentVcNtb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                } else {
                    if (!(rbtSecVirial.IsChecked == true)) {
                        return;
                    }
                    ntbChemical_1_Vc.Visibility = Visibility.Visible;
                    DoubleAnimation da = View.ControlAnimations.TextBoxHeightUpAnimation;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, ntbChemical_1_Vc);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                }
            });
        }
        private void ntbVcCollapsed_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= ntbVcCollapsed_Completed;
            ntbChemical_1_Vc.Visibility = Visibility.Collapsed;
        }
        private void HcbAnimation(HeaderComboBox currentHcb) {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                Storyboard sb = new Storyboard();
                if (currentHcb != null) {
                    if ((rbtKay.IsChecked == true && currentHcb == hcbGceMethod) || (rbtCeos.IsChecked == true && currentHcb == hcbCeosType)) {
                        return;
                    }
                    sb.Duration = View.ControlAnimations.GeneralAniDuation;
                    DoubleAnimation da = View.ControlAnimations.HeaderComboBoxHeightDownAnimation;
                    da.Completed += hcbCollapsed_Completed;
                    sb.Children.Add(da);
                    Storyboard.SetTarget(da, currentHcb);
                    Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                    sb.Begin();
                } else {
                    HeaderComboBox toVisibleHcb;
                    if (rbtKay.IsChecked == true) {
                        toVisibleHcb = hcbGceMethod;
                    } else if (rbtCeos.IsChecked == true) {
                        toVisibleHcb = hcbCeosType;
                    } else {
                        //注意，以下代码（至 return; ）在 View 默认初始化 ntbVc，ntbW 为不可见时，选中 rbtSecVirial 才会触发。目前而言一般不会触发，出于安全性考虑仍然保留
                        NumberTextBox currentWNtb;
                        if (ntbChemical_1_w.Visibility == Visibility.Visible) {
                            currentWNtb = ntbChemical_1_w;
                        } else {
                            currentWNtb = null;
                        }
                        NtbAcentricAnimation(currentWNtb);
                        NumberTextBox currentVcNtb;
                        if (ntbChemical_1_Vc.Visibility == Visibility.Visible) {
                            currentVcNtb = ntbChemical_1_Vc;
                        } else {
                            currentVcNtb = null;
                        }
                        NtbCriticalMoleVolumeAnimation(currentVcNtb);
                        return;
                    }
                    hcbToVisibleAnimation(toVisibleHcb);
                }
            });
        }
        private void hcbCollapsed_Completed(object sender, EventArgs e) {
            (sender as AnimationClock).Completed -= hcbCollapsed_Completed;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate () {
                hcbCeosType.Visibility = Visibility.Collapsed;
                hcbGceMethod.Visibility = Visibility.Collapsed;
                HeaderComboBox toVisibleHcb;
                if (rbtKay.IsChecked == true) {
                    toVisibleHcb = hcbGceMethod;
                } else if (rbtCeos.IsChecked == true) {
                    toVisibleHcb = hcbCeosType;
                } else {
                    NumberTextBox currentWNtb;
                    if (ntbChemical_1_w.Visibility == Visibility.Visible) {
                        currentWNtb = ntbChemical_1_w;
                    } else {
                        currentWNtb = null;
                    }
                    NtbAcentricAnimation(currentWNtb);
                    NumberTextBox currentVcNtb;
                    if (ntbChemical_1_Vc.Visibility == Visibility.Visible) {
                        currentVcNtb = ntbChemical_1_Vc;
                    } else {
                        currentVcNtb = null;
                    }
                    NtbCriticalMoleVolumeAnimation(currentVcNtb);
                    return;
                }
                hcbToVisibleAnimation(toVisibleHcb);
            });
        }
        private void hcbToVisibleAnimation(HeaderComboBox toVisibleHcb) {
            hcbCeosType.SelectedIndex = -1;
            hcbGceMethod.SelectedIndex = -1;
            Storyboard sb = new Storyboard();
            toVisibleHcb.Visibility = Visibility.Visible;
            sb.Duration = View.ControlAnimations.GeneralAniDuation;
            DoubleAnimation da = View.ControlAnimations.HeaderComboBoxHeightUpAnimation;
            sb.Children.Add(da);
            Storyboard.SetTarget(da, toVisibleHcb);
            Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
            sb.Begin();
        }
        #endregion
        /// <summary>
        /// 进行计算
        /// </summary>
        private void btnCal_Click(object sender, RoutedEventArgs e) {
            //#region 注释区域
            //ntbBinIndexes.Text = "0";
            //ntbChemical_1_Pc.Text = "1.297";
            //ntbChemical_1_Tc.Text = "33.2";
            //ntbChemical_1_Vc.Text = "94";
            //ntbChemical_1_w.Text = "-0.22";
            //ntbChemical_1_x.Text = "0.25";
            //ntbChemical_1_Zc.Text = "0.274";

            //ntbChemical_2_Pc.Text = "3.394";
            //ntbChemical_2_Tc.Text = "126.2";
            //ntbChemical_2_Vc.Text = "203";
            //ntbChemical_2_w.Text = "0.040";
            //ntbChemical_2_x.Text = "0.75";
            //ntbChemical_2_Zc.Text = "0.281";

            //ntbP.Text = "40.5";
            //ntbT.Text = "573.15";
            //#endregion
            (sender as Button).IsEnabled = false;
            grdResult.Visibility = Visibility.Collapsed;
            if (IsInputLegal()) {
                ntbResultVV.Text = "";
                ntbResultVZ.Text = "";
                object calProc = new object();
                if (rbtKay.IsChecked == true) {
                    if (hcbGceMethod.SelectedIndex == 0) {
                        calProc = new Model.CorStt.CalMoleVolume.UsingSecVirial.SecVirialKayMixMediator(
                            new Model.CorStt.Models.SecVirial.ClassicPureModel(),
                            new Model.Material.BinaryComplex(
                                new Model.Material.Chemical(
                                    Convert.ToDouble(ntbChemical_1_Tc.Text),
                                    Convert.ToDouble(ntbChemical_1_Pc.Text),
                                    Convert.ToDouble(ntbChemical_1_w.Text)
                                    ),
                                new Model.Material.Chemical(
                                    Convert.ToDouble(ntbChemical_2_Tc.Text),
                                    Convert.ToDouble(ntbChemical_2_Pc.Text),
                                    Convert.ToDouble(ntbChemical_2_w.Text)),
                                Convert.ToDouble(ntbChemical_1_x.Text) / Convert.ToDouble(ntbChemical_2_x.Text)
                                ),
                            Convert.ToDouble(ntbT.Text),
                            Convert.ToDouble(ntbP.Text)
                            );
                    } else {
                        calProc = new Model.CorStt.CalMoleVolume.UsingCprDiagram.CprDiagKayMixMediator(
                            new Model.CorStt.Models.Diagram.CprModel(new CprDiagInterator()),
                            new Model.Material.BinaryComplex(
                                new Model.Material.Chemical(
                                    Convert.ToDouble(ntbChemical_1_Tc.Text),
                                    Convert.ToDouble(ntbChemical_1_Pc.Text),
                                    Convert.ToDouble(ntbChemical_1_w.Text)
                                    ),
                                new Model.Material.Chemical(
                                    Convert.ToDouble(ntbChemical_2_Tc.Text),
                                    Convert.ToDouble(ntbChemical_2_Pc.Text),
                                    Convert.ToDouble(ntbChemical_2_w.Text)),
                                Convert.ToDouble(ntbChemical_1_x.Text) / Convert.ToDouble(ntbChemical_2_x.Text)
                                ),
                            Convert.ToDouble(ntbT.Text),
                            Convert.ToDouble(ntbP.Text)
                            );
                    }
                    OutPutResult(calProc as Model.CorStt.CalMoleVolume.KayMixMediatorBase);
                } else if (rbtSecVirial.IsChecked == true) {
                    calProc = new Model.CorStt.CalMoleVolume.UsingSecVirial.SecVirialSVMixMediator(
                        new Model.CorStt.Models.SecVirial.ClassicPureModel(),
                        new Model.Material.BinaryComplex(
                            new Model.Material.Chemical(
                                Convert.ToDouble(ntbChemical_1_Tc.Text),
                                Convert.ToDouble(ntbChemical_1_Pc.Text),
                                Convert.ToDouble(ntbChemical_1_w.Text),
                                Convert.ToDouble(ntbChemical_1_Vc.Text),
                                Convert.ToDouble(ntbChemical_1_Zc.Text)
                                ),
                            new Model.Material.Chemical(
                                Convert.ToDouble(ntbChemical_2_Tc.Text),
                                Convert.ToDouble(ntbChemical_2_Pc.Text),
                                Convert.ToDouble(ntbChemical_2_w.Text),
                                Convert.ToDouble(ntbChemical_2_Vc.Text),
                                Convert.ToDouble(ntbChemical_2_Zc.Text)
                                ),
                            Convert.ToDouble(ntbChemical_1_x.Text) / Convert.ToDouble(ntbChemical_2_x.Text)
                        ),
                        Convert.ToDouble(ntbT.Text),
                        Convert.ToDouble(ntbP.Text),
                        Convert.ToDouble(ntbBinIndexes.Text)
                        );
                    OutPutResult(calProc as Model.CorStt.CalMoleVolume.UsingSecVirial.SecVirialSVMixMediator);
                } else if (rbtCeos.IsChecked == true) {
                    if (hcbCeosType.SelectedIndex == 0) {
                        calProc = new Model.Ceos.CalMoleVolume.CeosGasMixMediator(
                            new Model.Ceos.Models.VanderWaalsModel(),
                            new Model.Material.BinaryComplex(
                                new Model.Material.Chemical(Convert.ToDouble(ntbChemical_1_Tc.Text), Convert.ToDouble(ntbChemical_1_Pc.Text)),
                                new Model.Material.Chemical(Convert.ToDouble(ntbChemical_2_Tc.Text), Convert.ToDouble(ntbChemical_2_Pc.Text)),
                                Convert.ToDouble(ntbChemical_1_x.Text) / Convert.ToDouble(ntbChemical_2_x.Text)
                                ),
                            Convert.ToDouble(ntbT.Text),
                            Convert.ToDouble(ntbP.Text),
                            Convert.ToDouble(ntbBinIndexes.Text)
                        );
                    } else if (hcbCeosType.SelectedIndex == 1) {
                        calProc = new Model.Ceos.CalMoleVolume.CeosGasMixMediator(
                            new Model.Ceos.Models.RedlichKwongModel(),
                            new Model.Material.BinaryComplex(
                                new Model.Material.Chemical(Convert.ToDouble(ntbChemical_1_Tc.Text), Convert.ToDouble(ntbChemical_1_Pc.Text)),
                                new Model.Material.Chemical(Convert.ToDouble(ntbChemical_2_Tc.Text), Convert.ToDouble(ntbChemical_2_Pc.Text)),
                                Convert.ToDouble(ntbChemical_1_x.Text) / Convert.ToDouble(ntbChemical_2_x.Text)
                                ),
                            Convert.ToDouble(ntbT.Text),
                            Convert.ToDouble(ntbP.Text),
                            Convert.ToDouble(ntbBinIndexes.Text)
                            );
                    } else if (hcbCeosType.SelectedIndex == 2) {
                        calProc = new Model.Ceos.CalMoleVolume.CeosGasMixMediator(
                            new Model.Ceos.Models.SoaveRedlichKwongModel(),
                            new Model.Material.BinaryComplex(
                                new Model.Material.Chemical(Convert.ToDouble(ntbChemical_1_Tc.Text), Convert.ToDouble(ntbChemical_1_Pc.Text), Convert.ToDouble(ntbChemical_1_w.Text)),
                                new Model.Material.Chemical(Convert.ToDouble(ntbChemical_2_Tc.Text), Convert.ToDouble(ntbChemical_2_Pc.Text), Convert.ToDouble(ntbChemical_2_w.Text)),
                                Convert.ToDouble(ntbChemical_1_x.Text) / Convert.ToDouble(ntbChemical_2_x.Text)
                              ),
                            Convert.ToDouble(ntbT.Text),
                            Convert.ToDouble(ntbP.Text),
                            Convert.ToDouble(ntbBinIndexes.Text)
                            );
                    } else if (hcbCeosType.SelectedIndex == 3) {
                        calProc = new Model.Ceos.CalMoleVolume.CeosGasMixMediator(
                            new Model.Ceos.Models.PengRobinsonModel(),
                            new Model.Material.BinaryComplex(
                                new Model.Material.Chemical(Convert.ToDouble(ntbChemical_1_Tc.Text), Convert.ToDouble(ntbChemical_1_Pc.Text), Convert.ToDouble(ntbChemical_1_w.Text)),
                                new Model.Material.Chemical(Convert.ToDouble(ntbChemical_2_Tc.Text), Convert.ToDouble(ntbChemical_2_Pc.Text), Convert.ToDouble(ntbChemical_2_w.Text)),
                                Convert.ToDouble(ntbChemical_1_x.Text) / Convert.ToDouble(ntbChemical_2_x.Text)
                              ),
                            Convert.ToDouble(ntbT.Text),
                            Convert.ToDouble(ntbP.Text),
                            Convert.ToDouble(ntbBinIndexes.Text)
                            );
                    }
                    OutPutResult(DoCal(calProc as Model.Ceos.CalMoleVolume.CeosGasMixMediator));
                }
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
        /// 异步执行的 Cubic Eos 计算过程 cubicCalOrder.Inilialize()，至少进行2000ms
        /// </summary>
        /// <param name="cubicCalOrder">要进行计算的视图模型</param>
        private Model.Ceos.CalMoleVolume.CeosGasMixMediator.Result DoCal(Model.Ceos.CalMoleVolume.CeosGasMixMediator calProc)
            => calProc.GetResult();
        /// <summary>
        /// Kay 混合模型过程输出
        /// </summary>
        /// <param name="calProc">进行计算的视图模型</param>
        private void OutPutResult(Model.CorStt.CalMoleVolume.KayMixMediatorBase calProc) {
            runX1.Text = ntbChemical_1_x.Text;
            runTc1.Text = ntbChemical_1_Tc.Text;
            runPc1.Text = ntbChemical_1_Pc.Text;
            runW1.Text = "null";
            runVc1.Text = "null";
            runZc1.Text = "null";
            runX2.Text = ntbChemical_2_x.Text;
            runTc2.Text = ntbChemical_2_Tc.Text;
            runPc2.Text = ntbChemical_2_Pc.Text;
            runW2.Text = "null";
            runVc2.Text = "null";
            runZc2.Text = "null";

            paraActualState.Inlines.Clear();
            paraActualState.Inlines.Add($"T = {calProc.Atmos_T } K\nP = {calProc.Atmos_P} MPa\n");
            runMixModel.Text = "Kay's Mix Rule.";

            secParameters.Blocks.Clear();
            if (calProc is Model.CorStt.CalMoleVolume.UsingSecVirial.SecVirialKayMixMediator) {
                Model.CorStt.CalMoleVolume.UsingSecVirial.SecVirialKayMixMediator.Result result
                    = (calProc as Model.CorStt.CalMoleVolume.UsingSecVirial.SecVirialKayMixMediator).GetResult();
                secParameters.Blocks.Add(new Paragraph(new Run
                    ($"Using Secondary Virial Coefficients.\n\nVirtual mixing parameters :\n\nTcm = {result.MixCriticalTemperature} K\nPcm = {result.MixCriticalPressure } MPa\nωm = {result.MixAcentricFactor}\nTrm = {result.MixRelativeTemperature}\nPrm = {result.MixRelativePressure}\n\nBº = {result.MixB0}\nB¹ = {result.MixB1}\nB = {result.MixB}\n\nZº = {result.MixCprFactorBase}\nZ¹ = {result.MixCprFactorCrec}\n\nResults :")));
                paraResult.Inlines.Clear();
                paraResult.Inlines.Add(new Run($"Mole volume V = {result.GasResult.MoleVol} cm³/mol\nCompressibility factor Z ={result.GasResult.CprFactor }"));
                ntbResultVV.Text = result.GasResult.MoleVol.ToString();
                ntbResultVZ.Text = result.GasResult.CprFactor.ToString();
            } else if (calProc is Model.CorStt.CalMoleVolume.UsingCprDiagram.CprDiagKayMixMediator) {
                Model.CorStt.CalMoleVolume.UsingCprDiagram.CprDiagKayMixMediator.Result result
                    = (calProc as Model.CorStt.CalMoleVolume.UsingCprDiagram.CprDiagKayMixMediator).GetResult();
                if (result.GasResult.IsCalcAborted) {
                    secParameters.Blocks.Add(new Paragraph(new Run("Calculation process is aborted.\n\nResults :")));
                    paraResult.Inlines.Clear();
                    paraResult.Inlines.Add(new Run("The calculation is aborted."));
                    ntbResultVV.Text = "null";
                    ntbResultVZ.Text = "null";
                } else {
                    secParameters.Blocks.Add(new Paragraph(new Run($"Using Generalized Compressibiliy Diagram.\n\nVirtual mixing parameters :\n\nTcm = {result.MixCriticalTemperature} K\nPcm = {result.MixCriticalPressure} MPa\nωm = {result.MixAcentricFactor}\nTrm = {result.MixRelativeTemperature}\nPrm = {result.MixRelativePressure}\n\nZº = {result.MixCprFactorBase}\nZ¹ = {result.MixCprFactorCrec}\n\nResults :")));
                    paraResult.Inlines.Clear();
                    paraResult.Inlines.Add(new Run($"Mole volume V = {result.GasResult.MoleVol} cm³/mol\nCompressibility factor Z ={result.GasResult.CprFactor}"));
                    ntbResultVV.Text = result.GasResult.MoleVol.ToString();
                    ntbResultVZ.Text = result.GasResult.CprFactor.ToString();
                }
            }
            ShowResult();
        }
        /// <summary>
        /// SecVirial 混合模型过程输出
        /// </summary>
        /// <param name="calProcess">进行计算的视图模型</param>
        private void OutPutResult(Model.CorStt.CalMoleVolume.UsingSecVirial.SecVirialSVMixMediator calProc) {
            runX1.Text = ntbChemical_1_x.Text;
            runTc1.Text = ntbChemical_1_Tc.Text;
            runPc1.Text = ntbChemical_1_Pc.Text;
            runW1.Text = ntbChemical_1_w.Text;
            runVc1.Text = ntbChemical_1_Vc.Text;
            runZc1.Text = ntbChemical_1_Zc.Text;
            runX2.Text = ntbChemical_2_x.Text;
            runTc2.Text = ntbChemical_2_Tc.Text;
            runPc2.Text = ntbChemical_2_Pc.Text;
            runW2.Text = ntbChemical_2_w.Text;
            runVc2.Text = ntbChemical_2_Vc.Text;
            runZc2.Text = ntbChemical_2_Zc.Text;

            Model.CorStt.CalMoleVolume.UsingSecVirial.SecVirialSVMixMediator.Result result = calProc.GetResult();

            paraActualState.Inlines.Clear();
            paraActualState.Inlines.Add($"T = {result.GasResult.Temperature} K\nP = {result.GasResult.Pressure} MPa\n");
            runMixModel.Text = "Secondary Virial mix rule.";

            secParameters.Blocks.Clear();
            secParameters.Blocks.Add(new Paragraph(new Run($"Chemical - 1 :\n\nBº = {result.B0_11}\nB¹ = {result.B1_11}\nB₁₁ = {result.B_11} cm³/mol\n\nChemical - 2 :\n\nBº = {result.B0_22}\nB¹ = {result.B1_22}\nB₂₂ = {result.B_22} cm³/mol\n\nCrossover Secondary Virial factor:")));
            secParameters.Blocks.Add(new Paragraph(new Run($"Tc₁₂ = {result.MixCriticalTemperature} K\nZc₁₂ = {result.MixCriticalCprFactor}\nVc₁₂ = {result.MixCriticalMoleVol} cm³/mol\nPc₁₂ = {result.MixCriticalPressure} MPa\nω₁₂ = {result.MixAcentricFactor}\n\nBº₁₂ = {result.B0_12}\nB¹₁₂ = {result.B1_12 }\nB₁₂ = {result.B_12} cm³/mol\n\nBm = {result.MixB} cm³/mol\n\nResults:")));

            paraResult.Inlines.Clear();
            paraResult.Inlines.Add(new Run($"Mole volume V = {result.GasResult.MoleVol} cm³/ mol\nCompressibility factor Z = {result.GasResult.CprFactor}"));

            ntbResultVV.Text = result.GasResult.MoleVol.ToString();
            ntbResultVZ.Text = result.GasResult.CprFactor.ToString();

            ShowResult();
        }
        /// <summary>
        /// CubicEos 混合模型过程输出
        /// </summary>
        /// <param name="calProcess">进行计算的视图模型</param>
        private void OutPutResult(Model.Ceos.CalMoleVolume.CeosGasMixMediator.Result calResult) {
            runX1.Text = ntbChemical_1_x.Text;
            runTc1.Text = ntbChemical_1_Tc.Text;
            runPc1.Text = ntbChemical_1_Pc.Text;
            runVc1.Text = "null";
            runZc1.Text = "null";
            runX2.Text = ntbChemical_2_x.Text;
            runTc2.Text = ntbChemical_2_Tc.Text;
            runPc2.Text = ntbChemical_2_Pc.Text;
            runVc2.Text = "null";
            runZc2.Text = "null";
            if (calResult.ModelType == Model.Ceos.Models.CubicEosType.VanDerWaals || calResult.ModelType == Model.Ceos.Models.CubicEosType.RedlichKwong) {
                runW1.Text = "null";
                runW2.Text = "null";
            } else {
                runW1.Text = ntbChemical_1_w.Text;
                runW2.Text = ntbChemical_2_w.Text;
            }
            paraActualState.Inlines.Clear();
            paraActualState.Inlines.Add($"T = {calResult.GasResult.Temperature } K\nP = {calResult.GasResult.Pressure} MPa\nK = {calResult.BinaryCorrelationIndex}");
            switch (calResult.ModelType) {
                case Model.Ceos.Models.CubicEosType.VanDerWaals:
                    runMixModel.Text = "Cubic equation of state mix rule.\n\nChosen cubic equation of state:\nVan der Waals.";
                    break;
                case Model.Ceos.Models.CubicEosType.RedlichKwong:
                    runMixModel.Text = "Cubic equation of state mix rule.\n\nChosen cubic equation of state:\nRedlich - Kwong.";
                    break;
                case Model.Ceos.Models.CubicEosType.SoaveRedlichKwong:
                    runMixModel.Text = "Cubic equation of state mix rule.\n\nChosen cubic equation of state:\nSoave - Redlich - Kwong.";
                    break;
                case Model.Ceos.Models.CubicEosType.PengRobinson:
                    runMixModel.Text = "Cubic equation of state mix rule.\n\nChosen cubic equation of state:\nPeng - Robinson.";
                    break;
            }
            secParameters.Blocks.Clear();
            secParameters.Blocks.Add(new Paragraph(new Run($"Chemical - 1 :\n\na(T)₁₁ = {calResult.Ceos_At11} MPa·(cm³/mol)²\nb(T)₁ = {calResult.Ceos_Bt1} cm³/mol\n\nChemical - 2 :\n\na(T)₂₂ = {calResult.Ceos_At22} MPa·(cm³/mol)²\nb(T)₂ = {calResult.Ceos_Bt2} cm³/mol\n\nCrossover factor:\n\na(T)₁₂ = {calResult.Ceos_At12} MPa·(cm³/mol)²")));
            secParameters.Blocks.Add(new Paragraph(new Run($"Mixture properties :\n\na(T)m = {calResult.Ceos_At} MPa·(cm³/mol)²\nb(T)m = {calResult.Ceos_Bt} cm³/mol\nA(T)m = {calResult.Ceos_AZ}\nB(T)m = {calResult.Ceos_BZ}")));
            secParameters.Blocks.Add(new Paragraph(new Run("/* ======== Iteration start ======== */\n\nGas Phase :")));
            int gasStepCount = 1;
            foreach (Model.Ceos.CalMoleVolume.CeosStepInfo stepInfo in calResult.GasStep) {
                string step = $"Iteration Step {gasStepCount} :\n\nZ({gasStepCount}) = {stepInfo.StepCprFactor}\nf(Z) = {stepInfo.StepFunc}\nf'(Z) = {stepInfo.StepDerivFunc}\nZ({gasStepCount + 1}) = {stepInfo.StepNextCprFactor}\n|Z({gasStepCount}) - Z({gasStepCount + 1})| = {stepInfo.StepDiff}";
                secParameters.Blocks.Add(new Paragraph(new Run(step)));
                gasStepCount++;
            }
            secParameters.Blocks.Add(new Paragraph(new Run("/* ======== Iteration finished ======== */\n\nResults :")));
            paraResult.Inlines.Clear();
            if (calResult.GasResult.IsConverged) {
                paraResult.Inlines.Add(new Run($"Mole volume V = {calResult.GasResult.MoleVol} cm³/mol\nCompressibility factor Z = {calResult.GasResult.CprFactor}"));
                ntbResultVV.Text = calResult.GasResult.MoleVol.ToString();
                ntbResultVZ.Text = calResult.GasResult.CprFactor.ToString();
            } else {
                ntbResultVV.Text = "null";
                ntbResultVZ.Text = "null";
                if (calResult.GasResult.CprFactor == -1.0) {
                    paraResult.Inlines.Add(new Run("The iteration diverges, please try other cubic equation of state."));
                } else if (calResult.GasResult.CprFactor == -2.0) {
                    paraResult.Inlines.Add(new Run("The iteration could not converge in 100 steps, please try other cubic equation of state."));
                }
            }
            ShowResult();
        }
        /// <summary>
        /// 文本框的集合
        /// </summary>
        private List<NumberTextBox> ntbList
        {
            get
            {
                List<NumberTextBox> ntbGather = new List<NumberTextBox> { };
                List<NumberTextBox> ntbBin = new List<NumberTextBox> { ntbBinIndexes };
                List<NumberTextBox> ntbGeneral = new List<NumberTextBox> { ntbChemical_1_x, ntbChemical_1_Tc, ntbChemical_1_Pc, ntbChemical_2_x, ntbChemical_2_Tc, ntbChemical_2_Pc };
                List<NumberTextBox> ntbW = new List<NumberTextBox> { ntbChemical_1_w, ntbChemical_2_w };
                List<NumberTextBox> ntbZcVc = new List<NumberTextBox> { ntbChemical_1_Vc, ntbChemical_1_Zc, ntbChemical_2_Vc, ntbChemical_2_Zc };
                List<NumberTextBox> ntbCondition = new List<NumberTextBox> { ntbT, ntbP };
                if (rbtKay.IsChecked == true) {
                    ntbGather.AddRange(ntbGeneral);
                    ntbGather.AddRange(ntbW);
                } else if (rbtSecVirial.IsChecked == true) {
                    ntbGather.AddRange(ntbBin);
                    ntbGather.AddRange(ntbGeneral);
                    ntbGather.AddRange(ntbW);
                    ntbGather.AddRange(ntbZcVc);
                } else if (rbtCeos.IsChecked == true) {
                    ntbGather.AddRange(ntbBin);
                    ntbGather.AddRange(ntbGeneral);
                    if (hcbCeosType.SelectedIndex == 2 || hcbCeosType.SelectedIndex == 3) {
                        ntbGather.AddRange(ntbW);
                    }
                }
                ntbGather.AddRange(ntbCondition);
                return ntbGather;
            }
        }
        /// <summary>
        /// 单选的集合
        /// </summary>
        private List<RadioButton> rbtList
            => new List<RadioButton> { rbtCeos, rbtKay, rbtSecVirial };
        /// <summary>
        /// hcb 的集合
        /// </summary>
        private List<HeaderComboBox> hcbList
        {
            get
            {
                if (rbtCeos.IsChecked == true) {
                    return new List<HeaderComboBox> { hcbCeosType };
                } else if (rbtKay.IsChecked == true) {
                    return new List<HeaderComboBox> { hcbGceMethod };
                } else {
                    return null;
                }
            }
        }
        /// <summary>
        /// 输入验证
        /// </summary>
        private bool IsInputLegal() {
            if (!DataChecker.CheckRadioSelection(rbtList)) {
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
        /// <summary>
        /// 和 = 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                }
            } catch (Exception) {
                if (sender as Controls.NumberTextBox == ntbChemical_1_x) {
                    ntbChemical_1_x.Text = "Input Error";
                } else if (sender as Controls.NumberTextBox == ntbChemical_2_x) {
                    ntbChemical_2_x.Text = "Input Error";
                }
            }
        }
        private void DbBtnCon_Click(object sender, RoutedEventArgs e) {
            DbConView db = new DbConView();
            db.Owner = Window.GetWindow(this);
            db.ShowDialog();
            if (db.IsChemGot == true) {
                if (sender == btnDbCon_1) {
                    ntbChemical_1_Tc.Text = db.Chem.Tc.ToString();
                    ntbChemical_1_Pc.Text = db.Chem.Pc.ToString();
                    ntbChemical_1_w.Text = db.Chem.W.ToString();
                    ntbChemical_1_Zc.Text = db.Chem.Zc.ToString();
                    ntbChemical_1_Vc.Text = db.Chem.Vc.ToString();
                } else if (sender == btnDbCon_2) {
                    ntbChemical_2_Tc.Text = db.Chem.Tc.ToString();
                    ntbChemical_2_Pc.Text = db.Chem.Pc.ToString();
                    ntbChemical_2_w.Text = db.Chem.W.ToString();
                    ntbChemical_2_Zc.Text = db.Chem.Zc.ToString();
                    ntbChemical_2_Vc.Text = db.Chem.Vc.ToString();
                }
            } else {
                return;
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            if (App.isDbExist == true) {
                btnDbCon_1.Visibility = Visibility.Visible;
                btnDbCon_2.Visibility = Visibility.Visible;
            } else {
                btnDbCon_1.Visibility = Visibility.Collapsed;
                btnDbCon_2.Visibility = Visibility.Collapsed;
            }
        }
    }
}
