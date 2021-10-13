using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;


namespace ChemEngThermCal {
    static class CetcDALDisLayer {
        private static string cnString = "Provider = Microsoft.Jet.OLEDB.4.0;Data Source = " + System.IO.Directory.GetCurrentDirectory() + @"\cetcdb.mdb";
        private static OleDbDataAdapter dAdapt = null;
        static CetcDALDisLayer() {
            ConfigureAdapter(out dAdapt);
        }

        private static void ConfigureAdapter(out OleDbDataAdapter dAdapt) {
            dAdapt = new OleDbDataAdapter("Select * From Chemical", cnString);
            OleDbCommandBuilder build = new OleDbCommandBuilder(dAdapt);
            dAdapt.Fill(ds, "Chemical");
        }

        private static DataSet ds = new DataSet();

        public static DataSet GetAllChemical() {
            return ds;
        }

        public static void UpdataChemical(DataSet modifiedTable) {
            dAdapt.Update(modifiedTable);
        }


    }
}
