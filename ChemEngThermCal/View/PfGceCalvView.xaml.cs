using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChemEngThermCal.Controls;
using ChemEngThermCal.Model.CorStt.CalMoleVolume.UsingSecVirial;
using ChemEngThermCal.Model.CorStt.CalMoleVolume.UsingCprDiagram;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Animation;

namespace ChemEngThermCal.View {
    /// <summary>
    /// pfGceCalvView.xaml 的交互逻辑
    /// </summary>
    public partial class PfGceCalvView : Page {
        public PfGceCalvView() {
            InitializeComponent();
            grdResult.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 所有文本框的集合
        /// </summary>
        private List<NumberTextBox> ntbList
            => new List<NumberTextBox> { ntbTc, ntbPc, ntbW, ntbT, ntbP };
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
        /// 开始计算
        /// </summary> 
        private void btnCal_Click(object sender, RoutedEventArgs e) {
            grdResult.Visibility = Visibility.Collapsed;
            (sender as Button).IsEnabled = false;
            if (IsInputLegal()) {
                Model.CorStt.CalMoleVolume.MediatorPureBase calProc;
                if (hcbCalMethod.SelectedIndex == 0) {
                    calProc = new SecVirialPureMediator(
                        new Model.CorStt.Models.SecVirial.ClassicPureModel(),
                        new Model.Material.Chemical(
                            Convert.ToDouble(ntbTc.Text),
                            Convert.ToDouble(ntbPc.Text),
                            Convert.ToDouble(ntbW.Text)
                            ),
                        Convert.ToDouble(ntbT.Text),
                        Convert.ToDouble(ntbP.Text)
                        );
                    OutPutResult(DoCal(calProc as SecVirialPureMediator));
                } else if (hcbCalMethod.SelectedIndex == 1) {
                    calProc = new CprDiagPureMediator(
                      new Model.CorStt.Models.Diagram.CprModel(new CprDiagInterator()),
                      new Model.Material.Chemical(
                          Convert.ToDouble(ntbTc.Text),
                          Convert.ToDouble(ntbPc.Text),
                          Convert.ToDouble(ntbW.Text)
                      ),
                      Convert.ToDouble(ntbT.Text),
                      Convert.ToDouble(ntbP.Text)
                      );
                    OutPutResult((calProc as CprDiagPureMediator).GetResult());
                }
            }
            (sender as Button).IsEnabled = true;
        }
        /// <summary>
        /// 进行计算
        /// </summary> 
        private SecVirialPureMediator.Result DoCal(SecVirialPureMediator secVirialPureMediator)
            => secVirialPureMediator.GetResult();
        private void OutPutResult(SecVirialPureMediator.Result result) {
            ntbResultB.Visibility = Visibility.Visible;
            ntbResultV.Text = result.GasResult.MoleVol.ToString();
            ntbResultZ.Text = result.GasResult.CprFactor.ToString();
            ntbResultTr.Text = result.RelativeTemperature.ToString();
            ntbResultPr.Text = result.RelativePressure.ToString();
            ntbResultB0.Text = result.B0.ToString();
            ntbResultB1.Text = result.B1.ToString();
            ntbResultB.Text = result.B.ToString();
            ntbResultZ0.Text = result.Z0.ToString();
            ntbResultZ1.Text = result.Z1.ToString();
            ShowResult();
        }
        private void OutPutResult(CprDiagPureMediator.Result result) {
            if (result.GasResult.IsCalcAborted == true) {
                return;
            } else {
                ntbResultB.Visibility = Visibility.Collapsed;
                ntbResultV.Text = result.GasResult.MoleVol.ToString();
                ntbResultZ.Text = result.GasResult.CprFactor.ToString();
                ntbResultTr.Text = result.RelativeTemperature.ToString();
                ntbResultPr.Text = result.RelativePressure.ToString();
                ntbResultZ0.Text = result.Z0.ToString();
                ntbResultZ1.Text = result.Z1.ToString();
                ShowResult();
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
