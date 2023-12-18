using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DXH.ViewModel;

namespace NFCDemo
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "user.csv";
            AllUser.Clear();
            if (File.Exists(path))
            {
                var lines = System.IO.File.ReadAllLines(path, Encoding.GetEncoding("GB2312"));
                foreach (var line in lines)
                {
                    var items = line.Split(',');
                    AllUser.Add(new User() { Name = items[1], ID = items[0] });
                }
            }
            PLCManager.PLCCountChanged += PLCManager_PLCCountChanged;

            path = "MachineData.json";
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var machineDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MachineData>>(json);
                if (machineDatas != null)
                {
                    MachineDatas.Clear();
                    foreach (var machineData in machineDatas)
                    {
                        MachineDatas.Add(machineData);
                    }
                }
            }
        }

        private void PLCManager_PLCCountChanged(object sender, EventArgs e)
        {
            
        }
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        public static ObservableCollection<User> AllUser { get; set; } = new ObservableCollection<User>();
        public static ObservableCollection<MachineData> MachineDatas { get; set; } = new ObservableCollection<MachineData>();
        public static ObservableCollection<ProductionRecord> ProductionRecords { get; set; }=new ObservableCollection<ProductionRecord>();

        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static void Cancel()
        {
            cts.Cancel();
        }

        public string TimeOut
        {
            get => Global.TimeOut;
            set
            {
                Global.TimeOut = value;
            }
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string ID { get; set; }
    }
    /// <summary>
    /// 机台信息
    /// </summary>
    public class MachineData
    {
        public string Name { get; set; }
        public string COM { get; set; }
        /// <summary>
        /// 单pcs耗时，单位秒
        /// </summary>
        public double CT { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Open { get; set; }
    }

    public class ProductionRecord
    {
        public string EmployeeName { get; set; }
        public string MachineName { get; set; }
        //生成30天的数据
        public double day1 { get; set; }
        public double day2 { get; set; }
        public double day3 { get; set; }
        public double day4 { get; set; }
        public double day5 { get; set; }
        public double day6 { get; set; }
        public double day7 { get; set; }
        public double day8 { get; set; }
        public double day9 { get; set; }
        public double day10 { get; set; }
        public double day11 { get; set; }
        public double day12 { get; set; }
        public double day13 { get; set; }
        public double day14 { get; set; }
        public double day15 { get; set; }
        public double day16 { get; set; }
        public double day17 { get; set; }
        public double day18 { get; set; }
        public double day19 { get; set; }
        public double day20 { get; set; }
        public double day21 { get; set; }
        public double day22 { get; set; }
        public double day23 { get; set; }
        public double day24 { get; set; }
        public double day25 { get; set; }
        public double day26 { get; set; }
        public double day27 { get; set; }
        public double day28 { get; set; }
        public double day29 { get; set; }
        public double day30 { get; set;}
        public double day31 { get; set;}
    }
}
