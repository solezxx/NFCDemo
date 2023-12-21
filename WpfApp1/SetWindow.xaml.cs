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
    }
}
