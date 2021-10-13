using System;
using System.Collections.Generic;
using ChemEngThermCal.Model.Material;

namespace ChemEngThermCal.Model.CorStt {
    /// <summary>
    /// 普遍化关联式计算结果的基类型
    /// </summary>
    public abstract class ResultBase : IGasResult {
        /// <summary>
        /// 普遍化关联式计算结果的基类型
        /// </summary>
        /// <param name="T">环境温度</param>
        /// <param name="P">环境压力</param>
        /// <param name="gasV">气相摩尔体积</param>
        /// <param name="gasZ">气相压缩因子</param>
        /// <param name="isGasAborted">气相计算是否被放弃</param>
        /// <param name="isGasConverged">气相结果是否收敛</param>
        public ResultBase(double T, double P, double gasV, double gasZ, bool isGasAborted, bool isGasConverged) {
            GasResult = new Model.ResultBase(T, P, gasV, gasZ, isGasAborted, isGasConverged);
        }
        /// <summary>
        /// 气相结果
        /// </summary>
        public Model.ResultBase GasResult { get; private set; }
    }
}
/// <summary>
/// 普遍化关联式 模型
/// </summary>
namespace ChemEngThermCal.Model.CorStt.Models {
    /// <summary>
    /// 对应态原理 抽象基类
    /// </summary>
    public abstract class CorspdStateBaseModel {
        /// <summary>
        /// 获取气相压力
        /// </summary>
        /// <param name="cprFactor">压缩因子</param>
        /// <param name="temperature">温度</param>
        /// <param name="moleVol">摩尔体积</param>
        public double GetGasPressure(double cprFactor, double temperature, double moleVol)
            => cprFactor * 8.314 * temperature / moleVol;
        /// <summary>
        /// V =
        /// </summary>
        /// <param name="cprFactor">Z</param>
        /// <returns>V</returns>
        public double GetGasMoleVolume(double cprFactor, double actualTemperature, double actualPressure)
            => cprFactor * 8.314 * actualTemperature / actualPressure;
        /// <summary>
        /// 获取相对性质 Xr =
        /// </summary>
        /// <param name="actualProperty">实际性质</param>
        /// <param name="criticalProperty">临界性质</param> 
        public double GetReletiveProperty(double actualProperty, double criticalProperty)
            => actualProperty / criticalProperty;
    }
}
/// <summary>
/// SecVirial法计算模型
/// </summary>
namespace ChemEngThermCal.Model.CorStt.Models.SecVirial {
    /// <summary>
    /// SecVirial 模型要进行逸度计算所必须实现的接口
    /// </summary>
    public interface ISecVirialFugacityCal {
        double GetFugacityCoe(double acentricFactor, double relativeTemperature, double relativePressure);
        double GetFugacityCoe(Material.Chemical chemical, double temperature, double pressure);
    }
    /// <summary>
    /// SecVirial 抽象基类型
    /// </summary>
    public abstract class SecVirialBaseModel : CorspdStateBaseModel, ISecVirialFugacityCal {
        public SecVirialBaseModel() : base() { }
        /// <summary>
        /// B^
        /// </summary>
        public double GetSecVirialCoe(double relativeTemperature, double acentricFactor)
            => GetSecVirialCoeBase(relativeTemperature) + acentricFactor * GetSecVirialCoeCrec(relativeTemperature);
        /// <summary>
        /// Z0 =
        /// </summary>
        /// <param name="relativeTemperature">Tr</param>
        /// <param name="relativePressure">Pr</param>
        /// <returns></returns>
        public double GetCprBase(double relativeTemperature, double relativePressure)
            => 1.0 + GetSecVirialCoeBase(relativeTemperature) * relativePressure / relativeTemperature;
        /// <summary>
        /// Z1 =
        /// </summary>
        /// <param name="relativeTemperature">Tr</param>
        /// <param name="relativePressure">Pr</param>
        /// <returns></returns>
        public double GetCprCrec(double relativeTemperature, double relativePressure)
            => GetSecVirialCoeCrec(relativeTemperature) * relativePressure / relativeTemperature;
        /// <summary>
        /// B0 =
        /// </summary>
        public abstract double GetSecVirialCoeBase(double relativeTemperature);
        /// <summary>
        /// B0 =
        /// </summary>
        public double GetSecVirialCoeBase(Material.Chemical chemical, double temperature)
            => GetSecVirialCoeBase(temperature / chemical.CriticalTemperature);
        /// <summary>
        /// B1 =
        /// </summary>
        public abstract double GetSecVirialCoeCrec(double relativeTemperature);
        /// <summary>
        /// B1 =
        /// </summary>
        public double GetSecVirialCoeCrec(Material.Chemical chemical, double temperature)
            => GetSecVirialCoeCrec(GetReletiveProperty(temperature, chemical.CriticalTemperature));
        /// <summary>
        /// Z =
        /// </summary>
        /// <param name="cprBase">Z0</param>
        /// <param name="cprCrec">Z1</param>
        /// <param name="acentricFactor">偏心因子</param>
        /// <returns>Z</returns>
        public double GetCprFactor(double cprBase, double cprCrec, double acentricFactor)
            => cprBase + acentricFactor * cprCrec;
        /// <summary>
        /// 获取 Virial 方程的形式
        /// </summary>
        public abstract SecVirialType EqtType { get; }
        /// <summary>
        /// 计算逸度
        /// </summary>
        /// <param name="acentricFactor">偏心因子</param>
        /// <param name="relativeTemperature">相对温度</param>
        /// <param name="relativePressure">相对压力</param>
        /// <returns></returns>
        public double GetFugacityCoe(double acentricFactor, double relativeTemperature, double relativePressure)
            => Math.Exp(relativePressure / relativeTemperature * GetSecVirialCoe(relativeTemperature, acentricFactor));
        /// <summary>
        /// 计算逸度
        /// </summary>
        /// <param name="chemical">目标化合物</param>
        /// <param name="temperature">温度</param>
        /// <param name="pressure">压力</param>
        /// <returns></returns>
        public double GetFugacityCoe(Material.Chemical chemical, double temperature, double pressure)
            => GetFugacityCoe(chemical.AcentricFactor, GetReletiveProperty(temperature, chemical.CriticalTemperature), GetReletiveProperty(pressure, chemical.CriticalPressure));
    }
    /// <summary>
    /// Tsonopoulos 式
    /// </summary>
    public class TsonopoulosPureModel : SecVirialBaseModel {
        /// <summary>
        /// SecVirial模型的类型
        /// </summary>
        public override SecVirialType EqtType => SecVirialType.Tsonopoulos;
        /// <summary>
        /// B0
        /// </summary>
        public override double GetSecVirialCoeBase(double relativeTemperature)
            => 0.1445
            - 0.33 / relativeTemperature
            - 0.1385 / Math.Pow(relativeTemperature, 2.0)
            - 0.0121 / Math.Pow(relativeTemperature, 3.0)
            - 0.000607 / Math.Pow(relativeTemperature, 8.0);
        /// <summary>
        /// B1
        /// </summary>
        public override double GetSecVirialCoeCrec(double relativeTemperature)
            => 0.0637
            + 0.331 / Math.Pow(relativeTemperature, 2.0)
            - 0.423 / Math.Pow(relativeTemperature, 3.0)
            - 0.008 / Math.Pow(relativeTemperature, 8.0);
    }
    /// <summary>
    /// Classic 式
    /// </summary>
    public class ClassicPureModel : SecVirialBaseModel {
        /// <summary>
        /// SecVirial模型的类型
        /// </summary>
        public override SecVirialType EqtType => SecVirialType.Classic;
        /// <summary>
        /// B0
        /// </summary>
        public override double GetSecVirialCoeBase(double relativeTemperature)
            => 0.083 - 0.422 / Math.Pow(relativeTemperature, 1.6);
        /// <summary>
        /// B1
        /// </summary>
        public override double GetSecVirialCoeCrec(double relativeTemperature)
            => 0.139 - 0.172 / Math.Pow(relativeTemperature, 4.2);
    }
    /// <summary>
    /// SecVirial 类型的枚举
    /// </summary>
    public enum SecVirialType {
        Classic = 1,
        Tsonopoulos = 2
    }
}
/// <summary>
/// 压缩因子图法计算模型
/// </summary>
namespace ChemEngThermCal.Model.CorStt.Models.Diagram {
    /// <summary>
    /// 图表交互对象的结果
    /// </summary>
    public class InteractResult {
        /// <summary>
        /// 图表交互对象的结果
        /// </summary>
        /// <param name="interactBase">X0</param>
        /// <param name="interactCrec">X1</param>
        public InteractResult(double interactBase, double interactCrec) {
            InteractBase = interactBase;
            InteractCrec = interactCrec;
        }
        /// <summary>
        /// N0
        /// </summary>
        public double InteractBase { get; private set; }
        /// <summary>
        /// N1
        /// </summary>
        public double InteractCrec { get; private set; }
    }
    /// <summary>
    /// 对应态原理 图表法数据交互对象 所必需实现的接口
    /// </summary>
    public interface IDiagramInteractor {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="relativeTemperature">Tr</param>
        /// <param name="relativePressure">Pr</param>
        void Initialize(double relativeTemperature, double relativePressure);
        /// <summary>
        /// 是否被放弃
        /// </summary>
        bool IsAborted { get; }
        /// <summary>
        /// [ Z0, Z1 ]
        /// </summary>
        /// <returns></returns>
        InteractResult GetInteracResult();
    }
    /// <summary>
    /// 普遍化压缩因子图法 计算模型
    /// </summary>
    public class CprModel : CorspdStateBaseModel {
        /// <summary>
        /// 普遍化压缩因子模型
        /// </summary>
        /// <param name="interactor">对应态原理 图表法数据交互对象</param>
        public CprModel(IDiagramInteractor interactor) : base() {
            _interactor = interactor;
        }
        //中介者对象
        protected IDiagramInteractor _interactor;
        /// <summary>
        /// [ Z0 , Z1 ]
        /// </summary>
        /// <param name="relativePressure">Pr</param>
        /// <returns>[ Z0 , Z1 ]</returns>
        public InteractResult InteractResult { get; protected set; }
        /// <summary>
        /// 计算是否被放弃
        /// </summary>
        /// <param name="relativeTemperature">Tr</param>
        /// <param name="relativePressure">Pr</param>
        /// <returns></returns>
        public bool IsDataAcquired(double relativeTemperature, double relativePressure) {
            _interactor.Initialize(relativeTemperature, relativePressure);
            InteractResult = _interactor.GetInteracResult();
            return _interactor.IsAborted;
        }
        /// <summary>
        /// Z =
        /// </summary>
        /// <param name="cprBase">Z0</param>
        /// <param name="cprCrec">Z1</param>
        /// <param name="acentricFactor">偏心因子</param>
        /// <returns>Z</returns>
        public double GetCprFactor(double cprBase, double cprCrec, double acentricFactor)
            => cprBase + acentricFactor * cprCrec;
    }
    /// <summary>
    /// 普遍化逸度系数图法 计算模型
    /// </summary>
    public class FugModel : CorspdStateBaseModel {
        /// <summary>
        /// 普遍化逸度系数图法 模型
        /// </summary>
        /// <param name="interactor"></param>
        public FugModel(IDiagramInteractor interactor) : base() {
            _interactor = interactor;
        }
        //中介者对象
        private IDiagramInteractor _interactor;
        /// <summary>
        /// 交互结果
        /// </summary>
        public InteractResult InteractResult { get; protected set; }
        /// <summary>
        /// 计算是否被放弃
        /// </summary>
        /// <param name="relativeTemperature">Tr</param>
        /// <param name="relativePressure">Pr</param>
        /// <returns></returns>
        public bool IsBaseCrecAcquired(double relativeTemperature, double relativePressure) {
            _interactor.Initialize(relativeTemperature, relativePressure);
            InteractResult = _interactor.GetInteracResult();
            return _interactor.IsAborted;
        }
        /// <summary>
        /// 逸度系数 ψ =
        /// </summary>
        /// <param name="fugBase">ψ0</param>
        /// <param name="fugCrec">ψ1</param>
        /// <param name="acentricFactor">偏心因子param>
        /// <returns>ψ</returns>
        public double GetFugacityCoe(double fugBase, double fugCrec, double acentricFactor)
            => fugBase * Math.Pow(fugCrec, acentricFactor);
    }
}
/// <summary>
/// 计算压力
/// </summary>
namespace ChemEngThermCal.Model.CorStt.CalPressure {
    /// <summary>
    /// GCE迭代步骤记录器
    /// </summary>
    public class GceStepInfo {
        /// <summary>
        /// 实例化一个 普遍化关联式计算的步骤记录器
        /// </summary>
        /// <param name="cprFactor">Z(n)</param>
        /// <param name="relativeTemperature">Tr, K</param>
        /// <param name="relativePressure">Pr, mpa</param>
        /// <param name="cprFactorBase">Z0</param>
        /// <param name="cprFactorCorrection">Z1</param>
        /// <param name="nextCprFactor">Z(n+1)</param>
        /// <param name="diffFlag">|Z(n+1)-Z(n)|</param>
        public GceStepInfo(Algorithm.CorSttIterator.StepInfo rawStep) {
            CprFactor = rawStep.CprFactor;
            RelativePressure = rawStep.RelativePressure;
            CprFactorBase = rawStep.CprBase;
            CprFactorCrec = rawStep.CprCrec;
            NextCprFactor = rawStep.NextCprFactor;
            DiffFlag = rawStep.DiffValue;
        }
        /// <summary>
        /// Zn
        /// </summary>
        public double CprFactor { get; private set; }
        /// <summary>
        /// Pr
        /// </summary>
        public double RelativePressure { get; private set; }
        /// <summary>
        /// Z0
        /// </summary>
        public double CprFactorBase { get; private set; }
        /// <summary>
        /// Z1
        /// </summary>
        public object CprFactorCrec { get; private set; }
        /// <summary>
        /// Z n+1
        /// </summary>
        public object NextCprFactor { get; private set; }
        /// <summary>
        /// Δ
        /// </summary>
        public double DiffFlag { get; private set; }
    }
    /// <summary>
    /// 对应态原理计算压力 中介者 的抽象基类
    /// </summary>
    public abstract class CalPressureBaseMediator {
        /// <summary>
        /// 对应态原理计算压力 中介者 的抽象基类
        /// </summary>
        /// <param name="atmosTemperature">T</param>
        /// <param name="atmosMoleVolume">P</param>
        public CalPressureBaseMediator(double atmosTemperature, double atmosMoleVolume) {
            Atmos_V = atmosMoleVolume;
            Atmos_T = atmosTemperature;
        }
        /// <summary>
        /// V =
        /// </summary>
        public double Atmos_V { get; private set; }
        /// <summary>
        /// T =
        /// </summary>
        public double Atmos_T { get; private set; }
    }
    /// <summary>
    /// 对应态原理计算纯物系压力 中介者 的抽象基类
    /// </summary>
    public abstract class CalPressurePureBaseMediator : CalPressureBaseMediator, MixRules.IPureFluidModel {
        /// <summary>
        /// 对应态原理计算纯物系压力 中介者 的抽象基类
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="atmosTemperature">T</param>
        /// <param name="atmosMoleVolume">P</param>
        public CalPressurePureBaseMediator(Material.Chemical chemical, double atmosTemperature, double atmosMoleVolume)
            : base(atmosTemperature, atmosMoleVolume) {
            _chemical = chemical;
        }
        /// <summary>
        /// 通过计算条件来确定 应当 使用的对应态模型
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="atmosTemperature">T</param>
        /// <param name="atmosMoleVolume">V</param>
        /// <param name="cprDiagInteractor">压缩因子图法的交互接口对象，由于直接返回相应的中介者模型因此必须传入</param>
        /// <returns></returns>
        public static CalPressurePureBaseMediator GetProperModel(Material.Chemical chemical, double atmosTemperature, double atmosMoleVolume, Models.Diagram.IDiagramInteractor cprDiagInteractor) {
            double Vr = atmosMoleVolume / chemical.CriticalVolume;
            if (Vr >= 2.0) {
                return new UsingSecVirial.SecVirialPureMediator(new Models.SecVirial.ClassicPureModel(), chemical, atmosTemperature, atmosMoleVolume);
            } else {
                return new UsingCprDiagram.CprDiagPureMediator(new Models.Diagram.CprModel(cprDiagInteractor), chemical, atmosTemperature, atmosMoleVolume);
            }
        }
        /// <summary>
        /// 计算结果的基类型
        /// </summary>
        public abstract class CalPressureResultBase : ResultBase {
            /// <summary>
            /// 普遍化关联式 计算压力 结果的基类型
            /// </summary>
            /// <param name="T">环境温度</param>
            /// <param name="P">环境压力</param>
            /// <param name="gasV">气相摩尔体积</param>
            /// <param name="gasZ">气相压缩因子</param>
            /// <param name="isGasAborted">气相计算是否被放弃</param>
            /// <param name="isGasConverged">气相结果是否收敛</param>
            /// <param name="gasStep">气相计算信息</param>
            /// <param name="chemical">化合物</param>
            public CalPressureResultBase(double T, double P, double gasV, double gasZ, bool isGasAborted, bool isGasConverged,
                List<GceStepInfo> gasStep, Material.Chemical chemical)
                : base(T, P, gasV, gasZ, isGasAborted, isGasConverged) {
                TargetChemical = chemical;
                RelativeTemperature = T / chemical.CriticalTemperature;
                RelativePressure = P / chemical.CriticalPressure;
                GasStepInfo = gasStep;
            }
            /// <summary>
            /// 相对温度
            /// </summary>
            public double RelativeTemperature { get; private set; }
            /// <summary>
            /// 相对压力
            /// </summary>
            public double RelativePressure { get; private set; }
            /// <summary>
            /// 气相迭代步骤信息
            /// </summary>
            public List<GceStepInfo> GasStepInfo { get; private set; }
            /// <summary>
            /// 化合物
            /// </summary>
            public Material.Chemical TargetChemical { get; private set; }
        }
        #region MixRules.IPureFluidModel 的显式实现
        //化合物
        protected Material.Chemical _chemical;
        /// <summary>
        /// 化合物
        /// </summary>
        Material.Chemical MixRules.IPureFluidModel.Chemical
            => _chemical;
        #endregion
    }
}
/// <summary>
/// Sec Virial
/// </summary>
namespace ChemEngThermCal.Model.CorStt.CalPressure.UsingSecVirial {
    /// <summary>
    /// 纯组分 SecVirial 普遍化计算 压力 的中介者
    /// </summary>
    public class SecVirialPureMediator : CalPressurePureBaseMediator, Algorithm.ICorSttItrMediator {
        /// <summary>
        /// 纯组分 SecVirial 普遍化计算 压力 的中介者
        /// </summary>
        /// <param name="model">Sec Virial 模型</param>
        /// <param name="chemical">化合物</param>
        /// <param name="atmosTemperature">T</param>
        /// <param name="atmosMoleVolume">P</param>
        public SecVirialPureMediator(Models.SecVirial.SecVirialBaseModel model, Material.Chemical chemical, double atmosTemperature, double atmosMoleVolume)
            : base(chemical, atmosTemperature, atmosMoleVolume) {
            Model = model;
        }
        //所持有的模型对象
        protected Models.SecVirial.SecVirialBaseModel Model { get; private set; }
        /// <summary>
        /// 包含计算过程的所有信息
        /// </summary>
        public class Result : CalPressureResultBase {
            public Result(double T, double P, double gasV, double gasZ, bool isGasConverged,
                List<GceStepInfo> gasStep, Material.Chemical chemical, Models.SecVirial.SecVirialBaseModel model)
                : base(T, P, gasV, gasZ, false, isGasConverged, gasStep, chemical) {
                ModelType = model.EqtType;
            }
            public Models.SecVirial.SecVirialType ModelType { get; private set; }
        }
        /// <summary>
        /// 获取结果
        /// </summary>
        public Result GetResult() {
            double gasCpr, pressure;
            bool isGasConverged;
            Algorithm.CorSttIterator calProc = new Algorithm.CorSttIterator(this, 1.0, 0.0001, 100);
            gasCpr = calProc.Result;
            if (gasCpr == -1.0 || gasCpr == -2.0) {
                pressure = gasCpr;
                isGasConverged = false;
            } else {
                pressure = Model.GetGasPressure(gasCpr, Atmos_T, Atmos_V);
                isGasConverged = true;
            }
            List<GceStepInfo> gasStepInfo = new List<GceStepInfo> { };
            foreach (Algorithm.CorSttIterator.StepInfo rawStep in calProc.StepInfoList) {
                gasStepInfo.Add(new GceStepInfo(rawStep));
            }
            return new Result(Atmos_T, pressure, Atmos_V, gasCpr, isGasConverged, gasStepInfo, _chemical, Model);
        }
        #region Algorithm.ICorspdStateIterator 的显式实现
        double Algorithm.ICorSttItrMediator.GetRelativePressure(double cprFactor)
           => cprFactor * 8.314 * Atmos_T / Atmos_V / _chemical.CriticalPressure;

