using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Gateway;

namespace ModbusProtocol
{
    internal class SignalWithRegInfo
    {
        internal Signal signal;
        internal int signalRegistersNumInGroup;
        internal int registerAddress;
        internal ModbusDataType datatype;

        internal SignalWithRegInfo(Signal signal, int signalRegistersNum, ModbusDataType datatype, int registerAddress) 
        {
            this.signal = signal;
            this.signalRegistersNumInGroup = signalRegistersNum;
            this.datatype = datatype;
            this.registerAddress = registerAddress;
        }
    }

    // we put here each signal, which belongs to this device
    // signals will be grouped by type
    // each type has arrays of the Signals with types and adresses
    // polling will be splitted on this arrays
    internal class ModbusSlave
    {
        internal int slaveId;
        internal List<RequestGroup> holdingRegisters = new List<RequestGroup>();
        internal List<RequestGroup> inputRegisters = new List<RequestGroup>();

        // if we want to write one signal - add it to special list (queue) 
        //      signal must be taken from "signalsToRequest" list as tuple
        // on polling we will check this Before polling
        // if not empty - take value of this signal  - try to write and clean it on any result even on error.
        // then make a polling

        internal Queue<SignalWithRegInfo> SignalsToWriteFc6 = new Queue<SignalWithRegInfo>();

        internal SignalWithRegInfo GetSignalFromHoldings(Signal signal)
        {
            foreach (var group in holdingRegisters)
            {
                var signalWithReg = group.signalsToRequest.FirstOrDefault(x => x.signal == signal);
                if (signalWithReg is not null)
                {
                    return signalWithReg;
                }
            }

            return null;
        }

        internal void WriteSignalToDevice(Signal signal)
        {
            var signalToWrite = GetSignalFromHoldings(signal);
            if (signalToWrite is not null)
            {
                SignalsToWriteFc6.Enqueue(signalToWrite);
            }
            else
            {
                // TODO write to log error
            }
        }
    }

    // One group represents one request
    internal class RequestGroup
    {
        internal int startAddress;
        internal int registerNum;
        internal List<SignalWithRegInfo> signalsToRequest = new List<SignalWithRegInfo>();

        internal void UpdateSignalsAfterRequest(short[] responceResultWords)
        {
            int registerCounter = 0;
            for (int i = 0; i < signalsToRequest.Count;)
            {
                // check datatype of each signal and convert 1-2-4 words to this type
                // move index on 1-2-4 values as well

                // TODO care about signal's datatype, !WILL BE! casting problems
                switch (signalsToRequest[i].datatype)
                {
                    case ModbusDataType.Word:
                        signalsToRequest[i].signal.Value = (int)responceResultWords[i];
                        i++;
                        break;
                    case ModbusDataType.Float:
                        signalsToRequest[i].signal.Value = ModbusRtuOld.ComPortHelper.getFloat(
                            responceResultWords, i, ModbusRtuOld.FLOAT_BYTE_ORDER.F1032);
                        i += 2;
                        break;
                    //case ModbusDataType.Double:
                    //    signalsToRequest[i].signal.Value = responceResultWords[i];
                    //    break;
                    default:
                        signalsToRequest[i].signal.Value = responceResultWords[i];
                        i++;
                        break;
                }


            }



        }

        internal void UpdateSignalsAfterRequest(int[] responceResult)
        {
            int registerCounter = 0;
            for (int i = 0; i < signalsToRequest.Count; i++)
            {
                // TODO here must be a lot of data conversion
                // Now it is only for INTs
                signalsToRequest[i].signal.Value = responceResult[i];
            }



        }


    }


}
