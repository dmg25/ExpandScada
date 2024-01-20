using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Gateway;

namespace ModbusProtocol
{
    // we put here each signal, which belongs to this device
    // signals will be grouped by type
    // each type has arrays of the Signals with types and adresses
    // polling will be splitted on this arrays
    internal class ModbusSlave
    {
        internal int slaveId;
        internal List<RequestGroup> holdingRegisters = new List<RequestGroup>();
        internal List<RequestGroup> inputRegisters = new List<RequestGroup>();
    }

    // One group represents one request
    internal class RequestGroup
    {
        internal int startAddress;
        internal int registerNum;
        internal List<(Signal signal, int signalRegistersNum, ModbusDataType datatype)> signalsToRequest = new List<(Signal signal, int signalRegistersNum, ModbusDataType datatype)>();

        internal void UpdateSignalsAfterRequest(short[] responceResultWords)
        {
            int registerCounter = 0;
            for (int i = 0; i < signalsToRequest.Count;)
            {
                // check datatype of each signal and convert 1-2-4 words to this type
                // move index on 1-2-4 values as well

                // TODO care about signal's datatype, maybe casting problems
                switch (signalsToRequest[i].datatype)
                {
                    case ModbusDataType.Word:
                        signalsToRequest[i].signal.Value = responceResultWords[i];
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
