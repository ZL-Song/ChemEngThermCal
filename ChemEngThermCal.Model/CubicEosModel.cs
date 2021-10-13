using System;
using System.Collections.Generic;
using ChemEngThermCal.Model.Material;
/// <summary>
/// 立方型状态方程的模型
/// </summary>
namespace ChemEngThermCal.Model.Ceos.Models {
    /*  需要注意：

        RK 方程 计算 剩余焓、剩余熵、逸度系数 计算式，以 冯新，宣爱国等著《化工热力学》（第一版第五次印刷）中 P111 式 4-53(φ)； P66 式 3-51(Hr)，式 3-52(Sr) 为来源。

        SRK PR 方程 计算 剩余焓、剩余熵、逸度系数 计算式，以 陈新志等著《化工热力学》（第三版第一次印刷）中 P44 表 3.1 为来源。

        第一，但是，以上计算式 关于 T 的 a b 全部被以 关于 Z 的 A B 替代。见 CeosBaseModel 抽象类中 GetAt()，GetBt()，与 GetAZ(), GetBZ() 方法之间的联系。

        第二，上述来源中，PR 方程计算 逸度系数 的算式是错误的。该式第四项 1 / ( 2^1.5 * b * R * T) * ln(...) 应为 a / ( 2^1.5 * b * R * T) * ln(...)，

            意即，该项分子不应为 1 * ln(...) 而应为 a * ln(...)。

        */
    /// <summary>
    /// Ceos要进行逸度计算所必需实现的接口
    /// </summary>
    public interface ILiqApplicableCeos {
        /// <summary>
        /// 进行逸度计算
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns></returns>
        double GetFugacityCoe(Material.Chemical chemical, double cprFactor, double temperature, double pressure);
        /// <summary>
        /// 偏离焓
        /// </summary>
        /// <param name="chemical"></param>
        /// <param name="cprFactor"></param>
        /// <param name="temperature"></param>
        /// <param name="pressure"></param>
        /// <returns></returns>
        double GetResidualEnthalpy(Material.Chemical chemical, double cprFactor, double temperature, double pressure);
        /// <summary>
        /// 偏离熵
        /// </summary>
        /// <param name="chemical"></param>
        /// <param name="cprFactor"></param>
        /// <param name="temperature"></param>
        /// <param name="pressure"></param>
        /// <returns></returns>
        double GetResidualEntropy(Material.Chemical chemical, double cprFactor, double temperature, double pressure);
    }
    /// <summary>
    /// Eos 方程类型的枚举
    /// </summary>
    public enum CubicEosType {
        VanDerWaals = 0,
        RedlichKwong = 1,
        SoaveRedlichKwong = 2,
        PengRobinson = 3
    }
    /// <summary>
    /// 立方型状态方程的基类型
    /// </summary>
    public abstract class CeosBaseModel {
        #region 由子类实现的成员
        /// <summary>
        /// α(Tr)  
        /// </summary>
        public abstract double GetAlphaTemperatureRelative(Chemical chemical, double actualTemperature);
        /// <summary>
        /// ac
        /// </summary>
        /// <param name="chemical">目标化合物</param>
        /// <returns>ac</returns>
        public double GetAc(Chemical chemical)
            => OmigaA * Math.Pow(8.314 * chemical.CriticalTemperature, 2.0) / chemical.CriticalPressure;
        /// <summary>
        /// σ
        /// </summary>
        public abstract double Sigma { get; }
        /// <summary>
        /// ε
        /// </summary>
        public abstract double Epsilon { get; }
        /// <summary>
        /// Ωa
        /// </summary>
        public abstract double OmigaA { get; }
        /// <summary>
        /// Ωb
        /// </summary>
        public abstract double OmigaB { get; }
        /// <summary>
        /// 模板方法：f(Z)的计算值
        /// </summary>
        /// <param name="cprFactor">气相压缩因子</param>
        /// <returns>f(Z)</returns>
        public abstract double DoFuncZnCal(double cprFactor, double aT, double bT, double actualTemperature, double actualPressure);
        /// <summary>
        /// 模板方法：f'(Z)的计算值
        /// </summary>
        /// <param name="cprFactor">气相压缩因子</param>
        /// <returns>f'(Z)</returns>
        public abstract double DoDerivFuncZnCal(double cprFactor, double aT, double bT, double actualTemperature, double actualPressure);
        /// <summary>
        /// 指示方程是否可用于液相计算
        /// </summary>
        public abstract bool IsLiqPhaseApplicable { get; }
        /// <summary>
        /// 获取当前模型类型
        /// </summary>
        public abstract CubicEosType EqtType { get; }
        #endregion
        /// <summary>
        /// A(T)m——Z
        /// </summary>
        public double GetAZ(double aT, double actualTemperature, double actualPressure)
            => aT * actualPressure / Math.Pow(8.314 * actualTemperature, 2.0);
        /// <summary>
        /// A(T)m——Z
        /// </summary>
        public double GetAZ(Material.Chemical chemical, double actualTemperature, double actualPressure)
           => GetAt(chemical, actualTemperature) * actualPressure / Math.Pow(8.314 * actualTemperature, 2.0);
        /// <summary>
        /// B(T)m——Z
        /// </summary>
        public double GetBZ(double bT, double actualTemperature, double actualPressure)
            => bT * actualPressure / 8.314 / actualTemperature;
        /// <summary>
        /// B(T)m——Z
        /// </summary>
        public double GetBZ(Material.Chemical chemical, double actualTemperature, double actualPressure)
            => GetBt(chemical) * actualPressure / 8.314 / actualTemperature;
        /// <summary>
        /// a(T)——T
        /// </summary>
        /// <param name="chemical">目标化合物</param>
        /// <returns>a(T)</returns>
        public double GetAt(Material.Chemical chemical, double actualTemperature)
            => GetAlphaTemperatureRelative(chemical, actualTemperature) * GetAc(chemical);
        /// <summary>
        /// b(T)——T
        /// </summary>
        /// <param name="chemical">目标化合物</param>
        /// <returns>b(T)</returns>
        public double GetBt(Material.Chemical chemical)
            => OmigaB * 8.314 * chemical.CriticalTemperature / chemical.CriticalPressure;
        /// <summary>
        /// V =
        /// </summary>
        /// <param name="cprFactor">Z</param>
        /// <returns>V</returns>
        public double GetMoleVolume(double cprFactor, double actualTemperature, double actualPressure)
            => cprFactor * 8.314 * actualTemperature / actualPressure;
        /// <summary>
        /// 蒸发焓
        /// </summary> 
        /// <param name="gasResidualEnthalpy">气相剩余焓</param>
        /// <param name="liqResidualEnthalpy">液相剩余焓</param>
        /// <param name="temperature">温度</param>
        /// <returns>蒸发焓</returns>
        public double GetVaporizationEnthalpy(double gasResidualEnthalpy, double liqResidualEnthalpy, double temperature)
            => (gasResidualEnthalpy - liqResidualEnthalpy) * 8.314 * temperature;
        /// <summary>
        /// 蒸发熵
        /// </summary> 
        /// <param name="gasResidualEntropy">气相剩余熵</param>
        /// <param name="liqResidualEntropy">液相剩余熵</param> 
        /// <returns>蒸发熵</returns>
        public double GetVaporizationEntropy(double gasResidualEntropy, double liqResidualEntropy)
            => (gasResidualEntropy - liqResidualEntropy) * 8.314;
    }
    /// <summary>
    /// Van der Waals 方程，派生自 PureBaseModel
    /// </summary>
    public class VanderWaalsModel : CeosBaseModel {
        public override double GetAlphaTemperatureRelative(Material.Chemical chemical, double actualTemperature) => 1.0;
        public override double Sigma => 0.0;
        public override double Epsilon => 0.0;
        public override double OmigaA => 27.0 / 64.0;
        public override double OmigaB => 1.0 / 8.0;
        public override bool IsLiqPhaseApplicable => false;
        public override CubicEosType EqtType => CubicEosType.VanDerWaals;
        public override double DoFuncZnCal(double cprFactor, double aT, double bT, double actualTemperature, double actualPressure)
            => Math.Pow(cprFactor, 3.0)
            - (bT * actualPressure / (8.314 * actualTemperature) + 1.0) * Math.Pow(cprFactor, 2.0)
            + aT * actualPressure / Math.Pow(8.314 * actualTemperature, 2.0) * cprFactor
            - aT * bT * Math.Pow(actualPressure, 3.0) / Math.Pow(8.314 * actualTemperature, 3.0);
        public override double DoDerivFuncZnCal(double cprFactor, double aT, double bT, double actualTemperature, double actualPressure)
            => 3.0 * Math.Pow(cprFactor, 2.0)
            - 2.0 * (bT * actualPressure / (8.314 * actualTemperature) + 1.0) * cprFactor
            + aT * actualPressure / Math.Pow(8.314 * actualTemperature, 2.0);
    }
    /// <summary>
    /// 可用于液相计算的 Ceos 的基类型
    /// </summary>
    public abstract class LiqApplicableCeos : CeosBaseModel, ILiqApplicableCeos {
        public abstract double GetFugacityCoe(Chemical chemical, double cprFactor, double temperature, double pressure);
        public abstract double GetResidualEnthalpy(Chemical chemical, double cprFactor, double temperature, double pressure);
        public abstract double GetResidualEntropy(Chemical chemical, double cprFactor, double temperature, double pressure);
    }
    /// <summary>
    /// RK 方程，派生自 EosBaseCalModel
    /// </summary>
    public class RedlichKwongModel : LiqApplicableCeos {
        public override double GetAlphaTemperatureRelative(Material.Chemical chemical, double actualTemperature)
            => Math.Pow(actualTemperature / chemical.CriticalTemperature, -0.5);
        public override double Sigma => 1.0;
        public override double Epsilon => 0.0;
        public override double OmigaA => 0.42748;
        public override double OmigaB => 0.08664;
        public override bool IsLiqPhaseApplicable => true;
        public override CubicEosType EqtType => CubicEosType.RedlichKwong;
        public override double DoFuncZnCal(double cprFactor, double aT, double bT, double actualTemperature, double actualPressure)
            => Math.Pow(cprFactor, 3.0)
            - Math.Pow(cprFactor, 2.0)
            + (GetAZ(aT, actualTemperature, actualPressure) - GetBZ(bT, actualTemperature, actualPressure) - Math.Pow(GetBZ(bT, actualTemperature, actualPressure), 2.0)) * cprFactor
            - GetAZ(aT, actualTemperature, actualPressure) * GetBZ(bT, actualTemperature, actualPressure);
        public override double DoDerivFuncZnCal(double cprFactor, double aT, double bT, double actualTemperature, double actualPressure)
            => 3.0 * Math.Pow(cprFactor, 2.0)
            - 2.0 * cprFactor
            + (GetAZ(aT, actualTemperature, actualPressure) - GetBZ(bT, actualTemperature, actualPressure) - Math.Pow(GetBZ(bT, actualTemperature, actualPressure), 2.0));
        /// <summary>
        /// 逸度系数
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns>逸度系数 φ</returns>
        public override double GetFugacityCoe(Chemical chemical, double cprFactor, double temperature, double pressure)
            => Math.Exp(cprFactor - 1.0
                - Math.Log(cprFactor - GetBZ(chemical, temperature, pressure))
                - Math.Log(1.0 + GetBZ(chemical, temperature, pressure) / cprFactor)
                    * GetAZ(chemical, temperature, pressure) / GetBZ(chemical, temperature, pressure));
        /// <summary>
        /// 剩余焓
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns></returns>
        public override double GetResidualEnthalpy(Chemical chemical, double cprFactor, double temperature, double pressure)
            => cprFactor - 1.0
                - 1.5 * GetAZ(chemical, temperature, pressure) / GetBZ(chemical, temperature, pressure)
                    * Math.Log(1.0 + GetBZ(chemical, temperature, pressure) / cprFactor);
        /// <summary>
        /// 剩余熵
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns></returns>
        public override double GetResidualEntropy(Chemical chemical, double cprFactor, double temperature, double pressure)
            => Math.Log(cprFactor - GetBZ(chemical, temperature, pressure))
                    - Math.Log(1.0 + GetBZ(chemical, temperature, pressure) / cprFactor)
                        * GetAZ(chemical, temperature, pressure) / 2.0 / GetBZ(chemical, temperature, pressure);
    }
    /// <summary>
    /// SRK 方程，派生自 RedlichKwongCalModel
    /// </summary>
    public class SoaveRedlichKwongModel : RedlichKwongModel {
        public override double GetAlphaTemperatureRelative(Material.Chemical chemical, double actualTemperature)
            => Math.Pow(1.0 + (0.48 + 1.574 * chemical.AcentricFactor - 0.176 * Math.Pow(chemical.AcentricFactor, 2.0)) * (1.0 - Math.Sqrt(actualTemperature / chemical.CriticalTemperature)), 2.0);
        public override CubicEosType EqtType => CubicEosType.SoaveRedlichKwong;
        /// <summary>
        /// 逸度系数
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns>逸度系数 φ</returns>
        public override double GetFugacityCoe(Material.Chemical chemical, double cprFactor, double temperature, double pressure)
            => Math.Exp(cprFactor - 1.0
                - Math.Log(cprFactor - GetBZ(chemical, temperature, pressure))
                - Math.Log(1.0 + GetBZ(chemical, temperature, pressure) / cprFactor)
                    * GetAZ(chemical, temperature, pressure) / GetBZ(chemical, temperature, pressure));
        /// <summary>
        /// 剩余焓
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns></returns>
        public override double GetResidualEnthalpy(Material.Chemical chemical, double cprFactor, double temperature, double pressure)
            => cprFactor - 1.0
                - Math.Log(1.0 + GetBZ(chemical, temperature, pressure) / cprFactor)
                    * (GetAt(chemical, temperature) + (0.48 + 1.574 * chemical.AcentricFactor - 0.176 * Math.Pow(chemical.AcentricFactor, 2.0))
                        * Math.Sqrt(GetAt(chemical, temperature) * GetAc(chemical) * temperature / chemical.CriticalTemperature))
                    / GetBt(chemical) / 8.314 / temperature;
        /// <summary>
        /// 剩余熵
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns></returns>
        public override double GetResidualEntropy(Material.Chemical chemical, double cprFactor, double temperature, double pressure)
            => Math.Log(cprFactor - GetBZ(chemical, temperature, pressure))
                - Math.Log(1.0 + GetBZ(chemical, temperature, pressure) / cprFactor)
                    * (0.48 + 1.574 * chemical.AcentricFactor - 0.176 * Math.Pow(chemical.AcentricFactor, 2.0))
                    * Math.Sqrt(GetAt(chemical, temperature) * GetAc(chemical) / temperature / chemical.CriticalTemperature)
                    / GetBt(chemical) / 8.314;
    }
    /// <summary>
    /// PR 方程，派生自 EosBaseCalModel
    /// </summary>
    public class PengRobinsonModel : LiqApplicableCeos {
        public override double GetAlphaTemperatureRelative(Material.Chemical chemical, double actualTemperature)
            => Math.Pow((1.0 + (0.37464 + 1.54226 * chemical.AcentricFactor - 0.26992 * Math.Pow(chemical.AcentricFactor, 2.0)) * (1.0 - Math.Sqrt(actualTemperature / chemical.CriticalTemperature))), 2.0);
        public override double Sigma => 1.0 + Math.Sqrt(2.0);
        public override double Epsilon => 1.0 - Math.Sqrt(2.0);
        public override double OmigaA => 0.45742;
        public override double OmigaB => 0.0778;
        public override bool IsLiqPhaseApplicable => true;
        public override CubicEosType EqtType => CubicEosType.PengRobinson;
        public override double DoFuncZnCal(double cprFactor, double aT, double bT, double actualTemperature, double actualPressure)
            => Math.Pow(cprFactor, 3.0)
            - (1 - GetBZ(bT, actualTemperature, actualPressure)) * Math.Pow(cprFactor, 2.0)
            + (GetAZ(aT, actualTemperature, actualPressure) - 2.0 * GetBZ(bT, actualTemperature, actualPressure) - 3.0 * Math.Pow(GetBZ(bT, actualTemperature, actualPressure), 2.0)) * cprFactor
            - (GetAZ(aT, actualTemperature, actualPressure) * GetBZ(bT, actualTemperature, actualPressure) - Math.Pow(GetBZ(bT, actualTemperature, actualPressure), 2.0) - Math.Pow(GetBZ(bT, actualTemperature, actualPressure), 3.0));

