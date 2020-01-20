using System;
using System.Collections.Generic;

namespace InventoryManagement.Entity.Common
{
    public class Offer
    {
        public int AID { get; set; }
        public DateTime OfferFromDt { get; set; }
        public DateTime OfferToDt { get; set; }
        public string OfferFromDtStr { get; set; }
        public string OfferToDtStr { get; set; }
        public int? OfferStartDay { get; set; }
        public int? OfferEndDay { get; set; }
        public string OfferDatePart { get; set; }
        public decimal OfferOnValue { get; set; }
        public string OfferExceptSubCat { get; set; }
        public string FreeProdIDs { get; set; }
        public string FreeProdQty { get; set; }
        public decimal? OfferOnBV { get; set; }
        public string ConfFreeProdIDs { get; set; }
        public string ConfFreeProdQtys { get; set; }
        public string checkBillWith { get; set; }
        public string OfferBillType { get; set; }
        public string IdDateStr { get; set; }
        public Nullable<System.DateTime> IdDate { get; set; }
        public string IdStaus { get; set; }
        public string SortFirstBy { get; set; }
        public string ActiveStatus { get; set; }
        public string ForNewIds { get; set; }        
        public decimal? ForMonth { get; set; }
        public string ProductString { get; set; }
        public string ActionName { get; set; }
        public int? IdDays { get; set; }
        public List<OfferProduct> objProductList { get; set; }
        public string ParentProductString { get; set; }        
        public int LoggedinUser { get; set; }
        public List<OfferProduct> ParentProductList { get; set; }       
        public decimal? TotalOfferBv { get; set; }
        public decimal? TotalOfferBvper { get; set; }
        public string OfferType { get; set; }
        public string OfferName { get; set; }
        //new addition
        public string Remark { get; set; }
        public string ExtraPVApplicable { get; set; }
        public decimal? PVValue { get; set; }
        public decimal? PVPer { get; set; }
        public string CombineWithOffer { get; set; }
        public string CheckFirstBillWith { get; set; }
        public bool CombineOffer { get; set; }
        public decimal? CBAmount { get; set; }
        public int OfferFrequncy { get; set; }
        public int CustBillNo { get; set; }
    }

    public class OfferProduct
    {
        public decimal AID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Confirm { get; set; }
        public decimal Qty { get; set; }
        public decimal? PVValue { get; set; }
        public decimal? PVPer { get; set; }
        public bool IsParent { get; set; }
        public string IsBvApplied { get; set; }
        public string IsBuyProduct { get; set; }

        //new addition
        public decimal? Discount { get; set; }
        public decimal? DiscountPer { get; set; }
        public string OnMRP { get; set; }
        public string Scheme { get; set; }
        public decimal? Rupee { get; set; }
        public bool IncludeInOffer { get; set; }

        public decimal freeQty{ get; set; }
        public decimal PV { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string ProductType { get; set; }
    }

}