using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class PaytmGateway
    {
        public string ORDER_ID { get; set; }
        public string Mobile { get; set; }
        public string UniQID { get; set; }
        public string regid { get; set; }
        public string amount{ get; set; }        
        public string shpcharge{ get; set; }
        public string scmemtype { get; set; }
        public string email { get; set; }
        public string coupon { get; set; }
    }
}