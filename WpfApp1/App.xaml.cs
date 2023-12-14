using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NFCDemo
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //防止打开两个
        private static System.Threading.Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, "NFCDemo", out ret);
           
            if (!ret)
            {
                MessageBox.Show("程序已经在运行中...");
                Environment.Exit(0);
            }
            base.OnStartup(e);
        }
    }
}
