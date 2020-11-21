using log4net;
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
    /// AddCheckWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddCheckWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<AssemblyType> AssemblyList = new List<AssemblyType>();
        private MainDAL dal;
        private List<string> OPList = new List<string>();
        public delegate void AddCheckHandler(object sender, bool mark);
        public event AddCheckHandler addCheckHandler;

        public AddCheckWindow(MainDAL dal)
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
                    TypeItems.ItemsSource = itemlist;
                    TypeItems.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            CheckCodeInfo info = new CheckCodeInfo();
            var item = TypeItems.SelectedItem.ToString();
            if (TypeItems.HasItems && !string.IsNullOrEmpty(item))
            {
                var type = AssemblyList.Find(f => f.FAssemblyName == item);
                info.FAssemblyID = type.FInterID;
            }
            var gwItem = GwItems.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(gwItem))
            {
                info.FGWID = (GwType)Enum.Parse(typeof(GwType), gwItem);
            }

            var code = codeRule.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                Info.Text = "装配校验码不能为空！";
                return;
            }
            info.FBarCodeRule = code;

            if (dal.GetExistsCheckCode(info.FGWID,info.FAssemblyID))
            {
                Info.Text = "已存在此工位的装配类型数据！";
                return;
            }

            if (dal.AddCheckItem(info))
            {
                addCheckHandler(this, true);
                this.Close();
            }
            else
            {
                Info.Text = "新增失败";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            addCheckHandler(this, false);
            this.Close();
        }
    }
}
