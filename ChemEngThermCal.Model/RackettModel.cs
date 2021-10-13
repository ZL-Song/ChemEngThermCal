using System;

namespace ChemEngThermCal.Model {
    /// <summary>
    /// Rackett 方程算法
    /// </summary>
    public static class Rackett {
        /// <summary>
        /// Rackett 方程的类型
        /// </summary>
        public enum Expression {
            AlphaBeta = 0,
            AcentricFactor = 1
        }
        /// <summary>
        /// 以 α β 形式的 Rackett 方程进行计算
        /// </summary>
        public static double GetSaturateLiquidVolume(double criticalTemperature, double criticalPressure, double alpha, double beta, double actualTemperature)
            => (8.314 * criticalTemperature / criticalPressure)
                * Math.Pow(
                    alpha + beta * (1 - actualTemperature/criticalTemperature),
                    1 + Math.Pow(
                        (1 - actualTemperature / criticalTemperature),
                        (2.0 / 7.0)
                        )
                    );
        /// <summary>
        ///  以 ω 形式的 Rackett 方程进行计算
        /// </summary>
        public static double GetSaturateLiquidVolume(double criticalTemperature, double criticalPressure, double acentricFactor, double actualTemperature)
            => (8.314 * criticalTemperature / criticalPressure)
                * Math.Pow(
                    0.29056 - 0.08775 * acentricFactor,
                    1 + Math.Pow(
                        (1 - actualTemperature / criticalTemperature),
                        (2.0 / 7.0)
                        )
                    );
        /// <summary>
        ///  以 ω 形式的 Rackett 方程进行计算
        /// </summary>
        public static double GetSaturateLiquidVolume(Material.Chemical chemical, double actualTemperature)
            => (8.314 * chemical.CriticalTemperature / chemical.CriticalPressure)
                * Math.Pow(
                    0.29056 - 0.08775 * chemical.AcentricFactor,
                    1 + Math.Pow(
                        (1 - actualTemperature / chemical.CriticalTemperature),
                        (2.0 / 7.0)
                        )
                    );
    }
}
