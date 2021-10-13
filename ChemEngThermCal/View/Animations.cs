using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ChemEngThermCal.View {
    /// <summary>
    /// 动画
    /// </summary>
    public static class ControlAnimations {
        static ControlAnimations() {
            _animeEase.EasingMode = EasingMode.EaseOut;
        }
        private static Duration _animationDuation = new Duration(TimeSpan.FromMilliseconds(500));
        private static double _heightToValue = 0;
        private static CircleEase _animeEase = new CircleEase();
        public static Duration GeneralAniDuation
        {
            get { return _animationDuation; }
        }
        #region TextBox 动画
        private static double _textBoxHeightFromValue = 30.5;
        /// <summary>
        /// TextBox 的 移出 动画
        /// </summary>
        public static DoubleAnimation TextBoxHeightDownAnimation
        {
            get
            {
                DoubleAnimation da = new DoubleAnimation(_textBoxHeightFromValue, _heightToValue, _animationDuation);
                da.EasingFunction = _animeEase;
                return da;
            }
        }
        /// <summary>
        /// TextBox 的 移入 动画
        /// </summary>
        public static DoubleAnimation TextBoxHeightUpAnimation
        {
            get
            {
                DoubleAnimation da = new DoubleAnimation(_heightToValue, _textBoxHeightFromValue, _animationDuation);
                da.EasingFunction = _animeEase;
                return da;
            }
        }
        #endregion
        #region HeaderedComboBox 动画
        private static double _comboBoxHeightFromValue = 72;
        /// <summary>
        /// HeaderComboBox 的 移出 动画
        /// </summary>
        public static DoubleAnimation HeaderComboBoxHeightDownAnimation
        {
            get
            {
                DoubleAnimation da = new DoubleAnimation(_comboBoxHeightFromValue, _heightToValue, _animationDuation);
                da.EasingFunction = _animeEase;
                return da;
            }
        }
        /// <summary>
        /// HeaderComboBox 的 移入 动画 
        /// </summary>
        public static DoubleAnimation HeaderComboBoxHeightUpAnimation
        {
            get
            {
                DoubleAnimation da = new DoubleAnimation(_heightToValue, _comboBoxHeightFromValue, _animationDuation);
                da.EasingFunction = _animeEase;
                return da;
            }
        }
        #endregion
        #region Image 动画
        private static double _opacityToValue = 0;
        private static double _opacityFromValue = 1;
        private static double _HeightFromValue = 60;
        /// <summary>
        /// 渐出 动画
        /// </summary>
        public static DoubleAnimation FadeOutAnimation
        {
            get
            {
                DoubleAnimation da = new DoubleAnimation(_opacityFromValue, _opacityToValue, _animationDuation);
                da.EasingFunction = _animeEase;
                return da;
            }
        }
        /// <summary>
        /// 渐入 动画
        /// </summary>
        public static DoubleAnimation FadeInAnimation
        {
            get
            {
                DoubleAnimation da = new DoubleAnimation(_opacityToValue, _opacityFromValue, new Duration(TimeSpan.FromMilliseconds(300)));
                return da;
            }
        }

        public static DoubleAnimation ImageHeightUpAnimation
        {
            get
            {
                DoubleAnimation da = new DoubleAnimation(0, _HeightFromValue, _animationDuation);
                return da;
            }
        }
        #endregion
        #region Frame 移出 / 移入 动画
        private static Thickness _thicknessInStartValue = new Thickness(50, 0, -50, 0);
        private static Thickness _thicknessOutStartValsue = new Thickness(-50, 0, 50, 0);
        private static Thickness _thicknessEndValue = new Thickness(0);
        public static ThicknessAnimation FrameMoveInAnimation
        {
            get
            {
                ThicknessAnimation ta = new ThicknessAnimation(_thicknessInStartValue, _thicknessEndValue, GeneralAniDuation);
                ta.EasingFunction = _animeEase;
                return ta;
            }
        }
        public static ThicknessAnimation FrameMoveOutAnimation
        {
            get
            {
                ThicknessAnimation ta = new ThicknessAnimation(_thicknessOutStartValsue, _thicknessEndValue, GeneralAniDuation);
                ta.EasingFunction = _animeEase;
                return ta;
            }
        }
        #endregion
        #region Frame 移出 / 移入 动画 
        private static Thickness _thicknessUpValue = new Thickness(0, 20, 0, -20);
        public static ThicknessAnimation LabelMoveOutAnimation
        {
            get
            {
                ThicknessAnimation ta = new ThicknessAnimation(_thicknessEndValue, _thicknessUpValue, GeneralAniDuation);
                ta.EasingFunction = _animeEase;
                return ta;
            }
        }
        public static ThicknessAnimation LabelMoveInAnimation
        {
            get
            {
                ThicknessAnimation ta = new ThicknessAnimation(_thicknessUpValue, _thicknessEndValue, GeneralAniDuation);
                ta.EasingFunction = _animeEase;
                return ta;
            }
        }
        #endregion
    }
    /// <summary>
    /// 用于与 Model 交互的数据类型转换类
    /// </summary>
    public static class IO {
        /// <summary>
        /// 将输入的字符串转换为 Double
        /// </summary>
        /// <param name="stringsToConvert">要转换的字符串数组</param>
        /// <returns>转换结果的数组</returns>
        public static double[] StringsConvertToDoubles(string[] stringsToConvert) {
            double[] convertResult = new double[stringsToConvert.Length];
            for (int i = 0; i < stringsToConvert.Length; i++) {
                convertResult[i] = Convert.ToDouble(stringsToConvert[i]);
            }
            return convertResult;
        }
        /// <summary>
        /// 将输入的Double转换为 字符串 
        /// </summary>
        /// <param name="doublesToConvert">要转换的Double数组</param>
        /// <returns>转换结果的数组</returns>
        public static string[] DoublesConvertToStrings(double[] doublesToConvert) {
            string[] convertResult = new string[doublesToConvert.Length];
            for (int i = 0; i < doublesToConvert.Length; i++) {
                convertResult[i] = Convert.ToString(doublesToConvert[i]);
            }
            return convertResult;
        }
    }
}
