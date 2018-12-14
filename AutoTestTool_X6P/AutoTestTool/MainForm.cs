using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Threading;





namespace AutoTestTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }



        /*****************************************定义类型*******************************************/
        enum Command
        {
            CMD_KEY_TEST = 0x01,               //按键测试
            CMD_CARD_TEST = 0x02,              //刷卡测试   
            CMD_LCD_TEST = 0x03,               //LCD测试
            CMD_2G_TEST = 0x04,                //2G测试
            CMD_TRUMPET_TEST = 0x05,           //喇叭测试
            CMD_RELAY_TEST = 0x06,             //继电器测试     
            CMD_SET_PCB = 0x07,                //设置PCB                    
            CMD_SET_SN = 0x08,                 //设置桩号
            CMD_BT_TEST = 0x09,                //蓝牙测试
            CMD_GET_FW = 0x0A,                 //获取软件版本
            CMD_READ_PCB_CODE = 0x0B,          //读取PCB编码
            CMD_SET_REGISTER_CODE = 0x0C,      //设置注册码
            CMD_SET_DEV_TYPE = 0x0D,           //设置设备类型
            CMD_SET_2_4G_GW_ADD = 0x0E,        //设置2.4G网关地址        
            CMD_SET_TERMINAL_INFO = 0x0F,      //设置中端信息
            CMD_SET_SERVER_ADDR = 0x10,        //设置服务器地址
            CMD_SET_SERVER_PORT = 0x11,        //设置服务器端口
            CMD_SET_PRINT_SWITCH = 0x12,       //设置打印开关
            CMD_REBOOT = 0x13,                 //设备重启
            CMD_SET_DEVICE_ID = 0x14,          //设置识别码
            CMD_SET_RTC = 0x15,                //设置RTC
            CMD_GET_RTC = 0x16,                //读取RTC
            CMD_FLASH_TEST = 0x17,             //flash测试


            CMD_LED_TEST = 0x1A,             //指示灯测试
            CMD_485_TEST = 0x1B,             //485测试
            CMD_ONLINE_TEST = 0x1C,             //联网测试
            CMD_4G_COMPATIBLE_2G = 0x1D,         //4G兼容2G
            CMD_GET_CHARGER_SN = 0x1E,          //获取桩编号
            CMD_GET_DEVICE_CODE = 0x1F,         //获取识别码
            CMD_START_AGING_TEST = 0x20,        //启动老化测试
            CMD_GET_AGING_TEST_RESULT = 0x21,   //获取老化结果


            CMD_FW_UPDATE_REQ,               //固件升级请求
            CMD_FW_SEND,                     //固件下发
            TestMode = 0x99
        };

        enum TEST_MODE
        {
            TEST_MODE_START = 0x00,
            TEST_MODE_STOP
        };

        //struct DelayTimesFlag
        //{
        //    public int MainBoardPowerDelayFlag;
        //    public int MainBoardLEDDelayFlag;
        //    public int MainBoardTrumpetDelayFlag;
        //    public int MainBoardRelayDelayFlag;
        //    public int MainBoardFlashDelayFlag;

        //}
        

        struct GetResult
        {
            public int      testMode;
            public int      testModeAllow;
            public int      key;
            public int[]    keyValue;
            public int      tapCard;
            public string   cardNum;
            public int      lcd;
            public int      _2G;
            public int      _2gCSQ;
            public string   _2G_Iccid;
            public int      trumpet;
            public int      relay;
            public int      measurementChip;
            public int[]    getPower;
            public int      SetCID;
            public int      SetPcbCode;
            public int      SetRegisterCode;
            public string   MainBoardCode;
            public string   InterfaceBoardCode;
            public int      BLE;
            public string   FwVersion;
            public UInt32   UsedTime_interface;
            public UInt32   UsedTime_main;
            public UInt32   UsedTime_Charger;
        };
        struct CountDownTime
        {
            public int testMode;
            public int key;
            public int tapCard;
            public int lcd;
            public int _2G;
            public int trumpet;
            public int relay;
            public int PowerSource;
            public int SetCID;
            public int SetPcbCode;
            public int BLE;
            //public int _2_4G;
            public int flash;
            public int setRtc;
            public int getRtc;

        };

        /*****************************************变量声明*******************************************/
        Dictionary<string, object> TestSettingInfo = new Dictionary<string, object>
        {
            {"ChargerModel","X6" },
            {"CountDown",30 },
            //{"CardNum", "A1000000" },
            {"CsqLowerLimit",20 },
            {"CsqUpperLimit",60 },
            {"PowerLowerLimit",0 },
            {"PowerUpperLimit",1000 },
        };

        public static List<byte> arraybuffer = new List<byte> { };

        //DelayTimesFlag MainBoardTimesDelayFlag = new DelayTimesFlag
        //{
         //   MainBoardPowerDelayFlag = 0,
         //   MainBoardLEDDelayFlag = 0,
         //   MainBoardTrumpetDelayFlag = 0,
         //   MainBoardRelayDelayFlag = 0,
         //   MainBoardFlashDelayFlag = 0,
        //};

        GetResult GetResultObj = new GetResult
        {
            testMode = -1,
            testModeAllow = -1,
            key = -1,
            keyValue = new int[12],
            tapCard = -1,
            lcd = -1,
            _2G = -1,
            _2gCSQ = -1,
            _2G_Iccid = "",
            trumpet = -1,
            relay = -1,
            measurementChip = -1,
            SetPcbCode = -1,
            BLE = -1,
            SetRegisterCode = -1,
            cardNum = "",
            getPower = new int[12],
            FwVersion = "",
            UsedTime_interface = 0,
            UsedTime_main = 0,
            UsedTime_Charger = 0,
            MainBoardCode = "",
            InterfaceBoardCode = ""
        };

        CountDownTime countDownTime_MB = new CountDownTime {
            testMode = 0,
            key = 0,
            tapCard = 0,
            lcd = 0,
            _2G = 0,
            PowerSource = 0,
            trumpet = 0,
            relay = 0,
            SetCID = 0,
            SetPcbCode = 0,
            BLE = 0,
            //_2_4G = 0,
            flash = 0,
            setRtc = 0,
            getRtc = 0
        };
        CountDownTime countDownTime_SB = new CountDownTime {
            testMode = 0,
            key = 0,
            tapCard = 0,
            lcd = 0,
            _2G = 0,
            PowerSource = 0,
            trumpet = 0,
            relay = 0,
            SetCID = 0,
            SetPcbCode = 0,
            BLE = 0,
            //_2_4G = 0,
            flash = 0,
            setRtc = 0,
            getRtc = 0
    };
        CountDownTime countDownTimeCharger = new CountDownTime {
            testMode = 0,
            key = 0,
            tapCard = 0,
            lcd = 0,
            _2G = 0,
            PowerSource = 0,
            trumpet = 0,
            relay = 0,
            SetCID = 0,
            SetPcbCode = 0,
            BLE = 0,
            //_2_4G = 0,
            flash = 0,
            setRtc = 0,
            getRtc = 0
        };

        static byte sequence = 0;
        public static bool MBTestingFlag = false;
        public static bool SBTestingFlag = false;
        public static bool ChargerTestingFlag = false;
        Thread MBTestThread;
        Thread SBTestThread;
        Thread ChargerTestThread;
        static int MBTabSelectIndex;
        static int SBTabSelectIndex;
        static int chargerTestSelectIndex;
        static int PreMBTabSelectIndex = 0;
        static int PreSBTabSelectIndex;
        static int PrechargerTestSelectIndex;
        static int TestMeunSelectIndex;
        static int PCBATestSelectIndex;

        Dictionary<string, string> MBTestResultDir = new Dictionary<string, string>();
        Dictionary<string, string> SBTestResultDir = new Dictionary<string, string>();
        Dictionary<string, string> ChargerTestResultDir = new Dictionary<string, string>();

        UInt32 RtcCount = 0;//定时器计数
        int tick = 0;
      //  bool MsgDebug = true;
        bool MsgDebug = false;
        string reportPath = @".\智能报表";
        bool OnlineFlg = false;
        bool onlineDectecFlag = false;
        Thread onlineDetectThread;
        public int OnlineDetectTime;
        /***************************************函数定义*********************************************/
        //串口接收
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int n = serialPort1.BytesToRead;
            byte[] buf = new byte[n];
            serialPort1.Read(buf, 0, n);
            
            arraybuffer.AddRange(buf);
            
            TestDataProc(arraybuffer.ToArray());
        }

        //串口发送
        private bool SendSerialData(byte[] data)
        {
            bool ret = false;

            try
            {
                if (serialPort1 != null)
                {
                    serialPort1.Write(data, 0, data.Length);

                    if (MsgDebug)
                    {
                        string send = "";
                        for (int j = 0; j < data.Length; j++)
                        {
                            send += data[j].ToString("X2") + " ";
                        }
                        LOG("Send: " + send);
                    }
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                LOG(ex.Message);
                ret = false;
            }
            return ret;
        }

        public void LOG(String text)
        {
            try
            {
                this.textBoxDebug.Invoke(
                    new MethodInvoker(delegate
                    {
                        this.textBoxDebug.AppendText(text + "\r\n");
                    }
                 )
                );

                this.textBoxDebugInfo.Invoke(
                    new MethodInvoker(delegate
                    {
                        this.textBoxDebugInfo.AppendText(text + "\r\n");
                    }
                 )
                );

                this.textBox_Log.Invoke(
                    new MethodInvoker(delegate
                    {
                        this.textBox_Log.AppendText(text + "\r\n");
                    }
                 )
                );

            }
            catch (Exception ex)
            {
                LOG(ex.Message);
                //MessageBox.Show(ex.Message);
            }
        }

        public void AgingTestLOG(String text)
        {
            try
            {
                this.textBoxDebug.Invoke(
                    new MethodInvoker(delegate
                    {
                        this.textBox_AgingLog.AppendText(text + "\r\n");
                    }
                 )
                );
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private static byte[] MakeSendArray(byte cmd, byte[] data)
        {
            UInt16 length;
            List<byte> list = new List<byte> { };
            byte[] srtDes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            list.Add(0xAA);
            list.Add(0x55);

            list.AddRange(srtDes);
            byte ver = 0x01;
            sequence++;

            if (data != null)
            {
                length = (UInt16)(1 + 1 + 1 + data.Length + 1);
            }
            else
            {
                length = 2;
            }

            list.Add((byte)(length));
            list.Add((byte)(length >> 8));
            list.Add(ver);
            list.Add(sequence);
            list.Add(cmd);
            if (data != null)
            {
                list.AddRange(data);
            }

            list.Add(ProcTestData.caculatedCRC(list.ToArray(), list.Count));

            return list.ToArray();
        }

        private void TestDataProc(byte[] data)
        {
            byte[] buf = new byte[data.Length];
            Array.Copy(data, buf, data.Length);

            try
            {
                if (buf.Length > 17)
                {
                    for (int i = 0; i < buf.Length; i++)
                    {
                        if (buf[i] == 0xAA && buf[i + 1] == 0x55)
                        {
                            int length = (buf[i + 12]) + (buf[i + 13] << 8) + 14;
                            if (buf.Length >= (length + i))
                            {
                                byte checkSum = buf[length + i - 1];
                                byte[] validFrame = new byte[length];

                                Array.Copy(buf, i, validFrame, 0, validFrame.Length);

                                byte calcCRC = ProcTestData.caculatedCRC(validFrame, validFrame.Length - 1);

                                if (MsgDebug)
                                {
                                    string receive = "";
                                    //LOG("接收到的n:" + ((UInt32)buf.Length).ToString());
                                    LOG("接收到的cmd:" + ((UInt32)validFrame[16]).ToString("X2"));
                                    for (int j = 0; j < validFrame.Length; j++)
                                    {
                                        receive += validFrame[j].ToString("X2") + " ";
                                    }
                                    LOG("Receive: " + receive);
                                }
                                
                                if (calcCRC == checkSum)
                                {     
                                    arraybuffer.Clear();

                                    byte cmd = validFrame[16];

                                    switch (cmd)
                                    {
                                        case (byte)Command.TestMode://测试模式请求
                                        {
                                            MessageTestModeHandle(validFrame);
                                        }
                                        break;

                                        case (byte)Command.CMD_KEY_TEST:
                                            break;

                                        case (byte)Command.CMD_CARD_TEST://刷卡
                                            MessageCardTestHandle(validFrame);
                                            break;

                                        case (byte)Command.CMD_LCD_TEST:
                                            GetResultObj.lcd = validFrame[17];
                                            break;

                                        case (byte)Command.CMD_2G_TEST:
                                            Message2GTestHandle(validFrame);
                                            break;

                                        case (byte)Command.CMD_TRUMPET_TEST:
                                            GetResultObj.trumpet = validFrame[17];
                                            break;

                                        case (byte)Command.CMD_RELAY_TEST://继电器
                                            MessageRelayTestHandle(validFrame);
                                            break;

                                        case (byte)Command.CMD_SET_PCB:
                                            GetResultObj.SetPcbCode = validFrame[18];
                                            if (validFrame[17] == 0x00)
                                            {
                                                if (GetResultObj.SetPcbCode == 0x00)
                                                {

                                                    LOG("主板编码设置成功");
                                                }
                                                else
                                                {
                                                    LOG("主板编码设置失败");
                                                }
                                            }
                                            else if (validFrame[18] == 0x01)
                                            {
                                                if (GetResultObj.SetPcbCode == 0x00)
                                                {
                                                    LOG("副板编码设置成功");
                                                }
                                                else
                                                {
                                                    LOG("副板编码设置失败");
                                                }
                                            }
                                            break;

                                        case (byte)Command.CMD_SET_SN:
                                            GetResultObj.SetCID = validFrame[17];
                                            getDeviceIDFlag = 1;
                                            break;

                                        case (byte)Command.CMD_BT_TEST://蓝牙测试
                                            getBlueToothFlag = 1;
                                            GetResultObj.BLE = validFrame[17];

                                            LOG("蓝牙接收数据:"+ GetResultObj.BLE.ToString("D2"));
                                            if (TestMeunSelectIndex == 0)//PCBA测试
                                            {
                                                /*
                                                if (PCBATestSelectIndex == 0)//主板测试
                                                {
                                                }
                                                else if (PCBATestSelectIndex == 1)//副板测试
                                                {
                                                    switch (GetResultObj.BLE)
                                                    {
                                                        case 0x01:
                                                            SBTestResultDir["蓝牙"] = "通过";
                                                            updateControlText(skinLabel_SB_BT_RESULT, "测试通过", Color.Green);
                                                            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
                                                            break;
                                                        case 0x00:
                                                            updateControlText(skinLabel_SB_BT_RESULT, "测试不通过", Color.Red);
                                                            SBTestResultDir["蓝牙"] = "不通过";
                                                            break;

                                                        case 0x02:
                                                            SBTestResultDir["蓝牙"] = "无";
                                                            updateControlText(skinLabel_SB_BT_RESULT, "此PCB不带蓝牙模块", Color.Black);
                                                            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
                                                            break;
                                                    }
                                                }
                                                */
                                            }
                                            else if (TestMeunSelectIndex == 1)//整机测试
                                            {
                                                switch (GetResultObj.BLE)
                                                {
                                                    case 0x01:
                                                        ChargerTestResultDir["蓝牙"] = "通过";
                                                        updateControlText(skinLabel_SB_BT_RESULT, "测试通过", Color.Green);
                                                        updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                                                        break;
                                                    case 0x00:
                                                        updateControlText(skinLabel_SB_BT_RESULT, "测试不通过", Color.Red);
                                                        ChargerTestResultDir["蓝牙"] = "不通过";
                                                        break;

                                                    case 0x02:
                                                        ChargerTestResultDir["蓝牙"] = "无";
                                                        updateControlText(skinLabel_SB_BT_RESULT, "此PCB不带蓝牙模块", Color.Black);
                                                        updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                                                        break;
                                                }
                                            }
                                            break;

                                        case (byte)Command.CMD_GET_FW:
                                            if (validFrame[17] == 0x00)
                                            {
                                                int fwVer = (int)((validFrame[18] << 8) | (validFrame[19]));
                                                int subver = validFrame[20];
                                                GetResultObj.FwVersion = fwVer + "." + subver;
                                            }
                                            else if (validFrame[17] == 0x01)
                                            {
                                                GetResultObj.FwVersion = validFrame[18].ToString("D3");
                                            }
                                            LOG("validFrame[17]:" + validFrame[17].ToString("D2"));
                                            LOG("validFrame[18]:" + validFrame[18].ToString("D2"));
                                            LOG("validFrame[19]:" + validFrame[19].ToString("D2"));
                                            LOG("validFrame[20]:" + validFrame[20].ToString("D2"));
                                            if (MBTestingFlag)
                                            {
                                                MBTestResultDir["软件版本"] = GetResultObj.FwVersion;
                                            }
                                            else if (SBTestingFlag)
                                            {
                                                SBTestResultDir["软件版本"] = GetResultObj.FwVersion;
                                            } 
                                            else if (ChargerTestingFlag)
                                            {
                                                if (validFrame[17] == 0)
                                                {
                                                    ChargerTestResultDir["软件版本"] = GetResultObj.FwVersion;
                                                }
                                                else
                                                {
                                                    ChargerTestResultDir["副板软件版本"] = GetResultObj.FwVersion;
                                                }

                                            }
                                            LOG("软件版本:" + GetResultObj.FwVersion);
                                            break;

                                        case (byte)Command.CMD_READ_PCB_CODE://读取PCB编号
                                            string str = "";
                                            bool isZero = true;
                                            for (int m = 0; m < 8; m++)
                                            {
                                                if (validFrame[18 + m] != 0x00 && isZero == true)
                                                {
                                                    isZero = false;
                                                }

                                                if (isZero == false)
                                                {
                                                    str += validFrame[18 + m].ToString("X2");
                                                }

                                            }
                                            if (validFrame[17] == 0)
                                            {

                                                GetResultObj.MainBoardCode = str;
                                                ChargerTestResultDir["主板编号"] = GetResultObj.MainBoardCode;
                                            }
                                            else if (validFrame[17] == 1)
                                            {
                                                GetResultObj.InterfaceBoardCode = str;
                                                ChargerTestResultDir["按键板编号"] = GetResultObj.InterfaceBoardCode;
                                            }
                                            break;
                                        case (byte)Command.CMD_SET_REGISTER_CODE:
                                            GetResultObj.SetRegisterCode = validFrame[17];
                                            break;
                                        case (byte)Command.CMD_SET_DEV_TYPE:
                                            break;
                                        case (byte)Command.CMD_SET_2_4G_GW_ADD:
                                            break;
                                        case (byte)Command.CMD_SET_TERMINAL_INFO:
                                            break;
                                        case (byte)Command.CMD_SET_SERVER_ADDR:
                                            break;
                                        case (byte)Command.CMD_SET_SERVER_PORT:
                                            break;
                                        case (byte)Command.CMD_SET_PRINT_SWITCH:
                                            break;
                                        case (byte)Command.CMD_REBOOT:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                LOG("设备重启成功");
                                            }
                                            else
                                            {
                                                LOG("设备重启失败");
                                            }
                                            break;
                                        case (byte)Command.CMD_SET_DEVICE_ID:
                                            break;
                                        case (byte)Command.CMD_SET_RTC:
                                            MessageSetRtcHandle(validFrame);
                                            break;
                                        case (byte)Command.CMD_GET_RTC:
                                            MessageGetRtctHandle(validFrame);
                                            break;
                                        case (byte)Command.CMD_FLASH_TEST:
                                            MessageFlashTestHandle(validFrame);
                                            break;
                                        case (byte)Command.CMD_LED_TEST:
                                            if (validFrame[17] == 0)
                                            {
                                                LOG("指示灯测试回复OK.");
                                            }
                                            break;
                                        case (byte)Command.CMD_ONLINE_TEST:
                                            if (validFrame[17] == 0)//成功
                                            {
                                                OnlineFlg = true;
                                                LOG("设备联网成功.");
                                                //onlineDectecFlag = false;
                                                updateControlText(skinButton_OnlineStartDetect, "开始检测");
                                                updateControlText(skinLabel_OnlineDetectResult, "联网成功", Color.Green);

                                            }
                                            else
                                            {
                                                LOG("设备联网中.");
                                            }
                                            break;
                                        case (byte)Command.CMD_GET_CHARGER_SN://获取桩号
                                            string sn = "";
                                            bool iszero = true;

                                            if (validFrame.Length < (17+8))
                                            {
                                                LOG("获取桩号接收数据不对!");
                                                break;
                                            }
                                            getSnFlag = 1;
                                            for (int m = 0; m < 8; m++)
                                            {
                                                if ((validFrame[17 + m] != 0x00) && (iszero == true))
                                                {
                                                    iszero = false;
                                                }

                                                if (iszero == false)
                                                {
                                                    sn += validFrame[17 + m].ToString("X2");
                                                }
                                            }
                                            if (TestMeunSelectIndex == 4)
                                            {
                                                LOG("桩号:" + sn);
                                                updateControlText(skinLabel_OnlineDetectStation, sn, Color.Green);
                                            }
                                            break;
                                        case (byte)Command.CMD_GET_DEVICE_CODE://获取识别码
                                            string devCode = "";
                                            int timesFlag = 0;
                                            //bool isZeroFlg = true;
                                            byte[] validFrameData = new byte[4];
                                            onlineDectecFlag = false;
                                            if (validFrame.Length < (17 + 8))
                                            {
                                                LOG("获取识别码接收数据不对!");
                                                break;
                                            }
                                            getDeviceCodeFlag = 1;
                                            
                                            for (int m = 0; m < 8; m ++)
                                            {
                                                if (validFrame[17 + m] > 0)
                                                {
                                                    timesFlag++;
                                                }
                                                if (timesFlag > 0)
                                                {
                                                    if ((1 == timesFlag) && (0x10 > validFrame[17 + m]))
                                                    {
                                                        devCode += validFrame[17 + m].ToString("X1");
                                                    }
                                                    else {
                                                        devCode += validFrame[17 + m].ToString("X2");
                                                    }
                                                }
                                            }
                                            if (TestMeunSelectIndex == 4)
                                            {
                                                LOG("识别码:" + devCode);
                                                updateControlText(skinLabel_OnlineDeviceCode, devCode, Color.Green);
                                            }
                                            break;
                                        case (byte)Command.CMD_START_AGING_TEST:
                                            if (validFrame[17] == 0)//成功
                                            {
                                                AgingTestLOG("老化测试开启成功.");
                                            }
                                            else
                                            {
                                                AgingTestLOG("老化测试开启失败.");
                                            }
                                            break;
                                        case (byte)Command.CMD_GET_AGING_TEST_RESULT:
                                            byte agingTestItem = validFrame[17];
                                            AgingTestLOG("老化测试项: " + agingTestItem);
                                            byte itemCmd;
                                            UInt16 successCnt;
                                            UInt16 failCnt;
                                            string agingTestResult = "";
                                            for (int item = 0; item < agingTestItem; item++)
                                            {
                                                itemCmd = validFrame[18 + 5 * item];
                                                successCnt = (UInt16)((validFrame[19 + 5 * item] << 8) | validFrame[20 + 5 * item]);
                                                failCnt = (UInt16)((validFrame[21 + 5 * item] << 8) | validFrame[22 + 5 * item]);
                                                switch (itemCmd)
                                                {
                                                    case (byte)Command.CMD_2G_TEST:
                                                        agingTestResult += "2G测试结果:\r\n" + "成功次数:" + successCnt.ToString() + "\r\n失败次数:" + failCnt.ToString() + "\r\n";
                                                        break;
                                                    case (byte)Command.CMD_FLASH_TEST:
                                                        agingTestResult += "FLASH测试结果:\r\n" + "成功次数:" + successCnt.ToString() + "\r\n失败次数:" + failCnt.ToString() + "\r\n";
                                                        break;
                                                    case (byte)Command.CMD_485_TEST:
                                                        agingTestResult += "485测试结果:\r\n" + "成功次数:" + successCnt.ToString() + "\r\n失败次数:" + failCnt.ToString() + "\r\n";
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            AgingTestLOG(agingTestResult);
                                            break;
                                        default:
                                            break;
                                    }

                                }
                                else
                                {
                                    LOG("recv data err");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG(ex.Message);
                //MessageBox.Show(ex.Message);
            }
        }

        int countdownTime;
        //UInt32 TestTime;
        //主板测试线程
        private void MainBoardTestProcess()
        {
             
            bool selectIndexUpgradeFlag = false;
            countdownTime = Convert.ToInt32(TestSettingInfo["CountDown"]);

            while (MBTestingFlag == true)
            {
                if (PreMBTabSelectIndex != MBTabSelectIndex)
                {
                    PreMBTabSelectIndex = MBTabSelectIndex;
                    
                    selectIndexUpgradeFlag = true;
                }

                switch (MBTabSelectIndex)
                {
                    case 0x00:
                        LOG("扫描主板二维码.");
                        break;
                    case 0x01://电源
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            //TestTime = GetRtcCount();
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.PowerSource = countdownTime;
                            MBTestResultDir["电源"] = "";
                            updateControlText(skinLabel_MB_POWER_RESULT, "");
                            LOG("检测电源是否正常.");
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("检测电源超时.");
                            MBTestResultDir["电源"] = "不通过";
                            updateControlText(skinLabel_MB_POWER_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;
                    case 0x02://指示灯
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            //TestTime = GetRtcCount();
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.lcd = countdownTime;
                            MBTestResultDir["指示灯"] = "";
                            updateControlText(skinLabel_MB_LED_RESULT, "");
                            LOG("指示灯测试.");
                            //发送指示灯测试指令
                            SendLedTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("指示灯测试超时.");
                            MBTestResultDir["指示灯"] = "不通过";
                            updateControlText(skinLabel_MB_LED_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }

                        break;
                    case 0x03://喇叭
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            //TestTime = GetRtcCount();
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.trumpet = countdownTime;
                            MBTestResultDir["喇叭"] = "";
                            updateControlText(skinLabel_MB_TRUMPT_RESULT, "");
                            LOG("喇叭测试.");
                            //发送喇叭测试指令
                            SendTrumptTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("喇叭测试超时.");
                            MBTestResultDir["喇叭"] = "不通过";
                            updateControlText(skinLabel_MB_TRUMPT_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;
                    case 0x04://继电器
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            //TestTime = GetRtcCount();
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.relay = countdownTime;
                            MBTestResultDir["继电器"] = "";
                            updateControlText(skinLabel_MB_RELAY_RESULT, "");
                            updateControlText(skinLabelRelayResult1, "");
                            updateControlText(skinLabelRelayResult2, "");
                            LOG("继电器测试.");
                            //发送继电器测试指令
                            SendRelayTestReq(0x00, 0x00);
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("继电器测试超时.");
                            MBTestResultDir["继电器"] = "不通过";
                            updateControlText(skinLabel_MB_RELAY_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;
                    case 0x05://flash
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            //TestTime = GetRtcCount();
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.flash = countdownTime;
                            MBTestResultDir["FLASH"] = "";
                            updateControlText(skinLabel_MB_FLASH_RESULT, "");
                            LOG("FLASH测试.");
                            //发送FLASH测试指令
                            SendFlashTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("FLASH测试超时.");
                            MBTestResultDir["FLASH"] = "不通过";
                            updateControlText(skinLabel_MB_FLASH_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        //if ((1 == MainBoardTimesDelayFlag.MainBoardFlashDelayFlag) && ((GetCurrentTimeStamp() - ItemTestTime) >= 2))
                        //{
                        //    updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        //}
                        break;
                    case 0x06://setRTC
                        if(selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            //TestTime = GetRtcCount();
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.setRtc = countdownTime;
                            MBTestResultDir["SETRTC"] = "";
                            updateControlText(skinLabel_MB_SETRTC_RESULT, "");
                            LOG("设置RTC时间.");
                            //发送设置RTC时间指令
                            SendSetRtcTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("设置RTC时间超时.");
                            MBTestResultDir["SETRTC"] = "不通过";
                            updateControlText(skinLabel_MB_SETRTC_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;
                    case 0x07://getRTC
                        if(selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            //TestTime = GetRtcCount();
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.getRtc = countdownTime;
                            MBTestResultDir["GETRTC"] = "";
                            updateControlText(skinLabel_MB_GETRTC_RESULT, "");
                            LOG("读取RTC时间.");
                            //发送读取RTC时间指令
                            SendGetRtcTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("读取RTC时间超时.");
                            MBTestResultDir["GETRTC"] = "不通过";
                            updateControlText(skinLabel_MB_GETRTC_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;
                    case 0x08://2G
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;

                            LOG("主板2G测试.");
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB._2G = countdownTime;
                            MBTestResultDir["2G"] = "";
                            MBTestResultDir["信号值"] = "";
                            updateControlText(skinLabel_MB_2G_RESULT, "");
                            Send2GTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("主板2G测试超时.");
                            MBTestResultDir["2G"] = "不通过";
                            updateControlText(skinLabel_MB_2G_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;
                    case 0x09://结束测试
                        SendGetFwVersionReq(0x00);
                        Thread.Sleep(100);
                        SendSetPcbCodeReq(0x00, textBox_MB_QRCode.Text.Trim());
                        Thread.Sleep(100);
                        GetResultObj.UsedTime_main = GetCurrentTimeStamp() - GetResultObj.UsedTime_main;
                        MBTestResultDir["测试用时"] = (GetResultObj.UsedTime_main / 60) + "分 " + ((GetResultObj.UsedTime_main) % 60) + "秒";
                        MBTestResultDir = ModifyResultData(MBTestResultDir);
                        LOG("结束测试\r\n用时:" + MBTestResultDir["测试用时"]);

                        ShowMainboardResult();
                        SendTestModeReq(0x01);

                        //写入excel表
                        ProcTestData.WriteReport(TestSettingInfo["ChargerModel"] + "_PCBA_主板.xlsx", TestSettingInfo["ChargerModel"] + "_PCBA_主板", MBTestResultDir);

                       
                        string mysqlCmd = ProcTestData.MainboardTestMysqlCommand(
                            TestSettingInfo["ChargerModel"].ToString(),
                            MBTestResultDir["PCB编号"],
                            MBTestResultDir["测试员"], 
                            MBTestResultDir["软件版本"],
                            MBTestResultDir["测试结果"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["电源"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["指示灯"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["喇叭"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["继电器"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["FLASH"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["SETRTC"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["GETRTC"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["测试时间"], 
                            GetResultObj.UsedTime_main
                            );

                        if (ProcTestData.SendMysqlCommand(mysqlCmd, true) == true)
                        {
                            LOG("主板测试记录添加数据库成功");
                            ProcTestData.DealBackUpData(ProcTestData.backupMysqlCmdFile);
                        }

                        updateControlText(textBox_MB_QRCode, "");
                        MBTestingFlag = false;

                        Thread.Sleep(100);
                        SendDevReboot();
                        break;
                    default:
                        break;
                }
                Thread.Sleep(200);
            }
        }

        //副板测试线程
        private void SubBoardTestProcess()
        {
            bool selectIndexUpgradeFlag = false;
            countdownTime = Convert.ToInt32(TestSettingInfo["CountDown"]);

            while (SBTestingFlag == true)
            {
                if (PreSBTabSelectIndex != SBTabSelectIndex)
                {
                    PreSBTabSelectIndex = SBTabSelectIndex;
                    selectIndexUpgradeFlag = true;
                }
                
                switch (SBTabSelectIndex)
                {
                    case 0x00:
                        LOG("扫描副板二维码.");
                        break;
                    case 0x01://蓝牙
                        if (selectIndexUpgradeFlag == true)
                        {
                            LOG("副板蓝牙测试.");
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_SB.BLE = countdownTime;
                            SBTestResultDir["蓝牙"] = "";
                            updateControlText(skinLabel_SB_BT_RESULT, ""); 
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("副板蓝牙测试超时.");
                            SBTestResultDir["蓝牙"] = "不通过";
                            updateControlText(skinLabel_SB_BT_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
                        }   
                        break;
                    case 0x02://2G
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;

                            LOG("副板2G测试.");
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_SB._2G = countdownTime;
                            SBTestResultDir["2G"] = "";
                            updateControlText(skinLabel_SB_2G_RESULT, "");
                            Send2GTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("副板2G测试超时.");
                            SBTestResultDir["2G"] = "不通过";
                            updateControlText(skinLabel_SB_2G_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
                        }
                        break;
                    case 0x03://结束测试
                        SendGetFwVersionReq(0x01);
                        Thread.Sleep(100);

                        GetResultObj.UsedTime_interface = GetCurrentTimeStamp() - GetResultObj.UsedTime_interface;
                        SBTestResultDir["测试用时"] = (GetResultObj.UsedTime_interface / 60) + "分 " + ((GetResultObj.UsedTime_interface) % 60) + "秒";
                        SBTestResultDir = ModifyResultData(SBTestResultDir);
                        LOG("结束测试\r\n用时:" + SBTestResultDir["测试用时"]);

                        ShowSubBoardResult();
                        SendTestModeReq(0x01);

                        //写入excel表
                        ProcTestData.WriteReport(TestSettingInfo["ChargerModel"] + "_PCBA_副板.xlsx", TestSettingInfo["ChargerModel"] + "_PCBA_副板", SBTestResultDir);
                        
                        string mysqlCmd = ProcTestData.SubBoardTestMysqlCommand(
                            TestSettingInfo["ChargerModel"].ToString(),
                            SBTestResultDir["PCB编号"],
                            SBTestResultDir["测试员"],
                            SBTestResultDir["软件版本"],
                            SBTestResultDir["测试结果"] == "通过" ? "Pass" : "Fail",
                            SBTestResultDir["蓝牙"] == "通过" ? "Pass" : (SBTestResultDir["蓝牙"] == "无" ? "Without" : "Fail"),
                            SBTestResultDir["2G"] == "通过" ? "Pass" : (SBTestResultDir["2G"] == "无" ? "Without" : "Fail"),
                            SBTestResultDir["测试时间"], 
                            GetResultObj.UsedTime_interface
                            );

                        if (ProcTestData.SendMysqlCommand(mysqlCmd, true) == true)
                        {
                            LOG("副板测试记录添加数据库成功");
                            ProcTestData.DealBackUpData(ProcTestData.backupMysqlCmdFile);
                        }

                        updateControlText(textBox_SB_QR, "");
                        SBTestingFlag = false;
                        break;
                   
                    default:
                        break;
                }

                Thread.Sleep(200);
            }
        }

        //整机测试线程
        private void ChargerTestProcess()
        {
            bool selectIndexUpgradeFlag = false;
            countdownTime = Convert.ToInt32(TestSettingInfo["CountDown"]);

            while (ChargerTestingFlag == true)
            {
                if(PrechargerTestSelectIndex != chargerTestSelectIndex)
                {
                    PrechargerTestSelectIndex = chargerTestSelectIndex;
                    selectIndexUpgradeFlag = true;
                }

                switch (chargerTestSelectIndex)
                {
                    case 0x00:
                        LOG("扫描电桩二维码.");
                        break;
                    case 0x01:
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTimeCharger.lcd = countdownTime;
                            ChargerTestResultDir["指示灯"] = "";
                            updateControlText(skinLabel_CHG_LED_RESULT, "");
                            LOG("整机指示灯测试.");
                            
                            SendLedTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("整机指示灯测试超时.");
                            ChargerTestResultDir["指示灯"] = "不通过";
                            updateControlText(skinLabel_CHG_LED_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                        }
                        break;
                    case 0x02:
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTimeCharger.trumpet = countdownTime;
                            ChargerTestResultDir["喇叭"] = "";
                            updateControlText(skinLabel_CHG_TRUMPT_RESULT, "");
                            LOG("整机喇叭测试.");

                            SendTrumptTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("整机喇叭测试超时.");
                            ChargerTestResultDir["喇叭"] = "不通过";
                            updateControlText(skinLabel_CHG_TRUMPT_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                        }
                        break;
                    case 0x03:
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTimeCharger.BLE = countdownTime;
                            ChargerTestResultDir["蓝牙"] = "";
                            updateControlText(skinLabel_CHG_BT_RESULT, "");
                            SendBlueToothTestReq();
                            LOG("整机蓝牙测试.");
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("整机蓝牙测试超时.");
                            ChargerTestResultDir["蓝牙"] = "不通过";
                            updateControlText(skinLabel_CHG_BT_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                        }
                        break;
                    case 0x04://2G测试
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                           
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTimeCharger._2G = countdownTime;
                            ChargerTestResultDir["2G"] = "";
                            updateControlText(skinLabel_CHG_2G_RESULT, "");
                            LOG("整机2G测试.");
                            Send2GTestReq();
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("整机2G测试超时.");
                            ChargerTestResultDir["2G"] = "不通过";
                            updateControlText(skinLabel_CHG_2G_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                        }
                        break;
                    case 0x05://继电器测试
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTimeCharger.relay = countdownTime;
                            ChargerTestResultDir["继电器"] = "";
                            updateControlText(skinLabel_CHG_RELAY1_POWER, "");
                            updateControlText(skinLabel_CHG_RELAY2_POWER, "");
                            updateControlText(skinLabel_CHG_RELAY_RESULT, "");
                            LOG("整机继电器测试.");
                            SendRelayTestReq(0x00, 0x00);
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("整机继电器测试超时.");
                            ChargerTestResultDir["继电器"] = "不通过";
                            updateControlText(skinLabel_CHG_RELAY_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                        }
                        break;

                    case 0x06://结束测试
                        SendGetFwVersionReq(0x00);
                        Thread.Sleep(100);
                        SendGetPcdCode(0x00);
                        Thread.Sleep(100);

                        GetResultObj.UsedTime_Charger = GetCurrentTimeStamp() - GetResultObj.UsedTime_Charger;
                        ChargerTestResultDir["测试用时"] = (GetResultObj.UsedTime_Charger / 60) + "分 " + ((GetResultObj.UsedTime_Charger) % 60) + "秒";
                        ChargerTestResultDir = ModifyResultData(ChargerTestResultDir);
                        LOG("结束测试\r\n用时:" + ChargerTestResultDir["测试用时"]);

                        ShowChgBoardResult();
                        if ((ChargerTestResultDir["测试结果"] == "通过") ||(ChargerTestResultDir["测试结果"] == "Pass"))
                        {
                            SendSetID(textBox_WholeChg_SN_QR.Text.Trim());
                        }
                        SendTestModeReq(0x01);

                        //写入excel表
                        ProcTestData.WriteReport(TestSettingInfo["ChargerModel"] + "_整机测试.xlsx", TestSettingInfo["ChargerModel"] + "_整机测试", ChargerTestResultDir);

                        //ProcTestData.WriteReport(TestSettingInfo["ChargerModel"] + "_整机测试.xlsx", TestSettingInfo["ChargerModel"] + "_整机测试", chargerTestData);
  
                        string cmd = ProcTestData.ChargerTestMysqlCommand (
                                TestSettingInfo["ChargerModel"].ToString(),
                                ChargerTestResultDir["电桩号"],
                                ChargerTestResultDir["测试员"],
                                ChargerTestResultDir["软件版本"],
                                ChargerTestResultDir["主板编号"],
                                ChargerTestResultDir["测试结果"] == "通过" ? "Pass" : "Fail",
                                ChargerTestResultDir["指示灯"] == "通过" ? "Pass" : "Fail",
                                ChargerTestResultDir["喇叭"] == "通过" ? "Pass" : "Fail",
                                ChargerTestResultDir["蓝牙"] == "通过" ? "Pass" : (ChargerTestResultDir["蓝牙"] == "无" ? "Without" : "Fail"),
                                ChargerTestResultDir["2G"] == "通过" ? "Pass" : (ChargerTestResultDir["2G"] == "无" ? "Without" : "Fail"),
                                ChargerTestResultDir["继电器"] == "通过" ? "Pass" : "Fail",
                                ChargerTestResultDir["测试时间"], 
                                GetResultObj.UsedTime_Charger 
                            );

                        if (ProcTestData.SendMysqlCommand(cmd, true) == true)
                        {
                            LOG("整机测试记录添加数据库成功");
                            ProcTestData.DealBackUpData(ProcTestData.backupMysqlCmdFile);
                        }
                        updateControlText(textBox_WholeChg_SN_QR, "");
                        ChargerTestingFlag = false;

                        Thread.Sleep(100);
                        SendDevReboot();
                        break;
     
                    default:
                        break;
                }

                Thread.Sleep(200);
            }
        }

        //发送测试请求
        private void SendTestModeReq(byte mode)
        {
            byte[] data = { mode };
            int wait = 0, n = 0;

            GetResultObj.testMode = -1;
            GetResultObj.testModeAllow = -1;
           
            SendSerialData(MakeSendArray((byte)Command.TestMode, data));
            
            while (GetResultObj.testMode == -1) {
                Thread.Sleep(500);
                if (wait++ > 10) {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.TestMode, data));
                }

                if (n > 3) {
                    break;
                }
            }

            if (n > 10) {
                if (MessageBox.Show((mode == 0) ? "请求开始失败！\r\n是否重试" : "请求结束失败！\r\n是否重试", "提示",
                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendTestModeReq(mode);
                }
            }
        }

        //发送按键测试指令0x01
        private void SendKeyTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_KEY_TEST, null));
        }

        //发送刷卡测试指令0x02
        private void SendCardTestReq()
        {
            GetResultObj.tapCard = -1;
            SendSerialData(MakeSendArray((byte)Command.CMD_CARD_TEST, null));
        }

        //发送刷卡测试指令0x13
        private void SendDevReboot()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_REBOOT, null));
        }

        //发送刷卡测试指令0x02
        private void SendLedTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_LED_TEST, null));
        }
        

        //发送喇叭测试指令0x05
        private void SendTrumptTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_TRUMPET_TEST, null));
        }

        //联网测试指令
        private void SendOnlineTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_ONLINE_TEST, null));
        }

        //获取识别码
        public int getDeviceCodeFlag = -1;
        private void SendGetDeviceCode()
        {
            getDeviceCodeFlag = -1;
            SendSerialData(MakeSendArray((byte)Command.CMD_GET_DEVICE_CODE, null));
            int wait = 0, n = 0;
            while (getDeviceCodeFlag == -1)
            {
                Thread.Sleep(200);
                if (wait++ > 10)
                {
                    wait = 0;
                    SendSerialData(MakeSendArray((byte)Command.CMD_GET_DEVICE_CODE, null));
                    n++;
                }
                if (n > 3)
                {
                    break;
                }
            }

            if (n > 3)
            {
                if (MessageBox.Show("获取识别码失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendGetDeviceCode();
                }
            }
        }
        

        //获取桩号
        public int getSnFlag = -1;
        private void SendGetID()
        {
            int wait = 0, n = 0;
            getSnFlag = -1;
            SendSerialData(MakeSendArray((byte)Command.CMD_GET_CHARGER_SN, null));
            
            while (getSnFlag == -1)
            {
                Thread.Sleep(200);
                if (wait++ > 10)
                {
                    wait = 0;
                    SendSerialData(MakeSendArray((byte)Command.CMD_GET_CHARGER_SN, null));
                    n++;
                }
                if (n > 3)
                {
                    break;
                }
            }

            if (n > 3)
            {
                if (MessageBox.Show("获取桩号失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendGetID();
                }
            }
        }

        //发送继电器测试指令0x06
        private void SendRelayTestReq(byte operate, byte ch)
        {
            byte[] data = { operate, ch };
            GetResultObj.relay = -1;
            SendSerialData(MakeSendArray((byte)Command.CMD_RELAY_TEST, data));
        }

        //发送Flash测试指令0x17
        private void SendFlashTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_FLASH_TEST, null));
        }

        //发送设置Rtc指令0x15
        private void SendSetRtcTestReq()
        {
            DateTime dt = DateTime.UtcNow;
            byte[] data = new byte[4];

            UInt32 currentUtc = GetCurrentTimeStamp();
            LOG("currentUtc:" + currentUtc.ToString());

            data[0] = (byte)((currentUtc >> 24) & 0xFF);
            data[1] = (byte)((currentUtc >> 16) & 0xFF);
            data[2] = (byte)((currentUtc >> 8) & 0xFF);
            data[3] = (byte)((currentUtc >> 0) & 0xFF);

            SendSerialData(MakeSendArray((byte)Command.CMD_SET_RTC, data));
        }

        public int getDeviceIDFlag = -1;
        //发送设置桩号指令0x08
        private void SendSetID(string id)
        {
            string str = ProcTestData.fillString(id, 16, '0', 0);
            byte[] data = ProcTestData.stringToBCD(str);

            SendSerialData(MakeSendArray((byte)Command.CMD_SET_SN, data));
            int wait = 0, n = 0;
            while (getDeviceIDFlag == -1)
            {
                Thread.Sleep(30);
                if (wait++ > 10)
                {
                    wait = 0;
                    SendSerialData(MakeSendArray((byte)Command.CMD_SET_SN, data));
                    n++;
                }
                if (n > 10)
                {
                    break;
                }
            }

            if (n > 10)
            {
                if (MessageBox.Show("桩号设置失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendSetID(id);
                }
            }
        }

        public int getBlueToothFlag = -1;
        //发送读取蓝牙指令0x09
        private void SendBlueToothTestReq()
        {
            int wait = 0, n = 0;
            SendSerialData(MakeSendArray((byte)Command.CMD_BT_TEST, null));
            
            while (getBlueToothFlag == -1)
            {
                Thread.Sleep(30);
                if (wait++ > 10)
                {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.CMD_BT_TEST, null));
                }
                if (n > 10)
                {
                    break;
                }

            }
        }

        //发送读取2G信号指令0x04
        private void Send2GTestReq()
        {
            GetResultObj._2G = -1;
            SendSerialData(MakeSendArray((byte)Command.CMD_2G_TEST, null));
            /*
            int wait = 0, n = 0;

            while (GetResultObj.tapCard == -1)
            {
                Thread.Sleep(30);
                if (wait++ > 10)
                {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.CMD_2G_TEST, null));
                }
                if (n > 10)
                {
                    break;
                }

            }
            */
        }

        //发送读取Rtc指令0x16
        private void SendGetRtcTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_GET_RTC, null));
        }

        private void SendGetFwVersionReq(byte operate)
        {
            byte[] data = { operate };
            GetResultObj.FwVersion = "";
            SendSerialData(MakeSendArray((byte)Command.CMD_GET_FW, data));
            int waittime = 0, n = 0;
            while (GetResultObj.FwVersion == "")
            {
                Thread.Sleep(300);
                waittime++;
                if (waittime > 10)
                {
                    n++;
                    waittime = 0;
                    SendSerialData(MakeSendArray((byte)Command.CMD_GET_FW, data));
                }
                if (n > 3)
                {
                    break;
                }
            }
            if (n > 3)
            {
                if (MessageBox.Show("获取PCB软件版本失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendGetFwVersionReq(operate);
                }
            }
        }

        private void SendSetPcbCodeReq(byte type, string code)
        {
            List<byte> data = new List<byte>();
            data.Add(type);
            string str = ProcTestData.fillString(code, 16, '0', 0);
            data.AddRange(ProcTestData.stringToBCD(str));
            GetResultObj.SetPcbCode = -1;
            SendSerialData(MakeSendArray((byte)Command.CMD_SET_PCB, data.ToArray()));
            int wait = 0, n = 0;
            while (GetResultObj.SetPcbCode == -1)
            {
                Thread.Sleep(300);
                if (wait++ > 10)
                {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.CMD_SET_PCB, data.ToArray()));

                }
                if (n > 3)
                {
                    break;
                }

            }

            if (n > 10)
            {
                if (MessageBox.Show("PCB编号设置失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendSetPcbCodeReq(type, code);
                }
            }
        }

        private void SendGetPcdCode(byte operate)
        {
            byte[] data = { operate };
            SendSerialData(MakeSendArray((byte)Command.CMD_READ_PCB_CODE, data));
            int waittime = 0, n = 0;

            if (operate == 0)
            {
                GetResultObj.MainBoardCode = "";
                SendSerialData(MakeSendArray((byte)Command.CMD_READ_PCB_CODE, data));
                while (GetResultObj.MainBoardCode == "")
                {
                    Thread.Sleep(300);
                    waittime++;
                    if (waittime > 10)
                    {
                        n++;
                        waittime = 0;
                        SendSerialData(MakeSendArray((byte)Command.CMD_READ_PCB_CODE, data));
                    }
                    if (n > 3)
                    {
                        break;
                    }
                }
            }
            else if (operate == 1)
            {
                GetResultObj.InterfaceBoardCode = "";
                SendSerialData(MakeSendArray((byte)Command.CMD_READ_PCB_CODE, data));
                while ((GetResultObj.InterfaceBoardCode == ""))
                {
                    Thread.Sleep(300);
                    waittime++;
                    if (waittime > 10)
                    {
                        n++;
                        waittime = 0;
                        SendSerialData(MakeSendArray((byte)Command.CMD_READ_PCB_CODE, data));
                    }
                    if (n > 3)
                    {
                        break;
                    }
                }
            }

            if (n > 3)
            {
                if (MessageBox.Show("获取PCB编号失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendGetPcdCode(operate);
                }
            }
        }

        //开启老化测试指令
        private void SendStartAgingTestReq()
        {
            AgingTestLOG("老化测试请求.");
            SendSerialData(MakeSendArray((byte)Command.CMD_START_AGING_TEST, null));
        }

        //获取老化测试指令
        private void SendGetAgingTestResultReq()
        {
            AgingTestLOG("获取老化测试结果请求.");
            SendSerialData(MakeSendArray((byte)Command.CMD_GET_AGING_TEST_RESULT, null));
        }

        public UInt32 ItemTestTime=0;
        //测试模式命令消息处理
        private void MessageTestModeHandle(byte[] pkt)
        {
            GetResultObj.testMode = pkt[17];
            GetResultObj.testModeAllow = pkt[18];

            MBTestingFlag = false;
            SBTestingFlag = false;
            ChargerTestingFlag = false;
           
                if (TestMeunSelectIndex == 0) { //PCBA测试
                if (PCBATestSelectIndex == 0) { //主板测试
                    if (GetResultObj.testMode == 0x00)//开始测试ack
                    {
                        if (GetResultObj.testModeAllow == 0x00)//成功
                        {
                            LOG("主板请求开始测试成功.");
                            updateControlText(skinButton_PCBA_STARTTEST, "结束测试");
                            MBTestingFlag = true;
                            MBTabSelectIndex=1;       
                            updateTableSelectedIndex(skinTabControl_MB, MBTabSelectIndex);

                            DateTime now = DateTime.Now;
                            MBTestResultDir.Clear();
                            MBTestResultDir.Add("PCB编号", textBox_MB_QRCode.Text.Trim());
                            MBTestResultDir.Add("测试员", ProcTestData.PresentAccount);
                            MBTestResultDir.Add("软件版本", "");
                            MBTestResultDir.Add("测试结果", "");
                            MBTestResultDir.Add("电源", "");
                            MBTestResultDir.Add("指示灯", "");
                            //MBTestResultDir.Add("卡号", "00000000");
                            MBTestResultDir.Add("喇叭", "");
                            MBTestResultDir.Add("继电器", "");
                            MBTestResultDir.Add("FLASH", "");
                            MBTestResultDir.Add("SETRTC", "");
                            MBTestResultDir.Add("GETRTC", "");
                            MBTestResultDir.Add("2G", "");
                            MBTestResultDir.Add("信号值", "");
                            MBTestResultDir.Add("测试时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            MBTestResultDir.Add("测试用时", "0");

                            GetResultObj.UsedTime_main = GetCurrentTimeStamp();
                            

                            if (MBTestThread != null)
                            {
                                MBTestThread.Abort();
                                MBTestThread = null;
                            }
                            MBTestThread = new Thread(MainBoardTestProcess);
                            MBTestThread.Start();
                        } else {
                            LOG("主板请求开始测试失败.");
                            updateControlText(skinButton_PCBA_STARTTEST, "开始测试");
                            MBTestingFlag = false;
                        }
                    } else {//结束测试ack
                        LOG("主板请求结束测试成功.");
                        MBTestingFlag = false;
                        updateControlText(skinButton_PCBA_STARTTEST, "开始测试");
                    }
                }
                else if (PCBATestSelectIndex == 1)                                      //副板测试
                {
                    if (GetResultObj.testMode == 0x00)//副板开始测试请求回复
                    {
                        if (GetResultObj.testModeAllow == 0x00)//成功
                        {
                            LOG("副板请求开始测试成功.");
                            updateControlText(skinButton_PCBA_STARTTEST, "结束测试");
                            SBTestingFlag = true;

                            DateTime now = DateTime.Now;
                            SBTestResultDir.Clear();
                            SBTestResultDir.Add("PCB编号", textBox_SB_QR.Text.Trim());
                            SBTestResultDir.Add("测试员", ProcTestData.PresentAccount);
                            SBTestResultDir.Add("软件版本", "");
                            SBTestResultDir.Add("测试结果", "");
                            //SBTestResultDir.Add("卡号", "00000000");
                            SBTestResultDir.Add("蓝牙", "");
                            SBTestResultDir.Add("2G", "");
                            SBTestResultDir.Add("测试时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            SBTestResultDir.Add("测试用时", "0");

                            GetResultObj.UsedTime_interface = GetCurrentTimeStamp();
                            SBTabSelectIndex=1;
                            updateTableSelectedIndex(skinTabControl_SB, SBTabSelectIndex);

                            if (SBTestThread != null)
                            {
                                SBTestThread.Abort();
                                SBTestThread = null;
                            }

                            SBTestThread = new Thread(SubBoardTestProcess);
                            SBTestThread.Start();
                        }
                        else
                        {
                            LOG("副板请求开始测试失败.");
                            updateControlText(skinButton_PCBA_STARTTEST, "开始测试");
                            SBTestingFlag = false;
                        }
                    }
                    else//副板结束测试请求回复
                    {
                        LOG("副板请求结束测试成功...");
                        SBTestingFlag = false;
                        updateControlText(skinButton_PCBA_STARTTEST, "开始测试");
                    }
                }
            }
            else if (TestMeunSelectIndex == 1)                                              //整机测试
            {
                if (GetResultObj.testMode == 0x00)//整机开始测试请求回复
                {
                    if (GetResultObj.testModeAllow == 0x00)//成功
                    {
                        LOG("整机请求开始测试成功.");
                        updateControlText(skinButton_WholeChg_StartTest, "结束测试");
                        ChargerTestingFlag = true;         

                        DateTime now = DateTime.Now;
                        ChargerTestResultDir.Clear();
                        ChargerTestResultDir.Add("电桩号", textBox_WholeChg_SN_QR.Text.Trim());
                        ChargerTestResultDir.Add("主板编号", "");
                        ChargerTestResultDir.Add("软件版本", "");
                        ChargerTestResultDir.Add("测试员", ProcTestData.PresentAccount);
                        ChargerTestResultDir.Add("测试结果", "");
                        ChargerTestResultDir.Add("指示灯", "");
                        ChargerTestResultDir.Add("喇叭", "");
                        ChargerTestResultDir.Add("蓝牙", "");
                        ChargerTestResultDir.Add("2G", "");
                        ChargerTestResultDir.Add("继电器", "");
                        ChargerTestResultDir.Add("测试时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        ChargerTestResultDir.Add("测试用时", "0");

                        GetResultObj.UsedTime_Charger = GetCurrentTimeStamp();
                        chargerTestSelectIndex=1;
                        updateTableSelectedIndex(skinTabControl_WholeChg, chargerTestSelectIndex);

                        if (ChargerTestThread != null)
                        {
                            ChargerTestThread.Abort();
                            ChargerTestThread = null;
                        }
                        ChargerTestThread = new Thread(ChargerTestProcess);
                        ChargerTestThread.Start();
                    }
                    else
                    {
                        LOG("整机请求开始测试失败.");
                        updateControlText(skinButton_WholeChg_StartTest, "开始测试");
                        ChargerTestingFlag = false;
                    }
                }
                else//整机结束测试请求回复
                {
                    LOG("整机请求结束测试成功...");
                    ChargerTestingFlag = false;
                    updateControlText(skinButton_WholeChg_StartTest, "开始测试");
                }

            }    
        }

        //按键测试消息处理
        private void MessageKeyTestHandle(byte[] pkt)
        {
            if (TestMeunSelectIndex == 0)//PCBA测试
            {
                if (PCBATestSelectIndex == 0)//主板测试
                {
                }
                else if (PCBATestSelectIndex == 1)//副板测试
                {

                }
            }
            else if (TestMeunSelectIndex == 1)//整机测试
            {

            }
        }

        //刷卡测试消息处理
        private void MessageCardTestHandle(byte[] pkt)
        {
      
        }

        //指示灯测试消息处理
        private void MessageLcdTestHandle(byte[] pkt)
        {
            if (TestMeunSelectIndex == 0)//PCBA测试
            {
                if (PCBATestSelectIndex == 0)//主板测试
                {
                    if (pkt[17] == 0x00)//成功
                    {

                    }
                }
                else if (PCBATestSelectIndex == 1)//副板测试
                {

                }
            }
            else if (TestMeunSelectIndex == 1)//整机测试
            {

            }
        }

        //2G测试消息处理
        private void Message2GTestHandle(byte[] pkt)
        {
            GetResultObj._2G = pkt[17];
            GetResultObj._2gCSQ = pkt[18];
            GetResultObj._2G_Iccid = Encoding.ASCII.GetString(pkt, 19, 2);

            LOG("信号值:" + GetResultObj._2gCSQ);
            if (GetResultObj._2G_Iccid.IndexOf('\0') >= 0)
            {
                GetResultObj._2G_Iccid = GetResultObj._2G_Iccid.Remove(GetResultObj._2G_Iccid.IndexOf('\0'));
            }

            if (TestMeunSelectIndex == 0)//PCBA测试
            {
                if (PCBATestSelectIndex == 0)//主板测试
                {
                    if (GetResultObj._2G == 0x00)//通过
                    {
                        //加入信号值判断
                        if ((GetResultObj._2gCSQ >= Convert.ToByte(TestSettingInfo["CsqLowerLimit"])
                            && GetResultObj._2gCSQ <= Convert.ToByte(TestSettingInfo["CsqUpperLimit"]))
                            && (GetResultObj._2G_Iccid != null))
                        {
                            updateControlText(skinLabel_MB_2G_RESULT, "通过\r\n信号值:" + GetResultObj._2gCSQ, Color.Green);
                            MBTestResultDir["2G"] = "通过";
                            MBTestResultDir["信号值"] = GetResultObj._2gCSQ.ToString();
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                            LOG("主板2G模块通过.");
                        }
                        else
                        {
                            updateControlText(skinLabel_MB_2G_RESULT, "不通过\r\n信号值:" + GetResultObj._2gCSQ, Color.Red);
                            MBTestResultDir["2G"] = "不通过";
                            MBTestResultDir["信号值"] = GetResultObj._2gCSQ.ToString();
                            LOG("主板2G模块不通过.");
                        }
                    }
                }
                else if (PCBATestSelectIndex == 1)//副板测试
                {
                    if (GetResultObj._2G == 0x00)//通过
                    {
                        //加入信号值判断
                        if ((GetResultObj._2gCSQ >= Convert.ToByte(TestSettingInfo["CsqLowerLimit"])
                            && GetResultObj._2gCSQ <= Convert.ToByte(TestSettingInfo["CsqUpperLimit"]))
                            && (GetResultObj._2G_Iccid != null))
                        {
                            updateControlText(skinLabel_SB_2G_RESULT, "通过\r\n信号值:" + GetResultObj._2gCSQ, Color.Green);
                            SBTestResultDir["2G模块"] = "通过";
                            SBTestResultDir["2G"] = "通过";
                            SBTestResultDir["信号值"] = GetResultObj._2gCSQ.ToString();
                            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
                            LOG("副板2G模块通过.");
                        }
                        else
                        {
                            updateControlText(skinLabel_SB_2G_RESULT, "不通过\r\n信号值:" + GetResultObj._2gCSQ, Color.Red);
                            SBTestResultDir["2G模块"] = "不通过";
                            SBTestResultDir["2G"] = "不通过";
                            SBTestResultDir["信号值"] = GetResultObj._2gCSQ.ToString();
                            LOG("副板2G模块不通过.");
                        }
                    }
                    else
                    {
                        updateControlText(skinLabel_SB_2G_RESULT, "不通过", Color.Red);
                        SBTestResultDir["2G模块"] = "不通过";
                        SBTestResultDir["2G"] = "不通过";
                        SBTestResultDir["信号值"] = GetResultObj._2gCSQ.ToString();
                        LOG("副板2G模块不通过11.");
                    }
                }
            }
            else if (TestMeunSelectIndex == 1)//整机测试
            {
                if (GetResultObj._2G == 0x00)//通过
                {
                    //加入信号值判断
                    if ((GetResultObj._2gCSQ >= Convert.ToByte(TestSettingInfo["CsqLowerLimit"])
                        && GetResultObj._2gCSQ <= Convert.ToByte(TestSettingInfo["CsqUpperLimit"]))
                        && (GetResultObj._2G_Iccid != null))
                    {
                        updateControlText(skinLabel_CHG_2G_RESULT, "通过\r\n信号值:" + GetResultObj._2gCSQ, Color.Green);
                        ChargerTestResultDir["2G模块"] = "通过";
                        ChargerTestResultDir["2G"] = "通过";
                        ChargerTestResultDir["信号值"] = GetResultObj._2gCSQ.ToString();
                        updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                        ChargerTestResultDir["ICCID"] = GetResultObj._2G_Iccid;
                        LOG("整机测试2G模块通过.");
                    }
                    else
                    {
                        updateControlText(skinLabel_CHG_2G_RESULT, "不通过\r\n信号值:" + GetResultObj._2gCSQ, Color.Red);
                        ChargerTestResultDir["2G模块"] = "不通过";
                        ChargerTestResultDir["2G"] = "不通过";
                        ChargerTestResultDir["信号值"] = GetResultObj._2gCSQ.ToString();
                        ChargerTestResultDir["ICCID"] = "00000000000";
                        LOG("整机测试2G模块不通过.");
                    }
                }
                else
                {
                    updateControlText(skinLabel_CHG_2G_RESULT, "不通过", Color.Red);
                    ChargerTestResultDir["2G模块"] = "不通过";
                    ChargerTestResultDir["2G"] = "不通过";
                    ChargerTestResultDir["信号值"] = GetResultObj._2gCSQ.ToString();
                    ChargerTestResultDir["ICCID"] = "00000000000";
                    LOG("整机测试2G模块不通过11.");
                }
            }
            else if (TestMeunSelectIndex == 4)
            {
                if (GetResultObj._2G == 0x00)
                {
                    updateControlText(skinLabel_OnlineDecCsqVal, GetResultObj._2gCSQ.ToString(), Color.Green);
                }
            }
        }

        //继电器测试消息处理
        private void MessageRelayTestHandle(byte[] pkt)
        {
            GetResultObj.relay = pkt[17];
           
            int operate = pkt[17];//继电器动作
            byte ch = pkt[18];    //第N路 
            string power1 = "";
            string power2 = "";
            string resultStr = "";
            bool pass = false;

            int Relay1_Power = ((pkt[19] << 8) | (pkt[20])) / 10;
            int Relay2_Power = ((pkt[21] << 8) | (pkt[22])) / 10;

            GetResultObj.getPower[ch] = Relay1_Power;
            GetResultObj.getPower[ch + 1] = Relay2_Power;

            power1 += GetResultObj.getPower[ch] + "W";
            power2 += GetResultObj.getPower[ch + 1] + "W";

            updateControlText(skinLabelRelayResult1, power1);
            updateControlText(skinLabelRelayResult2, power2);

            updateControlText(skinLabel_CHG_RELAY1_POWER, power1);
            updateControlText(skinLabel_CHG_RELAY2_POWER, power2);

            resultStr = "第";
            for (int j = 0; j < 2; j++)
            {
                //LOG("jjjjj:" + j);
                //LOG("GetResultObj.getPower[j]:" + GetResultObj.getPower[j]);
                //LOG("PowerLowerLimit:" + Convert.ToInt32(TestSettingInfo["PowerLowerLimit"]));
                //LOG("PowerUpperLimi:" + Convert.ToInt32(TestSettingInfo["PowerUpperLimit"]));
                if (GetResultObj.getPower[j] < Convert.ToInt32(TestSettingInfo["PowerLowerLimit"]) || GetResultObj.getPower[j] > Convert.ToInt32(TestSettingInfo["PowerUpperLimit"]))
                {
                    resultStr += (j + 1) + " ";
                    pass = false;
                }
                else {
                    pass = true;
                }
            }
            resultStr += "路功率不正常";

            if (TestMeunSelectIndex == 0)//PCBA测试
            {
                if (PCBATestSelectIndex == 0)//主板测试
                {
                    if (pass)
                    {
                        LOG("继电器测试pass,功率正常.");
                        MBTestResultDir["继电器"] = "通过";   
                        updateControlText(skinLabel_MB_RELAY_RESULT, "测试通过", Color.Green);
                        updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                    }
                    else
                    {
                        LOG("继电器测试失败,功率不正常.");
                        MBTestResultDir["继电器"] = "不通过";
                        updateControlText(skinLabel_MB_RELAY_RESULT, "测试不通过,"+ resultStr, Color.Red);
                    }
                }
            }
            else if (TestMeunSelectIndex == 1)//整机测试
            {
                if (pass)
                {
                    LOG("整机测试,继电器测试成功,功率正常.");
                    ChargerTestResultDir["继电器"] = "通过";
                    updateControlText(skinLabel_CHG_RELAY_RESULT, "测试通过", Color.Green);
                    updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
                }
                else
                {
                    LOG("整机测试,继电器测试失败,功率不正常.");
                    ChargerTestResultDir["继电器"] = "不通过";
                    updateControlText(skinLabel_CHG_RELAY_RESULT, "测试不通过," + resultStr, Color.Red);
                }
            }
        }

        //FLASH测试消息处理
        private void MessageFlashTestHandle(byte[] pkt)
        {
            if (TestMeunSelectIndex == 0)//PCBA测试
            {
                if (PCBATestSelectIndex == 0)//主板测试
                {
                    if (pkt[17] == 0x00)//成功
                    {
                        LOG("FLASH测试成功.");
                        MBTestResultDir["FLASH"] = "通过";
                        updateControlText(skinLabel_MB_FLASH_RESULT, "通过", Color.Green);
                        ////MainBoardTimesDelayFlag.MainBoardFlashDelayFlag = 1;
                        updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                    }
                    else
                    {
                        LOG("FLASH测试失败.");
                        MBTestResultDir["FLASH"] = "不通过";
                        updateControlText(skinLabel_MB_FLASH_RESULT, "不通过", Color.Red);
                        //updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                    }
                }
            }
            else if (TestMeunSelectIndex == 1)//整机测试
            {

            }
        }

        //设置RTC时间消息处理
        private void MessageSetRtcHandle(byte[] pkt)
        {
            if (TestMeunSelectIndex == 0)//PCBA测试
            {
                if (PCBATestSelectIndex == 0)//主板测试
                {
                    if (pkt[17] == 0x00)//成功
                    {
                        LOG("设置RTC时间成功.");
                        MBTestResultDir["SETRTC"] = "通过";
                        updateControlText(skinLabel_MB_SETRTC_RESULT, "通过", Color.Green);
                        updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                    }
                    else
                    {
                        LOG("设置RTC时间失败.");
                        MBTestResultDir["SETRTC"] = "不通过";
                        updateControlText(skinLabel_MB_SETRTC_RESULT, "不通过", Color.Red);
                        //updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                    }
                }
            }
            else if (TestMeunSelectIndex == 1)//整机测试
            {

            }
        }

        //读取RTC时间消息处理
        private void MessageGetRtctHandle(byte[] pkt)           //liutao
        {
            if (TestMeunSelectIndex == 0)//PCBA测试
            {
                if (PCBATestSelectIndex == 0)//主板测试
                {

                    UInt32 stationRtcCount = (UInt32)((pkt[17] << 24) | (pkt[18]<<16) | (pkt[19] << 8) | pkt[20]);
                    UInt32 currentCount = GetCurrentTimeStamp();
                    UInt32 tmpCount = 0;
                    
                    LOG("读取RTC时间成功," + stationRtcCount.ToString());
                    if (currentCount > stationRtcCount)
                    {
                        tmpCount = (currentCount - stationRtcCount) % 60;
                        LOG("RTC差值:" + tmpCount.ToString());
                        if (tmpCount < 20)
                        {
                            LOG("RTC校验OK.");
                            MBTestResultDir["GETRTC"] = "通过";
                            updateControlText(skinLabel_MB_GETRTC_RESULT, "通过", Color.Green);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        else
                        {
                            LOG("RTC校验err.");
                            MBTestResultDir["GETRTC"] = "不通过";
                            updateControlText(skinLabel_MB_GETRTC_RESULT, "不通过", Color.Red);
                        }
                    }
                    else
                    {
                        tmpCount = (stationRtcCount - currentCount) % 60;
                        LOG("RTC差值:" + tmpCount.ToString());
                        if (tmpCount < 20)
                        { 
                            LOG("RTC校验OK.");
                            MBTestResultDir["GETRTC"] = "通过";
                            updateControlText(skinLabel_MB_GETRTC_RESULT, "通过", Color.Green);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        else
                        {
                            LOG("RTC校验err.");
                            MBTestResultDir["GETRTC"] = "不通过";
                            updateControlText(skinLabel_MB_GETRTC_RESULT, "不通过", Color.Red);
                        }
                    }
                }
            }
            else if (TestMeunSelectIndex == 1)//整机测试
            {

            }
        }


        //打开串口按钮监听
        private void skinButtonOpenSerial_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialPort1.IsOpen)
                {
                    serialPort1.BaudRate = int.Parse(skinComboBox_SerialBuateSelect.SelectedItem.ToString());
                    serialPort1.PortName = skinComboBox_SerialPortSelect.SelectedItem.ToString();
                    serialPort1.Open();
                    if (serialPort1.IsOpen)
                    {
                        skinButtonOpenSerial.Text = "关闭串口";
                    }
                }
                else
                {
                    serialPort1.Close();

                    if (!serialPort1.IsOpen)
                    {
                        skinButtonOpenSerial.Text = "打开串口";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //选择串口端口号
        private void skinComboBox_SerialPortSelect_DropDown(object sender, EventArgs e)
        {
            try
            {
                skinComboBox_SerialPortSelect.Items.Clear();

                foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                {//获取多少串口
                    skinComboBox_SerialPortSelect.Items.Add(s);
                }

                if (skinComboBox_SerialPortSelect.Items.Count > 0)
                {
                    skinComboBox_SerialPortSelect.SelectedIndex = 0;
                    skinComboBox_SerialBuateSelect.SelectedIndex = 0;
                }
                else
                {
                    skinComboBox_SerialPortSelect.Text = "";
                }
            }
            catch (Exception ex)
            {
                LOG(ex.Message);
                //MessageBox.Show(ex.Message, "提示");
            }
        }

        //波特率索引更新
        private void skinComboBox_SerialBuateSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.BaudRate = int.Parse(skinComboBox_SerialBuateSelect.SelectedItem.ToString());
        }


        //主板PCB二维码ENTER键监听
        private void textBox_MB_QRCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                skinButton_MB_Confirm_Click(sender, e);
            }
        }

        //主板PCB确认键
        private void skinButton_MB_Confirm_Click(object sender, EventArgs e)
        {
            skinButton_PCBA_STARTTEST_Click(sender, e);
        }


        public static UInt32 GetCurrentTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToUInt32(ts.TotalSeconds);
        }

        //PCBA开始测试
        private void skinButton_PCBA_STARTTEST_Click(object sender, EventArgs e)
        {
            if ((textBox_MB_QRCode.Text == "" && skinTabControl_PCBATest.SelectedTab == skinTabPage_MainBoard)
                    || (textBox_SB_QR.Text == "" && skinTabControl_PCBATest.SelectedTab == skinTabPage_SubBoard))
            {
                MessageBox.Show("PCB编码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox_MB_QRCode.Text = "";
                textBox_SB_QR.Text = "";
                TestSettingInfo["ChargerModel"] = skinComboBox_ChgType.SelectedItem;
                return;
            }

            PCBATestSelectIndex = skinTabControl_PCBATest.SelectedIndex;

            if (skinTabControl_PCBATest.SelectedIndex == 0)//主板
            {
                if (MBTestingFlag == false)
                {
                    LOG("主板请求开始测试.");
                    SendTestModeReq((byte)TEST_MODE.TEST_MODE_START);
                }
                else
                {
                    LOG("主板请求结束测试.");
                    SendTestModeReq((byte)TEST_MODE.TEST_MODE_STOP);
                }
            }
            else if (skinTabControl_PCBATest.SelectedIndex == 1) //副板
            {
                if (SBTestingFlag == false)
                {
                    LOG("副板请求开始测试.");
                    SendTestModeReq((byte)TEST_MODE.TEST_MODE_START);
                }
                else
                {
                    LOG("副板请求结束测试.");
                    SendTestModeReq((byte)TEST_MODE.TEST_MODE_STOP);
                }
            }
        }

        //PCBA智能报表
        private void skinButton_PCBA_REPORTDIR_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", reportPath);
            }
            catch (Exception ex)
            {
                LOG(ex.Message);
            }
        }

        //PCBA日志清除
        private void skinButton_PCBA_CLEAR_LOG_Click(object sender, EventArgs e)
        {
            textBoxDebug.Text = "";
        }

        //主板电源测试成功
        private void skinButton_MB_Power_Success_Click(object sender, EventArgs e)
        {
            LOG("主板检测电源成功.");
            MBTestResultDir["电源"] = "通过";
            updateControlText(skinLabel_MB_POWER_RESULT, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        //主板电源测试失败
        private void skinButton_MB_Power_Fail_Click(object sender, EventArgs e)
        {
            LOG("主板检测电源失败.");
            MBTestResultDir["电源"] = "不通过";
            updateControlText(skinLabel_MB_POWER_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        //主板电源测试跳过
        private void skinButton_MB_Power_Over_Click(object sender, EventArgs e)
        {
            LOG("跳过主板检测电源.");
            MBTestResultDir["电源"] = "跳过";
            updateControlText(skinLabel_MB_POWER_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        //主板电源重新测试
        private void skinButton_MB_Power_rTest_Click(object sender, EventArgs e)
        {

        }

        private void skinButton_MB_LED_SUCCESS_Click(object sender, EventArgs e)
        {
            LOG("主板检测指示灯成功.");
            MBTestResultDir["指示灯"] = "通过";
            updateControlText(skinLabel_MB_LED_RESULT, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_LED_FALI_Click(object sender, EventArgs e)
        {
            LOG("主板检测指示灯失败.");
            MBTestResultDir["指示灯"] = "不通过";
            updateControlText(skinLabel_MB_LED_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_LED_OVER_Click(object sender, EventArgs e)
        {
            LOG("主板跳过检测指示灯.");
            //MBTestResultDir["指示灯"] = "跳过";
            updateControlText(skinLabel_MB_LED_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_LED_RTEST_Click(object sender, EventArgs e)
        {
            countDownTime_MB.lcd = countdownTime;
            MBTestResultDir["指示灯"] = "";
            updateControlText(skinLabel_MB_LED_RESULT, "");
            LOG("主板指示灯重新测试.");
            //发送指示灯测试指令
            SendLedTestReq();
        }

        private void skinButton_MB_TRUMPT_SUCCESS_Click(object sender, EventArgs e)
        {
            LOG("主板喇叭测试成功.");
            MBTestResultDir["喇叭"] = "通过";
            updateControlText(skinLabel_MB_TRUMPT_RESULT, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_TRUMPT_FAIL_Click(object sender, EventArgs e)
        {
            LOG("主板喇叭测试失败.");
            MBTestResultDir["喇叭"] = "不通过";
            updateControlText(skinLabel_MB_TRUMPT_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_TRUMPT_SKIP_Click(object sender, EventArgs e)
        {
            LOG("跳过主板喇叭测试.");
            //MBTestResultDir["喇叭"] = "跳过";
            updateControlText(skinLabel_MB_TRUMPT_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_TRUMPT_RTEST_Click(object sender, EventArgs e)
        {
            countDownTime_MB.trumpet = countdownTime;
            MBTestResultDir["喇叭"] = "";
            updateControlText(skinLabel_MB_TRUMPT_RESULT, "");
            LOG("主板喇叭重新测试.");
            //发送喇叭测试指令
            SendTrumptTestReq();
        }

        private void skinButtonRelaySkip_Click(object sender, EventArgs e)
        {
            LOG("跳过主板继电器测试.");
            //MBTestResultDir["继电器"] = "跳过";
            updateControlText(skinLabel_MB_RELAY_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButtonRelayReTest_Click(object sender, EventArgs e)
        {
            countDownTime_MB.relay = countdownTime;
            MBTestResultDir["继电器"] = "";
            updateControlText(skinLabel_MB_RELAY_RESULT, "");
            LOG("主板继电器重新测试.");
            //发送继电器测试指令
            SendRelayTestReq(0x00, 0x00);
        }

        private void skinButton_MB_FLASH_SKIP_Click(object sender, EventArgs e)
        {
            LOG("跳过主板FLASH测试.");
            //MBTestResultDir["Flash"] = "跳过";
            updateControlText(skinLabel_MB_FLASH_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);

        }

        private void skinButton_MB_FLASH_RTEST_Click(object sender, EventArgs e)
        {
            countDownTime_MB.flash = countdownTime;
            MBTestResultDir["FLASH"] = "";
            updateControlText(skinLabel_MB_FLASH_RESULT, "");
            LOG("主板FLASH重新测试.");
            //发送FLASH测试指令
            SendFlashTestReq();
        }

        private void skinButton_MB_SETRTC_SKIP_Click(object sender, EventArgs e)
        {
            LOG("跳过主板设置RTC时间.");
            //MBTestResultDir["setRtc"] = "跳过";
            updateControlText(skinLabel_MB_SETRTC_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_SETRTC_RTEST_Click(object sender, EventArgs e)
        {
            countDownTime_MB.setRtc = countdownTime;
            MBTestResultDir["SETRTC"] = "";
            updateControlText(skinLabel_MB_SETRTC_RESULT, "");
            LOG("主板重新设置RTC时间.");
            //发送设置RTC时间指令
            SendSetRtcTestReq();
        }

        private void skinButton_MB_GETRTC_SKIP_Click(object sender, EventArgs e)
        {
            LOG("跳过主板读取RTC时间.");
            //MBTestResultDir["GETRTC"] = "跳过";
            updateControlText(skinLabel_MB_GETRTC_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_GETRTC_RTEST_Click(object sender, EventArgs e)
        {
            countDownTime_MB.getRtc = countdownTime;
            MBTestResultDir["GETRTC"] = "";
            updateControlText(skinLabel_MB_GETRTC_RESULT, "");
            LOG("主板重新读取RTC时间.");
            //发送读取RTC时间指令
            SendGetRtcTestReq();
        }

        private void skinButton_MB_2G_SKIP_TIME_Click(object sender, EventArgs e)
        {
            LOG("跳过主板2G测试.");
            updateControlText(skinLabel_MB_2G_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_2G_RTEST_Click(object sender, EventArgs e)
        {
            LOG("主板2G重新测试.");
            ItemTestTime = GetCurrentTimeStamp();
            countDownTime_MB._2G = countdownTime;
            MBTestResultDir["2G"] = "";
            MBTestResultDir["信号值"] = "";
            updateControlText(skinLabel_MB_2G_RESULT, "");
            Send2GTestReq();
        }

        //副板扫描二维码按enter键开始测试
        private void textBox_SB_QR_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                skinButton_SB_CONFIRM_Click(sender, e);
            }
        }

        private void skinButton_SB_CONFIRM_Click(object sender, EventArgs e)
        {
            skinButton_PCBA_STARTTEST_Click(sender, e);
        }

        private void skinButton_SB_BT_SUCCESS_Click(object sender, EventArgs e)
        {
            LOG("副板蓝牙测试成功.");
            SBTestResultDir["蓝牙"] = "通过";
            updateControlText(skinLabel_SB_BT_RESULT, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
        }

        private void skinButton_SB_BT_FAIL_Click(object sender, EventArgs e)
        {
            LOG("副板蓝牙测试失败.");
            SBTestResultDir["蓝牙"] = "不通过";
            updateControlText(skinLabel_SB_BT_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
        }

        private void skinButton_SB_BT_OVER_Click(object sender, EventArgs e)
        {
            LOG("副板跳过蓝牙测试.");
            updateControlText(skinLabel_SB_BT_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
        }

        private void skinButton_SB_BT_RTEST_Click(object sender, EventArgs e)
        {
            LOG("副板蓝牙重新测试.");
            countDownTime_SB.BLE = countdownTime;


        }

        private void skinButton_SB_2G_OVER_Click(object sender, EventArgs e)
        {
            LOG("跳过副板2G测试.");
            updateControlText(skinLabel_SB_2G_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_SB, ++SBTabSelectIndex);
        }
        private void skinButton_SB_2G_RTEST_Click(object sender, EventArgs e)
        {
            LOG("副板2G重新测试");
            countDownTime_SB._2G = countdownTime;

            Send2GTestReq();
        }

        //保存测试项设置
        private void skinButton_SaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                TestSettingInfo["ChargerModel"] = skinComboBox_ChgType.SelectedItem;
                TestSettingInfo["CountDown"] = skinNumericUpDown_TestOverTime.Value;
                //TestSettingInfo["CardNum"] = textBox_TestCardNum.Text;
                TestSettingInfo["CsqLowerLimit"] = skinNumericUpDown_CSQLowerLimit.Value;
                TestSettingInfo["CsqUpperLimit"] = skinNumericUpDown_CSQUpperLimit.Value;
                TestSettingInfo["PowerLowerLimit"] = skinNumericUpDown_PowerLowerLimit.Value;
                TestSettingInfo["PowerUpperLimit"] = skinNumericUpDown_PowerUpperLimit.Value;
                ProcTestData.WriteConfig(ProcTestData.testConfigFile, TestSettingInfo);             
                MessageBox.Show("保存成功", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }

        private void textBox_WholeChg_SN_QR_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                skinButton_WholeChg_StartTest_Click(sender, e);
            }
        }

        private void skinButton_WholeChg_SN_Confirm_Click(object sender, EventArgs e)
        {
            skinButton_WholeChg_StartTest_Click(sender, e);
        }

        private void skinButton_WholeChg_Led_Success_Click(object sender, EventArgs e)
        {
            try
            {
                LOG("整机指示灯测试成功.");
                ChargerTestResultDir["指示灯"] = "通过";
                updateControlText(skinLabel_CHG_LED_RESULT, "通过", Color.Green);
                updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
            }
            catch (Exception ex)
            {
                //LOG(ex.Message);
            }
        }

        private void skinButton_WholeChg_Led_Fail_Click(object sender, EventArgs e)
        {
            LOG("整机指示灯测试失败.");
            ChargerTestResultDir["指示灯"] = "不通过";
            updateControlText(skinLabel_CHG_LED_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_WholeChg_Led_Over_Click(object sender, EventArgs e)
        {
            LOG("跳过整机指示灯测试.");
            updateControlText(skinLabel_CHG_LED_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_WholeChg_Led_RTest_Click(object sender, EventArgs e)
        {
            countDownTimeCharger.lcd = countdownTime;
            ChargerTestResultDir["指示灯"] = "";
            updateControlText(skinLabel_CHG_LED_RESULT, "");
            LOG("整机指示灯重新测试.");

            SendLedTestReq();
        }
        
        private void skinButton_CHG_TRUMPT_SUCCESS_Click(object sender, EventArgs e)
        {
            LOG("整机喇叭测试成功.");
            ChargerTestResultDir["喇叭"] = "通过";
            updateControlText(skinLabel_CHG_TRUMPT_RESULT, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_CHG_TRUMPT_FAIL_Click(object sender, EventArgs e)
        {
            LOG("整机喇叭测试失败.");
            ChargerTestResultDir["喇叭"] = "不通过";
            updateControlText(skinLabel_CHG_TRUMPT_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_CHG_TRUMPT_SKIP_Click(object sender, EventArgs e)
        {
            LOG("跳过整机喇叭测试.");
            updateControlText(skinLabel_CHG_TRUMPT_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_CHG_TRUMPT_RTEST_Click(object sender, EventArgs e)
        {
            countDownTimeCharger.trumpet = countdownTime;
            ChargerTestResultDir["喇叭"] = "";
            updateControlText(skinLabel_CHG_TRUMPT_RESULT, "");
            LOG("整机喇叭重新测试.");

            SendTrumptTestReq();
        }

        private void skinButton_WholeChg_BT_Success_Click(object sender, EventArgs e)
        {
            LOG("整机蓝牙测试成功.");
            ChargerTestResultDir["蓝牙"] = "通过";
            updateControlText(skinLabel_CHG_BT_RESULT, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_WholeChg_BT_Fail_Click(object sender, EventArgs e)
        {
            LOG("整机蓝牙测试失败.");
            ChargerTestResultDir["蓝牙"] = "不通过";
            updateControlText(skinLabel_CHG_BT_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_WholeChg_BT_Over_Click(object sender, EventArgs e)
        {
            LOG("跳过整机蓝牙测试.");
            updateControlText(skinLabel_CHG_BT_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_WholeChg_BT_RTest_Click(object sender, EventArgs e)
        {
            countDownTimeCharger.BLE = countdownTime;
            ChargerTestResultDir["蓝牙"] = "";
            updateControlText(skinLabel_CHG_BT_RESULT, "");
            LOG("蓝牙重新测试.");

            SendBlueToothTestReq();
        }

        private void skinButton_WholeChg_2G_Over_Click(object sender, EventArgs e)
        {
            LOG("跳过整机2G测试.");
            updateControlText(skinLabel_CHG_2G_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_WholeChg_2G_RTest_Click(object sender, EventArgs e)
        {
            countDownTimeCharger._2G = countdownTime;
            ChargerTestResultDir["2G"] = "";
            updateControlText(skinLabel_CHG_2G_RESULT, "");
            LOG("整机2G重新测试.");

            Send2GTestReq();
        }
        
        private void skinButton_CHG_RELAY_SKIP_Click(object sender, EventArgs e)
        {
            LOG("跳过整机继电器测试.");
            updateControlText(skinLabel_CHG_RELAY_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_WholeChg, ++chargerTestSelectIndex);
        }

        private void skinButton_CHG_RELAY_RTEST_Click(object sender, EventArgs e)
        {
            countDownTimeCharger.relay = countdownTime;
            ChargerTestResultDir["继电器"] = "";
            updateControlText(skinLabel_CHG_RELAY_RESULT, "");
            LOG("整机继电器测试.");

            SendRelayTestReq(0x00, 0x00);
        }

        //菜单测试项索引更新监听
        private void skinTabControl_TestMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            MBTestingFlag = false;
            SBTestingFlag = false;
            ChargerTestingFlag = false;
            TestMeunSelectIndex = skinTabControl_TestMenu.SelectedIndex;
            //onlineDectecFlag = false;
            //skinButton_OnlineStartDetect.Text = "开始检测";
            switch (skinTabControl_TestMenu.SelectedIndex)
            {
                case 0://PCBA测试
                    skinTabControl_PCBATest.SelectedIndex = 0;
                    skinTabControl_MB.SelectedIndex = 0;
                    textBox_MB_QRCode.Focus();
                    updateControlText(textBox_MB_QRCode, "");
                    updateControlText(textBox_SB_QR, "");
                    break;
                case 1://整机测试         
                    chargerTestSelectIndex = 0;
                    skinTabControl_WholeChg.SelectedIndex = 0;
                    textBox_WholeChg_SN_QR.Focus();
                    updateControlText(textBox_WholeChg_SN_QR, "");
                    break;
                case 2://测试设置
                    skinComboBox_SerialPortSelect.Focus();

                    break;
                case 3://二维码检测
                       // textBoxChargerIdQrCode.Focus();       //liutao
                    break;
                default:
                    break;

            }
        }

        //PCBA测试项索引监听
        private void skinTabControl_PCBATest_SelectedIndexChanged(object sender, EventArgs e)
        {
            PCBATestSelectIndex = skinTabControl_PCBATest.SelectedIndex;
            updateControlText(textBox_MB_QRCode, "");
            updateControlText(textBox_SB_QR, "");
            MBTestingFlag = false;
            SBTestingFlag = false;
            MBTabSelectIndex = 0;
            SBTabSelectIndex = 0;
            switch (skinTabControl_PCBATest.SelectedIndex)
            {
                case 0://主板
                    skinTabControl_MB.SelectedIndex = 0;
                    textBox_MB_QRCode.Focus();
                    break;

                case 1://副板
                    skinTabControl_SB.SelectedIndex = 0;
                    textBox_SB_QR.Focus();
                    break;
                default:
                    break;

            }
        }

        //主板测试项索引更新监听
        private void skinTabControl_MB_SelectedIndexChanged(object sender, EventArgs e)
        {
            MBTabSelectIndex = skinTabControl_MB.SelectedIndex;
            if (skinTabControl_MB.SelectedIndex == 0) {
                textBox_MB_QRCode.Focus();
            } 
        }

        //副板测试项索引更新监听
        private void skinTabControl_SB_SelectedIndexChanged(object sender, EventArgs e)
        {
            SBTabSelectIndex = skinTabControl_SB.SelectedIndex;
            switch (skinTabControl_SB.SelectedIndex)
            {
                case 0:
                    textBox_SB_QR.Focus();
                    break;
                default:
                    break;
            }

        }

        //整机测试项索引更新监听
        private void skinTabControl_WholeChg_SelectedIndexChanged(object sender, EventArgs e)
        {
            chargerTestSelectIndex = skinTabControl_WholeChg.SelectedIndex;
            if (chargerTestSelectIndex == 0) {
                textBox_WholeChg_SN_QR.Focus();
            }
        }

        private void skinTabControl_TestMenu_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if ((getPresentTabPage(skinTabControl_TestMenu) == skinTabPage_PCBATest)
                || (getPresentTabPage(skinTabControl_TestMenu) == skinTabPage_WholeChgTest)
                || (getPresentTabPage(skinTabControl_TestMenu) == skinTabPage_QRCode_Detect)
                || (getPresentTabPage(skinTabControl_TestMenu) == skinTabPage_OnlineTest))
            {

                if (serialPort1.IsOpen == false)
                {
                    MessageBox.Show("请先打开串口", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }

            }
        }

        private void skinButton_WholeChg_StartTest_Click(object sender, EventArgs e)
        {
            if (textBox_WholeChg_SN_QR.Text == "" )
            {
                MessageBox.Show("桩号不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox_WholeChg_SN_QR.Text = "";
                return;
            }

            if (textBox_WholeChg_SN_QR.Text.IndexOf(ProcTestData.X10QrCodeUrl) == 0)
            {
                //LOG("ProcTestData.X10QrCodeUrl.Length:0x"+ ProcTestData.X10QrCodeUrl.Length.ToString("X2"));
                //LOG("textBox_WholeChg_SN_QR.Text:" + textBox_WholeChg_SN_QR.Text);
                textBox_WholeChg_SN_QR.Text = textBox_WholeChg_SN_QR.Text.Remove(0, ProcTestData.X10QrCodeUrl.Length);
                //LOG("textBox_WholeChg_SN_QR.TextA:" + textBox_WholeChg_SN_QR.Text);
                System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^\d+$");

                if (rex.IsMatch(textBox_WholeChg_SN_QR.Text) == false)
                {
                    MessageBox.Show("桩号包含非数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox_WholeChg_SN_QR.Text = "";
                    return;
                }
            }
            else
            {
                MessageBox.Show("二维码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox_WholeChg_SN_QR.Text = "";
                return;
            }
            
            TestSettingInfo["ChargerModel"] = skinComboBox_ChgType.SelectedItem;
            if (ChargerTestingFlag == false)
            {
                LOG("整机请求开始测试.");
                SendTestModeReq((byte)TEST_MODE.TEST_MODE_START);
            }
            else
            {
                LOG("整机请求结束测试.");
                SendTestModeReq((byte)TEST_MODE.TEST_MODE_STOP);
            }
        }

        private void skinButton_WholeChg_ReportDir_Click(object sender, EventArgs e)
        {
            skinButton_PCBA_REPORTDIR_Click(sender, e);
        }

        private void skinButton_WholeChg_ClearLog_Click(object sender, EventArgs e)
        {
            textBoxDebugInfo.Text = "";
        }

        //更新控件的文字内容
        private void updateControlText(Control control, string text)
        {
            try
            {
                control.Invoke(
                new MethodInvoker(delegate {
                    control.Text = text;
                }
              )
           );
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                LOG(ex.Message);
            }
        }

        //更新控件的文字内容及颜色
        private void updateControlText(Control control, string text, Color color)
        {
            try
            {
                control.Invoke(
                new MethodInvoker(delegate {

                    control.Text = text;
                    control.ForeColor = color;
                }
              )
           );
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                LOG(ex.Message);
            }

        }

        //倒计时显示
        int ItemCountDown(int time, Label label, TabControl tabControl, TabPage tabPage)
        {
            if (time > 0)
            {
                time--;
                updateControlText(label, time.ToString("D2"));
                if (time == 0)
                {
                    if (tabControl.SelectedTab == tabPage)
                    {
                        tabControl.SelectedIndex++;
                    }

                }
            }
            return time;
        }

        //定时器处理函数
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tick++ >= 10)
            {
                RtcCount++;
                tick = 0;
                //LOG("Tick.");
                if (MBTestingFlag)//测试倒计时
                {
                    countDownTime_MB.PowerSource = ItemCountDown(countDownTime_MB.PowerSource, skinLabel_MB_PowerTimeCountDown, skinTabControl_MB, skinTabPage_MainBoard_Power);
                    countDownTime_MB.lcd = ItemCountDown(countDownTime_MB.lcd, skinLabel_MB_LED_TIMECOUNTDOWN, skinTabControl_MB, skinTabPage_MainBoard_Led);
                    //countDownTime_MB.tapCard = ItemCountDown(countDownTime_MB.tapCard, skinLabel_MB__CARD_TIMECOUNTDOWN, skinTabControl_MB, skinTabPage_MainBoard_CARD);
                    countDownTime_MB.trumpet = ItemCountDown(countDownTime_MB.trumpet, skinLabel_MB_TRUMPT_TIME, skinTabControl_MB, skinTabPage_MB_TRUMPT);
                    countDownTime_MB.relay = ItemCountDown(countDownTime_MB.relay, skinLabel_MB_Relay_TIME, skinTabControl_MB, skinTabPage_MB_RELAY);
                    countDownTime_MB.flash = ItemCountDown(countDownTime_MB.flash, skinLabel_MB_FLASH_TIME, skinTabControl_MB, skinTabPage_MB_FLASH);
                    countDownTime_MB.setRtc = ItemCountDown(countDownTime_MB.setRtc, skinLabel_MB_SETRTC_TIME, skinTabControl_MB, skinTabPage_GET_RTC);
                    countDownTime_MB.getRtc = ItemCountDown(countDownTime_MB.getRtc, skinLabel_MB_GETRTC_TIME, skinTabControl_MB, skinTabPage_MB_SET_RTC);
                    countDownTime_MB._2G = ItemCountDown(countDownTime_MB._2G, skinLabel_MB_2G_TIME, skinTabControl_MB, skinTabPage_MB_2G);
                }
                else if (SBTestingFlag)
                {
                    //countDownTime_SB.tapCard = ItemCountDown(countDownTime_SB.tapCard, skinLabel_SB_CARD_TIME, skinTabControl_SB, skinTabPage_SB_CARD);
                    countDownTime_SB.BLE = ItemCountDown(countDownTime_SB.BLE, skinLabel_SB_BT_TIME, skinTabControl_SB, skinTabPage_SB_BLUETOOTH);
                    countDownTime_SB._2G = ItemCountDown(countDownTime_SB._2G, skinLabel_SB_24G_TIME, skinTabControl_SB, skinTabPage_SB_2POINT4G);
                }
                else if (ChargerTestingFlag)
                {
                    countDownTimeCharger.lcd = ItemCountDown(countDownTimeCharger.lcd, skinLabel_WholeChg_Led_Time, skinTabControl_WholeChg, skinTabPage_WholeChg_Led);
                    //countDownTimeCharger.tapCard = ItemCountDown(countDownTimeCharger.tapCard, skinLabel_CHG_CARD_TIME, skinTabControl_WholeChg, skinTabPage_CHG_CARD);
                    countDownTimeCharger.trumpet = ItemCountDown(countDownTimeCharger.trumpet, skinLabel_CHG_TRUMPT_TIME, skinTabControl_WholeChg, skinTabPage_CHG_TRUMPT);
                    countDownTimeCharger.BLE = ItemCountDown(countDownTimeCharger.BLE, skinLabel_WholeChg_BT_Time, skinTabControl_WholeChg, skinTabPage_WholeChg_Bt);
                    countDownTimeCharger._2G = ItemCountDown(countDownTimeCharger._2G, skinLabel_WholeChg_2G_Time, skinTabControl_WholeChg, skinTabPage_WholeChg_2G);
                    countDownTimeCharger.relay = ItemCountDown(countDownTimeCharger.relay, skinLabel_CHG_RELAY_TIME, skinTabControl_WholeChg, skinTabPage_CHG_RELAY);
                }
                else if (onlineDectecFlag)
                {
                    OnlineDetectTime = ItemCountDown(OnlineDetectTime, skinLabel_OnlineDetectTime, skinTabControl_TestMenu, skinTabPage_OnlineTest);
                }
            }
        }

        private UInt32 GetRtcCount()
        {
            return RtcCount;
        }

        //获取当前tab控件的页
        private TabPage getPresentTabPage(TabControl tabControl)
        {
            TabPage tabPage = null;
            try
            {
                tabControl.Invoke(
                new MethodInvoker(delegate
                {
                    tabPage = tabControl.SelectedTab;
                }));
            }
            catch (Exception ex)
            {

                LOG(ex.Message);
            }
            return tabPage;
        }

        //更新table控件的索引
        private void updateTableSelectedIndex(TabControl tabControl, int index)
        {
            try
            {
                tabControl.Invoke(
                new MethodInvoker(delegate {

                    tabControl.SelectedIndex = index;
                }
              )
           );
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                LOG(ex.Message);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            skinTabControl_TestMenu.SelectTab(skinTabPage_CurrentUser);
            skinTabPage_CurrentUser.Text = "用户:" + ProcTestData.PresentAccount;
            if (ProcTestData.PresentAccount == "Admin")
            {
                skinButton_AccountSettings.Visible = true;
            }
            else
            {
                skinButton_AccountSettings.Visible = false;
            }

            TestSettingInfo = ProcTestData.ReadConfig(ProcTestData.testConfigFile, TestSettingInfo);

            skinComboBox_ChgType.SelectedItem = TestSettingInfo["ChargerModel"];
            skinComboBox_ChgType.SelectedIndex = 0;
            skinNumericUpDown_TestOverTime.Value = Convert.ToDecimal(TestSettingInfo["CountDown"]);
            //textBox_TestCardNum.Text = TestSettingInfo["CardNum"].ToString();
            skinNumericUpDown_CSQLowerLimit.Value = Convert.ToDecimal(TestSettingInfo["CsqLowerLimit"]);
            skinNumericUpDown_CSQUpperLimit.Value = Convert.ToDecimal(TestSettingInfo["CsqUpperLimit"]);
            skinNumericUpDown_PowerLowerLimit.Value = Convert.ToDecimal(TestSettingInfo["PowerLowerLimit"]);
            skinNumericUpDown_PowerUpperLimit.Value = Convert.ToDecimal(TestSettingInfo["PowerUpperLimit"]);

            timer1.Enabled = true;
            timer1.Start();

            try
            {
                if (Directory.Exists(reportPath) == false)
                {
                    Directory.CreateDirectory(reportPath);
                }

                //添加串口项目  
                foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                {//获取有多少个COM口  
                   skinComboBox_SerialPortSelect.Items.Add(s);
                }
                if (skinComboBox_SerialPortSelect.Items.Count > 0)
                {
                    skinComboBox_SerialPortSelect.SelectedIndex = 0;
                    skinComboBox_SerialBuateSelect.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            ProcTestData.DealBackUpData(ProcTestData.backupMysqlCmdFile);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (MBTestThread != null)
                {
                    if (MBTestThread.IsAlive)
                    {
                        MBTestThread.Abort();
                    }
                }

                if (SBTestThread != null)
                {
                    if (SBTestThread.IsAlive)
                    {
                        SBTestThread.Abort();
                    }
                }

                if (ChargerTestThread != null)
                {
                    if (ChargerTestThread.IsAlive)
                    {
                        ChargerTestThread.Abort();
                    }
                }

                if (onlineDetectThread != null)
                {
                    if (onlineDetectThread.IsAlive)
                    {
                        onlineDetectThread.Abort();
                    }
                }

                this.Dispose();
                this.Close(); 
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Application.Exit();
            }
        }

        private Color decideColor(string text)
        {

            switch (text)
            {
                case "通过":
                    return Color.Green;

                case "不通过":
                    return Color.Red;

                default:
                    return Color.Black;

            }
        }

        private void ShowMainboardResult()
        {
            updateControlText(MB_PCB_RESULT_VAL, MBTestResultDir["PCB编号"], Color.Black);
            updateControlText(MB_TESTOR_RESULT_VAL, MBTestResultDir["测试员"], Color.Black);
            updateControlText(MB_FW_RESULT_VAL, MBTestResultDir["软件版本"], Color.Black);
            updateControlText(MB_ALL_RESULT_VAL, MBTestResultDir["测试结果"], decideColor(MBTestResultDir["测试结果"]));
            updateControlText(MB_POWER_RESULT_VAL, MBTestResultDir["电源"], decideColor(MBTestResultDir["电源"]));
            updateControlText(MB_LED_RESULT_VAL, MBTestResultDir["指示灯"], decideColor(MBTestResultDir["指示灯"]));
            //updateControlText(MB_CARD_RESULT_VAL, MBTestResultDir["刷卡"] + "  卡号:" + MBTestResultDir["卡号"], decideColor(MBTestResultDir["刷卡"]));
            updateControlText(MB_TRUMPT_RESULT_VAL, MBTestResultDir["喇叭"], decideColor(MBTestResultDir["喇叭"]));
            updateControlText(MB_RELAY_RESULT_VAL, MBTestResultDir["继电器"], decideColor(MBTestResultDir["继电器"]));
            updateControlText(MB_FLASH_RESULT_VAL, MBTestResultDir["FLASH"], decideColor(MBTestResultDir["FLASH"]));
            updateControlText(MB_SET_RTC_RESULT_VAL, MBTestResultDir["SETRTC"], decideColor(MBTestResultDir["SETRTC"]));
            updateControlText(MB_GET_RTC_RESULT_VAL, MBTestResultDir["GETRTC"], decideColor(MBTestResultDir["GETRTC"]));
            updateControlText(skinLabel_MB_2G_RESULT_VAL, MBTestResultDir["2G"], decideColor(MBTestResultDir["2G"]));
            updateControlText(MB_TEST_USED_TIME_VAL, MBTestResultDir["测试用时"], Color.Black);
            updateControlText(skinLabel_MB_TEST_START_TIME, MBTestResultDir["测试时间"], Color.Black);

        }

        private void ShowSubBoardResult()
        {
            updateControlText(skinLabel_SB_RESULT_VAL, SBTestResultDir["PCB编号"], Color.Black);
            updateControlText(skinLabel_SB_TESTOR_RESULT_VAL, SBTestResultDir["测试员"], Color.Black);
            updateControlText(skinLabel_SB_FW_RESULT_VAL, SBTestResultDir["软件版本"], Color.Black);
            updateControlText(skinLabel_SB_ALL_RESULT_VAL, SBTestResultDir["测试结果"], decideColor(SBTestResultDir["测试结果"]));
            //updateControlText(skinLabel_SB_CARD_RESULT_VAL, SBTestResultDir["刷卡"] + "  卡号:" + SBTestResultDir["卡号"], decideColor(SBTestResultDir["刷卡"]));
            updateControlText(skinLabel_SB__BT_RESULT_VAL, SBTestResultDir["蓝牙"], decideColor(SBTestResultDir["蓝牙"]));
            updateControlText(skinLabel_SB_2G_RESULT_VAL, SBTestResultDir["2G"], decideColor(SBTestResultDir["2G"]));
            updateControlText(skinLabel_SB_TEST_USETIME_VAL, SBTestResultDir["测试用时"], Color.Black);
            updateControlText(skinLabel_SB_TEST_START_TIME, SBTestResultDir["测试时间"], Color.Black);
            //SBTestResultDir["2G"] = "通过";
        }

        private void ShowChgBoardResult()
        {
            updateControlText(skinLabel_CHG_STATION_ID_RESLUT_VAL, ChargerTestResultDir["电桩号"], Color.Black);
            updateControlText(skinLabel_CHG_MB_QR_RES_VAL, ChargerTestResultDir["主板编号"], Color.Black);
          //  updateControlText(skinLabel_CHG_SB_QR_RES_VAL, ChargerTestResultDir["软件版本"], Color.Black);
            updateControlText(skinLabel_CHG_TESTOR_RES_VAL, ChargerTestResultDir["测试员"], Color.Black);
            updateControlText(skinLabel_CHG_FW_RES_VAL, ChargerTestResultDir["软件版本"], Color.Black);
            updateControlText(skinLabeL_CHG_TEST_RES_VAL, ChargerTestResultDir["测试结果"], decideColor(ChargerTestResultDir["测试结果"]));
            updateControlText(skinLabel_CHG_LED_RES_VAL, ChargerTestResultDir["指示灯"], decideColor(ChargerTestResultDir["指示灯"]));
            //updateControlText(skinLabel_CHG_CARD_RES_VAL, ChargerTestResultDir["刷卡"] + "  卡号:" + ChargerTestResultDir["卡号"], decideColor(ChargerTestResultDir["刷卡"]));
            updateControlText(skinLabel_CHG_TRUMPET_RES_VAL, ChargerTestResultDir["喇叭"], decideColor(ChargerTestResultDir["喇叭"]));
            updateControlText(skinLabel_CHG_BT_RES_VAL, ChargerTestResultDir["蓝牙"], decideColor(ChargerTestResultDir["蓝牙"]));
            updateControlText(skinLabel_CHG_2G_RES_VAL, ChargerTestResultDir["2G"], decideColor(ChargerTestResultDir["2G"]));
            updateControlText(skinLabel_CHG_RELAY_RES_VAL, ChargerTestResultDir["继电器"], decideColor(ChargerTestResultDir["继电器"]));
            updateControlText(skinLabel_CHG_TEST_USEDTIME_RES_VAL, ChargerTestResultDir["测试用时"], Color.Black);
            updateControlText(skinLabel_CHG_TEST_TIME_RES_VAL, ChargerTestResultDir["测试时间"], Color.Black);
        }

        private Dictionary<string, string> ModifyResultData(Dictionary<string, string> inputDic)
        {


            string resKey = "", resValue = "";


            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (var item in inputDic)
            {
                dictionary.Add(item.Key, item.Value);
            }

            foreach (var item in dictionary)
            {
                if (item.Value == "" && item.Key != "测试结果")
                {
                    inputDic[item.Key] = "未测试";
                }
                else if (item.Value == "不通过" && item.Value != "无")
                {
                    inputDic["测试结果"] = "不通过";
                }

                if (inputDic[item.Key] == "未测试")
                {
                    inputDic["测试结果"] = "不通过";
                }
            }
            if (inputDic["测试结果"] == "")
            {
                inputDic["测试结果"] = "通过";
            }

            foreach (var item in inputDic)
            {
                resKey += item.Key + " :\r\n";
                resValue += item.Value + "\r\n";
            }

            return inputDic;
        }

        private void skinButton_AccountSettings_Click(object sender, EventArgs e)
        {
            AccountSettingForm accountSettingForm = new AccountSettingForm();
            accountSettingForm.ShowDialog();
        }

        private void textBoxChargerIdQrCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                if (textBoxChargerIdQrCode.Text.IndexOf(ProcTestData.X10QrCodeUrl) == 0)
                {
                    textBoxChargerIdQrCode.Text = textBoxChargerIdQrCode.Text.Remove(0, ProcTestData.X10QrCodeUrl.Length);
                    System.Text.RegularExpressions.Regex rex =
                    new System.Text.RegularExpressions.Regex(@"^\d+$");
                    //判断字符串是否全为数字
                    if (rex.IsMatch(textBoxChargerIdQrCode.Text) == true)
                    {
                        labelQrCodeChargerId.Text = textBoxChargerIdQrCode.Text;

                        bool res = ProcTestData.CheckChgIdInSql(labelQrCodeChargerId.Text);


                        textBoxChargerIdQrCode.Text = "";
                        if (res == false)
                        {
                            updateControlText(labelCheckResult, "桩号重复", Color.Red);
                        }
                        else
                        {
                            updateControlText(labelCheckResult, "通过", Color.Green);
                            //string mysqlCmd = "INSERT INTO  product_charger_id_tbl(charger_id,testor) VALUES( '" + labelQrCodeChargerId.Text + "','" + ProcTestData.PresentAccount + "');";
                            //ProcTestData.SendMysqlCommand(mysqlCmd, false);
                        }
                    }
                    else
                    {
                        MessageBox.Show("桩号包含非数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxChargerIdQrCode.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("二维码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxChargerIdQrCode.Text = "";
                }
                textBoxChargerIdQrCode.Focus();
            }
        }

        private void skinButtonCleanRecord_Click(object sender, EventArgs e)
        {
            if (ProcTestData.PresentAccount == "Admin")
            {
                if (MessageBox.Show("是否确认清空已保存的桩号列表？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (MessageBox.Show("此操作不可逆！\r\n确认删除？", "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        //string mysqldeletecmd = "DELETE FROM product_charger_id_tbl";
                        //ProcTestData.SendMysqlCommand(mysqldeletecmd, false);
                    }
                }
            }
            else
            {
                MessageBox.Show("此操作权限仅限管理员！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        //升级按钮监听
        string filepath = @".\X6_Upgrade_Tools_V1.1\release\";
        private void skinButton_upGrade_Click(object sender, EventArgs e)
        {
            try
            {
                //选择烧录软件安装路径
                System.Diagnostics.Process.Start(filepath + "X6_Upgrade.exe");
            }
            catch (Exception ex)
            {
                LOG("升级工具打开出错!");
                LOG(ex.Message);
            }
        }

        private void skinSplitContainer9_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox_SB_QR_TextChanged(object sender, EventArgs e)
        {

        }

        private void skinNumericUpDown_TestOverTime_ValueChanged(object sender, EventArgs e)
        {

        }

        private void skinLabel116_Click(object sender, EventArgs e)
        {

        }

        private void onlineDetectProcess()
        {
            OnlineFlg = false;
            ItemTestTime = GetCurrentTimeStamp();
            OnlineDetectTime = 80;
            //updateControlText(skinLabel_OnlineDetectTime, OnlineDetectTime.ToString("D2"));
            LOG("查询是否联网成功?");
            SendOnlineTestReq();
            //Thread.Sleep(1000);
            int i = 0;
            while (true)
            {
                if ((GetCurrentTimeStamp() - ItemTestTime) >= 20)//超时
                {
                    ItemTestTime = GetCurrentTimeStamp();
                    LOG("查询是否联网成功?");
                    SendOnlineTestReq();
                    i++;
                }
                if (OnlineFlg == true)
                {
                    OnlineFlg = false;

                    LOG("获取桩号");
                    SendGetID();
                    Thread.Sleep(100);
                    LOG("获取识别码.");
                    SendGetDeviceCode();
                    Thread.Sleep(100);
                    LOG("获取信号值");
                    Send2GTestReq();
                    break;
                }

                if (i >= 5)
                {
                    onlineDectecFlag = false;
                    LOG("联网超时.");
                    updateControlText(skinButton_OnlineStartDetect, "开始检测");
                    updateControlText(skinLabel_OnlineDetectResult, "联网超时", Color.Red);
                    updateControlText(skinLabel_OnlineDetectTime, "0");

                    LOG("获取桩号");
                    SendGetID();
                    Thread.Sleep(100);
                    LOG("获取信号值");
                    Send2GTestReq();
                    break;
                }
                Thread.Sleep(100);
            }
        }

        private void skinButton_OnlineStartDetect_Click(object sender, EventArgs e)
        {
            updateControlText(skinLabel_OnlineDetectResult, "");
            updateControlText(skinLabel_OnlineDetectStation, "");
            updateControlText(skinLabel_OnlineDecCsqVal, "");
            updateControlText(skinLabel_OnlineDeviceCode, "");
            if (onlineDectecFlag == false)
            {
                onlineDectecFlag = true;
                skinButton_OnlineStartDetect.Text = "停止检测";

                onlineDetectThread = new Thread(onlineDetectProcess);
                onlineDetectThread.Start();
            }
            else
            {
                onlineDectecFlag = false;
                skinButton_OnlineStartDetect.Text = "开始检测";

                if (onlineDetectThread != null)
                {
                    if (onlineDetectThread.IsAlive)
                    {
                        onlineDetectThread.Abort();
                    }
                }
            }
        }

        private void skinButton_Log_Clear_Click(object sender, EventArgs e)
        {
            textBox_Log.Text = "";
        }

        private void textBox_Log_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox_MB_QRCode_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_WholeChg_SN_QR_TextChanged(object sender, EventArgs e)
        {

        }

        private void skinButton_StartAgingTest_Click(object sender, EventArgs e)
        {
            SendStartAgingTestReq();
        }

        private void skinButton_GetAgingTestResult_Click(object sender, EventArgs e)
        {
            SendGetAgingTestResultReq();
        }

        private void skinButton_AgingClearLog_Click(object sender, EventArgs e)
        {
            textBox_AgingLog.Text = "";
        }

       
    }
}















