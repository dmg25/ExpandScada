using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ModbusProtocol.ModbusRtuOld
{
    /// <summary>
    /// Варианты последовательностей байт в двойных регистрах типа Float
    /// </summary>
    internal enum FLOAT_BYTE_ORDER
    {
        F3210 = 0,
        F1032 = 1
    }


    internal class ComPortHelper
    {
        internal static float getFloat(byte[] b, int startInd)    //F1032
        {
            byte[] floatVal = new byte[4];

            floatVal[1] = b[startInd + 0];
            floatVal[0] = b[startInd + 1];
            floatVal[3] = b[startInd + 2];
            floatVal[2] = b[startInd + 3];


            return BitConverter.ToSingle(floatVal, 0);
        }

        internal static Int16 getWord(byte[] b, int startInd)
        {
            byte[] shortVal = new byte[2];
            shortVal[0] = b[startInd + 1];
            shortVal[1] = b[startInd + 0];
            return BitConverter.ToInt16(shortVal, 0);
        }

        internal static Int32 getDWord(byte[] b, int startInd)
        {
            byte[] wordVal = new byte[4];
            wordVal[2] = b[startInd + 1];
            wordVal[3] = b[startInd + 0];
            wordVal[0] = b[startInd + 3];
            wordVal[1] = b[startInd + 2];
            return BitConverter.ToInt32(wordVal, 0);
        }

        internal static void packValueToByteArray(byte[] array, int startIndex, short val)
        {
            byte[] bVal = BitConverter.GetBytes(val);
            array[startIndex] = bVal[1];
            array[startIndex + 1] = bVal[0];
        }

        internal static void packValueToByteArray(byte[] array, int startIndex, ushort val)
        {
            byte[] bVal = BitConverter.GetBytes(val);
            array[startIndex] = bVal[1];
            array[startIndex + 1] = bVal[0];
        }

        internal static void packValueToByteArray(byte[] array, int startIndex, float val)    //F1032
        {
            byte[] bVal = BitConverter.GetBytes(val);
            array[startIndex] = bVal[1];
            array[startIndex + 1] = bVal[0];
            array[startIndex + 2] = bVal[3];
            array[startIndex + 3] = bVal[2];
        }


        internal static int readNumberFromCom(SerialPort comPort, byte[] recvBuf, int offset, int count, int timeoutMs, byte modbusAddr, string deviceName)
        {
            int readed = 0;
            DateTime t1 = DateTime.Now;
            // try
            // {
            while (comPort.BytesToRead < count)
            {
                TimeSpan dt = DateTime.Now - t1;
                if (dt.TotalMilliseconds > timeoutMs)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(50);
                if (!comPort.IsOpen)
                {
                    //MainLog.logFrames.WithProperty("source", $"{deviceName}:{modbusAddr}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                    return 0;
                }
            }

            if (!comPort.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{modbusAddr}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return 0;
            }

            readed = comPort.Read(recvBuf, offset, count); // количество байт в ответе, с учетом диапазона
                                                           //  }
                                                           // catch (Exception ex) { return 0; }
            return readed;
        }

        internal static float getFloat(byte[] b, int startInd, FLOAT_BYTE_ORDER fByteOrder)
        {
            byte[] floatVal = new byte[4];

            switch (fByteOrder)
            {
                case FLOAT_BYTE_ORDER.F3210:        // для чтения/записи в TGD
                    {
                        floatVal[3] = b[startInd + 0];
                        floatVal[2] = b[startInd + 1];
                        floatVal[1] = b[startInd + 2];
                        floatVal[0] = b[startInd + 3];
                    }
                    break;
                case FLOAT_BYTE_ORDER.F1032:        // для чтения/записи в HMUX
                    {
                        floatVal[1] = b[startInd + 0];
                        floatVal[0] = b[startInd + 1];
                        floatVal[3] = b[startInd + 2];
                        floatVal[2] = b[startInd + 3];
                    }
                    break;
            }

            return BitConverter.ToSingle(floatVal, 0);
        }


        internal static float getFloat(short[] shb, int startInd, FLOAT_BYTE_ORDER fByteOrder)
        {
            byte[] b = new byte[4];

            b[0] = (byte)((shb[startInd + 0] & 0xFF00) >> 8);
            b[1] = (byte)((shb[startInd + 0] & 0x00FF));
            b[2] = (byte)((shb[startInd + 1] & 0xFF00) >> 8);
            b[3] = (byte)((shb[startInd + 1] & 0x00FF));

            return getFloat(b, 0, fByteOrder);
        }

        internal static void packValueToByteArray(byte[] array, int startIndex, float val, FLOAT_BYTE_ORDER fByteOrder)
        {
            byte[] bVal = BitConverter.GetBytes(val);
            switch (fByteOrder)
            {
                case FLOAT_BYTE_ORDER.F3210:        // чтение/запись в TGD
                    {
                        array[startIndex] = bVal[3];
                        array[startIndex + 1] = bVal[2];
                        array[startIndex + 2] = bVal[1];
                        array[startIndex + 3] = bVal[0];
                    }
                    break;

                case FLOAT_BYTE_ORDER.F1032:       // чтение/запись в HMUX     
                    {
                        array[startIndex] = bVal[1];
                        array[startIndex + 1] = bVal[0];
                        array[startIndex + 2] = bVal[3];
                        array[startIndex + 3] = bVal[2];
                    }
                    break;
            }
        }
    }
}
