using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class ProductSummary
    {
        public string ProdID { get; set; }
        public string ProductName { get; set; }
        public decimal DP { get; set; }
        public decimal WrToFr { get; set; }
        public decimal WrToDist { get; set; }
        public decimal FrToDist { get; set; }
    }
}