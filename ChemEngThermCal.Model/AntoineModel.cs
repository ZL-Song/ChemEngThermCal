using System;

namespace ChemEngThermCal.Model.Antoine {
    /// <summary>
    /// Antoine 方程的类型
    /// </summary>
    public enum Expression {
        MegaPascalKevin = 1,
        KiloPascalCelsius = 2,
        PascalKevin = 4
    }
    /// <summary>
    /// Antoine 参数组的结构
    /// </summary>
    public struct AntParaGroup {
        /// <summary>
        /// 初始化 Antoine 方程系数组
        /// </summary>
        /// <param name="indexA">a</param>
        /// <param name="indexB">b</param>
        /// <param name="indexC">c</param>
        public AntParaGroup(double indexA, double indexB, double indexC) {
            _indexA = indexA;
            _indexB = indexB;
            _indexC = indexC;
        }
        //Antoine Coefficients
        private double _indexA, _indexB, _indexC;
        /// <summary>
        /// A
        /// </summary>
        public double IndexA => _indexA;
        /// <summary>
        /// B
        /// </summary>
        public double IndexB => _indexB;
        /// <summary>
        /// C
        /// </summary>
        public double IndexC => _indexC;
    }
    /// <summary>
    /// 封装 Antoine 模型的适配器
    /// </summary>
    public class AntoineMediator {
        /// <summary>
        /// 实例化一个 Antoine 模型策略上下文类型
        /// </summary>
        /// <param name="model">要访问的 Antoine 模型</param>
        public AntoineMediator(AntoineModelBase model) {
            targetModel = model;
        }
        //持有的 Antoine 模型
        private AntoineModelBase targetModel;
        /// <summary>
        /// 指定温度下计算饱和压力，压力单位随模型类型不同而不同
        /// </summary>
        /// <param name="temperature">温度</param>
        /// <returns>饱和压力</returns>
        public double GetSaturatedPressure(Material.Chemical chemical, double temperature)
            => targetModel.GetSaturaPressure(chemical, temperature);
        /// <summary>
        /// 指定压力下计算沸点，沸点单位随模型类型不同而不同
        /// </summary>
        /// <param name="pressure">压力</param>
        /// <returns>沸点</returns>
        public double GetBoilingTemperature(Material.Chemical chemical, double pressure)
            => targetModel.GetBoilingTemperature(chemical, pressure);
        /// <summary>
        /// 获取模型类型
        /// </summary>
        public Expression modelType
            => targetModel.Exp;
    }
    /// <summary>
    /// Antoine 方程的基类型
    /// </summary>
    public abstract class AntoineModelBase {
        /// <summary>
        /// 获取当前的 Antoine 模型类型
        /// </summary>
        public abstract Expression Exp { get; }
        /// <summary>
        /// 根据温度获取当前 Antoine 方程计算的物质 饱和压力
        /// </summary>
        /// <param name="temperature">环境温度</param>
        /// <returns>饱和压力</returns>
        public abstract double GetSaturaPressure(Material.Chemical chemical, double temperature);
        /// <summary>
        /// 根据压力获取当前 Antoine 方程计算的物质 沸点
        /// </summary>
        /// <param name="pressure">环境压力</param>
        /// <returns>沸点</returns>
        public abstract double GetBoilingTemperature(Material.Chemical chemical, double pressure);
    }
    /// <summary>
    /// MPa K 为单位的 Antoine 方程模型的基类型
    /// </summary>
    public class MegaPascalKevinAntoineModel : AntoineModelBase {
        /// <summary>
        /// 获取当前的 Antoine 模型类型
        /// </summary>
        public override Expression Exp
            => Expression.MegaPascalKevin;
        /// <summary>
        /// 根据温度获取当前 Antoine 方程计算的物质 饱和压力
        /// </summary>
        /// <param name="temperature">环境温度，K</param>
        /// <returns>饱和压力，MPa</returns>
        public override double GetSaturaPressure(Material.Chemical chemical, double temperature)
            => Math.Exp(chemical.GetAntoinePara().IndexA - (chemical.GetAntoinePara().IndexB / (chemical.GetAntoinePara().IndexC + temperature)));
        /// <summary>
        /// 根据压力获取当前 Antoine 方程计算的物质 沸点
        /// </summary>                                   
        /// <param name="pressure">环境压力</param>
        /// <returns>沸点</returns>
        public override double GetBoilingTemperature(Material.Chemical chemical, double pressure)
            => chemical.GetAntoinePara().IndexB / (chemical.GetAntoinePara().IndexA - Math.Log(pressure)) - chemical.GetAntoinePara().IndexC;
    }
    /// <summary>
    /// kPa 摄氏度 为单位的 Antoine 方程模型
    /// </summary>
    public class KiloPascalCelsiusAntoineModel : AntoineModelBase {
        /// <summary>
        /// 获取当前的 Antoine 模型类型
        /// </summary>
        public override Expression Exp
            => Expression.KiloPascalCelsius;
        /// <summary>
        /// 根据温度获取当前 Antoine 方程计算的物质 饱和压力
        /// </summary>
        /// <param name="temperature">环境温度，摄氏度</param>
        /// <returns>饱和压力，kPa</returns>
        public override double GetSaturaPressure(Material.Chemical chemical, double temperature)
            => Math.Pow(10, (chemical.GetAntoinePara().IndexA - (chemical.GetAntoinePara().IndexB / (chemical.GetAntoinePara().IndexC + temperature))));
        /// <summary>
        /// 根据压力获取当前 Antoine 方程计算的物质 沸点
        /// </summary>
        /// <param name="pressure">环境压力，kPas</param>
        /// <returns>沸点，摄氏度</returns>
        public override double GetBoilingTemperature(Material.Chemical chemical, double pressure)
            => Math.Pow(10, (chemical.GetAntoinePara().IndexA - (chemical.GetAntoinePara().IndexB / (chemical.GetAntoinePara().IndexC + pressure))));
    }
    /// <summary>
    /// Pa , K 为单位的 Antoine 方程模型
    /// </summary>
    public class PascalKevinAntoineModel : AntoineModelBase {
        /// <summary>
        /// 获取当前的 Antoine 模型类型
        /// </summary>
        public override Expression Exp
            => Expression.PascalKevin;
        /// <summary>
        /// 根据温度获取当前 Antoine 方程计算的物质 饱和压力
        /// </summary>
        /// <param name="temperature">环境温度，K</param>
        /// <returns>饱和压力，MPa</returns>
        public override double GetSaturaPressure(Material.Chemical chemical, double temperature)
            => Math.Pow(Math.E, (chemical.GetAntoinePara().IndexA - (chemical.GetAntoinePara().IndexB / (chemical.GetAntoinePara().IndexC + temperature))));
        /// <summary>
        /// 根据压力获取当前 Antoine 方程计算的物质 沸点
        /// </summary>
        /// <param name="pressure">环境压力</param>
        /// <returns>沸点</returns>
        public override double GetBoilingTemperature(Material.Chemical chemical, double pressure)
            => chemical.GetAntoinePara().IndexB / (chemical.GetAntoinePara().IndexA - Math.Log(pressure)) - chemical.GetAntoinePara().IndexC;
    }
}