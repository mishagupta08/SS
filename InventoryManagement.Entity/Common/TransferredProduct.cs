using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class TransferredProduct
    {
        public string RcptNo { get; set; }
        public string RDateStr { get; set; }
        public string IdNo { get; set; }
        public string MemName { get; set; }
        public string ProdID { get; set; }
        public string ProductName { get; set; }
        public int Qty { get; set; }
        public string ToMemberName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}