using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Timers;


namespace Demo
{
    public partial class dlgDemo : Form
    {
        IntPtr m_hCom;
        CReader reader = new CReader();
        //byte[] m_serNum;
        //byte[] m_uid;
        //int m_flag;
        //byte[] m_uchar;
        public dlgDemo()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetCommState(
          IntPtr hFile,  //通信设备句柄 
          ref DCB lpDCB    // 设备控制块DCB 
        );

        [DllImport("kernel32.dll")]
        private static extern bool SetCommState(
          IntPtr hFile,  // 通信设备句柄 
          ref DCB lpDCB    // 设备控制块 
        ); 

        [StructLayout(LayoutKind.Sequential)]//托管内存中的物理布局
        public struct DCB
        {
            //taken from c struct in platform sdk  
            public int DCBlength;           // sizeof(DCB)  
            public int BaudRate;            // 指定当前波特率 current baud rate 
            // these are the c struct bit fields, bit twiddle flag to set 
            public int fBinary;          // 指定是否允许二进制模式,在windows95中必须主TRUE binary mode, no EOF check  
            public int fParity;          // 指定是否允许奇偶校验 enable parity checking  
            public int fOutxCtsFlow;      // 指定CTS是否用于检测发送控制，当为TRUE,CTS为OFF，发送将被挂起。 CTS output flow control  
            public int fOutxDsrFlow;      // 指定DSR是否用于检测发送控制 DSR output flow control  
            public int fDtrControl;       // DTR_CONTROL_DISABLE值将DTR置为OFF, DTR_CONTROL_ENABLE值将DTR置为ON, DTR_CONTROL_HANDSHAKE允许DTR"握手" DTR flow control type  
            public int fDsrSensitivity;   // 当该值为TRUE时DSR为OFF时接收的字节被忽略 DSR sensitivity  
            public int fTXContinueOnXoff; // 指定当接收缓冲区已满,并且驱动程序已经发送出XoffChar字符时发送是否停止。TRUE时，在接收缓冲区接收到缓冲区已满的字节XoffLim且驱动程序已经发送出XoffChar字符中止接收字节之后，发送继续进行。　FALSE时，在接收缓冲区接收到代表缓冲区已空的字节XonChar且驱动程序已经发送出恢复发送的XonChar之后，发送继续进行。XOFF continues Tx  
            public int fOutX;          // TRUE时，接收到XoffChar之后便停止发送接收到XonChar之后将重新开始 XON/XOFF out flow control  
            public int fInX;           // TRUE时，接收缓冲区接收到代表缓冲区满的XoffLim之后，XoffChar发送出去接收缓冲区接收到代表缓冲区空的XonLim之后，XonChar发送出去 XON/XOFF in flow control  
            public int fErrorChar;     // 该值为TRUE且fParity为TRUE时，用ErrorChar 成员指定的字符代替奇偶校验错误的接收字符 enable error replacement  
            public int fNull;          // eTRUE时，接收时去掉空（0值）字节 enable null stripping  
            public int fRtsControl;     // RTS flow control  
            /*RTS_CONTROL_DISABLE时,RTS置为OFF 
             RTS_CONTROL_ENABLE时, RTS置为ON 
           RTS_CONTROL_HANDSHAKE时, 
           当接收缓冲区小于半满时RTS为ON 
              当接收缓冲区超过四分之三满时RTS为OFF 
           RTS_CONTROL_TOGGLE时, 
           当接收缓冲区仍有剩余字节时RTS为ON ,否则缺省为OFF*/

            public int fAbortOnError;   // TRUE时,有错误发生时中止读和写操作 abort on error  
            public int fDummy2;        // 未使用 reserved  

            public uint flags;
            public ushort wReserved;          // 未使用,必须为0 not currently used  
            public ushort XonLim;             // 指定在XON字符发送这前接收缓冲区中可允许的最小字节数 transmit XON threshold  
            public ushort XoffLim;            // 指定在XOFF字符发送这前接收缓冲区中可允许的最小字节数 transmit XOFF threshold  
            public byte ByteSize;           // 指定端口当前使用的数据位   number of bits/byte, 4-8  
            public byte Parity;             // 指定端口当前使用的奇偶校验方法,可能为:EVENPARITY,MARKPARITY,NOPARITY,ODDPARITY  0-4=no,odd,even,mark,space  
            public byte StopBits;           // 指定端口当前使用的停止位数,可能为:ONESTOPBIT,ONE5STOPBITS,TWOSTOPBITS  0,1,2 = 1, 1.5, 2  
            public char XonChar;            // 指定用于发送和接收字符XON的值 Tx and Rx XON character  
            public char XoffChar;           // 指定用于发送和接收字符XOFF值 Tx and Rx XOFF character  
            public char ErrorChar;          // 本字符用来代替接收到的奇偶校验发生错误时的值 error replacement character  
            public char EofChar;            // 当没有使用二进制模式时,本字符可用来指示数据的结束 end of input character  
            public char EvtChar;            // 当接收到此字符时,会产生一个事件 received event character  
            public ushort wReserved1;         // 未使用 reserved; do not use  
        }

        private System.Timers.Timer timerClock = new System.Timers.Timer();   

