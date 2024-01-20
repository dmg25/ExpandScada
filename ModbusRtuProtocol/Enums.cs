using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusProtocol
{
    public enum RegisterType : byte
    {
        InputRegister,
        HoldingRegister
    }

    public enum ModbusDataType : int
    {
        Word = 1,
        Float = 2,
        Double = 4,
    }
}
