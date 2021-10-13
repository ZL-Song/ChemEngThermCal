using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChemEngThermCal.View {
    /// <summary>
    /// 用于数据检查
    /// </summary>
    class DataChecker {
        /// <summary>
        /// 验证 NumberTextBox 集合中 输入是否为 数字 ，null 为通过
        /// </summary>  
        public static Controls.NumberTextBox CheckInputNumeric(List<Controls.NumberTextBox> ntbToCheck) {
            if (ntbToCheck == null) {
                return null;
            }
            foreach (Controls.NumberTextBox ntb in ntbToCheck) {
                try {
                    Convert.ToDouble(ntb.Text);
                } catch {
                    return ntb;
                }
            }
            return null;
        }
        /// <summary>
        /// 验证 Radiobutton 集合中 是否有被选中的元素，true 为通过
        /// </summary> 
        public static bool CheckRadioSelection(List<RadioButton> rbtToCheck) {
            foreach (RadioButton rbt in rbtToCheck) {
                if (rbt.IsChecked == true) {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 验证 ComboBox 集合中 是否有未选中元素的元素，null 为验证通过
        /// </summary> 
        public static Controls.HeaderComboBox CheckComboSelection(List<Controls.HeaderComboBox> cbbToCheck) {
            if (cbbToCheck == null) {
                return null;
            }
            foreach (Controls.HeaderComboBox cbb in cbbToCheck) {
                if (cbb.SelectedIndex == -1) {
                    return cbb;
                }
            }
            return null;
        }
        /// <summary>
        /// 验证 NumberTextBox 集合中 输入是否为数字且其和是否为 1，true为验证通过
        /// </summary> 
        public static bool CheckIsSumOne(List<Controls.NumberTextBox> ntbToCheck) {
            double sum = 0;
            if (CheckInputNumeric(ntbToCheck) != null) {
                return false;
            } else {
                foreach (Controls.NumberTextBox ntb in ntbToCheck) {
                    sum += Convert.ToDouble(ntb.Text);
                }
                if (sum == 1.0) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }
}