        private void dlgDemo_Load(object sender, EventArgs e)
        {
            int i;
            string strTmp;
            byte[] comm = new byte[256];
            /*int ret = reader.GetSysComm(ref comm[0]);
            if (0 == ret)                                                       //参数设置中的显示
            {
                for (i = 0; i < comm[0]; i++)                                     //设置串口号下拉框初值
                {
                    strTmp = string.Format("COM{0:X}", comm[i + 1]);
                    m_cobComPort.Items.Add(strTmp);
                }
                m_cobComPort.SelectedIndex = 0;
            }else*/
            {
                for (i = 1; i < 0x1F; i++)                                     //设置串口号下拉框初值
                {
                    strTmp = string.Format("COM{0}", i);
                    m_cobComPort.Items.Add(strTmp);
                }
                m_cobComPort.SelectedIndex = 0;
            }
            /////系统设置模块
            m_cobComBaudrate.Items.Clear();
            m_cobComBaudrate.Items.Add("9600");                         //设置波特率下拉框初值
            m_cobComBaudrate.Items.Add("19200");
            m_cobComBaudrate.Items.Add("38400");
            m_cobComBaudrate.Items.Add("57600");
            m_cobComBaudrate.Items.Add("115200");
            m_cobComBaudrate.SelectedIndex = 0;

            m_cobDataBits.Items.Clear();
            m_cobDataBits.Items.Add("5");                               //设置数据位长度下拉框初值
            m_cobDataBits.Items.Add("6");
            m_cobDataBits.Items.Add("7");
            m_cobDataBits.Items.Add("8");
            m_cobDataBits.SelectedIndex = 3;

            m_cobParity.Items.Clear();
            m_cobParity.Items.Add("NONE");                              //设置奇偶校验下拉框初值
            m_cobParity.Items.Add("ODD");
            m_cobParity.Items.Add("EVEN");
            m_cobParity.SelectedIndex = 0;

            m_txtComAddr.Text = "00";                                 //设置当前地址textbox初值

            m_txtNewComAddr.Text = "00";                              //设置新地址textbox初值

            m_radio16hex.Select();

            m_txtSerNum.Text = "AA BB AA BB AA BB AA BB";               //设置序列号初值

            m_txtPosition.Text = "01";                                //设置读写用户信息初值
            m_txtDataLength.Text = "78";
            m_txtUserInfo.Text = "AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55 AA 55";

            m_txtLEDDuration.Text = "18";                             //设置LED初值
            m_txtLEDFreq.Text = "0A";

            m_txtBuzzerDuration.Text = "18";                          //设置Buzzer初值
            m_txtBuzzerFreq.Text = "0A";

            //提示栏初始值
            m_txtMessageParaSet.Text = "\r\n Welcome!\r\n";   //设置信息框初值
            m_txtMessage15693.Text = "\r\n Welcome!\r\n";   //设置信息框初值
            m_txtMessageA.Text = "\r\n Welcome!\r\n";   //设置信息框初值
            m_txtMessageB.Text = "\r\n Welcome!\r\n";   //设置信息框初值
            //15693中的显示
            m_txtInventoryFlag.Text = "06";                           //设置寻卡时的标识初值

            m_txtAFI.Text = "00";                                     //设置AFI初值

            m_txtMaskValue.Text = "";                                   //设置掩码

            m_cobFlagReadWrite.Items.Clear();
            m_cobFlagReadWrite.Items.Add("0X02");                       //设置读写标识下拉框初值
            m_cobFlagReadWrite.Items.Add("0X22");
            m_cobFlagReadWrite.Items.Add("0X42");
            m_cobFlagReadWrite.SelectedIndex = 2;

            m_cobStartBlock.Items.Clear();
            for (i = 0; i < 64; i++)                                    //设置读写块地址下拉框初值
            {
                strTmp = string.Format("{0:X2}", i);
                m_cobStartBlock.Items.Add(strTmp);
            }
            m_cobStartBlock.SelectedIndex = 0;

            m_cobBlockNum.Items.Clear();
            for (i = 1; i < 64; i++)                                    //设置读写块数下拉框初值
            {
                strTmp = string.Format("{0:X2}", i);
                m_cobBlockNum.Items.Add(strTmp);
            }
            m_cobBlockNum.SelectedIndex = 0;

            m_txtDataReadWrite.Text = "11 11 11 11";                    //设置写数据初值

            m_cobFlag.Items.Clear();
            m_cobFlag.Items.Add("0X02");                                //设置标识下拉框初值
            m_cobFlag.Items.Add("0X22");
            m_cobFlag.Items.Add("0X42");
            m_cobFlag.SelectedIndex = 0;

            //m_txtUID.Text = "5E CA B4 18 00 00 07 E0";                  //设置卡号初值

            m_cobFlagLock.Items.Clear();
            m_cobFlagLock.Items.Add("0X02");                            //设置锁块标识下拉框初值
            m_cobFlagLock.Items.Add("0X22");
            m_cobFlagLock.Items.Add("0X42");
            m_cobFlagLock.SelectedIndex = 0;

            m_cobBlockLock.Items.Clear();
            for (i = 1; i < 64; i++)                                    //设置锁专人块号下拉框初值
            {
                strTmp = string.Format("{0:X2}", i);
                m_cobBlockLock.Items.Add(strTmp);
            }
            m_cobBlockLock.SelectedIndex = 0;

            m_txtCmdLength.Text = "02";            //设置命令长度初值

            m_txtCmd.Text = "02 2B";                  //设置命令初值


            //A Card
            m_cobACardMode.Items.Clear();
            m_cobACardMode.Items.Add("All");
            m_cobACardMode.Items.Add("Idll");
            m_cobACardMode.SelectedIndex = 1;

            m_cobSNModeA.Items.Clear();
            m_cobSNModeA.Items.Add("All");
            m_cobSNModeA.Items.Add("Idll");
            m_cobSNModeA.SelectedIndex = 1;

            m_cobSNHaltA.Items.Clear();
            m_cobSNHaltA.Items.Add("None");
            m_cobSNHaltA.Items.Add("Halt");
            m_cobSNHaltA.SelectedIndex = 0;

            m_cobRWBlkStartA.Items.Clear();
            for (i = 0; i < 64; i++) 
            {
                strTmp = string.Format("{0:X2}", i);
                m_cobRWBlkStartA.Items.Add(strTmp);
            }
            m_cobRWBlkStartA.SelectedIndex = 0;


            m_cobRWBlkNumA.Items.Clear();
            for (i = 1; i < 5; i++)
            {
                strTmp = string.Format("{0:X2}", i);
                m_cobRWBlkNumA.Items.Add(strTmp);
            }
            m_cobRWBlkNumA.SelectedIndex = 0;

            m_cobRWModeA.Items.Clear();
            m_cobRWModeA.Items.Add("Idle+KeyA");
            m_cobRWModeA.Items.Add("All+KeyA");
            m_cobRWModeA.Items.Add("Idle+KeyB");
            m_cobRWModeA.Items.Add("All+KeyB");
            m_cobRWModeA.SelectedIndex = 0;

            m_txtRWKeyA.Text = "FF FF FF FF FF FF";
            m_txtRWDataA.Text = "FF FF FF FF FF FF FF FF FF FF FF FF FF FF AA BB";

            m_cobEPSectorA.Items.Clear();
            for (i = 0; i < 0x10; i++)
            {
                strTmp = string.Format("{0:X1}", i);
                m_cobEPSectorA.Items.Add(strTmp);
            }
            m_cobEPSectorA.SelectedIndex = 0;

            m_cobEPModeA.Items.Clear();
            m_cobEPModeA.Items.Add("Idle+KeyA");
            m_cobEPModeA.Items.Add("All+KeyA");
            m_cobEPModeA.Items.Add("Idle+KeyB");
            m_cobEPModeA.Items.Add("All+KeyB");
            m_cobEPModeA.SelectedIndex = 0;

            m_txtEPValueA.Text = "01 00 00 00";
            m_txtEPKeyA.Text = "FF FF FF FF FF FF";

            m_cobCMDCRCA.Items.Clear();
            m_cobCMDCRCA.Items.Add("None");
            m_cobCMDCRCA.Items.Add("CRC");
            m_cobCMDCRCA.SelectedIndex = 0;
            m_txtCMDLenA.Text = "0D";
            m_txtCMDDataA.Text = "00 A4 04 0C 08 A0 00 00 03 41 00 00 01";

            m_txtPCIDB.Text = "41 30 0A 10";
            m_txtCMDLenB.Text = "0A";
            m_txtCMDDataB.Text = "00 00 07 00 A4 00 00 02 3F 00";
        }

