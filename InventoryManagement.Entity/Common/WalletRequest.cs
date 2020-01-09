using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class WalletRequest
    {
        public DateTime ReqDate { get; set; }       
        public string ReqDateStr { get; set; }
        public string ApproveDateStr { get; set; }
        public string ReqNo { get; set; }
        public decimal ApproveBy { get; set; }
        public string ReqBy { get; set; }
        public string ReqByName { get; set; }
        public decimal PID { get; set; }
        public string Paymode { get; set; }
        public decimal Amount { get; set; }
        public string ChqNo { get; set; }
        public DateTime ChqDate { get; set; }
        public string ChqDateStr { get; set; }
        public decimal BankID { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string ScannedFileName { get; set; }
        public string Remarks { get; set; }
        public string GridString { get; set; }
        public string IsApproved { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApproveRemark { get; set; }
        public List<PaymodeModel> objListPaymode { get; set; }
        //public string ScannedFileName { get; set; }
    }

    public class PaymodeModel {
        public string PID { get; set; }
        public string Paymode { get; set; }
        public string TransNoLbl { get; set; }
        public string TransDateLbl { get; set; }
        public string IsBankDtl { get; set; }
        public string IsBranchDtl { get; set; }
        public string BankType { get; set; }
    }
}