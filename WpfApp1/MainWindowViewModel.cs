﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

            for (int i = 0; i < MachineDatas.Count; i++)
            {
                if (DisplayCollection.Count <= i)
                {
                    DisplayCollection.Add(new DisplayData() { MachineName = MachineDatas[i].Name, Statistics = new ObservableCollection<LocalStatistic>() });
                }
                else
                {
                    DisplayCollection[i].MachineName = MachineDatas[i].Name;
                }
                path = Global.SavePath + $"{MachineDatas[i].Name}\\{DateTime.Now.ToString("yyyy-MM-dd")}.csv";
                if (File.Exists(path))
                {
                    var newAllLines = File.ReadAllLines(path, Encoding.GetEncoding("GB2312")).ToList();
                    if (newAllLines != null)
                    {
                        newAllLines.RemoveAt(0);

                        foreach (var line in newAllLines)
                        {
                            var items = line.Split(',');
                            var name = items[2];
                            var count1 = int.Parse(items[4]);
                            var yield = DisplayCollection[i].Statistics.FirstOrDefault(x => x.UserName == name);
                            if (yield == null)
                            {
                                DisplayCollection[i].Statistics.Add(new LocalStatistic() { UserName = name, UserCount = count1 });
                            }
                            else
                            {
                                yield.UserCount += count1;
                            }
                        }
                    }
                }
            }
        }

        private void PLCManager_PLCCountChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < DisplayCollection.Count; i++)

            {
                DisplayCollection[i].MachineCount = PLCManager.Count[i];
            }
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
        public static ObservableCollection<DisplayData> DisplayCollection { get; set; } = new ObservableCollection<DisplayData>();
        public static ObservableCollection<LocalStatistic> SearchList { get; set; } = new ObservableCollection<LocalStatistic>();
        public static ObservableCollection<MachineData> MachineDatas { get; set; } = new ObservableCollection<MachineData>();


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
    /// 产量统计
    /// </summary>
    public class LocalStatistic:ViewModelBase
    {
        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string _machineId;

        public string MachineId
        {
            get { return _machineId; }
            set
            {
                _machineId = value; 
                OnPropertyChanged(nameof(MachineId));
            }
        }

        private int _userCount;

        public int UserCount
        {
            get { return _userCount; }
            set
            {
                _userCount = value;
                OnPropertyChanged(nameof(UserCount));
            }
        }
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

    public class DisplayData : ViewModelBase
    {
        private string _machineName;

        public string MachineName
        {
            get { return _machineName; }
            set
            {
                _machineName = value;
                OnPropertyChanged(nameof(MachineName));
            }
        }

        private int _machineCount;

        public int MachineCount
        {
            get { return _machineCount; }
            set
            {
                _machineCount = value;
                OnPropertyChanged(nameof(MachineCount));
            }
        }
        public ObservableCollection<LocalStatistic> Statistics { get ; set; }
    }
}
