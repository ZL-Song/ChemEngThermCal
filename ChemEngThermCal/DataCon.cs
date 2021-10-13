using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;

namespace ChemEngThermCal {
    /// <summary>
    /// DBConnection 的摘要说明
    /// </summary>
    public class DBConnection {
        OleDbConnection con = null;


        public DBConnection() {
            string conString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\cetcdbtest.mdb";
            using (OleDbConnection cn = new OleDbConnection()) {
                cn.ConnectionString = conString;
                cn.Open();
                string strOledb = "Select * From Chemical";
                OleDbCommand myCommand = new OleDbCommand(strOledb, cn);
            }
        }

        public OleDbConnection Con
            => con;
    }

    public class ChemicalOper {
        public ChemicalOper() {

        }

    }
}
namespace ChemEngThermCal.View {
    /// <summary>
    /// 从数据库获取的 化合物实例
    /// </summary>
    public class ChemicalInfo {
        public ChemicalInfo(double tc, double pc, double w, double zc, double vc) {
            Tc = tc;
            Pc = pc;
            W = w;
            Zc = zc;
            Vc = vc;
        }
        public double Pc { get; private set; }
        public double Tc { get; private set; }
        public double Vc { get; private set; }
        public double W { get; private set; }
        public double Zc { get; private set; }
    }
}
