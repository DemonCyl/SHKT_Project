using FastReport;
using FastReport.Barcode;
using HslCommunication;
using HslCommunication.Profinet.Omron;
using HslCommunication.Profinet.Siemens;
using log4net;
using Newtonsoft.Json;
using Panuon.UI.Silver;
using SHKT_Project.DAL;
using SHKT_Project.Entity;
using SHKT_Project.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SHKT_Project
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConfigData config;
        private MainDAL dal;
        //private SiemensS7Net plc;
        private OmronFinsNet plc;
        private OperateResult connect;
        private DispatcherTimer timer = null;
        private SerialPort port1;
        private SerialPort port2;
        private SerialPort port3;
        private SerialPort port4;
        private bool remark = false;
        private AssemblyType assemblyType;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private static BitmapImage IFalse = new BitmapImage(new Uri("/Static/01.png", UriKind.Relative));
        private static BitmapImage ITrue = new BitmapImage(new Uri("/Static/02.png", UriKind.Relative));
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private CheckCodeInfo codeInfo1;
        private CheckCodeInfo codeInfo2;
        private CheckCodeInfo codeInfo3;
        private CheckCodeInfo codeInfo4;
        private List<CheckCodeInfoEntry> entry1;
        private List<CheckCodeInfoEntry> entry2;
        private List<CheckCodeInfoEntry> entry3;
        private List<string> barlist1 = new List<string>();
        private List<string> barlist2 = new List<string>();
        private List<string> barlist3 = new List<string>();
        private List<float> valuelist = new List<float>();
        private bool saveMark1 = false;
        private bool saveMark2 = false;
        private bool saveMark3 = false;
        private bool saveMark4 = false;
        private bool checkMark = false;


        public MainWindow()
        {
            InitializeComponent();
            #region 启动时串口最大化显示
            this.WindowState = WindowState.Maximized;
            Rect rc = SystemParameters.WorkArea; //获取工作区大小
            //this.Topmost = true;
            this.Left = 0; //设置位置
            this.Top = 0;
            this.Width = rc.Width;
            this.Height = rc.Height;
            #endregion

            LoadJsonData();
            Init();

            //plc = new SiemensS7Net(SiemensPLCS.S1200, config.IpAddress)
            //{
            //    ConnectTimeOut = 5000
            //};

            plc = new OmronFinsNet(config.IpAddress, config.IpPort)
            {
                SA1 = 10,
                ConnectTimeOut = 5000
            };


            connect = plc.ConnectServer();

            #region PLC连接定时器
            //timer = new System.Windows.Threading.DispatcherTimer();
            //timer.Tick += new EventHandler(ThreadCheck);
            //timer.Interval = new TimeSpan(0, 0, 0, 5);
            //timer.Start();
            #endregion

        }

        private void LoadJsonData()
        {
            try
            {
                using (var sr = File.OpenText("C:\\config\\SHConfig.json"))
                {
                    string JsonStr = sr.ReadToEnd();
                    config = JsonConvert.DeserializeObject<ConfigData>(JsonStr);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private void Init()
        {

            dal = new MainDAL(config);

            var typeList = dal.GetAllAssemblyType();
            if (typeList.Any())
            {
                StartType(typeList.FirstOrDefault());
            }

        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            CheckConfigWindow checkConfigWindow = new CheckConfigWindow(dal)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            checkConfigWindow.Show();
            checkConfigWindow.Activate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TypeSetPage nw = new TypeSetPage(dal)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            nw.assemblyTypeHandler += new TypeSetPage.AssemblyTypeHandler(AssemblyTypeSet);
            nw.ShowDialog();
            nw.Activate();
        }

        private void AssemblyTypeSet(object sender, AssemblyType type)
        {
            StartType(type);
        }
        private void StartType(AssemblyType type)
        {
            this.assemblyType = type;

            typeName.Text = type.FAssemblyName;

            DefaultText1();
            DefaultText2();
            DefaultText3();
            DefaultText4();
            checkMark = false;
            var mark = false;
            // Init 各工位检验码
            var list = dal.GetCheckCodeInfo(string.Empty, type.FInterID);
            if (list.Any())
            {
                list.ForEach(f =>
                {
                    f.entries = dal.GetCheckCodeInfoEntry(f.FInterID);
                });
            }

            codeInfo1 = list.Find(f => f.FGWID == GwType.OP01);
            if (codeInfo1 == null)
            {
                remark1.Text = $"该工位没有配置{type.FAssemblyName}的检验码";
                mark = true;
            }
            else
            {
                entry1 = codeInfo1.entries;
                if (!entry1.Any())
                {
                    remark1.Text = $"该工位没有配置{type.FAssemblyName}的零件检验码";
                    mark = true;
                }
            }
            codeInfo2 = list.Find(f => f.FGWID == GwType.OP40);
            if (codeInfo2 == null)
            {
                remark2.Text = $"该工位没有配置{type.FAssemblyName}的检验码";
                mark = true;
            }
            else
            {
                entry2 = codeInfo2.entries;
                if (!entry2.Any())
                {
                    remark2.Text = $"该工位没有配置{type.FAssemblyName}的零件检验码";
                    mark = true;
                }
            }
            codeInfo3 = list.Find(f => f.FGWID == GwType.OP70);
            if (codeInfo3 == null)
            {
                remark3.Text = $"该工位没有配置{type.FAssemblyName}的检验码";
                mark = true;
            }
            else
            {
                entry3 = codeInfo3.entries;
                if (!entry3.Any())
                {
                    remark3.Text = $"该工位没有配置{type.FAssemblyName}的零件检验码";
                    mark = true;
                }
            }
            //codeInfo4 = list.Find(f => f.FGWID == GwType.OP80);
            //if (codeInfo4 == null)
            //{
            //    remark4.Text = $"该工位没有配置{type.FAssemblyName}的检验码";
            //    mark = true;
            //}

            checkMark = mark;
            //log.Info(checkMark);

            list1.ItemsSource = barlist1;
            list2.ItemsSource = barlist2;
            list3.ItemsSource = barlist3;
            list4.ItemsSource = valuelist;
        }

        private void DefaultText1()
        {
            gwcode1.Text = "";
            remark1.Text = "";
            list1.ItemsSource = null;
            list1.Items.Refresh();
            barlist1.Clear();
        }
        private void DefaultText2()
        {
            gwcode2.Text = "";
            remark2.Text = "";
            list2.ItemsSource = null;
            list2.Items.Refresh();
            barlist2.Clear();
        }
        private void DefaultText3()
        {
            gwcode3.Text = "";
            remark3.Text = "";
            list3.ItemsSource = null;
            list3.Items.Refresh();
            barlist3.Clear();
        }
        private void DefaultText4()
        {
            gwcode4.Text = "";
            remark4.Text = "";
            list4.ItemsSource = null;
            list4.Items.Refresh();
            valuelist.Clear();
        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow hw = new HistoryWindow(dal)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            hw.Show();
            hw.Activate();
        }

        /// <summary>
        /// 装配条码打印功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPut_Click(object sender, RoutedEventArgs e)
        {
            string bar = null;
            // 装配条码按规则生成

            try
            {
                string path = "C:\\config\\template.frx";
                Report report = new Report();
                report.Prepare();
                report.Load(path);
                var barcodeObject = report.FindObject("Barcode1") as BarcodeObject;
                if (barcodeObject != null)
                {
                    barcodeObject.Text = bar;
                    barcodeObject.Barcode = new Barcode128();
                }

                report.Show();
                //report.PrintPrepared();
                //report.PrintSettings.ShowDialog = false;
                //report.Print();
                //report.Dispose();

                //默认不显示打印机选择页面
                //report.PrintSettings.ShowDialog = false;
                //获取打印机的名称，这里是通过封装的方法去获取打印机名，这里可以直接指定“打印机名称”;
                //string strPrintName = PrinterHelper.GetPrintSetting("LocationPrint");
                ////当前操作打印机
                //report.PrintSettings.Printer = strPrintName;
                ////启动打印
                //report.Print();

                //report.Dispose();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        /// <summary>
        /// PLC 数据读取
        /// </summary>
        private void DataReload()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) =>
            {
                try
                {
                    #region 提示Info
                    if (gwcode1.Text.Trim() != string.Empty)
                    {
                        if (entry1.Any())
                        {
                            remark1.Text = $"漏扫零件：{entry1.First().FCodeRule}";
                        }
                    }
                    if (gwcode2.Text.Trim() != string.Empty)
                    {
                        if (entry2.Any())
                        {
                            remark2.Text = $"漏扫零件：{entry2.First().FCodeRule}";
                        }
                    }
                    if (gwcode3.Text.Trim() != string.Empty)
                    {
                        if (entry3.Any())
                        {
                            remark3.Text = $"漏扫零件：{entry3.First().FCodeRule}";
                        }
                    }
                    #endregion

                    #region 获取气检数据

                    #endregion

                    #region 获取保存信号 -- 放行信号
                    // OP01
                    var singal1 = plc.ReadBool("");
                    if (singal1.IsSuccess)
                    {
                        if (singal1.Content)
                        {
                            if (!saveMark1)
                            {
                                var record1 = new RecordInfo();
                                record1.FGWID = GwType.OP01;
                                record1.FAssemblyID = assemblyType.FInterID;
                                var bar1 = gwcode1.Text.Trim();
                                if (bar1 == string.Empty)
                                {
                                    remark1.Text = "装配条码为空！";
                                }
                                else
                                {
                                    record1.FBar = bar1;
                                    var recordEntry1 = new List<RecordInfoEntry>();
                                    if (barlist1 != null && barlist1.Any())
                                    {
                                        barlist1.ForEach(f =>
                                        {
                                            recordEntry1.Add(new RecordInfoEntry(f));
                                        });
                                    }
                                    record1.entries = recordEntry1;

                                    if (this.SaveData(record1, GwType.OP01)) // true
                                    {
                                        // write plc ??

                                        gwcode1.Text = "";
                                        barlist1.Clear();
                                        remark1.Text = "保存成功!";
                                        saveMark1 = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            saveMark1 = false;
                        }
                    }

                    // OP40
                    var singal2 = plc.ReadBool("");
                    if (singal2.IsSuccess)
                    {
                        if (singal2.Content)
                        {
                            if (!saveMark2)
                            {
                                var record2 = new RecordInfo();
                                record2.FGWID = GwType.OP40;
                                record2.FAssemblyID = assemblyType.FInterID;
                                var bar2 = gwcode2.Text.Trim();
                                if (bar2 == string.Empty)
                                {
                                    remark2.Text = "装配条码为空！";
                                }
                                else
                                {
                                    record2.FBar = bar2;
                                    var recordEntry2 = new List<RecordInfoEntry>();
                                    if (barlist2 != null && barlist2.Any())
                                    {
                                        barlist2.ForEach(f =>
                                        {
                                            recordEntry2.Add(new RecordInfoEntry(f));
                                        });
                                    }
                                    record2.entries = recordEntry2;

                                    if (this.SaveData(record2, GwType.OP40)) // true
                                    {
                                        // write plc ??

                                        gwcode2.Text = "";
                                        barlist2.Clear();
                                        remark2.Text = "保存成功!";
                                        saveMark2 = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            saveMark2 = false;
                        }
                    }

                    // OP70
                    var singal3 = plc.ReadBool("");
                    if (singal3.IsSuccess)
                    {
                        if (singal3.Content)
                        {
                            if (!saveMark3)
                            {
                                var record3 = new RecordInfo();
                                record3.FGWID = GwType.OP70;
                                record3.FAssemblyID = assemblyType.FInterID;
                                var bar3 = gwcode3.Text.Trim();
                                if (bar3 == string.Empty)
                                {
                                    remark3.Text = "装配条码为空！";
                                }
                                else
                                {
                                    record3.FBar = bar3;
                                    record3.FCustBar = custcode.Text.Trim();
                                    //var recordEntry3 = new List<RecordInfoEntry>();
                                    //if (barlist3 != null && barlist3.Any())
                                    //{
                                    //    barlist3.ForEach(f =>
                                    //    {
                                    //        recordEntry3.Add(new RecordInfoEntry(f));
                                    //    });
                                    //}
                                    //record3.entries = recordEntry3;

                                    if (this.SaveData(record3, GwType.OP70)) // true
                                    {
                                        // write plc ??

                                        gwcode3.Text = "";
                                        barlist3.Clear();
                                        remark3.Text = "保存成功!";
                                        saveMark3 = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            saveMark3 = false;
                        }
                    }

                    // OP80
                    var singal4 = plc.ReadBool("");
                    if (singal4.IsSuccess)
                    {
                        if (singal4.Content)
                        {
                            if (!saveMark4)
                            {
                                var record4 = new RecordInfo();
                                record4.FGWID = GwType.OP80;
                                record4.FAssemblyID = assemblyType.FInterID;
                                var bar4 = gwcode4.Text.Trim();
                                if (bar4 == string.Empty)
                                {
                                    remark4.Text = "客户条码为空！";
                                }
                                else
                                {
                                    record4.FCustBar = bar4;
                                    var recordEntry4 = new List<RecordInfoEntry1>();
                                    if (valuelist != null && valuelist.Any())
                                    {
                                        valuelist.ForEach(f =>
                                        {
                                            recordEntry4.Add(new RecordInfoEntry1(f));
                                        });
                                    }
                                    record4.entries1 = recordEntry4;

                                    if (this.SaveData(record4, GwType.OP80)) // true
                                    {
                                        // write plc ??

                                        gwcode4.Text = "";
                                        valuelist.Clear();
                                        remark4.Text = "保存成功!";
                                        saveMark4 = true;

                                        ShowDetail(bar4);
                                    }
                                }
                            }
                        }
                        else
                        {
                            saveMark4 = false;
                        }
                    }

                    #endregion

                    remark = true;
                }
                catch (Exception ex)
                {
                    log.Error("------PLC访问出错------");
                    log.Error(ex.Message);
                    dispatcherTimer.Stop();
                    remark = false;
                }
            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(10);
            dispatcherTimer.Start();
        }

        public void ThreadCheck(object sender, EventArgs e)
        {
            var check = plc.ReadUInt16("DB2000.0");
            if (check.IsSuccess)
            {
                PLCImage.Source = ITrue;

                if (!remark && !checkMark)
                {
                    // run thread
                    this.GetConnection01(config.PortName1, config.BaudRate1);
                    this.GetConnection02(config.PortName2, config.BaudRate2);
                    this.GetConnection03(config.PortName3, config.BaudRate3);
                    this.GetConnection04(config.PortName4, config.BaudRate4);

                    DataReload();

                }
            }
            else
            {
                PLCImage.Source = IFalse;
            }
        }

        #region 扫描枪01
        public bool GetConnection01(string name, int rate)
        {
            bool mark = false;
            if (port1 == null)
            {
                port1 = new SerialPort(name, rate, Parity.None, 8, StopBits.One);
                port1.DtrEnable = true;
                port1.RtsEnable = true;
                port1.ReadTimeout = 100;
                port1.DataReceived += SerialPort_DataReceived01;
                mark = OpenPort(port1);
            }
            return mark;
        }

        private void SerialPort_DataReceived01(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = (SerialPort)sender;
                //开启接收数据线程
                Thread threadReceiveSub = new Thread(new ParameterizedThreadStart(ReceiveData01));
                threadReceiveSub.IsBackground = true;
                threadReceiveSub.Start(serialPort);
            }
            catch (Exception ex)
            {
                log.Error("thread  " + ex.Message);
            }
        }

        private void ReceiveData01(object serialPortobj)
        {
            try
            {
                SerialPort serialPort = (SerialPort)serialPortobj;

                //防止数据接收不完整 线程sleep(100)
                Thread.Sleep(300);

                string str = serialPort.ReadExisting();
                str = str.Trim();

                if (str == string.Empty)
                {
                    return;
                }
                else
                {
                    BarCodeMatch(str, GwType.OP01);
                }
            }
            catch (Exception ex)
            {
                log.Error("data  " + ex.Message);
            }
        }

        private void Port1Close()
        {
            if (port1 != null && port1.IsOpen)
            {
                port1.Close();
                port1 = null;
            }
        }
        #endregion

        #region 扫描枪02
        public bool GetConnection02(string name, int rate)
        {
            bool mark = false;
            if (port2 == null)
            {
                port2 = new SerialPort(name, rate, Parity.None, 8, StopBits.One);
                port2.DtrEnable = true;
                port2.RtsEnable = true;
                port2.ReadTimeout = 100;
                port2.DataReceived += SerialPort_DataReceived02;
                mark = OpenPort(port2);
            }
            return mark;
        }

        private void SerialPort_DataReceived02(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = (SerialPort)sender;
                //开启接收数据线程
                Thread threadReceiveSub = new Thread(new ParameterizedThreadStart(ReceiveData02));
                threadReceiveSub.IsBackground = true;
                threadReceiveSub.Start(serialPort);
            }
            catch (Exception ex)
            {
                log.Error("thread  " + ex.Message);
            }
        }

        private void ReceiveData02(object serialPortobj)
        {
            try
            {
                SerialPort serialPort = (SerialPort)serialPortobj;

                //防止数据接收不完整 线程sleep(100)
                Thread.Sleep(300);

                string str = serialPort.ReadExisting();
                str = str.Trim();

                if (str == string.Empty)
                {
                    return;
                }
                else
                {
                    BarCodeMatch(str, GwType.OP40);
                }
            }
            catch (Exception ex)
            {
                log.Error("data  " + ex.Message);
            }
        }

        private void Port2Close()
        {
            if (port2 != null && port2.IsOpen)
            {
                port2.Close();
                port2 = null;
            }
        }
        #endregion

        #region 扫描枪03
        public bool GetConnection03(string name, int rate)
        {
            bool mark = false;
            if (port3 == null)
            {
                port3 = new SerialPort(name, rate, Parity.None, 8, StopBits.One);
                port3.DtrEnable = true;
                port3.RtsEnable = true;
                port3.ReadTimeout = 100;
                port3.DataReceived += SerialPort_DataReceived03;
                mark = OpenPort(port3);
            }
            return mark;
        }

        private void SerialPort_DataReceived03(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = (SerialPort)sender;
                //开启接收数据线程
                Thread threadReceiveSub = new Thread(new ParameterizedThreadStart(ReceiveData03));
                threadReceiveSub.IsBackground = true;
                threadReceiveSub.Start(serialPort);
            }
            catch (Exception ex)
            {
                log.Error("thread  " + ex.Message);
            }
        }

        private void ReceiveData03(object serialPortobj)
        {
            try
            {
                SerialPort serialPort = (SerialPort)serialPortobj;

                //防止数据接收不完整 线程sleep(100)
                Thread.Sleep(300);

                string str = serialPort.ReadExisting();
                str = str.Trim();

                if (str == string.Empty)
                {
                    return;
                }
                else
                {
                    BarCodeMatch(str, GwType.OP70);
                }
            }
            catch (Exception ex)
            {
                log.Error("data  " + ex.Message);
            }
        }

        private void Port3Close()
        {
            if (port3 != null && port3.IsOpen)
            {
                port3.Close();
                port3 = null;
            }
        }
        #endregion

        #region 扫描枪04
        public bool GetConnection04(string name, int rate)
        {
            bool mark = false;
            if (port4 == null)
            {
                port4 = new SerialPort(name, rate, Parity.None, 8, StopBits.One);
                port4.DtrEnable = true;
                port4.RtsEnable = true;
                port4.ReadTimeout = 100;
                port4.DataReceived += SerialPort_DataReceived04;
                mark = OpenPort(port4);
            }
            return mark;
        }

        private void SerialPort_DataReceived04(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = (SerialPort)sender;
                //开启接收数据线程
                Thread threadReceiveSub = new Thread(new ParameterizedThreadStart(ReceiveData04));
                threadReceiveSub.IsBackground = true;
                threadReceiveSub.Start(serialPort);
            }
            catch (Exception ex)
            {
                log.Error("thread  " + ex.Message);
            }
        }

        private void ReceiveData04(object serialPortobj)
        {
            try
            {
                SerialPort serialPort = (SerialPort)serialPortobj;

                //防止数据接收不完整 线程sleep(100)
                Thread.Sleep(300);

                string str = serialPort.ReadExisting();
                str = str.Trim();

                if (str == string.Empty)
                {
                    return;
                }
                else
                {
                    BarCodeMatch(str, GwType.OP80);
                }
            }
            catch (Exception ex)
            {
                log.Error("data  " + ex.Message);
            }
        }

        private void Port4Close()
        {
            if (port4 != null && port4.IsOpen)
            {
                port4.Close();
                port4 = null;
            }
        }
        #endregion

        public void BarCodeMatch(string barcode, GwType gw)
        {
            Dispatcher.InvokeAsync(() =>
            {
                switch (gw)
                {
                    case GwType.OP01:
                        if (gwcode1.Text.Trim() == string.Empty)
                        {
                            if (barcode.Contains(codeInfo1.FBarCodeRule))
                            {
                                gwcode1.Text = barcode;
                            }
                            else
                            {
                                remark1.Text = "装配条码 NG！";
                            }
                        }
                        else
                        {
                            var co = entry1.Find(f => barcode.Contains(f.FCodeRule));
                            if (co != null)
                            {
                                barlist1.Add(barcode);
                                entry1.Remove(co);
                            }

                        }
                        break;
                    case GwType.OP40:
                        if (gwcode2.Text.Trim() == string.Empty)
                        {
                            if (barcode.Contains(codeInfo2.FBarCodeRule))
                            {
                                gwcode2.Text = barcode;
                            }
                            else
                            {
                                remark2.Text = "装配条码 NG！";
                            }
                        }
                        else
                        {
                            var co = entry2.Find(f => barcode.Contains(f.FCodeRule));
                            if (co != null)
                            {
                                barlist2.Add(barcode);
                                entry2.Remove(co);
                            }

                        }
                        break;
                    case GwType.OP70:
                        // 需要更改
                        if (gwcode3.Text.Trim() == string.Empty)
                        {
                            if (barcode.Contains(codeInfo3.FBarCodeRule))
                            {
                                gwcode3.Text = barcode;
                            }
                            else
                            {
                                remark3.Text = "装配条码 NG！";
                            }
                        }
                        else if (custcode.Text.Trim() == string.Empty)
                        {
                            //var co = entry3.Find(f => barcode.Contains(f.FCodeRule));
                            //if (co != null)
                            //{
                            //    barlist3.Add(barcode);
                            //    entry3.Remove(co);
                            //}
                            
                            custcode.Text = barcode;
                        }
                        break;
                    case GwType.OP80:

                        // 扫客户条码
                        if (gwcode4.Text.Trim() == string.Empty)
                            gwcode4.Text = barcode;

                        //if (gwcode4.Text.Trim() == string.Empty)
                        //{
                        //    if (barcode.Contains(codeInfo4.FBarCodeRule))
                        //    {
                        //        gwcode4.Text = barcode;
                        //    }
                        //    else
                        //    {
                        //        remark4.Text = "装配条码 NG！";
                        //    }
                        //}
                        break;
                }
            });
        }

        private bool OpenPort(SerialPort serialPort)
        {
            string message = null;
            try//这里写成异常处理的形式以免串口打不开程序崩溃
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (serialPort.IsOpen)
            {
                log.Info("连接成功！");
                return true;
            }
            else
            {
                log.Error("打开失败!原因为： " + message);
                return false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connect.IsSuccess)
            {
                plc.ConnectClose();
            }
            if (timer != null && timer.IsEnabled)
                timer.Stop();

            this.Port1Close();
            this.Port2Close();
            this.Port3Close();
            this.Port4Close();

            log.Info("PLC Disconnected!");
        }

        /// <summary>
        /// save data
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool SaveData(RecordInfo info, GwType type)
        {
            bool re = false;
            if (info != null)
            {
                re = dal.SaveInfo(info, type);
            }

            return re;
        }

        private void ShowDetail(string custBar)
        {
            HistoryWindow hw = new HistoryWindow(dal)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            hw.custCode.Text = custBar;
            hw.Show();
            hw.Query();
            hw.Activate();

        }

    }
}
