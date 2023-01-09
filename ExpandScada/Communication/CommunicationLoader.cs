using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Common.Communication;
using System.Reflection;

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
        const string PROTOCOLS_FOLDER = @"..\..\Protocols\Debug\net6.0";

        public static void LoadAllProtocols()
        {
            // Try to find the Protocols folder
            if (!Directory.Exists(PROTOCOLS_FOLDER))
            {
                // TODO redo on exception or/and log
                //Console.WriteLine("Folder Protocols doesn't exists");
                return;
            }

            string[] files = System.IO.Directory.GetFiles(PROTOCOLS_FOLDER, "*.dll");

            for (int i = 0; i < files.Length; i++)
            {
                var protocol = LoadDll(files[i]);
                if (protocol == null)
                {
                    continue;
                }
                CommunicationManager.communicationProtocols.Add(protocol);
            }


        }

        private static CommunicationProtocol LoadDll(string filePath)
        {
            object result = null;
            try
            {
                var dllFile = new FileInfo(filePath);
                var DLL = Assembly.LoadFile(dllFile.FullName);

                var types = DLL.GetExportedTypes();

                result = Activator.CreateInstance(types[0]);
            }
            catch
            {
                // TODO add some log or throw new exception
                return null;
            }
            

            return result as CommunicationProtocol;

        }
    }
}
