using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class CreateOffer
    {
        public string OfferType { get; set; }
        public List<Offer> OfferList { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public string ManageStock { get; set; }
        public string OfferFor { get; set; }
        public string ProductCode { get; set; }
        public string Batch { get; set; }
        public string ProductName { get; set; }
        public string ProdSaleOn { get; set; }
        public decimal MRP { get; set; }
        public decimal DP { get; set; }
        public decimal BV { get; set; }
        public decimal Qty { get; set; }
        public decimal FreeQty { get; set; }
        public string ParentProduct { get; set; }
        public string FreeProduct { get; set; }
        public int LoggedinUser { get; set; }
        public List<ProductModel> ParentProductList { get; set; }
        public List<ProductModel> FreeProductList { get; set; }
    }

    public class Offers {
        public decimal Id { get; set; }
        public string Name { get; set; }
    }

    
}