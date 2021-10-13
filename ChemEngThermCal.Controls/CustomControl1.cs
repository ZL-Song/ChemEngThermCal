using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ChemEngThermCal.Controls {
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:ChemEngThermCal.Controls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:ChemEngThermCal.Controls;assembly=ChemEngThermCal.Controls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误: 
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class HeaderComboBox : ComboBox {
        static HeaderComboBox() {
            //覆盖初始化声明
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderComboBox), new FrameworkPropertyMetadata(typeof(HeaderComboBox)));
        }
        #region DependencyProperties
        //公共属性 Header
        public string Header
        {
            get;
            set;
        }
        //注册依赖项属性Header
        /// <summary>
        /// TabItem 的副标题
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(HeaderComboBox),
            new FrameworkPropertyMetadata(""));
        #endregion
    }
    public class NumberTextBox : TextBox {
        /// <summary>
        /// 实例化一个带前置内容与后置内容并只允许输入数字的 NumberTextBox
        /// </summary>
        static NumberTextBox() {
            //覆盖初始化声明
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberTextBox), new FrameworkPropertyMetadata(typeof(NumberTextBox)));
        }

        #region DependencyProperties
        //公共属性 LeadingContent
        public string LeadingContent
        {
            get;
            set;
        }
        //公共属性 FollowingingContent
        public string FollowingContent
        {
            get;
            set;
        }
        //注册依赖项属性 LeadingContent
        /// <summary>
        /// 文本框的前置内容
        /// </summary>
        public static readonly DependencyProperty LeadingContentProperty =
            DependencyProperty.Register("LeadingContent", typeof(string), typeof(NumberTextBox),
            new FrameworkPropertyMetadata(""));
        //注册依赖项属性 FollowingContent
        /// <summary>
        /// 文本框的后置置内容
        /// </summary>
        public static readonly DependencyProperty FollowingContentProperty =
            DependencyProperty.Register("FollowingContent", typeof(string), typeof(NumberTextBox),
            new FrameworkPropertyMetadata(""));
        #endregion

        /// <summary>
        /// 重写 OnKeyDown 事件，仅允许数字输入
        /// </summary>
        /// <param name="e">事件数据</param>
        protected override void OnKeyDown(KeyEventArgs e) {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                e.Key == Key.Back || e.Key == Key.NumLock || e.Key == Key.Tab) {
                e.Handled = false;
            } else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal) {
                if (Text.IndexOf('.') == -1) {
                    e.Handled = false;
                } else {
                    e.Handled = true;
                }
            } else if (e.Key == Key.Subtract || e.Key == Key.OemMinus) {
                if (Text.IndexOf('-') == -1) {
                    e.Handled = false;
                } else {
                    e.Handled = true;
                }
            } else {
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
        /// <summary>
        /// 重写 OnGotFocus 事件，聚焦时选定全部文本
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(RoutedEventArgs e) {
            base.OnGotFocus(e);
            SelectAll();
        }
    }
    public class ViceHeaderRadioButton : RadioButton {
        /// <summary>
        /// 实例化一个带有副标题的 TabItem
        /// </summary>
        static ViceHeaderRadioButton() {
            //覆盖初始化声明
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ViceHeaderRadioButton), new FrameworkPropertyMetadata(typeof(ViceHeaderRadioButton)));
        }
        #region DependencyProperties
        //公共属性 ViceHeader
        public string ViceHeader
        {
            get;
            set;
        }
        //注册依赖项属性ViceHeader
        /// <summary>
        /// TabItem 的副标题
        /// </summary>
        public static readonly DependencyProperty ViceHeaderProperty =
            DependencyProperty.Register("ViceHeader", typeof(string), typeof(ViceHeaderRadioButton),
            new FrameworkPropertyMetadata(""));
        #endregion
    }
}
