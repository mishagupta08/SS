using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class Package
    {
        public decimal KId { get; set; }
        public decimal KitId { get; set; }
        public string KitName { get; set; }
        public decimal KitAmount { get; set; }
        public decimal KitUnit { get; set; }
        public decimal BV { get; set; }
        public string Remarks { get; set; }
        public string ActiveStatus { get; set; }
        public System.DateTime RecTimeStamp { get; set; }
        public string LastModified { get; set; }
        public string UserCode { get; set; }
        public decimal UserId { get; set; }
        public string IsAdd { get; set; }
        public ProductModel objProduct { get; set; }
        public List<ProductModel> objProductList { get; set; }
        public string objProductListStr { get; set; }
        public User UserDetails { get; set; }
    }

    public partial class PackageProducts
    {
        public decimal KID { get; set; }
        public decimal KPId { get; set; }
        public decimal KitId { get; set; }
        public decimal ProdId { get; set; }
        public string ProdName { get; set; }
        public string ActiveStatus { get; set; }
        public Nullable<System.DateTime> RecTimeStatus { get; set; }
        public string LastModified { get; set; }
        public decimal UserId { get; set; }
        public int Qty { get; set; }
        public string Barcode { get; set; }
        public string Itemcode { get; set; }
        public decimal DP { get; set; }
        public decimal Rate { get; set; }
        public decimal BV { get; set; }
    }
}