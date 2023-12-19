using Demo;
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
using System.Reflection;
using System.Windows.Controls.Primitives;

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
            ColumnIni();
            FirstStartReadCsv();
        }

        public void FirstStartReadCsv()
        {
            try
            {
                MainWindowViewModel.ProductionRecords.Clear();
                //查找路径下的文件夹的数量
                var dirs = Directory.GetDirectories(Global.SavePath);
                foreach (var dir in dirs)
                {
                    var files = Directory.GetFiles(dir, "*.csv");
                    foreach (var file in files)
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        var fileDateTime = DateTime.ParseExact(fileName, "yyyy-MM-dd", null);
                        if (fileDateTime.Month == DateTime.Now.Month)
                        {
                            var day = fileDateTime.Day;
                            var allLines = File.ReadAllLines(file, Encoding.GetEncoding("GB2312")).ToList();
                            allLines.RemoveAt(0);
                            foreach (var line in allLines)
                            {
                                var items = line.Split(',');
                                var name = items[2];
                                var machineName = items[3];
                                var count = int.Parse(items[4]);
                                var yield = MainWindowViewModel.ProductionRecords.FirstOrDefault(x => (x.EmployeeName == name && x.Date == day));
                                if (yield == null)
                                {
                                    int indexMachineData = MainWindowViewModel.MachineDatas.IndexOf(MainWindowViewModel.MachineDatas.FirstOrDefault(x => x.Name == machineName));
                                    var temp = new ProductionRecord()
                                    {
                                        Date = day,
                                        MachineCount = new int[MainWindowViewModel.MachineDatas.Count],
                                        EmployeeName = name,
                                    };
                                    temp.MachineCount[indexMachineData] = count;
                                    double duration = 0;
                                    for (int i = 0; i < temp.MachineCount.Length; i++)
                                    {
                                        duration += temp.MachineCount[i] * MainWindowViewModel.MachineDatas[i].CT;
                                    }
                                    temp.Duration = Math.Round(duration / 3600, 2);
                                    MainWindowViewModel.ProductionRecords.Add(temp);
                                }
                                else
                                {
                                    int indexMachineData = MainWindowViewModel.MachineDatas.IndexOf(MainWindowViewModel.MachineDatas.FirstOrDefault(x => x.Name == machineName));
                                    yield.MachineCount[indexMachineData] += count;
                                    double duration = 0;
                                    for (int i = 0; i < yield.MachineCount.Length; i++)
                                    {
                                        duration += yield.MachineCount[i] * MainWindowViewModel.MachineDatas[i].CT;
                                    }
                                    yield.Duration = Math.Round(duration / 3600, 2);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LdrLog(e.Message);
            }
        }

        /// <summary>
        /// 初始化DataGrid的列
        /// </summary>
        public void ColumnIni()
        {
            DataGrid.Columns.Clear();
            DataGrid.ItemsSource = MainWindowViewModel.ProductionRecords;
            DataGridTextColumn column;
            column = new DataGridTextColumn()
            {
                Header = "日期",
                Binding = new Binding("Date")
            };
            DataGrid.Columns.Add(column);
            foreach (var machineData in MainWindowViewModel.MachineDatas)
            {
                var index = MainWindowViewModel.MachineDatas.IndexOf(machineData);
                column = new DataGridTextColumn()
                {
                    Header = machineData.Name,
                    Binding = new Binding($"MachineCount[{index}]")
                };
                DataGrid.Columns.Add(column);
            }
            column = new DataGridTextColumn()
            {
                Header = "合计(小时)",
                Binding = new Binding("Duration")
            };
            DataGrid.Columns.Add(column);

            column = new DataGridTextColumn()
            {
                Header = "员工",
                Binding = new Binding("EmployeeName")
            };
            DataGrid.Columns.Add(column);
        }

        // 获取 DataGrid 的单元格
        private DataGridCell GetCell(DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            var rowContainer = GetRow(dataGrid, rowIndex);

            if (rowContainer != null)
            {
                var presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                if (presenter != null)
                {
                    // 获取指定列的单元格
                    return (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                }
            }

            return null;
        }

        // 获取 DataGrid 的行
        private DataGridRow GetRow(DataGrid dataGrid, int index)
        {
            var rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);

            if (rowContainer == null)
            {
                // 如果行尚未生成，请手动刷新
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
                rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }

            return rowContainer;
        }

        // 获取 VisualChild
        private T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                var visual = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = visual as T;
                if (child != null)
                {
                    break;
                }
                else
                {
                    child = GetVisualChild<T>(visual);
                    if (child != null)
                    {
                        break;
                    }
                }
            }

            return child;
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
                    LdrLog($"{MainWindowViewModel.MachineDatas[e].Name}未刷卡但提示保存");
                    PLCManager.XinjiePLC.ModbusWrite(1, 15, 200 + e, new[] { 1 });
                    return;
                }

                if (PLCManager.Count[e]!=0)//产量为0不保存也不刷新
                {
                    CSVFile.AddNewLine(path,
                        new[]
                        {
                            DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(),
                            LastUser[e].Name,MainWindowViewModel.MachineDatas[e].Name, PLCManager.Count[e].ToString()
                        });
                    ReadCsvAndRefresh(path);
                }
                LdrLog($"{LastUser[e]}自动下机");
                LastUser[e] = null;
                PLCManager.XinjiePLC.ModbusWrite(1, 15, 200 + e, new[] { 1 });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void ReadCsvAndRefresh(string path)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                    var fileDateTime = DateTime.ParseExact(fileName, "yyyy-MM-dd", null);
                    if (fileDateTime.Month == DateTime.Now.Month)
                    {
                        var day = fileDateTime.Day;
                        //获取csv中的最后一行
                        var lastLine = File.ReadLines(path).Last();
                        var items = lastLine.Split(',');
                        var name = items[2];
                        var machineName = items[3];
                        var count = int.Parse(items[4]);
                        var yield = MainWindowViewModel.ProductionRecords.FirstOrDefault(x => (x.EmployeeName == name && x.Date == day));
                        if (yield == null)
                        {
                            int indexMachineData = MainWindowViewModel.MachineDatas.IndexOf(MainWindowViewModel.MachineDatas.FirstOrDefault(x => x.Name == machineName));
                            var temp = new ProductionRecord()
                            {
                                Date = day,
                                MachineCount = new int[MainWindowViewModel.MachineDatas.Count],
                                EmployeeName = name,
                            };
                            temp.MachineCount[indexMachineData] = count;
                            double duration = 0;
                            for (int i = 0; i < temp.MachineCount.Length; i++)
                            {
                                duration += temp.MachineCount[i] * MainWindowViewModel.MachineDatas[i].CT;
                            }
                            temp.Duration = duration;
                            MainWindowViewModel.ProductionRecords.Add(temp);
                        }
                        else
                        {
                            int indexMachineData = MainWindowViewModel.MachineDatas.IndexOf(MainWindowViewModel.MachineDatas.FirstOrDefault(x => x.Name == machineName));
                            yield.MachineCount[indexMachineData] = count;
                            double duration = 0;
                            for (int i = 0; i < yield.MachineCount.Length; i++)
                            {
                                duration += yield.MachineCount[i] * MainWindowViewModel.MachineDatas[i].CT;
                            }
                            yield.Duration = duration;
                        }
                    }
                }));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
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
                                        if (LastUser[machineIndex] == null)//如果上一个人结束了
                                        {
                                            LastUser[machineIndex] = user;
                                        }
                                        else//上一个人没结束
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
                                            if (PLCManager.Count[machineIndex]!=0)
                                            {
                                                CSVFile.AddNewLine(path,
                                                    new[]
                                                    {
                                                        DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(),
                                                        LastUser[machineIndex].Name, Global.MachineID,
                                                        PLCManager.Count[machineIndex].ToString()
                                                    });
                                                ReadCsvAndRefresh(path);
                                            }
                                            LdrLog($"{LastUser[machineIndex]}下机，{user}上机");
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
                            else
                            {
                                //断开重连
                                int ret = reader[machineIndex].OpenComm(Convert.ToInt32(MainWindowViewModel.MachineDatas[machineIndex].COM.Replace("COM", "")), 9600);
                                if (0 == ret)
                                {
                                    LdrLog("Restart NFC " + MainWindowViewModel.MachineDatas[machineIndex].Name);
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

        private void test_click(object sender, RoutedEventArgs e)
        {

        }
    }
}
