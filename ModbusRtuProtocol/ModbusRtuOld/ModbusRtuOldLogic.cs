using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace ModbusProtocol.ModbusRtuOld
{
    internal enum ModbusFunctions
    {
        FNC1_READ_MULTIPLE_COILS_STATUS = 0x01,    // 0xxxx DO
        FNC2_READ_MULTIPLE_INPUT_DISCRETES = 0x02, // 1xxxx DI
        FNC3_READ_MULTIPLE_REGISTERS = 0x03,       // 4xxxx AO
        FNC4_READ_MULTIPLE_INPUT_REGISTERS = 0x04, // 3xxxx AI
        FNC5_WRITE_SINGLE_COIL = 0x05,             // 0xxxx DO
        FNC6_WRITE_SINGLE_REGISTER = 0x06,         // 4xxxx AO
        FNC15_FORCE_MULTIPLE_COILS = 0x0F,         // 0xxxx DO
        FNC16_WRITE_MULTIPLE_REGISTERS = 0x10      // 4xxxx AO
    }

    internal enum ModbusErrors
    {
        NO_ERRORS = 0x00,
        READ_TIMEOUT = 0x01,
        BAD_CRC = 0x02,
        INCORERCT_REPLY = 0x03
    }

    internal class Modbus
    {

        internal static ModbusErrors mbError;

        internal Modbus()
        {
        }

        internal static int writeMultipleRegistersFNC16(SerialPort com, byte id, Int16 referenceNumber, Int16 wordCount, byte[] regBuf, string deviceName)
        {
            mbError = 0;
            if (!com.IsOpen)
            {
                ////MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return 0;
            }
            com.DiscardInBuffer();
            com.DiscardOutBuffer();
            byte[] msg = new byte[7 + wordCount * 2 + 2];
            msg[0] = id;
            msg[1] = (byte)ModbusFunctions.FNC16_WRITE_MULTIPLE_REGISTERS;

            byte[] tmpBuf = BitConverter.GetBytes(referenceNumber);
            msg[2] = tmpBuf[1];
            msg[3] = tmpBuf[0];

            tmpBuf = BitConverter.GetBytes(wordCount);
            msg[4] = tmpBuf[1];
            msg[5] = tmpBuf[0];
            msg[6] = (byte)(wordCount * 2);

            Array.Copy(regBuf, 0, msg, 7, wordCount * 2);
            byte[] crc16 = calcCrc16(msg, 7 + wordCount * 2);
            msg[msg.Length - 1] = crc16[0];
            msg[msg.Length - 2] = crc16[1];

            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return 0;
            }

            com.Write(msg, 0, msg.Length);
            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Request: {BitConverter.ToString(msg)}");

            byte[] recvBuf = new byte[255];
            int writeCount = 0;
            try
            {
                int readed = ComPortHelper.readNumberFromCom(com, recvBuf, 0, 2, com.ReadTimeout, id, deviceName);
                if (recvBuf[0] == id && recvBuf[1] == (byte)ModbusFunctions.FNC16_WRITE_MULTIPLE_REGISTERS)
                {
                    readed += ComPortHelper.readNumberFromCom(com, recvBuf, 2, 6, com.ReadTimeout, id, deviceName);
                    //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Reply: {BitConverter.ToString(recvBuf, 0, readed)}");
                    if (readed == 8)
                    {
                        byte[] crc = calcCrc16(recvBuf, readed - 2);

                        if (recvBuf[readed - 1] == crc[0] && recvBuf[readed - 2] == crc[1])
                        {
                            Int16 refNumber = ComPortHelper.getWord(recvBuf, 2);
                            Int16 wordCnt = ComPortHelper.getWord(recvBuf, 4);
                            if (referenceNumber == refNumber && wordCount == wordCnt) writeCount = wordCount;
                        }
                    }
                }
                else
                {
                    if (!com.IsOpen)
                    {
                        //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                        return 0;
                    }
                    readed = com.Read(recvBuf, 2, com.BytesToRead);
                    //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Write registes error - {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                }
            }
            catch (TimeoutException)
            {
                mbError = ModbusErrors.READ_TIMEOUT;
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"TIMEOUT");
            }

            return writeCount;
        }


        //id - адрес, referenceNumber - адрес регистра, regValue - значение для записи
        internal static bool writeRegisterFNC6(SerialPort com, byte id, Int16 referenceNumber, Int16 regValue, string deviceName)
        {
            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return false;
            }
            com.DiscardInBuffer();
            com.DiscardOutBuffer();

            mbError = ModbusErrors.NO_ERRORS;
            byte[] msg = new byte[6 + 2];
            msg[0] = id;
            msg[1] = (byte)ModbusFunctions.FNC6_WRITE_SINGLE_REGISTER;

            byte[] tmpBuf = BitConverter.GetBytes(referenceNumber);
            msg[2] = tmpBuf[1];
            msg[3] = tmpBuf[0];

            tmpBuf = BitConverter.GetBytes(regValue);
            msg[4] = tmpBuf[1];
            msg[5] = tmpBuf[0];

            byte[] crc16 = calcCrc16(msg, 6);
            msg[msg.Length - 1] = crc16[0];
            msg[msg.Length - 2] = crc16[1];

            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return false;
            }

            com.Write(msg, 0, msg.Length);
            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Request: {BitConverter.ToString(msg)}");

            byte[] recvBuf = new byte[255];
            bool successful = false;
            try
            {
                int readed = ComPortHelper.readNumberFromCom(com, recvBuf, 0, 2, com.ReadTimeout, id, deviceName);
                if (recvBuf[0] == id && recvBuf[1] == (byte)ModbusFunctions.FNC6_WRITE_SINGLE_REGISTER)
                {
                    readed += ComPortHelper.readNumberFromCom(com, recvBuf, 2, 6, com.ReadTimeout, id, deviceName);
                    if (readed == 8)
                    {
                        byte[] crc = calcCrc16(recvBuf, readed - 2);

                        if (recvBuf[readed - 1] == crc[0] && recvBuf[readed - 2] == crc[1])
                        {
                            Int16 refNumber = ComPortHelper.getWord(recvBuf, 2);
                            Int16 respRegValue = ComPortHelper.getWord(recvBuf, 4);
                            successful = (referenceNumber == refNumber && regValue == respRegValue);
                        }
                    }
                    //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Reply: {BitConverter.ToString(recvBuf, 0, readed)}");
                }
                else
                {
                    if (!com.IsOpen)
                    {
                        //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                        return false;
                    }
                    com.ReadExisting();
                    //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Write registes error - {BitConverter.ToString(recvBuf, 0, readed)}");
                }
            }
            catch (TimeoutException)
            {
                mbError = ModbusErrors.READ_TIMEOUT;
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"TIMEOUT");
            }
            return successful;
        }

        private static int readMultipleRegisters(SerialPort com, ModbusFunctions fc, byte id, Int16 referenceNumber, Int16 wordCount, byte[] recvRegBuf, string deviceName)
        {
            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return 0;
            }
            com.DiscardInBuffer();
            com.DiscardOutBuffer();
            byte[] msg = makeMbReadRequest(fc, id, referenceNumber, wordCount);
            int byteCount = 0;
            try
            {
                if (!com.IsOpen)
                {
                    //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                    return 0;
                }

                com.Write(msg, 0, msg.Length);
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Request: {BitConverter.ToString(msg)}");

                byte[] recvBuf = new byte[3 + wordCount * 2 + 2];


                int readed = ComPortHelper.readNumberFromCom(com, recvBuf, 0, 3, com.ReadTimeout, id, deviceName);
                if (readed == 3 && recvBuf[0] == id && recvBuf[1] == (byte)fc && recvBuf[2] == wordCount * 2)
                {
                    readed = ComPortHelper.readNumberFromCom(com, recvBuf, 3, recvBuf[2] + 2, com.ReadTimeout, id, deviceName);
                    byte[] crc16 = calcCrc16(recvBuf, 3 + recvBuf[2]);
                    if (recvBuf[1] == (byte)fc && recvRegBuf != null && recvRegBuf.Length >= wordCount * 2 &&
                        recvBuf[recvBuf.Length - 1] == crc16[0] && recvBuf[recvBuf.Length - 2] == crc16[1])
                    {
                        Array.Copy(recvBuf, 3, recvRegBuf, 0, recvBuf[2]);
                        byteCount = recvBuf[2];
                        //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Reply: {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                    }
                    else
                    {
                        if (recvBuf[recvBuf.Length - 1] != crc16[0] && recvBuf[recvBuf.Length - 2] != crc16[1])
                        {
                            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Incorrect CRC: {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                            mbError = ModbusErrors.BAD_CRC;
                        }
                        else
                        {
                            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Incorrect reply: {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                        }
                    }
                }
                else
                {
                    if (!com.IsOpen)
                    {
                        //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                        return 0;
                    }
                    if (com.BytesToRead > 0 && com.BytesToRead < recvBuf.Length - readed)
                        readed = com.Read(recvBuf, 3, com.BytesToRead);
                    else readed = 0;
                    //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Incorrect reply: {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                }
                mbError = 0;
            }
            catch (TimeoutException)
            {
                mbError = ModbusErrors.READ_TIMEOUT;
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"TIMEOUT");
            }

            return byteCount;
        }

        internal static int readMultipleRegistersFNC3(SerialPort com, byte id, Int16 referenceNumber, Int16 wordCount, byte[] recvRegBuf, string deviceName)
        {
            return readMultipleRegisters(com, ModbusFunctions.FNC3_READ_MULTIPLE_REGISTERS, id, referenceNumber, wordCount, recvRegBuf, deviceName);
        }

        internal static int readMultipleInputRegistersFNC4(SerialPort com, byte id, Int16 referenceNumber, Int16 wordCount, byte[] recvRegBuf, string deviceName)
        {
            return readMultipleRegisters(com, ModbusFunctions.FNC4_READ_MULTIPLE_INPUT_REGISTERS, id, referenceNumber, wordCount, recvRegBuf, deviceName);
        }

        internal static List<string> readDeviceIdentificationFNC43(SerialPort com, byte id, byte readDeviceCode, byte objectId, string deviceName)
        {
            byte[] request = new byte[5 + 2];
            request[0] = id;
            request[1] = 0x2B;
            request[2] = 0x0E;
            request[3] = readDeviceCode;
            request[4] = 0; //objectId;

            byte[] crc = calcCrc16(request, 5);
            request[5] = crc[1];
            request[6] = crc[0];

            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return null;
            }

            com.DiscardInBuffer();
            com.DiscardOutBuffer();

            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return null;
            }

            com.Write(request, 0, request.Length);

            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Request: {BitConverter.ToString(request)}");

            byte[] recvBuf = new byte[255];

            List<string> result = new List<string>();

            //string str = "";
            try
            {
                int readed = ComPortHelper.readNumberFromCom(com, recvBuf, 0, 8, com.ReadTimeout, id, deviceName);
                int nObj = (int)recvBuf[7];

                for (int i = 0; i < nObj; i++)
                {
                    readed = ComPortHelper.readNumberFromCom(com, recvBuf, 0, 2, com.ReadTimeout, id, deviceName);
                    if (readed == 2)
                    {
                        readed = ComPortHelper.readNumberFromCom(com, recvBuf, 0, (int)recvBuf[1], com.ReadTimeout, id, deviceName);
                        //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Reply: {BitConverter.ToString(recvBuf, 0, readed + 2)}");
                        result.Add(Encoding.GetEncoding(1251).GetString(recvBuf, 0, readed));
                        //str = str + string.Format("{0} - {1}\n", i + 1, Encoding.GetEncoding(1251).GetString(recvBuf, 0, readed));
                    }
                }
            }
            catch (TimeoutException)
            {
                mbError = ModbusErrors.READ_TIMEOUT;
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"TIMEOUT");
                //if (str != null) str = "TIMOUT\r\n";
            }
            catch (Exception ex)
            {
                mbError = ModbusErrors.READ_TIMEOUT;
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(ex.Message);
                //if (str != null) str = "TIMOUT\r\n";
            }

            return result;
        }

        internal static int readMultipleCoilsStatusFNC1(SerialPort com, byte id, Int16 referenceNumber, Int16 bitCount, byte[] recvBitBuf, string deviceName)
        {
            return readMultipleCoilsDiscretes(com, ModbusFunctions.FNC1_READ_MULTIPLE_COILS_STATUS, id, referenceNumber, bitCount, recvBitBuf, deviceName);
        }

        private static int readMultipleCoilsDiscretes(SerialPort com, ModbusFunctions fc, byte id, Int16 referenceNumber, Int16 bitCount, byte[] recvBitBuf, string deviceName)
        {
            mbError = ModbusErrors.NO_ERRORS;

            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return 0;
            }

            com.DiscardInBuffer();
            com.DiscardOutBuffer();
            byte[] msg = makeMbReadRequest(fc, id, referenceNumber, bitCount);

            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return 0;
            }

            com.Write(msg, 0, msg.Length);
            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Request: {BitConverter.ToString(msg)}");

            int dataBytesToRead = (bitCount + 7) / 8;
            int byteCountAdd = 0;
            if (bitCount % 8 > 0) byteCountAdd = 1;
            byte[] recvBuf = new byte[3 + dataBytesToRead + 2];
            int byteCount = 0;
            try
            {
                int readed = ComPortHelper.readNumberFromCom(com, recvBuf, 0, 3, com.ReadTimeout, id, deviceName);
                if (readed == 3 && recvBuf[0] == id && recvBuf[1] == (byte)fc && recvBuf[2] + byteCountAdd == dataBytesToRead)
                {
                    readed = ComPortHelper.readNumberFromCom(com, recvBuf, 3, recvBuf[2] + byteCountAdd + 2, com.ReadTimeout, id, deviceName);
                    byte[] crc16 = calcCrc16(recvBuf, 3 + recvBuf[2] + byteCountAdd);
                    if (recvBitBuf != null && recvBitBuf.Length >= dataBytesToRead &&
                        recvBuf[recvBuf.Length - 1] == crc16[0] && recvBuf[recvBuf.Length - 2] == crc16[1])
                    {
                        Array.Copy(recvBuf, 3, recvBitBuf, 0, recvBuf[2] + byteCountAdd);
                        byteCount = recvBuf[2] + byteCountAdd;
                        //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Reply: {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                    }
                    else
                    {
                        if (recvBuf[recvBuf.Length - 1] != crc16[0] && recvBuf[recvBuf.Length - 2] != crc16[1])
                        {
                            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Incorrect CRC: {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                            mbError = ModbusErrors.BAD_CRC;
                        }
                        else
                        {
                            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Incorrect reply: {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                            mbError = ModbusErrors.INCORERCT_REPLY;
                        }
                    }
                }
                else
                {
                    if (!com.IsOpen)
                    {
                        //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                        return 0;
                    }
                    if (com.BytesToRead < recvBuf.Length - readed)
                        readed = com.Read(recvBuf, 3, com.BytesToRead);
                    else readed = 0;
                    //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Incorrect reply: {BitConverter.ToString(recvBuf, 0, readed + 3)}");
                    mbError = ModbusErrors.INCORERCT_REPLY;
                }

            }
            catch (TimeoutException)
            {
                mbError = ModbusErrors.READ_TIMEOUT;
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"TIMEOUT");
            }

            return byteCount;
        }

        internal static bool writeSingleCoilFNC5(SerialPort com, byte id, Int16 referenceNumber, bool value, string deviceName)
        {
            mbError = ModbusErrors.NO_ERRORS;

            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return false;
            }

            com.DiscardInBuffer();
            com.DiscardOutBuffer();
            byte[] msg = new byte[6 + 2];
            msg[0] = id;
            msg[1] = (byte)ModbusFunctions.FNC5_WRITE_SINGLE_COIL;

            byte[] tmpBuf = BitConverter.GetBytes(referenceNumber);
            msg[2] = tmpBuf[1];
            msg[3] = tmpBuf[0];

            msg[4] = value == true ? (byte)0xFF : (byte)0;
            msg[5] = 0;

            byte[] crc16 = calcCrc16(msg, 6);
            msg[6] = crc16[1];
            msg[7] = crc16[0];

            if (!com.IsOpen)
            {
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error(CommonVM.GetText("PortClosedUnexpectedly"));
                return false;
            }

            com.Write(msg, 0, msg.Length);
            //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Trace($"Request: {BitConverter.ToString(msg)}");

            byte[] recvBuf = new byte[6 + 2];
            bool retCode = true;
            try
            {
                int readed = ComPortHelper.readNumberFromCom(com, recvBuf, 0, 6 + 2, com.ReadTimeout, id, deviceName);
                for (int i = 0; i < 6 + 2; i++)
                    if (readed != 6 + 2 || recvBuf[i] != msg[i])
                    {
                        //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"Write single coil error: {BitConverter.ToString(recvBuf, 0, readed)}");
                        retCode = false;
                        break;
                    }
            }
            catch (TimeoutException)
            {
                mbError = ModbusErrors.READ_TIMEOUT;
                //MainLog.logFrames.WithProperty("source", $"{deviceName}:{id}").Error($"TIMEOUT");
                retCode = false;
            }

            return retCode;

        }


        private static byte[] makeMbReadRequest(ModbusFunctions fc, byte id, Int16 referenceNumber, Int16 wordCount)
        {
            byte[] request = new byte[6 + 2];
            request[0] = id;
            request[1] = (byte)fc;
            byte[] tmpBuf = BitConverter.GetBytes(referenceNumber);
            request[2] = tmpBuf[1];
            request[3] = tmpBuf[0];
            tmpBuf = BitConverter.GetBytes(wordCount);
            request[4] = tmpBuf[1];
            request[5] = tmpBuf[0];

            byte[] crc = calcCrc16(request, 6);
            request[6] = crc[1];
            request[7] = crc[0];

            return request;
        }

        private static byte[] calcCrc16(byte[] msg, int msgLen)
        {
            byte uchCRCHi = 0xFF;
            byte uchCRCLo = 0xFF;
            int uIndex;
            int ind = 0;
            while (msgLen-- > 0)  /* pass through message buffer*/
            {
                uIndex = uchCRCHi ^ msg[ind]; /* calculate the CRC */
                ind++;
                uchCRCHi = (byte)(uchCRCLo ^ auchCRCHi[uIndex]);
                uchCRCLo = auchCRCLo[uIndex];
            }
            short crc16 = (short)((uchCRCHi << 8) | uchCRCLo);
            return BitConverter.GetBytes(crc16);
        }

        static byte[] auchCRCHi = {
                                        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                                        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                                        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                                        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                                        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                                        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                                        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                                        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                                        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                                        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                                        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                                        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                                        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                                        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                                        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                                        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                                        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                                        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                                        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                                        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                                        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                                        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                                        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                                        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                                        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                                        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40};

        static byte[] auchCRCLo = {
                                        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
                                        0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
                                        0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
                                        0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
                                        0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
                                        0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
                                        0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
                                        0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
                                        0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
                                        0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
                                        0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
                                        0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
                                        0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
                                        0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
                                        0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
                                        0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
                                        0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
                                        0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
                                        0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
                                        0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
                                        0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
                                        0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
                                        0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
                                        0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
                                        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
                                        0x43, 0x83, 0x41, 0x81, 0x80, 0x40};

    }
}
