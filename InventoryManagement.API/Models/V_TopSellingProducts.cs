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
    
    public partial class V_TopSellingProducts
    {
        public decimal GroupId { get; set; }
        public string PartyCode { get; set; }
        public string PartyName { get; set; }
        public decimal CatId { get; set; }
        public string ProdId { get; set; }
        public string ProductName { get; set; }
        public string BatchNo { get; set; }
        public decimal Customer { get; set; }
        public decimal Distributor { get; set; }
        public decimal Depot { get; set; }
        public decimal Branch { get; set; }
        public Nullable<decimal> TotalQty { get; set; }
    }
}
