using Demo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;

namespace NFCDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            viewModel = DataContext as MainWindowViewModel;
            Global.Ini();
            PLCManager.Initialize();
            PLCManager.XinjiePLC.ModbusStateChanged += Modbus_ConnectStateChanged;  
            OpenCom();
            ReadA();
        }

        private void Modbus_ConnectStateChanged(object sender, bool e)
        {
            Dispatcher.BeginInvoke(new Action((() =>
            {
                LdrLog("PLC连接：" + e);
                if (e)
                {
                    PlcState.Fill = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    PlcState.Fill = new SolidColorBrush(Colors.Red);
                }
            })));
        }

        UserManager userManager;
        MainWindowViewModel viewModel;

        User LastUser = null;
        public void OpenCom()
        {
            if (IntPtr.Zero == reader.GetHComm())                   //判断串口是否打开
            {
                int ret = reader.OpenComm(Convert.ToInt32(Global.NFCCom.Replace("COM", "")), int.Parse(Global.BaudRate));
                if (0 == ret)
                {
                    LdrLog("NFC Open success!");
                }
                else
                {
                    LdrLog("NFC Open failed!");
                }
            }
        }
        private void Modbus_ConnectStateChanged(object sender, string e)
        {
            LdrLog("PLC连接：" + e);
            if (e == "Connected")
            {
                PlcState.Fill = new SolidColorBrush(Colors.Green);
            }
            else
            {
                PlcState.Fill = new SolidColorBrush(Colors.Red);
            }
        }
        private async void ReadA()
        {
            await Task.Run((() =>
            {
                while (true)
                {
                    try
                    {
                        if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
                        {
                            string strTmp = "\r\n ";
                            byte[] buf = new byte[256];
                            byte[] snr = new byte[128];
                            byte mode, blkStart, blkNum = 0;
                            int deviceAddr = CPublic.HexStringToInt("00");    //获得设备地址
                            if (deviceAddr < 0 || deviceAddr > 255)
                            {
                                LdrLog("Device address must between 0X00-0XFF!");
                                continue;
                            }

                            mode = (byte)0;
                            blkNum = (byte)1;
                            blkStart = (byte)0;
                            strTmp = CPublic.StrToHexStr("FF FF FF FF FF FF");
                            snr = CPublic.CharToByte(strTmp);                           //获得卡号

                            int ret = reader.MF_Read(deviceAddr, mode, blkStart, blkNum, ref snr[0], ref buf[0]);

                            if (0 == ret)
                            {
                                strTmp = "";
                                LdrLog("SN of card is:");

                                int count = 0;
                                while (snr[count++] != 0x00) ;
                                count--;

                                if (count == 4)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        strTmp += string.Format("{0:X2} ", snr[i]);
                                    }
                                }
                                else if (count == 7)
                                {
                                    for (int i = 0; i < 7; i++)
                                    {
                                        strTmp += string.Format("{0:X2} ", snr[i]);
                                    }
                                }

                                LdrLog(strTmp);

                                //匹配用户
                                var user = MainWindowViewModel.AllUser.FirstOrDefault(x => x.ID == strTmp);
                                if (user != null)
                                {
                                    LdrLog("扫到人员：" + user.Name);

                                    MessageBox.Show("扫到人员：" + user.Name, "提示", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    PLCManager.XinjiePLC.ModbusWrite(1, 15, 100, new[] { 1 });
                                    var a = PLCManager.XinjiePLC.ModbusRead(1, 3, 0, 1);
                                    if (LastUser == null)
                                    {
                                        LastUser = user;
                                        LdrLog("第一次上机，不保存");
                                    }
                                    else
                                    {
                                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory+"产量统计"))
                                        {
                                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "产量统计");
                                        }
                                        string path = AppDomain.CurrentDomain.BaseDirectory + $"产量统计\\{DateTime.Now.ToString("yyyy-MM-dd")}.csv";
                                        if (!File.Exists(path))
                                        {
                                            CSVFile.AddNewLine(path, new[] { "日期", "时间", "员工", "机台编号", "产量" });
                                        }
                                        //判断文件是否打开
                                        if (IsFileInUse(path))
                                        {
                                            LdrLog("文件被占用，请关闭文件后再试");
                                            MessageBox.Show("文件被占用，请关闭文件后再试");
                                            continue;
                                        }
                                        CSVFile.AddNewLine(path,
                                            new[]
                                            {
                                                DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(),
                                                LastUser.Name, Global.MachineID, a[0].ToString()
                                            });
                                        LdrLog("保存csv成功，文件位置：" + path);
                                        LastUser = user;
                                    }
                                }
                                else
                                {
                                    var res = MessageBox.Show("未查询到人员信息，卡号：" + strTmp + "\r\n" + "是否录入人员", "提示！", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
                                    if (res == MessageBoxResult.Yes)
                                    {
                                        //让用户输入字符
                                        var name = Microsoft.VisualBasic.Interaction.InputBox($"请输入卡号为{strTmp}的姓名", "录入人员", "", -1, -1);
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            //保存到csv文件
                                            string path = AppDomain.CurrentDomain.BaseDirectory + "user.csv";
                                            //判断是否有重名
                                            var isExist = MainWindowViewModel.AllUser.FirstOrDefault(x => x.Name == name);
                                            if (isExist != null)
                                            {
                                                MessageBox.Show("已存在该人员，请重新录入");
                                                continue;
                                            }
                                            CSVFile.AddNewLine(path, new[] { strTmp, name });
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                MainWindowViewModel.AllUser.Add(new User() { ID = strTmp, Name = name });
                                            });
                                            LdrLog($"添加人员{name},卡号{strTmp}");
                                            LdrLog("如需保存产量，请录入后再次刷卡，否则无效");
                                        }
                                    }
                                }
                            }
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception e)
                    {
                        LdrLog(e.Message);
                    }
                }
            }));
        }
        static bool IsFileInUse(string filePath)
        {
            try
            {
                using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // 文件未被占用
                    return false;
                }
            }
            catch (IOException)
            {
                // 文件被占用
                return true;
            }
        }
        CReader reader = new CReader();
        string LogHeader = " -> ";
        object LogLock = new object();
        int LogLine = 0;
        public async void LdrLog(string log)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                lock (LogLock)
                {
                    LogLine++;
                    string logstr = DateTime.Now.ToString() + LogHeader + log + Environment.NewLine;
                    TextBox.AppendText(logstr);
                    Global.SaveLog(logstr);
                    if (LogLine > 500)//最多500行。
                    {
                        LogLine = 1;
                        TextBox.Clear();
                    }
                }
            });

        }
        private double ScrollOffset = 0;
        private int SelectionStart = 0;
        private int SelectionLength = 0;
        private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollOffset = Scroll.VerticalOffset;
            if (!Scroll.IsMouseOver && SelectionLength == 0)
            {
                Scroll.ScrollToEnd();
                SelectionStart = 0;
                SelectionLength = 0;
            }
            else
            {
                if (SelectionLength == 0 && SelectionStart == 0)
                { }
                else
                    TextBox.Select(SelectionStart, SelectionLength);
                Scroll.ScrollToVerticalOffset(ScrollOffset);
            }
        }
        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox.ScrollToEnd();
        }
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (TextBox.SelectionStart == 0 && TextBox.SelectionLength == 0)
            {
            }
            else
            {
                SelectionStart = TextBox.SelectionStart;
                SelectionLength = TextBox.SelectionLength;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            userManager = new UserManager();
            userManager.ShowDialog();
        }
    }
}
