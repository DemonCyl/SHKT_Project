using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHKT_Project.Entity
{
    public class RecordInfo
    {
        [IgnoreColumn]
        public long FInterID { get; set; }

        [DisplayName("工位")]
        [ColumnWidth("60")]
        [ReadOnlyColumn]
        public GwType FGWID { get; set; }

        [IgnoreColumn]
        public int FAssemblyID { get; set; }

        [DisplayName("装配类型")]
        [ColumnWidth("110")]
        [ReadOnlyColumn]
        public string FAssemblyName { get; set; }

        [DisplayName("装配条码")]
        [ColumnWidth("230")]
        public string FBar { get; set; }

        [IgnoreColumn]
        public string FCustBar { get; set; }

        [DisplayName("日期")]
        [ColumnWidth("188")]
        public DateTime FDate { get; set; }

        [IgnoreColumn]
        public List<RecordInfoEntry> entries { get; set; }

        [IgnoreColumn]
        public List<RecordInfoEntry1> entries1 { get; set; }
    }
}
