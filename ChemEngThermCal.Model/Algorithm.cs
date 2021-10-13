using System;
using System.Collections.Generic;

namespace ChemEngThermCal.Model.Algorithm {
    /// <summary>
    /// 迭代器的父类
    /// </summary>
    public abstract class IteratorBase {
        public IteratorBase(double startValue, double itrPrecision, int maxStep) {
            StartValue = startValue;
            ItrPrecision = Math.Abs(itrPrecision);
            MaxStep = Math.Abs(maxStep);
        }
        /// <summary>
        /// 获取迭代初值
        /// </summary>
        public double StartValue { get; private set; }
        /// <summary>
        /// 获取迭代精度
        /// </summary>
        public double ItrPrecision { get; private set; }
        /// <summary>
        /// 获取最大迭代步数
        /// </summary>
        public int MaxStep { get; private set; }
        /// <summary>
        /// 获取迭代结果
        /// </summary>
        public double Result { get; protected set; }
    }
    /// <summary>
    /// 要进行 牛顿-拉弗森 迭代必须实现的接口
    /// </summary>
    public interface INewtonRaphsonItrMediator {
        /// <summary>
        /// F(x) =
        /// </summary>
        /// <param name="x">x</param>
        /// <returns>F(x)</returns>
        double DoFuncCal(double x);
        /// <summary>
        /// F'(x) =
        /// </summary>
        /// <param name="x">x</param>
        /// <returns>F'(x)</returns>
        double DoDerivFuncCal(double x);
    }
    /// <summary>
    /// 迭代器的实例：采用 牛顿-拉弗森法
    /// </summary>
    public class NewtonRaphsonIterator : IteratorBase {
        /// <summary>
        /// 实例化一个基于 牛顿-拉弗森算法 的迭代器
        /// </summary>
        /// <param name="adapter">调用并为迭代算法提供支持的对象：必须实现 INewtonRaphson 接口的对象</param>
        /// <param name="startValue">迭代起点</param>
        /// <param name="itrPrecision">迭代精度</param>
        /// <param name="maxStep">最大迭代步数，超出则判定发散</param>
        public NewtonRaphsonIterator(INewtonRaphsonItrMediator adapter, double startValue, double itrPrecision, int maxStep)
            : base(startValue, itrPrecision, maxStep) {
            _adapter = adapter;
            DoCalculate();
        }
        //迭代算法适配器
        private readonly INewtonRaphsonItrMediator _adapter;
        /// <summary>
        /// 进行计算
        /// </summary>
        public void DoCalculate() {
            List<StepInfo> stepInfo = new List<StepInfo> { };
            double diffFlag, func, derivFunc, value, nextValue;
            int timeFlag = 0;
            value = StartValue;
            do {
                func = _adapter.DoFuncCal(value);
                derivFunc = _adapter.DoDerivFuncCal(value);
                nextValue = value - func / derivFunc;
                diffFlag = Math.Abs(value - nextValue);
                stepInfo.Add(new StepInfo(value, func, derivFunc, nextValue, diffFlag));
                if (CheckIsDiverge(nextValue, timeFlag, stepInfo)) {
                    return;
                }
                timeFlag++;
                value = nextValue;
            } while (diffFlag > ItrPrecision);
            Result = value;
            StepInfoList = stepInfo;
        }
        /// <summary>
        /// 检测当前算法是否发散，如果发散(true)，则将退出迭代并赋值至 _result 和 _stepInfoList
        /// 若值为 -1，则表示计算过程中迭代发散
        /// 若值为 -2，则表示计算过程中迭代次数过多（ 大于100次 ），判定迭代发散
        /// </summary>
        /// <param name="currentValue">当前值</param>
        /// <param name="stepCount">已进行的迭代次数</param>
        /// <param name="stepInfo">当前迭代数据</param>
        /// <returns>true：迭代发散；false：迭代收敛</returns>
        protected bool CheckIsDiverge(double currentValue, int stepCount, List<StepInfo> stepInfo) {
            if (double.IsInfinity(currentValue)) {
                Result = -1.0;
                StepInfoList = stepInfo;
                return true;
            }
            if (stepCount > MaxStep) {
                Result = -2.0;
                StepInfoList = stepInfo;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 迭代过程的记录
        /// </summary>
        public List<StepInfo> StepInfoList { get; protected set; }
        /// <summary>
        /// 迭代步骤记录单元的实例
        /// </summary>
        public class StepInfo {
            public StepInfo(double value, double funcValue, double derivFuncValue, double nextValue, double diffFlag) {
                Value = value;
                FuncValue = funcValue;
                DerivFuncValue = derivFuncValue;
                NextValue = nextValue;
                DiffValue = diffFlag;
            }
            public double DerivFuncValue { get; private set; }
            public double DiffValue { get; private set; }
            public double FuncValue { get; private set; }
            public double NextValue { get; private set; }
            public double Value { get; private set; }
        }
    }
    /// <summary>
    /// 要进行 对应态原理 迭代必须实现的接口
    /// </summary>
    public interface ICorSttItrMediator {
        double GetRelativePressure(double cprFactor);
        double GetTargetFactor(double cprBase, double cprCrec);
        CorSttIterator.Msg GetNextValue(double relativePressure);
    }
    /// <summary>
    /// 迭代器实例：对应态原理方法
    /// </summary>
    public class CorSttIterator : IteratorBase {
        public CorSttIterator(ICorSttItrMediator adapter, double startValue, double itrPrecision, int maxStep)
            : base(startValue, itrPrecision, maxStep) {
            _med = adapter;
            DoCalculate();
        }
        //迭代算法适配器
        private readonly ICorSttItrMediator _med;
        /// <summary>
        /// 进行计算
        /// </summary>
        public void DoCalculate() {
            double cprFactor = 1.0;         //Z(n)
            double stepRelativePressure,    //Pr
                   nextCprFactor,           //Z(n+1)
                   cprFactorBase,           //Z0
                   cprFactorCrec;           //Z1
            double diffFlag;          //diffFlag
            int timeFlag = 1;
            List<StepInfo> stepInfo = new List<StepInfo> { };
            do {
                //Pr
                stepRelativePressure = _med.GetRelativePressure(cprFactor);
                Msg nextValue = _med.GetNextValue(stepRelativePressure);
                if (nextValue.IsAborted) {
                    Result = -3.0;
                    StepInfoList = stepInfo;
                    return;
                }
                //Z0
                cprFactorBase = nextValue.ValueBase;
                //Z1
                cprFactorCrec = nextValue.ValueCrec;
                //Z(n+1)
                nextCprFactor = _med.GetTargetFactor(cprFactorBase, cprFactorCrec);
                diffFlag = Math.Abs(nextCprFactor - cprFactor);
                /* 记录计算信息 */
                stepInfo.Add(new StepInfo(cprFactor, stepRelativePressure, cprFactorBase, cprFactorCrec, nextCprFactor, diffFlag));
                /* 记录计算信息 end */
                /* 检查是否发散 */
                if (CheckIsDiverge(nextCprFactor, timeFlag, stepInfo)) { return; }
                /* 检查是否发散 end */
                cprFactor = nextCprFactor;
                timeFlag += 1;
            } while (diffFlag > ItrPrecision);
            Result = cprFactor;
            StepInfoList = stepInfo;
        }
        /// <summary>
        /// 检测当前算法是否发散，如果发散(true)，则将退出迭代并赋值至 _result 和 _stepInfoList
        /// 若值为 -1，则表示计算过程中迭代发散
        /// 若值为 -2，则表示计算过程中迭代次数过多（ 大于100次 ），判定迭代发散
        /// </summary>
        /// <param name="currentValue">当前值</param>
        /// <param name="stepCount">已进行的迭代次数</param>
        /// <param name="stepInfo">当前迭代数据</param>
        /// <returns>true：迭代发散；false：迭代收敛</returns>
        private bool CheckIsDiverge(double currentValue, int stepCount, List<StepInfo> stepInfo) {
            if (double.IsInfinity(currentValue)) {
                Result = -1.0;
                StepInfoList = stepInfo;
                return true;
            }
            if (stepCount > MaxStep) {
                Result = -2.0;
                StepInfoList = stepInfo;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 迭代过程的记录
        /// </summary>
        public List<StepInfo> StepInfoList { get; protected set; }
        /// <summary>
        /// 迭代步骤记录单元的实例
        /// </summary>
        public class StepInfo {
            public StepInfo(double cprFactor, double relativePressure, double cprBase, double cprCrec, double nextCprFactor, double diffFlag) {
                CprFactor = cprFactor;
                RelativePressure = relativePressure;
                CprBase = cprBase;
                CprCrec = cprCrec;
                NextCprFactor = nextCprFactor;
                DiffValue = diffFlag;
            }
            public double RelativePressure { get; private set; }
            public double CprBase { get; private set; }
            public double CprCrec { get; private set; }
            public double CprFactor { get; private set; }
            public double NextCprFactor { get; private set; }
            public double DiffValue { get; private set; }
        }
        /// <summary>
        /// 信息传递器
        /// </summary>
        public class Msg {
            public Msg(bool isAborted, double valueBase, double valueCrec) {
                IsAborted = isAborted;
                ValueBase = valueBase;
                ValueCrec = valueCrec;
            }
            public bool IsAborted { get; private set; }
            public double ValueBase { get; private set; }
            public double ValueCrec { get; private set; }
        }
    }
    /// <summary>
    /// 要进行 纯物质相平衡 迭代必须实现的接口
    /// </summary>
    public interface IVlePureItrMediator {
        Vle.FugCoe.CeosFugAdapter.LiqFugResult GetFugacity(double unknownVar);
        double ItrFormula(double unknownVar, Vle.FugCoe.CeosFugAdapter.LiqFugResult fug);
    }
    /// <summary>
    /// 迭代器实例：纯物质相平衡计算
    /// </summary>
    public class VlePureIterator : IteratorBase {
        /// <summary>
        /// 实例化新的纯物质相平衡迭代器
        /// </summary>
        /// <param name="mediator">要进行相平衡迭代的对象</param>
        /// <param name="startValue">独立变量，T 或者 P</param>
        /// <param name="itrPrecision">迭代精度</param>
        /// <param name="maxStep">最大迭代步数</param>
        public VlePureIterator(IVlePureItrMediator mediator, double startValue, double itrPrecision, int maxStep)
            : base(startValue, itrPrecision, maxStep) {
            _mediator = mediator;
            DoCalculate();
        }
        //迭代算法适配器
        private readonly IVlePureItrMediator _mediator;
        /// <summary>
        /// 进行计算
        /// </summary>
        private void DoCalculate() {
            double value = StartValue;
            double nextValue;
            Vle.FugCoe.CeosFugAdapter.LiqFugResult fug;
            double diffFlag;
            int timeFlag = 1;
            List<StepInfo> rawStep = new List<StepInfo> { };
            do {
                fug = _mediator.GetFugacity(value);
                diffFlag = Math.Abs(fug.GasFugacity - fug.LiqFugacity);
                nextValue = _mediator.ItrFormula(value, fug);
                rawStep.Add(new StepInfo(value, nextValue, fug, diffFlag));
                if (CheckIsDiverge(fug, value, timeFlag, rawStep)) { return; }
                timeFlag += 1;
                value = nextValue;
            } while (diffFlag > ItrPrecision);
            Result = value;
            rawStepInfo = rawStep;
        }
        /// <summary>
        /// 检测当前算法是否发散，如果发散(true)，则将退出迭代并赋值至 _result 和 _stepInfoList
        /// 若值为 -1，则表示计算过程中迭代发散
        /// 若值为 -2，则表示计算过程中迭代次数过多（ 大于100次 ），判定迭代发散
        /// </summary>
        /// <param name="currentValue">当前值</param>
        /// <param name="stepCount">已进行的迭代次数</param>
        /// <param name="stepInfo">当前迭代数据</param>
        /// <returns>true：迭代发散；false：迭代收敛</returns>
        private bool CheckIsDiverge(Vle.FugCoe.CeosFugAdapter.LiqFugResult fug, double dependent, int stepCount, List<StepInfo> stepInfo) {
            if (double.IsInfinity(dependent)) {
                Result = -1.0;
                rawStepInfo = stepInfo;
                return true;
            }
            if (stepCount > MaxStep) {
                Result = -2.0;
                rawStepInfo = stepInfo;
                return true;
            }
            if (fug.GasFugacity == -3.0 || fug.LiqFugacity == -3.0) {
                Result = -3.0;
                rawStepInfo = stepInfo;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 迭代过程的记录
        /// </summary>
        public List<StepInfo> rawStepInfo { get; protected set; }
        /// <summary>
        /// 迭代步骤记录单元的实例
        /// </summary>
        public class StepInfo {
            public StepInfo(double value, double nextValue, Vle.FugCoe.CeosFugAdapter.LiqFugResult fugResult, double diff) {
                DiffValue = diff;
                FugResult = fugResult;
                Value = value;
                NextValue = nextValue;
            }
            public double DiffValue { get; private set; }
            public Vle.FugCoe.CeosFugAdapter.LiqFugResult FugResult { get; private set; }
            public double NextValue { get; private set; }
            public double Value { get; private set; }
        }
    }

    public interface IVleBinItrMediator {
        double GetFstN1(double unknownVar);
        double GetFstN2(double unknownVar);
        double GetN1(double unknownVar, double n1, double n2);
        double GetN2(double unknownVar, double n1, double n2);
        double ItrFomula(double unknownVar, double n1, double n2);
    }

    public class VleBinIterator : IteratorBase {
        public VleBinIterator(IVleBinItrMediator mediator, double startValue, double itrPrecision, int maxStep)
            : base(startValue, itrPrecision, maxStep) {
            _mediator = mediator;
            DoCalculate();
        }
        //算法适配器
        private readonly IVleBinItrMediator _mediator;
        /// <summary>
        /// 进行计算
        /// </summary>
        private void DoCalculate() {
            //Tn 或者 Pn
            double value = StartValue;
            //Tn+1 或者 Pn+1
            double nextValue;
            //xi 或者 yi
            double n1 = _mediator.GetFstN1(value);
            double n2 = _mediator.GetFstN2(value);
            n1 /= (n1 + n2);
            n2 = 1.0 - n1;
            double diffFlag;
            int timeFlag = 1;
            do {
                n1 = _mediator.GetN1(value, n1, n2);
                n2 = _mediator.GetN2(value, n1, n2);
                nextValue = _mediator.ItrFomula(value, n1, n2);
                diffFlag = Math.Abs(1 - n1 - n2);
                timeFlag += 1;
                if (CheckIsDiverge(n1, n2, value, timeFlag)) { return; }
                value = nextValue;
                n1 /= (n1 + n2);
                n2 = 1.0 - n1;
            } while (diffFlag > ItrPrecision);
            Result = value;
        }
        /// <summary>
        /// 检测当前算法是否发散，如果发散(true)，则将退出迭代并赋值至 _result
        /// 若值为 -1，则表示计算过程中迭代发散
        /// 若值为 -2，则表示计算过程中迭代次数过多（ 大于100次 ），判定迭代发散
        /// 若值为 -3，则表示计算过程中被放弃，判定迭代发散
        /// </summary>
        /// <param name="currentValue">当前值</param>
        /// <param name="stepCount">已进行的迭代次数</param>
        /// <param name="stepInfo">当前迭代数据</param>
        /// <returns>true：迭代发散；false：迭代收敛</returns>
        private bool CheckIsDiverge(double n1, double n2, double dependent, int stepCount) {
            if (double.IsInfinity(dependent) || double.IsInfinity(n1) || double.IsInfinity(n2)) {
                Result = -1.0;
                return true;
            }
            if (stepCount > MaxStep) {
                Result = -2.0;
                return true;
            }
            if (n1 == -1.0 || n2 == -1.0) {
                Result = -3.0;
                return true;
            }
            return false;
        }
    }
}