using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class PaytmGateway
    {
        public string ORDER_ID { get; set; }
        public decimal Mobile { get; set; }
        public string UniQID { get; set; }
        public string regid { get; set; }
        public string amount{ get; set; }        
        public string shpcharge{ get; set; }
        public string scmemtype { get; set; }
        public string email { get; set; }
        public string coupon { get; set; }
        public string PaymentStatus { get; set; }
        public string BillStatus { get; set; }
        public string TxnId { get; set; }
        public string pgType { get; set; }
        public string pgTranStatus { get; set; }
        public string action { get; set; }
    }
}