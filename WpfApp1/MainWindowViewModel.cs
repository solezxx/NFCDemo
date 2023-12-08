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
        public int Count
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

            path= AppDomain.CurrentDomain.BaseDirectory + $"产量统计\\{DateTime.Now.ToString("yyyy-MM-dd")}.csv";
            if (File.Exists(path))
            {
                var newAllLines = File.ReadAllLines(path, Encoding.GetEncoding("GB2312")).ToList();
                if (newAllLines != null)
                {
                    newAllLines.RemoveAt(0);
                    var newAllYield = new List<Yield>();
                    foreach (var line in newAllLines)
                    {
                        var items = line.Split(',');
                        var name = items[2];
                        var count1 = int.Parse(items[4]);
                        var yield = newAllYield.FirstOrDefault(x => x.Name == name);
                        if (yield == null)
                        {
                            newAllYield.Add(new Yield() { Name = name, Count = count1 });
                        }
                        else
                        {
                            yield.Count += count1;
                        }
                    }

                    AllYield.Clear();
                    foreach (var yield in newAllYield)
                    {
                        AllYield.Add(yield);
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
        public static ObservableCollection<Yield> AllYield { get; set; } = new ObservableCollection<Yield>();
        public static ObservableCollection<LocalStatistic> SearchList { get; set; } = new ObservableCollection<LocalStatistic>();


        public string TimeOut
        {
            get => Global.TimeOut;
            set
            {
                Global.TimeOut= value;
            }
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string ID { get; set; }
    }

    public class Yield
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class LocalStatistic
    {
        public string Name { get; set; }
        public string MachineId { get; set; }
        public int Count { get; set; }

    }
}
