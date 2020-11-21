using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHKT_Project.Entity
{
    public class RecordInfoEntry
    {
        public RecordInfoEntry() { }

        public RecordInfoEntry( string code)
        {
            this.FCode = code;
        }

        public RecordInfoEntry(long id,string code)
        {
            this.FInterID = id;
            this.FCode = code;
        }

        [IgnoreColumn]
        public long FEntryID { get; set; }

        [IgnoreColumn]
        public long FInterID { get; set; }

        [DisplayName("零件条码")]
        [ColumnWidth("198")]
        public string FCode { get; set; }

    }
}
