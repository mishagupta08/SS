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
    
    public partial class V_AccountStatement
    {
        public System.DateTime VoucherDate { get; set; }
        public decimal VoucherNo { get; set; }
        public string RefNo { get; set; }
        public string Narration { get; set; }
        public string PartyCode { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public Nullable<decimal> Balance { get; set; }
    }
}
