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
        internal List<RequestGroup> holdingRegisters = new List<RequestGroup>();
        internal List<RequestGroup> inputRegisters = new List<RequestGroup>();
    }

    // One group represents one request
    internal class RequestGroup
    {
        internal int startAddress;
        internal int registerNum;
        internal List<(Signal signal, int signalRegistersNum)> signalsToRequest = new List<(Signal signal, int signalRegistersNum)>();

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