        private void tabPageParaSet_Click(object sender, EventArgs e)
        {

        }

        private void tabPageISO15693_Click(object sender, EventArgs e)
        {

        }

        private void btnOpenCloseCom_Click(object sender, EventArgs e)
        {
            if ("Open" == btnOpenCloseCom.Text || "打开串口" == btnOpenCloseCom.Text)                     //判断串口是否已经打开
            {
                if (IntPtr.Zero == reader.GetHComm())                   //判断串口是否打开
                {
                    int ret = reader.OpenComm(m_cobComPort.SelectedIndex + 1,int.Parse(m_cobComBaudrate.Text));
                    if (0 == ret)
                    {
                        if ("Open" == btnOpenCloseCom.Text)
                        {
                            btnOpenCloseCom.Text = "Close";
                        }
                        else
                        {
                            btnOpenCloseCom.Text = "关闭串口";
                        }
                        m_txtMessageParaSet.Text += "\r\n Open success! \r\n";
                    }
                    else
                    {
                        m_txtMessageParaSet.Text += "\r\n Open fail! \r\n";
                    }
                }
            }
            else
            {
                if (IntPtr.Zero != reader.GetHComm())                  //判断串口是否打开
                {
                    int ret = reader.CloseComm();
                    if (0 == ret)
                    {
                        if ("Close" == btnOpenCloseCom.Text)
                        {
                            btnOpenCloseCom.Text = "Open";
                        }
                        else
                        {
                            btnOpenCloseCom.Text = "打开串口";
                        }
                        m_hCom = IntPtr.Zero;
                        m_txtMessageParaSet.Text += "\r\n Close success! \r\n";
                    }
                    else
                    {
                        m_txtMessageParaSet.Text += "\r\n Close fail! \r\n";
                    }
                }
                else 
                {
                    if ("Close" == btnOpenCloseCom.Text)
                    {
                        btnOpenCloseCom.Text = "Open";
                    }
                    else
                    {
                        btnOpenCloseCom.Text = "打开串口";
                    }
                }
            }
            return;
        }

        private void cobParity_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cobComNewAddr_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnSetBaudrate_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                byte[] buf = new byte[256];
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                //int ret = reader.SetBaudrate(deviceAddr, (byte)int.Parse(m_cobComBaudrate.Text), ref buf[0]);
                //有问题,m_cobComBaudrate.Text 一直为空，即0， 也就说波特率一直是9600
                int ret = reader.SetBaudrate(deviceAddr, (byte)m_cobComBaudrate.SelectedIndex, ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageParaSet.Text += "\r\n Baudrate is setting!\r\n";
                }
                else
                {
                    m_txtMessageParaSet.Text += "\r\n Baudrate set error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            string strTmp;
            byte[] buf = new byte[256];
            byte[] comm = new byte[256];
            int i,j, ret = reader.GetSysComm(ref comm[0]);
	        int []baudrate={9600,19200,38400,57600,115200};

            btnTest.Enabled = false;//使按钮失效

            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                reader.CloseComm();//如果串口打开，关闭
            }

            for (i = 0; i < comm[0]; i++)
            {
                reader.OpenComm(comm[i + 1],9600);//用默认的波特率打开串口
                if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
                {
                    DCB dcb = new DCB();
                    for (j = 0;j < 5;j++)
                    {
                        GetCommState(reader.GetHComm(), ref dcb);
                        dcb.BaudRate = baudrate[j];
                        SetCommState(reader.GetHComm(),ref dcb);//可以直接设置不同的波特率吗???
                        ret = reader.GetSerNum(0x00, ref buf[0]);//进行通信，是否能通
                        if (0 == ret)
                        {
                            strTmp = string.Format("\r\n COM{0:X},", comm[i + 1]);
                            m_txtMessageParaSet.Text += strTmp;
                            strTmp = baudrate[j].ToString("D") + "bps\r\n";
                            m_txtMessageParaSet.Text += strTmp;
                            reader.CloseComm();
                            btnOpenCloseCom.Text = "打开串口";//测试串口的波特率，不应该
                            btnTest.Enabled = true;
                            return;
                        }
                    }
                    reader.CloseComm();
                }
            }
            btnOpenCloseCom.Text = "打开串口";
            btnTest.Enabled = true;
        }

