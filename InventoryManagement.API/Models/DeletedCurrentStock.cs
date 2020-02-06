//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InventoryManagement.API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DeletedCurrentStock
    {
        public decimal StockId { get; set; }
        public decimal FSessId { get; set; }
        public string SupplierCode { get; set; }
        public System.DateTime StockDate { get; set; }
        public string RefNo { get; set; }
        public string FCode { get; set; }
        public decimal GroupId { get; set; }
        public string ProdId { get; set; }
        public string BatchCode { get; set; }
        public string Barcode { get; set; }
        public decimal Qty { get; set; }
        public string SType { get; set; }
        public string BType { get; set; }
        public string Remarks { get; set; }
        public string ActiveStatus { get; set; }
        public string EntryBy { get; set; }
        public string StockFor { get; set; }
        public System.DateTime RecTimeStamp { get; set; }
        public decimal UserId { get; set; }
        public string Version { get; set; }
        public string IsDisp { get; set; }
        public string InvoiceNo { get; set; }
        public decimal DispQty { get; set; }
        public string BillType { get; set; }
        public string ProdType { get; set; }
        public decimal DUserId { get; set; }
        public string DReason { get; set; }
        public Nullable<System.DateTime> DRecTimeStamp { get; set; }
        public string ItemCode { get; set; }
    }
}
