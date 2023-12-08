using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Windows;

namespace NFCDemo
{
    public static class Global
    {
        public static void Ini()
        {
            LoadIni();
        }
        #region  PLC参数
        static string mModbusRTU_COM = "COM1";
        public static string ModbusRTU_COM
        {
            get { return mModbusRTU_COM; }
            set
            {
                mModbusRTU_COM = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("PLC参数", "串口号", mModbusRTU_COM, GlobalFile);
            }
        }

        static string plcbaudRate = "9600";
        public static string PLCBaudRate
        {
            get { return plcbaudRate; }
            set
            {
                plcbaudRate = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("PLC参数", "波特率", plcbaudRate, GlobalFile);
            }
        }

        #endregion

        #region 读卡器参数

        static string _nfcCom = "COM2";
        public static string NFCCom
        {
            get { return _nfcCom; }
            set
            {
                _nfcCom = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("读卡器参数", "串口号", _nfcCom, GlobalFile);
            }
        }
        static string baudRate = "9600";
        public static string BaudRate
        {
            get { return baudRate; }
            set
            {
                baudRate = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("读卡器参数", "波特率", baudRate, GlobalFile);
            }
        }

        #endregion

        #region 其他

        static string machineid = "1";
        public static string MachineID
        {
            get { return machineid; }
            set
            {
                machineid = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("其他", "机台编号", machineid, GlobalFile);
            }
        }

        static string timeout = "1";
        public static string TimeOut
        {
            get { return timeout; }
            set
            {
                timeout = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("其他", "超时时间", timeout, GlobalFile);
            }
        }
        #endregion

        public static string GlobalFile = AppDomain.CurrentDomain.BaseDirectory + "Global.ini";
        public static void LoadIni()
        {
            if (File.Exists(GlobalFile))
            {

                mModbusRTU_COM = DXH.Ini.DXHIni.ContentReader("PLC参数", "串口号", mModbusRTU_COM, GlobalFile);
                plcbaudRate = DXH.Ini.DXHIni.ContentReader("PLC参数", "波特率", plcbaudRate, GlobalFile);

                _nfcCom = DXH.Ini.DXHIni.ContentReader("读卡器参数", "串口号", _nfcCom, GlobalFile);
                baudRate = DXH.Ini.DXHIni.ContentReader("读卡器参数", "波特率", baudRate, GlobalFile);

                machineid = DXH.Ini.DXHIni.ContentReader("其他", "机台编号", machineid, GlobalFile);
                timeout = DXH.Ini.DXHIni.ContentReader("其他", "超时时间", timeout, GlobalFile);
            }
            else
            {
                ModbusRTU_COM = mModbusRTU_COM;
                PLCBaudRate = plcbaudRate;

                NFCCom = _nfcCom;
                BaudRate = baudRate;

                MachineID = machineid;
                TimeOut = timeout;
            }
        }
        public static string FilePath = Directory.GetCurrentDirectory() + "/logs/";
        static object LogLock = new object();
        public static async void SaveLog(string message)
        {
            Task Task_SaveLog = Task.Run(() =>
            {
                try
                {
                    lock (LogLock)
                    {
                        if (!Directory.Exists(FilePath + DateTime.Now.ToString("yyyy-MM")))
                            Directory.CreateDirectory(FilePath + DateTime.Now.ToString("yyyy-MM"));
                        File.AppendAllText(FilePath + DateTime.Now.ToString("yyyy-MM") + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.") + DateTime.Now.Millisecond + "    " + message + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SaveDebugLog:" + ex.Message);
                }
            });
            await Task_SaveLog;
        }
    }
}
