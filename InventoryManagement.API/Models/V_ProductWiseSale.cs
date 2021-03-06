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
    
    public partial class V_ProductWiseSale
    {
        public decimal FSessId { get; set; }
        public decimal SBillNo { get; set; }
        public string BillNo { get; set; }
        public decimal UserSBillNo { get; set; }
        public string UserBillNo { get; set; }
        public System.DateTime BillDate { get; set; }
        public string DispBillDate { get; set; }
        public string FCode { get; set; }
        public string PartyName { get; set; }
        public string TINNo { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public Nullable<decimal> Qty { get; set; }
        public decimal MRP { get; set; }
        public decimal DP { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountPer { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> TaxPer { get; set; }
        public Nullable<decimal> TaxAmount { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<decimal> NetPayable { get; set; }
        public Nullable<decimal> BVValue { get; set; }
        public Nullable<decimal> RPValue { get; set; }
        public string PartyType { get; set; }
        public string SoldPartyName { get; set; }
        public string StateName { get; set; }
        public string ProductType { get; set; }
    }
}
