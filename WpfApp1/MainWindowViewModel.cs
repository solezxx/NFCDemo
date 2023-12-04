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

namespace WpfApp1
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
            string path= AppDomain.CurrentDomain.BaseDirectory + "user.csv";
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

        public static ObservableCollection<User> AllUser{ get; set; } = new ObservableCollection<User>();
    }

    public class User
    {
        public string Name { get; set; }
        public string ID { get; set; }
    }
}
