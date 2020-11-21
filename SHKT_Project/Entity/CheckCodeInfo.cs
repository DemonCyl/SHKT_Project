using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHKT_Project.Entity
{
    public class CheckCodeInfo
    {
        [IgnoreColumn]
        public int FInterID { get; set; }

        [DisplayName("工位")]
        [ColumnWidth("150")]
        [ReadOnlyColumn]
        public GwType FGWID { get; set; }

        [IgnoreColumn]
        public int FAssemblyID { get; set; }

        [DisplayName("装配类型")]
        [ColumnWidth("150")]
        [ReadOnlyColumn]
        public string FAssemblyName { get; set; }

        [DisplayName("装配检验码")]
        [ColumnWidth("246")]
        public string FBarCodeRule { get; set; }

        [IgnoreColumn]
        public List<CheckCodeInfoEntry> entries { get; set; }
    }
}
