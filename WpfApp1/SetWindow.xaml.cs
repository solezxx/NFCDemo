using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Demo;
using Newtonsoft.Json;

namespace NFCDemo
{
    /// <summary>
    /// SetWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetWindow : Window
    {
        public SetWindow()
        {
            InitializeComponent();
            _mainWindow = Application.Current.MainWindow as MainWindow;
            //获取电脑的所有com口
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                ComboBox.Items.Add(port);
            }
        }

        private MainWindow _mainWindow;
        private async void Save_Btn_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run((() =>
            {
                try
                {
                    //发送超时时间
                    List<int> list = new List<int>();
                    for (int i = 0; i < MainWindowViewModel.MachineDatas.Count; i++)
                    {
                        list.Add((MainWindowViewModel.MachineDatas[i].MT) * 10);
                    }

                    if (PLCManager.XinjiePLC.ModbusState)
                    {
                        PLCManager.XinjiePLC.ModbusWrite(1, 16, 100, list.ToArray());
                    }

                    //保存到json文件
                    string json = JsonConvert.SerializeObject(MainWindowViewModel.MachineDatas);
                    System.IO.File.WriteAllText("MachineData.json", json);
                    MainWindowViewModel.Cancel();
                    MessageBox.Show("保存成功！");
                    Dispatcher.BeginInvoke(new Action((() =>
                    {
                        _mainWindow.Init();
                    })));
                }
                catch (Exception exception)
                {
                    MessageBox.Show("保存失败：" + exception.Message);
                }
            }));
        }

        private void Add_click(object sender, RoutedEventArgs e)
        {
            //MainWindowViewModel.MachineDatas.Clear();
            MainWindowViewModel.MachineDatas.Add(new MachineData()
            {
                Name = "输入名字",
                SerNum = "输入序列号",
                CT = 1,
                Open = true,
                MT = 30
            });
        }

        private void Delete_click(object sender, RoutedEventArgs e)
        {
            if (MainWindowViewModel.MachineDatas != null)
            {
                MainWindowViewModel.MachineDatas.RemoveAt(DataGrid.SelectedIndex);
            }
        }

        private CReader reader;
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectButton.Content.ToString()=="连接")
            { 
                reader = new CReader();
                var ret = reader.OpenComm(Convert.ToInt32(ComboBox.Text.Replace("COM", "")), 9600);
                if (ret == 0)//连接成功
                {
                    ConnectButton.Content = "已连接";
                }
                else
                {
                    MessageBox.Show("连接失败");
                }
            }
            else
            {
                reader.CloseComm();
                ConnectButton.Content = "连接";
            }
        }

        private void ReadSerNum(object sender, RoutedEventArgs e)
        {
            var buf = new byte[256];
            if (ConnectButton.Content.ToString()=="已连接")
            {
                var ret=reader.GetSerNum(0x00, ref buf[0]);
                if (0 == ret )
                {
                    var strTmp = "";
                    for (int j = 0; j < 8; j++)
                    {
                        strTmp += buf[j + 1].ToString("X2") + " ";
                    }

                    MessageBox.Show(strTmp);
                }
            }
        }

        private void SetSerNum_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
