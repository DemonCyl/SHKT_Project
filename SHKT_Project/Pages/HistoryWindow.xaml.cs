using log4net;
using Panuon.UI.Silver;
using SHKT_Project.DAL;
using SHKT_Project.Entity;
using System;
using System.Collections.Generic;
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

namespace SHKT_Project.Pages
{
    /// <summary>
    /// HistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<AssemblyType> AssemblyList = new List<AssemblyType>();
        private MainDAL dal;
        private List<string> OPList = new List<string>();
        private List<RecordInfo> recordInfos = new List<RecordInfo>();

        public HistoryWindow(MainDAL dal)
        {
            this.dal = dal;
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            QueryTypeItems();

            foreach (var t in Enum.GetValues(typeof(GwType)))
            {
                OPList.Add(t.ToString());
            }
            OPList.Insert(0, "");
            GwItems.ItemsSource = OPList;
            GwItems.SelectedIndex = 0;
        }

        private void QueryTypeItems()
        {
            TypeItems.ItemsSource = null;

            try
            {
                AssemblyList = dal.GetAllAssemblyType();
                if (AssemblyList.Any())
                {
                    var itemlist = new List<string>();
                    AssemblyList.ForEach(f =>
                    {
                        itemlist.Add(f.FAssemblyName);
                    });
                    itemlist.Insert(0, "");
                    TypeItems.ItemsSource = itemlist;
                    TypeItems.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void Query()
        {
            conList.ItemsSource = null;
            entryList.ItemsSource = null;
            int typeID = 0;
            var item = TypeItems.SelectedItem.ToString();
            //GwType gwID;
            var gwItem = GwItems.SelectedItem.ToString();
            if (TypeItems.HasItems && !string.IsNullOrEmpty(item))
            {
                var type = AssemblyList.Find(f => f.FAssemblyName == item);
                typeID = type.FInterID;
            }

            recordInfos = dal.GetRecordInfo(gwItem, typeID, barCode.Text.Trim());
            if (recordInfos.Any())
            {
                recordInfos.ForEach(f =>
                {
                    f.entries = dal.GetRecordInfoEntry(f.FInterID);
                });
            }

            conList.ItemsSource = recordInfos;
            conList.SelectedIndex = 0;
            conList.Items.Refresh();
        }

        private void conList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (conList.HasItems)
            {
                var selectedInfo = conList.SelectedItem as RecordInfo;
                entryList.ItemsSource = selectedInfo.entries;
                //log.Info(selectedInfo.FBarCodeRule);
            }
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {
            if (conList.HasItems)
            {
                var selectedInfo = conList.SelectedItem as RecordInfo;
                selectedInfo.entries.Add(new RecordInfoEntry(selectedInfo.FInterID, ""));
                //log.Info(selectedInfo.FBarCodeRule);
                entryList.SelectedIndex = selectedInfo.entries.Count - 1;
                entryList.Items.Refresh();

                CancelBte.Visibility = Visibility.Visible;
                saveBte.Visibility = Visibility.Visible;
                changeBte.Visibility = Visibility.Hidden;
                deleteBte.Visibility = Visibility.Hidden;
                entryList.IsReadOnly = false;
            }
        }

        private void SaveEntry_Click(object sender, RoutedEventArgs e)
        {
            if (entryList.HasItems)
            {
                var selectedInfo = entryList.SelectedItem as RecordInfoEntry;
                if (selectedInfo == null)
                {
                    Info.Text = "请选择项目!";
                    return;
                }
                if (string.IsNullOrEmpty(selectedInfo.FCode))
                {
                    Info.Text = "零件条码不能为空!";
                    return;
                }
                if (selectedInfo.FEntryID == 0)
                {
                    Info.Text = dal.AddRecordEntryItem(selectedInfo) ? "零件条码新增成功！" : "零件条码新增失败！";
                }
                else
                {
                    Info.Text = dal.UpdateRecordEntryItem(selectedInfo) ? "零件条码更改成功！" : "零件条码更改失败！";
                }

                Query();
                CancelBte.Visibility = Visibility.Hidden;
                saveBte.Visibility = Visibility.Hidden;
                changeBte.Visibility = Visibility.Visible;
                deleteBte.Visibility = Visibility.Visible;
                entryList.IsReadOnly = true;
            }

        }

        private void UpdateEntry_Click(object sender, RoutedEventArgs e)
        {
            CancelBte.Visibility = Visibility.Visible;
            saveBte.Visibility = Visibility.Visible;
            changeBte.Visibility = Visibility.Hidden;
            deleteBte.Visibility = Visibility.Hidden;
            entryList.IsReadOnly = false;
        }

        private void DeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxX.Show("确认删除此条零件校验码？", "确认", Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    if (conList.HasItems)
                    {
                        var selectedInfo = entryList.SelectedItem as RecordInfoEntry;
                        if (selectedInfo == null)
                        {
                            Info.Text = "请选择项目!";
                            return;
                        }
                        Info.Text = dal.DeleteRecordEntryItem(selectedInfo.FEntryID) ? "零件条码删除成功！" : "零件条码删除失败！";

                        Query();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
            }
        }

        private void CancelEntry_Click(object sender, RoutedEventArgs e)
        {
            Query();
            CancelBte.Visibility = Visibility.Hidden;
            saveBte.Visibility = Visibility.Hidden;
            changeBte.Visibility = Visibility.Visible;
            deleteBte.Visibility = Visibility.Visible;
            entryList.IsReadOnly = true;
        }
    }
}
