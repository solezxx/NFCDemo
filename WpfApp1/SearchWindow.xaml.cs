using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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

namespace NFCDemo
{
    /// <summary>
    /// SearchWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string path=AppDomain.CurrentDomain.BaseDirectory+"产量统计";
            //读取文件夹中的所有csv文件
            DateTime startDateTime=StartTime.SelectedDate ?? DateTime.MinValue;
            DateTime endDateTime=EndTime.SelectedDate ?? DateTime.MaxValue;
            try
            {
                // 获取文件夹中的所有文件
                string[] allFiles = Directory.GetFiles(path, "*.csv");

                // 根据文件名日期筛选文件
                List<string> files = new List<string>();
                foreach (string file in allFiles)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    DateTime fileDateTime = DateTime.ParseExact(fileName, "yyyy-MM-dd", null);
                    if (fileDateTime >= startDateTime && fileDateTime <= endDateTime)
                    {
                        files.Add(file);
                    }
                }

                // 读取文件内容
                List<LocalStatistic> localStatistics = new List<LocalStatistic>();
                foreach (string file in files)
                {
                    var lines = File.ReadAllLines(file, Encoding.GetEncoding("GB2312")).ToList();
                    lines.RemoveAt(0);
                    foreach (string line in lines)
                    {
                        string[] items = line.Split(',');
                        string name = items[2];
                        string id = items[3];
                        int count = int.Parse(items[4]);
                        LocalStatistic localStatistic = localStatistics.FirstOrDefault(x => x.UserName == name);
                        if (localStatistic == null)
                        {
                            localStatistics.Add(new LocalStatistic() { UserName = name,MachineId =id, UserCount = count });
                        }
                        else
                        {
                            localStatistic.UserCount += count;
                        }
                    }
                }
                MainWindowViewModel.SearchList.Clear();
                foreach (var localStatistic in localStatistics)
                {
                    MainWindowViewModel.SearchList.Add(localStatistic);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