        double Algorithm.ICorSttItrMediator.GetTargetFactor(double cprBase, double cprCrec)
           => Model.GetCprFactor(cprBase, cprCrec, _chemical.AcentricFactor);

        Algorithm.CorSttIterator.Msg Algorithm.ICorSttItrMediator.GetNextValue(double relativePressure)
           => new Algorithm.CorSttIterator.Msg(false, Model.GetCprBase(Atmos_T / _chemical.CriticalTemperature, relativePressure), Model.GetCprCrec(Atmos_T / _chemical.CriticalTemperature, relativePressure));
        #endregion
    }
}
/// <summary>
/// 压缩因子图法
/// </summary>
namespace ChemEngThermCal.Model.CorStt.CalPressure.UsingCprDiagram {
    /// <summary>
    /// 普遍化压缩因子图法 中介者
    /// </summary>
    public class CprDiagPureMediator : CalPressurePureBaseMediator, Algorithm.ICorSttItrMediator {
        /// <summary>
        /// 普遍化压缩因子图法 中介者
        /// </summary>
        /// <param name="model">普遍化压缩因子图法 模型</param>
        /// <param name="chemical">化合物</param>
        /// <param name="atmosTemperature">T</param>
        /// <param name="atmosMoleVolume">P</param>
        public CprDiagPureMediator(Models.Diagram.CprModel model, Material.Chemical chemical, double atmosTemperature, double atmosMoleVolume)
            : base(chemical, atmosTemperature, atmosMoleVolume) {
            Model = model;
        }
        //所持有的模型对象
        protected Models.Diagram.CprModel Model { get; private set; }
        /// <summary>
        /// 包含计算的所有信息
        /// </summary>
        public class Result : CalPressureResultBase {
            public Result(double T, double P, double gasV, double gasZ, bool isAborted, bool isGasConverged, List<GceStepInfo> gasStep, Material.Chemical chemical)
                : base(T, P, gasV, gasZ, isAborted, isGasConverged, gasStep, chemical) { }
        }
        /// <summary>
        /// 获取结果
        /// </summary>
        public Result GetResult() {
            double gasCpr, pressure;
            bool isGasConverged, isCalAborted;
            Algorithm.CorSttIterator calProc = new Algorithm.CorSttIterator(this, 1.0, 0.0001, 100);
            gasCpr = calProc.Result;
            if (gasCpr == -1.0 || gasCpr == -2.0) {
                pressure = gasCpr;
                isGasConverged = false;
                isCalAborted = false;
            } else if (gasCpr == -3.0) {
                pressure = gasCpr;
                isGasConverged = false;
                isCalAborted = true;
            } else {
                pressure = Model.GetGasPressure(gasCpr, Atmos_T, Atmos_V);
                isGasConverged = true;
                isCalAborted = false;
            }
            List<GceStepInfo> gasStepInfo = new List<GceStepInfo> { };
            foreach (Algorithm.CorSttIterator.StepInfo rawStep in calProc.StepInfoList) {
                gasStepInfo.Add(new GceStepInfo(rawStep));
            }
            return new Result(Atmos_T, pressure, Atmos_V, gasCpr, isCalAborted, isGasConverged, gasStepInfo, _chemical);
        }
        #region Algorithm.ICorspdStateIterator 的显式实现
        double Algorithm.ICorSttItrMediator.GetRelativePressure(double cprFactor)
           => cprFactor * 8.314 * Atmos_T / Atmos_V / _chemical.CriticalPressure;

