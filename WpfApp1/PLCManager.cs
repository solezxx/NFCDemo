using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DXH.Modbus;

namespace NFCDemo
{
    public static class PLCManager
    {

        public static DXH.Modbus.DXHModbusRTU XinjiePLC;

        public static void Initialize()
        {
            XinjiePLC = new DXHModbusRTU(Global.ModbusRTU_COM, int.Parse(Global.PLCBaudRate));
            XinjiePLC.StartConnect();
            StartReadPLC();
        }

        static bool mStartReadPLCStatus = false;
        public static async void StartReadPLC()
        {
            if (!mStartReadPLCStatus)
                mStartReadPLCStatus = true;
            else
                return;

            await Task.Run(() =>
            {
                while (mStartReadPLCStatus)
                {
                    try
                    {
                        System.Threading.Thread.Sleep(100);
                        var count = XinjiePLC.ModbusRead(1, 3, 0, 20);
                        var stop = XinjiePLC.ModbusRead(1, 1, 300, 20);
                        if (count == null || stop == null)
                            continue;
                        Count = count;
                        Stop = stop;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("StartReadPLC:" + ex.Message);
                    }
                }
            });
        }

        private static int[] _count = new int[20];

        public static int[] Count
        {
            get { return _count; }
            set
            {
                _count = value;
                PLCCountChanged?.Invoke(null, null);
            }
        }
        private static int[] _stop = new int[20];

        public static int[] Stop
        {
            get { return _stop; }
            set
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] != _stop[i])
                    {
                        if (value[i] == 1)
                            PLCStopChanged?.Invoke(null, i);
                    }
                }
                _stop = value;
            }
        }

        public static event EventHandler<int> PLCStatusChanged;
        public static event EventHandler PLCCountChanged;
        public static event EventHandler<int> PLCStopChanged;


        static int[] mPLCStatus = new int[100];
        public static int[] PLCStatus
        {
            get { return mPLCStatus; }
            set
            {
                if (value != null)
                {
                    for (int i = 0; i < mPLCStatus.Length; i++)
                    {
                        if (value[i] != mPLCStatus[i])
                        {
                            if (value[i] == 1)
                            {
                                if (PLCStatusChanged != null)
                                {
                                    PLCStatusChanged(null, i);
                                }
                            }
                        }
                    }
                    mPLCStatus = value;
                }
            }
        }
    }
}
