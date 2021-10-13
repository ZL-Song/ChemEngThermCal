using System;
using System.Collections.Generic;
using ChemEngThermCal.Model.Material;
using ChemEngThermCal.Model.Vle.FugCoe;
using ChemEngThermCal.Model.Vle.ActCoe.Models;
/// <summary>
/// 汽液平衡计算 活度系数 法
/// </summary>
namespace ChemEngThermCal.Model.Vle.ActCoe {
    /// <summary>
    /// 二元物系，以 活度系数 + EOS 法进行 VLE 计算中介者的基类型
    /// </summary>
    public abstract class MixVleBaseMediator : MixRules.IMixFluidModel {
        /// <summary>
        /// 初始化一个 二元物系 VLE 计算中介者的基类型
        /// </summary>
        /// <param name="fugMediator">逸度中介器</param>
        /// <param name="actMediator">活度中介器</param>
        public MixVleBaseMediator(GasFugacityMediator fugMediator, ActivityMediator actMediator, Antoine.AntoineMediator atnMediator, BinaryComplex complex, double independentVar, CalType calType) {
            _fugMediator = fugMediator;
            _actMediator = actMediator;
            _atnMediator = atnMediator;
            _knownComplex = complex;
            KnownVar = independentVar;
            MediatorType = calType;
        }
        /// <summary>
        /// 立方型状态方程逸度中介器
        /// </summary>
        protected GasFugacityMediator _fugMediator { get; private set; }
        /// <summary>
        /// 立方型状态方程活度中介器
        /// </summary>
        protected ActivityMediator _actMediator { get; private set; }
        /// <summary>
        /// Antoine 方程 计算饱和压力 中介器
        /// </summary>
        protected Antoine.AntoineMediator _atnMediator { get; private set; }
        /// <summary>
        /// 仅允许库内部访问：指示中介者负责的计算类型
        /// </summary>
        protected CalType MediatorType { get; private set; }
        /// <summary>
        /// 仅允许自身及嵌套类访问：已知的环境变量 T 或者 P
        /// </summary>
        private double KnownVar { get; }
        /// <summary>
        /// 获取 饱和压力
        /// </summary>
        /// <param name="chem">指示组分</param>
        /// <param name="temperature">该参数在等压计算时为 T，等温计算时为 P，根据需要进行调用</param>
        /// <returns>饱和压力 Psi</returns>
        protected abstract double GetSaturatedPressure(Chemical chem, double temperature);
        /// <summary>
        /// 获取 饱和压力下的逸度
        /// </summary>
        /// <param name="chem">指示组分</param>
        /// <param name="unknownVar_TOrP">该参数在等压计算时为 T，等温计算时为 P，根据需要进行调用</param>
        /// <returns>饱和压力 Psi</returns>
        protected abstract IGasFugResult GetSaturatedFugacity(Chemical chem, double temperature);
        /// <summary>
        /// 获取 饱和压力下的逸度
        /// </summary>
        /// <param name="chem">指示组分</param>
        /// <param name="unknownVar_TOrP">该参数在等压计算时为 T，等温计算时为 P，根据需要进行调用</param>
        /// <returns>饱和压力 Psi</returns>
        protected abstract IGasFugResult GetFugacity(Chemical chem, double temperature, double pressure);
        /// <summary>
        /// 获取 饱和压力下的活度
        /// </summary>
        /// <param name="chem">指示组分</param>
        /// <param name="unknownVar_TOrP">该参数在等压计算时为 T，等温计算时为 P，根据需要进行调用</param>
        /// <returns>饱和压力 Psi</returns>
        protected abstract ActivityBaseModel.ActResult GetActivity(BinaryComplex liqComplex, double temperature);
        /// <summary>
        /// 获取 Poynting 因子
        /// </summary>
        /// <param name="chem">指示组分</param>
        /// <param name="unknownVar_TOrP">该参数在等压计算时为 T，等温计算时为 P，根据需要进行调用</param>
        /// <returns>获取 Poynting 因子</returns>
        protected abstract double GetPoyntingFactor(Chemical chem, double unknownVar_TOrP);
        /// <summary>
        /// 第一组分计算步骤
        /// </summary>
        protected List<Step> _fstComponentItrStepList = new List<Step> { };
        /// <summary>
        /// 第二组分计算步骤
        /// </summary>
        protected List<Step> _secComponentItrStepList = new List<Step> { };
        /// <summary>
        /// 进行计算
        /// </summary>
        public Result GetResult() {
            AlgorithmMediator algorithmMed = new AlgorithmMediator(this);
            double inilVar;
            if (MediatorType == CalType.IsobaricBubblePt || MediatorType == CalType.IsobaricDewPt) {
                inilVar = _atnMediator.GetBoilingTemperature(_knownComplex.FstComponent, KnownVar) * _knownComplex.FstComponentMoleFraction
                    + _atnMediator.GetBoilingTemperature(_knownComplex.SecComponent, KnownVar) * _knownComplex.SecComponentMoleFraction;
            } else if (MediatorType == CalType.IsothemalBubblePt || MediatorType == CalType.IsothemalDewPt) {
                inilVar = _atnMediator.GetSaturatedPressure(_knownComplex.FstComponent, KnownVar) * _knownComplex.FstComponentMoleFraction
                    + _atnMediator.GetSaturatedPressure(_knownComplex.SecComponent, KnownVar) * _knownComplex.SecComponentMoleFraction;
            } else {
                throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
            }
            Algorithm.VleBinIterator calItr = new Algorithm.VleBinIterator(algorithmMed, inilVar, 0.0001, 100);
            if (_fstComponentItrStepList.ToArray().Length != _secComponentItrStepList.ToArray().Length) {
                throw new IndexOutOfRangeException("两步骤记录次数不对等");
            }
            Step fstLastStep = _fstComponentItrStepList.ToArray()[0];
            Step secLastStep = _secComponentItrStepList.ToArray()[0];
            foreach (Step step in _fstComponentItrStepList) {
                fstLastStep = step;
            }
            foreach (Step step in _secComponentItrStepList) {
                secLastStep = step;
            }
            double var = calItr.Result;
            if (var == -1.0 || var == -2.0) {
                if (MediatorType == CalType.IsobaricBubblePt || MediatorType == CalType.IsobaricDewPt) {
                    return new Result(var, KnownVar, false, false, fstLastStep, secLastStep, _knownComplex, _fstComponentItrStepList, _secComponentItrStepList, MediatorType);
                } else if (MediatorType == CalType.IsothemalBubblePt || MediatorType == CalType.IsothemalDewPt) {
                    return new Result(KnownVar, var, false, false, fstLastStep, secLastStep, _knownComplex, _fstComponentItrStepList, _secComponentItrStepList, MediatorType);
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
            } else if (var == -3.0) {
                if (MediatorType == CalType.IsobaricBubblePt || MediatorType == CalType.IsobaricDewPt) {
                    return new Result(var, KnownVar, true, false, fstLastStep, secLastStep, _knownComplex, _fstComponentItrStepList, _secComponentItrStepList, MediatorType);
                } else if (MediatorType == CalType.IsothemalBubblePt || MediatorType == CalType.IsothemalDewPt) {
                    return new Result(KnownVar, var, true, false, fstLastStep, secLastStep, _knownComplex, _fstComponentItrStepList, _secComponentItrStepList, MediatorType);
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
            } else {
                if (MediatorType == CalType.IsobaricBubblePt || MediatorType == CalType.IsobaricDewPt) {
                    return new Result(var, KnownVar, false, true, fstLastStep, secLastStep, _knownComplex, _fstComponentItrStepList, _secComponentItrStepList, MediatorType);
                } else if (MediatorType == CalType.IsothemalBubblePt || MediatorType == CalType.IsothemalDewPt) {
                    return new Result(KnownVar, var, false, true, fstLastStep, secLastStep, _knownComplex, _fstComponentItrStepList, _secComponentItrStepList, MediatorType);
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
            }
        }
        /// <summary>
        /// 包含混合物相平衡计算的所有结果
        /// </summary>
        public class Result {
            public Result(double T, double P, bool isAborted, bool isConverged,
                Step fstLastStep, Step secLastStep, BinaryComplex complex, List<Step> fstStepList, List<Step> secStepList, CalType calType) {
                Temperature = T;
                Pressure = P;
                IsAborted = isAborted;
                IsConverged = isConverged;
                FstLastStep = fstLastStep;
                SecLastStep = secLastStep;
                TargetComplex = complex;
                FstStepList = fstStepList;
                SecStepList = secStepList;
                CalculateType = calType;
            }
            /// <summary>
            /// 指示计算类型
            /// </summary>
            public CalType CalculateType { get; private set; }
            /// <summary>
            /// 第一化合物迭代过程结果
            /// </summary>
            public List<Step> FstStepList { get; private set; }
            /// <summary>
            /// 计算是否被放弃
            /// </summary>
            public bool IsAborted { get; private set; }
            /// <summary>
            /// 计算是否收敛
            /// </summary>
            public bool IsConverged { get; private set; }
            /// <summary>
            /// 第一组分最终结果
            /// </summary>
            public Step FstLastStep { get; private set; }
            /// <summary>
            /// 第二组分最终结果
            /// </summary>
            public Step SecLastStep { get; private set; }
            /// <summary>
            /// 全局压力
            /// </summary>
            public double Pressure { get; private set; }
            /// <summary>
            /// 第二化合物迭代过程结果
            /// </summary>
            public List<Step> SecStepList { get; private set; }
            /// <summary>
            /// 目标二元混合物系
            /// </summary>
            public BinaryComplex TargetComplex { get; private set; }
            /// <summary>
            /// 全局温度
            /// </summary>
            public double Temperature { get; private set; }
        }
        /// <summary>
        /// 计算步骤的实例
        /// </summary>
        public class Step {
            public Step(double n, double act, double fug, double pS, double fugS, double poynting, IGasFugResult fugResult, IGasFugResult fugSResult, double T, double P) {
                Activity = act;
                FugacityResult = fugResult;
                Fugacity = fug;
                SaturatedFugacity = fugS;
                SaturatedPressure = pS;
                SaturatedFugacityResult = fugSResult;
                Poynting = poynting;
                MoleFraction = n;
                Temperature = T;
                Pressure = P;
            }
            public double Activity { get; private set; }
            public double Fugacity { get; private set; }
            public IGasFugResult FugacityResult { get; private set; }
            public double MoleFraction { get; private set; }
            public double Poynting { get; private set; }
            public double Pressure { get; private set; }
            public double SaturatedFugacity { get; private set; }
            public IGasFugResult SaturatedFugacityResult { get; private set; }
            public double SaturatedPressure { get; private set; }
            public double Temperature { get; private set; }
        }
        /// <summary>
        /// 混合物汽液平衡计算类型
        /// </summary>
        public enum CalType {
            /// <summary>
            /// 等压泡点 p x => T y
            /// </summary>
            IsobaricBubblePt = 1,
            /// <summary>
            /// 等压露点 p y => T x
            /// </summary>
            IsobaricDewPt = 2,
            /// <summary>
            /// 等温泡点 T x => P y
            /// </summary>
            IsothemalBubblePt = 4,
            /// <summary>
            /// 等温露点 T y => P x
            /// </summary>
            IsothemalDewPt = 8
        }
        /// <summary>
        /// 用于抽象算法交互 的内部类，相平衡准则 实际上在此类中实现
        /// </summary>
        internal class AlgorithmMediator : Algorithm.IVleBinItrMediator {
            public AlgorithmMediator(MixVleBaseMediator modelMediator) {
                _modelMed = modelMediator;
            }
            private MixVleBaseMediator _modelMed;
            #region Algorithm.IVleBinItrMediator 的显式实现
            /// <summary>
            /// 获取 x1 或者 y1，获取值的意义由外部类 calType 枚举类型 MediatorType 属性决定
            /// </summary>
            /// <param name="unknownVar">当前的 温度 或者 压力，该值的意义由外部类 calType 枚举类型 MediatorType 属性决定</param>
            /// <returns>x1 或者 y1</returns>
            double Algorithm.IVleBinItrMediator.GetN1(double unknownVar, double n1, double n2) {
                double pS, fugS, act, fug, poynting, n_1;
                IGasFugResult tempFugS, tempFug;
                BinaryComplex unknownComplex = new BinaryComplex(_modelMed._knownComplex.FstComponent, _modelMed._knownComplex.SecComponent, n1 / n2);
                //第一组分活度
                if (_modelMed.MediatorType == CalType.IsobaricDewPt) {
                    //P y => T x
                    act = _modelMed.GetActivity(unknownComplex, unknownVar).FstAct;
                } else if (_modelMed.MediatorType == CalType.IsobaricBubblePt) {
                    //P x => T y
                    act = _modelMed.GetActivity(_modelMed._knownComplex, unknownVar).FstAct;
                } else if (_modelMed.MediatorType == CalType.IsothemalDewPt) {
                    //T y => P x
                    act = _modelMed.GetActivity(unknownComplex, _modelMed.KnownVar).FstAct;
                } else if (_modelMed.MediatorType == CalType.IsothemalBubblePt) {
                    //T x => P y
                    act = _modelMed.GetActivity(_modelMed._knownComplex, _modelMed.KnownVar).FstAct;
                } else {
                    return -1.0;
                }
                if (_modelMed.MediatorType == CalType.IsobaricDewPt || _modelMed.MediatorType == CalType.IsobaricBubblePt) {
                    //P => T
                    //饱和压力
                    pS = _modelMed.GetSaturatedPressure(_modelMed._knownComplex.FstComponent, unknownVar);
                    //饱和压力下逸度 
                    tempFugS = _modelMed.GetSaturatedFugacity(_modelMed._knownComplex.FstComponent, unknownVar);
                    if (tempFugS != null) {
                        if (tempFugS.IsAborted) {
                            return -1.0;
                        }
                        fugS = tempFugS.GasFugacity;
                    } else {
                        fugS = 1.0;
                    }
                    //逸度
                    tempFug = _modelMed.GetFugacity(_modelMed._knownComplex.FstComponent, unknownVar, _modelMed.KnownVar);
                    if (tempFug != null) {
                        if (tempFug.IsAborted) {
                            return -1.0;
                        }
                        fug = tempFug.GasFugacity;
                    } else {
                        fug = 1.0;
                    }
                } else {
                    //T => P
                    //饱和压力
                    pS = _modelMed.GetSaturatedPressure(_modelMed._knownComplex.FstComponent, _modelMed.KnownVar);
                    //饱和压力下逸度 
                    tempFugS = _modelMed.GetSaturatedFugacity(_modelMed._knownComplex.FstComponent, _modelMed.KnownVar);
                    if (tempFugS != null) {
                        fugS = tempFugS.GasFugacity;
                        if (tempFugS.IsAborted) {
                            return -1.0;
                        }
                    } else {
                        fugS = 1.0;
                    }
                    //逸度
                    tempFug = _modelMed.GetFugacity(_modelMed._knownComplex.FstComponent, _modelMed.KnownVar, unknownVar);
                    if (tempFug != null) {
                        fug = tempFug.GasFugacity;
                        if (tempFug.IsAborted) {
                            return -1.0;
                        }
                    } else {
                        fug = 1.0;
                    }
                }
                poynting = _modelMed.GetPoyntingFactor(_modelMed._knownComplex.FstComponent, unknownVar);
                if (_modelMed.MediatorType == CalType.IsobaricBubblePt) {
                    // N1 = y1, unknownVar = T
                    n_1 = pS * fugS * act * _modelMed._knownComplex.FstComponentMoleFraction * poynting
                        / (_modelMed.KnownVar * fug);
                    _modelMed._fstComponentItrStepList.Add(new Step(n_1, act, fug, pS, fugS, poynting, tempFug, tempFugS, unknownVar, _modelMed.KnownVar));
                } else if (_modelMed.MediatorType == CalType.IsobaricDewPt) {
                    // N1 = x1, unknownVar = T
                    n_1 = _modelMed.KnownVar * _modelMed._knownComplex.FstComponentMoleFraction * fug
                        / (pS * fugS * act * poynting);
                    _modelMed._fstComponentItrStepList.Add(new Step(n_1, act, fug, pS, fugS, poynting, tempFug, tempFugS, unknownVar, _modelMed.KnownVar));
                } else if (_modelMed.MediatorType == CalType.IsothemalBubblePt) {
                    // N1 = y1, unknownVar = P
                    n_1 = pS * fugS * act * _modelMed._knownComplex.FstComponentMoleFraction * poynting
                        / (unknownVar * fug);
                    _modelMed._fstComponentItrStepList.Add(new Step(n_1, act, fug, pS, fugS, poynting, tempFug, tempFugS, _modelMed.KnownVar, unknownVar));
                } else if (_modelMed.MediatorType == CalType.IsothemalDewPt) {
                    // N1 = x1, unknownVar = P
                    n_1 = unknownVar * _modelMed._knownComplex.FstComponentMoleFraction * fug
                        / (pS * fugS * act * poynting);
                    _modelMed._fstComponentItrStepList.Add(new Step(n_1, act, fug, pS, fugS, poynting, tempFug, tempFugS, _modelMed.KnownVar, unknownVar));
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
                return n_1;
            }
            /// <summary>
            /// 获取 x2 或者 y2，获取值的意义由外部类 calType 枚举类型 MediatorType 属性决定
            /// </summary>
            /// <param name="unknownVar">当前的 温度 或者 压力，该值的意义由外部类 calType 枚举类型 MediatorType 属性决定</param>
            /// <returns>x2 或者 y2</returns>
            double Algorithm.IVleBinItrMediator.GetN2(double unknownVar, double n1, double n2) {
                double pS, fugS, act, fug, poynting, n_2;
                IGasFugResult tempFugS, tempFug;
                BinaryComplex unknownComplex = new BinaryComplex(_modelMed._knownComplex.FstComponent, _modelMed._knownComplex.SecComponent, n1 / n2);
                //第二组分活度
                if (_modelMed.MediatorType == CalType.IsobaricDewPt) {
                    act = _modelMed.GetActivity(unknownComplex, unknownVar).SecAct;
                } else if (_modelMed.MediatorType == CalType.IsobaricBubblePt) {
                    act = _modelMed.GetActivity(_modelMed._knownComplex, unknownVar).SecAct;
                } else if (_modelMed.MediatorType == CalType.IsothemalDewPt) {
                    act = _modelMed.GetActivity(unknownComplex, _modelMed.KnownVar).SecAct;
                } else if (_modelMed.MediatorType == CalType.IsothemalBubblePt) {
                    act = _modelMed.GetActivity(_modelMed._knownComplex, _modelMed.KnownVar).SecAct;
                } else {
                    return -1.0;
                }
                //第二组分饱和压力/逸度，逸度
                if (_modelMed.MediatorType == CalType.IsobaricDewPt || _modelMed.MediatorType == CalType.IsobaricBubblePt) {
                    //饱和压力
                    pS = _modelMed.GetSaturatedPressure(_modelMed._knownComplex.SecComponent, unknownVar);
                    //饱和压力下逸度 
                    tempFugS = _modelMed.GetSaturatedFugacity(_modelMed._knownComplex.SecComponent, unknownVar);
                    if (tempFugS != null) {
                        if (tempFugS.IsAborted) {
                            return -1.0;
                        }
                        fugS = tempFugS.GasFugacity;
                    } else {
                        fugS = 1.0;
                    }
                    //逸度
                    tempFug = _modelMed.GetFugacity(_modelMed._knownComplex.SecComponent, unknownVar, _modelMed.KnownVar);
                    if (tempFug != null) {
                        if (tempFugS.IsAborted) {
                            return -1.0;
                        }
                        fug = tempFug.GasFugacity;
                    } else {
                        fug = 1.0;
                    }
                } else {
                    //饱和压力
                    pS = _modelMed.GetSaturatedPressure(_modelMed._knownComplex.SecComponent, _modelMed.KnownVar);
                    //饱和压力下逸度 
                    tempFugS = _modelMed.GetSaturatedFugacity(_modelMed._knownComplex.SecComponent, _modelMed.KnownVar);
                    if (tempFugS != null) {
                        fugS = tempFugS.GasFugacity;
                        if (tempFugS.IsAborted) {
                            return -1.0;
                        }
                    } else {
                        fugS = 1.0;
                    }
                    //逸度
                    tempFug = _modelMed.GetFugacity(_modelMed._knownComplex.SecComponent, _modelMed.KnownVar, unknownVar);
                    if (tempFug != null) {
                        fug = tempFug.GasFugacity;
                        if (tempFug.IsAborted) {
                            return -1.0;
                        }
                    } else {
                        fug = 1.0;
                    }
                }
                //Poynting因子
                poynting = _modelMed.GetPoyntingFactor(_modelMed._knownComplex.SecComponent, unknownVar);
                if (_modelMed.MediatorType == CalType.IsobaricBubblePt) {
                    // N1 = y1, unknownVar = T
                    n_2 = pS * fugS * act * _modelMed._knownComplex.SecComponentMoleFraction * poynting
                        / (_modelMed.KnownVar * fug);
                    _modelMed._secComponentItrStepList.Add(new Step(n_2, act, fug, pS, fugS, poynting, tempFug, tempFugS, unknownVar, _modelMed.KnownVar));
                } else if (_modelMed.MediatorType == CalType.IsobaricDewPt) {
                    // N1 = x1, unknownVar = T
                    n_2 = _modelMed.KnownVar * _modelMed._knownComplex.SecComponentMoleFraction * fug
                        / (pS * fugS * act * poynting);
                    _modelMed._secComponentItrStepList.Add(new Step(n_2, act, fug, pS, fugS, poynting, tempFug, tempFugS, unknownVar, _modelMed.KnownVar));
                } else if (_modelMed.MediatorType == CalType.IsothemalBubblePt) {
                    // N1 = y1, unknownVar = P
                    n_2 = pS * fugS * act * _modelMed._knownComplex.SecComponentMoleFraction * poynting
                        / (unknownVar * fug);
                    _modelMed._secComponentItrStepList.Add(new Step(n_2, act, fug, pS, fugS, poynting, tempFug, tempFugS, _modelMed.KnownVar, unknownVar));
                } else if (_modelMed.MediatorType == CalType.IsothemalDewPt) {
                    // N1 = x1, unknownVar = P
                    n_2 = unknownVar * _modelMed._knownComplex.SecComponentMoleFraction * fug
                        / (pS * fugS * act * poynting);
                    _modelMed._secComponentItrStepList.Add(new Step(n_2, act, fug, pS, fugS, poynting, tempFug, tempFugS, _modelMed.KnownVar, unknownVar));
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
                return n_2;
            }
            /// <summary>
            /// 温度 或者 压力 的迭代式
            /// </summary>
            /// <param name="unknownVar">温度 或者 压力，该值的意义由外部类 calType 枚举类型 MediatorType 属性决定</param>
            /// <param name="n1">x2 或者 y2，该值的意义由外部类 calType 枚举类型 MediatorType 属性决定</param>
            /// <param name="n2">x2 或者 y2，该值的意义由外部类 calType 枚举类型 MediatorType 属性决定</param>
            /// <returns>T or P</returns>
            double Algorithm.IVleBinItrMediator.ItrFomula(double unknownVar, double n1, double n2) {
                double sum_n, nextUnknownVar;
                sum_n = n1 + n2;
                if (_modelMed.MediatorType == CalType.IsobaricBubblePt || _modelMed.MediatorType == CalType.IsothemalDewPt) {
                    //T = (1 + 0.1 * (1 - ΣYi)) * T | P = (1 + 0.1 * (1 - ΣXi)) * P;
                    nextUnknownVar = (1 + 0.1 * (1 - sum_n)) * unknownVar;
                } else if (_modelMed.MediatorType == CalType.IsobaricDewPt || _modelMed.MediatorType == CalType.IsothemalBubblePt) {
                    //T = (1 - 0.1 * (1 - ΣYi)) * T | P = (1 - 0.1 * (1 - ΣXi)) * P;
                    nextUnknownVar = (1 - 0.1 * (1 - sum_n)) * unknownVar;
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
                return nextUnknownVar;
            }
            /// <summary>
            /// N1 初值
            /// </summary>
            /// <param name="unknownVar"></param>
            /// <returns></returns>
            double Algorithm.IVleBinItrMediator.GetFstN1(double unknownVar) {
                double n1_Inil;
                if (_modelMed.MediatorType == CalType.IsobaricBubblePt) {
                    // N1 = y1, unknownVar = T 
                    n1_Inil = _modelMed._knownComplex.FstComponentMoleFraction * _modelMed.GetSaturatedPressure(_modelMed._knownComplex.FstComponent, unknownVar) / _modelMed.KnownVar;
                } else if (_modelMed.MediatorType == CalType.IsothemalBubblePt) {
                    // N1 = y1, unknownVar = P 
                    n1_Inil = _modelMed._knownComplex.FstComponentMoleFraction * _modelMed.GetSaturatedPressure(_modelMed._knownComplex.FstComponent, unknownVar) / unknownVar;
                } else if (_modelMed.MediatorType == CalType.IsobaricDewPt) {
                    // N1 = x1, unknownVar = T 
                    n1_Inil = _modelMed._knownComplex.FstComponentMoleFraction * _modelMed.KnownVar / _modelMed.GetSaturatedPressure(_modelMed._knownComplex.FstComponent, unknownVar);
                } else if (_modelMed.MediatorType == CalType.IsothemalDewPt) {
                    // N1 = x1, unknownVar = P 
                    n1_Inil = _modelMed._knownComplex.FstComponentMoleFraction * unknownVar / _modelMed.GetSaturatedPressure(_modelMed._knownComplex.FstComponent, unknownVar);
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
                return n1_Inil;
            }
            /// <summary>
            /// N2 初值
            /// </summary>
            /// <param name="unknownVar"></param>
            /// <returns></returns>
            double Algorithm.IVleBinItrMediator.GetFstN2(double unknownVar) {
                double n2_Inil;
                if (_modelMed.MediatorType == CalType.IsobaricBubblePt) {
                    // N2 = y2, unknownVar = T
                    n2_Inil = _modelMed._knownComplex.SecComponentMoleFraction * _modelMed.GetSaturatedPressure(_modelMed._knownComplex.SecComponent, unknownVar) / _modelMed.KnownVar;
                } else if (_modelMed.MediatorType == CalType.IsobaricDewPt) {
                    // N2 = x2, unknownVar = T 
                    n2_Inil = _modelMed._knownComplex.SecComponentMoleFraction * _modelMed.KnownVar / _modelMed.GetSaturatedPressure(_modelMed._knownComplex.SecComponent, unknownVar);
                } else if (_modelMed.MediatorType == CalType.IsothemalBubblePt) {
                    // N2 = y2, unknownVar = P 
                    n2_Inil = _modelMed._knownComplex.SecComponentMoleFraction * _modelMed.GetSaturatedPressure(_modelMed._knownComplex.SecComponent, unknownVar) / unknownVar;
                } else if (_modelMed.MediatorType == CalType.IsothemalDewPt) {
                    // N2 = x2, unknownVar = P 
                    n2_Inil = _modelMed._knownComplex.SecComponentMoleFraction * unknownVar / _modelMed.GetSaturatedPressure(_modelMed._knownComplex.SecComponent, unknownVar);
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
                return n2_Inil;
            }
            #endregion
        }
        #region MixRules.IMixFluidModel 的显式实现
        //私有混合物成员
        protected BinaryComplex _knownComplex;
        /// <summary>
        /// 二元混合物
        /// </summary>
        BinaryComplex MixRules.IMixFluidModel.Complex
            => _knownComplex;
        #endregion
    }
    /// <summary> 
    /// 真实系等温泡点汽液平衡
    /// </summary>
    public class MixActRealisticVleMediator : MixVleBaseMediator {
        public MixActRealisticVleMediator(CalType calType, GasFugacityMediator fugMediator, ActivityMediator actMediator, Antoine.AntoineMediator atnMediator, BinaryComplex liqComplex, double actualTemperature)
            : base(fugMediator, actMediator, atnMediator, liqComplex, actualTemperature, calType) { }

        protected override ActivityBaseModel.ActResult GetActivity(BinaryComplex liqComplex, double temperature)
            => _actMediator.GetAct(liqComplex, temperature);

        protected override IGasFugResult GetFugacity(Chemical chem, double temperature, double pressure)
            => _fugMediator.GetFugacity(chem, temperature, pressure);

        protected override double GetPoyntingFactor(Chemical chem, double unknownVar_TorP)
            => 1.0;

        protected override IGasFugResult GetSaturatedFugacity(Chemical chem, double temperature)
            => _fugMediator.GetFugacity(chem, temperature, _atnMediator.GetSaturatedPressure(chem, temperature));

        protected override double GetSaturatedPressure(Chemical chem, double temperature) {
            return _atnMediator.GetSaturatedPressure(chem, temperature);
        }
    }
    /// <summary>
    /// 半理想系等温泡点汽液平衡
    /// </summary>
    public class MixActSemiIdealMediator : MixActRealisticVleMediator {
        public MixActSemiIdealMediator(CalType calType, ActivityMediator actMediator, Antoine.AntoineMediator atnMediator, BinaryComplex liqComplex, double actualTemperature)
            : base(calType, null, actMediator, atnMediator, liqComplex, actualTemperature) { }

        protected override IGasFugResult GetFugacity(Chemical chem, double temperature, double pressure)
            => null;

        protected override IGasFugResult GetSaturatedFugacity(Chemical chem, double temperature)
            => null;
    }
    /// <summary>
    /// 全理想系等温泡点汽液平衡
    /// </summary>
    public class MixActIdealMediator : MixActSemiIdealMediator {
        public MixActIdealMediator(CalType calType, Antoine.AntoineMediator atnMediator, BinaryComplex liqComplex, double actualTemperature)
            : base(calType, null, atnMediator, liqComplex, actualTemperature) { }

        protected override ActivityBaseModel.ActResult GetActivity(BinaryComplex liqComplex, double temperature)
            => new ActivityBaseModel.ActResult(1.0, 1.0);
    }
}
/// <summary>
/// 活度系数
/// </summary>
namespace ChemEngThermCal.Model.Vle.ActCoe.Models {
    /// <summary>
    /// 活度系数模型中介者
    /// </summary>
    public class ActivityMediator {
        /// <summary>
        /// 活度系数模型中介者
        /// </summary>
        /// <param name="targetModel">要与之通讯的活度系数模型</param>
        public ActivityMediator(ActivityBaseModel targetModel) {
            _model = targetModel;
        }
        //持有的活度模型
        private ActivityBaseModel _model;
        /// <summary>
        /// 获取 活度系数，返回 new ActResult(-1, -1) 时为子类型继承错误，不显式抛出异常
        /// </summary>
        /// <param name="liqComplex">液相混合物</param>
        /// <param name="temperature">当前温度</param>
        /// <returns>new ActResult(fstAct, secAct, double[] relevants)</returns>
        public ActivityBaseModel.ActResult GetAct(BinaryComplex liqComplex, double temperature) {
            if (_model is ClassicActivityBaseModel) {
                return (_model as ClassicActivityBaseModel).GetActResult(liqComplex);
            } else if (_model is AdvancedActivityBaseModel) {
                return (_model as AdvancedActivityBaseModel).GetActResult(liqComplex, temperature);
            } else {
                return new ActivityBaseModel.ActResult(-1, -1);
            }
        }
    }
    /// <summary>
    /// 活度系数模型的基类型
    /// </summary>
    public abstract class ActivityBaseModel {
        /// <summary>
        /// 活度模型的基类型
        /// </summary>
        /// <param name="modelPara12">模型参数 12</param>
        /// <param name="modelPara21">模型参数 21</param>
        public ActivityBaseModel(double modelPara12, double modelPara21) {
            ModelPara12 = modelPara12;
            ModelPara21 = modelPara21;
        }
        /// <summary>
        /// 模型参数 12
        /// </summary>
        public double ModelPara12 { get; private set; }
        /// <summary>
        /// 模型参数 21
        /// </summary>
        public double ModelPara21 { get; private set; }
        /// <summary>
        /// 包含活度系数计算结果的全部对象
        /// </summary>
        public class ActResult {
            /// <summary>
            /// 实例化一个 活度系数结果
            /// </summary>
            /// <param name="fstAct"></param>
            /// <param name="secAct"></param>
            /// <param name="relevants"></param>
            public ActResult(double fstAct, double secAct) {
                FstAct = fstAct;
                SecAct = secAct;
            }
            /// <summary>
            /// 第一化合物的活度系数
            /// </summary>
            public double FstAct { get; private set; }
            /// <summary>
            /// 第二化合物的活度系数
            /// </summary>
            public double SecAct { get; private set; }
        }
    }
    /// <summary>
    /// 经典活度系数模型的基类：与温度无关的活度模型
    /// </summary>
    public abstract class ClassicActivityBaseModel : ActivityBaseModel {
        /// <summary>
        /// 经典活度系数模型的基类：与温度无关的活度模型
        /// </summary>
        /// <param name="modelPara12">模型参数 12</param>
        /// <param name="modelPara21">模型参数 21</param>
        public ClassicActivityBaseModel(double modelPara12, double modelPara21)
            : base(modelPara12, modelPara21) { }
        /// <summary>
        /// 获取活度系数组
        /// </summary>
        /// <param name="liqComplex">液相混合物</param>
        /// <returns>活度系数组</returns>
        public abstract ActResult GetActResult(BinaryComplex liqComplex);
    }
    /// <summary>
    /// 对称性系数模型
    /// </summary>
    public class SymmetryModel : ClassicActivityBaseModel {
        /// <summary>
        /// 对称性系数模型
        /// </summary>
        /// <param name="A">模型系数</param>
        public SymmetryModel(double A) : base(A, A) { }
        /// <summary>
        /// 获取活度结果
        /// </summary>
        /// <param name="liqComplex">液相二元混合物</param>
        /// <returns>获取活度计算的结果</returns>
        public override ActResult GetActResult(BinaryComplex liqComplex) {
            double fstActCoe = Math.Pow(Math.E, (ModelPara12 * Math.Pow(liqComplex.SecComponentMoleFraction, 2.0)));
            double secActCoe = Math.Pow(Math.E, (ModelPara21 * Math.Pow(liqComplex.FstComponentMoleFraction, 2.0)));
            return new ActResult(fstActCoe, secActCoe);
        }
    }
    /// <summary>
    /// Margules 活度系数模型
    /// </summary>
    public class MargulesModel : ClassicActivityBaseModel {
        /// <summary>
        /// Margules 活度系数模型
        /// </summary>
        /// <param name="A12">参数12</param>
        /// <param name="A21">参数21</param>
        public MargulesModel(double A12, double A21) : base(A12, A21) { }
        /// <summary>
        /// 获取活度结果
        /// </summary>
        /// <param name="liqComplex">液相二元混合物</param>
        /// <returns>获取活度计算的结果</returns>
        public override ActResult GetActResult(BinaryComplex liqComplex) {
            double fstActCoe = Math.Pow(Math.E, (ModelPara12 + 2.0 * (ModelPara21 - ModelPara12) * liqComplex.FstComponentMoleFraction) * Math.Pow(liqComplex.SecComponentMoleFraction, 2.0));
            double secActCoe = Math.Pow(Math.E, (ModelPara21 + 2.0 * (ModelPara12 - ModelPara21) * liqComplex.SecComponentMoleFraction) * Math.Pow(liqComplex.FstComponentMoleFraction, 2.0));
            return new ActResult(fstActCoe, secActCoe);
        }
    }
    /// <summary>
    /// Van Laar 活度系数模型
    /// </summary>
    public class VanLaarModel : ClassicActivityBaseModel {
        /// <summary>
        /// VanLaar 活度系数模型
        /// </summary>
        /// <param name="A12">参数12</param>
        /// <param name="A21">参数21</param>
        public VanLaarModel(double A12, double A21) : base(A12, A21) { }
        /// <summary>
        /// 获取活度结果
        /// </summary>
        /// <param name="liqComplex">液相二元混合物</param>
        /// <returns>获取活度计算的结果</returns>
        public override ActResult GetActResult(BinaryComplex liqComplex) {
            double fstActCoe = Math.Pow(Math.E, ModelPara12 * Math.Pow(ModelPara21 * liqComplex.SecComponentMoleFraction / (ModelPara12 * liqComplex.FstComponentMoleFraction + ModelPara21 * liqComplex.SecComponentMoleFraction), 2.0));
            double secActCoe = Math.Pow(Math.E, ModelPara21 * Math.Pow(ModelPara12 * liqComplex.FstComponentMoleFraction / (ModelPara12 * liqComplex.FstComponentMoleFraction + ModelPara21 * liqComplex.SecComponentMoleFraction), 2.0));
            return new ActResult(fstActCoe, secActCoe);
        }
    }
    /// <summary>
    /// Wilson 活度系数模型
    /// </summary>
    public class WilsonModel : ClassicActivityBaseModel {
        /// <summary>
        /// Wilson 活度系数模型
        /// </summary>
        /// <param name="A12">参数12</param>
        /// <param name="A21">参数21</param>
        public WilsonModel(double A12, double A21) : base(A12, A21) { }
        /// <summary>
        /// 获取活度结果
        /// </summary>
        /// <param name="liqComplex">液相二元混合物</param>
        /// <returns>获取活度计算的结果</returns>
        public override ActResult GetActResult(BinaryComplex liqComplex) {
            double fstActCoe = Math.Pow(
                Math.E,
                0.0 - Math.Log(liqComplex.FstComponentMoleFraction + ModelPara12 * liqComplex.SecComponentMoleFraction)
                + liqComplex.SecComponentMoleFraction
                     * (ModelPara12 / (liqComplex.FstComponentMoleFraction + ModelPara12 * liqComplex.SecComponentMoleFraction)
                        - ModelPara21 / (liqComplex.SecComponentMoleFraction + ModelPara21 * liqComplex.FstComponentMoleFraction)
                       )
                    );
            double secActCoe = Math.Pow(
                Math.E,
                0.0 - Math.Log(liqComplex.SecComponentMoleFraction + ModelPara21 * liqComplex.FstComponentMoleFraction)
                + liqComplex.FstComponentMoleFraction
                    * (ModelPara21 / (liqComplex.SecComponentMoleFraction + ModelPara21 * liqComplex.FstComponentMoleFraction)
                        - ModelPara12 / (liqComplex.FstComponentMoleFraction + ModelPara12 * liqComplex.SecComponentMoleFraction)
                       )
                    );
            return new ActResult(fstActCoe, secActCoe);
        }
    }
    /// <summary>
    /// 改进活度系数模型的基类：与温度相关的活度模型
    /// </summary>
    public abstract class AdvancedActivityBaseModel : ActivityBaseModel {
        /// <summary>
        /// 改进活度系数模型的基类：与温度相关的活度模型
        /// </summary>
        /// <param name="modelPara12">模型参数 12</param>
        /// <param name="modelPara21">模型参数 21</param>
        public AdvancedActivityBaseModel(double modelPara12, double modelPara21)
            : base(modelPara12, modelPara21) { }
        /// <summary>
        /// 获取活度系数组
        /// </summary>
        /// <param name="liqComplex">液相混合物</param>
        /// <param name="actualTemperature">当前温度</param>
        /// <returns>活度系数组</returns>
        public abstract ActResult GetActResult(BinaryComplex liqComplex, double temperature);
    }
    /// <summary>
    /// Wilson 能量参数 活度系数模型
    /// </summary>
    public class WilsonEnergyParaModel : AdvancedActivityBaseModel {
        /// <summary>
        /// 实例化一个 Wilson模型参数 活度系数模型计算模型。
        /// </summary>
        /// <param name="lambda12">能量参数 λ12 - λ11</param>
        /// <param name="lambda21">能量参数 λ21 - λ22</param>
        public WilsonEnergyParaModel(double lambda12, double lambda21)
            : base(lambda12, lambda21) { }
        /// <summary>
        /// 获取活度结果，其 relevant 为：[ A12 , A21 ]
        /// </summary>
        /// <param name="liqComplex">液相二元混合物</param>
        /// <param name="temperature">温度</param>
        /// <returns>活度系数的计算结果</returns>
        public override ActResult GetActResult(BinaryComplex liqComplex, double temperature) {
            double A12 =
                Rackett.GetSaturateLiquidVolume(liqComplex.SecComponent, temperature)
                / Rackett.GetSaturateLiquidVolume(liqComplex.FstComponent, temperature)
                * Math.Exp((0.0 - ModelPara12) / (8.314 * temperature));
            double A21 =
                Rackett.GetSaturateLiquidVolume(liqComplex.FstComponent, temperature)
                / Rackett.GetSaturateLiquidVolume(liqComplex.SecComponent, temperature)
                * Math.Exp((0.0 - ModelPara21) / (8.314 * temperature));
            ActResult result = new WilsonModel(A12, A21).GetActResult(liqComplex);
            return new WilsonEnergyResult(result.FstAct, result.SecAct, A12, A21);
        }
        /// <summary>
        /// Wilson 能量参数 活度模型计算结果
        /// </summary>
        public class WilsonEnergyResult : ActResult {
            public WilsonEnergyResult(double fstAct, double secAct, double a12, double a21)
                : base(fstAct, secAct) {
                A12 = a12;
                A21 = a21;
            }
            public double A12 { get; private set; }
            public double A21 { get; private set; }
        }
    }
    /// <summary>
    /// NTRL 活度系数模型
    /// </summary>
    public class NrtlModel : AdvancedActivityBaseModel {
        /// <summary>
        /// 实例化一个 NTRL 活度系数模型计算模型。
        /// </summary>
        /// <param name="g12">g12-g11</param>
        /// <param name="g21">g21-g22</param>
        /// <param name="alpha">α</param>
        public NrtlModel(double g12, double g21, double alpha)
            : base(g12, g21) {
            _alpha = alpha;
        }
        /// <summary>
        /// α
        /// </summary>
        private double _alpha;
        /// <summary>
        /// 获取活度结果，其 relevant 为：[ tau12, tau21, G12, G21 ]
        /// </summary>
        /// <param name="liqComplex">液相二元混合物</param>
        /// <param name="temperature">温度</param>
        /// <returns>活度系数的计算结果</returns>
        public override ActResult GetActResult(BinaryComplex liqComplex, double temperature) {
            double tau12 = ModelPara12 / (8.314 * temperature);
            double tau21 = ModelPara21 / (8.314 * temperature);
            double G12 = Math.Exp(0.0 - tau12 * _alpha);
            double G21 = Math.Exp(0.0 - tau21 * _alpha);
            double fstActCoe = Math.Pow(Math.E,
                Math.Pow(liqComplex.SecComponentMoleFraction, 2.0)
                * (
                    tau21 * Math.Pow(G21, 2.0) / Math.Pow(liqComplex.FstComponentMoleFraction + liqComplex.SecComponentMoleFraction * G21, 2.0)
                    + tau12 * G12 / Math.Pow(liqComplex.SecComponentMoleFraction + liqComplex.FstComponentMoleFraction * G12, 2.0)
                ));
            double secActCoe = Math.Pow(Math.E,
                Math.Pow(liqComplex.FstComponentMoleFraction, 2.0)
                * (
                    tau12 * Math.Pow(G12, 2.0) / Math.Pow(liqComplex.SecComponentMoleFraction + liqComplex.FstComponentMoleFraction * G12, 2.0)
                    + tau21 * G21 / Math.Pow(liqComplex.FstComponentMoleFraction + liqComplex.SecComponentMoleFraction * G21, 2.0)
                ));
            return new NrtlResult(fstActCoe, secActCoe, tau12, tau21, G12, G21);
        }
        /// <summary>
        /// NRTL 活度模型计算结果
        /// </summary>
        public class NrtlResult : ActResult {
            public NrtlResult(double fstAct, double secAct, double tau12, double tau21, double g12, double g21)
                : base(fstAct, secAct) {
                Tau12 = tau12;
                Tau21 = tau21;
                G12 = g12;
                G21 = g21;
            }
            public double G12 { get; private set; }
            public double G21 { get; private set; }
            public double Tau12 { get; private set; }
            public double Tau21 { get; private set; }
        }
    }
}
/// <summary>
/// 逸度系数
/// </summary>
namespace ChemEngThermCal.Model.Vle.FugCoe {
    /// <summary>
    /// 逸度中介者
    /// </summary>
    public class GasFugacityMediator {
        /// <summary>
        /// 初始化一个 逸度中介器
        /// </summary>
        /// <param name="targetModelAdapter">逸度适配器</param>
        public GasFugacityMediator(IGasApplibleFugAdapter targetModelAdapter) {
            _modelAdapter = targetModelAdapter;
        }
        //私有逸度适配器
        private IGasApplibleFugAdapter _modelAdapter;
        /// <summary>
        /// 获取气相逸度
        /// </summary>
        /// <param name="T">温度</param>
        /// <param name="P">压力</param>
        /// <returns></returns>
        public IGasFugResult GetFugacity(Chemical chemical, double T, double P) {
            return _modelAdapter.GetGasFugResult(chemical, T, P);
        }
    }
    /// <summary>
    /// 接口：能用应用于气相的逸度适配器
    /// </summary>
    public interface IGasApplibleFugAdapter {
        IGasFugResult GetGasFugResult(Chemical chemical, double T, double P);
    }
    /// <summary>
    /// 接口：指示包含气相逸度计算结果
    /// </summary>
    public interface IGasFugResult {
        double GasFugacity { get; }
        bool IsAborted { get; }
    }
    /// <summary>
    /// Ceos 逸度系数适配器
    /// </summary>
    public class CeosFugAdapter : IGasApplibleFugAdapter {
        /// <summary>
        /// Ceos 逸度系数适配器
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="targetModel">实现了 Ceos 逸度计算接口的对象</param>
        public CeosFugAdapter(Ceos.Models.LiqApplicableCeos targetModel) {
            Model = targetModel;
        }
        //持有的 Ceos 逸度计算对象
        public Ceos.Models.LiqApplicableCeos Model { private set; get; }
        /// <summary>
        /// 计算 气相逸度系数
        /// </summary>
        /// <param name="T">温度</param>
        /// <param name="P">压力</param>
        /// <returns>CeosFugResult 对象</returns>
        public IGasFugResult GetGasFugResult(Chemical chemical, double T, double P) {
            if (Model is Ceos.Models.CeosBaseModel) {
                Ceos.CalMoleVolume.CeosPureMediator.Result calResult
                    = new Ceos.CalMoleVolume.CeosPureMediator(Model as Ceos.Models.CeosBaseModel, chemical, T, P).GetResult();
                double gasCpr = calResult.GasResult.CprFactor;
                double liqCpr = calResult.LiqResult.CprFactor;
                double gasFug = Model.GetFugacityCoe(chemical, gasCpr, T, P);
                double liqFug = Model.GetFugacityCoe(chemical, liqCpr, T, P);
                return new GasFugResult(gasFug, calResult, false);
            } else {
                return new GasFugResult(-3.0, null, true);
            }
        }
        /// <summary>
        /// Ceos 模型逸度系数计算的结果, 包含气相逸度
        /// </summary>
        public class GasFugResult : IGasFugResult {
            /// <summary>
            /// Ceos 模型逸度系数计算结果
            /// </summary>
            /// <param name="gasFug"></param>
            /// <param name="cprFactor"></param>
            /// <param name="moleVolume"></param>
            /// <param name="stepList"></param>
            public GasFugResult(double gasFug, Ceos.CalMoleVolume.CeosPureMediator.Result eosResult, bool isAborted) {
                GasFugacity = gasFug;
                EosResult = eosResult;
                isAborted = IsAborted;
            }
            /// <summary>
            /// 包含 EOS 计算的所有内容
            /// </summary>
            public Ceos.CalMoleVolume.CeosPureMediator.Result EosResult { get; private set; }
            /// <summary>
            /// 计算是否被放弃
            /// </summary>
            public bool IsAborted { get; private set; }
            /// <summary>
            /// 气相逸度系数
            /// </summary>
            public double GasFugacity { get; private set; }
        }
        /// <summary>
        /// 计算 气相及液相 逸度系数
        /// </summary>
        /// <param name="T">温度</param>
        /// <param name="P">压力</param>
        /// <returns>CeosFugResult 对象</returns>
        public LiqFugResult GetLiqFugResult(Chemical chemical, double T, double P) {
            if (Model is Ceos.Models.CeosBaseModel) {
                Ceos.CalMoleVolume.CeosPureMediator.Result calResult
                    = new Ceos.CalMoleVolume.CeosPureMediator(Model, chemical, T, P).GetResult();
                double gasCpr = calResult.GasResult.CprFactor;
                double liqCpr = calResult.LiqResult.CprFactor;
                double gasFug = Model.GetFugacityCoe(chemical, gasCpr, T, P);
                double liqFug = Model.GetFugacityCoe(chemical, liqCpr, T, P);
                return new LiqFugResult(gasFug, liqFug, calResult, false);
            } else {
                return new LiqFugResult(-3.0, -3.0, null, true);
            }
        }
        /// <summary>
        /// Ceos 模型逸度系数计算的结果, 包含气相及液相逸度
        /// </summary>
        public class LiqFugResult : GasFugResult {
            /// <summary>
            /// Ceos 模型逸度系数计算结果
            /// </summary>
            /// <param name="gasFug"></param>
            /// <param name="cprFactor"></param>
            /// <param name="moleVolume"></param>
            /// <param name="stepList"></param>
            public LiqFugResult(double gasFug, double liqFug, Ceos.CalMoleVolume.CeosPureMediator.Result eosResult, bool isAborted)
                : base(gasFug, eosResult, isAborted) {
                LiqFugacity = liqFug;
            }
            /// <summary>
            /// 液相逸度系数
            /// </summary>
            public double LiqFugacity { get; private set; }
        }
    }
    /// <summary>
    /// SV 逸度系数适配器
    /// </summary>
    public class SecVirialFugacityyAdapter : IGasApplibleFugAdapter {
        /// <summary>
        /// SV 逸度系数适配器
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="targetModel">SecVirial 对象</param>
        public SecVirialFugacityyAdapter(CorStt.Models.SecVirial.SecVirialBaseModel targetModel) {
            _model = targetModel;
        }
        //持有的 SecVirial 逸度计算模型
        private CorStt.Models.SecVirial.SecVirialBaseModel _model;
        /// <summary>
        /// 计算 气相逸度系数
        /// </summary>
        /// <param name="T">温度</param>
        /// <param name="P">压力</param>
        /// <returns>CeosFugResult 对象</returns>
        public IGasFugResult GetGasFugResult(Chemical chemical, double T, double P) {
            double fug = _model.GetFugacityCoe(chemical, T, P);
            CorStt.CalMoleVolume.UsingSecVirial.SecVirialPureMediator.Result calResult
                = new CorStt.CalMoleVolume.UsingSecVirial.SecVirialPureMediator(_model, chemical, T, P).GetResult();
            double tr = T / chemical.CriticalTemperature;
            double pr = P / chemical.CriticalPressure;
            double svBase = calResult.B0;
            double svCrec = calResult.B1;
            double sv = calResult.B;
            double cpr = calResult.GasResult.CprFactor;
            double vol = calResult.GasResult.MoleVol;
            return new GasFugResult(fug, tr, pr, svBase, svCrec, sv, cpr, vol);
        }
        /// <summary>
        /// SV 模型逸度系数计算的结果
        /// </summary>
        public class GasFugResult : IGasFugResult {
            /// <summary>
            /// SV 模型逸度系数计算的结果
            /// </summary>
            /// <param name="fugacity">逸度系数</param>
            /// <param name="tr">相对温度</param>
            /// <param name="pr">相对压力</param>
            /// <param name="svBase">B0</param>
            /// <param name="svCrec">B1</param>
            /// <param name="sv">B^</param>
            public GasFugResult(double fugacity, double tr, double pr, double svBase, double svCrec, double sv, double cpr, double vol, bool isAborted = false) {
                GasFugacity = fugacity;
                RelativeTemperature = tr;
                RelativePressure = pr;
                SecVirialBase = svBase;
                SecVirialCrec = svCrec;
                SecVirialCoe = sv;
                GasCprFactor = cpr;
                GasMoleVol = vol;
                IsAborted = isAborted;
            }
            /// <summary>
            /// 逸度系数
            /// </summary>
            public double GasFugacity { get; private set; }
            /// <summary>
            /// 计算是否被放弃
            /// </summary>
            public bool IsAborted { get; private set; }
            /// <summary>
            /// 相对压力
            /// </summary>
            public double RelativePressure { get; private set; }
            /// <summary>
            /// 相对温度
            /// </summary>
            public double RelativeTemperature { get; private set; }
            /// <summary>
            /// B0
            /// </summary>
            public double SecVirialBase { get; private set; }
            /// <summary>
            /// B1
            /// </summary>
            public double SecVirialCoe { get; private set; }
            /// <summary>
            /// B^
            /// </summary>
            public double SecVirialCrec { get; private set; }
            /// <summary>
            /// Z
            /// </summary>
            public double GasCprFactor { get; private set; }
            /// <summary>
            /// B^
            /// </summary>
            public double GasMoleVol { get; private set; }
        }
    }
    /// <summary>
    /// 查图法 逸度系数适配器
    /// </summary>
    public class DiagramFugacityyAdapter : IGasApplibleFugAdapter {
        /// <summary>
        /// 查图法 逸度系数适配器
        /// </summary>
        /// <param name="chemical">化合物</param>
        /// <param name="targetModel">普遍化逸度系数图 计算的对象</param>
        public DiagramFugacityyAdapter(CorStt.Models.Diagram.FugModel targetModel) {
            _model = targetModel;
        }
        /// <summary>
        /// 持有的 普遍化逸度系数图 模型
        /// </summary>
        private CorStt.Models.Diagram.FugModel _model;
        /// <summary>
        /// 计算气相逸度系数，返回 DiagramFugResult 对象，isAborted 为 true 时为被放弃
        /// </summary>
        /// <param name="actualTemperature">温度</param>
        /// <param name="actualPressure">压力</param>
        /// <returns>DiagramFugResult 对象</returns>
        public IGasFugResult GetGasFugResult(Chemical chemical, double T, double P) {
            double tr = T / chemical.CriticalTemperature;
            double pr = P / chemical.CriticalPressure;
            if (_model.IsBaseCrecAcquired(tr, pr)) {
                double fugBase = _model.InteractResult.InteractBase;
                double fugCrec = _model.InteractResult.InteractCrec;
                double fugacity = _model.GetFugacityCoe(fugBase, fugCrec, chemical.AcentricFactor);
                return new GasFugResult(tr, pr, fugBase, fugCrec, fugacity, false);
            } else {
                return new GasFugResult(tr, pr, -3.0, -3.0, -3.0, true);
            }
        }
        /// <summary>
        /// 查图法 进行逸度计算的结果
        /// </summary>
        public class GasFugResult : IGasFugResult {
            /// <summary>
            /// 查图法 进行逸度计算的结果
            /// </summary>
            /// <param name="tr">相对温度</param>
            /// <param name="pr">相对压力</param>
            /// <param name="fugBase">ψ0</param>
            /// <param name="fugCrec">ψ1</param>
            /// <param name="fugacity">ψ</param>
            public GasFugResult(double tr, double pr, double fugBase, double fugCrec, double fugacity, bool isAborted) {
                GasFugacity = fugacity;
                RelativeTemperature = tr;
                RelativePressure = pr;
                FugacityBase = fugBase;
                FugacityCrec = fugCrec;
                IsAborted = isAborted;
            }
            /// <summary>
            /// 逸度系数
            /// </summary>
            public double GasFugacity { get; private set; }
            /// <summary>
            /// ψ0
            /// </summary>
            public double FugacityBase { get; private set; }
            /// <summary>
            /// ψ1
            /// </summary>
            public double FugacityCrec { get; private set; }
            /// <summary>
            /// 相对压力
            /// </summary>
            public double RelativePressure { get; private set; }
            /// <summary>
            /// 相对温度
            /// </summary>
            public double RelativeTemperature { get; private set; }
            /// <summary>
            /// 计算是否被放弃
            /// </summary>
            public bool IsAborted { get; private set; }
        }
    }
}
/// <summary>
/// 汽液平衡计算 Eos 法
/// </summary>
namespace ChemEngThermCal.Model.Vle.Eos {
    /// <summary>
    /// 单一物系，以 EOS 法进行 VLE 计算中介者的基类型
    /// </summary>
    public class  PureMediator : MixRules.IPureFluidModel {
        public PureMediator(CalType caltype, double independentVar, CeosFugAdapter fugMediator, Chemical chemical) {
            _fugMediator = fugMediator;
            _chemical = chemical;
            MediatorType = caltype;
            KnownVar = independentVar;
        }
        /// <summary>
        /// 立方型状态方程逸度中介器
        /// </summary>
        protected CeosFugAdapter _fugMediator { get; private set; }
        #region MixRules.IPureFluidModel 接口的显式实现
        protected Chemical _chemical;
        /// <summary>
        /// 目标化合物
        /// </summary>
        Chemical MixRules.IPureFluidModel.Chemical
           => _chemical;
        #endregion
        /// <summary>
        /// 计算类型
        /// </summary>
        public CalType MediatorType { get; }
        /// <summary>
        /// 仅允许自身及嵌套类访问：已知的环境变量 T 或者 P
        /// </summary>
        protected double KnownVar { get; }
        /// <summary>
        /// 获取计算结果
        /// </summary> 
        public Result GetResult() {
            AlgorithmMediator algrithmMed = new AlgorithmMediator(this);
            double inilVar;
            if (MediatorType == CalType.Isothermal) {
                //KnownVar = T, unknownVar = P
                inilVar = _chemical.CriticalPressure * Math.Pow(10.0, ((7.0 * (1.0 + _chemical.AcentricFactor) / 3.0) * (1.0 - _chemical.CriticalTemperature / KnownVar)));
            } else if (MediatorType == CalType.Isobaric) {
                //KnownVar = P, unknownVar = T
                inilVar = _chemical.CriticalTemperature / (1.0 - (3.0 * Math.Log10(KnownVar / _chemical.CriticalPressure)) / (7.0 * (1.0 + _chemical.AcentricFactor)));
            } else {
                throw new NullReferenceException("获取的中介者类不是受支持的 2种（等压，等温）计算类型之一");
            }
            Algorithm.VlePureIterator calItr = new Algorithm.VlePureIterator(algrithmMed, inilVar, 0.0001, 1000);
            StepInfo lastStep = new StepInfo(calItr.rawStepInfo.ToArray()[0]);
            List<StepInfo> stepList = new List<StepInfo> { };
            foreach (Algorithm.VlePureIterator.StepInfo rawStep in calItr.rawStepInfo) {
                stepList.Add(new StepInfo(rawStep));
                lastStep = new StepInfo(rawStep);
            }
            double var = calItr.Result;
            double gasRsdEnthalpy, liqRsdEnthalpy, gasRsdEntropy, liqRsdEntropy, vaporizationEnthalpy, vaporizationEntropy, gasFug, liqFug, gasCpr, liqCpr, gasVol, liqVol;
            if (var == -1.0 || var == -2.0) {
                if (MediatorType == CalType.Isothermal) {
                    //KnownVar = T, unknownVar = P
                    return new Result(KnownVar, var, var, var, false, var, var, var, var, var, false, var, var, var, var, var, inilVar, stepList, _chemical, MediatorType);
                } else if (MediatorType == CalType.Isobaric) {
                    //KnownVar = P, unknownVar = T
                    return new Result(var, KnownVar, var, var, false, var, var, var, var, var, false, var, var, var, var, var, inilVar, stepList, _chemical, MediatorType);
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 2种（等压，等温）计算类型之一");
                }
            } else {
                gasRsdEnthalpy = _fugMediator.Model.GetResidualEnthalpy(_chemical, lastStep.FugResult.EosResult.GasResult.CprFactor, lastStep.FugResult.EosResult.GasResult.Temperature, lastStep.FugResult.EosResult.GasResult.Pressure);
                liqRsdEnthalpy = _fugMediator.Model.GetResidualEnthalpy(_chemical, lastStep.FugResult.EosResult.LiqResult.CprFactor, lastStep.FugResult.EosResult.GasResult.Temperature, lastStep.FugResult.EosResult.GasResult.Pressure);
                gasRsdEntropy = _fugMediator.Model.GetResidualEntropy(_chemical, lastStep.FugResult.EosResult.GasResult.CprFactor, lastStep.FugResult.EosResult.GasResult.Temperature, lastStep.FugResult.EosResult.GasResult.Pressure);
                liqRsdEntropy = _fugMediator.Model.GetResidualEntropy(_chemical, lastStep.FugResult.EosResult.LiqResult.CprFactor, lastStep.FugResult.EosResult.GasResult.Temperature, lastStep.FugResult.EosResult.GasResult.Pressure);

                vaporizationEnthalpy = (_fugMediator.Model as Ceos.Models.CeosBaseModel).GetVaporizationEnthalpy(gasRsdEnthalpy, liqRsdEnthalpy, lastStep.FugResult.EosResult.GasResult.Temperature);
                vaporizationEntropy = (_fugMediator.Model as Ceos.Models.CeosBaseModel).GetVaporizationEntropy(gasRsdEntropy, liqRsdEntropy);

                gasFug = lastStep.FugResult.GasFugacity;
                liqFug = lastStep.FugResult.LiqFugacity;
                gasCpr = lastStep.FugResult.EosResult.GasResult.CprFactor;
                gasVol = lastStep.FugResult.EosResult.GasResult.MoleVol;
                liqCpr = lastStep.FugResult.EosResult.LiqResult.CprFactor;
                liqVol = lastStep.FugResult.EosResult.LiqResult.MoleVol;

                if (liqCpr == -3.0) {
                    if (MediatorType == CalType.Isothermal) {
                        //KnownVar = T, unknownVar = P
                        return new Result(KnownVar, var, gasVol, gasCpr, true, gasFug, gasRsdEnthalpy, gasRsdEntropy, liqVol, liqCpr, false, liqFug, liqRsdEnthalpy, liqRsdEntropy, vaporizationEnthalpy, vaporizationEntropy, inilVar, stepList, _chemical, MediatorType);
                    } else if (MediatorType == CalType.Isobaric) {
                        //KnownVar = P, unknownVar = T
                        return new Result(var, KnownVar, gasVol, gasCpr, true, gasFug, gasRsdEnthalpy, gasRsdEntropy, liqVol, liqCpr, false, liqFug, liqRsdEnthalpy, liqRsdEntropy, vaporizationEnthalpy, vaporizationEntropy, inilVar, stepList, _chemical, MediatorType);
                    } else {
                        throw new NullReferenceException("获取的中介者类不是受支持的 2种（等压，等温）计算类型之一");
                    }
                } else {
                    if (MediatorType == CalType.Isothermal) {
                        //KnownVar = T, unknownVar = P
                        return new Result(KnownVar, var, gasVol, gasCpr, true, gasFug, gasRsdEnthalpy, gasRsdEntropy, liqVol, liqCpr, true, liqFug, liqRsdEnthalpy, liqRsdEntropy, vaporizationEnthalpy, vaporizationEntropy, inilVar, stepList, _chemical, MediatorType);
                    } else if (MediatorType == CalType.Isobaric) {
                        //KnownVar = P, unknownVar = T
                        return new Result(var, KnownVar, gasVol, gasCpr, true, gasFug, gasRsdEnthalpy, gasRsdEntropy, liqVol, liqCpr, true, liqFug, liqRsdEnthalpy, liqRsdEntropy, vaporizationEnthalpy, vaporizationEntropy, inilVar, stepList, _chemical, MediatorType);
                    } else {
                        throw new NullReferenceException("获取的中介者类不是受支持的 2种（等压，等温）计算类型之一");
                    }
                }
            }
        }
        /// <summary>
        /// 用于抽象算法交互 的内部类
        /// </summary>
        public sealed class AlgorithmMediator : Algorithm.IVlePureItrMediator {
            public AlgorithmMediator(PureMediator modelMed) {
                _modelMed = modelMed;
            }
            private PureMediator _modelMed;
            #region Algorithm.IVlePureItrMediator 的显式实现
            CeosFugAdapter.LiqFugResult Algorithm.IVlePureItrMediator.GetFugacity(double unknownVar) {
                if (_modelMed.MediatorType == CalType.Isothermal) {
                    //KnownVar = T, unknownVar = P
                    return _modelMed._fugMediator.GetLiqFugResult(_modelMed._chemical, _modelMed.KnownVar, unknownVar);
                } else if (_modelMed.MediatorType == CalType.Isobaric) {
                    //KnownVar = P, unknownVar = T
                    return _modelMed._fugMediator.GetLiqFugResult(_modelMed._chemical, unknownVar, _modelMed.KnownVar);
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
            }
            double Algorithm.IVlePureItrMediator.ItrFormula(double unknownVar, CeosFugAdapter.LiqFugResult fug) {
                if (_modelMed.MediatorType == CalType.Isothermal) {
                    //KnownVar = T, unknownVar = P
                    return unknownVar * (1.0 - Math.Log(fug.GasFugacity / fug.LiqFugacity) / (fug.EosResult.GasResult.CprFactor - fug.EosResult.LiqResult.CprFactor));
                } else if (_modelMed.MediatorType == CalType.Isobaric) {
                    //KnownVar = P, unknownVar = T
                    double gasResiEnthalpy = _modelMed._fugMediator.Model.GetResidualEnthalpy(_modelMed._chemical, fug.EosResult.GasResult.CprFactor, unknownVar, _modelMed.KnownVar);
                    double liqResiEnthalpy = _modelMed._fugMediator.Model.GetResidualEnthalpy(_modelMed._chemical, fug.EosResult.LiqResult.CprFactor, unknownVar, _modelMed.KnownVar);
                    return unknownVar * (1.0 + Math.Log(fug.GasFugacity / fug.LiqFugacity) / (gasResiEnthalpy - liqResiEnthalpy));
                } else {
                    throw new NullReferenceException("获取的中介者类不是受支持的 4种（等压泡点，等压露点，等温泡点，等温露点）计算类型之一");
                }
            }
            #endregion
        }
        /// <summary>
        /// 相平衡计算信息的基类型
        /// </summary>
        public class Result {
            /// <summary>
            /// 相平衡计算信息的基类型
            /// </summary>
            /// <param name="chemical">目标化合物</param>
            /// <param name="gasResidualEnthalpy">气相剩余焓</param>
            /// <param name="liqResidualEnthalpy">液相剩余焓</param>
            /// <param name="gasResidualEntropy">气相剩余熵</param>
            /// <param name="liqResidualEntropy">液相剩余熵</param>
            /// <param name="vaporizationEnthalpy">蒸发焓</param>
            /// <param name="vaporizationentropy">蒸发熵</param>
            /// <param name="gasFugacity">气相逸度</param>
            /// <param name="liqFugacity">液相逸度</param>
            /// <param name="gasCpr">气相压缩因子</param>
            /// <param name="liqCpr">液相压缩因子</param>
            /// <param name="gasVol">气相摩尔体积</param>
            /// <param name="liqVol">液相摩尔体积</param>
            /// <param name="isConverged">计算是否收敛</param>
            /// <param name="isAborted">计算是否被中途放弃</param>
            public Result(double temperature, double pressure,
                double gasMoleVol, double gasCpr, bool isGasConverged, double gasFugacity, double gasResidualEnthalpy, double gasResidualEntropy,
                double liqMoleVol, double liqCpr, bool isLiqConverged, double liqFugacity, double liqResidualEnthalpy, double liqResidualEntropy,
                double vaporizationEnthalpy, double vaporizationentropy,
                double inilVar, List<StepInfo> stepList, Chemical chemical, CalType calType) {
                LiqResult = new VleResultBase(temperature, pressure, liqMoleVol, liqCpr, false, isLiqConverged, liqFugacity, liqResidualEnthalpy, liqResidualEntropy);
                GasResult = new VleResultBase(temperature, pressure, gasMoleVol, gasCpr, false, isLiqConverged, gasFugacity, gasResidualEnthalpy, gasResidualEntropy);
                CalculateType = calType;
                Chemical = chemical;
                VaporizationEnthalpy = vaporizationEnthalpy;
                Vaporizationentropy = vaporizationentropy;
                InilValue = inilVar;
                ItrStepInfoList = stepList;
            }
            /// <summary>
            /// 液相结果
            /// </summary>
            public VleResultBase LiqResult { get; private set; }
            /// <summary>
            /// 气相结果
            /// </summary>
            public VleResultBase GasResult { get; private set; }
            /// <summary>
            /// 蒸发焓
            /// </summary>
            public double VaporizationEnthalpy { get; private set; }
            /// <summary>
            /// 蒸发熵
            /// </summary>
            public double Vaporizationentropy { get; private set; }
            /// <summary>
            /// 当前化合物
            /// </summary>
            public Material.Chemical Chemical { get; private set; }
            /// <summary>
            /// 第一次计算的估计压力
            /// </summary>
            public double InilValue { get; private set; }
            /// <summary>
            /// 迭代步骤信息
            /// </summary>
            public List<StepInfo> ItrStepInfoList { get; private set; }
            /// <summary>
            /// 计算类型
            /// </summary>
            public CalType CalculateType { get; private set; }
        }
        /// <summary>
        /// 计算类型的枚举
        /// </summary>
        public enum CalType {
            /// <summary>
            /// T known
            /// </summary>
            Isothermal = 0,
            /// <summary>
            /// P known
            /// </summary>
            Isobaric = 1
        }
        /// <summary>
        /// 迭代步骤记录器的实例
        /// </summary>
        public class StepInfo {
            public StepInfo(Algorithm.VlePureIterator.StepInfo rawStep) {
                DependentVar = rawStep.Value;
                NextDependentVar = rawStep.NextValue;
                DiffValue = rawStep.DiffValue;
                FugResult = rawStep.FugResult;
            }
            /// <summary>
            /// 两相逸度差值
            /// </summary>
            public double DiffValue { get; private set; }
            /// <summary>
            /// 包含所有逸度计算过程的结果
            /// </summary>
            public CeosFugAdapter.LiqFugResult FugResult { get; private set; }
            /// <summary>
            /// 迭代后压力
            /// </summary>
            public double NextDependentVar { get; private set; }
            /// <summary>
            /// 迭代前压力
            /// </summary>
            public double DependentVar { get; private set; }
        }
    }
}