        private void btnGetVersion_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.GetVersionNum(deviceAddr, ref buf[0]);
                if (0 == ret)
                {
                    for (int i = 0; i < buf[0];i++ )
                    {
                        strTmp += (char)buf[i + 1];//怎么可以这么相加？？
                    }
                    m_txtMessageParaSet.Text += "\r\n Version:" + strTmp;
                    m_txtMessageParaSet.Text += "\r\n";
                }
                else
                {
                    m_txtMessageParaSet.Text += "\r\n Get version error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnGetSerNum_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.GetSerNum(deviceAddr, ref buf[0]);
                if (0 == ret)
                {
                    strTmp = "\r\n Device Addr:" + buf[0].ToString() + "\r\n Ser Num:";
                    for (int i = 0; i < 8; i++)
                    {
                        strTmp += buf[i + 1].ToString("X2") + " ";
                    }
                    m_txtMessageParaSet.Text += strTmp + "\r\n";
                }
                else
                {
                    m_txtMessageParaSet.Text += "\r\n Get ser num error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnSetAddr_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                byte[] buf = new byte[256];
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                byte newDeviceAddr = (byte)CPublic.HexStringToInt(m_txtNewComAddr.Text);    //获得新设备地址
                if (newDeviceAddr < 0 || newDeviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }
                //为什么要这样进行设置呢？？
                int ret = reader.SetDeviceAddress(deviceAddr, newDeviceAddr, ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address is setting!\r\n";
                }else
                {
                    m_txtMessageParaSet.Text += "\r\n Address set error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnSetSerNum_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                byte[] buf = new byte[256];
                byte[] SerNumByte;
                SerNumByte = CPublic.CharToByte(m_txtSerNum.Text);//可以这样进行赋值？？
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.SetSerNum(deviceAddr, ref SerNumByte[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageParaSet.Text += "\r\n Ser number is setting!\r\n";
                }else
                {
                    m_txtMessageParaSet.Text += "\r\n Ser num set error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnReadUserInfo_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp;
                int numBlk,numLen;
                numBlk = CPublic.HexStringToInt(m_txtPosition.Text);
                numLen = CPublic.HexStringToInt(m_txtDataLength.Text);
                byte[] buf = new byte[256];

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ReadUserInfo(deviceAddr, numBlk, numLen, ref buf[0]);
                if (0 == ret)
                {
                    strTmp = "\r\n User info:";
                    for (int i = 0; i < numLen; i++)
                    {
                        strTmp += buf[i].ToString("X2") + " ";
                    }
                    m_txtMessageParaSet.Text += strTmp + "\r\n";
                }
                else
                {
                    m_txtMessageParaSet.Text += "\r\n Read user info error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnWriteUserInfo_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp;
                int numBlk, numLen;
                byte[] buf;
                numBlk = CPublic.HexStringToInt(m_txtPosition.Text);
                numLen = CPublic.HexStringToInt(m_txtDataLength.Text);

                strTmp = CPublic.StrToHexStr(m_txtUserInfo.Text);
                buf = CPublic.CharToByte(strTmp);
                if (numLen != (strTmp.Length / 2 + strTmp.Length % 2))
                {
                    m_txtMessageParaSet.Text += "\r\n Write user info error,data length unequal to the data input!\r\n";
                    return;
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.WriteUserInfo(deviceAddr, numBlk, numLen, ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageParaSet.Text += "\r\n Write User info success!\r\n";
                }else
                {
                    m_txtMessageParaSet.Text += "\r\n Write user info error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnSetLED_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                byte freq, duration;
                byte[] buf = new byte[256];

                freq = (byte)CPublic.HexStringToInt(m_txtLEDDuration.Text);                      //获取LED设置频率

                duration = (byte)CPublic.HexStringToInt(m_txtLEDFreq.Text);                     //获取LED设置次数

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ControlLED(deviceAddr, freq, duration, ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageParaSet.Text += "\r\n LED is setting!\r\n";
                }
                else
                {
                    m_txtMessageParaSet.Text += "\r\n LED set error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnSetBuzzer_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                byte freq, duration;
                byte[] buf = new byte[256];

                freq = (byte)CPublic.HexStringToInt(m_txtBuzzerDuration.Text);        //获取蜂鸣器设置频率

                duration = (byte)CPublic.HexStringToInt(m_txtBuzzerFreq.Text);                    //获取蜂鸣器设置次数

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageParaSet.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ControlBuzzer(deviceAddr, freq, duration, ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageParaSet.Text += "\r\n Buzzer is setting!\r\n";
                }
                else
                {
                    m_txtMessageParaSet.Text += "\r\n Buzzer set error!";
                    m_txtMessageParaSet.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageParaSet.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnClearParaSetRet_Click(object sender, EventArgs e)
        {
            m_txtMessageParaSet.Text = "";
        }

        private void btn15693Request_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                byte flag, afi, dataLen, nrOfCard = 0;
                byte[] buf = new byte[256];
                byte[] maskValue;
                string str,strTmp;

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                flag = (byte)CPublic.HexStringToInt(m_txtInventoryFlag.Text);               //获取寻卡时标识

                afi = (byte)CPublic.HexStringToInt(m_txtAFI.Text);                          //获取AFI

                strTmp = CPublic.StrToHexStr(m_txtMaskValue.Text);
                maskValue = CPublic.CharToByte(strTmp);                                     //获得掩码字符串
                dataLen = (byte)(strTmp.Length / 2 + strTmp.Length % 2);                    //获得掩码字符串长度 长度这样算？？
                if (dataLen > 6)
                {
                    m_txtMessage15693.Text += "\r\n Inquire card error,Mask value must less than 7!\r\n";
                    return;
                }
                int ret = reader.ISO15693_Inventory(deviceAddr, flag, afi, dataLen, ref maskValue[0], ref nrOfCard, ref buf[0]);
                if (0 == ret)
                {
                    str = "\r\nnumber of card is:" + nrOfCard.ToString() + "\r\n";
                    m_txtUIDReadWrite.Text = "";
                    m_txtUIDLock.Text = "";
                    m_txtUID.Text = "";

                    for (int i = 0; i < 10 * nrOfCard; i++)
                    {
                        strTmp = string.Format("{0:X2} ", buf[i]);

                        if (i > 1 && i < 10)
                        {
                            //m_uid[j++] = buf[i];
                            m_txtUIDReadWrite.Text += strTmp;
                            m_txtUIDLock.Text += strTmp;
                            m_txtUID.Text += strTmp;
                        }
                        str += strTmp;
                    }
                    m_txtMessage15693.Text += str + "\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Inquire card error!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
            CheckForIllegalCrossThreadCalls = true;
        }

        private void btnRead15693_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string str, strTmp;
                byte[] buf = new byte[256];
                byte[] uid = new byte[8];
                byte flag,blkAdd, numBlk,returnlen = 0;

                flag = (byte)CPublic.HexStringToInt(m_cobFlagReadWrite.Text);               //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUIDReadWrite.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                blkAdd = (byte)CPublic.HexStringToInt(m_cobStartBlock.Text);                //获取所要读取的块地址

                numBlk = (byte)CPublic.HexStringToInt(m_cobBlockNum.Text);                  //获取所要读取的块数

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_Read(deviceAddr, flag, blkAdd, numBlk, ref uid[0],ref returnlen, ref buf[0]);
                if (0 == ret)
                {
                    strTmp = string.Format("{0:X2} ", returnlen);
                    str = "\r\n Length of read:" + strTmp + "\r\n Data:";
                    for (int i = 0; i < returnlen; i++)
                    {
                        strTmp = string.Format(" {0:X2} ", buf[i]);
                        if (strTmp.Length < 2)
                        {
                            strTmp = strTmp.PadLeft(2, '0');
                        }
                        str += strTmp;
                        //m_txtData.Text = strTmp;
                    }
                    m_txtMessage15693.Text += str + "\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Read ISO15693 card error!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnProtectionStatus_Click(object sender, EventArgs e)
        {

        }

        private void btnGetMulSecurity15693_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string str, strTmp;
                byte[] buf = new byte[256];
                byte[] flags = new byte[64];
                byte[] uid = new byte[8];
                byte flag, blkAdd, numBlk, returnlen = 0;

                flag = (byte)CPublic.HexStringToInt(m_cobFlagReadWrite.Text);               //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUIDReadWrite.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                blkAdd = (byte)CPublic.HexStringToInt(m_cobStartBlock.Text);                //获取所要读取的块地址

                numBlk = (byte)CPublic.HexStringToInt(m_cobBlockNum.Text);                  //获取所要读取的块数

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_GetMulSecurity(deviceAddr, flag, blkAdd, numBlk, ref uid[0],ref flags[0], ref returnlen, ref buf[0]);
                if (0 == ret)
                {
                    str = "\r\n Read Flags:";
                    for (int i = 0; i < numBlk; i++)//
                    {
                        strTmp = string.Format(" {0:X2} ", flags[i]);
                        str += strTmp;
                    }
                    
                    strTmp = string.Format("{0:X2} ", returnlen);
                    str += "\r\n Length of mul security data:" + strTmp + "\r\n Data:";
                    for (int i = 0; i < returnlen; i++)
                    {
                        strTmp = string.Format(" {0:X2} ", buf[i]);
                        str += strTmp;
                    }
                    str += "\r\n";
                    m_txtMessage15693.Text += str;
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Read ISO15693 card error!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnWrite15693_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] buf, uid = new byte[8],flags = new byte[64];
                byte flag, blkAdd, numBlk;

                flag = (byte)CPublic.HexStringToInt(m_cobFlagReadWrite.Text);               //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUIDReadWrite.Text);//应该根据flag来判断是否需要uid
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                blkAdd = (byte)CPublic.HexStringToInt(m_cobStartBlock.Text);                //获取所要读取的块地址

                numBlk = (byte)CPublic.HexStringToInt(m_cobBlockNum.Text);                  //获取所要读取的块数

                strTmp = CPublic.StrToHexStr(m_txtDataReadWrite.Text);                      //获取用户所要写入的数据
                buf = CPublic.CharToByte(strTmp);
                if ((strTmp.Length / 2 + strTmp.Length % 2) != (numBlk * 4))
                {
                    m_txtMessage15693.Text += "\r\n The length of data isn't equal to block num multiply four!\r\n";
                    return;
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_Write(deviceAddr, flag, blkAdd, numBlk, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Write ISO15693 card success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Write ISO15693 card error!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnGetSysInfo_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string str,strTmp;
                byte[] uid = new byte[8], flags = new byte[64];
                byte[] buf = new byte[256];
                byte flag;

                flag = (byte)CPublic.HexStringToInt(m_cobFlag.Text);                        //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_GetSysInfo(deviceAddr, flag, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    str = "\r\n System info:\r\n";
                    strTmp = string.Format("\r\n Flags:{0:X2}", buf[0]);
                    str += strTmp;
                    strTmp = string.Format("\r\n INFO Flags:{0:X2}\r\n UID:", buf[1]);
                    str += strTmp;

                    strTmp = "";
                    for (int i = 2; i < 10; i++)
                    {
                        strTmp += buf[i].ToString("X2") + " ";
                    }
                    str += strTmp;

                    strTmp = string.Format("\r\n DSFID:{0:X2}", buf[10]);
                    str += strTmp;

                    strTmp = string.Format("\r\n AFI:{0:X2}\r\n Other fields:", buf[11]);
                    str += strTmp;

                    strTmp = "";
                    for (int i = 12; i < 15; i++)
                    {
                        strTmp += buf[i].ToString("X2") + " ";
                    }
                    str += strTmp;

                    m_txtMessage15693.Text += str + "\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Get system info error!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnStayQuiet_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] uid = new byte[8], flags = new byte[64];
                byte[] buf = new byte[256];
                byte flag;

                flag = (byte)CPublic.HexStringToInt(m_cobFlag.Text);                        //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_StayQuiet(deviceAddr, flag, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Stay quiet success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Stay quiet failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] uid = new byte[8], flags = new byte[64];
                byte[] buf = new byte[256];
                byte flag;

                flag = (byte)CPublic.HexStringToInt(m_cobFlag.Text);                        //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("please choose the flag which is 0x22!", "warning", MessageBoxButtons.OK);
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_Select(deviceAddr, flag, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Select card success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Select card failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnResetReady_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] uid = new byte[8], flags = new byte[64];
                byte[] buf = new byte[256];
                byte flag;

                flag = (byte)CPublic.HexStringToInt(m_cobFlag.Text);                        //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_ResetToReady(deviceAddr, flag, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Reset to ready success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Reset to ready failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnWriteDESFID_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] uid = new byte[8], flags = new byte[64];
                byte[] buf = new byte[256];
                byte flag, DSFID = 0; ;

                flag = (byte)CPublic.HexStringToInt(m_cobFlag.Text);                        //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                if (m_txtDSFID.Text.Length == 0)
                {
                    MessageBox.Show(this, "Please enter DSFID!", "warning!", MessageBoxButtons.OK);
                    return;
                }
                else if (m_txtDSFID.Text.Length > 2)
                {
                    MessageBox.Show(this, "Please enter two number!", "warning", MessageBoxButtons.OK);
                    return;
                }
                else
                {
                    DSFID = (byte)CPublic.HexStringToInt(m_txtDSFID.Text);//可输入数组的窗口，最好是先变换成字符16进制字符串，再来转换 兼容性//获得DSFID
                    if (DSFID == 1)
                    {
                        return;
                    }
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_WriteDSFID(deviceAddr, flag, DSFID, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Write DSFID success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Write DSFID failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnWriteAFI_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] uid = new byte[8], flags = new byte[64];
                byte[] buf = new byte[256];
                byte flag, afi = 0;

                flag = (byte)CPublic.HexStringToInt(m_cobFlag.Text);                        //获取标识号

                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                if (m_txtDSFID.Text.Length == 0)
                {
                    MessageBox.Show(this, "Please enter AFI!", "warning!", MessageBoxButtons.OK);
                    return;
                }
                else if (m_txtDSFID.Text.Length > 2)
                {
                    MessageBox.Show(this, "Please enter two number", "warning", MessageBoxButtons.OK);
                    return;
                }
                else
                {
                    afi = (byte)CPublic.HexStringToInt(m_txtDSFID.Text); //怎么从这里读取,应该有一个AFI输入框吧？ //获afi号
                    if (afi == 1)//HexStringToInt 函数出现问题,格式不正确
                    {
                        return;
                    }
                }
                
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_WriteAFI(deviceAddr, flag, afi, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Write afi success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Write afi failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnLockFAI_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] uid = new byte[8], flags = new byte[64];
                byte[] buf = new byte[256];
                byte flag;

                flag = (byte)CPublic.HexStringToInt(m_cobFlag.Text);                        //获取标识号
                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_LockAFI(deviceAddr, flag, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Lock AFI success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Lock AFI failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnLockDESFid_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] uid = new byte[8], flags = new byte[64];
                byte[] buf = new byte[256];
                byte flag;

                flag = (byte)CPublic.HexStringToInt(m_cobFlag.Text);                        //获取标识号
                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_LockDSFID(deviceAddr, flag, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Lock DESFid success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Lock DESFid failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string strTmp;
                byte[] uid = new byte[8];
                byte[] buf = new byte[256];
                byte flag,blogLock;

                flag = (byte)CPublic.HexStringToInt(m_cobFlagLock.Text);                    //获取标识号
                if (flag == 0x22)
                {
                    strTmp = CPublic.StrToHexStr(m_txtUID.Text);
                    uid = CPublic.CharToByte(strTmp);                           //获得卡号
                    if (strTmp.Length != 16)
                    {
                        m_txtMessage15693.Text += "\r\n UID of card ISO15693 is not 8 byte!\r\n";
                        return;
                    }
                }

                blogLock = (byte)CPublic.HexStringToInt(m_cobBlockLock.Text);                 //获得锁块的块号

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_Lock(deviceAddr, flag, blogLock, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessage15693.Text += "\r\n Lock success!\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Lock failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnTransferCmd_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                                           //判断串口是否打开
            {
                string str, strTmp;
                byte[] buf = new byte[256];
                byte[] cmd;
                byte returnLen = 0;
                int cmdLength;

                strTmp = CPublic.StrToHexStr(m_txtCmdLength.Text);
                cmdLength = CPublic.HexStringToInt(strTmp);                    //获取命令长度

                strTmp = CPublic.StrToHexStr(m_txtCmd.Text);                      //获得命令
                cmd = CPublic.CharToByte(strTmp);
                if (strTmp.Length != cmdLength)
                {
                    m_txtMessageParaSet.Text += "\r\n The length of cmd isn't equal to cmd input!\r\n";
                    return;
                }

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);                 //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessage15693.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.ISO15693_TransCmd(deviceAddr,ref cmd[0],cmdLength,ref returnLen,ref buf[0]);
                if (0 == ret)
                {
                    str = "\r\n Cmd is execute,Data:";
                    for (int i = 0; i < returnLen; i++)
                    {
                        strTmp = string.Format(" {0:X2} ", buf[i]);
                        str += strTmp;
                    }
                    m_txtMessage15693.Text += str + "\r\n";
                }
                else
                {
                    m_txtMessage15693.Text += "\r\n Lock failed!";
                    m_txtMessage15693.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessage15693.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void m_txtMessageParaSet_TextChanged(object sender, EventArgs e)
        {
            m_txtMessageParaSet.SelectionStart = m_txtMessageParaSet.Text.Length;

            m_txtMessageParaSet.ScrollToCaret(); 
        }

        private void m_txtMessage15693_TextChanged(object sender, EventArgs e)
        {
            m_txtMessage15693.SelectionStart = m_txtMessage15693.Text.Length;

            m_txtMessage15693.ScrollToCaret();
        }

        private void m_checkAutoRequest_CheckedChanged(object sender, EventArgs e)
        {
            timerClock.Enabled = false;
            if (m_checkAutoRequest.Checked)
            {
                timerClock.Elapsed += new ElapsedEventHandler(btn15693Request_Click);
                timerClock.Interval = 1000;
                timerClock.Enabled = true;
            }
        }

        private void btnClear15693Ret_Click(object sender, EventArgs e)
        {
            m_txtMessage15693.Text = "";
        }

        private void btnClearARet_Click(object sender, EventArgs e)
        {
            m_txtMessageA.Text = "";
        }

        private void m_btnACardRequest_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte requestMode;
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                requestMode = (byte)m_cobACardMode.SelectedIndex;
                int ret = reader.MF_Request(deviceAddr, requestMode, ref buf[0]);
                if (0 == ret)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        strTmp += string.Format("{0:X2} ", buf[i]) ;
                    }
                    m_txtMessageA.Text += "\r\n Type of card is:" + strTmp;
                    m_txtMessageA.Text += "\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Get card type error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnACardAnticoll_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte flag = 0;
                
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.MF_Anticoll(deviceAddr, ref flag, ref buf[0]);
                       
                //MessageBox.Show(count.ToString());
                if (0 == ret)
                {
                    if (0 == flag)
                    {
                        m_txtMessageA.Text += "\r\n Detected one card.\r\n";
                    }
                    else
                    {
                        m_txtMessageA.Text += "\r\n Detected more than one card.\r\n SN of one. ";
                    }

                    ////////////////////返回卡号的长度，字节数 并且有问题，返回的是数字，假如卡号中也有一个字节为数字0的话，长度就会不正确
                    /////////////////// 那该怎么处理呢，可以从底层返回长度，修改API（也有两种方法 1.单独一个参数，2.从其他参数一起传上来）
                    //////////////////  可以在上层做处理，但是怎么处理内，
                    int count = 0;
                    while (buf[count++] != 0x00) ;
                    count--;

                    //if (count == 4)
                    //int len = buf.ToString().Length / 2 + buf.ToString().Length % 2;
                    //MessageBox.Show(len.ToString("D"));
                    if (count == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }
                    else if (count == 7)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }
                   
                    m_txtUIDSelectA.Text = strTmp;//在UIDtextbox中显示得到的序列号，可能有问题

                    m_txtMessageA.Text += "\r\n Number of card is:" + strTmp;
                    m_txtMessageA.Text += "\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Anticoll error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnSelectA_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] uid = new byte[64];
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                strTmp = CPublic.StrToHexStr(m_txtUIDSelectA.Text);
                uid = CPublic.CharToByte(strTmp);                           //获得卡号
                
                int ret = reader.MF_Select(deviceAddr, ref uid[0], ref buf[0]);
                if (0 == ret)
                {
                    strTmp = "\r\n ";
                    //有关4位和7位的兼容
                    for (int i = 0; i < 4; i++)
                    {
                        strTmp += string.Format("{0:X2} ", buf[i]); 
                    }
                    //vc版本的直接就拿m_txtUIDSelectA.text的内容进行操作，那么这段代码就无用了
                    m_txtMessageA.Text += "\r\n Select card is:" + strTmp;
                    m_txtMessageA.Text += "\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Select card error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnHaltA_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.MF_Halt(deviceAddr);
                if (0 == ret)
                {
                    m_txtMessageA.Text += "\r\n Halt card succeed!\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Halt card error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) ;
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnGetSNA_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];

                //Array.Clear(buf, 0, 8);

                byte mode,cmd,flag = 0;

                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);          //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                mode = (byte)m_cobSNModeA.SelectedIndex;
                cmd = (byte)m_cobSNHaltA.SelectedIndex;
                int ret = reader.MF_GET_SNR(deviceAddr,mode,cmd,ref flag, ref buf[0]);
               
                //MessageBox.Show(buf[2].ToString("X2"));
                if (0 == ret)
                {
                    if (0 == flag)
                    {
                        m_txtMessageA.Text += "\r\n Detected one card.\r\n";
                    }
                    else
                    {
                        m_txtMessageA.Text += "\r\n Detected more than one card.\r\n SN of one. ";
                    }

                    //还是有问题，假如卡号有一位为0x00 也是有问题的，解决的办法只有改库，增添一个参数，代表返回值得长度。
                    int count = 0;
                    while (buf[count++] != 0x00) ;
                    count--;

                    if (count == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }
                    else if (count == 7)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }

                    m_txtMessageA.Text += "\r\n Number of card is:" + strTmp;
                    m_txtMessageA.Text += "\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Get card number error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnReadA_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] snr = new byte[128];
                byte mode, blkStart, blkNum = 0;
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                mode = (byte)m_cobRWModeA.SelectedIndex;
                blkNum = (byte)(m_cobRWBlkNumA.SelectedIndex + 1);
                blkStart = (byte)m_cobRWBlkStartA.SelectedIndex;
                strTmp = CPublic.StrToHexStr(m_txtRWKeyA.Text);
                snr = CPublic.CharToByte(strTmp);                           //获得卡号

                int ret = reader.MF_Read(deviceAddr, mode, blkStart, blkNum, ref snr[0], ref buf[0]);

                 if (0 == ret)
                {
                    strTmp = "\r\n ";
                    m_txtMessageA.Text += "\r\n SN of card is:";

                    int count = 0;
                    while (snr[count++] != 0x00) ;
                    count--;

                    if (count == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            strTmp += string.Format("{0:X2} ", snr[i]);
                        }
                    }
                    else if (count == 7)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            strTmp += string.Format("{0:X2} ", snr[i]);
                        }
                    }
                    
                    m_txtMessageA.Text += strTmp;
                    //MessageBox.Show(buf[0].ToString("X2"));
                    strTmp = "\r\n ";
                    for (int i = 0; i < 16 * blkNum; i++)
                    {
                        strTmp += string.Format("{0:X2} ", buf[i]); 
                    }

                    m_txtMessageA.Text += "\r\n Received data from card is:" + strTmp;
                    m_txtMessageA.Text += "\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Read card error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }
        //
        private void btnWriteA_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] key = new byte[8];
                byte[] text = new byte[32];
                byte mode, blkStart, blkNum = 0;
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                mode = (byte)m_cobRWModeA.SelectedIndex;
                blkNum = (byte)(m_cobRWBlkNumA.SelectedIndex + 1);
                blkStart = (byte)m_cobRWBlkStartA.SelectedIndex;
                strTmp = CPublic.StrToHexStr(m_txtRWKeyA.Text);
                key = CPublic.CharToByte(strTmp);                           //获得卡号

                strTmp = CPublic.StrToHexStr(m_txtRWDataA.Text);
                text = CPublic.CharToByte(strTmp);                           //获得写入数据

                int ret = reader.MF_Write(deviceAddr, mode, blkStart, blkNum, ref key[0], ref text[0], ref buf[0]);

                if (0 == ret)
                {
                    strTmp = "\r\n ";
                    m_txtMessageA.Text += "\r\n SN of card is:";

                    int count = 0;
                    while (buf[count++] != 0x00) ;
                    count--;

                    if (count == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }
                    else if (count == 7)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }
                    m_txtMessageA.Text += strTmp;
                    m_txtMessageA.Text += "\r\n write succeed!\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Write card error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnEPInitValA_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] key = new byte[128];
                byte mode, sector = 0;
                int value,deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                mode = (byte)m_cobRWModeA.SelectedIndex;
                sector = (byte)m_cobEPSectorA.SelectedIndex;
                strTmp = CPublic.StrToHexStr(m_txtEPKeyA.Text);
                key = CPublic.CharToByte(strTmp);                           //转换key

                strTmp = CPublic.StrToHexStr(m_txtEPValueA.Text);
                //MessageBox.Show(strTmp);
                value = CPublic.HexStringToInt(strTmp);                     //获得写入数据
                if (value == -1)
                {
                    return;
                }
                //MessageBox.Show(value.ToString("X8"));

                int ret = reader.MF_InitVal(deviceAddr, mode, sector,ref key[0],value,ref buf[0]);
                if (0 == ret)
                {
                    strTmp = "\r\n ";
                    m_txtMessageA.Text += "\r\n Electronic Purse initialization succeed!\r\n Initialization card:";

                    int count = 0;
                    while (buf[count++] != 0x00);
                    count--;

                    //MessageBox.Show(buf.ToString().Length.ToString("D"));
                    //if (buf.ToString().Length == 5)
                    if (count == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]); 
                        }
                    }
                    else if (count == 7)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }
                   
                    m_txtMessageA.Text += strTmp + "\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Initialization Electronic Purse error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnEPDecValA_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] key = new byte[128];
                byte mode, sector = 0;
                int value;
                UInt32 result = 0;
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                mode = (byte)m_cobRWModeA.SelectedIndex;
                sector = (byte)m_cobEPSectorA.SelectedIndex;
                strTmp = CPublic.StrToHexStr(m_txtEPKeyA.Text);
                key = CPublic.CharToByte(strTmp);                           //key

                strTmp = CPublic.StrToHexStr(m_txtEPValueA.Text);
                value = CPublic.HexStringToInt(strTmp);                           //获得写入数据

                int ret = reader.MF_Dec(deviceAddr, mode, sector, ref key[0], value, ref result, ref buf[0]);
                if (0 == ret)
                {
                    strTmp = "\r\n ";
                    m_txtMessageA.Text += "\r\n Electronic Purse Decrease succeed!\r\n Decrease card:";
                    
                    int count = 0;
                    while (buf[count++] != 0x00);
                    count--;

                    if (count == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }
                    else if (count == 7)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            strTmp += string.Format("{0:X2} ", buf[i]);
                        }
                    }

                    m_txtMessageA.Text += strTmp + "\r\n Result value:";
                    //显示计算结果
                    //MessageBox.Show(result.ToString("X8"));
                    strTmp = string.Format("{0:X2} ", (result / 0x1000000) % 0x100);
                    strTmp += string.Format("{0:X2} ", (result / 0x10000) % 0x100);
                    strTmp += string.Format("{0:X2} ", (result / 0x100) % 0x100);
                    strTmp += string.Format("{0:X2} ", result % 0x100);

                    m_txtMessageA.Text += strTmp;
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Electronic Purse decrease error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnEPIncValA_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] key = new byte[128];
                byte mode, sector = 0;
                int value; 
                UInt32 result = 0;
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                mode = (byte)m_cobRWModeA.SelectedIndex;
                sector = (byte)m_cobEPSectorA.SelectedIndex;
                strTmp = CPublic.StrToHexStr(m_txtEPKeyA.Text);
                key = CPublic.CharToByte(strTmp);                           //获得卡号

                strTmp = CPublic.StrToHexStr(m_txtEPValueA.Text);
                value = CPublic.HexStringToInt(strTmp);                           //获得写入数据

                int ret = reader.MF_Inc(deviceAddr, mode, sector, ref key[0], value, ref result, ref buf[0]);
                if (0 == ret)
                {
                    strTmp = "\r\n ";
                    m_txtMessageA.Text += "\r\n Electronic Purse increase succeed!\r\n Increase card:";
                    for (int i = 0; i < 4; i++)
                    {
                        strTmp += string.Format("{0:X2} ", buf[i]); 
                    }
                    m_txtMessageA.Text += strTmp + "\r\n Result value:";
                    
                    strTmp = string.Format("{0:X2} ", (result / 0x1000000) % 0x100);
                    strTmp += string.Format("{0:X2} ", (result / 0x10000) % 0x100);
                    strTmp += string.Format("{0:X2} ", (result / 0x100) % 0x100);
                    strTmp += string.Format("{0:X2} ", result % 0x100);

                    m_txtMessageA.Text += strTmp;
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Electronic Purse increase error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnCMDSend_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] cmd = new byte[128];
                byte CRC,dataLen,returnLen = 0;
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageA.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                CRC = (byte)m_cobCMDCRCA.SelectedIndex;
                strTmp = CPublic.StrToHexStr(m_txtCMDDataA.Text);
                cmd = CPublic.CharToByte(strTmp);                           //获得卡号
                dataLen = (byte)CPublic.HexStringToInt(m_txtCMDLenA.Text); ;

                int ret = reader.MF_TransferCMD(deviceAddr, CRC, dataLen,ref cmd[0], ref returnLen, ref buf[0]);
                if (0 == ret)
                {
                    //返回的数据看命令而定
                    strTmp = "\r\n ";
                    m_txtMessageA.Text += "\r\n SN of card is:";
                    for (int i = 0; i < 4; i++)
                    {
                        strTmp += string.Format("{0:X2} ", buf[i]); 
                    }
                    m_txtMessageA.Text += strTmp;
                    m_txtMessageA.Text += "\r\n";
                }
                else
                {
                    m_txtMessageA.Text += "\r\n Read card error!";
                    m_txtMessageA.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageA.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void m_txtMessageA_TextChanged(object sender, EventArgs e)
        {
            m_txtMessageA.SelectionStart = m_txtMessageA.Text.Length;

            m_txtMessageA.ScrollToCaret();
        }

        private void m_txtMessageB_TextChanged(object sender, EventArgs e)
        {
            m_txtMessageB.SelectionStart = m_txtMessageB.Text.Length;

            m_txtMessageB.ScrollToCaret();
        }

        private void btnRequestB_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte returnLen = 0;
                //这里HexStringToInt函数返回值与“地址范围”有重叠，该怎么处理比较好？？有很多处
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);//获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageB.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.Request_B(deviceAddr,ref returnLen, ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageB.Text += "\r\n Received length of data from card is:" + string.Format("{0:X2} ", returnLen);
                    for (int i = 0; i < returnLen; i++)
                    {
                        strTmp += string.Format("{0:X2} ", buf[i]);
                    }
                    m_txtMessageB.Text += "\r\n Received data from card is:" + strTmp;
                    m_txtMessageB.Text += "\r\n";
                }
                else
                {
                    m_txtMessageB.Text += "\r\n Get card error!";
                    m_txtMessageB.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageB.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnResetB_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                byte[] buf = new byte[256];
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageB.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.RESET_B(deviceAddr, ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageB.Text += "\r\n Reset succeed!\r\n";
                }
                else
                {
                    m_txtMessageB.Text += "\r\n Reset error!";
                    m_txtMessageB.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageB.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnAnticollB_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte returnLen = 0;
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageB.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                int ret = reader.Anticoll_B(deviceAddr, ref returnLen, ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageB.Text += "\r\n Received length of data from card is:" + string.Format("{0:X2} ", returnLen);
                    for (int i = 0; i < returnLen; i++)
                    {
                        strTmp += string.Format("{0:X2} ", buf[i]);
                    }
                    m_txtMessageB.Text += "\r\n Received data from card is:" + strTmp;
                    m_txtMessageB.Text += "\r\n";
                }
                else
                {
                    m_txtMessageB.Text += "\r\n Get card error!";
                    m_txtMessageB.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageB.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnWriteB_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] SerialNum = new byte[128];
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageB.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                strTmp = CPublic.StrToHexStr(m_txtPCIDB.Text);
                SerialNum = CPublic.CharToByte(strTmp);                           //获得写入数据

                int ret = reader.Attrib_B(deviceAddr, ref SerialNum[0], ref buf[0]);
                if (0 == ret)
                {
                    m_txtMessageB.Text += "\r\n Attrib succeed!" + strTmp;
                    m_txtMessageB.Text += "\r\n";
                }
                else
                {
                    m_txtMessageB.Text += "\r\n Attrib error!";
                    m_txtMessageB.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageB.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void btnCMDB_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != reader.GetHComm())                             //判断串口是否打开
            {
                string strTmp = "\r\n ";
                byte[] buf = new byte[256];
                byte[] cmd = new byte[128];
                byte cmdSize, returnLen = 0;
                int deviceAddr = CPublic.HexStringToInt(m_txtComAddr.Text);    //获得设备地址
                if (deviceAddr < 0 || deviceAddr > 255)
                {
                    m_txtMessageB.Text += "\r\n Device address must between 0X00-0XFF!\r\n";
                    return;
                }

                strTmp = CPublic.StrToHexStr(m_txtCMDDataB.Text);
                cmd = CPublic.CharToByte(strTmp);                           //获得卡号
                
                cmdSize = (byte)CPublic.HexStringToInt(m_txtCMDLenB.Text); ;

                int ret = reader.TransferCMD_B(deviceAddr, cmdSize, ref cmd[0], ref returnLen, ref buf[0]);
                if (0 == ret)
                {
                    strTmp = "\r\n ";
                    m_txtMessageB.Text += "\r\n Received data from card is:";
                    for (int i = 0; i < returnLen; i++)
                    {
                        strTmp += string.Format("{0:X2} ", buf[i]);
                    }
                    m_txtMessageB.Text += strTmp;
                    m_txtMessageB.Text += "\r\n";
                }
                else
                {
                    m_txtMessageB.Text += "\r\n Send CMD error!";
                    m_txtMessageB.Text += "\r\n Api error:" + CPublic.ApiError(ret) + " Return error:" + CPublic.ReturnCodeError(ref buf[0]);
                }
            }
            else
            {
                m_txtMessageB.Text += "\r\n Comm isn't open!\r\n";
            }
        }

        private void m_radio16hex_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void m_txtInventoryFlag_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnClearBRet_Click(object sender, EventArgs e)
        {
            m_txtMessageB.Text = "";
        }

        private void m_cobComPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}