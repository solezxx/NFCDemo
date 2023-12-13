using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DXH.ViewModel;

namespace NFCDemo
{
    public class MainWindowViewModel : ViewModelBase
    {
        public int[] Count
        {
            get
            {
                return PLCManager.Count;
            }
        }

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
                            var yield = DisplayCollection[i].FirstOrDefault(x => x.UserName == name);
                            if (yield == null)
                            {
                                DisplayCollection[i].Add(new LocalStatistic() { UserName = name,UserCount = count1 });
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
            OnPropertyChanged(nameof(Count));
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
        public static ObservableCollection<ObservableCollection<LocalStatistic>> DisplayCollection { get; set; } = new ObservableCollection<ObservableCollection<LocalStatistic>>();
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
    public class LocalStatistic
    {
        public string UserName { get; set; }
        public string MachineId { get; set; }
        public int UserCount { get; set; }
    }

    /// <summary>
    /// 机台信息
    /// </summary>
    public class MachineData
    {
        public string Name { get; set; }
        public bool Open { get; set; }
        public string COM { get; set; }
    }
}
