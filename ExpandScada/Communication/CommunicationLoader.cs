using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Common.Communication;
using Common.Gateway;
using System.Reflection;
using System.Data.SQLite;
using ExpandScada.SignalsGateway;

namespace ExpandScada.Communication
{
    /*      How it works:
     *  - we are searching for dlls in the some folder. 
     *  - each found dll trying convert to abstract protocol class 
     *  - if there is some dll, but converts with error - write to log or smth
     *  - make some name for this protocol (like ID converted?)
     *  - add this protocol to the list of the protocols
     * */

    public class CommunicationLoader
    {
        //const string PROTOCOLS_FOLDER = @"..\..\Protocols\Debug\net6.0";

        public static void LoadAllProtocols(string protocolsFolderPath, string dbPath)
        {
            // Try to find the Protocols folder
            if (!Directory.Exists(protocolsFolderPath))
            {
                // TODO redo on exception or/and log
                //Console.WriteLine("Folder Protocols doesn't exists");
                return;
            }

            string[] files = System.IO.Directory.GetFiles(protocolsFolderPath, "*.dll");

            for (int i = 0; i < files.Length; i++)
            {
                // TODO this instance exists only for checking, maybe we can simplify it
                var protocol = LoadDll(files[i]);
                if (protocol == null)
                {
                    continue;
                }
                CheckAndRegisterProtocol(protocol, dbPath, files[i]);
            }


        }

        private static CommunicationProtocol LoadDll(string filePath)
        {
            try
            {
                var dllFile = new FileInfo(filePath);
                var DLL = Assembly.LoadFile(dllFile.FullName);
                //var DLL = Assembly.LoadFrom(dllFile.FullName);

                var types = DLL.GetExportedTypes();

                foreach (var type in types)
                {
                    // TODO why it doesn't work???
                    //if (type is CommunicationProtocol)

                    //if (type.BaseType == "CommunicationProtocol")
                    if (type.BaseType == typeof(CommunicationProtocol))
                    {
                        return Activator.CreateInstance(type) as CommunicationProtocol;
                    }
                }
                return null;
            }
            catch
            {
                // TODO add some log or throw new exception
                return null;
            }
            
        }

        public static void CheckAndRegisterProtocol(CommunicationProtocol protocol, string dbPath, string protocolDllPath)
        {
            // TODO redo DB query, do not create connection every time!
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            //sqlite_conn = new SQLiteConnection("Data Source=..\\..\\Project\\test1.db; Version=3;");
            sqlite_conn = new SQLiteConnection($"Data Source={dbPath}; Version=3;");

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


            // check if this protocol is in the protocol table. Save ID of it
            sqlite_cmd.CommandText = $"SELECT ID FROM tblCommunicationProtocols where Name = \"{protocol.Name}\"";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            int protocolId = -1;
            if (sqlite_datareader.Read())
            {
                protocolId = sqlite_datareader.GetInt32(0);
            }
            else
            {
                return;
            }

            // check if this protocol is in the channels list (by ID) all ID with this name save
            sqlite_cmd.Reset();
            sqlite_cmd.CommandText = $"SELECT ID, Name, SettingsString FROM tblCommunicationChannels where Protocol = {protocolId}";
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            List<(int id, string channelName, string settingsString)> registeredProtocols = new List<(int id, string channelName, string settingsString)>();

            while (sqlite_datareader.Read())
            {
                int id = sqlite_datareader.GetInt32(0);
                string channelName = sqlite_datareader.GetString(1);
                string settingsString = sqlite_datareader.GetString(2);

                registeredProtocols.Add((id, channelName, settingsString));
            }

            if (registeredProtocols.Count == 0)
            {
                return;
            }

            // check if each channel ID is used by at least one signal in the list
            
            

            foreach (var tmpProtocol in registeredProtocols)
            {
                // get all signals ID with parameters string
                sqlite_cmd.Reset();
                sqlite_cmd.CommandText = $"SELECT ID, CommunicationSettings FROM tblSignals where CommunicationChannel = {tmpProtocol.id}";
                sqlite_datareader = sqlite_cmd.ExecuteReader();

                List<(Signal signal, string settings)> signals = new List<(Signal signal, string settings)>();

                // get all signals from global dictionary and initialize protocol with them
                while (sqlite_datareader.Read())
                {
                    int id = sqlite_datareader.GetInt32(0);
                    string settingsString = sqlite_datareader.GetString(1);

                    Signal signal = SignalStorage.allSignals[id];
                    signals.Add((signal, settingsString));
                }

                if (signals.Count == 0)
                {
                    continue;
                }

                var newProtocol = LoadDll(protocolDllPath);
                newProtocol.ChannelName = tmpProtocol.channelName;
                newProtocol.InitializeProtocol(signals, tmpProtocol.settingsString);
                CommunicationManager.communicationProtocols.Add(tmpProtocol.id, newProtocol);
            }

            sqlite_conn.Close();
        }


    }
}
