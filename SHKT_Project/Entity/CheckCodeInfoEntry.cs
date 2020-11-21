using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHKT_Project.Entity
{
    public class CheckCodeInfoEntry
    {
        public CheckCodeInfoEntry() { }

        public CheckCodeInfoEntry(int id,string rule)
        {
            this.FInterID = id;
            this.FCodeRule = rule;
        }

        [IgnoreColumn]
        public int FEntryID { get; set; }

        [IgnoreColumn]
        public int FInterID { get; set; }

        [DisplayName("零件检验码")]
        [ColumnWidth("198")]
        public string FCodeRule { get; set; }

    }
}
