﻿using Demo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            PLCManager.PLCStopChanged += PLCManager_PLCStopChanged;
            Init();
        }
        public void Init()
        {
            for (int i = 0; i < MainWindowViewModel.MachineDatas.Count; i++)
            {
                if (MainWindowViewModel.MachineDatas[i].Open)
                {
                    reader[i] = new CReader();
                    OpenCom(i);
                }
            }

            try
            {
                MainWindowViewModel.ProductionRecords.Clear();
                //查找路径下的文件夹的数量
                var dirs = Directory.GetDirectories(Global.SavePath);
                for (int i = 0; i < dirs.Length; i++)
                {
                    var files = Directory.GetFiles(dirs[i], "*.csv");
                    for (int j = 0; j < files.Length; j++)
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(files[j]);
                        var fileDateTime = DateTime.ParseExact(fileName, "yyyy-MM-dd", null);

                        if (fileDateTime.Month == DateTime.Now.Month)
                        {
                            var day = fileDateTime.Day;
                            var allLines = File.ReadAllLines(files[j], Encoding.GetEncoding("GB2312")).ToList();
                            allLines.RemoveAt(0);
                            foreach (var line in allLines)
                            {
                                var items = line.Split(',');
                                var name = items[2];
                                var machineName = items[3];
                                var count1 = double.Parse(items[4]);
                                var yield = MainWindowViewModel.ProductionRecords.FirstOrDefault(x => (x.EmployeeName == name && x.MachineName == machineName));
                                if (yield == null)
                                {
                                    MainWindowViewModel.ProductionRecords.Add(new ProductionRecord() { EmployeeName = name, MachineName = machineName });
                                    //找到属性名为day+count1的属性并赋值
                                    var property = MainWindowViewModel.ProductionRecords.FirstOrDefault(x => x.EmployeeName == name && x.MachineName == machineName).GetType().GetProperty("day" + day);
                                    property.SetValue(MainWindowViewModel.ProductionRecords.FirstOrDefault(x => x.EmployeeName == name && x.MachineName == machineName), count1);
                                }
                                else
                                {
                                    var property = MainWindowViewModel.ProductionRecords.FirstOrDefault(x => x.EmployeeName == name && x.MachineName == machineName).GetType().GetProperty("day" + day);
                                    var value = (double)property.GetValue(MainWindowViewModel.ProductionRecords.FirstOrDefault(x => x.EmployeeName == name && x.MachineName == machineName));
                                    property.SetValue(MainWindowViewModel.ProductionRecords.FirstOrDefault(x => x.EmployeeName == name && x.MachineName == machineName), value + count1);
                                }
                            }
                        }
                    }
                }
                //最后一行统计时间
                var lastLine = new ProductionRecord() { EmployeeName = "当日时间", MachineName = "合计" };
                for (int i = 1; i <= DateTime.Now.Day; i++)
                {
                    var property = lastLine.GetType().GetProperty("day" + i);
                    double value = 0;
                    //数量*MachineData的CT
                    foreach (var productionRecord in MainWindowViewModel.ProductionRecords)
                    {
                        var property1 = productionRecord.GetType().GetProperty("day" + i);
                        var value1 = (double)property1.GetValue(productionRecord);
                        value += value1 * MainWindowViewModel.MachineDatas.FirstOrDefault(x => x.Name == productionRecord.MachineName).CT;
                    }
                    property.SetValue(lastLine, value);
                }
                MainWindowViewModel.ProductionRecords.Add(lastLine);
            }
            catch (Exception e)
            {
               LdrLog(e.Message);
            }
        }

        private void PLCManager_PLCStopChanged(object sender, int e)
        {
            try
            {
                if (!Directory.Exists(Global.SavePath + $"{MainWindowViewModel.MachineDatas[e].Name}"))
                {
                    Directory.CreateDirectory(Global.SavePath + $"{MainWindowViewModel.MachineDatas[e].Name}");
                }
                string path = Global.SavePath + $"{MainWindowViewModel.MachineDatas[e].Name}\\{DateTime.Now.ToString("yyyy-MM-dd")}.csv";
                if (!File.Exists(path))
                {
                    CSVFile.AddNewLine(path, new[] { "日期", "时间", "员工", "机台编号", "产量" });
                }
                ////判断文件是否打开
                //if (IsFileInUse(path))
                //{
                //    LdrLog("文件被占用，请关闭文件后再试");
                //    MessageBox.Show("文件被占用，请关闭文件后再试");
                //    return;
                //}

                if (LastUser[e] == null)
                {
                    LdrLog("未刷卡");
                    PLCManager.XinjiePLC.ModbusWrite(1, 15, 200 + e, new[] { 1 });
                    return;
                }
                CSVFile.AddNewLine(path,
                    new[]
                    {
                    DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(),
                    LastUser[e].Name,MainWindowViewModel.MachineDatas[e].Name, PLCManager.Count[e].ToString()
                    });
                LdrLog("保存csv成功，文件位置：" + path);
                LastUser[e] = null;

                PLCManager.XinjiePLC.ModbusWrite(1, 15, 200 + e, new[] { 1 });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
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
        SetWindow setWindow;
        MainWindowViewModel viewModel;

        User[] LastUser = new User[20];
        public void OpenCom(int index)
        {
            if (IntPtr.Zero == reader[index].GetHComm())                   //判断串口是否打开
            {
                int ret = reader[index].OpenComm(Convert.ToInt32(MainWindowViewModel.MachineDatas[index].COM.Replace("COM", "")), 9600);
                if (0 == ret)
                {
                    LdrLog($"NFC {MainWindowViewModel.MachineDatas[index].Name} Open success!");
                    ReadNfc(index);
                }
                else
                {
                    LdrLog($"NFC {MainWindowViewModel.MachineDatas[index].Name} Open failed!");
                    MessageBox.Show($"NFC {MainWindowViewModel.MachineDatas[index].Name} 打开失败!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        object lockObj = new object();

        private bool[] isRead = new bool[20];
        private async void ReadNfc(int machineIndex)
        {
            var machineName = MainWindowViewModel.MachineDatas[machineIndex].Name;
            await Task.Run((() =>
            {
                while (true)
                {
                    try
                    {
                        lock (lockObj)
                        {
                            if (MainWindowViewModel.cts.Token.IsCancellationRequested)
                            {
                                MainWindowViewModel.cts.Token.ThrowIfCancellationRequested();
                            }
                            if (IntPtr.Zero != reader[machineIndex].GetHComm()) //判断串口是否打开
                            {
                                string strTmp = "\r\n ";
                                byte[] buf = new byte[256];
                                byte[] snr = new byte[128];
                                byte mode, blkStart, blkNum = 0;
                                int deviceAddr = CPublic.HexStringToInt("00"); //获得设备地址

                                mode = (byte)0;
                                blkNum = (byte)1;
                                blkStart = (byte)0;
                                strTmp = CPublic.StrToHexStr("FF FF FF FF FF FF");
                                snr = CPublic.CharToByte(strTmp); //获得卡号

                                int ret = reader[machineIndex].MF_Read(deviceAddr, mode, blkStart, blkNum, ref snr[0], ref buf[0]);

                                if (0 == ret)
                                {
                                    if (isRead[machineIndex] != false)
                                    {
                                        continue;
                                    }
                                    isRead[machineIndex] = true;
                                    reader[machineIndex].ControlBuzzer(deviceAddr, (byte)10, (byte)1, ref buf[0]);
                                    strTmp = "";
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

                                    LdrLog("SN of card is:" + strTmp);

                                    //匹配用户
                                    var user = MainWindowViewModel.AllUser.FirstOrDefault(x => x.ID == strTmp);
                                    if (user != null)
                                    {
                                        LdrLog($"{machineName}扫到人员：" + user.Name);
                                        if (LastUser[machineIndex] == null)
                                        {
                                            LastUser[machineIndex] = user;
                                        }
                                        else
                                        {
                                            if (!Directory.Exists(Global.SavePath + machineName))
                                            {
                                                Directory.CreateDirectory(Global.SavePath + machineName);
                                            }

                                            string path = Global.SavePath + machineName +
                                                          $"\\{DateTime.Now.ToString("yyyy-MM-dd")}.csv";
                                            if (!File.Exists(path))
                                            {
                                                CSVFile.AddNewLine(path, new[] { "日期", "时间", "员工", "机台编号", "产量" });
                                            }

                                            ////判断文件是否打开
                                            //if (IsFileInUse(path))
                                            //{
                                            //    LdrLog("文件被占用，请关闭文件后再试");
                                            //    MessageBox.Show("文件被占用，请关闭文件后再试");
                                            //    continue;
                                            //}

                                            CSVFile.AddNewLine(path,
                                                new[]
                                                {
                                                    DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(),
                                                    LastUser[machineIndex].Name, Global.MachineID,
                                                    PLCManager.Count[machineIndex].ToString()
                                                });
                                            LdrLog("保存csv成功，文件位置：" + path);
                                            LastUser[machineIndex] = user;

                                        }

                                        //只要扫到有人员就给信号
                                        PLCManager.XinjiePLC.ModbusWrite(1, 15, 200 + machineIndex, new[] { 1 });
                                        Thread.Sleep(300);
                                        PLCManager.XinjiePLC.ModbusWrite(1, 15, 100 + machineIndex, new[] { 1 });
                                    }
                                    else
                                    {
                                        var res = MessageBox.Show("未查询到人员信息，卡号：" + strTmp + "\r\n" + "是否录入人员", "提示！",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None,
                                            MessageBoxOptions.DefaultDesktopOnly);
                                        if (res == MessageBoxResult.Yes)
                                        {
                                            //让用户输入字符
                                            var name = Microsoft.VisualBasic.Interaction.InputBox($"请输入卡号为{strTmp}的姓名",
                                                "录入人员", "", -1, -1);
                                            if (!string.IsNullOrEmpty(name))
                                            {
                                                //保存到csv文件
                                                string path = AppDomain.CurrentDomain.BaseDirectory + "user.csv";
                                                //判断是否有重名
                                                var isExist =
                                                    MainWindowViewModel.AllUser.FirstOrDefault(x => x.Name == name);
                                                if (isExist != null)
                                                {
                                                    MessageBox.Show("已存在该人员，请重新录入");
                                                    continue;
                                                }

                                                CSVFile.AddNewLine(path, new[] { strTmp, name });
                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                    MainWindowViewModel.AllUser.Add(new User()
                                                    { ID = strTmp, Name = name });
                                                });
                                                LdrLog($"添加人员{name},卡号{strTmp},如需保存产量，请重新刷卡，否则无效！");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    isRead[machineIndex] = false;
                                }
                            }
                            Thread.Sleep(10); //轮询10ms
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is OperationCanceledException)
                        {
                            return;
                        }
                        else
                        {
                            LdrLog(e.Message);
                        }
                    }
                }
            }), MainWindowViewModel.cts.Token);
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

        private CReader[] reader = new CReader[20];
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

        private void Set_Btn_Click(object sender, RoutedEventArgs e)
        {
            setWindow = new SetWindow();
            setWindow.ShowDialog();
        }

        private int keycount = 0;
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V)
            {
                keycount++;
            }
            else
            {
                keycount = 0;
            }

            if (keycount == 5)
            {
                keycount = 0;
                if (Button_Admin1.Visibility == Visibility.Collapsed)
                {
                    Button_Admin1.Visibility = Visibility.Visible;
                    Button_Admin2.Visibility = Visibility.Visible;
                }
                else
                {
                    Button_Admin1.Visibility = Visibility.Collapsed;
                    Button_Admin2.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