        double Algorithm.ICorSttItrMediator.GetTargetFactor(double cprBase, double cprCrec)
           => Model.GetCprFactor(cprBase, cprCrec, _chemical.AcentricFactor);

        Algorithm.CorSttIterator.Msg Algorithm.ICorSttItrMediator.GetNextValue(double relativePressure) {
            bool isAborted = Model.IsDataAcquired(Atmos_T / _chemical.CriticalTemperature, relativePressure);
            if (isAborted) {
                return new Algorithm.CorSttIterator.Msg(true, -3.0, -3.0);
            } else {
                return new Algorithm.CorSttIterator.Msg(false, Model.InteractResult.InteractBase, Model.InteractResult.InteractCrec);
            }
        }
        #endregion
    }
}
/// <summary>
/// 计算摩尔体积
/// </summary>
namespace ChemEngThermCal.Model.CorStt.CalMoleVolume {
    /// <summary>
    /// 所有计算目标为摩尔体积的中介者的基类型
    /// </summary>
    public abstract class MediatorBase {
        /// <summary>
        /// 所有计算目标为摩尔体积的中介者的基类型
        /// </summary>
        /// <param name="atmosTemperature">T</param>
        /// <param name="atmosPressure">P</param>
        public MediatorBase(double atmosTemperature, double atmosPressure) {
            Atmos_T = atmosTemperature;
            Atmos_P = atmosPressure;
        }
        /// <summary>
        /// T =
        /// </summary>
        public double Atmos_T { get; private set; }
        /// <summary>
        /// P =
        /// </summary>
        public double Atmos_P { get; private set; }
    }
    /// <summary>
    /// 混合物系基础抽象类
    /// </summary>
    public abstract class MediatorMixBase : MediatorBase, MixRules.IMixFluidModel {
        /// <summary>
        /// 混合物系计算基础抽象类
        /// </summary> 
        public MediatorMixBase(BinaryComplex complex, double atmosTemperature, double atmosPressure)
            : base(atmosTemperature, atmosPressure) {
            _complex = complex;
        }
        #region MixRules.IMixFluidModel 的显式实现
        //私有混合物成员
        protected BinaryComplex _complex;
        /// <summary>
        /// 二元混合物
        /// </summary>
        BinaryComplex MixRules.IMixFluidModel.Complex
            => _complex;
        #endregion
        /// <summary>
        /// 混合物系计算结果的基类型
        /// </summary>
        public abstract class MixResultBase : ResultBase {
            /// <summary>
            /// 混合物系计算结果的基类型
            /// </summary>
            /// <param name="T">环境温度</param>
            /// <param name="P">环境压力</param>
            /// <param name="gasV">气相摩尔体积</param>
            /// <param name="gasZ">气相压缩因子</param>
            /// <param name="isGasAborted">气相计算是否被放弃</param>
            /// <param name="isGasConverged">气象结果是否收敛</param>
            /// <param name="mixTc">混合物临界温度</param>
            /// <param name="mixPc">混合物临界压力</param>
            /// <param name="mixW">混合物压缩因子</param>
            public MixResultBase(double T, double P, double gasV, double gasZ, bool isGasAborted, bool isGasConverged, double mixTc, double mixPc, double mixW)
                : base(T, P, gasV, gasZ, isGasAborted, isGasConverged) {
                MixCriticalTemperature = mixTc;
                MixCriticalPressure = mixPc;
                MixAcentricFactor = mixW;
                MixRelativeTemperature = T / MixCriticalTemperature;
                MixRelativePressure = P / MixCriticalPressure;
            }
            /// <summary>
            /// 混合物相对压力
            /// </summary>
            public double MixRelativePressure { get; private set; }
            /// <summary>
            /// 混合物相对温度
            /// </summary>
            public double MixRelativeTemperature { get; private set; }
            /// <summary>
            /// 混合物临界温度
            /// </summary>
            public double MixCriticalTemperature { get; private set; }
            /// <summary>
            /// 混合物临界压力
            /// </summary>
            public double MixCriticalPressure { get; private set; }
            /// <summary>
            /// 混合物压缩因子
            /// </summary>
            public double MixAcentricFactor { get; private set; }
        }
    }
    /// <summary>
    /// 纯物系基础抽象类
    /// </summary>
    public abstract class MediatorPureBase : MediatorBase, MixRules.IPureFluidModel {
        public MediatorPureBase(Chemical chemical, double atmosTemperature, double atmosPressure)
            : base(atmosTemperature, atmosPressure) {
            _chemical = chemical;
        }
        #region MixRules.IPureFluidModel 接口的显式实现
        protected Chemical _chemical;
        /// <summary>
        /// 目标化合物
        /// </summary>
        Chemical MixRules.IPureFluidModel.Chemical
           => _chemical;
        #endregion
        /// <summary>
        /// 纯物系计算结果的基类型
        /// </summary>
        public abstract class PureResultBase : ResultBase {
            /// <summary>
            /// 纯物系计算结果的基类型
            /// </summary>
            /// <param name="T">环境温度</param>
            /// <param name="P">环境压力</param>
            /// <param name="gasV">气相摩尔体积</param>
            /// <param name="gasZ">气相压缩因子</param>
            /// <param name="isGasAborted">气相计算是否被放弃</param>
            /// <param name="isGasConverged">气相结果是否收敛</param>
            /// <param name="chemical">化合物</param>
            public PureResultBase(double T, double P, double Tr, double Pr, double gasV, double gasZ, bool isGasAborted, bool isGasConverged, Chemical chemical)
                : base(T, P, gasV, gasZ, isGasAborted, isGasConverged) {
                Chemical = chemical;
                RelativeTemperature = Tr;
                RelativePressure = Pr;
            }
            /// <summary>
            /// 化合物
            /// </summary>
            public Chemical Chemical { get; private set; }
            public double RelativePressure { get; private set; }
            public double RelativeTemperature { get; private set; }
        }
    }
    /// <summary>
    /// 对应态原理计算混合物系摩尔体积 Kay 混合规则 中介者 的抽象基类
    /// </summary>
    public abstract class KayMixMediatorBase : MediatorMixBase {
        /// <summary>
        /// 对应态原理计算混合物系摩尔体积 Kay 混合规则 中介者 的抽象基类
        /// </summary>
        /// <param name="complex">二元混合体系</param>
        /// <param name="atmosTemperature">全局温度</param>
        /// <param name="atmosPressure">全局压力</param>
        public KayMixMediatorBase(BinaryComplex complex, double atmosTemperature, double atmosPressure)
            : base(complex, atmosTemperature, atmosPressure) { }
        /// <summary>
        /// 通过计算条件来确定 应当 使用的对应态模型
        /// </summary>
        /// <param name="complex">混合体系</param>
        /// <param name="atmosTemperature">T</param>
        /// <param name="atmosMoleVolume">P</param>
        /// <param name="cprDiagInteractor">压缩因子图法的交互接口对象，由于直接返回相应的中介者模型因此必须传入</param>
        /// <returns></returns>
        public static KayMixMediatorBase GetProperModel(BinaryComplex complex, double atmostemperature, double atmosmolevolume, Models.Diagram.IDiagramInteractor cprDiagInteractor) {
            double Tr = complex.FstComponent.CriticalTemperature * complex.FstComponentMoleFraction + complex.SecComponent.CriticalTemperature * complex.SecComponentMoleFraction;
            double Pr = complex.FstComponent.CriticalPressure * complex.FstComponentMoleFraction + complex.SecComponent.CriticalPressure * complex.SecComponentMoleFraction;
            if ((0.428 * Pr + 0.5751 - Tr) < 0) {
                return new UsingSecVirial.SecVirialKayMixMediator(new Models.SecVirial.ClassicPureModel(), complex, atmostemperature, atmosmolevolume);
            } else {
                return new UsingCprDiagram.CprDiagKayMixMediator(new Models.Diagram.CprModel(cprDiagInteractor), complex, atmostemperature, atmosmolevolume);
            }
        }
        /// <summary>
        /// 结果的实例
        /// </summary>
        public abstract class KayResultBase : MixResultBase {
            public KayResultBase(double T, double P, double gasV, double gasZ, bool isGasAborted, bool isGasConverged,
                double mixTc, double mixPc, double mixW, double mixZ0, double mixZ1, Material.BinaryComplex complex)
                 : base(T, P, gasV, gasZ, isGasAborted, isGasConverged, mixTc, mixPc, mixW) {
                MixCprFactorBase = mixZ0;
                MixCprFactorCrec = mixZ1;
                TargetComplex = complex;
            }
            public double MixCprFactorBase { get; private set; }
            public double MixCprFactorCrec { get; private set; }
            public BinaryComplex TargetComplex { get; private set; }
        }
    }
}
/// <summary>
/// Sec Virial
/// </summary>
namespace ChemEngThermCal.Model.CorStt.CalMoleVolume.UsingSecVirial {
    /// <summary>
    /// SecVirial 计算纯物质摩尔体积 的中介者
    /// </summary>
    public class SecVirialPureMediator : MediatorPureBase {
        /// <summary>
        /// SecVirial 计算纯物质摩尔体积 的中介者
        /// </summary>
        /// <param name="model">Sec Virial 模型</param>
        /// <param name="chemical">化合物</param>
        /// <param name="atmosTemperature">温度</param>
        /// <param name="atmosPressure">压力</param>
        public SecVirialPureMediator(Models.SecVirial.SecVirialBaseModel model, Chemical chemical, double atmosTemperature, double atmosPressure)
            : base(chemical, atmosTemperature, atmosPressure) {
            Model = model;
        }
        /// <summary>
        /// 持有的 SecVirial 计算模型
        /// </summary>
        public Models.SecVirial.SecVirialBaseModel Model { get; private set; }
        /// <summary>
        /// 计算结果的实例
        /// </summary>
        public class Result : PureResultBase {
            public Result(double T, double P, double Tr, double Pr, double gasV, double gasZ, Chemical chemical, double b0, double b1, double b, double z0, double z1)
                : base(T, P, Tr, Pr, gasV, gasZ, false, true, chemical) {
                B0 = b0;
                B1 = b1;
                B = b;
                Z0 = z0;
                Z1 = z1;
            }
            public double B { get; private set; }
            public double B0 { get; private set; }
            public double B1 { get; private set; }
            public double Z0 { get; private set; }
            public double Z1 { get; private set; }
            //其他成员
        }
        /// <summary>
        /// 气相计算
        /// </summary>
        public Result GetResult() {
            double tr = Atmos_T / _chemical.CriticalTemperature;
            double pr = Atmos_P / _chemical.CriticalPressure;
            double b0 = Model.GetSecVirialCoeBase(tr);
            double b1 = Model.GetSecVirialCoeCrec(tr);
            double b = Model.GetSecVirialCoe(tr, _chemical.AcentricFactor);
            double z0 = Model.GetCprBase(tr, pr);
            double z1 = Model.GetCprCrec(tr, pr);
            double cpr = Model.GetCprFactor(z0, z1, _chemical.AcentricFactor);
            double vol = Model.GetGasMoleVolume(cpr, Atmos_T, Atmos_P);
            return new Result(Atmos_T, Atmos_P, tr, pr, vol, cpr, _chemical, b0, b1, b, z0, z1);
        }
    }
    /// <summary>
    /// SecVirial 计算混合物系摩尔体积 Kay 混合规则 的中介者
    /// </summary>
    public class SecVirialKayMixMediator : KayMixMediatorBase {
        /// <summary>
        /// SecVirial 计算混合物系摩尔体积 Kay 混合规则 的中介者
        /// </summary>
        /// <param name="model">Sec Virial 模型</param>
        /// <param name="chemical">化合物</param>
        /// <param name="atmosTemperature">温度</param>
        /// <param name="atmosPressure">压力</param>
        public SecVirialKayMixMediator(Models.SecVirial.SecVirialBaseModel model, Material.BinaryComplex complex, double atmosTemperature, double atmosPressure)
            : base(complex, atmosTemperature, atmosPressure) {
            Model = model;
        }
        /// <summary>
        /// 持有的 SecVirial 计算模型
        /// </summary>
        protected Models.SecVirial.SecVirialBaseModel Model { get; private set; }
        /// <summary>
        /// 气相计算
        /// </summary>
        public Result GetResult() {
            MixRules.KayMixRule mixRule = new MixRules.KayMixRule(this);
            double Tcm = mixRule.Mix_Tc;
            double Pcm = mixRule.Mix_Pc;
            double wm = mixRule.Mix_w;
            double Trm = Atmos_T / Tcm;
            double Prm = Atmos_P / Pcm;
            double B0 = Model.GetSecVirialCoeBase(Trm);
            double B1 = Model.GetSecVirialCoeCrec(Trm);
            double B = Model.GetSecVirialCoe(Trm, wm);
            double Z0 = Model.GetCprBase(Trm, Prm);
            double Z1 = Model.GetCprCrec(Trm, Prm);
            double gasCpr = Model.GetCprFactor(Z0, Z1, wm);
            double gasMoleVol = Model.GetGasMoleVolume(gasCpr, Atmos_T, Atmos_P);
            return new Result(Atmos_T, Atmos_P, _complex, Tcm, Pcm, wm, Trm, Prm, B0, B1, B, Z0, Z1, gasCpr, gasMoleVol);
        }
        /// <summary>
        /// 计算结果的实例
        /// </summary>
        public class Result : KayResultBase {
            public Result(double T, double P, Material.BinaryComplex complex,
                double mixTc, double mixPc, double mixW, double mixTr, double mixPr,
                double mixB0, double mixB1, double mixB, double mixZ0, double mixZ1,
                double gasCpr, double gasMoleVol)
                : base(T, P, gasMoleVol, gasCpr, false, true, mixTc, mixPc, mixW, mixZ0, mixZ1, complex) {
                MixB0 = mixB0;
                MixB1 = mixB1;
                MixB = mixB;
            }
            public double MixB0 { get; private set; }
            public double MixB1 { get; private set; }
            public double MixB { get; private set; }
        }
    }
    /// <summary>
    /// SecVirial 计算混合物系摩尔体积 SecVirial 混合规则 的中介者
    /// </summary>
    public class SecVirialSVMixMediator : MediatorMixBase, MixRules.ISecVirialMixMediator {
        public SecVirialSVMixMediator(Models.SecVirial.SecVirialBaseModel model, BinaryComplex complex, double atmosTemperature, double atmosPressure, double binCorIndex = 0)
            : base(complex, atmosTemperature, atmosPressure) {
            _complex = complex;
            _binCorIndex = binCorIndex;
            Model = model;
        }
        /// <summary>
        /// 持有的 SecVirial 计算模型
        /// </summary>
        protected Models.SecVirial.SecVirialBaseModel Model { get; private set; }
        /// <summary>
        /// 气相计算
        /// </summary>
        public Result GetResult() {
            MixRules.SecVirialRule mixRule = new MixRules.SecVirialRule(this);
            double[] getIndexes = mixRule.GetRelevants(Atmos_T);
            double Vcm = mixRule.Mix_Vc;
            double Zcm = mixRule.Mix_Zc;
            double Tcm = mixRule.Mix_Tc;
            double Pcm = mixRule.Mix_Pc;
            double wm = mixRule.Mix_w;
            double Trm = Atmos_T / Tcm;
            double Prm = Atmos_P / Pcm;
            double B0_11 = getIndexes[0];
            double B1_11 = getIndexes[1];
            double B_11 = getIndexes[2];
            double B0_22 = getIndexes[3];
            double B1_22 = getIndexes[4];
            double B_22 = getIndexes[5];
            double B0_12 = getIndexes[6];
            double B1_12 = getIndexes[7];
            double B_12 = getIndexes[8];
            double MixB = getIndexes[9];
            double gasCpr = 1.0 + MixB * Atmos_P / 8.314 / Atmos_T;
            double gasMoleVol = Model.GetGasMoleVolume(gasCpr, Atmos_T, Atmos_P);
            return new Result(Atmos_T, Atmos_P, gasMoleVol, gasCpr, _complex, Tcm, Zcm, Vcm, Pcm, wm, B0_11, B1_11, B_11, B0_22, B1_22, B_22, B0_12, B1_12, B_12, MixB);
        }
        #region MixRules.ISecVirialMixMediator 的显式实现
        /// <summary>
        /// 实现 ISecVirialMixMediator 用于与 SecVirial 混合规则 交互
        /// </summary>
        Models.SecVirial.SecVirialBaseModel MixRules.ISecVirialMixMediator.GetTargetModel()
           => Model;
        //私有二元相互作用参数
        private double _binCorIndex;
        /// <summary>
        /// 二元相互作用参数 B
        /// </summary>
        double MixRules.IBinCorINdexMixMediator.BinCorIndex
            => _binCorIndex;
        /// <summary>
        /// 二元混合物
        /// </summary>
        BinaryComplex MixRules.IMixFluidModel.Complex
            => _complex;
        #endregion
        /// <summary>
        /// 计算结果的实例
        /// </summary>
        public class Result : MixResultBase {
            public Result(double T, double P, double gasV,
                double gasZ, BinaryComplex complex, double mixTc, double mixZc, double mixVc, double mixPc, double mixW,
                double b0_11, double b1_11, double b_11,
                double b0_22, double b1_22, double b_22,
                double b0_12, double b1_12, double b_12, double B)
                : base(T, P, gasV, gasZ, false, true, mixTc, mixPc, mixW) {
                MixCriticalCprFactor = mixZc;
                MixCriticalMoleVol = mixVc;
                B0_11 = b0_11;
                B1_11 = b1_11;
                B_11 = b_11;
                B0_22 = b0_22;
                B1_22 = b1_22;
                B_22 = b_22;
                B0_12 = b0_12;
                B1_12 = b1_12;
                B_12 = b_12;
                MixB = B;
            }
            public double B0_11 { get; private set; }
            public double B0_12 { get; private set; }
            public double B0_22 { get; private set; }
            public double B1_11 { get; private set; }
            public double B1_12 { get; private set; }
            public double B1_22 { get; private set; }
            public double B_11 { get; private set; }
            public double B_12 { get; private set; }
            public double B_22 { get; private set; }
            public double MixB { get; private set; }
            public double MixCriticalMoleVol { get; private set; }
            public double MixCriticalCprFactor { get; private set; }
        }
    }
}
/// <summary>
/// 压缩因子图法
/// </summary>
namespace ChemEngThermCal.Model.CorStt.CalMoleVolume.UsingCprDiagram {
    /// <summary>
    ///  压缩因子图法 计算纯物系摩尔体积 中介者
    /// </summary>
    public class CprDiagPureMediator : MediatorPureBase {
        public CprDiagPureMediator(Models.Diagram.CprModel model, Chemical chemical, double atmosTemperature, double atmosPressure)
            : base(chemical, atmosTemperature, atmosPressure) {
            Model = model;
        }
        //持有的图表法交互计算模型
        protected Models.Diagram.CprModel Model { get; private set; }
        /// <summary>
        /// 计算结果
        /// </summary>
        public class Result : PureResultBase {
            public Result(double T, double P, double Tr, double Pr, double gasV, double gasZ, Chemical chemical, bool isGasAborted, double z0, double z1)
                : base(T, P, Tr, Pr, gasV, gasZ, isGasAborted, !isGasAborted, chemical) {
                Z0 = z0;
                Z1 = z1;
            }
            public double Z0 { get; private set; }
            public double Z1 { get; private set; }
        }
        /// <summary>
        /// 气相计算
        /// </summary>
        public Result GetResult() {
            double tr = Model.GetReletiveProperty(Atmos_T, _chemical.CriticalTemperature);
            double pr = Model.GetReletiveProperty(Atmos_P, _chemical.CriticalPressure);
            bool isAborted = Model.IsDataAcquired(tr, pr);
            double z0 = Model.InteractResult.InteractBase;
            double z1 = Model.InteractResult.InteractCrec;
            double gasZ = Model.GetCprFactor(z0, z1, _chemical.AcentricFactor);
            double gasV = Model.GetGasMoleVolume(gasZ, Atmos_T, Atmos_P);
            return new Result(Atmos_T, Atmos_P, tr, pr, gasV, gasZ, _chemical, isAborted, z0, z1);
        }
    }
    /// <summary>
    /// 压缩因子图法 计算混合物系摩尔体积 Kay 混合规则 中介者 的抽象基类
    /// </summary>
    public class CprDiagKayMixMediator : KayMixMediatorBase {
        public CprDiagKayMixMediator(Models.Diagram.CprModel model, BinaryComplex complex, double atmosTemperature, double atmosPressure)
            : base(complex, atmosTemperature, atmosPressure) {
            Model = model;
        }
        //持有的图表法交互计算模型
        protected Models.Diagram.CprModel Model { get; private set; }
        /// <summary>
        /// 计算结果的实例
        /// </summary>
        public class Result : KayResultBase {
            public Result(double T, double P, double gasZ,
                double gasV, bool isGasAborted, Material.BinaryComplex complex, double mixTc, double mixPc,
                double mixW, double mixZ0, double mixZ1)
                : base(T, P, gasV, gasZ, isGasAborted, !isGasAborted, mixTc, mixPc, mixW, mixZ0, mixZ1, complex) { }
        }
        /// <summary>
        /// 气相计算
        /// </summary>
        public Result GetResult() {
            MixRules.KayMixRule mixRule = new MixRules.KayMixRule(this);
            double Tcm = mixRule.Mix_Tc;
            double Pcm = mixRule.Mix_Pc;
            double wm = mixRule.Mix_w;
            double Trm = Atmos_T / Tcm;
            double Prm = Atmos_P / Pcm;
            bool isAborted = Model.IsDataAcquired(Trm, Prm);
            double Z0 = Model.InteractResult.InteractBase;
            double Z1 = Model.InteractResult.InteractCrec;
            double gasCpr = Model.GetCprFactor(Z0, Z1, wm);
            double gasMoleVol = Model.GetGasMoleVolume(gasCpr, Atmos_T, Atmos_P);
            return new Result(Atmos_T, Atmos_P, gasCpr, gasMoleVol, isAborted, _complex, Tcm, Pcm, wm, Z0, Z1);
        }
    }
}