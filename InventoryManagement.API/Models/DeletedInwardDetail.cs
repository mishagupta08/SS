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
    
    public partial class DeletedInwardDetail
    {
        public decimal DID { get; set; }
        public decimal IdNo { get; set; }
        public decimal FSessId { get; set; }
        public string InwardBy { get; set; }
        public decimal GrNo { get; set; }
        public string InwardNo { get; set; }
        public System.DateTime InwardDate { get; set; }
        public string SupplierCode { get; set; }
        public string ProdCode { get; set; }
        public string ProdName { get; set; }
        public string BatchNo { get; set; }
        public string Barcode { get; set; }
        public decimal Qty { get; set; }
        public decimal FreeQty { get; set; }
        public decimal Mrp { get; set; }
        public decimal SaleRate { get; set; }
        public decimal Dp { get; set; }
        public decimal TradeDiscount { get; set; }
        public decimal TradeAmount { get; set; }
        public decimal LotDiscount { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal AValue { get; set; }
        public decimal AValueAmt { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal TotalAmt { get; set; }
        public string InwardFor { get; set; }
        public string Status { get; set; }
        public System.DateTime RecTimeStamp { get; set; }
        public decimal UserId { get; set; }
        public string Version { get; set; }
        public string ActiveStatus { get; set; }
        public string TaxType { get; set; }
        public decimal OrdQty { get; set; }
        public decimal ShortQty { get; set; }
        public decimal DemQty { get; set; }
        public decimal ExpQty { get; set; }
        public decimal RemQty { get; set; }
        public decimal ShortAmt { get; set; }
        public decimal DemAmt { get; set; }
        public decimal ExpAmt { get; set; }
        public decimal TtlDedcAmt { get; set; }
        public string BType { get; set; }
        public Nullable<System.DateTime> MfgDate { get; set; }
        public Nullable<System.DateTime> ExpDate { get; set; }
        public decimal CGST { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGST { get; set; }
        public decimal SGSTAmt { get; set; }
        public System.DateTime DeleteRecTimeStamp { get; set; }
        public decimal DUserId { get; set; }
        public string ItemCode { get; set; }
    }
}
