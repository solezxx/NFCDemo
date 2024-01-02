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
using DXH.ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
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
            ReadLastUser();
            PLCManager.Initialize();
            PLCManager.XinjiePLC.ModbusStateChanged += Modbus_ConnectStateChanged;
            PLCManager.PLCStopChanged += PLCManager_PLCStopChanged;
            Init();
        }

        public List<string> ConnectedNameList = new List<string>();
        List<string> findNameList = new List<string>();
        public void Init()
        {
            try
            {
                findNameList.Clear();
                ConnectedNameList.Clear();
                var buf = new byte[256];
                //获取电脑的所有COM口
                var coms = System.IO.Ports.SerialPort.GetPortNames();
                CReader[] temp = new CReader[coms.Length];
                //尝试连接所有的COM口
                for (int i = 0; i < coms.Length; i++)
                {
                    temp[i] = new CReader();
                    if (IntPtr.Zero != temp[i].GetHComm() || coms[i] == Global.ModbusRTU_COM)//如果已经打开或者遍历到了PLC的串口，跳过
                        continue;
                    int ret = temp[i].OpenComm(Convert.ToInt32(coms[i].Replace("COM", "")), 9600);
                    if (0 == ret)
                    {
                        var getNumRet = temp[i].GetSerNum(0x00, ref buf[0]);
                        //如果有返回说明是读卡器
                        if (0 == getNumRet || 1 == getNumRet)
                        {
                            var strTmp = "";
                            for (int j = 0; j < 8; j++)
                            {
                                strTmp += buf[j + 1].ToString("X2") + " ";
                            }
                            LdrLog($"{coms[i]}序列号：{strTmp}");
                            //判断与配置中的序列号是否一致，一致则开始连接
                            foreach (var machineData in MainWindowViewModel.MachineDatas)
                            {
                                if (machineData.SerNum.Replace(" ", "") == strTmp.Replace(" ", ""))
                                {
                                    machineData.COM = coms[i];
                                    temp[i].CloseComm();//关闭temp实例的端口，给真实的实例打开端口
                                    findNameList.Add(machineData.Name);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //没反应说明可能不是读卡器，断开连接
                            temp[i].CloseComm();
                        }
                    }
                }

                for (int j = 0; j < findNameList.Count; j++)
                {
                    OpenCom(MainWindowViewModel.MachineDatas.IndexOf(MainWindowViewModel.MachineDatas.FirstOrDefault(x => x.Name == findNameList[j] && x.Open)));
                }

                //判断连接数量是否与配置中Open的数量一致
                if (ConnectedNameList.Count != MainWindowViewModel.MachineDatas.Count(x => x.Open))
                {
                    //弹窗显示哪几个没连接
                    string str = "";

                    foreach (var machineData in MainWindowViewModel.MachineDatas.Where(x => x.Open))
                    {
                        if (!ConnectedNameList.Contains(machineData.Name))
                        {
                            str += machineData.Name + " ";
                        }
                    }
                    LdrLog(str + "连接失败！");
                    MessageBox.Show(str + "连接失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                ColumnIni();
                FirstStartReadCsv();
                SetTodayRowColor();
            }
            catch (Exception e)
            {
                LdrLog("程序初始化报错：" + e.Message);
            }
        }

        public void ReadLastUser()
        {
            try
            {
                //读取Global中保存的LastUsers，初始化LastUser
                var lastUsers = Global.LastUsers.Split(',');
                if (lastUsers.Length < MainWindowViewModel.MachineDatas.Count)
                {
                    return;
                }
                for (int i = 0; i < MainWindowViewModel.MachineDatas.Count; i++)
                {
                    if (!string.IsNullOrEmpty(lastUsers[i]))
                    {
                        LastUser[i] = MainWindowViewModel.AllUser.FirstOrDefault(x => x.Name == lastUsers[i]);
                    }
                }
            }
            catch (Exception e)
            {
                LdrLog("读取上次刷卡人员报错：" + e.Message);
            }
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
                    var machineName = dir.Replace(Global.SavePath, "");
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
                //按日期排序
                var temp2 = MainWindowViewModel.ProductionRecords.OrderBy(x => x.Date).ToList();
                MainWindowViewModel.ProductionRecords.Clear();
                foreach (var productionRecord in temp2)
                {
                    MainWindowViewModel.ProductionRecords.Add(productionRecord);
                }

                if (DataGrid.Items.Count>0)
                {
                    //DataGrid滚动到最后一行
                    DataGrid.ScrollIntoView(DataGrid.Items[DataGrid.Items.Count - 1]);
                }
                //创建柱状图
                SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "实际工时",
                        Values = new ChartValues<double>(),
                        DataLabels = true,
                        MaxColumnWidth = 30
                    }
                };

                //adding series will update and animate the chart automatically
                SeriesCollection.Add(new ColumnSeries
                {
                    Title = "理论工时",
                    Values = new ChartValues<double>(),
                    DataLabels = true,
                    MaxColumnWidth = 30
                });

                //把每个人的工时统计相加生成一个新的集合
                var tempDuration = MainWindowViewModel.ProductionRecords.GroupBy(x => x.EmployeeName).Select(x => new ProductionRecord()
                {
                    EmployeeName = x.Key,
                    MachineCount = new int[MainWindowViewModel.MachineDatas.Count],
                    Duration = x.Sum(y => y.Duration),
                }).ToList();
                foreach (var productionRecord in tempDuration)
                {
                    SeriesCollection[0].Values.Add(productionRecord.Duration);
                    SeriesCollection[1].Values.Add(10.5 * MainWindowViewModel.ProductionRecords.Count(x => x.EmployeeName == productionRecord.EmployeeName));
                }
                Labels.Clear();
                foreach (var productionRecord in tempDuration)
                {
                    Labels.Add(productionRecord.EmployeeName);
                }
                Formatter = value => value.ToString("N");

                DataContext = this;
            }
            catch (Exception e)
            {
                LdrLog("程序初始化读取文件报错：" + e.Message);
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
                Binding = new Binding("Date"),
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
        private void SetTodayRowColor()
        {
            DataGrid.Items.Refresh();
            for (int i = 0; i < DataGrid.Items.Count; i++)
            {
                var item = DataGrid.Items[i] as ProductionRecord;
                if (item.Date == DateTime.Now.Day)
                {
                    var row = GetRow(DataGrid, i);
                    if (row != null)
                    {
                        row.Background = new SolidColorBrush(Colors.LightGreen);
                    }
                }
            }
        }
        // 获取 DataGrid 的行
        private DataGridRow GetRow(DataGrid dataGrid, int index)
        {
            var rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);

            if (rowContainer == null)
            {
                // 如果行尚未生成，请手动刷新
                dataGrid.UpdateLayout();
                //dataGrid.ScrollIntoView(dataGrid.Items[index]);
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

                if (PLCManager.Count[e] != 0)//产量为0不保存也不刷新
                {
                    CSVFile.AddNewLine(path,
                        new[]
                        {
                            DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(),
                            LastUser[e].Name,MainWindowViewModel.MachineDatas[e].Name, PLCManager.Count[e].ToString()
                        });
                    ReadCsvAndRefresh(path);
                }
                LdrLog($"{LastUser[e].Name}自动下机");
                LastUser[e] = null;
                //将最后一次刷卡的人员保存到文件
                string lastusers = "";
                foreach (var u in LastUser)
                {
                    lastusers += (u == null ? "" : u.Name) + ",";
                }
                Global.LastUsers = lastusers;
                PLCManager.XinjiePLC.ModbusWrite(1, 15, 200 + e, new[] { 1 });
            }
            catch (Exception exception)
            {
                MessageBox.Show("PLC给结束信号报错：" + exception.Message);
            }
        }

        public void ReadCsvAndRefresh(string path)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var machineName = path.Replace(Global.SavePath, "").Replace($"\\{DateTime.Now.ToString("yyyy-MM-dd")}.csv", "");
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                    var fileDateTime = DateTime.ParseExact(fileName, "yyyy-MM-dd", null);
                    if (fileDateTime.Month == DateTime.Now.Month)
                    {
                        var day = fileDateTime.Day;
                        //获取csv中的最后一行
                        var lastLine = File.ReadLines(path, Encoding.GetEncoding("GB2312")).Last();
                        var items = lastLine.Split(',');
                        var name = items[2];
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
                            yield.MachineCount = yield.MachineCount;//写入才能触发set
                            double duration = 0;
                            for (int i = 0; i < yield.MachineCount.Length; i++)
                            {
                                duration += yield.MachineCount[i] * MainWindowViewModel.MachineDatas[i].CT;
                            }
                            yield.Duration = Math.Round(duration / 3600, 2);
                        }
                    }
                    //按日期排序
                    var temp2 = MainWindowViewModel.ProductionRecords.OrderBy(x => x.Date).ToList();
                    MainWindowViewModel.ProductionRecords.Clear();
                    foreach (var productionRecord in temp2)
                    {
                        MainWindowViewModel.ProductionRecords.Add(productionRecord);
                    }
                    //DataGrid滚动到最后一行
                    DataGrid.ScrollIntoView(DataGrid.Items[DataGrid.Items.Count - 1]);

                    //更新柱状图
                    SeriesCollection[0].Values.Clear();
                    SeriesCollection[1].Values.Clear();
                    //把每个人的工时统计相加生成一个新的集合
                    var tempDuration = MainWindowViewModel.ProductionRecords.GroupBy(x => x.EmployeeName).Select(x => new ProductionRecord()
                    {
                        EmployeeName = x.Key,
                        MachineCount = new int[MainWindowViewModel.MachineDatas.Count],
                        Duration = x.Sum(y => y.Duration),
                    }).ToList();

                    foreach (var productionRecord in tempDuration)
                    {
                        SeriesCollection[0].Values.Add(productionRecord.Duration);
                        SeriesCollection[1].Values.Add(10.5 * MainWindowViewModel.ProductionRecords.Count(x => x.EmployeeName == productionRecord.EmployeeName));
                    }
                    Labels.Clear();
                    foreach (var productionRecord in tempDuration)
                    {
                        Labels.Add(productionRecord.EmployeeName);
                    }
                    Formatter = value => value.ToString("N");
                    DataContext = this;
                    SetTodayRowColor();
                    MergeData();
                }));
            }
            catch (Exception e)
            {
                MessageBox.Show("读取csv刷新界面报错：" + e.Message);
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
            reader[index] = new CReader();
            if (IntPtr.Zero == reader[index].GetHComm())                   //判断串口是否打开
            {
                int ret = reader[index].OpenComm(Convert.ToInt32(MainWindowViewModel.MachineDatas[index].COM.Replace("COM", "")), 9600);
                if (0 == ret)
                {
                    LdrLog($"NFC {MainWindowViewModel.MachineDatas[index].Name} Open success!");
                    ReadNfc(index);
                    ConnectedNameList.Add(MainWindowViewModel.MachineDatas[index].Name);
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
                                    if (String.IsNullOrEmpty(strTmp))
                                        continue;
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
                                            if (PLCManager.Count[machineIndex] != 0)
                                            {
                                                CSVFile.AddNewLine(path,
                                                    new[]
                                                    {
                                                        DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(),
                                                        LastUser[machineIndex].Name, MainWindowViewModel.MachineDatas[machineIndex].Name,
                                                        PLCManager.Count[machineIndex].ToString()
                                                    });
                                                ReadCsvAndRefresh(path);
                                            }
                                            LdrLog($"{LastUser[machineIndex].Name}下机，{user.Name}上机");
                                            LastUser[machineIndex] = user;
                                        }
                                        //将最后一次刷卡的人员保存到文件
                                        string lastusers = "";
                                        foreach (var u in LastUser)
                                        {
                                            lastusers += (u == null ? "" : u.Name) + ",";
                                        }
                                        Global.LastUsers = lastusers;

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
                            //关闭串口
                            reader[machineIndex].CloseComm();
                            return;
                        }
                        else
                        {
                            LdrLog($"读取NFC {MainWindowViewModel.MachineDatas[machineIndex]}报错：" + e.Message);
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

        public SeriesCollection SeriesCollection { get; set; }
        public ObservableCollection<string> Labels { get; set; } = new ObservableCollection<string>();
        public Func<double, string> Formatter { get; set; }

        private void DataGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            // 查找DataGrid内部的ScrollViewer
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(DataGrid);

            // 注册ScrollChanged事件
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += DataGrid_ScrollChanged;
            }
        }
        private async void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            await Task.Run((() =>
            {
                Dispatcher.BeginInvoke(new Action((SetTodayRowColor)));
            }));
        }
        private T FindVisualChild<T>(DependencyObject visual) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(visual, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            try
            {
                MainWindowViewModel.Cancel();
            }
            catch (Exception exception)
            {
            }
        }
        object mergaData = new object();
        /// <summary>
        /// 保存DataGrid的数据到桌面
        /// </summary>
        public void MergeData()
        {
            try
            {
                lock (mergaData)
                {
                    //将DataGrid中的数据合并到一个文件，放到桌面
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{DateTime.Now.ToString("yyyy-MM")}.csv";
                    if (!File.Exists(path))
                    {
                        CSVFile.AddNewLine(path, new[] { "日期", "员工", "机台编号", "产量" });
                    }
                    else
                    {
                        File.Delete(path);
                        CSVFile.AddNewLine(path, new[] { "日期", "员工", "机台编号", "产量" });
                    }

                    foreach (var productionRecord in MainWindowViewModel.ProductionRecords)
                    {
                        for (int i = 0; i < MainWindowViewModel.MachineDatas.Count; i++)
                        {
                            if (productionRecord.MachineCount[i] != 0)
                            {
                                CSVFile.AddNewLine(path, new[]
                                {
                                productionRecord.Date.ToString(), productionRecord.EmployeeName,
                                MainWindowViewModel.MachineDatas[i].Name, productionRecord.MachineCount[i].ToString()
                            });
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LdrLog(exception.Message);
            }
        }
    }
}
