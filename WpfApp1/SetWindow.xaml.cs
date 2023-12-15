﻿using System;
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
        }

        private void Save_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> list = new List<int>();
                for (int i = 0; i < MainWindowViewModel.MachineDatas.Count; i++)
                {
                    list.Add(int.Parse(Global.TimeOut) * 10);
                }
                PLCManager.XinjiePLC.ModbusWrite(1, 16, 100, list.ToArray());
                //保存到json文件
                string json = JsonConvert.SerializeObject(MainWindowViewModel.MachineDatas);
                System.IO.File.WriteAllText("MachineData.json", json);
                MessageBox.Show("保存成功！");
            }
            catch (Exception exception)
            {
                MessageBox.Show("保存失败：" + exception.Message);
            }
        }

        private void Add_click(object sender, RoutedEventArgs e)
        {
            //MainWindowViewModel.MachineDatas.Clear();
            MainWindowViewModel.MachineDatas.Add(new MachineData()
            {
                Name = "输入名字",
                COM = "COM100",
                CT = 1,
                Open = true
            });
        }

        private void Delete_click(object sender, RoutedEventArgs e)
        {
            if (MainWindowViewModel.MachineDatas!=null)
            {
                MainWindowViewModel.MachineDatas.RemoveAt(DataGrid.SelectedIndex);
            }
        
        }
    }
}
