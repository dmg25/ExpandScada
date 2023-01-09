using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Common.Gateway;
using Common;

namespace ExpandScada.SignalsGateway
{
    public class SignalLoader
    {
        public static void LoadAllSignals(string filePath)
        {
            // connection

            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            //sqlite_conn = new SQLiteConnection("Data Source=..\\..\\Project\\test1.db; Version=3;");
            sqlite_conn = new SQLiteConnection($"Data Source={filePath}; Version=3;");

            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                //TODO log and exception
                throw;
            }

            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM tblSignals";

            sqlite_datareader = sqlite_cmd.ExecuteReader();

            while (sqlite_datareader.Read())
            {
                int id = sqlite_datareader.GetInt32(0);
                string name = sqlite_datareader.GetString(1);
                var dataType = (SignalDataType)sqlite_datareader.GetByte(2);

                Type[] type = { GetTypeByField(dataType) };
                Type tmpObj = typeof(Signal<>);
                Type makeme = tmpObj.MakeGenericType(type);

                Signal tmpSignal = (Signal)Activator.CreateInstance(makeme);
                tmpSignal.id = id;
                tmpSignal.name = name;
                SignalStorage.allSignals.TryAdd(id, tmpSignal);
            }




            sqlite_conn.Close();
        }



        private static Type GetTypeByField(SignalDataType type)
        {
            switch (type)
            {
                case SignalDataType.Integer:
                    return typeof(int);
                case SignalDataType.Float:
                    return typeof(float);
                default:
                    throw new Exception("Unknown type of data");
            }
        }

    }
}
