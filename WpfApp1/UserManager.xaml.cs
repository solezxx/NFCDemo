using System;
using System.Collections.Generic;
using System.IO;
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

namespace NFCDemo
{
    /// <summary>
    /// UserManager.xaml 的交互逻辑
    /// </summary>
    public partial class UserManager : Window
    {
        public UserManager()
        {
            InitializeComponent();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要删除吗？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var user = (sender as Button).DataContext as User;
                MainWindowViewModel.AllUser.Remove(user);
                string path = AppDomain.CurrentDomain.BaseDirectory + "user.csv";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                foreach (var item in MainWindowViewModel.AllUser)
                {
                    CSVFile.AddNewLine(path, new []{item.ID,item.Name});
                }
            }
        }
    }
}
