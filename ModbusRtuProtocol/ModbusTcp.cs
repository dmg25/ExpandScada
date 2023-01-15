using Common.Communication;
using Common.Gateway;
using EasyModbus;


namespace ModbusProtocol
{
    public class ModbusTcp : CommunicationProtocol
    {
        public override string Name { get; set; } = "ModbusTcp";
        ModbusClient modbusClient;
        Thread pollingThread;
        int pollingInterval;
        bool stopPolling = false;
        List<ModbusSlave> slaves = new List<ModbusSlave>();
        

        /// <summary>
        /// First initialization, parse settings and check them. Check equipment and possibility for starting
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="protocolSettings"></param>
        public override void InitializeProtocol(List<(Signal signal, string settings)> signals, string protocolSettingsString)
        {
            var protocolSettings = ParseSettingStringToValues(GetProtocolSettings(), protocolSettingsString);
            modbusClient = new ModbusClient((string)protocolSettings["IP address"], Convert.ToInt32(protocolSettings["Port"]));

            pollingInterval = Convert.ToInt32(protocolSettings["Polling interval"]);

            pollingThread = new Thread(Polling);




        }

        /// <summary>
        /// Start loop(s) of communication protocol
        /// </summary>
        public override void StartCommunication()
        {
            if (pollingThread.ThreadState == ThreadState.Stopped)
            {
                pollingThread.Resume();
            }
            pollingThread.Start();
        }

        /// <summary>
        /// Stop loops of communication with possibility to start them again if necessary
        /// </summary>
        public override void StopCommunication()
        {
            pollingThread.Interrupt();
        }

        /// <summary>
        /// Release all resources/equipment, finish all threads, dispose objects if necessary
        /// </summary>
        public override void FinishAndDisposeCommunication()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Parameters for this protocol (speed, equipment names, addresses ... )
        /// </summary>
        /// <returns></returns>
        public override List<SettingsParameter> GetProtocolSettings()
        {
            return new List<SettingsParameter>() 
            {
                new SettingsParameter<string>(
                    "190.201.100.100", 
                    "IP address", 
                    "IP address of slave device",
                    delegate(string val)
                    {
                        if (CheckIpAddressStructure(val))
                        {
                            return "Wrong IP address format";
                        }
                        return String.Empty;
                    }),
                new SettingsParameter<uint>(
                    502,
                    "Port",
                    "TCP port of slave device"),
                new SettingsParameter<uint>(
                    50,
                    "Polling interval",
                    "Interval between each requests in ms"),
                //new SettingsParameter<uint>(
                //    500,
                //    "Timeout",
                //    "Timeout for responce waiting"),
                //new SettingsParameter<bool>(
                //    false,
                //    "Unite requests",
                //    "Allow to unite different requests to one big request, but with not registered registers between. Be sure, that device can return them"),

            };
        }

        /// <summary>
        /// Parameters for each signal which is involved in this protocol
        /// </summary>
        /// <returns></returns>
        public override List<SettingsParameter> GetSignalSettings()
        {
            // device address byte 1-123
            // register type array of strings for combobox
            // register address uint (0-9999)
            // byte order string array of variants
            return new List<SettingsParameter>()
            {
                new SettingsParameter<byte>(
                    1,
                    "Device address",
                    "Slave device address on the bus",
                    delegate(byte val)
                    {
                        if (val <= 0 || val > 123)
                        {
                            return "Device address can be 1-123";
                        }
                        return String.Empty;
                    }),
                new SettingsParameter<uint>(
                    0,
                    "Register Address",
                    "Address of register(or first register, if the value takes more than one). can be 0-9999",
                    delegate(uint val)
                    {
                        if (val < 0 || val > 9999)
                        {
                            return "Register address can be 0-9999";
                        }
                        return String.Empty;
                    }),
                //new SettingsParameter<string[]>(
                //    new string[] {"0123","2301"},
                //    "Byte order",
                //    "Byte order for reading/writing values which takes 2 and more registres"),
                new SettingsParameter<string[]>(
                    new string[] {"Input register","Holding register"},
                    "Type of register",
                    "Byte order for reading/writing values which takes 2 and more registres"),


            };
        }

        /// <summary>
        /// List of special signals for this protocol, like connected/disconnected/error/status...
        /// </summary>
        /// <returns></returns>
        public override List<SettingsParameter> GetSystemSignals()
        {
            throw new NotImplementedException();
        }







        bool CheckIpAddressStructure(string ip)
        {
            string [] ipNumbersSrt = ip.Split('.');
            if (ipNumbersSrt.Length != 4)
            {
                return false;
            }

            for (int i = 0; i < ipNumbersSrt.Length; i++)
            {
                try
                {
                    byte ipElement  = Convert.ToByte(ipNumbersSrt[i]);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        
        Dictionary<string, object> ParseSettingStringToValues(List<SettingsParameter> settings, string settingsString)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            string[] splittedString = settingsString.Trim().Split(';');
            if (splittedString.Length != settings.Count)
            {
                throw new Exception($"Wrong number of settings in string {settingsString}");
            }

            for (int i = 0; i < splittedString.Length; i++)
            {
                result.Add(settings[i].Name, settings[i].GetTypedValueByString(splittedString[i]));
            }

            return result;
        }

        void Polling()
        {
            while (!stopPolling)
            {
                foreach (var slave in slaves)
                {
                    foreach (var group in slave.inputRegisters)
                    {
                        group.UpdateSignalsAfterRequest(modbusClient.ReadInputRegisters(group.startAddress, group.registerNum));
                        Thread.Sleep(pollingInterval);
                    }

                    foreach (var group in slave.holdingRegisters)
                    {
                        group.UpdateSignalsAfterRequest(modbusClient.ReadHoldingRegisters(group.startAddress, group.registerNum));
                        Thread.Sleep(pollingInterval);
                    }
                }
            }
        }

    }
}