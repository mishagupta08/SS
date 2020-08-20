using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class LogInfo
    {
        public DateTime LogDate { get; set; }
        public string LogStr { get; set; }
        public string UserName { get; set; }
        public decimal UserId { get; set; }
    }

    public class PaymentGatewayReport
    {
        public int ID { get; set; }
        public string IdNo { get; set; }
        public string PartyName{ get; set; }
        public Nullable<decimal> reqAmt { get; set; }
        public string mop { get; set; }
        public string Refno { get; set; }
        public Nullable<System.DateTime> invdate { get; set; }
        public string invbank { get; set; }
        public string invbranch { get; set; }
        public string depbank { get; set; }
        public string depbranch { get; set; }
        public string depcity { get; set; }
        public Nullable<System.DateTime> depdate { get; set; }
        public string depslip { get; set; }
        public string remarks { get; set; }
        public string ORDER_ID { get; set; }
        public string PaymentStatus { get; set; }
        public string BillStatus { get; set; }
        public string TXNID { get; set; }
        public string Recode { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string pgtype { get; set; }
        public string PGTranStatus { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
    }
}