        public override double DoDerivFuncZnCal(double cprFactor, double aT, double bT, double actualTemperature, double actualPressure)
            => 3.0 * Math.Pow(cprFactor, 2.0)
            - 2.0 * (1 - GetBZ(bT, actualTemperature, actualPressure)) * cprFactor
            + (GetAZ(aT, actualTemperature, actualPressure) - 2.0 * GetBZ(bT, actualTemperature, actualPressure) - 3.0 * Math.Pow(GetBZ(bT, actualTemperature, actualPressure), 2.0));
        /// <summary>
        /// 逸度系数
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns>逸度系数 φ</returns>
        public override double GetFugacityCoe(Chemical chemical, double cprFactor, double temperature, double pressure)
            => Math.Exp((cprFactor - 1.0)
            - Math.Log(cprFactor - GetBZ(chemical, temperature, pressure))
            - Math.Log((cprFactor + (Math.Sqrt(2.0) + 1.0) * GetBZ(chemical, temperature, pressure)) / (cprFactor - (Math.Sqrt(2.0) - 1.0) * GetBZ(chemical, temperature, pressure)))
               * GetAZ(chemical, temperature, pressure) / Math.Pow(2.0, 1.5) / GetBZ(chemical, temperature, pressure));
        public double GetFugacityCoe(double az, double bz, double cprFactor, double temperature, double pressure)
            => Math.Exp((cprFactor - 1.0)
                - Math.Log(cprFactor - bz)
                - Math.Log((cprFactor + (Math.Sqrt(2.0) + 1.0) * bz) / (cprFactor - (Math.Sqrt(2.0) - 1.0) * bz))
                * az / Math.Pow(2.0, 1.5) / bz);
        /// <summary>
        /// 剩余焓
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns></returns>
        public override double GetResidualEnthalpy(Chemical chemical, double cprFactor, double temperature, double pressure)
            => cprFactor - 1.0
                - (GetAt(chemical, temperature)
                + (0.374635 + 1.54226 * chemical.AcentricFactor - 0.26992 * Math.Pow(chemical.AcentricFactor, 2.0))
                    * Math.Sqrt(GetAc(chemical) * GetAt(chemical, temperature) * temperature / chemical.CriticalTemperature))
                * Math.Log((cprFactor + (Math.Sqrt(2.0) + 1.0) * GetBZ(chemical, temperature, pressure)) / (cprFactor - (Math.Sqrt(2.0) - 1.0) * GetBZ(chemical, temperature, pressure)))
                / Math.Pow(2.0, 1.5) / GetBt(chemical) / 8.314 / temperature;
        /// <summary>
        /// 剩余熵
        /// </summary>  
        /// <param name="chemical">化合物</param>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns></returns>
        public override double GetResidualEntropy(Chemical chemical, double cprFactor, double temperature, double pressure)
            => Math.Log(cprFactor - GetBZ(chemical, temperature, pressure))
                - Math.Log((cprFactor + (Math.Sqrt(2.0) + 1.0) * GetBZ(chemical, temperature, pressure)) / (cprFactor - (Math.Sqrt(2.0) - 1.0) * GetBZ(chemical, temperature, pressure)))
                    * (0.374635 + 1.54226 * chemical.AcentricFactor - 0.26992 * Math.Pow(chemical.AcentricFactor, 2.0))
                    * Math.Sqrt(GetAc(chemical) * GetAt(chemical, temperature) / (temperature * chemical.CriticalTemperature))
                    / Math.Pow(2.0, 1.5) / GetBt(chemical) / 8.314;
    }
}
/// <summary>
/// 立方型状态方程计算摩尔体积 中介者
/// </summary>
namespace ChemEngThermCal.Model.Ceos.CalMoleVolume {
    public abstract class ResultBase : IGasResult {
        public ResultBase(double at, double AZ, double bt, double BZ, Models.CubicEosType type, double T, double P, double Z, double V, bool isAborted, bool isConverged) {
            Ceos_At = at;
            Ceos_AZ = AZ;
            Ceos_Bt = bt;
            Ceos_BZ = BZ;
            ModelType = type;
            GasResult = new Model.ResultBase(T, P, V, Z, isAborted, isConverged);
        }
        /// <summary>
        /// a(T)
        /// </summary>
        public double Ceos_At { get; private set; }
        /// <summary>
        /// b(T)
        /// </summary>
        public double Ceos_Bt { get; private set; }
        /// <summary>
        /// A(Z)
        /// </summary>
        public double Ceos_AZ { get; private set; }
        /// <summary>
        /// B(Z)
        /// </summary>
        public double Ceos_BZ { get; private set; }
        /// <summary>
        /// 模型类别
        /// </summary>
        public Models.CubicEosType ModelType { get; private set; }
        /// <summary>
        /// 气相结果
        /// </summary>
        public Model.ResultBase GasResult { get; private set; }
    }
    /// <summary>
    /// 立方型状态方程基类型中介器
    /// </summary>
    public abstract class CeosBaseMediator {
        public CeosBaseMediator(Models.CeosBaseModel model, double actualTemperature, double actualPressure) : base() {
            Atmos_T = actualTemperature;
            Atmos_P = actualPressure;
            CeosType = model.EqtType;
            CeosModel = model;
        }
        /// <summary>
        /// 指定模型类型的枚举
        /// </summary>
        public Models.CubicEosType CeosType { get; private set; }
        /// <summary>
        /// 环境压力，MPa
        /// </summary>
        public double Atmos_P { get; private set; }
        /// <summary>
        /// 环境温度，K
        /// </summary>
        public double Atmos_T { get; private set; }
        //所持有的模型对象
        protected Models.CeosBaseModel CeosModel { get; private set; }
    }
    /// <summary>
    /// 纯组分Ceos 计算中介器
    /// </summary>
    public class CeosPureMediator
        : CeosBaseMediator, MixRules.IPureFluidModel, Algorithm.INewtonRaphsonItrMediator {
        public CeosPureMediator(Models.CeosBaseModel model, Chemical chemical, double actualTemperature, double actualPressure)
            : base(model, actualTemperature, actualPressure) {
            _chemical = chemical;
        }
        /// <summary>
        /// 计算结果的实例
        /// </summary>
        public class Result : ResultBase, ILiqResult {
            /// <summary>
            /// Ceos 纯物质计算结果
            /// </summary>
            /// <param name="T">环境温度</param>
            /// <param name="P">环境压力</param>
            /// <param name="gasV">气相摩尔体积</param>
            /// <param name="gasZ">气相压缩因子</param>
            /// <param name="isGasConverged">气相是否收敛</param>
            /// <param name="gasStep">气相迭代数据</param>
            /// <param name="liqV">液相摩尔体积</param>
            /// <param name="liqZ">液相压缩因子</param>
            /// <param name="isLiqConverged"></param>
            /// <param name="liqStep">液相迭代数据</param>
            /// <param name="at">at</param>
            /// <param name="AZ">AZ</param>
            /// <param name="bt">bt</param>
            /// <param name="BZ">BZ</param>
            /// <param name="chemical">化合物</param>
            /// <param name="model">模型</param>
            /// <param name="isLiquidExist">液相是否存在</param>
            /// <param name="isLiqApplicable">模型是否可用于液相</param>
            public Result(double T, double P,
                double gasV, double gasZ, bool isGasConverged, List<CeosStepInfo> gasStep,
                double liqV, double liqZ, bool isLiqConverged, List<CeosStepInfo> liqStep,
                double at, double AZ, double bt, double BZ, Chemical chemical, Models.CeosBaseModel model, bool isLiquidExist, bool isLiqApplicable)
                : base(at, AZ, bt, BZ, model.EqtType, T, P, gasZ, gasV, false, isGasConverged) {
                TargetChemical = chemical;
                GasStep = gasStep;
                IsLiquidPhaseExist = isLiquidExist;
                isLiquidPhaseApplicable = isLiqApplicable;
                LiqStep = liqStep;
                LiqResult = new Model.ResultBase(T, P, liqV, liqZ, false, isLiqConverged);
            }
            /// <summary>
            /// 气相迭代数据
            /// </summary>
            public List<CeosStepInfo> GasStep { get; private set; }
            /// <summary>
            /// 液相迭代数据
            /// </summary>
            public List<CeosStepInfo> LiqStep { get; private set; }
            /// <summary>
            /// 液相结果是否可用
            /// </summary>
            public bool isLiquidPhaseApplicable { get; private set; }
            /// <summary>
            /// 液相是否存在
            /// </summary>
            public bool IsLiquidPhaseExist { get; private set; }
            /// <summary>
            /// 化合物
            /// </summary>
            public Chemical TargetChemical { get; private set; }
            /// <summary>
            /// 液相计算结果
            /// </summary>
            public Model.ResultBase LiqResult { get; private set; }
        }
        /// <summary>
        /// 进行计算并获取结果
        /// </summary>
        /// <returns></returns>
        public Result GetResult() {
            //液相是否存在
            bool isLiqExist;
            if (Atmos_T > _chemical.CriticalTemperature) {
                isLiqExist = false;
            } else {
                isLiqExist = true;
            }
            //模型是否可用于液相计算
            bool isLiqApplicable = CeosModel.IsLiqPhaseApplicable;
            //重要方程参数
            double at = CeosModel.GetAt(_chemical, Atmos_T);
            double az = CeosModel.GetAZ(_chemical, Atmos_T, Atmos_P);
            double bt = CeosModel.GetBt(_chemical);
            double bz = CeosModel.GetBZ(_chemical, Atmos_T, Atmos_P);
            //气相计算
            Algorithm.NewtonRaphsonIterator gasCal = new Algorithm.NewtonRaphsonIterator(this, 1.0, 0.0001, 100);
            double gasCpr;
            double gasVol;
            gasCpr = gasCal.Result;
            bool isGasConverged;
            if (gasCpr == -1.0 || gasCpr == -2.0) {
                gasVol = gasCpr;
                isGasConverged = false;
            } else {
                gasVol = CeosModel.GetMoleVolume(gasCpr, Atmos_T, Atmos_P);
                isGasConverged = true;
            }
            List<CeosStepInfo> gasStepInfo = new List<CeosStepInfo> { };
            foreach (Algorithm.NewtonRaphsonIterator.StepInfo rawStep in gasCal.StepInfoList) {
                gasStepInfo.Add(new CeosStepInfo(rawStep, Atmos_T, Atmos_P));
            }
            List<CeosStepInfo> liqStepInfo = new List<CeosStepInfo> { };
            double liqCpr;
            double liqVol;
            bool isLiqConverged;
            //液相计算
            if (isLiqExist && isLiqApplicable) {
                Algorithm.NewtonRaphsonIterator liqCal = new Algorithm.NewtonRaphsonIterator(this, CeosModel.GetBt(_chemical) * Atmos_P / 8.314 / Atmos_T, 0.0001, 1000);
                liqCpr = liqCal.Result;
                if (liqCpr == -1.0 || liqCpr == -2.0) {
                    liqVol = liqCpr;
                    isLiqConverged = false;
                } else {
                    liqVol = CeosModel.GetMoleVolume(liqCpr, Atmos_T, Atmos_P);
                    isLiqConverged = true;
                }
                foreach (Algorithm.NewtonRaphsonIterator.StepInfo rawStep in liqCal.StepInfoList) {
                    liqStepInfo.Add(new CeosStepInfo(rawStep, Atmos_T, Atmos_P));
                }
            } else {
                liqCpr = -3.0;
                liqVol = -3.0;
                isLiqConverged = false;
            }
            return new Result(Atmos_T, Atmos_P, gasVol, gasCpr, isGasConverged, gasStepInfo, liqVol, liqCpr, isLiqConverged, liqStepInfo, at, az, bt, bz, _chemical, CeosModel, isLiqExist, isLiqApplicable);
        }
        #region Algorithm.INewtonRaphsonItrMediator 接口的显式实现
        double Algorithm.INewtonRaphsonItrMediator.DoFuncCal(double x)
           => CeosModel.DoFuncZnCal(x, CeosModel.GetAt(_chemical, Atmos_T), CeosModel.GetBt(_chemical), Atmos_T, Atmos_P);
        double Algorithm.INewtonRaphsonItrMediator.DoDerivFuncCal(double x)
            => CeosModel.DoDerivFuncZnCal(x, CeosModel.GetAt(_chemical, Atmos_T), CeosModel.GetBt(_chemical), Atmos_T, Atmos_P);
        #endregion
        #region MixRules.IPureFluidModel 接口的显式实现
        protected Material.Chemical _chemical;
        /// <summary>
        /// 目标化合物
        /// </summary>
        Material.Chemical MixRules.IPureFluidModel.Chemical
           => _chemical;
        #endregion
    }
    /// <summary>
    /// 气体混合Ceos 计算中介器
    /// </summary>
    public class CeosGasMixMediator : CeosBaseMediator, MixRules.ICeosMixMediator, Algorithm.INewtonRaphsonItrMediator {
        public CeosGasMixMediator(Models.CeosBaseModel model, BinaryComplex gasComplex, double actualTemperature, double actualPressure, double binCorIndex = 0)
            : base(model, actualTemperature, actualPressure) {
            _complex = gasComplex;
            _binCorIndex = binCorIndex;
        }
        /// <summary>
        /// 计算结果的实例
        /// </summary>
        public class Result : ResultBase {
            /// <summary>
            ///  Ceos 真实气体混合物
            /// </summary>
            /// <param name="T">环境温度</param>
            /// <param name="P">环境压力</param> 
            /// <param name="gasV">气相摩尔体积</param>
            /// <param name="gasZ">气相压缩因子</param>
            /// <param name="isGasConverged">气相是否收敛</param>
            /// <param name="gasStep">气相迭代数据</param>
            /// <param name="at">a(T)</param>
            /// <param name="AZ">A(Z)</param>
            /// <param name="bt">b(T)</param>
            /// <param name="BZ">B(T)</param>
            /// <param name="complex">混合物</param>
            /// <param name="model">模型</param>
            /// <param name="bcIndex">二元相互作用参数</param>
            /// <param name="at11">a(T)11</param>
            /// <param name="at12">a(T)12</param>
            /// <param name="at22">a(T)22</param>
            /// <param name="bt1">b(T)1</param>
            /// <param name="bt2">b(T)2</param>
            public Result(double T, double P,
                double gasV, double gasZ, bool isGasConverged, List<CeosStepInfo> gasStep,
                double at, double AZ, double bt, double BZ,
                BinaryComplex complex, Models.CeosBaseModel model, double bcIndex, double at11, double at12, double at22, double bt1, double bt2)
                : base(at, AZ, bt, BZ, model.EqtType, T, P, gasZ, gasV, false, isGasConverged) {
                TargetComplex = complex;
                BinaryCorrelationIndex = bcIndex;
                Ceos_At11 = at11;
                Ceos_At12 = at12;
                Ceos_At22 = at22;
                Ceos_Bt1 = bt1;
                Ceos_Bt2 = bt2;
                GasStep = gasStep;
            }
            /// <summary>
            /// 二元相互作用参数
            /// </summary>
            public double BinaryCorrelationIndex { get; private set; }
            /// <summary>
            /// a(T)11
            /// </summary>
            public double Ceos_At11 { get; private set; }
            /// <summary>
            /// a(T)12
            /// </summary>
            public double Ceos_At12 { get; private set; }
            /// <summary>
            /// a(T)22
            /// </summary>
            public double Ceos_At22 { get; private set; }
            /// <summary>
            /// b(T)1
            /// </summary>
            public double Ceos_Bt1 { get; private set; }
            /// <summary>
            /// b(T)2
            /// </summary>
            public double Ceos_Bt2 { get; private set; }
            /// <summary>
            /// 气相迭代数据
            /// </summary>
            public List<CeosStepInfo> GasStep { get; private set; }
            /// <summary>
            /// 混合物
            /// </summary>
            public BinaryComplex TargetComplex { get; private set; }
        }
        /// <summary>
        /// 获取计算结果
        /// </summary> 
        public Result GetResult() {
            MixRules.CeosBinMixRule mixRule = new MixRules.CeosBinMixRule(this, Atmos_T);
            double[] assistIndexes = mixRule.GetComponentProps();
            double at11 = assistIndexes[0];
            double at12 = assistIndexes[1];
            double at22 = assistIndexes[2];
            double at = mixRule.Mix_aT;
            double Az = CeosModel.GetAZ(at, Atmos_T, Atmos_P);
            double bt1 = assistIndexes[3];
            double bt2 = assistIndexes[4];
            double bt = mixRule.Mix_bT;
            double Bz = CeosModel.GetBZ(bt, Atmos_T, Atmos_P);
            double bcIndex = _binCorIndex;
            Algorithm.NewtonRaphsonIterator calProc = new Algorithm.NewtonRaphsonIterator(this, 1.0, 0.0001, 100);
            double gasCpr, gasVol;
            bool isGasConverged;
            gasCpr = calProc.Result;
            if (gasCpr == -1.0 || gasCpr == -2.0) {
                gasVol = gasCpr;
                isGasConverged = false;
            } else {
                gasVol = CeosModel.GetMoleVolume(gasCpr, Atmos_T, Atmos_P);
                isGasConverged = true;
            }
            List<CeosStepInfo> gasStepInfo = new List<CeosStepInfo> { };
            foreach (Algorithm.NewtonRaphsonIterator.StepInfo rawStepInfo in calProc.StepInfoList) {
                gasStepInfo.Add(new CeosStepInfo(rawStepInfo, Atmos_T, Atmos_P));
            }
            return new Result(Atmos_T, Atmos_P, gasVol, gasCpr, isGasConverged, gasStepInfo, at, Az, bt, Bz, _complex, CeosModel, bcIndex, at11, at12, at22, bt1, bt2);
        }
        #region Algorithm.INewtonRaphsonItrMediator 接口的显式实现
        double Algorithm.INewtonRaphsonItrMediator.DoFuncCal(double x)
            => CeosModel.DoFuncZnCal(x, new MixRules.CeosBinMixRule(this, Atmos_T).Mix_aT, new MixRules.CeosBinMixRule(this, Atmos_T).Mix_bT, Atmos_T, Atmos_P);
        double Algorithm.INewtonRaphsonItrMediator.DoDerivFuncCal(double x)
            => CeosModel.DoDerivFuncZnCal(x, new MixRules.CeosBinMixRule(this, Atmos_T).Mix_aT, new MixRules.CeosBinMixRule(this, Atmos_T).Mix_bT, Atmos_T, Atmos_P);
        #endregion
        #region 实现 MixRules.ICeosMixMediator
        //二元混合物
        protected BinaryComplex _complex;
        /// <summary>
        /// 目标气体混合物
        /// </summary>
        Material.BinaryComplex MixRules.IMixFluidModel.Complex
           => _complex;
        protected double _binCorIndex;
        /// <summary>
        /// 二元相互作用参数
        /// </summary>
        double MixRules.IBinCorINdexMixMediator.BinCorIndex
            => _binCorIndex;
        /// <summary>
        /// Ceos模型
        /// </summary>
        /// <returns>_targetModel</returns>
        public Models.CeosBaseModel GetTargetModel()
            => CeosModel;
        #endregion
    }
    /// <summary>
    /// Cubic Eos 的 迭代步骤记录器类型
    /// </summary>
    public class CeosStepInfo {
        /// <summary>           
        /// 实例化一个用于 Cubic Eos 的 迭代步骤记录
        /// </summary>
        /// <param name="compressibilityFactor">压缩因子</param>
        /// <param name="func">方程函数计算结果</param>
        /// <param name="derivFunc">方程导函数计算结果</param>
        /// <param name="diff">迭代差值</param>
        /// <param name="nextCompressibilityFactor">后一步压缩因子</param>
        /// <param name="volume">摩尔体积</param>
        /// <param name="nextVolume">后一步摩尔体积</param>
        public CeosStepInfo(Algorithm.NewtonRaphsonIterator.StepInfo rawStepInfo, double actualTemperature, double actualPressure) {
            StepCprFactor = rawStepInfo.Value;
            StepFunc = rawStepInfo.FuncValue;
            StepDerivFunc = rawStepInfo.DerivFuncValue;
            StepDiff = rawStepInfo.DiffValue;
            StepNextCprFactor = rawStepInfo.NextValue;
            StepVolume = StepCprFactor * 8.314 * actualTemperature / actualPressure;
            StepNextVolume = StepNextCprFactor * 8.314 * actualTemperature / actualPressure;
        }
        #region 所记录的计算步骤的 属性
        /// <summary>
        /// 当前步骤的压缩因子
        /// </summary>
        public double StepCprFactor { get; internal set; }
        /// <summary>
        /// 当前步骤方程函数计算结果
        /// </summary>
        public double StepFunc { get; internal set; }
        /// <summary>
        /// 当前步骤方程导函数计算结果
        /// </summary>
        public double StepDerivFunc { get; internal set; }
        /// <summary>
        /// 当前步骤迭代差值
        /// </summary>
        public double StepDiff { get; internal set; }
        /// <summary>
        /// 当前步骤 迭代后 的压缩因子
        /// </summary>
        public double StepNextCprFactor { get; internal set; }
        /// <summary>
        /// 当前步骤摩尔体积
        /// </summary>
        public double StepVolume { get; internal set; }
        /// <summary>
        /// 当前步骤 迭代后 的摩尔体积
        /// </summary>
        public object StepNextVolume { get; internal set; }
        #endregion
    }
}