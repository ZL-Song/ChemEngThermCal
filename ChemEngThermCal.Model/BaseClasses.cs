using System;
using System.Collections.Generic;

namespace ChemEngThermCal.Model {
    /// <summary>
    /// 计算结果的基类型
    /// </summary>
    public class ResultBase {
        /// <summary>
        /// 计算结果的基类型
        /// </summary>
        /// <param name="T">温度，K</param>
        /// <param name="P">压力，MPa</param>
        /// <param name="V">摩尔体积，cm3/mol</param>
        /// <param name="Z">压缩因子，1</param>
        /// <param name="isAborted">计算是否被放弃</param>
        /// <param name="isConverged">计算是否收敛</param>
        public ResultBase(double T, double P, double V, double Z, bool isAborted, bool isConverged) {
            Temperature = T;
            Pressure = P;
            MoleVol = V;
            CprFactor = Z;
            IsCalcAborted = isAborted;
            IsConverged = isConverged;
        }
        /// <summary>
        /// 全局压缩因子
        /// </summary>
        public double CprFactor { get; private set; }
        /// <summary>
        /// 全局摩尔体积
        /// </summary>
        public double MoleVol { get; private set; }
        /// <summary>
        /// 全局压力
        /// </summary>
        public double Pressure { get; private set; }
        /// <summary>
        /// 全局温度
        /// </summary>
        public double Temperature { get; private set; }
        /// <summary>
        /// 计算是否被放弃
        /// </summary>
        public bool IsCalcAborted { get; private set; }
        /// <summary>
        /// 计算是否收敛
        /// </summary>
        public bool IsConverged { get; private set; }
    }
    /// <summary>
    /// 包含气相计算结果
    /// </summary>
    public interface IGasResult {
        ResultBase GasResult { get; }
    }
    /// <summary>
    /// 包含液相计算结果
    /// </summary>
    public interface ILiqResult {
        ResultBase LiqResult { get; }
    }

    public class VleResultBase : ResultBase {
        public VleResultBase(double T, double P, double V, double Z, bool isAborted, bool isConverged, double fugacity, double residualEnthalpy, double residualEntropy)
            : base(T, P, V, Z, isAborted, isConverged) {
            Fugacity = fugacity;
            ResidualEnthalpy = residualEnthalpy;
            ResidualEntropy = residualEntropy;
        }
        /// <summary>
        /// 逸度
        /// </summary>
        public double Fugacity { get; private set; }
        /// <summary>
        /// 剩余焓
        /// </summary>
        public double ResidualEnthalpy { get; private set; }
        /// <summary>
        /// 剩余熵
        /// </summary>
        public double ResidualEntropy { get; private set; }
    }
}

