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
    /// CheckConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CheckConfigWindow : Window
    {

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<AssemblyType> AssemblyList = new List<AssemblyType>();
        private MainDAL dal;
        private List<string> OPList = new List<string>();
        private List<CheckCodeInfo> checkCodeInfos = new List<CheckCodeInfo>();

        public CheckConfigWindow(MainDAL dal)
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

            checkCodeInfos = dal.GetCheckCodeInfo(gwItem, typeID);
            if (checkCodeInfos.Any())
            {
                checkCodeInfos.ForEach(f =>
                {
                    f.entries = dal.GetCheckCodeInfoEntry(f.FInterID);
                });
            }

            conList.ItemsSource = checkCodeInfos;
            conList.SelectedIndex = 0;
            conList.Items.Refresh();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddCheckWindow acw = new AddCheckWindow(dal);
            acw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            acw.addCheckHandler += new AddCheckWindow.AddCheckHandler(AddNewItem);
            acw.ShowDialog();
        }

        private void AddNewItem(object sender, bool mark)
        {
            if (mark)
                Query();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (conList.HasItems)
            {
                var selectedInfo = conList.SelectedItem as CheckCodeInfo;
                Info.Text = dal.UpdateCheckItem(selectedInfo) ? "更改成功！" : "更改失败！";

                Query();
                CancelBt.Visibility = Visibility.Hidden;
                saveBt.Visibility = Visibility.Hidden;
                changeBt.Visibility = Visibility.Visible;
                deleteBt.Visibility = Visibility.Visible;
                conList.IsReadOnly = true;
            }

        }

        private void conList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (conList.HasItems)
            {
                var selectedInfo = conList.SelectedItem as CheckCodeInfo;
                entryList.ItemsSource = selectedInfo.entries;
                //log.Info(selectedInfo.FBarCodeRule);
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            CancelBt.Visibility = Visibility.Visible;
            saveBt.Visibility = Visibility.Visible;
            changeBt.Visibility = Visibility.Hidden;
            deleteBt.Visibility = Visibility.Hidden;
            conList.IsReadOnly = false;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxX.Show("确认删除此条记录？", "确认", Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    if (conList.HasItems)
                    {
                        var selectedInfo = conList.SelectedItem as CheckCodeInfo;
                        Info.Text = dal.DeleteCheckItem(selectedInfo.FInterID) ? "删除成功！" : "删除失败！";

                        Query();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Query();
            CancelBt.Visibility = Visibility.Hidden;
            saveBt.Visibility = Visibility.Hidden;
            changeBt.Visibility = Visibility.Visible;
            deleteBt.Visibility = Visibility.Visible;
            conList.IsReadOnly = true;
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {
            if (conList.HasItems)
            {
                var selectedInfo = conList.SelectedItem as CheckCodeInfo;
                selectedInfo.entries.Add(new CheckCodeInfoEntry(selectedInfo.FInterID, ""));
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
                var selectedInfo = entryList.SelectedItem as CheckCodeInfoEntry;
                if (selectedInfo == null)
                {
                    Info.Text = "请选择项目!";
                    return;
                }
                if (string.IsNullOrEmpty(selectedInfo.FCodeRule))
                {
                    Info.Text="零件校验码不能为空!";
                    return;
                }    
                if (selectedInfo.FEntryID == 0)
                {
                    Info.Text = dal.AddCheckEntryItem(selectedInfo) ? "零件校验码新增成功！" : "零件校验码新增失败！";
                }
                else
                {
                    Info.Text = dal.UpdateCheckEntryItem(selectedInfo) ? "零件校验码更改成功！" : "零件校验码更改失败！";
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
                        var selectedInfo = entryList.SelectedItem as CheckCodeInfoEntry;
                        if (selectedInfo == null)
                        {
                            Info.Text = "请选择项目!";
                            return;
                        }
                        Info.Text = dal.DeleteCheckEntryItem(selectedInfo.FEntryID) ? "零件校验码删除成功！" : "零件校验码删除失败！";

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
