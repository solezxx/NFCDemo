using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class CSVFile
    {
        public static bool SaveToCsv(DataTable dt, string strFilePath, string tableHeader, string columName)
        {
            try
            {
                string text = "";
                StreamWriter streamWriter = new StreamWriter(strFilePath, append: false,encoding: Encoding.UTF8);
                streamWriter.WriteLine(tableHeader);
                streamWriter.WriteLine(columName);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    text = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j > 0)
                        {
                            text += ",";
                        }

                        text += dt.Rows[i][j].ToString();
                    }

                    streamWriter.WriteLine(text);
                }

                streamWriter.Close();
                return true;
            }
            catch
            {
                Console.WriteLine("保存CSV文件失败");
                return false;
            }
        }

        public static DataTable GetFromCsv(string filePath, int n, DataTable dt)
        {
            StreamReader streamReader = new StreamReader(filePath, Encoding.Default, detectEncodingFromByteOrderMarks: false);
            int num = 0;
            int num2 = 0;
            streamReader.Peek();
            while (streamReader.Peek() > 0)
            {
                num2++;
                string text = streamReader.ReadLine();
                if (num2 >= n + 1)
                {
                    string[] array = text.Split(',');
                    DataRow dataRow = dt.NewRow();
                    for (num = 0; num < array.Length; num++)
                    {
                        dataRow[num] = array[num];
                    }

                    dt.Rows.Add(dataRow);
                }
            }

            streamReader.Close();
            return dt;
        }

        public static bool AddNewLine(string strFilePath, string[] value)
        {
            try
            { 
                StreamWriter streamWriter = new StreamWriter(strFilePath, append: true, encoding: Encoding.GetEncoding("GB2312"));
                streamWriter.WriteLine(string.Join(",", value));
                streamWriter.Flush();
                streamWriter.Close();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("CSV添加一行数据失败");
                return false;
            }
        }
    }
}
