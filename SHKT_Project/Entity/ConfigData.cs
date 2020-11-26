using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHKT_Project.Entity
{
    public class ConfigData
    {
        #region 数据库配置
        public string DataIpAddress { get; set; }

        public string DataBaseName { get; set; }

        public string Uid { get; set; }

        public string Pwd { get; set; }
        #endregion

        #region 接收器配置
        public string PortName1 { get; set; }
        public int BaudRate1 { get; set; }

        public string PortName2 { get; set; }
        public int BaudRate2 { get; set; }

        public string PortName3 { get; set; }
        public int BaudRate3 { get; set; }

        public string PortName4 { get; set; }
        public int BaudRate4 { get; set; }
        #endregion

        public string IpAddress { get; set; }

        public int IpPort { get; set; }

    }

    public enum GwType
    {
        OP01 = 1,
        OP40 = 2,
        OP70 = 3,
        OP80 = 4
    }

}
