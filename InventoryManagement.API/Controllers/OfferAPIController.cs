using InventoryManagement.API.Models;
using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;

namespace InventoryManagement.API.Controllers
{
    public class OfferAPIController : Controller
    {
        public ResponseDetail saveOfferDynamicNew(Offer offerDetail)
        {
            ResponseDetail objResponse = new ResponseDetail();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    VisionOffer offer = new VisionOffer();
                    if (offerDetail.ActionName.ToLower() == "edit")
                    {
                        offer = (from r in entity.VisionOffers where r.OfferId == offerDetail.AID select r).FirstOrDefault();
                    }

                    var SplitDate = offerDetail.OfferFromDtStr.Split('-');
                    string NewDate = SplitDate[1] + "/" + SplitDate[0] + "/" + SplitDate[2];
                    offer.OfferFromDt = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));

                    SplitDate = offerDetail.OfferToDtStr.Split('-');
                    NewDate = SplitDate[1] + "/" + SplitDate[0] + "/" + SplitDate[2];
                    offer.OfferToDt = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));

                    if (!string.IsNullOrEmpty(offerDetail.IdDateStr))
                    {
                        SplitDate = offerDetail.IdDateStr.Split('-');
                        NewDate = SplitDate[1] + "/" + SplitDate[0] + "/" + SplitDate[2];
                        offer.IdDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    }

                    offer.ActiveStatus = offerDetail.ActiveStatus;

                    offer.OfferExceptSubCat = "0";

                    offer.OfferBillType = offerDetail.OfferBillType;

                    offer.OfferOnBV = offerDetail.OfferOnBV ?? 0;
                    offer.OfferOnValue = offerDetail.OfferOnValue;

                    offer.IsPVApplicable = offerDetail.ExtraPVApplicable;
                    if (offer.IsPVApplicable == "Y")
                    {
                        offer.ExtraPV = offerDetail.PVValue;
                    }
                    else
                    {
                        offer.ExtraPV = 0;
                    }

                    offer.ForNewIds = offerDetail.ForNewIds;

                    offer.IdStaus = string.IsNullOrEmpty(offerDetail.IdStaus)?"A": offerDetail.IdStaus;

                    offer.OfferType = offerDetail.OfferType;

                    offer.IdDays = offerDetail.IdDays;

                    offer.OfferBillType = offerDetail.OfferBillType;

                    offer.OfferName = offerDetail.OfferName;
                    offer.ConfFreeProdIDs = "";
                    offer.ConfFreeProdQtys = "";
                    offer.FreeProdIDs = "";
                    offer.Remarks = offerDetail.Remark;
                    offer.ContinueForMonth = offerDetail.ForMonth;
                    offer.CheckFirstBillWith =string.IsNullOrEmpty(offerDetail.checkBillWith) ?"A" : offerDetail.checkBillWith;
                    offer.CombineWithOffer = offerDetail.CombineWithOffer;
                    offer.CBAmount = offerDetail.CBAmount??0;
                    offer.OfferFrequency = offerDetail.OfferFrequncy;
                    if (offer.OfferBillType.ToLower() == "all")
                    {
                        offer.SortFirstBy = "V";
                    }
                    else
                    {
                        offer.SortFirstBy = "B";
                    }

                    offer.OfferDatePart = offerDetail.OfferDatePart;
                    if (offerDetail.OfferDatePart == "D")
                    {
                        offer.OfferStartDay = offerDetail.OfferStartDay;
                        offer.OfferEndDay = offerDetail.OfferEndDay;
                    }
                    else
                    {
                        offer.OfferStartDay = 1;
                        offer.OfferEndDay = 31;
                    }
                    int maxaid = 0;
                    if (offerDetail.ActionName.ToLower() == "add")
                    {
                        maxaid = (from r in entity.VisionOffers select r.OfferId).DefaultIfEmpty(0).Max();
                        maxaid += 1;
                        offer.OfferId = maxaid;
                        entity.VisionOffers.Add(offer);

                    }
                    else
                    {
                        maxaid = offerDetail.AID;
                        List<VisionOfferProduct> offerproduct = (from r in entity.VisionOfferProducts where r.OfferID == maxaid select r).ToList();
                        foreach (var record in offerproduct)
                        {
                            entity.VisionOfferProducts.Remove(record);
                        }
                    }

                    foreach (var product in offerDetail.objProductList)
                    {
                        VisionOfferProduct prod = new VisionOfferProduct();
                        prod.ProdID = product.ProductCode.ToString();
                        prod.ProdName = product.ProductName.ToString();
                        prod.Qty = product.Qty;
                        prod.OfferID = maxaid;
                        prod.OfferPV = product.PVValue;
                        prod.OfferPVPer = product.PVPer;
                        prod.RectimeStamp = DateTime.Now;
                        prod.ActiveStatus = "Y";
                        prod.IsConfirm = product.Confirm ?? "N";
                        prod.IsBuyProduct = product.IsParent ? "Y" : "N";
                        prod.OnMRP = product.OnMRP ?? "N";
                        prod.IsBVApplied = product.IsBvApplied ?? "N";
                        prod.scheme = product.Scheme;
                        prod.discount = product.Discount;
                        prod.discountPer = product.DiscountPer;
                        prod.ForRupee = product.Rupee;
                        prod.IncludeInOffer = product.IncludeInOffer;
                        entity.VisionOfferProducts.Add(prod);
                    }
                    int i = entity.SaveChanges();
                    if (i > 0)
                    {
                        objResponse.ResponseMessage = "Saved Successfully!";
                        objResponse.ResponseStatus = "OK";
                    }
                    else
                    {
                        objResponse.ResponseMessage = "Something went wrong!";
                        objResponse.ResponseStatus = "FAILED";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.ResponseMessage = "Something went wrong!";
                objResponse.ResponseStatus = "FAILED";
            }
            return objResponse;
        }

        public ResponseDetail CheckForOfferNew(DistributorBillModel objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            try
            {
                VisionOffer monthOfferList = new VisionOffer();
                VisionOffer EarlyRiserOffer = null;
                VisionOffer FirstBillOffer = null;
                var CurrentDate = DateTime.Now;
                using (var entity = new InventoryEntities())
                {
                    string OfferDatePart = ""; int OfferID = 0; string FreeProdIDs = ""; string FreeProdQtys = ""; string confProdIDs = ""; string ConfProdQtys = ""; string offerbillvalue = "0";

                    //if (objModel.objCustomer.IsFirstBill)
                    //{
                    if (!string.IsNullOrEmpty(objModel.objCustomer.Doj))
                    {
                        DateTime dateofjoining = Convert.ToDateTime(objModel.objCustomer.Doj);
                        DateTime checkDate = dateofjoining.AddDays(30);
                        //if (CurrentDate <= checkDate)
                        //{
                        //FirstBillOffer = (from r in entity.VisionOffers where r.OfferType.ToUpper() == "OFFERONVALUE" && r.OfferBillType.ToUpper()=="FIRSTBILL" && r.ActiveStatus.ToUpper() == "Y" && r.SortFirstBy.ToLower() == "B" && r.OfferOnValue <= objModel.objProduct.TotalNetPayable && r.OfferOnBV <= objModel.objProduct.TotalBV select r).OrderByDescending(o => o.OfferOnBV).FirstOrDefault();
                        FirstBillOffer = (from r in entity.VisionOffers where r.OfferType.ToUpper() == "OFFERONVALUE" && r.ActiveStatus.ToUpper() == "Y" && r.SortFirstBy.ToUpper() == "B" && r.OfferOnValue <= objModel.objProduct.TotalNetPayable && r.OfferOnBV <= objModel.objProduct.TotalBV select r).OrderByDescending(o => o.OfferOnBV).FirstOrDefault();
                        if (FirstBillOffer != null)
                        {
                            if (CurrentDate.Date >= FirstBillOffer.OfferFromDt.Date && CurrentDate.Date <= FirstBillOffer.OfferToDt.Date)
                                objResponse.ResponseStatus = "Success";
                            OfferDatePart = FirstBillOffer.OfferDatePart;
                            OfferID = FirstBillOffer.OfferId;
                            FreeProdIDs = FirstBillOffer.FreeProdIDs; FreeProdQtys = FirstBillOffer.FreeProdQtys.ToString();
                            confProdIDs = FirstBillOffer.ConfFreeProdIDs; ConfProdQtys = FirstBillOffer.ConfFreeProdQtys;
                            offerbillvalue = FirstBillOffer.OfferOnValue.ToString();
                        }
                        //}
                    }
                    //}
                    if (FirstBillOffer == null)
                    {
                        EarlyRiserOffer = (from r in entity.VisionOffers where r.OfferType.ToUpper() == "OFFERONVALUE" && r.ActiveStatus.ToUpper() == "Y" && r.SortFirstBy.ToUpper() != "B" && r.OfferDatePart == "D" && r.OfferOnValue <= objModel.objProduct.TotalNetPayable && r.OfferOnBV <= objModel.objProduct.TotalBV select r).OrderByDescending(o => o.OfferOnValue).FirstOrDefault();

                        if (EarlyRiserOffer != null)
                        {
                            if (CurrentDate.Date >= EarlyRiserOffer.OfferFromDt.Date && CurrentDate.Date <= EarlyRiserOffer.OfferToDt.Date)
                            {
                                if (CurrentDate.Date.Day >= EarlyRiserOffer.OfferStartDay && CurrentDate.Date.Day <= EarlyRiserOffer.OfferEndDay)
                                {
                                    objResponse.ResponseStatus = "Success";
                                    OfferDatePart = EarlyRiserOffer.OfferDatePart;
                                    OfferID = EarlyRiserOffer.OfferId;
                                    FreeProdIDs = EarlyRiserOffer.FreeProdIDs; FreeProdQtys = EarlyRiserOffer.FreeProdQtys.ToString();
                                    confProdIDs = EarlyRiserOffer.ConfFreeProdIDs; ConfProdQtys = EarlyRiserOffer.ConfFreeProdQtys;
                                    offerbillvalue = EarlyRiserOffer.OfferOnValue.ToString();
                                }
                            }
                        }
                        else
                        {
                            monthOfferList = (from r in entity.VisionOffers where r.OfferType.ToUpper() == "OFFERONVALUE" && r.ActiveStatus.ToUpper() == "Y" && r.SortFirstBy.ToUpper() != "B" && r.OfferDatePart == "R" && r.OfferOnValue <= objModel.objProduct.TotalNetPayable && r.OfferOnBV <= objModel.objProduct.TotalBV select r).OrderByDescending(o => o.OfferOnValue).FirstOrDefault();
                            if (monthOfferList != null)
                            {
                                if (CurrentDate.Date >= monthOfferList.OfferFromDt.Date && CurrentDate.Date <= monthOfferList.OfferToDt.Date)
                                    objResponse.ResponseStatus = "Success";
                                OfferDatePart = monthOfferList.OfferDatePart;
                                OfferID = monthOfferList.OfferId;
                                FreeProdIDs = monthOfferList.FreeProdIDs; FreeProdQtys = monthOfferList.FreeProdQtys.ToString();
                                confProdIDs = monthOfferList.ConfFreeProdIDs; ConfProdQtys = monthOfferList.ConfFreeProdQtys;
                                offerbillvalue = monthOfferList.OfferOnValue.ToString();
                            }
                        }
                    }
                    if (objResponse.ResponseStatus == "Success")
                    {
                        var freeproduct = FreeProdIDs.Split(',');
                        string productList = string.Empty;
                        //string Quant = string.Empty;
                        foreach (var prod in freeproduct)
                        {
                            if (prod != "")
                            {
                                var product = (from r in entity.M_ProductMaster where r.ProdId == prod select r.ProductName).FirstOrDefault();
                                productList += product + "~" + prod + ",";
                            }
                        }
                        if (productList.Length > 0) productList = productList.Substring(0, productList.Length - 1);
                        string confproductList = ""; string confproductQtyList = "";
                        if (confProdIDs != "")
                        {
                            string[] confproduct = confProdIDs.Split(',');
                            string[] confproductQty = ConfProdQtys.Split(',');
                            for (int i = 0; i < confproduct.Length; i++)
                            {
                                string ProdID_ = confproduct[i];
                                var product = (from r in entity.M_ProductMaster where r.ProdId == ProdID_ select r.ProductName).FirstOrDefault();
                                confproductList += product + "~" + confproduct[i] + ",";
                                confproductQtyList += confproductQty[i].ToString() + ",";
                            }
                            confproductList = confproductList.Substring(0, confproductList.Length - 1);
                        }
                        objResponse.ResponseMessage = OfferDatePart + "δ" + OfferID.ToString() + "δ" + productList + "δ" + FreeProdQtys.ToString() + "δ" + confproductList + "δ" + confproductQtyList + "δ" + offerbillvalue;
                    }
                    else
                        objResponse.ResponseMessage = "NoOffer";
                }
            }
            catch (Exception ex)
            {
                objResponse.ResponseStatus = "FAILED";
                objResponse.ResponseMessage = "Something went wrong";
            }
            return objResponse;
        }

        public Offer SelectedOfferDetailNew(string Offer)
        {
            Offer OfferDetail = new Offer();
            try
            {
                var CurrentDate = DateTime.Now;
                using (var entity = new InventoryEntities())
                {
                    decimal offerId = 0;
                    if (!string.IsNullOrEmpty(Offer))
                    {
                        offerId = Convert.ToDecimal(Offer);
                    }

                    OfferDetail = (from r in entity.VisionOffers
                                   where r.OfferId == offerId
                                   select new Offer
                                   {
                                       ForNewIds = r.ForNewIds,
                                       IdDate = r.IdDate,
                                       IdDays = r.IdDays,
                                       IdStaus = r.IdStaus,
                                       OfferBillType = r.OfferBillType,
                                       ExtraPVApplicable = r.IsPVApplicable,
                                       PVValue = r.ExtraPV,
                                       OfferOnBV = r.OfferOnBV,
                                       OfferOnValue = r.OfferOnValue
                                   }).FirstOrDefault();
                    if (OfferDetail != null)
                    {
                        OfferDetail.ParentProductList = new List<OfferProduct>();
                        OfferDetail.ParentProductList = (from r in entity.VisionOfferProducts
                                                         where r.OfferID == offerId && r.IsBuyProduct == "Y"
                                                         select new OfferProduct
                                                         {
                                                             ProductName = r.ProdName,
                                                             Qty = r.Qty,
                                                             OnMRP = r.OnMRP,
                                                             IsParent = true,
                                                             IncludeInOffer = r.IncludeInOffer??false
                                                         }).ToList();

                        OfferDetail.objProductList = new List<OfferProduct>();
                        OfferDetail.objProductList = (from r in entity.VisionOfferProducts
                                                      where r.OfferID == offerId && r.IsBuyProduct == "N"
                                                      select new OfferProduct
                                                      {
                                                          IsParent = false,
                                                          ProductName = r.ProdName,
                                                          Qty = r.Qty,
                                                          Scheme = r.scheme,
                                                          Discount = r.discount,
                                                          DiscountPer = r.discountPer,
                                                          PVPer = r.OfferPVPer,
                                                          PVValue = r.OfferPV,
                                                          Rupee = r.ForRupee
                                                      }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return OfferDetail;
        }

        public List<Offer> GetAllOffers()
        {
            List<Offer> offers = new List<Offer>();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    offers = (from r in entity.VisionOffers

                              select new Offer
                              {
                                  ActiveStatus = r.ActiveStatus,
                                  AID = r.OfferId,
                                  OfferFromDt = r.OfferFromDt,
                                  OfferToDt = r.OfferToDt,
                                  OfferOnValue = r.OfferOnValue,
                                  OfferOnBV = r.OfferOnBV,
                                  OfferType = r.OfferType,
                                  OfferName = r.OfferName
                              }).OrderBy(r => r.OfferType).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return offers;
        }

        public List<Offer> GetValidOfferList(string Doj, string UpgradeDate, string IsFirstBill, string ActiveStatus, string FormNo)
        {
            List<Offer> OfferList = new List<Offer>();
            try
            {
                string InvConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(InvConnectionString);
                string qry = "GetOfferCondition '"+ FormNo + "'";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = qry;
                cmd.Connection = SC;
           
                if (SC.State == System.Data.ConnectionState.Closed) SC.Open();
                string Condition = "";
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Condition = reader["Condition"].ToString();
                    }
                }
                qry = "select offerid,OfferName,isnull(CombineWithOffer,'') as Combinewithoffer,ForNewIds,OfferType,OfferBillType,offeronValue,OfferOnBV as OfferONPV from VRINv..VisionOffers " +
                " where ActiveStatus='Y'  " + Condition + "  ";
               cmd = new SqlCommand();
                cmd.CommandText = qry;
                cmd.Connection = SC;
                if (SC.State == System.Data.ConnectionState.Closed) SC.Open();      
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Offer obj = new Offer();
                        obj.AID = Convert.ToInt32( reader["OfferID"].ToString());
                        obj.OfferName = reader["OfferName"].ToString();
                        obj.OfferOnBV = Convert.ToDecimal( reader["OfferONPV"].ToString());
                        obj.OfferOnValue = Convert.ToDecimal(reader["OfferOnValue"].ToString());
                        obj.OfferType = reader["OfferType"].ToString();
                        obj.ForNewIds = reader["ForNewIds"].ToString();
                        obj.OfferBillType = reader["OfferBillType"].ToString();
                        obj.CombineOffer = reader["Combinewithoffer"].ToString().Length>0 ?true:false;
                        OfferList.Add(obj);

                    }
                }

                //if (SC.State == System.Data.ConnectionState.Open) SC.Close();
                //using (var entity = new InventoryEntities())
                //{
                //    List<VisionOffer> offers = new List<VisionOffer>();

                //    offers = (from r in entity.VisionOffers
                //              where r.ActiveStatus == "Y" && (r.OfferDatePart == "R" || (r.OfferDatePart == "D" && r.OfferStartDay <= CurrentDay && r.OfferEndDay >= CurrentDay))
                //              select r).ToList();

                //    OfferList = (from r in offers
                //                 where r.OfferFromDt.Date <= CurrentDate.Date && r.OfferToDt.Date >= CurrentDate.Date
                //                 select new Offer
                //                 {
                //                     AID = r.OfferId,
                //                     OfferOnBV = r.OfferOnBV,
                //                     OfferOnValue = r.OfferOnValue,
                //                     OfferType = r.OfferType,
                //                     ForNewIds = r.ForNewIds,
                //                     OfferBillType = r.OfferBillType,
                //                     OfferName = r.OfferName,
                //                     CombineOffer= r.CombineOffer??false
                //                 }).OrderBy(r => r.OfferType).ToList();

                //    //if (IsFirstBill == "true")
                //    //{
                //    //    OfferList = (from r in OfferList where r.OfferBillType.ToLower() != "repurchase" select r).ToList();
                //    //}
                //    //else
                //    //{
                //    //    OfferList = (from r in OfferList where r.OfferBillType.ToLower() != "firstbill" select r).ToList();
                //    //}                                                         
                //}
            }
            catch (Exception ex)
            {

            }
            return OfferList;
        }

        public Offer getOfferDetail(int id,string custId)
        {
            Offer offer = new Offer();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    offer = (from r in entity.VisionOffers
                             where r.OfferId == id
                             select new Offer
                             {
                                 AID = r.OfferId,
                                 ActiveStatus = r.ActiveStatus,
                                 IdDate = r.IdDate,
                                 IdStaus = r.IdStaus,
                                 ForMonth = r.ContinueForMonth,
                                 ForNewIds = r.ForNewIds,
                                 OfferFromDt = r.OfferFromDt,
                                 OfferToDt = r.OfferToDt,
                                 OfferBillType = r.OfferBillType,
                                 OfferOnValue = r.OfferOnValue,
                                 OfferOnBV = r.OfferOnBV,
                                 OfferDatePart = r.OfferDatePart,
                                 IdDays = r.IdDays,
                                 OfferStartDay = r.OfferStartDay,
                                 OfferEndDay = r.OfferEndDay,
                                 OfferName = r.OfferName,
                                 OfferType = r.OfferType,
                                 ExtraPVApplicable = r.IsPVApplicable,
                                 PVValue = r.ExtraPV,
                                 Remark = r.Remarks,
                                 CombineWithOffer = r.CombineWithOffer,
                                 checkBillWith = r.CheckFirstBillWith,
                                 CombineOffer = r.CombineOffer??false,
                                 CBAmount = r.CBAmount,
                                 OfferFrequncy = r.OfferFrequency??0
                             }).FirstOrDefault();

                    offer.OfferFromDtStr = offer.OfferFromDt.ToString("dd-MM-yyyy");
                    offer.OfferToDtStr = offer.OfferToDt.ToString("dd-MM-yyyy");
                    offer.IdDateStr = offer.IdDate.HasValue ? offer.IdDate.Value.ToString("dd-MM-yyyy") : string.Empty;
                    offer.CustBillNo = CheckOfferBill(id, custId) +1;
                    offer.objProductList = GetOfferProductsList(id);
                }
            }
            catch (Exception ex)
            {

            }
            return offer;
        }

        public List<OfferProduct> GetOfferProductsList(int id)
        {
            List<OfferProduct> objProductList = new List<OfferProduct>();
            try
            {
                using (var entity = new InventoryEntities())
                {

                    objProductList = (from r in entity.VisionOfferProducts
                                      where r.OfferID == id
                                      select new OfferProduct
                                      {
                                          AID = r.AID,
                                          ProductCode = r.ProdID,
                                          ProductName = r.ProdName,
                                          PVPer = r.OfferPVPer,
                                          PVValue = r.OfferPV,
                                          Qty = r.Qty,
                                          IsBvApplied = r.IsBVApplied,
                                          IsBuyProduct = r.IsBuyProduct,
                                          OnMRP = r.OnMRP,
                                          Confirm = r.IsConfirm,
                                          Discount = r.discount,
                                          DiscountPer = r.discountPer,
                                          Scheme = r.scheme,
                                          Rupee = r.ForRupee,
                                          IncludeInOffer = r.IncludeInOffer ?? false
                                      }).ToList();
                }

            }
            catch (Exception ex)
            {

            }
            return objProductList;
        }

        public ResponseDetail CheckOfferName(string offerName, int offerID)
        {
            ResponseDetail resp = new ResponseDetail();
            try
            {
                using (var db = new InventoryEntities())
                {
                    var result = (from r in db.VisionOffers where r.OfferName == offerName && r.OfferToDt >= DateTime.Now && r.OfferId!= offerID select r).FirstOrDefault();
                    if (result!= null)
                    { resp.ResponseStatus = "FAILED";
                        resp.ResponseMessage = "Offer name already exist.";
                    }
                    else
                    {
                        resp.ResponseStatus = "OK";
                        resp.ResponseMessage = "Success.";
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return resp;
        }

        public int CheckOfferBill(int OfferID, string CustID)
        {
            int bill = 0;
            try
            {
                decimal offer = 0;               
                using (var entity = new InventoryEntities())
                {
                    var list = (from r in entity.TrnBillDatas
                                where r.FCode == CustID && (r.OfferUID == OfferID || r.SpclOfferId == OfferID) && r.ActiveStatus == "Y" && r.FType == "M"
                                group r by new { r.BillNo } into BillResult
                                select new
                                {
                                    BillNo = BillResult.Key.BillNo
                                }).Count();
                    bill = list;
                }
            }
            catch (Exception ex)
            {
            }
            return bill;
        }

        public string CheckbtgtOffers(string checkwith, string check)
        {
            string response = "Sucess";
            try
            {
                if (!string.IsNullOrEmpty(checkwith) && !string.IsNullOrEmpty(check))
                {
                    int checkfor = Convert.ToInt32(check);
                    List<OfferProduct> checkproducts = GetOfferProductsList(checkfor).Where(r=>r.IsBuyProduct=="Y").ToList();
                    string[] checkwitharray = checkwith.Split(',');
                    for (int i = 0; i < checkwitharray.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(checkwitharray[i]))
                        {
                            int icheckwith = Convert.ToInt32(checkwitharray[i]);
                            List<OfferProduct> checkproductsby = GetOfferProductsList(checkfor).Where(r => r.IsBuyProduct == "Y").ToList();
                            foreach (var prod in checkproducts)
                            {
                                var IsExists = checkproductsby.Where(r => r.ProductCode == prod.ProductCode).FirstOrDefault();
                                if (IsExists != null)
                                {
                                    response = checkwitharray[i];
                                    return response;
                                }                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {

            }
            return response;
        }

        public List<OfferProduct> getProductsForOffer(string offerID)
        {
            List<OfferProduct> prodModel = new List<OfferProduct>();
            try
            {

                string Sql = string.Empty;
                string InvConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(InvConnectionString);

                SqlCommand cmd = new SqlCommand();
                if (!string.IsNullOrEmpty(offerID))
                {
                    Sql = "Exec GetFreeProducts '" + offerID + "'";
                    cmd.CommandText = Sql;
                    cmd.Connection = SC;
                    SC.Close();
                    SC.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OfferProduct tempobj = new OfferProduct();
                            tempobj.ProductCode = reader["ProdID"] != null ? reader["ProdID"].ToString() : "";
                            tempobj.ProductName = reader["ProdName"] != null ? reader["ProdName"].ToString() : "";
                            tempobj.Qty = reader["Qty"] != null ? Convert.ToDecimal(reader["Qty"]) : 0;
                            tempobj.Confirm = reader["IsConfirm"] != null ? reader["IsConfirm"].ToString() : "";
                            tempobj.Discount = reader["Discount"] != null ? Convert.ToDecimal(reader["Discount"]): 0;
                            tempobj.Rate = reader["Rate"] != null ? Convert.ToDecimal(reader["Rate"]) : 0;
                            tempobj.PV = reader["BV"] != null ? Convert.ToDecimal(reader["BV"].ToString()):0;
                            tempobj.PVValue = reader["BVValue"] != null ? Convert.ToDecimal(reader["BVValue"]):0;
                            tempobj.freeQty = reader["FreeQty"] != null ? Convert.ToDecimal(reader["FreeQty"]) : 0;
                            tempobj.ProductType = reader["ProdType"] != null ? reader["ProdType"].ToString() : "";
                            tempobj.Amount = reader["Amount"] != null ? Convert.ToDecimal( reader["Amount"].ToString()) : 0;                                                
                            prodModel.Add(tempobj);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return prodModel;
        }
    }
}