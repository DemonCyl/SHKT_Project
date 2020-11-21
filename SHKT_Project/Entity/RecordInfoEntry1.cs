using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHKT_Project.Entity
{
    public class RecordInfoEntry1
    {
        public RecordInfoEntry1() { }

        public RecordInfoEntry1(float f)
        {
            this.FVaule = f;
        }

        [IgnoreColumn]
        public long FEntryID { get; set; }

        [IgnoreColumn]
        public long FInterID { get; set; }

        [DisplayName("气检数据")]
        [ColumnWidth("198")]
        public float FVaule { get; set; }

    }
}
