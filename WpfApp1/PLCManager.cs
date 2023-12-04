using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DXH.Modbus;

namespace WpfApp1
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
                        var count = XinjiePLC.ModbusRead(1, 3, 0, 1);
                        if (count == null)
                            continue;
                        Count = count[0];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("StartReadPLC:" + ex.Message);
                    }
                }
            });
        }

        private static int _count;

        public static int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                PLCCountChanged?.Invoke(null,null);
            }
        }


        public static event EventHandler<int> PLCStatusChanged;
        public static event EventHandler PLCCountChanged;


        static int[] mPLCStatus = new int[6];
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
