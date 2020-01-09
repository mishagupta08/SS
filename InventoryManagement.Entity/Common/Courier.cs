using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class Courier
    {
        public decimal ID { get; set; }
        public string Name { get; set; }
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string ActiveStatus { get; set; }
        public int UserId { get; set; }
    }

    public class CourierDetail
    {
        public string Name { get; set; }
        public string Remark { get; set; }
        public int USerId { get; set; }
    }
}