namespace ChemEngThermCal.Model.Material {
    /// <summary>
    /// 化合物
    /// </summary>
    public class Chemical {
        /// <summary>
        /// 初始化一个化合物对象
        /// </summary>
        /// <param name="criticalTemperature">临界温度,Tc</param>
        /// <param name="criticalPressure">临界压力，Pc</param>
        /// <param name="acentricFactor">偏心因子, w</param>
        /// <param name="criticalVolume">临界摩尔体积，Vc</param>
        /// <param name="criticalCprFactor">临界压缩因子，Zc</param>
        /// <param name="moleFraction">摩尔分率，1</param>
        public Chemical(double criticalTemperature, double criticalPressure,
            double acentricFactor = 0, double criticalVolume = 0, double criticalCprFactor = 0,
            double antoineA = 1, double antoineB = 1, double antoineC = 1) {
            _criticalTemperature = criticalTemperature;
            _criticalPressure = criticalPressure;
            _acentricFactor = acentricFactor;
            _criticalVolume = criticalVolume;
            _criticalCprFactor = criticalCprFactor;
            _antoinePara = new Antoine.AntParaGroup(antoineA, antoineB, antoineC);
        }
        //临界温度,Tc
        private double _criticalTemperature;
        /// <summary>
        /// 临界温度，Tc
        /// </summary>
        public double CriticalTemperature => _criticalTemperature;
        //临界压力，Pc
        private double _criticalPressure;
        /// <summary>
        /// 临界压力，Pc
        /// </summary>      
        public double CriticalPressure => _criticalPressure;
        //偏心因子, w
        private double _acentricFactor;
        /// <summary>
        /// 偏心因子 w
        /// </summary>
        public double AcentricFactor => _acentricFactor;
        //临界摩尔体积，Vc
        private double _criticalVolume;
        /// <summary>
        /// 临界摩尔体积，Vc
        /// </summary>
        public double CriticalVolume => _criticalVolume;
        //临界压缩因子，Zc
        private double _criticalCprFactor;
        /// <summary>
        /// 临界压缩因子，Zc
        /// </summary>
        public double CriticalCprFactor => _criticalCprFactor;
        //Antoine 方程系数组
        private Antoine.AntParaGroup _antoinePara;
        /// <summary>
        /// 设定 Antoine 方程系数
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <param name="c">C</param>
        public void SetAntoinePara(double a, double b, double c) {
            _antoinePara = new Antoine.AntParaGroup(a, b, c);
        }
        /// <summary>
        /// 获取 Antoine 方程系数组
        /// </summary>
        /// <returns>Antoine 方程系数组</returns>
        public Antoine.AntParaGroup GetAntoinePara()
            => _antoinePara;
    }
    /// <summary>
    /// 二元混合物
    /// </summary>
    public class BinaryComplex {
        /// <summary>
        /// 实例化一个 二元混合体系
        /// </summary>
        /// <param name="fstChemical">第一组分</param>
        /// <param name="secChemical">第二组分</param>
        /// <param name="moleRatio">摩尔比：第一化合物 / 第二化合物</param>
        /// <param name="actPara12">活度参数 A12 默认值为 1</param>
        /// <param name="actPara21">活度参数 A21 默认值为 1<</param>
        public BinaryComplex(Chemical fstChemical, Chemical secChemical, double moleRatio) {
            FstComponent = fstChemical;
            SecComponent = secChemical;
            moleRatio = Math.Abs(moleRatio);
            FstComponentMoleFraction = moleRatio / (moleRatio + 1.0);
            SecComponentMoleFraction = 1.0 / (moleRatio + 1.0);
        }
        /// <summary>
        /// 第一组分
        /// </summary>
        public Chemical FstComponent { get; private set; }
        /// <summary>
        /// 第二组分
        /// </summary>
        public Chemical SecComponent { get; private set; }
        /// <summary>
        /// 获取 第一组分 的摩尔分率
        /// </summary>
        public double FstComponentMoleFraction { get; private set; }
        /// <summary>
        /// 获取 第二组分 的摩尔分率
        /// </summary>
        public double SecComponentMoleFraction { get; private set; }
    }
}
/// <summary>
/// 热力学混合规则
/// </summary>
namespace ChemEngThermCal.Model.MixRules {
    /// <summary>
    /// 表示 纯物质计算模型 的实例，此抽象类是所有纯物系计算模型的父类
    /// </summary>
    public interface IPureFluidModel {
        /// <summary>
        /// 化合物
        /// </summary>
        Material.Chemical Chemical { get; }
    }
    /// <summary>
    /// 表示 混合物计算模型 的实例，此抽象类是所有混合物系计算模型的父类
    /// </summary>
    public interface IMixFluidModel {
        /// <summary>
        /// 化合物的数组
        /// </summary>
        Material.BinaryComplex Complex { get; }
    }
    /// <summary>
    /// Ceos 混合规则的接口
    /// </summary>
    public interface ICeosMixMediator : IBinCorINdexMixMediator {
        Ceos.Models.CeosBaseModel GetTargetModel();
    }
    /// <summary>
    /// Ceos 混合规则
    /// </summary>
    public class CeosBinMixRule {
        public CeosBinMixRule(ICeosMixMediator mediator, double actualTemperature) {
            _mediator = mediator;
            _mix_aT11 = _mediator.GetTargetModel().GetAt(_mediator.Complex.FstComponent, actualTemperature);
            _mix_aT22 = _mediator.GetTargetModel().GetAt(_mediator.Complex.SecComponent, actualTemperature);
            _mix_aT12 = Math.Sqrt(_mix_aT11 * _mix_aT22) * (1 - _mediator.BinCorIndex);
            _mix_bT1 = _mediator.GetTargetModel().GetBt(_mediator.Complex.FstComponent);
            _mix_bT2 = _mediator.GetTargetModel().GetBt(_mediator.Complex.SecComponent);
        }
        //a(T)11,a(T)12,a(T)22,b(T)1,b(T)2
        private double _mix_aT11, _mix_aT12, _mix_aT22, _mix_bT1, _mix_bT2;
        //中介者
        private ICeosMixMediator _mediator;
        /// <summary>
        /// a(T)m——V
        /// </summary>
        public double Mix_aT
            => Math.Pow(_mediator.Complex.FstComponentMoleFraction, 2.0) * _mix_aT11
            + 2.0 * _mediator.Complex.FstComponentMoleFraction * _mediator.Complex.SecComponentMoleFraction * _mix_aT12
            + Math.Pow(_mediator.Complex.SecComponentMoleFraction, 2.0) * _mix_aT22;
        /// <summary>
        /// b(T)m——V
        /// </summary>
        public double Mix_bT
            => _mediator.Complex.FstComponentMoleFraction * _mix_bT1 + _mediator.Complex.SecComponentMoleFraction * _mix_bT2;
        /// <summary>
        /// 数组：a(T)11 a(T)12 a(T)22 b(T)1 b(T)2
        /// </summary>
        /// <returns>a(T)11 a(T)12 a(T)22 b(T)1 b(T)2</returns>
        public double[] GetComponentProps()
            => new double[5] {
                _mix_aT11,
                _mix_aT12,
                _mix_aT22,
                _mix_bT1,
                _mix_bT2
            };
    }
    /// <summary>
    /// Kay 混合规则
    /// </summary>
    public class KayMixRule {
        public KayMixRule(IMixFluidModel mediator) {
            _mediator = mediator;
        }
        //中介者
        private IMixFluidModel _mediator;
        /// <summary>
        /// Tcm
        /// </summary>
        public double Mix_Tc
            => _mediator.Complex.FstComponent.CriticalTemperature * _mediator.Complex.FstComponentMoleFraction
            + _mediator.Complex.SecComponent.CriticalTemperature * _mediator.Complex.SecComponentMoleFraction;
        /// <summary>
        /// Pcm
        /// </summary>
        public double Mix_Pc
            => _mediator.Complex.FstComponent.CriticalPressure * _mediator.Complex.FstComponentMoleFraction
            + _mediator.Complex.SecComponent.CriticalPressure * _mediator.Complex.SecComponentMoleFraction;
        /// <summary>
        /// wm
        /// </summary>
        public double Mix_w
            => _mediator.Complex.FstComponent.AcentricFactor * _mediator.Complex.FstComponentMoleFraction
            + _mediator.Complex.SecComponent.AcentricFactor * _mediator.Complex.SecComponentMoleFraction;
    }
    /// <summary>
    /// 需要 二元相互作用参数 的混合规则的交互所需要实现的接口
    /// </summary>
    public interface IBinCorINdexMixMediator : IMixFluidModel {
        double BinCorIndex { get; }
    }
    /// <summary>
    /// 与 SecVirial 混合规则交互所需要的接口
    /// </summary>
    public interface ISecVirialMixMediator : IBinCorINdexMixMediator {
        CorStt.Models.SecVirial.SecVirialBaseModel GetTargetModel();
    }
    /// <summary>
    /// SecVirial 混合规则
    /// </summary>
    public class SecVirialRule {
        public SecVirialRule(ISecVirialMixMediator mediator) {
            _mediator = mediator;
        }
        private double _mix_B_11, _mix_B0_11, _mix_B1_11, _mix_B_12, _mix_B0_12, _mix_B1_12, _mix_B_22, _mix_B0_22, _mix_B1_22, _mix_Bm;
        //中介者
        private ISecVirialMixMediator _mediator;
        /// <summary>
        /// Tc12
        /// </summary>
        public double Mix_Tc
            => Math.Sqrt(_mediator.Complex.FstComponent.CriticalTemperature * _mediator.Complex.SecComponent.CriticalTemperature) * (1.0 - _mediator.BinCorIndex);
        /// <summary>
        /// Vc12
        /// </summary>
        public double Mix_Vc
            => Math.Pow((Math.Pow(_mediator.Complex.FstComponent.CriticalVolume, 1.0 / 3.0) + Math.Pow(_mediator.Complex.SecComponent.CriticalVolume, 1.0 / 3.0)) / 2.0, 3.0);
        /// <summary>
        /// Zc12
        /// </summary>
        public double Mix_Zc
            => (_mediator.Complex.FstComponent.CriticalCprFactor + _mediator.Complex.SecComponent.CriticalCprFactor) / 2.0;
        /// <summary>
        /// w12
        /// </summary>
        public double Mix_w
            => (_mediator.Complex.FstComponent.AcentricFactor + _mediator.Complex.SecComponent.AcentricFactor) / 2.0;
        /// <summary>
        /// Pc12
        /// </summary>
        public double Mix_Pc
            => (Mix_Zc * 8.314 * Mix_Tc) / Mix_Vc;
        /// <summary>
        /// [ B0_11, B1_11, B_11, B0_22, B1_22, B_22, B0_12, B1_12, B_12 ,Bm ]
        /// </summary>
        public double[] GetRelevants(double temperature) {
            double n1 = _mediator.Complex.FstComponent.CriticalTemperature / _mediator.Complex.FstComponent.CriticalPressure * 8.314;
            double n2 = _mediator.Complex.SecComponent.CriticalTemperature / _mediator.Complex.SecComponent.CriticalPressure * 8.314;
            _mix_B0_11 = _mediator.GetTargetModel().GetSecVirialCoeBase(temperature / _mediator.Complex.FstComponent.CriticalTemperature);
            _mix_B1_11 = _mediator.GetTargetModel().GetSecVirialCoeCrec(temperature / _mediator.Complex.FstComponent.CriticalTemperature);
            _mix_B_11 = _mediator.GetTargetModel().GetSecVirialCoe(temperature / _mediator.Complex.FstComponent.CriticalTemperature, Mix_w) * n1;
            _mix_B0_12 = _mediator.GetTargetModel().GetSecVirialCoeBase(temperature / Mix_Tc);
            _mix_B1_12 = _mediator.GetTargetModel().GetSecVirialCoeCrec(temperature / Mix_Tc);
            _mix_B_12 = _mediator.GetTargetModel().GetSecVirialCoe(temperature / Mix_Tc, Mix_w) * 8.314 * Mix_Tc / Mix_Pc;
            _mix_B0_22 = _mediator.GetTargetModel().GetSecVirialCoeBase(temperature / _mediator.Complex.SecComponent.CriticalTemperature);
            _mix_B1_22 = _mediator.GetTargetModel().GetSecVirialCoeCrec(temperature / _mediator.Complex.SecComponent.CriticalTemperature);
            _mix_B_22 = _mediator.GetTargetModel().GetSecVirialCoe(temperature / _mediator.Complex.SecComponent.CriticalTemperature, Mix_w)*n2;
            _mix_Bm = Math.Pow(_mediator.Complex.FstComponentMoleFraction, 2.0) * _mix_B_11 +
            2.0 * _mediator.Complex.FstComponentMoleFraction * _mediator.Complex.SecComponentMoleFraction * _mix_B_12
            + Math.Pow(_mediator.Complex.SecComponentMoleFraction, 2.0) * _mix_B_22;
            return new double[10] {
                _mix_B0_11, _mix_B1_11, _mix_B_11,
                _mix_B0_22, _mix_B1_22, _mix_B_22,
                _mix_B0_12, _mix_B1_12, _mix_B_12,
                _mix_Bm
            };
        }

    }
}