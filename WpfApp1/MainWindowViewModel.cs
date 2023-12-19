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

            //获取MachineDatas的名字并转成list
            List<string> machineNames = new List<string>();
            foreach (var machineData in MachineDatas)
            {
                machineNames.Add(machineData.Name);
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

    public class ProductionRecord : ViewModelBase
    {
        private string _employeeName;

        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EmployeeName
        {
            get { return _employeeName; }
            set
            {
                _employeeName = value;
                OnPropertyChanged(nameof(EmployeeName));
            }
        }

        private int[] _machineCount;

        public int[] MachineCount
        {
            get { return _machineCount; }
            set
            {
                _machineCount = value;
                OnPropertyChanged(nameof(MachineCount));
            }
        }

        private int _date;

        /// <summary>
        /// 日期
        /// </summary>
        public int Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private double _duration;

        /// <summary>
        /// 统计时长，单位小时
        /// </summary>
        public double Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }
    }
}
