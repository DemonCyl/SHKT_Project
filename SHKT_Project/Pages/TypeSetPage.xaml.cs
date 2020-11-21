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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SHKT_Project.Pages
{
    /// <summary>
    /// TypeSetPage.xaml 的交互逻辑
    /// </summary>
    public partial class TypeSetPage : Window
    {
        private int mark = 0;
        private MainDAL dal;
        private List<AssemblyType> AssemblyList = new List<AssemblyType>();
        private AssemblyType info;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public delegate void AssemblyTypeHandler(object sender, AssemblyType assemblyType);
        public event AssemblyTypeHandler assemblyTypeHandler;

        public TypeSetPage(MainDAL dal)
        {
            this.dal = dal;
            InitializeComponent();

            Query();
        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {
            saveButton.Visibility = Visibility.Hidden;
            Query();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            saveButton.Visibility = Visibility.Visible;
            TypeItems.Visibility = Visibility.Hidden;
            NewTypeItem.Visibility = Visibility.Visible;
            IdText.Text = "";
            mark = 1;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            if (mark != 1 && mark != 2)
            {
                Info.Text = "无法保存！";
                return;
            }

            try
            {
                info = new AssemblyType();
                if (NewTypeItem.Text.Equals(""))
                {
                    Info.Text = "装配类型不能为空!";
                    return;
                }
                info.FAssemblyName = NewTypeItem.Text.Trim();

                if (IdText.Text.Equals(""))
                {
                    if(dal.GetExistsType(info.FAssemblyName))
                    {
                        Info.Text = "已存在该装配类型！";
                        return;
                    }
                    result = dal.SaveAssemblyItem(info);
                }
                else
                {
                    info.FInterID = int.Parse(IdText.Text.Trim());
                    result = dal.UpdateAssemblyItem(info);
                }
                if (result)
                {
                    Query();
                    TypeItems.SelectedItem = info.FAssemblyName;
                }
                Info.Text = result ? "保存成功!" : "保存失败!";

                saveButton.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (!TypeItems.HasItems)
            {
                Info.Text = "请选择要切换的类型！";
                return;
            }

            var item = TypeItems.SelectedItem.ToString();
            var type = AssemblyList.Find(f => f.FAssemblyName == item);
            
            assemblyTypeHandler(this, type);

            this.Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxX.Show("确认删除？", "确认", Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    int id = 0;
                    if (IdText.Text == null || IdText.Text.Equals(""))
                    {
                        Info.Text = "未选择类型！";
                    }
                    else
                    {
                        id = int.Parse(IdText.Text.Trim());
                    }

                    if (dal.DeleteAssemblyItem(id))
                    {
                        Info.Text = "删除成功！";
                        Query();
                    }
                    else
                    {
                        Info.Text = "删除失败！";
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
            }
        }

        private void Query()
        {
            TypeItems.Visibility = Visibility.Visible;
            NewTypeItem.Visibility = Visibility.Hidden;
            NewTypeItem.Text = "";
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
                    TypeItems.ItemsSource = itemlist;
                    TypeItems.SelectedIndex = 0;

                    mark = 2;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeItems.HasItems)
            {
                var item = TypeItems.SelectedItem.ToString();
                var type = AssemblyList.Find(f => f.FAssemblyName == item);

                IdText.Text = type.FInterID.ToString();
            }
        }
    }
}
