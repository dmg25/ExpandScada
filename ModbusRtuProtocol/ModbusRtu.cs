using Common.Communication;
using Common.Gateway;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusProtocol
{
    public class ModbusRtu : CommunicationProtocol
    {
        /*      Write one signal
         * 
         * 
         * 
         * 
         * 
         * */


        public override string Name { get; set; } = "ModbusRtu";
        //ModbusClient modbusClient;
        SerialPort comPort;
        Thread pollingThread;
        int pollingInterval;
        bool stopPolling = false;
        Dictionary<int, ModbusSlave> slaves = new Dictionary<int, ModbusSlave>();


        /// <summary>
        /// First initialization, parse settings and check them. Check equipment and possibility for starting
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="protocolSettings"></param>
        public override void InitializeProtocol(List<(Signal signal, string settings)> signals, string protocolSettingsString)
        {
            // Set up the channel
            var protocolSettings = ParseSettingStringToValues(GetProtocolSettings(), protocolSettingsString);

            comPort = new SerialPort();
            comPort.PortName = (string)protocolSettings["COM port name"];
            comPort.BaudRate = (int)protocolSettings["BaudRate"];
            comPort.DataBits = (int)protocolSettings["DataBits"];
            Enum.TryParse((string)protocolSettings["Parity"], out Parity parity);
            comPort.Parity = parity;
            Enum.TryParse((string)protocolSettings["StopBits"], out StopBits stopBits);
            comPort.StopBits = stopBits;

            // it's really necessary here?
            comPort.ReadTimeout = 2000;
            comPort.WriteTimeout = 2000;

            //modbusClient = new ModbusClient((string)protocolSettings["IP address"], Convert.ToInt32(protocolSettings["Port"]));

            pollingInterval = Convert.ToInt32(protocolSettings["Polling interval"]);
            pollingThread = new Thread(Polling);

            //Set up slaves/queries
            FindAllSlavesAndSetupThem(signals);

            Logger.Info("Communication channel {0} on port: {1} is initialized",
                ChannelName, (string)protocolSettings["COM port name"]);

        }

        /// <summary>
        /// Start loop(s) of communication protocol
        /// </summary>
        public override void StartCommunication()
        {
            try
            {
                if (pollingThread.ThreadState == ThreadState.Stopped)
                {
                    pollingThread.Resume();
                }

                if (!comPort.IsOpen)
                {
                    comPort.Open();
                    comPort.DiscardInBuffer();
                }

                pollingThread.Start();
                Logger.Info("Communication channel {0} is started", ChannelName);
            }
            catch (Exception ex)
            {
                Logger.Info($"Communication channel {ChannelName} crushed on start. Message: {ex.Message}");
            }


        }

        /// <summary>
        /// Stop loops of communication with possibility to start them again if necessary
        /// </summary>
        public override void StopCommunication()
        {
            pollingThread.Interrupt();
            Logger.Info("Communication channel {0} is stopped", ChannelName);

        }

        /// <summary>
        /// Release all resources/equipment, finish all threads, dispose objects if necessary
        /// </summary>
        public override void FinishAndDisposeCommunication()
        {
            try
            {
                if (comPort.IsOpen)
                {
                    comPort.DiscardInBuffer();
                    comPort.Close();
                }
            }
            catch
            {
                // TODO add log
            }
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
                    "COM1",
                    "COM port name",
                    "Name of COM port in format like COM1",
                    delegate(string val)
                    {
                        if (!val.StartsWith("COM"))
                        {
                            return "Wrong COM port name format";
                        }

                        return String.Empty;
                    }),
                new SettingsParameter<int>(
                    115200,
                    "BaudRate",
                    "Speed"),
                new SettingsParameter<int>(
                    8,
                    "DataBits",
                    "Number of Data bits"),
                new SettingsParameter<string>(
                    Parity.None.ToString(),
                    "Parity",
                    "Write it as a text (None,Odd,Even,Mark,Space)",
                    delegate(string val)
                    {
                        if (!Enum.TryParse(val, out Parity result))
                        {
                            return $"Value {val} is not a Parity";
                        }

                        return String.Empty;
                    }),
                new SettingsParameter<string>(
                    StopBits.One.ToString(),
                    "StopBits",
                    "Write it as a text (None,One,Two,OnePointFive)",
                    delegate(string val)
                    {
                        if (!Enum.TryParse(val, out StopBits result))
                        {
                            return $"Value {val} is not a StopBit";
                        }

                        return String.Empty;
                    }),
                new SettingsParameter<uint>(
                    50,
                    "Polling interval",
                    "Interval between each requests in ms"),

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
                 new SettingsParameter<string>(
                    RegisterType.HoldingRegister.ToString(),
                    "Type of register",
                    "Type of register in Modbus protocol (HoldingRegister/InputRegister)",
                    delegate(string val)
                    {
                        if (!Enum.TryParse(val, out RegisterType result))
                        {
                            return $"Value {val} is not a RegisterType";
                        }

                        return String.Empty;
                    }),
                 new SettingsParameter<string>(
                    ModbusDataType.Word.ToString(),
                    "RegisterDataType",
                    "Data type (length) of register (Word/Float/Double)",
                    delegate(string val)
                    {
                        if (!Enum.TryParse(val, out ModbusDataType result))
                        {
                            return $"Value {val} is not a ModbusDataType";
                        }

                        return String.Empty;
                    }),
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

        public override int GetDeviceIdBySignal(Signal signal)
        {
            foreach (var slave in slaves.Values)
            {
                foreach (var group in slave.holdingRegisters)
                {
                    var signalResult = group.signalsToRequest.FirstOrDefault(x => x.signal == signal);
                    if (signalResult is not null)
                    {
                        return slave.slaveId;
                    }
                }

                foreach (var group in slave.inputRegisters)
                {
                    var signalResult = group.signalsToRequest.FirstOrDefault(x => x.signal == signal);
                    if (signalResult is not null)
                    {
                        return slave.slaveId;
                    }
                }
            }

            return -1;
        }

        public override void WriteSignalToDevice(Signal signal, int deviceId)
        {
            if (!slaves.ContainsKey(deviceId))
            {
                // TODO log with error
                return;
            }

            slaves[deviceId].WriteSignalToDevice(signal);
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

        void FindAllSlavesAndSetupThem(List<(Signal signal, string settings)> signals)
        {
            // all signals in list in the loop
            // if in the current signal is slave address, but we have no this slave - create
            // for each new address
            //      check if there is group with the same query type, if not - create
            //      if is - check if the first address is on one more tha ours - add signal to this group in the beginning
            //      or if there is in the end address which less on one than ours - add ti the end.
            //      if nothing of this - create new request group

            // !!! TODO this parameter must be calculated by type of signal/register. int 1; float 2, double 4 ...
            //int numOfRegistersInSignal = 1;

            foreach (var signalPair in signals)
            {
                var signalSettings = ParseSettingStringToValues(GetSignalSettings(), signalPair.settings);

                Enum.TryParse((string)signalSettings["RegisterDataType"], out ModbusDataType dataType);
                int numOfRegistersInSignal = (int)dataType;
                int slaveIdFromSettings = Convert.ToInt32(signalSettings["Device address"]);
                var foundSlave = slaves.Values.FirstOrDefault(x => x.slaveId == slaveIdFromSettings);
                if (foundSlave == null)
                {
                    foundSlave = new ModbusSlave() { slaveId = slaveIdFromSettings };
                    slaves.Add(slaveIdFromSettings, foundSlave);
                }
                Enum.TryParse((string)signalSettings["Type of register"], out RegisterType regType);
                RegisterType registerType = regType;
                int registerAddress = Convert.ToInt32(signalSettings["Register Address"]);

                // TODO add more types of registers and mske it via swith case
                if (registerType == RegisterType.InputRegister)
                {
                    AttachSignalToRequestGroup(foundSlave.inputRegisters, signalPair.signal, 
                        registerAddress, dataType, numOfRegistersInSignal);
                }
                else
                {
                    AttachSignalToRequestGroup(foundSlave.holdingRegisters, signalPair.signal, 
                        registerAddress, dataType, numOfRegistersInSignal);
                }
            }
        }


        void AttachSignalToRequestGroup(List<RequestGroup> requestGroups,
            Signal signal, int registerAddress, 
            ModbusDataType datatype, int numOfRegistersInSignal)
        {
            var foundGroup = requestGroups.Find(x => x.startAddress == registerAddress + 1);
            if (foundGroup != null)
            {
                foundGroup.startAddress -= numOfRegistersInSignal;
                foundGroup.registerNum += numOfRegistersInSignal;
                foundGroup.signalsToRequest.Insert(0, new SignalWithRegInfo(signal, numOfRegistersInSignal, datatype, registerAddress));
                return;
            }

            foundGroup = requestGroups.Find(x => x.startAddress + x.registerNum == registerAddress - 1);
            if (foundGroup != null)
            {
                foundGroup.registerNum += numOfRegistersInSignal;
                foundGroup.signalsToRequest.Add(new SignalWithRegInfo(signal, numOfRegistersInSignal, datatype, registerAddress));
                return;
            }

            var newGroup = new RequestGroup()
            {
                startAddress = registerAddress,
                registerNum = numOfRegistersInSignal
            };
            newGroup.signalsToRequest.Add(new SignalWithRegInfo(signal, numOfRegistersInSignal, datatype, registerAddress));

            requestGroups.Add(newGroup);
        }

        // TODO case if we are lost the connection
        // TODO case if we are lose responce (timeout). maybe additional logic but for future
        void Polling()
        {
            while (!stopPolling)
            {
                foreach (var slave in slaves.Values)
                {
                    // check if we have to write something before polling
                    while (slave.SignalsToWriteFc6.Any())
                    {
                        var signalWithInfo = slave.SignalsToWriteFc6.Dequeue();
                        short value = Convert.ToInt16(signalWithInfo.signal.ValueToWrite);
                        bool success = ModbusProtocol.ModbusRtuOld.Modbus.writeRegisterFNC6(
                            comPort,
                            (byte)slave.slaveId,
                            (short)signalWithInfo.registerAddress,
                            value,
                            slave.slaveId.ToString()
                            );

                        if (!success)
                        {
                            // TODO alarming or something
                        }
                    }

                    // do polling
                    foreach (var group in slave.inputRegisters)
                    {
                        if (comPort.IsOpen)
                        {
                            byte[] resultBuf = new byte[group.registerNum * 2];
                            int readedBytes = ModbusProtocol.ModbusRtuOld.Modbus.readMultipleInputRegistersFNC4(
                                comPort,
                                (byte)slave.slaveId,
                                (short)group.startAddress,
                                (short)group.registerNum,
                                resultBuf,
                                slave.slaveId.ToString());

                            if (readedBytes == group.registerNum * 2)
                            {
                                short[] registersWords = new short[group.registerNum];
                                for (int i = 0; i < group.registerNum; i++)
                                {
                                    //byte b1 = resultBuf[i * 2];
                                    //byte b2 = resultBuf[i * 2 + 1];
                                    registersWords[i] = ModbusRtuOld.ComPortHelper.getWord(resultBuf, i * 2);
                                    //registersWords[i] = BitConverter.ToInt16(resultBuf, i * 2);
                                }

                                group.UpdateSignalsAfterRequest(registersWords);
                            }
                        }
                        // TODO polling interval must be not here waiting. Here put tiny sleep, but polling must wait outside of this cycle
                        Thread.Sleep(pollingInterval);
                    }

                    foreach (var group in slave.holdingRegisters)
                    {
                        if (comPort.IsOpen)
                        {
                            byte[] resultBuf = new byte[group.registerNum * 2];
                            int readedBytes = ModbusProtocol.ModbusRtuOld.Modbus.readMultipleRegistersFNC3(
                                comPort,
                                (byte)slave.slaveId,
                                (short)group.startAddress,
                                (short)group.registerNum,
                                resultBuf,
                                slave.slaveId.ToString());

                            if (readedBytes == group.registerNum * 2)
                            {
                                short[] registersWords = new short[group.registerNum];
                                for (int i = 0; i < group.registerNum; i++)
                                {
                                    //byte b1 = resultBuf[i * 2];
                                    //byte b2 = resultBuf[i * 2 + 1];
                                    registersWords[i] = ModbusRtuOld.ComPortHelper.getWord(resultBuf, i * 2);
                                    //registersWords[i] = BitConverter.ToInt16(resultBuf, i * 2);
                                }

                                group.UpdateSignalsAfterRequest(registersWords);
                            }
                        }
                        // TODO polling interval must be not here waiting. Here put tiny sleep, but polling must wait outside of this cycle
                        Thread.Sleep(pollingInterval);
                    }
                }
            }
        }

    }
}
