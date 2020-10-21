using InventoryManagement.API.Models;
using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static InventoryManagement.Entity.Common.StockReportModel;

namespace InventoryManagement.API.Controllers
{
    public class ReportAPIController : ApiController
    {
        public List<ProductDetails> GetAllProducts(decimal CategoryCode)
        {
            List<ProductDetails> objProduct = new List<ProductDetails>();
            try
            {
                using(var entity=new InventoryEntities())
                {
                    if (CategoryCode == 0)
                    {
                        objProduct = (from product in entity.M_ProductMaster
                                      where product.ActiveStatus == "Y"
                                      select new ProductDetails
                                      {
                                          CategoryId=(int)product.CatId,
                                          ProductCodeStr = product.ProdId,
                                          ProductName = product.ProductName
                                      }

                                    ).ToList();
                    }
                    else
                    {
                        objProduct = (from product in entity.M_ProductMaster
                                      where product.ActiveStatus == "Y" && product.CatId==CategoryCode
                                      select new ProductDetails
                                      {
                                          CategoryId = (int)product.CatId,
                                          ProductCodeStr = product.ProdId,
                                          ProductName = product.ProductName
                                      }

                                    ).ToList();
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return objProduct;
        }

        public List<ProductDetails> GetTransferredProduct()
        {
            List<ProductDetails> objProduct = new List<ProductDetails>();
            try
            {
                string AppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(AppConnectionString);
                string query = "Select b.ProdID,b.ProductName FROM ";
query = query + " (Select DISTINCT ProdID FROM TrnProductTransfer) a,VRINV..M_ProductMaster b WHERE a.ProdID = b.ProdID";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = query;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();
               using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        objProduct.Add(new ProductDetails
                        {
                            ProductCodeStr = reader["ProdID"].ToString(),
                            ProductName = reader["ProductName"].ToString()
                        });
                    }
                }
                if (objProduct.Count == 0)
                {
                    objProduct.Add(new ProductDetails
                    {
                        ProductCodeStr = "0",
                        ProductName = "--No products transferred yet--"
                    });
                }
            }
            catch (Exception ex)
            {

            }
            return objProduct;
        }

        public List<TransferredProduct> TransProductReport(string FromDate, string ToDate, string ProdCode)
        {
            List<TransferredProduct> objTransSumm = new List<TransferredProduct>();
            try
            {
                string cond_ ="";
                if (FromDate != "" && FromDate.ToUpper() != "ALL")
                    cond_ = " AND Cast(a.RecTimeStamp as  Date)>='" + FromDate + "'";
                if (ToDate != "" && ToDate.ToUpper() != "ALL")
                    cond_ = cond_ + " AND Cast(a.RecTimeStamp as  Date)<='" + ToDate + "'";
                if (ProdCode!="0" && ProdCode != "")
                    cond_ = cond_ + " AND a.ProdID='" + ProdCode + "'";
                string AppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(AppConnectionString);

                string query = "Select a.OrderNo RcptNo,Replace(Convert(varchar,a.RecTimeStamp,106),' ','-') as RcptDate, a.RecTimeStamp as RDate,a.Formno,b.IdNo,b.MemFirstName as MemName,a.ProdID,a.ProductName,a.Qty,CASE WHEN OrderType='S' THEN '-Self-' ELSE a.ToMemberName END ToMemberName ,OrderType FROM TrnProductTransfer a,M_memberMaster b";
                query = query + " WHERE a.Formno = b.Formno AND a.ActiveStatus='Y'" + cond_;

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = query;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        objTransSumm.Add(
                            new TransferredProduct
                            {
                                RcptNo = reader["RcptNo"].ToString(),
                                RDateStr = reader["RcptDate"].ToString(),
                                IdNo=reader["IdNo"].ToString(),
                                MemName=reader["MemName"].ToString(),
                                ProdID=reader["ProdID"].ToString(),
                                ProductName=reader["ProductName"].ToString(),
                                Qty=Convert.ToInt32(reader["Qty"].ToString()),
                                ToMemberName =reader["ToMemberName"].ToString()
                            });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objTransSumm;
        }

        public List<PartyModel> GetAllParty()
        {
            List<PartyModel> objpartyList = new List<PartyModel>();
            try
            {
                using(var entity=new InventoryEntities())
                {
                    objpartyList = (from party in entity.M_LedgerMaster
                                    where party.ActiveStatus == "Y" && party.GroupId!=5
                                    orderby party.PartyName
                                    select new PartyModel
                                    {
                                        PartyCode=party.PartyCode,
                                        PartyName=party.PartyName,
                                        GroupId=party.GroupId
                                    }
                                 ).ToList();
                }
            }
            catch(Exception ex)
            {

            }

            return objpartyList;
        }

        public List<StockReportModel> GetStockReport(string CategoryCode, string ProductCode, string PartyCode, bool IsBatchWise, string StockType)
        {
            List<StockReportModel> objStockModel = new List<StockReportModel>();
            decimal CatId = 0;
            decimal ProdCode = 0;
            try
            {
                if (!string.IsNullOrEmpty(CategoryCode))
                {
                    CatId = decimal.Parse(CategoryCode);
                }
                if (!string.IsNullOrEmpty(ProductCode))
                {
                    ProdCode = decimal.Parse(ProductCode);
                }
                using (var entity = new InventoryEntities())
                {
                    if (IsBatchWise)
                    {
                        objStockModel = (from r in entity.V_CurrentStockDetailNotForStockist
                                         orderby r.CatName, r.ProductName, r.BatchCode
                                         select new StockReportModel
                                         {
                                             PartyCode = r.PartyCode,
                                             PartyName = r.PartyName,
                                             MRP = r.MRP.ToString(),
                                             RateOrDP = r.DP.ToString(),
                                             Qty = r.Qty.ToString(),
                                             Category = r.CatName,
                                             DPStockValue = r.DPStockValue.ToString(),
                                             StockValue = r.StockValue.ToString(),
                                             ProductCode = r.ProdId,
                                             ProductName = r.ProductName,
                                             MRPSTockValue = r.MRPStockValue.ToString(),
                                             // Expired = r.IsExpired == "Y" ? "Yes" : "No",
                                             MfgDate = r.MfgDate.ToString(),
                                             ExpDate = r.ExpDate.ToString(),
                                             ExpDateD = r.ExpDate,
                                             MfgDateD = r.MfgDate,
                                             BatchNo = r.BatchCode,
                                             InStock=r.StockIn??0,
                                             StockOut=r.StockOut??0,
                                             Barcode = r.Barcode,

                                             Quantity = r.Qty ,//?? 0,
                                             CatCode = r.CatId,
                                             PV = r.PV.ToString(),
                                             PVStockValue = r.PVStockValue.ToString()
                                         }

                                               ).ToList();

                    }
                    else
                    {
                        objStockModel = (from r in entity.V_CurrentStockDetailNotForStockist
                                         orderby r.CatName, r.ProductName
                                         select new StockReportModel
                                         {
                                             PartyCode = r.PartyCode,
                                             PartyName = r.PartyName,
                                             MRP = r.MRP.ToString(),
                                             RateOrDP = r.DP.ToString(),
                                             Qty = r.Qty.ToString(),
                                             Category = r.CatName,
                                             DPStockValue = r.DPStockValue.ToString(),
                                             StockValue = r.StockValue.ToString(),
                                             ProductCode = r.ProdId,
                                             ProductName = r.ProductName,
                                             MRPSTockValue = r.MRPStockValue.ToString(),
                                             Expired = "",
                                             MfgDate = "",
                                             ExpDate = "",
                                             BatchNo = r.BatchCode,
                                             Barcode = r.Barcode,
                                             InStock = r.StockIn ?? 0,
                                             StockOut = r.StockOut ?? 0,
                                             Quantity = r.Qty,// ?? 0,
                                             CatCode = r.CatId,
                                             PV = r.PV.ToString(),
                                             PVStockValue = r.PVStockValue.ToString()
                                         }

                                               ).ToList();

                    }
                    if (StockType == "FinishStock")
                    {
                        objStockModel = objStockModel.Where(r => r.Quantity <= 0).ToList();
                    }
                    else
                    {
                        objStockModel = objStockModel.Where(r => r.Quantity > 0).ToList();
                    }

                    if (CatId != 0)
                    {
                        objStockModel = objStockModel.Where(r => r.CatCode==CatId).ToList();
                    }
                    if (ProdCode != 0)
                    {
                        objStockModel = objStockModel.Where(r => r.ProductCode == ProductCode).ToList();
                    }
                    if(PartyCode!="0" && PartyCode != "All")
                    {
                        objStockModel = objStockModel.Where(r => r.PartyCode == PartyCode).ToList();
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return objStockModel;
        }

        public List<StockReportModel> GetDateWiseStockReport(string CategoryCode, string ProductCode, string PartyCode, string FromDate, string ToDate)
        {
            List<StockReportModel> objStockModel = new List<StockReportModel>();
            decimal CatId = 0;
            decimal ProdCode = 0;
            try
            {

                DateTime startDate = Convert.ToDateTime(FromDate);
                DateTime endDate = Convert.ToDateTime(ToDate);
                if (!string.IsNullOrEmpty(ProductCode))
                {
                    ProdCode = decimal.Parse(ProductCode);
                }
                using (var entity = new InventoryEntities())
                {
                   
                    
                        objStockModel = (from r in entity.StockDetail(startDate, endDate)
                                         where ((PartyCode != "0" && PartyCode != "All" && r.FCode==PartyCode)|| PartyCode == "0" || PartyCode== "" || PartyCode == "All")
                                         orderby r.ProductName
                                         select new StockReportModel
                                         {
                                             PartyCode = r.FCode,
                                             PartyName = r.PartyName,
                                             ProductCode = r.ProdID,
                                             ItemCode=r.ItemCode,
                                             ProductName = r.ProductName,
                                             OpStock =r.OpStock,
                                             InStock =r.InStock,
                                             StockOut=r.StockOut,
                                             ClsStock=r.ClsStock,
                                             OpStockValue = r.OpStockValue??0,
                                             InStockValue = r.InStockValue ?? 0,
                                             StockOutValue = r.StockOutValue ?? 0,
                                             ClsStockValue = r.ClsStockValue ?? 0
                                         }
                                       ).ToList();

                    
                   if (ProdCode != 0)
                    {
                        objStockModel = objStockModel.Where(r => r.ProductCode == ProductCode).ToList();
                    }
                    if (PartyCode != "0" && PartyCode != "All")
                    {
                        objStockModel = objStockModel.Where(r => r.PartyCode == PartyCode).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objStockModel;
        }

        public List<StockReportModel> GetDailyStockReport(string CategoryCode, string ProductCode, string PartyCode, string FromDate, string ToDate)
        {
            List<StockReportModel> objStockModel = new List<StockReportModel>();
            decimal CatId = 0;
            decimal ProdCode = 0;
            try
            {
                DateTime startDate = Convert.ToDateTime(FromDate);
                DateTime endDate = Convert.ToDateTime(ToDate);
                string PartyCond = "";
                if (PartyCode != "" && PartyCode.ToUpper() != "ALL" && PartyCode != "0") PartyCond = " AND PartyCode='" + PartyCode + "'";
                string AppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(AppConnectionString);
                string query = "Select * FROM DailyStockDetail ('" + startDate.ToString("dd-MMM-yyyy") + "','" + endDate.ToString("dd-MMM-yyyy") + "','" + ProductCode + "')  WHERE (StockDateStr='" + startDate.ToString("dd-MMM-yyyy") + "' OR  StockDateStr='" + endDate.ToString("dd-MMM-yyyy") + "' OR InStock>0 OR StockOut>0  )" + PartyCond + " ORDER BY PartyCode,ProductName,StockDate";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = query;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        objStockModel.Add(new StockReportModel
                        {
                            PartyCode = reader["PartyCode"].ToString(),
                            PartyName = reader["PartyName"].ToString(),
                            ProductCode = reader["ProdID"].ToString(),
                            ItemCode = reader["ItemCode"].ToString(),
                            ProductName = reader["ProductName"].ToString(),
                            StockDateStr = reader["StockDateStr"].ToString(),
                            OpStock = Convert.ToDecimal(reader["OpStock"].ToString()),
                            InStock = Convert.ToDecimal(reader["InStock"].ToString()),
                            StockOut = Convert.ToDecimal(reader["StockOut"].ToString()),
                            ClsStock = Convert.ToDecimal(reader["ClsStock"].ToString()),
                            OpStockValue = Convert.ToDecimal(reader["OpStockValue"].ToString()),
                            InStockValue = Convert.ToDecimal(reader["InStockValue"].ToString()),
                            StockOutValue = Convert.ToDecimal(reader["StockOutValue"].ToString()),
                            ClsStockValue = Convert.ToDecimal(reader["ClsStockValue"].ToString())
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objStockModel;
        }

        public List<SalesReport> GetSalesReport(string FromDate, string ToDate, string CustomerId, string ProductCode, string CategoryCode, string PartyCode, string BType, string SalesType, string InvoiceType,string BillNo,string FType, decimal OfferWise)
        {
            List<SalesReport> objListSales = new List<SalesReport>();
            DateTime StartDate = DateTime.Now;
            DateTime EndDate = DateTime.Now;
            string SummType = "";
            if (SalesType.Contains("ProductWise"))
            {
                {
                    if (SalesType.ToLower() == "ProductWiseSumm".ToLower())
                        SummType = "Summ";
                    else
                        SummType = "Detail";
                }
                SalesType = "ProductWise";
            }
            try
            {
                using(var entity=new InventoryEntities())
                {
                    // string sqlFromdate = "", sqlToDate = "";
                    if (!string.IsNullOrEmpty(FromDate) && (!string.IsNullOrEmpty(ToDate)))
                    {
                        if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                        {
                            var SplitDate = FromDate.Split('-');
                            string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                            StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                            StartDate = StartDate.Date;
                        }
                        if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                        {
                            var SplitDate = ToDate.Split('-');
                            string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                            EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                            EndDate = EndDate.Date;
                        }
                        switch (SalesType)
                        {
                            case "":
                                break;
                            case "BillWise":
                                List<SalesReport> objSales = new List<SalesReport>();
                                objSales = (from result in entity.V_BillWiseSaleSummary
                                            //where result.BType != BType
                                            group result by new { result.UserBillNo, result.SBillNo, result.BillNo, result.BillDate,result.BillDateStr,result.OfferUID,result.FType, result.PartyCode, result.PartyName, result.FCode, result.Name,result.PVValue   , result.OrderNo, result.FSessId, result.OrderDate, result.BillType, result.SGSTAmt, result.CGSTAmt, result.IGSTAmt,result.Series,result.ISChallanBill }
                                            into BillResult
                                            orderby BillResult.Key.BillDate descending, BillResult.Key.SBillNo descending, BillResult.Key.PartyCode, BillResult.Key.FType
                                            select new SalesReport
                                            {
                                                BillNo = BillResult.Key.UserBillNo,
                                                InternalBillNo = BillResult.Key.BillNo,
                                                InternalsBillNo = BillResult.Key.SBillNo,
                                                BillDate = BillResult.Key.BillDate,
                                                StrBillDate=BillResult.Key.BillDateStr,
                                                PartyName = BillResult.Key.PartyName,
                                                PartyCode = BillResult.Key.PartyCode,
                                                CustCode = BillResult.Key.FCode,
                                                CustName = BillResult.Key.Name,
                                                Amount = BillResult.Sum(m => m.Amount).ToString(),
                                                TotalBV = BillResult.Sum(m => m.PVValue).ToString(), //BillResult.Sum(m => m.BVValue).ToString(),
                                                NetAmount = BillResult.Sum(m => m.NetAmt).ToString(),
                                                CGSTAmount = BillResult.Sum(m => m.CGSTAmt).ToString(),
                                                SGSTAmount = BillResult.Sum(m => m.SGSTAmt).ToString(),
                                                IGSTAmount = BillResult.Sum(m => m.TaxAmount).ToString(),
                                                //TaxAmount = BillResult.Sum(m => m.TaxAmount).ToString()
                                                FsessId = BillResult.Key.FSessId,
                                                OfferUID=  BillResult.Key.OfferUID,
                                                IsDelete = false,
                                                Reason = "",
                                                UserId = 0,
                                                InvoiceType = BillResult.Key.BillType,
                                                FType=BillResult.Key.FType,
                                                ChallanAgainst = BillResult.Key.ISChallanBill!=null && BillResult.Key.ISChallanBill == "Y"? "Challan Against "+ BillResult.Key.Series : "",
                                            }).ToList();

                                if (FType != null && FType != "" && FType != "A")
                                {
                                    if (FType != "M" && FType != "W")
                                    {
                                        objSales = objSales.Where(m => m.FType != "M").ToList();
                                        objSales = objSales.Where(m => m.FType != "W").ToList();
                                    }
                                    else
                                    objSales = objSales.Where(m => m.FType == FType).ToList();
                                }

                             if (OfferWise==0)
                                    objSales = objSales.Where(m => m.OfferUID == 0).ToList();
                             if (OfferWise==1)
                                    objSales = objSales.Where(m => m.OfferUID >0).ToList();

                                if (CustomerId != "All")
                                {
                                    objSales = objSales.Where(m => m.CustCode == CustomerId).ToList();
                                }
                                if(PartyCode!="All" && PartyCode != "0")
                                {
                                    objSales = objSales.Where(m => m.PartyCode == PartyCode).ToList();
                                }
                                if (InvoiceType != "")
                                {
                                    if (InvoiceType == "RI")
                                    {
                                        objSales = objSales.Where(m => m.InvoiceType != "B" && m.InvoiceType.ToLower() != "stock transfer").ToList();
                                    }
                                    else if(InvoiceType=="JI")
                                    {
                                        objSales = objSales.Where(m => m.InvoiceType == "B" && m.InvoiceType.ToLower() != "stock transfer").ToList();
                                    }
                                    else if (InvoiceType == "CB")
                                    {
                                        objSales = objSales.Where(m => m.ChallanAgainst != "").ToList();
                                    }
                                    else if (InvoiceType == "S")
                                    {
                                        objSales = objSales.Where(m => m.InvoiceType.ToLower() == "stock transfer").ToList();
                                    }
                                }
                                if (FromDate != "All" && ToDate != "All")
                                {
                                    foreach (var obj in objSales)
                                    {
                                        if (obj.BillDate >= StartDate.Date && obj.BillDate <= EndDate.Date)
                                        {
                                            objListSales.Add(obj);
                                        }
                                    }
                                }
                                else if (FromDate == "All" && ToDate != "All")
                                {
                                    foreach (var obj in objSales)
                                    {
                                        if (obj.BillDate <= EndDate.Date)
                                        {
                                            objListSales.Add(obj);
                                        }
                                    }
                                    
                                }
                                else if (FromDate != "All" && ToDate == "All")
                                {
                                    foreach (var obj in objSales)
                                    {
                                        if (obj.BillDate >= StartDate.Date)
                                        {
                                            objListSales.Add(obj);
                                        }
                                    }

                                }
                                else if (!string.IsNullOrEmpty(BillNo) && BillNo != "0" && BillNo != "All")
                                {

                                    objListSales = objSales.Where(m => m.BillNo == BillNo).ToList();
                                       
                                }
                                else
                                {
                                    foreach (var obj in objSales)
                                    {
                                        objListSales.Add(obj);
                                    }
                                }
                                
                                    break;
                            case "DateWise":
                                objSales = new List<SalesReport>();
                                objSales = (from m in (from r in entity.TrnBillMains
								                       //where r.SoldBy== PartyCode
                                                       from t in
                        (from p in entity.TrnPayModeDetails
                         where p.PayPrefix == "W" && p.BillNo == r.BillNo
                             //group p by new { p.BillNo, r.UserBillNo } into g
                             select new
                         {
                             WalletpaidAmount = p.Amount,
                             BillNo = r.UserBillNo
                         }
                        ).DefaultIfEmpty()

                                                       select new { r, t })
                                           
                                            group m by EntityFunctions.TruncateTime(m.r.BillDate)
                                                   into grouped
                                            orderby new { grouped.Key.Value }
                                            select new SalesReport
                                            {
                                                TotalBV = grouped.Sum(m => m.r.BvValue).ToString(),
                                                TotalQty = grouped.Sum(m => m.r.TotalQty).ToString(),
                                                TotalBillAmt = grouped.Sum(m => m.r.NetPayable).ToString(),
                                                RecordDate = grouped.Key.Value,
                                                NoOfBills = grouped.Count().ToString(),
                                                TotalAmount = grouped.Sum(m => m.r.Amount).ToString(),
                                                TotalTaxAmount = grouped.Sum(m => (m.r.TaxAmount + m.r.CGSTAmt + m.r.STaxAmount)).ToString(),
                                                TotalPaidByWallet = grouped.Sum(m => m.t.WalletpaidAmount).ToString()
                                                }
                                                   ).ToList();
                                if (PartyCode != "All" && PartyCode != "0" && PartyCode != "")
                                {
                                    objSales = objSales.Where(m => m.SoldBy == PartyCode).ToList(); 
                                }
                                if (FromDate != "All" && ToDate != "All") {
                                    foreach(var obj in objSales)
                                    {
                                        if(obj.RecordDate >= StartDate && obj.RecordDate <= EndDate)
                                        {
                                            objListSales.Add(obj);
                                        }
                                    }
                                }
                                else if (FromDate != "All" && ToDate == "All")
                                {
                                    
                                    foreach (var obj in objSales)
                                    {
                                        if (obj.RecordDate >= StartDate)
                                        {
                                            objListSales.Add(obj);
                                        }
                                    }
                                }
                                else if (FromDate == "All" && ToDate != "All")
                                {
                                    
                                    foreach (var obj in objSales)
                                    {
                                        if (obj.RecordDate <= EndDate)
                                        {
                                            objListSales.Add(obj);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var obj in objSales)
                                    {

                                        objListSales.Add(obj);

                                    }
                                    
                                }
                               
                                break;
                            case "ProductWise":
                                decimal CatCode = 0;
                                if (FromDate.ToUpper() == "ALL")
                                    StartDate = DateTime.Now.AddYears(-10);
                                if (ToDate.ToUpper() == "ALL")
                                    EndDate = DateTime.Now;
                                objSales = new List<SalesReport>();
                                if (!string.IsNullOrEmpty(CategoryCode))
                                {
                                    CatCode = decimal.Parse(CategoryCode);
                                }
                                if (SummType=="Summ")
                                objSales = (from r in entity.TrnBillDetails
                                            join t in entity.TrnBillMains on r.BillNo equals t.BillNo
                                            where t.BillDate >= StartDate && t.BillDate <= EndDate
                                            join l in entity.M_LedgerMaster on t.SoldBy equals l.PartyCode
                                            join p in entity.M_ProductMaster on r.ProductId equals p.ProdId
                                           // where r.ProdType == "P"
                                            group r by new { p.CatId, r.ProductId, r.ProductName, r.SoldBy,r.FType,  l.PartyName, r.Rate, TaxPerc= (  r.Tax+ r.CGST+ r.SGST )}
                                                    into g
                                            //where g.Key.SoldBy == PartyCode
                                            orderby g.Key.CatId, g.Key.ProductId
                                            select new SalesReport
                                            {
                                                //IdNo = g.Key.FCode,
                                                //Name = g.Key.PartyName,
                                                ProdCode = g.Key.ProductId,
                                                ProductName = g.Key.ProductName,
                                                TaxPer =( g.Key.TaxPerc).ToString(),//**
                                                TaxAmount = g.Sum(n=>n.TaxAmount).ToString(),
                                                CGSTAmount = g.Sum(n=>n.CGSTAmt).ToString(),
                                                SGSTAmount = g.Sum(n=>n.CGSTAmt).ToString(),
                                                //SGST = g.Key.SGST.ToString(),
                                                //CGST = g.Key.CGST.ToString(),
                                                //DiscPer = g.Key.DiscountPer.ToString(),
                                                DiscAmt = g.Sum(n=>n.Discount).ToString(),
                                                Rate = g.Key.Rate.ToString(),
                                                BasicAmt = g.Sum(n=>n.NetAmount).ToString(),
                                                Qty = g.Sum(n=>n.Qty).ToString(),
                                                NetAmount = g.Sum(n => n.NetAmount).ToString(),
                                                TotalAmt = g.Sum(n => n.NetAmount+n.CGSTAmt+n.SGSTAmt+n.TaxAmount).ToString(),//**
                                                CatCode=g.Key.CatId,
                                                PartyCode=g.Key.SoldBy,
                                                PartyName=g.Key.PartyName,
                                                FType=g.Key.FType

                                            }

                                              ).ToList();
                                else
                                    objSales = (from r in entity.TrnBillDetails
                                                join t in entity.TrnBillMains on r.BillNo equals t.BillNo
                                                where t.BillDate >= StartDate && t.BillDate <= EndDate
                                                join l in entity.M_LedgerMaster on t.SoldBy equals l.PartyCode
                                                join p in entity.M_ProductMaster on r.ProductId equals p.ProdId
                                                orderby t.BillNo,r.ProductId
                                                select new SalesReport
                                                {
                                                    //IdNo = g.Key.FCode,
                                                    //Name = g.Key.PartyName,
                                                    BillNo=t.UserBillNo,
                                                    RecordDate = t.BillDate,CustCode=t.FCode,CustName=t.PartyName,
                                                    ProdCode = r.ProductId,
                                                    ProductName = r.ProductName,
                                                    IGST = r.Tax.ToString(),//**
                                                    TaxPer = (r.Tax).ToString(),//**
                                                    TaxAmount = r.TaxAmount.ToString(),
                                                    CGSTAmount = r.CGSTAmt.ToString(),
                                                    SGSTAmount = r.CGSTAmt.ToString(),
                                                    SGST = r.SGST.ToString(),
                                                    CGST = r.CGST.ToString(),
                                                    //DiscPer = g.Key.DiscountPer.ToString(),
                                                    DiscAmt = r.Discount.ToString(),
                                                    Rate = r.Rate.ToString(),
                                                    PV = p.BV.ToString(),
                                                    BasicAmt = r.NetAmount.ToString(),
                                                    Qty = r.Qty.ToString(),
                                                    NetAmount =r.NetAmount.ToString(),
                                                   // TotalAmt = (r => r.NetAmount + r.CGSTAmt + r.SGSTAmt + r.TaxAmount).ToString(),//**
                                                    CatCode = p.CatId,
                                                    PartyCode = t.SoldBy,
                                                    PartyName = l.PartyName,
                                                    FType = t.FType
                                                }).ToList();

                                if (CatCode != 0)
                                {
                                    objSales = objSales.Where(m => m.CatCode == CatCode).ToList();
                                }
                                if (FType != null && FType != "" && FType != "A")
                                {
                                    if (FType != "M" && FType != "W")
                                    {
                                        objSales = objSales.Where(m => m.FType != "M").ToList();
                                        objSales = objSales.Where(m => m.FType != "W").ToList();
                                    }
                                    else
                                        objSales = objSales.Where(m => m.FType == FType).ToList();
                                }
                                if (ProductCode != "0")
                                {
                                    objSales = objSales.Where(m => m.ProdCode == ProductCode).ToList();
                                }
                                if (PartyCode != "All" && PartyCode != "0" && PartyCode != "")
                                {
                                    objSales = objSales.Where(m => m.PartyCode == PartyCode).ToList();
                                }
                                // if (FromDate!="All" && ToDate != "All")
                                // {
                                if (SummType != "Summ")
                                {
                                    foreach (var obj in objSales)
                                    {
                                      
                                        obj.StrBillDate = obj.RecordDate.ToString("dd-MMM-yyyy");
                                        obj.TaxPer = (Convert.ToDecimal( obj.IGST) + Convert.ToDecimal(obj.CGST) + Convert.ToDecimal(obj.SGST)).ToString();
                                        obj.TotalAmount =(Convert.ToDecimal(obj.NetAmount) + Convert.ToDecimal(obj.TaxAmount) + Convert.ToDecimal(obj.CGSTAmount) + Convert.ToDecimal(obj.SGSTAmount)).ToString();
                                        objListSales.Add(obj);
                                    }
                                }
                                else
                                {
                                    foreach (var obj in objSales)
                                    {
                                        objListSales.Add(obj);
                                    }
                                }

                             

                                //}
                                // else if (FromDate != "All" && ToDate == "All")
                                // {
                                //     foreach (var obj in objSales)
                                //     {
                                //         if (obj.BillDate >= StartDate)
                                //             objListSales.Add(obj);
                                //     }

                                // }
                                // else if (FromDate == "All" && ToDate != "All")
                                // {
                                //     foreach (var obj in objSales)
                                //     {
                                //         if (obj.BillDate <= EndDate)
                                //             objListSales.Add(obj);
                                //     }
                                // }
                                // else
                                // {
                                //     foreach (var obj in objSales)
                                //     {

                                //         objListSales.Add(obj);
                                //     }
                                // }
                                //"PartyCode": objResult[i].PartyCode, "PartyName": objResult[i].PartyName, "ProdCode": objResult[i].ProdCode, "ProductName": objResult[i].ProductName, "Qty": objResult[i].Qty, "Rate": objResult[i].Rate, "DiscPer": objResult[i].DiscPer, "DiscAmt": objResult[i].DiscAmt, "TaxPer": Tax, "TaxAmt": TaxAmt, "BasicAmt": objResult[i].BasicAmt, "TotalAmt": objResult[i].TotalAmt });
                                //objListSales = objListSales.GroupBy(m => new { m.PartyCode, m.PartyName, m.ProdCode, m.ProductName, m.Rate, m.DiscAmt, m.TaxPer }).Select(g => new SalesReport() {
                                //    ProdCode = g.Key.ProdCode,
                                //    ProductName = g.Key.ProductName,
                                //    TaxPer = g.Key.TaxPer.ToString(),
                                //    TaxAmount = g.Sum(r=> r.TaxAmount).ToString(),
                                //    CGSTAmount = g.Key.CGSTAmt.ToString(),
                                //    SGSTAmount = g.Key.CGSTAmt.ToString(),
                                //    SGST = g.Key.SGST.ToString(),
                                //    CGST = g.Key.CGST.ToString(),
                                //   // DiscPer = g.Key.DiscountPer.ToString(),
                                //    DiscAmt = g.Key.Discount.ToString(),
                                //    Rate = g.Key.Rate.ToString(),
                                //    BasicAmt = g.Key.NetAmount.ToString(),
                                //    Qty = g.Key.Qty.ToString(),
                                //    TotalAmt = (g.Key.NetAmount + g.Key.TaxAmount + g.Key.CGSTAmt + g.Key.SGSTAmt).ToString(),
                                //    BillDate = g.Key.BillDate,
                                //    CatCode = g.Key.CatId,
                                //    PartyCode = g.Key.SoldBy,
                                //    PartyName = g.Key.PartyName,
                                //    FType = g.Key.FType
                                //}).
                                break;
                            default:
                                break;
                        }

                    }

                }
            }
            catch(Exception ex)
            {
                //objListSales = new List<SalesReport>();
                //objListSales[0].ErrorMsg = ex.Message;
            }
            return objListSales;
        }

        public List<SalesReport> GetDeletedSalesReport(string FromDate, string ToDate, string CustomerId, string PartyCode, string BType,  string InvoiceType, string BillNo, string FType, decimal OfferWise, int DltDateWise)
        {
            List<SalesReport> objListSales = new List<SalesReport>();
            DateTime StartDate = DateTime.Now;
            DateTime EndDate = DateTime.Now;
            try
            {
                using (var entity = new InventoryEntities())
                {
                    // string sqlFromdate = "", sqlToDate = "";
                    if (!string.IsNullOrEmpty(FromDate) && (!string.IsNullOrEmpty(ToDate)))
                    {
                        if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                        {
                            var SplitDate = FromDate.Split('-');
                            string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                            StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                            StartDate = StartDate.Date;
                        }
                        if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                        {
                            var SplitDate = ToDate.Split('-');
                            string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                            EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                            EndDate = EndDate.Date;
                        }

                        List<SalesReport> objSales = new List<SalesReport>();
                                objSales = (from result in entity.V_DeletedSaleSummary
                                            where result.BType != BType
                                            group result by new { result.UserBillNo, result.SBillNo, result.BillNo,result.UID, result.BillDate, result.BillDateStr, result.OfferUID, result.FType, result.PartyCode, result.PartyName, result.FCode, result.Name, result.OrderNo, result.FSessId, result.OrderDate, result.BillType, result.SGSTAmt, result.CGSTAmt, result.IGSTAmt,result.DltDate,result.Dreason,result.Username,result.DRecTimeStamp }
                                                    into BillResult
                                            orderby BillResult.Key.BillDate descending, BillResult.Key.SBillNo descending, BillResult.Key.PartyCode, BillResult.Key.FType
                                            select new SalesReport
                                            {
                                                BillNo = BillResult.Key.UserBillNo,
                                                InternalBillNo = BillResult.Key.BillNo,
                                                InternalsBillNo = BillResult.Key.SBillNo,
                                                BillDate = BillResult.Key.BillDate,
                                                StrBillDate = BillResult.Key.BillDateStr,
                                                PartyName = BillResult.Key.PartyName,
                                                PartyCode = BillResult.Key.PartyCode,
                                                CustCode = BillResult.Key.FCode,
                                                CustName = BillResult.Key.Name,
                                                Amount = BillResult.Sum(m => m.Amount).ToString(),
                                                NetAmount = BillResult.Sum(m => m.NetAmt).ToString(),
                                                CGSTAmount = BillResult.Sum(m => m.CGSTAmt).ToString(),
                                                SGSTAmount = BillResult.Sum(m => m.SGSTAmt).ToString(),
                                                IGSTAmount = BillResult.Sum(m => m.TaxAmount).ToString(),
                                                //TaxAmount = BillResult.Sum(m => m.TaxAmount).ToString()
                                                FsessId = BillResult.Key.FSessId,
                                                OfferUID = BillResult.Key.OfferUID,
                                                IsDelete = false,
                                                Reason =BillResult.Key.Dreason,
                                                UserId = 0,
                                                UID=BillResult.Key.UID,
                                                Username=BillResult.Key.Username,
                                                DltDate=BillResult.Key.DltDate,
                                                DRecTimeStamp=BillResult.Key.DRecTimeStamp,
                                                InvoiceType = BillResult.Key.BillType,
                                                FType = BillResult.Key.FType
                                            }).ToList();

                                if (FType != null && FType != "" && FType != "A")
                                {
                                    if (FType != "M" && FType != "W")
                                    {
                                        objSales = objSales.Where(m => m.FType != "M").ToList();
                                        objSales = objSales.Where(m => m.FType != "W").ToList();
                                    }
                                    else
                                        objSales = objSales.Where(m => m.FType == FType).ToList();
                                }

                                if (OfferWise == 0)
                                    objSales = objSales.Where(m => m.OfferUID == 0).ToList();
                                if (OfferWise == 1)
                                    objSales = objSales.Where(m => m.OfferUID > 0).ToList();

                                if (CustomerId != "All")
                                {
                                    objSales = objSales.Where(m => m.CustCode == CustomerId).ToList();
                                }
                                if (PartyCode != "All" && PartyCode != "0")
                                {
                                    objSales = objSales.Where(m => m.PartyCode == PartyCode).ToList();
                                }
                                if (InvoiceType != "")
                                {
                                    if (InvoiceType == "RI")
                                    {
                                        objSales = objSales.Where(m => m.InvoiceType != "B").ToList();
                                    }
                                    else if (InvoiceType == "JI")
                                    {
                                        objSales = objSales.Where(m => m.InvoiceType == "B").ToList();
                                    }
                                }

                        if (DltDateWise == 1)
                        {
                            //**
                            if (FromDate != "All" && ToDate != "All")
                            {
                                foreach (var obj in objSales)
                                {
                                    if (obj.DRecTimeStamp.Date >= StartDate.Date && obj.DRecTimeStamp.Date <= EndDate.Date)
                                     objListSales.Add(obj);                                   
                                }
                            }
                            else if (FromDate == "All" && ToDate != "All")
                            {
                                foreach (var obj in objSales)
                                {
                                    if (obj.DRecTimeStamp.Date <= EndDate.Date)
                                     objListSales.Add(obj);
                                     }

                            }
                            else if (FromDate != "All" && ToDate == "All")
                            {
                                foreach (var obj in objSales)
                                {
                                    if (obj.DRecTimeStamp.Date >= StartDate.Date)
                                     objListSales.Add(obj);                                   
                                }

                            }
                            else if (!string.IsNullOrEmpty(BillNo) && BillNo != "0" && BillNo != "All")
                            {
                                objListSales = objSales.Where(m => m.BillNo == BillNo).ToList();
                            }
                            else
                            {
                                foreach (var obj in objSales)
                                      objListSales.Add(obj);
                               
                            }
                            //**
                        }
                        else
                        {
                            if (FromDate != "All" && ToDate != "All")
                            {
                                foreach (var obj in objSales)
                                {
                                    if (obj.BillDate >= StartDate.Date && obj.BillDate <= EndDate.Date)
                                    {
                                        objListSales.Add(obj);
                                    }
                                }
                            }
                            else if (FromDate == "All" && ToDate != "All")
                            {
                                foreach (var obj in objSales)
                                {
                                    if (obj.BillDate <= EndDate.Date)
                                    {
                                        objListSales.Add(obj);
                                    }
                                }

                            }
                            else if (FromDate != "All" && ToDate == "All")
                            {
                                foreach (var obj in objSales)
                                {
                                    if (obj.BillDate >= StartDate.Date)
                                    {
                                        objListSales.Add(obj);
                                    }
                                }

                            }
                            else if (!string.IsNullOrEmpty(BillNo) && BillNo != "0" && BillNo != "All")
                            {
                                objListSales = objSales.Where(m => m.BillNo == BillNo).ToList();
                            }
                            else
                            {
                                foreach (var obj in objSales)
                                {
                                    objListSales.Add(obj);
                                }
                            }
                        }                   

                    }

                }
            }
            catch (Exception ex)
            {
                //objListSales = new List<SalesReport>();
                //objListSales[0].ErrorMsg = ex.Message;
            }
            return objListSales;
        }

        public List<ProductModel> GetDltBillProduct(string UID)
        {
            List<ProductModel> objProdList = new List<ProductModel>();
            try
            {
                using (InventoryEntities db = new InventoryEntities())
                {
                    objProdList = (from s in db.DeletedBillDetails where s.UID == UID
                                   select  new ProductModel {
                                     ProductCodeStr=s.ProductId,
                                     ProductName=s.ProductName,
                                     DispQty=s.Qty,
                                     DP=s.DP,
                                     Amount =s.NetAmount,
                                     CGSTAmount=s.CGSTAmt,
                                     SGSTAmount=s.SGSTAmt,
                                     TaxAmt=s.TaxAmount
                                   }
                                   ).ToList();
                }
            }
            catch (Exception)
            {

            }
            return objProdList;
        }

        public List<StockJv> GetStockJvReport(string FromDate,string ToDate,string PartyCode,string ViewType)
        {
            List<StockJv> objStockJv = new List<StockJv>();
            try
            {
                DateTime StartDate = DateTime.Now;
                DateTime EndDate = DateTime.Now;
                if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                {
                    var SplitDate = FromDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    StartDate = StartDate.Date;
                }
                if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                {
                    var SplitDate = ToDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    EndDate = EndDate.Date;
                }
                using (var entity = new InventoryEntities())
                {
                    if (ViewType == "Both")
                    {
                        if (PartyCode != "0")
                        {
                            if (FromDate != "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  JvDate = r.StockDate.ToString(),
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate) && EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else if (FromDate != "All" && ToDate == "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate)).ToList();
                            }
                            else if (FromDate == "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                           ).ToList();
                            }
                        }
                        else
                        {
                            if (FromDate != "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate) && EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else if (FromDate != "All" && ToDate == "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate)).ToList();
                            }
                            else if (FromDate == "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                           ).ToList();
                            }
                        }
                    }
                    else if (ViewType == "Add")
                    {
                        if (PartyCode != "0")
                        {
                            if (FromDate != "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode && r.JType=="A"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate) && EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else if (FromDate != "All" && ToDate == "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode && r.JType == "A"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate)).ToList();
                            }
                            else if (FromDate == "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode && r.JType == "A"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode && r.JType == "A"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                           ).ToList();
                            }
                        }
                        else
                        {
                            if (FromDate != "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.JType == "A"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate) && EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else if (FromDate != "All" && ToDate == "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.JType == "A"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate)).ToList();
                            }
                            else if (FromDate == "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.JType == "A"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.JType == "A"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                           ).ToList();
                            }
                        }
                    }
                    else
                    {
                        //ViewType==Less
                        if (PartyCode != "0")
                        {
                            if (FromDate != "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode && r.JType == "L"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate) && EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else if (FromDate != "All" && ToDate == "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode && r.JType == "L"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate)).ToList();
                            }
                            else if (FromDate == "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode && r.JType == "L"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.FCode == PartyCode && r.JType == "L"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                           ).ToList();
                            }
                        }
                        else
                        {
                            if (FromDate != "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.JType == "L"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate) && EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else if (FromDate != "All" && ToDate == "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.JType == "L"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) >= EntityFunctions.TruncateTime(StartDate)).ToList();
                            }
                            else if (FromDate == "All" && ToDate != "All")
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.JType == "L"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                            ).Where(m => EntityFunctions.TruncateTime(m.StockDate) <= EntityFunctions.TruncateTime(EndDate)).ToList();
                            }
                            else
                            {
                                objStockJv = (from r in entity.TrnStockJvs
                                              where r.ActiveStatus == "Y" && r.JType == "L"
                                              select new StockJv
                                              {
                                                  StockDate = r.StockDate,
                                                  //JvDate=r.StockDate.Date.ToString(),
                                                  RefNo = r.RefNo,
                                                  JvNo = r.JvNo,
                                                  FCode = r.FCode,
                                                  PartyName = r.PartyName,
                                                  objProduct = new ProductModel()
                                                  {
                                                      ProductCodeStr = r.ProdId,
                                                      ProductName = r.ProductName,
                                                      BatchNo = r.BatchNo,
                                                      Quantity = r.Qty,

                                                  },
                                                  Remarks = r.Remarks,
                                                  SoldBy = r.SoldBy
                                              }

                                           ).ToList();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return objStockJv;


        }      

        public List<PurchaseReport> GetPurchaseSummary(string FromDate,string ToDate,string PartyCode,string SupplierCode,string ReportType,string InvoiceNo)
        {
            List<PurchaseReport> objListPurchaseSummary = new List<PurchaseReport>();
            DateTime StartDate = DateTime.Now;
            DateTime EndDate = DateTime.Now;
            try
            {
                using(var entity=new InventoryEntities())
                {
                    if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                    {
                        var SplitDate = FromDate.Split('-');
                        string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                        StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                        StartDate = StartDate.Date;
                    }
                    if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                    {
                        var SplitDate = ToDate.Split('-');
                        string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                        EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                        EndDate = EndDate.Date;
                    }
                    switch (ReportType)
                    {
                        case "Supplier":
                            if (PartyCode == "0" && SupplierCode=="0")
                            {
                                objListPurchaseSummary = (from r in entity.M_InwardMain
                                                          where r.ActiveStatus == "Y"
                                                          group r by new {r.SupplierName,r.SupplierCode }
                                                          into g
                                                          select new PurchaseReport
                                                          {
                                                            SupplierCode=g.Key.SupplierCode,
                                                            SupplierName=g.Key.SupplierName,
                                                            TotalQty=g.Sum(m=>m.TotalQty).ToString(),
                                                            Amount= g.Sum(m => m.TotalAmt).ToString(),
                                                            NetAmount= g.Sum(m => m.NetPayable).ToString(),
                                                            TaxAmount= g.Sum(m => m.TotalTaxAmt).ToString(),
                                                            TradeDisc= g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                            CashDisc= g.Sum(m => m.TotalCashDiscount).ToString()
                                                          }).ToList();
                            }
                            else if (PartyCode == "0" && SupplierCode != "0")
                            {
                                objListPurchaseSummary = (from r in entity.M_InwardMain
                                                          where r.ActiveStatus == "Y" && r.SupplierCode==SupplierCode
                                                          group r by new { r.SupplierName, r.SupplierCode }
                                                          into g
                                                          select new PurchaseReport
                                                          {
                                                              SupplierCode = g.Key.SupplierCode,
                                                              SupplierName = g.Key.SupplierName,
                                                              TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                              Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                              NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                              TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                              TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                              CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                          }).ToList();
                            }
                            else if (PartyCode != "0" && SupplierCode == "0")
                            {
                                objListPurchaseSummary = (from r in entity.M_InwardMain
                                                          where r.ActiveStatus == "Y" && r.InwardBy==PartyCode
                                                          group r by new { r.SupplierName, r.SupplierCode }
                                                          into g
                                                          select new PurchaseReport
                                                          {
                                                              SupplierCode = g.Key.SupplierCode,
                                                              SupplierName = g.Key.SupplierName,
                                                              TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                              Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                              NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                              TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                              TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                              CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                          }).ToList();
                            }
                            else
                            {
                                objListPurchaseSummary = (from r in entity.M_InwardMain
                                                          where r.ActiveStatus == "Y" && r.InwardBy==PartyCode && r.SupplierCode==SupplierCode
                                                          group r by new { r.SupplierName, r.SupplierCode }
                                                          into g
                                                          select new PurchaseReport
                                                          {
                                                              SupplierCode = g.Key.SupplierCode,
                                                              SupplierName = g.Key.SupplierName,
                                                              TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                              Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                              NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                              TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                              TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                              CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                          }).ToList();
                            }
                                break;
                        case "Invoice":
                            if (PartyCode == "0" && SupplierCode == "0")
                            {
                                objListPurchaseSummary = (from r in entity.M_InwardMain
                                                          where r.ActiveStatus == "Y"
                                                          
                                                          group r by new {r.InwardNo, r.InwardFor,r.RefNo,r.RecvDate,r.SupplierCode,r.SupplierName }
                                                             into g
                                                          orderby g.Key.RecvDate, g.Key.SupplierCode
                                                          select new PurchaseReport
                                                          {
                                                              InvoiceFor=g.Key.InwardFor,
                                                              InvoiceNo=g.Key.InwardNo,
                                                              RefNo=g.Key.RefNo,
                                                              InvoiceDateStr=g.Key.RecvDate,
                                                              SupplierCode=g.Key.SupplierCode,
                                                              SupplierName=g.Key.SupplierName,
                                                              TotalQty = g.Sum(m=>m.TotalQty).ToString(),
                                                              Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                              NetAmount = g.Sum(m=>m.NetPayable).ToString(),
                                                              TaxAmount =g.Sum(m=>m.TotalTaxAmt).ToString(),
                                                              TradeDisc = g.Sum(m=>m.TotalTradeDiscount).ToString(),
                                                              CashDisc = g.Sum(m=>m.TotalCashDiscount).ToString()
                                                          }).ToList();
                            }
                            else if (PartyCode == "0" && SupplierCode != "0")
                            {
                                objListPurchaseSummary = (from r in entity.M_InwardMain
                                                          where r.ActiveStatus == "Y" && r.SupplierCode == SupplierCode
                                                          group r by new { r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName }
                                                             into g
                                                          orderby g.Key.RecvDate, g.Key.SupplierCode
                                                          select new PurchaseReport
                                                          {
                                                              InvoiceFor = g.Key.InwardFor,
                                                              InvoiceNo = g.Key.InwardNo,
                                                              RefNo = g.Key.RefNo,
                                                              InvoiceDateStr = g.Key.RecvDate,
                                                              SupplierCode = g.Key.SupplierCode,
                                                              SupplierName = g.Key.SupplierName,
                                                              TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                              Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                              NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                              TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                              TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                              CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                          }).ToList();
                            }
                            else if (PartyCode != "0" && SupplierCode == "0")
                            {
                                objListPurchaseSummary = (from r in entity.M_InwardMain
                                                          where r.ActiveStatus == "Y" && r.InwardBy == PartyCode
                                                          group r by new { r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName,r.FSessId }
                                                             into g
                                                          orderby g.Key.RecvDate, g.Key.SupplierCode
                                                          select new PurchaseReport
                                                          {
                                                              InvoiceFor = g.Key.InwardFor,
                                                              FSessId = g.Key.FSessId,
                                                              InvoiceNo = g.Key.InwardNo,
                                                              RefNo = g.Key.RefNo,
                                                              InvoiceDateStr = g.Key.RecvDate,
                                                              SupplierCode = g.Key.SupplierCode,
                                                              SupplierName = g.Key.SupplierName,
                                                              TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                              Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                              NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                              TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                              TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                              CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                          }).ToList();
                            }
                            else
                            {
                                objListPurchaseSummary = (from r in entity.M_InwardMain
                                                          where r.ActiveStatus == "Y" && r.InwardBy == PartyCode && r.SupplierCode == SupplierCode
                                                          group r by new { r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName }
                                                             into g
                                                          orderby g.Key.RecvDate, g.Key.SupplierCode
                                                          select new PurchaseReport
                                                          {
                                                              InvoiceFor = g.Key.InwardFor,
                                                              InvoiceNo = g.Key.InwardNo,
                                                              RefNo = g.Key.RefNo,
                                                              InvoiceDateStr = g.Key.RecvDate,
                                                              SupplierCode = g.Key.SupplierCode,
                                                              SupplierName = g.Key.SupplierName,
                                                              TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                              Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                              NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                              TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                              TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                              CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                          }).ToList();
                            }
                            if (!string.IsNullOrEmpty(InvoiceNo) && InvoiceNo != "0" && InvoiceNo.ToLower()!="all")
                            {
                                objListPurchaseSummary = (from r in objListPurchaseSummary where r.InvoiceNo == InvoiceNo select r).ToList();
                            }
                            break;
                        case "":

                            break;
                    }
                    if (FromDate == "All" && ToDate != "All")
                    {
                        objListPurchaseSummary = objListPurchaseSummary.Where(m => m.InvoiceDateStr.Date <= EndDate.Date).ToList();
                    }
                    else if (FromDate != "All" && ToDate == "All")
                    {
                        objListPurchaseSummary = objListPurchaseSummary.Where(m => m.InvoiceDateStr.Date >= StartDate.Date).ToList();
                    }
                    else if (FromDate != "All" && ToDate != "All")
                    {
                        objListPurchaseSummary = objListPurchaseSummary.Where(m => m.InvoiceDateStr.Date >= StartDate.Date && m.InvoiceDateStr.Date <= EndDate.Date).ToList();
                    }
                    

                }
            }
            catch(Exception ex)
            {

            }

            return objListPurchaseSummary;
        }

        public List<PurchaseReport> GetPurchaseDetailSummary(string FromDate, string ToDate, string PartyCode, string SupplierCode, string ProductCode)
        {
            List<PurchaseReport> objListPurchaseSummary = new List<PurchaseReport>();
            DateTime StartDate = DateTime.Now;
            DateTime EndDate = DateTime.Now;
            try
            {
                using (var entity = new InventoryEntities())
                {
                    if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                    {
                        var SplitDate = FromDate.Split('-');
                        string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                        StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                        StartDate = StartDate.Date;
                    }
                    if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                    {
                        var SplitDate = ToDate.Split('-');
                        string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                        EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                        EndDate = EndDate.Date;
                    }


                    if (PartyCode == "0" && SupplierCode == "0")
                            {
                        if (ProductCode == "0")
                        {
                            objListPurchaseSummary = (from r in entity.M_InwardMain
                                                      where r.ActiveStatus == "Y"
                                                      join p in entity.M_InwardData on r.InwardNo equals p.InwardNo
                                                      group r by new {r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName, p.ProdCode, p.ProdName, p.BatchNo, p.Barcode, p.Qty, p.MRP, p.PRate, p.Amount, p.TradeDiscount, p.TradeAmount, p.Tax, p.TaxAmt, p.CGST, p.CGSTAmt, p.SGST, p.SGSTAmt, p.TotalAmount }
                                                         into g
                                                      orderby g.Key.RecvDate, g.Key.SupplierCode
                                                      select new PurchaseReport
                                                      {
                                                          InvoiceFor = g.Key.InwardFor,
                                                          InvoiceNo = g.Key.InwardNo,
                                                          RefNo = g.Key.RefNo,
                                                          InvoiceDateStr = g.Key.RecvDate,
                                                          SupplierCode = g.Key.SupplierCode,
                                                          SupplierName = g.Key.SupplierName,
                                                          objproduct = new ProductModel
                                                          {
                                                              ProductCodeStr = g.Key.ProdCode,
                                                              ProductName = g.Key.ProdName,
                                                              Quantity = g.Key.Qty,
                                                              Barcode = g.Key.Barcode,
                                                              BatchNo = g.Key.BatchNo,
                                                              DiscPer = g.Key.TradeDiscount,
                                                              DiscAmt = g.Key.TradeAmount,
                                                              TaxPer = g.Key.Tax,
                                                              TaxAmt = g.Key.TaxAmt,
                                                              CGST = g.Key.CGST,
                                                              CGSTAmount = g.Key.CGSTAmt,
                                                              SGST = g.Key.SGST,
                                                              SGSTAmount = g.Key.SGSTAmt,
                                                              MRP = g.Key.MRP,
                                                              Rate = g.Key.PRate,
                                                              Amount = g.Key.Amount,
                                                              TotalAmount = g.Key.TotalAmount
                                                          },
                                                          TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                          Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                          NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                          TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                          TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                          CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                      }).ToList();
                        }
                        else
                        {
                            objListPurchaseSummary = (from r in entity.M_InwardMain
                                                      where r.ActiveStatus == "Y"
                                                      join p in entity.M_InwardData on r.InwardNo equals p.InwardNo
                                                      where p.ProdCode==ProductCode
                                                      group r by new {r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName, p.ProdCode, p.ProdName, p.BatchNo, p.Barcode, p.Qty, p.MRP, p.PRate, p.Amount, p.TradeDiscount, p.TradeAmount, p.Tax, p.TaxAmt, p.CGST, p.CGSTAmt, p.SGST, p.SGSTAmt, p.TotalAmount }
                                                         into g
                                                      orderby g.Key.RecvDate, g.Key.SupplierCode
                                                      select new PurchaseReport
                                                      {
                                                          InvoiceFor = g.Key.InwardFor,
                                                          InvoiceNo = g.Key.InwardNo,
                                                          RefNo = g.Key.RefNo,
                                                          InvoiceDateStr = g.Key.RecvDate,
                                                          SupplierCode = g.Key.SupplierCode,
                                                          SupplierName = g.Key.SupplierName,
                                                          objproduct = new ProductModel
                                                          {
                                                              ProductCodeStr = g.Key.ProdCode,
                                                              ProductName = g.Key.ProdName,
                                                              Quantity = g.Key.Qty,
                                                              Barcode = g.Key.Barcode,
                                                              BatchNo = g.Key.BatchNo,
                                                              DiscPer = g.Key.TradeDiscount,
                                                              DiscAmt = g.Key.TradeAmount,
                                                              TaxPer = g.Key.Tax,
                                                              TaxAmt = g.Key.TaxAmt,
                                                              CGST = g.Key.CGST,
                                                              CGSTAmount = g.Key.CGSTAmt,
                                                              SGST = g.Key.SGST,
                                                              SGSTAmount = g.Key.SGSTAmt,
                                                              MRP = g.Key.MRP,
                                                              Rate = g.Key.PRate,
                                                              Amount = g.Key.Amount,
                                                              TotalAmount = g.Key.TotalAmount
                                                          },
                                                          TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                          Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                          NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                          TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                          TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                          CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                      }).ToList();
                        }
                            }
                            else if (PartyCode == "0" && SupplierCode != "0")
                            {
                        if (ProductCode == "0")
                        {
                            objListPurchaseSummary = (from r in entity.M_InwardMain
                                                      where r.ActiveStatus == "Y" && r.SupplierCode == SupplierCode
                                                      join p in entity.M_InwardData on r.InwardNo equals p.InwardNo
                                                      group r by new {r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName, p.ProdCode, p.ProdName, p.BatchNo, p.Barcode, p.Qty, p.MRP, p.PRate, p.Amount, p.TradeDiscount, p.TradeAmount, p.Tax, p.TaxAmt, p.CGST, p.CGSTAmt, p.SGST, p.SGSTAmt, p.TotalAmount }
                                                         into g
                                                      orderby g.Key.RecvDate, g.Key.SupplierCode
                                                      select new PurchaseReport
                                                      {
                                                          InvoiceFor = g.Key.InwardFor,
                                                          InvoiceNo = g.Key.InwardNo,
                                                          RefNo = g.Key.RefNo,
                                                          InvoiceDateStr = g.Key.RecvDate,
                                                          SupplierCode = g.Key.SupplierCode,
                                                          SupplierName = g.Key.SupplierName,
                                                          objproduct = new ProductModel
                                                          {
                                                              ProductCodeStr = g.Key.ProdCode,
                                                              ProductName = g.Key.ProdName,
                                                              Quantity = g.Key.Qty,
                                                              Barcode = g.Key.Barcode,
                                                              BatchNo = g.Key.BatchNo,
                                                              DiscPer = g.Key.TradeDiscount,
                                                              DiscAmt = g.Key.TradeAmount,
                                                              TaxPer = g.Key.Tax,
                                                              TaxAmt = g.Key.TaxAmt,
                                                              CGST = g.Key.CGST,
                                                              CGSTAmount = g.Key.CGSTAmt,
                                                              SGST = g.Key.SGST,
                                                              SGSTAmount = g.Key.SGSTAmt,
                                                              MRP = g.Key.MRP,
                                                              Rate = g.Key.PRate,
                                                              Amount = g.Key.Amount,
                                                              TotalAmount = g.Key.TotalAmount
                                                          },
                                                          TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                          Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                          NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                          TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                          TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                          CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                      }).ToList();
                        }
                        else
                        {
                            objListPurchaseSummary = (from r in entity.M_InwardMain
                                                      where r.ActiveStatus == "Y" && r.SupplierCode == SupplierCode
                                                      join p in entity.M_InwardData on r.InwardNo equals p.InwardNo
                                                      where p.ProdCode==ProductCode
                                                      group r by new { r.InwardNo,r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName, p.ProdCode, p.ProdName, p.BatchNo, p.Barcode, p.Qty, p.MRP, p.PRate, p.Amount, p.TradeDiscount, p.TradeAmount, p.Tax, p.TaxAmt, p.CGST, p.CGSTAmt, p.SGST, p.SGSTAmt, p.TotalAmount }
                                                             into g
                                                      orderby g.Key.RecvDate, g.Key.SupplierCode
                                                      select new PurchaseReport
                                                      {
                                                          InvoiceFor = g.Key.InwardFor,
                                                          InvoiceNo = g.Key.InwardNo,
                                                          RefNo = g.Key.RefNo,
                                                          InvoiceDateStr = g.Key.RecvDate,
                                                          SupplierCode = g.Key.SupplierCode,
                                                          SupplierName = g.Key.SupplierName,
                                                          objproduct = new ProductModel
                                                          {
                                                              ProductCodeStr = g.Key.ProdCode,
                                                              ProductName = g.Key.ProdName,
                                                              Quantity = g.Key.Qty,
                                                              Barcode = g.Key.Barcode,
                                                              BatchNo = g.Key.BatchNo,
                                                              DiscPer = g.Key.TradeDiscount,
                                                              DiscAmt = g.Key.TradeAmount,
                                                              TaxPer = g.Key.Tax,
                                                              TaxAmt = g.Key.TaxAmt,
                                                              CGST = g.Key.CGST,
                                                              CGSTAmount = g.Key.CGSTAmt,
                                                              SGST = g.Key.SGST,
                                                              SGSTAmount = g.Key.SGSTAmt,
                                                              MRP = g.Key.MRP,
                                                              Rate = g.Key.PRate,
                                                              Amount = g.Key.Amount,
                                                              TotalAmount = g.Key.TotalAmount
                                                          },
                                                          TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                          Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                          NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                          TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                          TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                          CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                      }).ToList();
                        }
                    }
                            else if (PartyCode != "0" && SupplierCode == "0")
                            {
                        if (ProductCode == "0")
                        {
                            objListPurchaseSummary = (from r in entity.M_InwardMain
                                                      where r.ActiveStatus == "Y" && r.InwardBy == PartyCode
                                                      join p in entity.M_InwardData on r.InwardNo equals p.InwardNo
                                                      group r by new {r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName, p.ProdCode, p.ProdName, p.BatchNo, p.Barcode, p.Qty, p.MRP, p.PRate, p.Amount, p.TradeDiscount, p.TradeAmount, p.Tax, p.TaxAmt, p.CGST, p.CGSTAmt, p.SGST, p.SGSTAmt, p.TotalAmount }
                                                         into g
                                                      orderby g.Key.RecvDate, g.Key.SupplierCode
                                                      select new PurchaseReport
                                                      {
                                                          InvoiceFor = g.Key.InwardFor,
                                                          InvoiceNo = g.Key.InwardNo,
                                                          RefNo = g.Key.RefNo,
                                                          InvoiceDateStr = g.Key.RecvDate,
                                                          SupplierCode = g.Key.SupplierCode,
                                                          SupplierName = g.Key.SupplierName,
                                                          objproduct = new ProductModel
                                                          {
                                                              ProductCodeStr = g.Key.ProdCode,
                                                              ProductName = g.Key.ProdName,
                                                              Quantity = g.Key.Qty,
                                                              Barcode = g.Key.Barcode,
                                                              BatchNo = g.Key.BatchNo,
                                                              DiscPer = g.Key.TradeDiscount,
                                                              DiscAmt = g.Key.TradeAmount,
                                                              TaxPer = g.Key.Tax,
                                                              TaxAmt = g.Key.TaxAmt,
                                                              CGST = g.Key.CGST,
                                                              CGSTAmount = g.Key.CGSTAmt,
                                                              SGST = g.Key.SGST,
                                                              SGSTAmount = g.Key.SGSTAmt,
                                                              MRP = g.Key.MRP,
                                                              Rate = g.Key.PRate,
                                                              Amount = g.Key.Amount,
                                                              TotalAmount = g.Key.TotalAmount
                                                          },
                                                          TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                          Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                          NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                          TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                          TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                          CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                      }).ToList();
                        }
                        else
                        {
                            objListPurchaseSummary = (from r in entity.M_InwardMain
                                                      where r.ActiveStatus == "Y" && r.InwardBy == PartyCode
                                                      join p in entity.M_InwardData on r.InwardNo equals p.InwardNo
                                                      where p.ProdCode==ProductCode
                                                      group r by new {r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName, p.ProdCode, p.ProdName, p.BatchNo, p.Barcode, p.Qty, p.MRP, p.PRate, p.Amount, p.TradeDiscount, p.TradeAmount, p.Tax, p.TaxAmt, p.CGST, p.CGSTAmt, p.SGST, p.SGSTAmt, p.TotalAmount }
                                                             into g
                                                      orderby g.Key.RecvDate, g.Key.SupplierCode
                                                      select new PurchaseReport
                                                      {
                                                          InvoiceFor = g.Key.InwardFor,
                                                          InvoiceNo = g.Key.InwardNo,
                                                          RefNo = g.Key.RefNo,
                                                          InvoiceDateStr = g.Key.RecvDate,
                                                          SupplierCode = g.Key.SupplierCode,
                                                          SupplierName = g.Key.SupplierName,
                                                          objproduct = new ProductModel
                                                          {
                                                              ProductCodeStr = g.Key.ProdCode,
                                                              ProductName = g.Key.ProdName,
                                                              Quantity = g.Key.Qty,
                                                              Barcode = g.Key.Barcode,
                                                              BatchNo = g.Key.BatchNo,
                                                              DiscPer = g.Key.TradeDiscount,
                                                              DiscAmt = g.Key.TradeAmount,
                                                              TaxPer = g.Key.Tax,
                                                              TaxAmt = g.Key.TaxAmt,
                                                              CGST = g.Key.CGST,
                                                              CGSTAmount = g.Key.CGSTAmt,
                                                              SGST = g.Key.SGST,
                                                              SGSTAmount = g.Key.SGSTAmt,
                                                              MRP = g.Key.MRP,
                                                              Rate = g.Key.PRate,
                                                              Amount = g.Key.Amount,
                                                              TotalAmount = g.Key.TotalAmount
                                                          },
                                                          TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                          Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                          NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                          TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                          TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                          CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                      }).ToList();
                        }
                    }
                            else
                            {
                        if (ProductCode == "0")
                        {
                            objListPurchaseSummary = (from r in entity.M_InwardMain
                                                      where r.ActiveStatus == "Y" && r.InwardBy == PartyCode && r.SupplierCode == SupplierCode
                                                      join p in entity.M_InwardData on r.InwardNo equals p.InwardNo
                                                      group r by new {r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName, p.ProdCode, p.ProdName, p.BatchNo, p.Barcode, p.Qty, p.MRP, p.PRate, p.Amount, p.TradeDiscount, p.TradeAmount, p.Tax, p.TaxAmt, p.CGST, p.CGSTAmt, p.SGST, p.SGSTAmt, p.TotalAmount }
                                                         into g
                                                      orderby g.Key.RecvDate, g.Key.SupplierCode
                                                      select new PurchaseReport
                                                      {
                                                          InvoiceFor = g.Key.InwardFor,
                                                          InvoiceNo = g.Key.InwardNo,
                                                          RefNo = g.Key.RefNo,
                                                          InvoiceDateStr = g.Key.RecvDate,
                                                          SupplierCode = g.Key.SupplierCode,
                                                          SupplierName = g.Key.SupplierName,
                                                          objproduct = new ProductModel
                                                          {
                                                              ProductCodeStr = g.Key.ProdCode,
                                                              ProductName = g.Key.ProdName,
                                                              Quantity = g.Key.Qty,
                                                              Barcode = g.Key.Barcode,
                                                              BatchNo = g.Key.BatchNo,
                                                              DiscPer = g.Key.TradeDiscount,
                                                              DiscAmt = g.Key.TradeAmount,
                                                              TaxPer = g.Key.Tax,
                                                              TaxAmt = g.Key.TaxAmt,
                                                              CGST = g.Key.CGST,
                                                              CGSTAmount = g.Key.CGSTAmt,
                                                              SGST = g.Key.SGST,
                                                              SGSTAmount = g.Key.SGSTAmt,
                                                              MRP = g.Key.MRP,
                                                              Rate = g.Key.PRate,
                                                              Amount = g.Key.Amount,
                                                              TotalAmount = g.Key.TotalAmount
                                                          },
                                                          TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                          Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                          NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                          TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                          TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                          CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                      }).ToList();
                        }
                        else
                        {
                            objListPurchaseSummary = (from r in entity.M_InwardMain
                                                      where r.ActiveStatus == "Y" && r.InwardBy == PartyCode && r.SupplierCode == SupplierCode
                                                      join p in entity.M_InwardData on r.InwardNo equals p.InwardNo
                                                      where p.ProdCode==ProductCode
                                                      group r by new {r.InwardNo, r.InwardFor, r.RefNo, r.RecvDate, r.SupplierCode, r.SupplierName, p.ProdCode, p.ProdName, p.BatchNo, p.Barcode, p.Qty, p.MRP, p.PRate, p.Amount, p.TradeDiscount, p.TradeAmount, p.Tax, p.TaxAmt, p.CGST, p.CGSTAmt, p.SGST, p.SGSTAmt, p.TotalAmount }
                                                             into g
                                                      orderby g.Key.RecvDate, g.Key.SupplierCode
                                                      select new PurchaseReport
                                                      {
                                                          InvoiceFor = g.Key.InwardFor,
                                                          InvoiceNo=g.Key.InwardNo,
                                                          RefNo = g.Key.RefNo,
                                                          InvoiceDateStr = g.Key.RecvDate,
                                                          SupplierCode = g.Key.SupplierCode,
                                                          SupplierName = g.Key.SupplierName,
                                                          objproduct = new ProductModel
                                                          {
                                                              ProductCodeStr = g.Key.ProdCode,
                                                              ProductName = g.Key.ProdName,
                                                              Quantity = g.Key.Qty,
                                                              Barcode = g.Key.Barcode,
                                                              BatchNo = g.Key.BatchNo,
                                                              DiscPer = g.Key.TradeDiscount,
                                                              DiscAmt = g.Key.TradeAmount,
                                                              TaxPer = g.Key.Tax,
                                                              TaxAmt = g.Key.TaxAmt,
                                                              CGST = g.Key.CGST,
                                                              CGSTAmount = g.Key.CGSTAmt,
                                                              SGST = g.Key.SGST,
                                                              SGSTAmount = g.Key.SGSTAmt,
                                                              MRP = g.Key.MRP,
                                                              Rate = g.Key.PRate,
                                                              Amount = g.Key.Amount,
                                                              TotalAmount = g.Key.TotalAmount
                                                          },
                                                          TotalQty = g.Sum(m => m.TotalQty).ToString(),
                                                          Amount = g.Sum(m => m.TotalAmt).ToString(),
                                                          NetAmount = g.Sum(m => m.NetPayable).ToString(),
                                                          TaxAmount = g.Sum(m => m.TotalTaxAmt).ToString(),
                                                          TradeDisc = g.Sum(m => m.TotalTradeDiscount).ToString(),
                                                          CashDisc = g.Sum(m => m.TotalCashDiscount).ToString()
                                                      }).ToList();
                        }
                            }
                            
                        
                    if (FromDate == "All" && ToDate != "All")
                    {
                        objListPurchaseSummary = objListPurchaseSummary.Where(m => m.InvoiceDateStr.Date <= EndDate.Date).ToList();
                    }
                    else if (FromDate != "All" && ToDate == "All")
                    {
                        objListPurchaseSummary = objListPurchaseSummary.Where(m => m.InvoiceDateStr.Date >= StartDate.Date).ToList();
                    }
                    else if (FromDate != "All" && ToDate != "All")
                    {
                        objListPurchaseSummary = objListPurchaseSummary.Where(m => m.InvoiceDateStr.Date >= StartDate.Date && m.InvoiceDateStr.Date <= EndDate.Date).ToList();
                    }


                }
            }
            catch (Exception ex)
            {

            }

            return objListPurchaseSummary;
        }

        public List<PurchaseReport> GetMonthWisePurchaseSummary(string Year, bool IsQuantity, bool IsAmount, string PartyCode, string SupplierCode)
        {
            List<PurchaseReport> objListPurchaseReport = new List<PurchaseReport>();
            try
            {
                using(var entity=new InventoryEntities())
                {
                    int YearInt = 0;
                    if (!string.IsNullOrEmpty(Year))
                    {
                        YearInt = int.Parse(Year);
                    }
                    if (Year == "0")
                    {
                            if(PartyCode=="0" && SupplierCode == "0")
                            {
                                objListPurchaseReport = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                                         select new PurchaseReport
                                                         {
                                                             SupplierCode = r.SupplierCode,
                                                             SupplierName = r.SupplierName,
                                                             PartyCode = r.PartyCode,
                                                             PartyName = r.PartyName,
                                                             Jan_Qty=r.JanQty.ToString(),
                                                             Jan_Pur=r.JanSale.ToString(),
                                                             Feb_Qty = r.FebQty.ToString(),
                                                             Feb_Pur = r.FebSale.ToString(),
                                                             March_Qty = r.MarQty.ToString(),
                                                             March_Pur = r.MarSale.ToString(),
                                                             April_Qty = r.AprQty.ToString(),
                                                             April_Pur = r.AprSale.ToString(),
                                                             May_Qty = r.MayQty.ToString(),
                                                             May_Pur = r.MaySale.ToString(),
                                                             June_Qty = r.JunQty.ToString(),
                                                             June_Pur = r.JunSale.ToString(),
                                                             July_Qty = r.JulQty.ToString(),
                                                             July_Pur = r.JulSale.ToString(),
                                                             August_Qty = r.AugQty.ToString(),
                                                             August_Pur = r.AugSale.ToString(),
                                                             Sep_Qty = r.SepQty.ToString(),
                                                             Sep_Pur = r.SepSale.ToString(),
                                                             Oct_Qty = r.OctQty.ToString(),
                                                             Oct_Pur = r.OctSale.ToString(),
                                                             Nov_Qty = r.NovQty.ToString(),
                                                             Nov_Pur = r.NovSale.ToString(),
                                                             Dec_Qty = r.DecQty.ToString(),
                                                             Dec_Pur = r.DecSale.ToString(),
                                                             TotalQty = r.TotalQty.ToString(),
                                                             TotalPurchase = r.TotalSale.ToString()
                                                         }

                                              ).ToList();
                            }
                            else if (PartyCode == "0" && SupplierCode != "0")
                            {
                                objListPurchaseReport = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                                         where r.SupplierCode==SupplierCode
                                                         select new PurchaseReport
                                                         {
                                                             SupplierCode = r.SupplierCode,
                                                             SupplierName = r.SupplierName,
                                                             PartyCode = r.PartyCode,
                                                             PartyName = r.PartyName,
                                                             Jan_Qty = r.JanQty.ToString(),
                                                             Jan_Pur = r.JanSale.ToString(),
                                                             Feb_Qty = r.FebQty.ToString(),
                                                             Feb_Pur = r.FebSale.ToString(),
                                                             March_Qty = r.MarQty.ToString(),
                                                             March_Pur = r.MarSale.ToString(),
                                                             April_Qty = r.AprQty.ToString(),
                                                             April_Pur = r.AprSale.ToString(),
                                                             May_Qty = r.MayQty.ToString(),
                                                             May_Pur = r.MaySale.ToString(),
                                                             June_Qty = r.JunQty.ToString(),
                                                             June_Pur = r.JunSale.ToString(),
                                                             July_Qty = r.JulQty.ToString(),
                                                             July_Pur = r.JulSale.ToString(),
                                                             August_Qty = r.AugQty.ToString(),
                                                             August_Pur = r.AugSale.ToString(),
                                                             Sep_Qty = r.SepQty.ToString(),
                                                             Sep_Pur = r.SepSale.ToString(),
                                                             Oct_Qty = r.OctQty.ToString(),
                                                             Oct_Pur = r.OctSale.ToString(),
                                                             Nov_Qty = r.NovQty.ToString(),
                                                             Nov_Pur = r.NovSale.ToString(),
                                                             Dec_Qty = r.DecQty.ToString(),
                                                             Dec_Pur = r.DecSale.ToString(),
                                                             TotalQty = r.TotalQty.ToString(),
                                                             TotalPurchase = r.TotalSale.ToString()
                                                         }

                                             ).ToList();
                            }
                            else if(PartyCode!="0" && SupplierCode == "0")
                            {
                                objListPurchaseReport = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                                         where r.PartyCode==PartyCode
                                                         select new PurchaseReport
                                                         {
                                                             SupplierCode = r.SupplierCode,
                                                             SupplierName = r.SupplierName,
                                                             PartyCode = r.PartyCode,
                                                             PartyName = r.PartyName,
                                                             Jan_Qty = r.JanQty.ToString(),
                                                             Jan_Pur = r.JanSale.ToString(),
                                                             Feb_Qty = r.FebQty.ToString(),
                                                             Feb_Pur = r.FebSale.ToString(),
                                                             March_Qty = r.MarQty.ToString(),
                                                             March_Pur = r.MarSale.ToString(),
                                                             April_Qty = r.AprQty.ToString(),
                                                             April_Pur = r.AprSale.ToString(),
                                                             May_Qty = r.MayQty.ToString(),
                                                             May_Pur = r.MaySale.ToString(),
                                                             June_Qty = r.JunQty.ToString(),
                                                             June_Pur = r.JunSale.ToString(),
                                                             July_Qty = r.JulQty.ToString(),
                                                             July_Pur = r.JulSale.ToString(),
                                                             August_Qty = r.AugQty.ToString(),
                                                             August_Pur = r.AugSale.ToString(),
                                                             Sep_Qty = r.SepQty.ToString(),
                                                             Sep_Pur = r.SepSale.ToString(),
                                                             Oct_Qty = r.OctQty.ToString(),
                                                             Oct_Pur = r.OctSale.ToString(),
                                                             Nov_Qty = r.NovQty.ToString(),
                                                             Nov_Pur = r.NovSale.ToString(),
                                                             Dec_Qty = r.DecQty.ToString(),
                                                             Dec_Pur = r.DecSale.ToString(),
                                                             TotalQty = r.TotalQty.ToString(),
                                                             TotalPurchase = r.TotalSale.ToString()
                                                         }

                                             ).ToList();
                            }
                            else
                            {
                                objListPurchaseReport = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                                         where r.PartyCode==PartyCode && r.SupplierCode==SupplierCode
                                                         select new PurchaseReport
                                                         {
                                                             SupplierCode = r.SupplierCode,
                                                             SupplierName = r.SupplierName,
                                                             PartyCode = r.PartyCode,
                                                             PartyName = r.PartyName,
                                                             Jan_Qty = r.JanQty.ToString(),
                                                             Jan_Pur = r.JanSale.ToString(),
                                                             Feb_Qty = r.FebQty.ToString(),
                                                             Feb_Pur = r.FebSale.ToString(),
                                                             March_Qty = r.MarQty.ToString(),
                                                             March_Pur = r.MarSale.ToString(),
                                                             April_Qty = r.AprQty.ToString(),
                                                             April_Pur = r.AprSale.ToString(),
                                                             May_Qty = r.MayQty.ToString(),
                                                             May_Pur = r.MaySale.ToString(),
                                                             June_Qty = r.JunQty.ToString(),
                                                             June_Pur = r.JunSale.ToString(),
                                                             July_Qty = r.JulQty.ToString(),
                                                             July_Pur = r.JulSale.ToString(),
                                                             August_Qty = r.AugQty.ToString(),
                                                             August_Pur = r.AugSale.ToString(),
                                                             Sep_Qty = r.SepQty.ToString(),
                                                             Sep_Pur = r.SepSale.ToString(),
                                                             Oct_Qty = r.OctQty.ToString(),
                                                             Oct_Pur = r.OctSale.ToString(),
                                                             Nov_Qty = r.NovQty.ToString(),
                                                             Nov_Pur = r.NovSale.ToString(),
                                                             Dec_Qty = r.DecQty.ToString(),
                                                             Dec_Pur = r.DecSale.ToString(),
                                                             TotalQty = r.TotalQty.ToString(),
                                                             TotalPurchase = r.TotalSale.ToString()
                                                         }

                                             ).ToList();
                            }
                       
                        
                        
                    }
                    else
                    {
                        if (PartyCode == "0" && SupplierCode == "0")
                        {
                            objListPurchaseReport = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                                     where r.BillYear == YearInt
                                                     select new PurchaseReport
                                                     {
                                                         SupplierCode = r.SupplierCode,
                                                         SupplierName = r.SupplierName,
                                                         PartyCode = r.PartyCode,
                                                         PartyName = r.PartyName,
                                                         Jan_Qty = r.JanQty.ToString(),
                                                         Jan_Pur = r.JanSale.ToString(),
                                                         Feb_Qty = r.FebQty.ToString(),
                                                         Feb_Pur = r.FebSale.ToString(),
                                                         March_Qty = r.MarQty.ToString(),
                                                         March_Pur = r.MarSale.ToString(),
                                                         April_Qty = r.AprQty.ToString(),
                                                         April_Pur = r.AprSale.ToString(),
                                                         May_Qty = r.MayQty.ToString(),
                                                         May_Pur = r.MaySale.ToString(),
                                                         June_Qty = r.JunQty.ToString(),
                                                         June_Pur = r.JunSale.ToString(),
                                                         July_Qty = r.JulQty.ToString(),
                                                         July_Pur = r.JulSale.ToString(),
                                                         August_Qty = r.AugQty.ToString(),
                                                         August_Pur = r.AugSale.ToString(),
                                                         Sep_Qty = r.SepQty.ToString(),
                                                         Sep_Pur = r.SepSale.ToString(),
                                                         Oct_Qty = r.OctQty.ToString(),
                                                         Oct_Pur = r.OctSale.ToString(),
                                                         Nov_Qty = r.NovQty.ToString(),
                                                         Nov_Pur = r.NovSale.ToString(),
                                                         Dec_Qty = r.DecQty.ToString(),
                                                         Dec_Pur = r.DecSale.ToString(),
                                                         TotalQty = r.TotalQty.ToString(),
                                                         TotalPurchase = r.TotalSale.ToString()
                                                     }

                                          ).ToList();
                        }
                        else if (PartyCode == "0" && SupplierCode != "0")
                        {
                            objListPurchaseReport = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                                     where r.BillYear == YearInt && r.SupplierCode == SupplierCode
                                                     select new PurchaseReport
                                                     {
                                                         SupplierCode = r.SupplierCode,
                                                         SupplierName = r.SupplierName,
                                                         PartyCode = r.PartyCode,
                                                         PartyName = r.PartyName,
                                                         Jan_Qty = r.JanQty.ToString(),
                                                         Jan_Pur = r.JanSale.ToString(),
                                                         Feb_Qty = r.FebQty.ToString(),
                                                         Feb_Pur = r.FebSale.ToString(),
                                                         March_Qty = r.MarQty.ToString(),
                                                         March_Pur = r.MarSale.ToString(),
                                                         April_Qty = r.AprQty.ToString(),
                                                         April_Pur = r.AprSale.ToString(),
                                                         May_Qty = r.MayQty.ToString(),
                                                         May_Pur = r.MaySale.ToString(),
                                                         June_Qty = r.JunQty.ToString(),
                                                         June_Pur = r.JunSale.ToString(),
                                                         July_Qty = r.JulQty.ToString(),
                                                         July_Pur = r.JulSale.ToString(),
                                                         August_Qty = r.AugQty.ToString(),
                                                         August_Pur = r.AugSale.ToString(),
                                                         Sep_Qty = r.SepQty.ToString(),
                                                         Sep_Pur = r.SepSale.ToString(),
                                                         Oct_Qty = r.OctQty.ToString(),
                                                         Oct_Pur = r.OctSale.ToString(),
                                                         Nov_Qty = r.NovQty.ToString(),
                                                         Nov_Pur = r.NovSale.ToString(),
                                                         Dec_Qty = r.DecQty.ToString(),
                                                         Dec_Pur = r.DecSale.ToString(),
                                                         TotalQty = r.TotalQty.ToString(),
                                                         TotalPurchase = r.TotalSale.ToString()
                                                     }

                                         ).ToList();
                        }
                        else if (PartyCode != "0" && SupplierCode == "0")
                        {
                            objListPurchaseReport = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                                     where r.BillYear == YearInt && r.PartyCode == PartyCode
                                                     select new PurchaseReport
                                                     {
                                                         SupplierCode = r.SupplierCode,
                                                         SupplierName = r.SupplierName,
                                                         PartyCode = r.PartyCode,
                                                         PartyName = r.PartyName,
                                                         Jan_Qty = r.JanQty.ToString(),
                                                         Jan_Pur = r.JanSale.ToString(),
                                                         Feb_Qty = r.FebQty.ToString(),
                                                         Feb_Pur = r.FebSale.ToString(),
                                                         March_Qty = r.MarQty.ToString(),
                                                         March_Pur = r.MarSale.ToString(),
                                                         April_Qty = r.AprQty.ToString(),
                                                         April_Pur = r.AprSale.ToString(),
                                                         May_Qty = r.MayQty.ToString(),
                                                         May_Pur = r.MaySale.ToString(),
                                                         June_Qty = r.JunQty.ToString(),
                                                         June_Pur = r.JunSale.ToString(),
                                                         July_Qty = r.JulQty.ToString(),
                                                         July_Pur = r.JulSale.ToString(),
                                                         August_Qty = r.AugQty.ToString(),
                                                         August_Pur = r.AugSale.ToString(),
                                                         Sep_Qty = r.SepQty.ToString(),
                                                         Sep_Pur = r.SepSale.ToString(),
                                                         Oct_Qty = r.OctQty.ToString(),
                                                         Oct_Pur = r.OctSale.ToString(),
                                                         Nov_Qty = r.NovQty.ToString(),
                                                         Nov_Pur = r.NovSale.ToString(),
                                                         Dec_Qty = r.DecQty.ToString(),
                                                         Dec_Pur = r.DecSale.ToString(),
                                                         TotalQty = r.TotalQty.ToString(),
                                                         TotalPurchase = r.TotalSale.ToString()
                                                     }

                                         ).ToList();
                        }
                        else
                        {
                            objListPurchaseReport = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                                     where r.BillYear==YearInt && r.PartyCode == PartyCode && r.SupplierCode == SupplierCode
                                                     select new PurchaseReport
                                                     {
                                                         SupplierCode = r.SupplierCode,
                                                         SupplierName = r.SupplierName,
                                                         PartyCode = r.PartyCode,
                                                         PartyName = r.PartyName,
                                                         Jan_Qty = r.JanQty.ToString(),
                                                         Jan_Pur = r.JanSale.ToString(),
                                                         Feb_Qty = r.FebQty.ToString(),
                                                         Feb_Pur = r.FebSale.ToString(),
                                                         March_Qty = r.MarQty.ToString(),
                                                         March_Pur = r.MarSale.ToString(),
                                                         April_Qty = r.AprQty.ToString(),
                                                         April_Pur = r.AprSale.ToString(),
                                                         May_Qty = r.MayQty.ToString(),
                                                         May_Pur = r.MaySale.ToString(),
                                                         June_Qty = r.JunQty.ToString(),
                                                         June_Pur = r.JunSale.ToString(),
                                                         July_Qty = r.JulQty.ToString(),
                                                         July_Pur = r.JulSale.ToString(),
                                                         August_Qty = r.AugQty.ToString(),
                                                         August_Pur = r.AugSale.ToString(),
                                                         Sep_Qty = r.SepQty.ToString(),
                                                         Sep_Pur = r.SepSale.ToString(),
                                                         Oct_Qty = r.OctQty.ToString(),
                                                         Oct_Pur = r.OctSale.ToString(),
                                                         Nov_Qty = r.NovQty.ToString(),
                                                         Nov_Pur = r.NovSale.ToString(),
                                                         Dec_Qty = r.DecQty.ToString(),
                                                         Dec_Pur = r.DecSale.ToString(),
                                                         TotalQty = r.TotalQty.ToString(),
                                                         TotalPurchase = r.TotalSale.ToString()
                                                     }

                                         ).ToList();
                        }
                    }
                    
                       
                    
                }
            }
            catch(Exception ex)
            {

            }
            return objListPurchaseReport;
        }

        public List<string> GetYearList()
        {
            List<string> objYearLists = new List<string>();
            try
            {
                using(var entity=new InventoryEntities())
                {
                    var Result = (from r in entity.V_MonthWiseSupplierPurchaseSummary
                                  select r.BillYear
                                  
                                ).OrderBy(m=>m.Value).ToList();
                    if (Result.Count() > 0)
                    {
                        var FirstYear = Result.First();
                        var LastYear = Result.Last();
                       
                        var RemainingYears = LastYear - FirstYear;
                        if (RemainingYears > 0)
                        {
                            objYearLists.Add(FirstYear.ToString());
                            for (int i = 1; i <= RemainingYears; i++)
                            {
                                objYearLists.Add((FirstYear+i).ToString());
                            }
                        }
                        else
                        {
                            objYearLists.Add(FirstYear.ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return objYearLists;
        }
        public List<string> GetSalesYearList()
        {
            List<string> objYearLists = new List<string>();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    var Result = (from r in entity.V_MonthWiseSaleSummary
                                  select r.BillYear

                                ).OrderBy(m => m.Value).ToList();
                    if (Result.Count() > 0)
                    {
                        var FirstYear = Result.First();
                        var LastYear = Result.Last();

                        var RemainingYears = LastYear - FirstYear;
                        if (RemainingYears > 0)
                        {
                            objYearLists.Add(FirstYear.ToString());
                            for (int i = 1; i <= RemainingYears; i++)
                            {
                                objYearLists.Add((FirstYear + i).ToString());
                            }
                        }
                        else
                        {
                            objYearLists.Add(FirstYear.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objYearLists;
        }

        public List<SalesReport> GetMonthWiseSalesSummary(string Year, bool IsQuantity, bool IsAmount, string PartyCode)
        {
            List<SalesReport> objSalesReport = new List<SalesReport>();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    int YearInt = 0;
                    if (!string.IsNullOrEmpty(Year))
                    {
                        YearInt = int.Parse(Year);
                    }
                    objSalesReport = (from r in entity.V_MonthWiseSaleSummary
                                      select new SalesReport
                                      {

                                          PartyCode = r.PartyCode,
                                          PartyName = r.PartyName,
                                          Jan_Qty = r.JanQty.ToString(),
                                          Jan_Sales = r.JanSale.ToString(),
                                          Feb_Qty = r.FebQty.ToString(),
                                          Feb_Sales = r.FebSale.ToString(),
                                          March_Qty = r.MarQty.ToString(),
                                          March_Sales = r.MarSale.ToString(),
                                          April_Qty = r.AprQty.ToString(),
                                          April_Sales = r.AprSale.ToString(),
                                          May_Qty = r.MayQty.ToString(),
                                          May_Sales = r.MaySale.ToString(),
                                          June_Qty = r.JunQty.ToString(),
                                          June_Sales = r.JunSale.ToString(),
                                          July_Qty = r.JulQty.ToString(),
                                          July_Sales = r.JulSale.ToString(),
                                          August_Qty = r.AugQty.ToString(),
                                          August_Sales = r.AugSale.ToString(),
                                          Sep_Qty = r.SepQty.ToString(),
                                          Sep_Sales = r.SepSale.ToString(),
                                          Oct_Qty = r.OctQty.ToString(),
                                          Oct_Sales = r.OctSale.ToString(),
                                          Nov_Qty = r.NovQty.ToString(),
                                          Nov_Sales = r.NovSale.ToString(),
                                          Dec_Qty = r.DecQty.ToString(),
                                          Dec_Sales = r.DecSale.ToString(),
                                          TotalQty = r.TotalQty.ToString(),
                                          TotalSales = r.TotalSale.ToString(),
                                          BillYear = r.BillYear ?? 0,

                                      }

                                          ).ToList();
                    if (Year != "0")
                    {
                        objSalesReport = objSalesReport.Where(m => m.BillYear == YearInt).ToList();
                    }
                    if (PartyCode != "0" && PartyCode != "All")
                    {

                        objSalesReport = objSalesReport.Where(m => m.PartyCode == PartyCode).ToList();
                    }

                }
                    
                
            }
            catch(Exception ex)
            {

            }
            return objSalesReport;
        }
        public List<SelectListItem> GetStateList()
        {
            List<SelectListItem> objStateList = new List<SelectListItem>();
            try
            {
                string AppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(AppConnectionString);
                
                string query = "Select * from M_StateDivMaster where RowStatus='Y' AND ActiveStatus='Y'";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = query;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();
                List<StateModel> M_StateDivMasterList = new List<StateModel>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        M_StateDivMasterList.Add(new StateModel
                        {
                            StateCode = decimal.Parse(reader["StateCode"].ToString()),
                            StateName = reader["StateName"].ToString()
                        });
                    }
                }
                using (var entity = new InventoryEntities())
                {
                    objStateList = (from r in M_StateDivMasterList
                                   
                                    select new SelectListItem
                                    {
                                        Value = r.StateCode.ToString(),
                                        Text = r.StateName
                                    }

                                  ).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return objStateList;
        }
        public List<StockReportModel> GetStockReceiptReport(string CategoryCode, string ProductCode, string PartyCode, string StateCode, string FromDate, string ToDate, string LoginPartyCode, bool isSummary)
        {
            List<StockReportModel> objStockModel = new List<StockReportModel>();
            decimal CatId = 0;
            string ProdCode = "0";
            decimal StCode = 0;
            string PCode = "All";
            DateTime StartDate = DateTime.Now;
            DateTime EndDate = DateTime.Now;
            try
            {

                string db = System.Configuration.ConfigurationManager.AppSettings["Database"];
                string dbInv = System.Configuration.ConfigurationManager.AppSettings["INVDatabase"];


                if (!string.IsNullOrEmpty(CategoryCode))
                {
                    CatId = decimal.Parse(CategoryCode);
                }
                if (!string.IsNullOrEmpty(ProductCode))
                {
                    ProdCode = ProductCode;
                    if (ProductCode == "0")
                    {
                        ProdCode = "All";
                    }
                }
                if (!string.IsNullOrEmpty(StateCode))
                {
                    StCode = decimal.Parse(StateCode);
                }
                if (!string.IsNullOrEmpty(PartyCode))
                {
                    PCode = PartyCode;
                    if (PartyCode == "0")
                    {
                        PCode = "All";
                    }

                }

                string Sql = "";
                string WhereCondition = "";
                string InvConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(InvConnectionString);
                SqlCommand cmd = new SqlCommand();

                using (var entity = new InventoryEntities())
                {
                    bool IsAdmin = false;
                    if (!string.IsNullOrEmpty(LoginPartyCode))
                    {
                        IsAdmin = (from r in entity.Inv_M_UserMaster where r.BranchCode == LoginPartyCode select r.IsAdmin).FirstOrDefault() == "Y" ? true : false;
                    }
                    if (!string.IsNullOrEmpty(FromDate) && (!string.IsNullOrEmpty(ToDate)))
                    {
                        if (FromDate != "All")
                        {
                            //var sqlFromdate = FromDate.Split('-');
                            //StartDate = new DateTime(Convert.ToInt16(sqlFromdate[2]), Convert.ToInt16(sqlFromdate[1]), Convert.ToInt16(sqlFromdate[0]));
                            var SplitFromDate = FromDate.Split('-');
                            FromDate = SplitFromDate[1] + "-" + SplitFromDate[0] + "-" + SplitFromDate[2];
                            string NewDate = SplitFromDate[1] + "/" + ("0" + SplitFromDate[0]).Substring(("0" + SplitFromDate[0]).Length - 2, 2) + "/" + SplitFromDate[2];
                            StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                            StartDate = StartDate.Date;
                        }
                        if (ToDate != "All")
                        {
                            //var sqlFromdate = ToDate.Split('-');
                            //EndDate = new DateTime(Convert.ToInt16(sqlFromdate[2]), Convert.ToInt16(sqlFromdate[1]), Convert.ToInt16(sqlFromdate[0]));
                            var SplitToDate = ToDate.Split('-');
                            string NewDate = SplitToDate[1] + "/" + ("0" + SplitToDate[0]).Substring(("0" + SplitToDate[0]).Length - 2, 2) + "/" + SplitToDate[2];
                            EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                            EndDate = EndDate.Date;
                        }

                        //if (PCode == "All")
                        //{
                        //    if (IsAdmin)
                        //    {

                        //    }
                        //    else
                        //    {
                        //        WhereCondition = WhereCondition + " AND c.FCode='" + LoginPartyCode + "'";
                        //    }
                        //}
                        //else
                        //{
                         //   WhereCondition = " AND a.FCode='" + PCode + "'";
                        //}
                        if(PCode!="0" && PCode != "All")
                        {
                            WhereCondition = " AND a.FCode='" + PCode + "'";
                        }

                        if (ProdCode == "All")
                        {
                            WhereCondition = WhereCondition + "";
                        }
                        else
                        {
                            WhereCondition = WhereCondition + " And b.ProdID='" + ProdCode + "'";
                        }

                        if (CatId == 0)
                        {
                            WhereCondition = WhereCondition + "";
                        }
                        else
                        {
                            WhereCondition = WhereCondition + " And b.CatID='" + CatId + "'";
                        }

                        //to add date range in last after result - pending;
                        string NewFromDate = StartDate.Date.ToString("dd-MMM-yyyy");
                        string NewToDate = EndDate.Date.ToString("dd-MMM-yyyy");
                        if (FromDate != "All" && ToDate != "All")
                        {
                            WhereCondition = WhereCondition + " and a.BillDate>='" + NewFromDate + "' and a.BillDate<='" + NewToDate + "'";
                        }
                        else if (FromDate != "All" && ToDate == "All")
                        {
                            WhereCondition = WhereCondition + " and a.BillDate>='" + NewFromDate + "'";
                        }
                        else if (FromDate == "All" && ToDate != "All")
                        {
                            WhereCondition = WhereCondition + "and a.BillDate<='" + NewToDate + "'";
                        }
                        else
                        {

                        }


                        if (StCode > 0)
                        {
                            WhereCondition = WhereCondition + " And d.StateCode=" + StCode;
                        }

                        if (isSummary)
                        {
                            Sql = "Select '' as BillNo,'' as BillDate, a.FCode, '' As PartyName, a.ProductId,";
                            Sql = Sql + " a.ProductName,Sum(a.Qty) as Qty,b.Dp as Rate,0 As TaxPer,Sum(a.TaxAmount+a.CGSTAmt+a.SGSTAmt) as TaxAmount, Sum(A.NetAmount) As Amount,";
                            Sql = Sql + " Sum(a.NetAmount+A.TaxAmount+a.CGSTAmt+a.SGSTAmt) as NetPayable,'' as STNNo,'' as BillNo, '' As PartyType,d.PartyCode + ' - ' + d.PartyName as SoldPartyName,St.StateName ";
                            Sql = Sql + " From TrnBillDetails as a,M_ProductMaster as b,TrnBillMain as c,M_LedgerMaster as d,"+ db + "..M_StateDivMaster as St ";
                            Sql = Sql + " Where st.RowStatus='Y' AND c.SoldBy=d.PartyCode And a.BillNo=c.BillNo And a.ProductId=b.ProdId AND d.StateCode=St.StateCode ";
                            Sql = Sql + WhereCondition;
                            Sql = Sql + " Group By a.FCode,a.ProductId,a.ProductName,b.Dp,d.PartyCode + ' - ' + d.PartyName,St.StateName Order By a.ProductId";
                        }
                        else
                        {
                            Sql = "Select c.GRNo,Replace(Convert(varchar,c.StkRecvDate,106),' ','-') as GRDate,c.FCode,c.PartyName,a.ProductId ,a.ProductName,a.Qty as Qty,a.Rate as Rate,a.Tax+a.CGST+a.SGST as TaxPer,(a.TaxAmount +a.CGSTAmt+a.SGSTAmt) as TaxAmount,(a.NetAmount) as Amount,c. NetPayable,c.UserBillNo as STNo,c.BillNo,Case When c.Ftype='M' then 'Distributor' else Case When c.FType in ('GC','W') then 'Customer' else Case When c.Ftype Not in('M','GC','W') then 'Franchise' end end end as PartyType,d.PartyCode + ' - ' + d.PartyName as SoldPartyName,St.StateName ";
                            Sql = Sql + " From TrnBillDetails as a,TrnBillMain as c,M_ProductMaster as b ,M_LedgerMaster as d,"+ db + "..M_StateDivMaster as St where st.RowStatus='Y' AND c.SoldBy=d.PartyCode And a.BillNo=c.BillNo AND d.StateCode=St.StateCode And a.ProductId=b.ProdId ";
                            Sql = Sql + WhereCondition;
                            //Sql = Sql + " Group By c.GRNo,c.StkRecvDate,a.ProductId,a.ProductName,a.Tax+a.CGST+a.SGST,a.Rate,c.FCode,c.PartyName,c.UserBillNo,c.FType,d.PartyCode + ' - ' + d.PartyName,c.BillNo,St.StateName ";
                            Sql = Sql + " Order By c.GRNo,c.FType,a.ProductId,c.Fcode";
                        }

                        cmd.CommandText = Sql;
                        cmd.Connection = SC;
                        SC.Close();
                        SC.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                StockReportModel tempobj = new StockReportModel();
                                tempobj.ProductCode = reader["ProductId"] != null ? reader["ProductId"].ToString() : "";
                                tempobj.ProductName = reader["ProductName"] != null ? reader["ProductName"].ToString() : "";
                                tempobj.PartyCode = reader["FCode"] != null ? reader["FCode"].ToString() : "";
                                tempobj.PartyName = reader["PartyName"] != null ? reader["PartyName"].ToString() : "";
                                tempobj.RateOrDP = reader["Rate"] != null ? reader["Rate"].ToString() : "";
                                tempobj.Qty = reader["Qty"] != null ? reader["Qty"].ToString() : "";
                                tempobj.TaxPer = reader["TaxPer"] != null ? reader["TaxPer"].ToString() : "";
                                tempobj.TaxAmt = reader["TaxAmount"] != null ? reader["TaxAmount"].ToString() : "";
                                tempobj.BasicAmt = reader["Amount"] != null ? reader["Amount"].ToString() : "";
                                tempobj.TotalAmt = reader["NetPayable"] != null ? reader["NetPayable"].ToString() : "";
                                tempobj.SoldBy = reader["SoldPartyName"] != null ? reader["SoldPartyName"].ToString() : "";
                                tempobj.StateName = reader["StateName"] != null ? reader["StateName"].ToString() : "";
                                if (isSummary == false)
                                {
                                    tempobj.StnNo = reader["STNo"] != null ? reader["STNo"].ToString() : "";
                                    tempobj.StrNo = reader["GRNo"] != null ? reader["GRNo"].ToString() : "";
                                    tempobj.StrDate = reader["GRDate"] != null ? reader["GRDate"].ToString() : "";
                                    if (tempobj.StrDate != "") {
                                        DateTime StrDate = Convert.ToDateTime(tempobj.StrDate);
                                        tempobj.StrDate = StrDate.ToShortDateString();
                                     }
                                }
                                else
                                {
                                    tempobj.StnNo = "";
                                    tempobj.StrNo = "";
                                    tempobj.StrDate = "";
                                }
                                objStockModel.Add(tempobj);


                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objStockModel;
        }

        public List<PartyWiseWalletDetails> GetPartyWiseWalletReport(string FromDate, string ToDate, string PartyCode, string ViewType)
        {
            List<PartyWiseWalletDetails> objPartyWalletDetails = new List<PartyWiseWalletDetails>();
            try
            {
                using(var entity=new InventoryEntities())
                {
                    var AllPartyDetails = new List<PartyWiseWalletDetails>();
                    
                    AllPartyDetails = (from r in entity.TrnVouchers
                                               where r.ActiveStatus == "Y" && r.VType =="R"
                                       from p in entity.M_LedgerMaster
                                               where p.PartyCode == r.CrTo || p.PartyCode == r.DrTo
                                               select new PartyWiseWalletDetails
                                               {
                                                   PartyCode = p.PartyCode,
                                                   PartyName = p.PartyName,
                                                   CrTo = r.CrTo,
                                                   DrTo = r.DrTo,
                                                   CrAmt = r.Amount.ToString(),
                                                   DrAmt = r.Amount.ToString(),
                                                   Narration = r.Narration,
                                                   Balance = "0",
                                                   TransDate = r.VoucherDate
                                               }
                                               ).Distinct().ToList();


                    if (PartyCode != "All" && PartyCode != "0")
                    {
                        AllPartyDetails = AllPartyDetails.Where(m => m.PartyCode == PartyCode).ToList();
                    }
                    DateTime StartDate = DateTime.Now.Date;
                    DateTime EndDate = DateTime.Now.Date;
                    if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                    {
                        var SplitDate = FromDate.Split('-');
                        string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                        StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                        StartDate = StartDate.Date;
                    }
                    if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                    {
                        var SplitDate = ToDate.Split('-');
                        string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                        EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                        EndDate = EndDate.Date;
                    }
                    if (FromDate != "All" && ToDate != "All")
                    {
                        AllPartyDetails = AllPartyDetails.Where(m => m.TransDate.Date >= StartDate.Date && m.TransDate.Date <= EndDate.Date).ToList();
                    }
                    else if (FromDate == "All" && ToDate != "All")
                    {
                        AllPartyDetails = AllPartyDetails.Where(m => m.TransDate.Date <= EndDate.Date).ToList();
                    }
                    else if (FromDate != "All" && ToDate == "All")
                    {
                        AllPartyDetails = AllPartyDetails.Where(m => m.TransDate.Date >= StartDate.Date).ToList();
                    }
                    decimal previousBalance = 0;
                    foreach (var obj in AllPartyDetails)
                    {
                        PartyWiseWalletDetails tempObj = new PartyWiseWalletDetails();
                        tempObj = obj;
                            if (obj.CrTo != "0")
                            {
                                tempObj.DrAmt = "0";
                                tempObj.DrAmtD = 0;
                                tempObj.CrAmtD = decimal.Parse(obj.CrAmt); 
                                tempObj.Balance = (previousBalance + decimal.Parse(obj.CrAmt)).ToString();
                            }
                            else
                            {
                                tempObj.CrAmt = "0";
                                tempObj.CrAmtD = 0;
                                tempObj.DrAmtD = decimal.Parse(obj.DrAmt);
                                tempObj.Balance = (previousBalance - decimal.Parse(obj.DrAmt)).ToString();
                            }

                            previousBalance = decimal.Parse(tempObj.Balance);
                       
                        
                        objPartyWalletDetails.Add(tempObj);
                    }
                    if (ViewType == "Balance")
                    {
                        objPartyWalletDetails = (from obj in objPartyWalletDetails
                                                 group obj by new { obj.PartyCode,obj.PartyName} into grouped
                                                 select new PartyWiseWalletDetails
                                                 {
                                                     PartyCode=grouped.Key.PartyCode,
                                                     PartyName=grouped.Key.PartyName,
                                                     CrAmt=grouped.Sum(m=>m.CrAmtD).ToString(),
                                                     DrAmt=grouped.Sum(m=>m.DrAmtD).ToString(),
                                                     Balance= (grouped.Sum(m => m.CrAmtD)- grouped.Sum(m => m.DrAmtD)).ToString()
                                                 }).ToList();
                    }
                    //DashboardAPIController objDashboardApi = new DashboardAPIController();

                   
                   

                }
            }
            catch(Exception ex)
            {

            }
            return objPartyWalletDetails;
        }


        public List<SaleRegister> GetSaleRegisterReport(string FromDate, string ToDate, string PartyCode)
        {
            List<SaleRegister> objReport = new List<SaleRegister>();
            string WhereCondition = string.Empty;
            string Fld = string.Empty;
            var dataTable = new DataTable();

            try
            {
                DateTime StartDate = new DateTime();
                DateTime EndDate = new DateTime();

                if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                {
                    var SplitDate = FromDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    StartDate = StartDate.Date;
                }
                if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                {
                    var SplitDate = ToDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    EndDate = EndDate.Date;
                }

                string NewFromDate = StartDate.Date.ToString("dd-MMM-yyyy");
                string NewToDate = EndDate.Date.ToString("dd-MMM-yyyy");

                string wherea = string.Empty;
                string whereb = string.Empty;
                string wheree = string.Empty;

                if (!string.IsNullOrEmpty(FromDate) && FromDate.ToUpper() != "ALL")
                {
                    wherea += " and a.BillDate>='" + NewFromDate + "'";
                    whereb += " and b.BillDate>='" + NewFromDate + "'";
                    wheree += " and e.BillDate>='" + NewFromDate + "'";

                }

                if (!string.IsNullOrEmpty(ToDate) && ToDate.ToUpper() != "ALL")
                {
                    wherea = wherea + " and a.BillDate <='" + NewToDate + "'";
                    whereb = whereb + " and b.BillDate <='" + NewToDate + "'";
                    wheree = wheree + " and e.BillDate <='" + NewToDate + "'";

                }

                if (!string.IsNullOrEmpty(PartyCode) && PartyCode.ToUpper() != "ALL" && PartyCode.ToUpper() != "0")
                {
                    wherea = wherea + " and a.SoldBy='" + PartyCode.Trim() + "' ";
                    whereb = whereb + " and b.SoldBy='" + PartyCode.Trim() + "' ";
                    wheree = wheree + " and e.SoldBy='" + PartyCode.Trim() + "' ";

                }

                

                string TaxPerCondi0_ = " and (a.Tax=0 AND a.CGST=0 AND a.SGST=0) ";
                string TaxPerCondi1_ = " and (a.Tax=5 OR a.CGST=2.5 OR a.SGST=2.5) ";
                string TaxPerCondi2_ = " and (b.Tax=12 OR b.CGST=6 OR b.SGST=6) ";
                string TaxPerCondi3_ = " and (b.Tax=18 OR b.CGST=9 OR b.SGST=9) ";
                string TaxPerCondi4_ = " and (e.Tax=28 OR e.CGST=14 OR e.SGST=14) ";


                string sql = " Select M.BillNo,M.Billdate,M.PartyCode as Code,M.PartyName,ISNULL(L.CSTNo,'') as GSTIN,M.[ExemptSale],M.Discount,NetAmount_5 as [Basic 5%] ,IGST_5 as [IGST@5%],CGST_5 as [CGST@2.5%],SGST_5 as [CGST @2.5%],NetAmount_12 as [Basic 12%],IGST_12 [IGST @12%],CGST_12 [CGST @6%],SGST_12 as [SGST @6%],NetAmount_18 as [Basic for 18%],IGST_18 [IGST @18%] ,CGST_18 [CGST @9%],SGST_18 [SGST @9%],NetAmount_28 [Basic 28%],IGST_28 [IGST @28%],CGST_28 [CGST @14%],SGST_28 [SGST @14%],TotalAmount as [Total Amt.],TotalIGSTAmt as [Total IGST],TotalCGSTAmt  [Total CGST],TotalSGSTAmt  [Total SGST],Rndoff as [Rnd.off],InvoiceAmt as [Bill Amount] FROM (";
                sql += " Select d.UserBillNo as BillNo,Replace(Convert(varchar, d.Billdate, 106), ' ', '-') as Billdate,d.Rndoff,d.PartyCode,d.PartyName,Sum(ExemptSale) as ExemptSale,Sum(d.Discount) as Discount,Sum(NetAmount_5) as NetAmount_5 ,Sum(IGST_5) as IGST_5,SUM(CGST_5) as CGST_5,SUM(SGST_5) as SGST_5,SUM(NetAmount_12) as 'NetAmount_12',SUM(IGST_12) as 'IGST_12',SUM(CGST_12) as CGST_12,SUM(SGST_12) as SGST_12,SUM(NetAmount_18) as 'NetAmount_18',SUM(IGST_18) as 'IGST_18',SUM(CGST_18) as CGST_18,SUM(SGST_18) as SGST_18,SUM(NetAmount_28) as 'NetAmount_28',SUM(IGST_28) as 'IGST_28',SUM(CGST_28) as CGST_28,SUM(SGST_28) as SGST_28,Sum(ExemptSale) + Sum(NetAmount_5) + Sum(NetAmount_12) + Sum(NetAmount_18) + SUM(NetAmount_28) as TotalAmount,Sum(IGST_5) + Sum(IGST_12) + Sum(IGST_18) + Sum(IGST_28) as TotalIGSTAmt,Sum(CGST_5) + Sum(CGST_12) + Sum(CGST_18) + Sum(CGST_28) as TotalCGSTAmt,Sum(SGST_5) + Sum(SGST_12) + Sum(SGST_18) + Sum(SGST_28) as TotalSGSTAmt,NetPayable as InvoiceAmt";
                sql += " from((";
                sql += " Select a.UserSBillNo,a.UserBillNo,a.Rndoff,a.NetPayable,a.BillDate,a.PartyCode,a.PartyName,a.Discount,a.NetAmount as ExemptSale ,0 as NetAmount_5,0 as IGST_5,0 as CGST_5,0 as SGST_5,0 as NetAmount_12,0 as IGST_12,0 as CGST_12,0 as SGST_12,0 as NetAmount_18,0 as IGST_18,0 as CGST_18,0 as SGST_18,0 as 'NetAmount_28',0 as 'IGST_28',0 as CGST_28,0 as SGST_28";
                sql += " from( Select b.UserSBillNo,b.UserBillNo,b.Rndoff,b.NetPayable,a.BillDate,b.FCode as PartyCode,b.PartyName+' - '+b.FCode as PartyName,a.Tax,Sum(a.Discount) as Discount,Sum(a.NetAmount) as NetAmount,0 as CGSTAmt,0 as SGSTAmt,0 as TaxAmount from TrnBillDetails as a,TrnBillMain as b where 1=1 " + TaxPerCondi0_ + wherea +" and a.BillNo=b.BillNo Group By b.UserSBillNo,b.UserBillNo,b.Rndoff,b.NetPayable,a.BillDate,a.Tax,b.PartyName,b.FCode ) as a ";
                sql += " UNION Select a.UserSBillNo,a.UserBillNo,a.Rndoff,a.NetPayable,a.BillDate,a.PartyCode,a.PartyName,a.Discount,0,a.NetAmount as 'NetAmount_5',a.TaxAmount as 'IGST_5',a.CGSTAmt as CGST_5,a.SGSTAmt as SGST_5,0 as 'NetAmount_12',0 as 'IGST_12',0 as CGST_12,0 as SGST_12,0 as NetAmount_18,0 as 'IGST_18',0 as CGST_18,0 as SGST_18,0 as 'NetAmount_28',0 as 'IGST_28',0 as CGST_28,0 as SGST_28";
                sql += " from(Select b.UserSBillNo,b.UserBillNo,b.Rndoff,b.NetPayable,a.BillDate,b.FCode as PartyCode,b.PartyName+' - '+b.FCode as PartyName,a.Tax,Sum(a.Discount) as Discount,Sum(a.NetAmount) as NetAmount,Sum(a.CGSTAmt) as CGSTAmt,Sum(a.SGSTAmt) as SGSTAmt,Sum(a.TaxAmount) as TaxAmount  from TrnBillDetails as a,TrnBillMain as b where 1=1 " + TaxPerCondi1_ + wherea + " and a.BillNo=b.BillNo Group By b.UserSBillNo,b.UserBillNo,b.NetPayable,b.Rndoff,a.BillDate,b.PartyName,a.Tax,a.CGST,a.SGST,b.FCode ) as a ";
                sql += " Union Select b.UserSBillNo,b.UserBillNo,b.Rndoff,b.NetPayable,b.BillDate,b.PartyCode,b.PartyName,b.Discount,0,0 as 'NetAmount_5',0 as 'IGST_5',0 as CGST_5,0 as SGST_5,NetAmount as 'NetAmount_12',TaxAmount as 'IGST_12',CGSTAmt as CGST_12,SGSTAmt as SGST_12,0 as NetAmount_18,0 as 'IGST_18',0 as CGST_18,0 as SGST_18,0 as 'NetAmount_28',0 as 'IGST_28',0 as CGST_28,0 as SGST_28";
                sql += " from(Select c.UserSBillNo,c.UserBillNo,c.Rndoff,c.NetPayable,b.BillDate,c.FCode as PartyCode,c.PartyName+' - '+c.FCode as PartyName,b.Tax,Sum(b.Discount) as Discount,Sum(b.NetAmount) as NetAmount,Sum(b.CGSTAmt) as CGSTAmt,Sum(b.SGSTAmt) as SGSTAmt,Sum(b.TaxAmount) as TaxAmount  from TrnBillDetails as b,TrnBillMain as c where 1=1 " + TaxPerCondi2_ + whereb + " and b.BillNo=c.BillNo Group By c.UserSBillNo,c.UserBillNo,c.NetPayable,c.Rndoff,b.BillDate,c.PartyName,b.Tax,b.CGST,b.SGST,c.FCode) as b";
                sql += " Union Select c.UserSBillNo,c.UserBillNo,c.Rndoff,c.NetPayable,c.BillDate,c.PartyCode,c.PartyName,c.Discount,0,0 as 'NetAmount_5',0 as 'IGST_5',0 as CGST_5,0 as SGST_5,0 as 'NetAmount_12',0 as 'IGST_12',0 as CGST_12,0 as SGST_12,NetAmount as NetAmount_18,TaxAmount as IGST_18,CGSTAmt as CGST_18,SGSTAmt as SGST_18,0 as 'NetAmount_28',0 as 'IGST_28',0 as CGST_28,0 as SGST_28";
                sql += " from (Select c.UserSBillNo,c.UserBillNo,c.Rndoff,c.NetPayable,b.BillDate,c.FCode as PartyCode,c.PartyName+' - '+c.FCode as PartyName,b.Tax,Sum(b.Discount) as Discount,Sum(b.NetAmount) as NetAmount,Sum(b.CGSTAmt) as CGSTAmt,Sum(b.SGSTAmt) as SGSTAmt,Sum(b.TaxAmount) as TaxAmount from TrnBillDetails as b,TrnBillMain as c where 1=1 " + TaxPerCondi3_ + whereb + " and b.BillNo=c.BillNo Group By c.UserSBillNo,c.UserBillNo,c.NetPayable,c.Rndoff,b.BillDate,c.PartyName,b.Tax,b.CGST,b.SGST,c.FCode) as c";
                sql += " Union Select e.UserSBillNo,e.UserBillNo,e.Rndoff,e.NetPayable,e.BillDate,e.PartyCode,e.PartyName,e.Discount,0,0 as 'NetAmount_5',0 as 'IGST_5',0 as CGST_5,0 as SGST_5,0 as 'NetAmount_12',0 as 'IGST_12',0 as CGST_12,0 as SGST_12,0 as 'NetAmount_18',0 as 'IGST_18',0 as CGST_18,0 as SGST_18,e.NetAmount as 'NetAmount_28',e.TaxAmount as 'IGST_28',e.CGSTAmt as 'CGST_28',e.SGSTAmt as 'SGST_28' ";
                sql += " from (Select f.UserSBillNo,f.UserBillNo,f.Rndoff,f.NetPayable,e.BillDate,e.FCode as PartyCode,f.PartyName+' - '+e.FCode as PartyName,e.Tax,Sum(e.Discount) as Discount,Sum(e.NetAmount) as NetAmount,Sum(e.CGSTAmt) as CGSTAmt,Sum(e.SGSTAmt) as SGSTAmt,Sum(e.TaxAmount) as TaxAmount from TrnBillDetails as e,TrnBillMain as f where 1=1 " + TaxPerCondi4_ + wheree +" and e.BillNo=f.BillNo Group By f.UserSBillNo,f.UserBillNo,f.NetPayable,f.Rndoff,e.BillDate,f.PartyName,e.Tax,e.CGST,e.SGST,e.FCode)as e)) as d  WHERE Cast(d.BillDate as Datetime)>='1-Jul-2017' Group By UserSBillNo,UserBillNo,Rndoff,BillDate,PartyCode,PartyName,NetPayable) as M LEFT JOIN M_LedgerMaster as L On M.PartyCode=L.PartyCode  Order By M.BillNo,M.BillDate";

                string InvConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(InvConnectionString);
                SqlCommand cmd = new SqlCommand();
                
                cmd.CommandText = sql;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                   while (reader.Read())
                    {
                        SaleRegister obj = new SaleRegister();
                        obj.Basic_12 = reader["Basic 12%"] != null ? Convert.ToDecimal(reader["Basic 12%"]) : 0 ;
                        obj.Basic_28 = reader["Basic 28%"] != null ? Convert.ToDecimal(reader["Basic 28%"]) : 0;
                        obj.Basic_5 = reader["Basic 5%"] != null ? Convert.ToDecimal(reader["Basic 5%"]) : 0;
                        obj.Basic_for_18 = reader["Basic for 18%"] != null ? Convert.ToDecimal(reader["Basic for 18%"]) : 0;
                        obj.BillAmount = reader["Bill Amount"] != null ? Convert.ToDecimal(reader["Bill Amount"]) : 0;
                        obj.Billdate = reader["Billdate"] != null ? Convert.ToString(reader["Billdate"]) : "";
                        obj.BillNo = reader["BillNo"] != null ? Convert.ToString(reader["BillNo"]) : "";
                        obj.CGST1_25 = reader["CGST@2.5%"] != null ? Convert.ToDecimal(reader["CGST@2.5%"]) : 0;
                        obj.CGST2_25 = reader["CGST @2.5%"] != null ? Convert.ToDecimal(reader["CGST @2.5%"]) : 0;
                        obj.CGST_14 = reader["CGST @14%"] != null ? Convert.ToDecimal(reader["CGST @14%"]) : 0;
                        obj.CGST_6 = reader["CGST @6%"] != null ? Convert.ToDecimal(reader["CGST @6%"]) : 0;
                        obj.CGST_9 = reader["CGST @9%"] != null ? Convert.ToDecimal(reader["CGST @9%"]) : 0;
                        obj.Code = reader["Code"] != null ? Convert.ToString(reader["Code"]) : "";
                        obj.Discount = reader["Discount"] != null ? Convert.ToDecimal(reader["Discount"]) : 0;
                        obj.ExemptSale = reader["ExemptSale"] != null ? Convert.ToDecimal(reader["ExemptSale"]) : 0;
                        obj.GSTIN = reader["GSTIN"] != null ? Convert.ToString(reader["GSTIN"]) : "";
                        obj.IGST_12 = reader["IGST @12%"] != null ? Convert.ToDecimal(reader["IGST @12%"]) : 0;
                        obj.IGST_18 = reader["IGST @18%"] != null ? Convert.ToDecimal(reader["IGST @18%"]) : 0;
                        obj.IGST_28 = reader["IGST @28%"] != null ? Convert.ToDecimal(reader["IGST @28%"]) : 0;
                        obj.IGST_5 = reader["IGST@5%"] != null ? Convert.ToDecimal(reader["IGST@5%"]) : 0;
                        obj.PartyName = reader["PartyName"] != null ? Convert.ToString(reader["PartyName"]) : "";
                        obj.RndOff = reader["Rnd.off"] != null ? Convert.ToDecimal(reader["Rnd.off"]) : 0;
                        obj.SGST_14 = reader["SGST @14%"] != null ? Convert.ToDecimal(reader["SGST @14%"]) : 0;
                        obj.SGST_6 = reader["SGST @6%"] != null ? Convert.ToDecimal(reader["SGST @6%"]) : 0;
                        obj.SGST_9 = reader["SGST @9%"] != null ? Convert.ToDecimal(reader["SGST @9%"]) : 0;
                        obj.TotalAmt = reader["Total Amt."] != null ? Convert.ToDecimal(reader["Total Amt."]) : 0;
                        obj.TotalCGST = reader["Total CGST"] != null ? Convert.ToDecimal(reader["Total CGST"]) : 0;
                        obj.TotalIGST = reader["Total IGST"] != null ? Convert.ToDecimal(reader["Total IGST"]) : 0;
                        obj.TotalSGST = reader["Total SGST"] != null ? Convert.ToDecimal(reader["Total SGST"]) : 0;
                        objReport.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
              
            }
            return objReport;
        }

        public List<PaymentSummaryReport> GetPaymentSummaryReport(string FromDate, string ToDate, string PartyCode, string Type)
        {
            List<PaymentSummaryReport> objReport = new List<PaymentSummaryReport>();
            string WhereCondition = string.Empty;
            string Fld = string.Empty;

            try
            {
                DateTime StartDate = new DateTime();
                DateTime EndDate = new DateTime();

                if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                {
                    var SplitDate = FromDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    StartDate = StartDate.Date;
                }
                if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                {
                    var SplitDate = ToDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    EndDate = EndDate.Date;
                }

                string NewFromDate = StartDate.Date.ToString("dd-MMM-yyyy");
                string NewToDate = EndDate.Date.ToString("dd-MMM-yyyy");

                if (!string.IsNullOrEmpty(PartyCode) && PartyCode.ToUpper() != "0")
                {
                    WhereCondition = WhereCondition + " and SoldBy ='" + PartyCode + "'";
                }

                if (!string.IsNullOrEmpty(FromDate) && FromDate.ToUpper() != "ALL")
                {
                    WhereCondition = WhereCondition + " and BillDate >='" + NewFromDate + "'";
                }

                if (!string.IsNullOrEmpty(ToDate) && ToDate.ToUpper() != "ALL")
                {
                    WhereCondition = WhereCondition + " and BillDate <='" + NewToDate + "'";
                }

                if (Type == "B")
                {
                    Fld = "BillNo,BillDate, SoldBy,SoldByName,OrderNo, FCode,PartyName";
                }
                else if (Type == "D")
                {
                    Fld = "BillDate,SoldBy,SoldByName";
                }
                else if (Type == "P")
                {
                    Fld = "SoldBy,SoldByName";
                }

                string sql = "Select " + Fld + ",Sum(BillAmt) as BillAmt,Sum(CashAmt) as C,Sum(ChqAmt) as Q,Sum(DDAmt) as D,Sum(CreditCardAmt) as CC,Sum(BankDeposit) as BD,Sum(WalletAmt) as W,Sum(DebitCardAmt) as DB,Sum(NetBanking) as NB,Sum(Credit) as T FROM V#PaymentModeWiseDetail WHERE 1=1" + WhereCondition + " GROUP BY " + Fld;

                string InvConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(InvConnectionString);
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PaymentSummaryReport tempobj = new PaymentSummaryReport();

                        tempobj.BillNo = "";
                        tempobj.BillDate = "";
                        tempobj.IDNo = "";
                        tempobj.IdName = "";
                        tempobj.Order = "";

                        if (Type == "B")
                        {
                            tempobj.BillNo = reader["BillNo"] != null ? reader["BillNo"].ToString() : "";
                            tempobj.BillDate = reader["BillDate"] != null ? Convert.ToDateTime(reader["BillDate"]).ToShortDateString() : "";
                            tempobj.IDNo = reader["FCode"] != null ? reader["FCode"].ToString() : "";
                            tempobj.IdName = reader["PartyName"] != null ? reader["PartyName"].ToString() : "";
                            tempobj.Order = reader["OrderNo"] != null ? reader["OrderNo"].ToString() : "";

                        }
                        else if (Type == "D")
                        {
                            tempobj.BillDate = reader["BillDate"] != null ? Convert.ToDateTime(reader["BillDate"]).ToShortDateString() : "";
                        }

                        tempobj.Name = reader["SoldByName"] != null ? reader["SoldByName"].ToString() : "";
                        tempobj.BillBy = reader["SoldBy"] != null ? reader["SoldBy"].ToString() : "";

                        tempobj.Amount = reader["BillAmt"] != null ? reader["BillAmt"].ToString() : "";
                        tempobj.Cash = reader["C"] != null ? reader["C"].ToString() : "";
                        tempobj.Cheque = reader["Q"] != null ? reader["Q"].ToString() : "";
                        tempobj.dd = reader["D"] != null ? reader["D"].ToString() : "";
                        tempobj.CreditCard = reader["CC"] != null ? reader["CC"].ToString() : "";
                        tempobj.BankDeposit = reader["BD"] != null ? reader["BD"].ToString() : "";
                        tempobj.DeditCard = reader["DB"] != null ? reader["DB"].ToString() : "";
                        tempobj.NetBanking = reader["NB"] != null ? reader["NB"].ToString() : "";
                        tempobj.Credit = reader["T"] != null ? reader["T"].ToString() : "";
                        tempobj.Wallet = reader["W"] != null ? reader["W"].ToString() : "";


                        objReport.Add(tempobj);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objReport;
        }

        public List<PaymentMode> GetPaymodeList()
        {
            List<PaymentMode> paymode = new List<PaymentMode>();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    paymode = (from r in entity.M_PayModeMaster
                               where r.IsShow == "Y"
                               select new PaymentMode
                               {
                                   payMode = r.PayMode,
                                   prefix = r.Prefix
                               }).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return (paymode);
        }

        public List<MonthlySumm>  GetMonthlyReport(string PartyCode,string BillType,string ProdType, string mnth)
        {
            List<MonthlySumm> objSumm= new List<MonthlySumm>();
            try
            {
                //using (var db = new InventoryEntities()) {
                //    objSumm = (from r in db.MonthWiseSummary(PartyCode, BillType, ProdType)
                //              select new MonthlySumm
                //              {
                //                  Mnth = r.Mnth,
                //                  NetPayable = r.NetPayable,
                //                  PvValue = r.PVValue
                //              }).ToList();
                //}
                string Sql = string.Empty;
                string InvConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(InvConnectionString);

                SqlCommand cmd = new SqlCommand();
                if (mnth.ToLower().Contains("all")) mnth = "";

                Sql = "Exec MonthlySummary '"+ PartyCode + "','"+ BillType +"','"+ ProdType + "','"+ mnth + "'";
                cmd.CommandText = Sql;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MonthlySumm obj = new MonthlySumm();
                        obj.Mnth = reader["mnth"].ToString();
                        obj.PartyCode= reader["PartyCode"].ToString();
                        obj.PartyName = reader["PartyName"].ToString();
                        obj.Mnth = reader["Mnth"].ToString();
                        obj.NetPayable= Convert.ToDecimal( reader["NetPayable"].ToString());
                        obj.PvValue = Convert.ToDecimal(reader["PVValue"].ToString());
                        objSumm.Add(obj);
                    }
                }

                    }
            catch (Exception)
            {
                throw;
            }
            return objSumm;
        }

        public List<ProductSummary> GetProductSummary(string FromDate,string ToDate)
        {
            List<ProductSummary> objSumm = new List<ProductSummary>();
            try
            {
                DateTime startDate, endDate;
                if (FromDate == "" || FromDate.ToUpper() == "ALL" || FromDate == null)
                    startDate = DateTime.Now.AddYears(-5);
                else
                    startDate = Convert.ToDateTime(FromDate);

                if (ToDate == "" || ToDate.ToUpper() == "ALL" || ToDate == null)
                    endDate = DateTime.Now;
                else
                    endDate = Convert.ToDateTime(ToDate);

                using (var db = new InventoryEntities())
                {
                    objSumm = (from r in db.ProductSummary(startDate, endDate)
                             select new ProductSummary {
                                 ProdID=r.ProdID,
                                 ProductName=r.ProductName,
                                 DP=r.DP,
                                 WrToFr=r.WrToFr,
                                 WrToDist = r.WrToDist,
                                 FrToDist = r.FrToDist,
                             }).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objSumm;
        }

        public List<SalesReturnReport> GetPurchaseReturnReport(string FromDate, string ToDate, string ProductCode, string CategoryCode, string SupplierCode, string Type)
        {
            var report = new List<SalesReturnReport>();
            try
            {
                DateTime StartDate = DateTime.Now.Date;
                DateTime EndDate = DateTime.Now.Date;
                if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                {
                    var SplitDate = FromDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    StartDate = StartDate.Date;
                }
                if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                {
                    var SplitDate = ToDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    EndDate = EndDate.Date;
                }

                string NewFromDate = StartDate.Date.ToString("dd-MMM-yyyy");
                string NewToDate = EndDate.Date.ToString("dd-MMM-yyyy");

                string WhereCondition = string.Empty;

                if (!string.IsNullOrEmpty(FromDate) && FromDate.ToUpper() != "ALL")
                {
                    WhereCondition = WhereCondition + " and c.ReturnDate>='" + NewFromDate + "'";
                }
                if (!string.IsNullOrEmpty(ToDate) && ToDate.ToUpper() != "ALL")
                {
                    WhereCondition = WhereCondition + " and c.ReturnDate<='" + NewToDate + "'";
                }
                if (!string.IsNullOrEmpty(ProductCode) && ProductCode.ToUpper() != "0")
                {
                    WhereCondition = WhereCondition + " and a.ProdId ='" + ProductCode + "'";
                }
                if (!string.IsNullOrEmpty(CategoryCode) && CategoryCode.ToUpper() != "0")
                {
                    WhereCondition = WhereCondition + " and b.CatId = '" + CategoryCode + "'";
                }
                if (!string.IsNullOrEmpty(SupplierCode) && SupplierCode.ToUpper() != "0")
                {
                    WhereCondition = WhereCondition + " and a.ReturnTo ='" + SupplierCode + "'";
                }


                string Sql = string.Empty;


                if (Type.ToLower() == "detail")
                {
                    Sql = "Select c.ReturnNo,Replace(Convert(varchar,c.ReturnDate,106),' ','-') as GRDate,c.ReturnBy,c.ReturnByName,a.ProdId ,a.ProductName,Sum(a.ReturnQty) as Qty,a.Rate as Rate,a.Tax as TaxPer,Sum(a.TaxAmount) as TaxAmount,Sum(a.Amount) as Amount,";
                    Sql = Sql + "Sum(a.Amount)+Sum(a.TaxAmount) as NetPayable,c.OrderNo as STNo,c.ReturnNo,'' as PartyType,ReturnToName,'' StateName";
                    Sql = Sql + " From TrnPurchaseReturnDetail as a,TrnPurchaseReturnMain as c,M_ProductMaster as b where  a.ReturnNo=c.ReturnNo And a.ProdId=b.ProdId";
                    Sql = Sql + WhereCondition;
                    Sql = Sql + " Group By c.ReturnNo,c.ReturnDate,a.ProdId,a.ProductName,a.Tax,a.Rate,c.ReturnBy,c.ReturnByName,ReturnToName,c.OrderNo Order By a.ProdId,c.ReturnBy";
                }

                else
                {
                    Sql = "Select '' as ReturnNo,'' as GRDate, a.ReturnBy, '' As PartyName,'' As ReturnByName,'' As STNo, a.ProdId,c.ReturnToName,";
                    Sql = Sql + " a.ProductName,Sum(a.ReturnQty) as Qty,b.Dp as Rate,0 As TaxPer,Sum(a.TaxAmount) as TaxAmount, Sum(A.Amount) As Amount,";
                    Sql = Sql + " Sum(a.Amount+A.TaxAmount) as NetPayable,'' as STNNo,'' as BillNo, '' As PartyType,d.PartyCode + ' - ' + d.PartyName as SoldPartyName,'' StateName ";
                    Sql = Sql + " From TrnPurchaseReturnDetail as a,M_ProductMaster as b,TrnPurchaseReturnMain as c ,M_LedgerMaster as d ";
                    Sql = Sql + " Where c.ReturnTo=d.PartyCode And a.ReturnNo=c.ReturnNo And a.ProdId=b.ProdId ";
                    Sql = Sql + WhereCondition;
                    Sql = Sql + " Group By a.ReturnBy,c.ReturnDate,c.ReturnToName,a.ProdId,a.ProductName,b.Dp,d.PartyCode + ' - ' + d.PartyName Order By a.ProdId";
                }

                string InvConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(InvConnectionString);
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = Sql;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesReturnReport tempobj = new SalesReturnReport();

                        tempobj.ProductCode = reader["ProdId"] != null ? reader["ProdId"].ToString() : "";
                        tempobj.ProductName = reader["ProductName"] != null ? reader["ProductName"].ToString() : "";
                        tempobj.STRNo = reader["ReturnNo"] != null ? reader["ReturnNo"].ToString() : "";
                        tempobj.STRDate = reader["GRDate"] != null ? reader["GRDate"].ToString() : "";
                        tempobj.ReturnBy = reader["ReturnBy"] != null ? reader["ReturnBy"].ToString() : "";
                        tempobj.ReturnByName = reader["ReturnByName"] != null ? reader["ReturnByName"].ToString() : "";
                        tempobj.Quantity = reader["Qty"] != null ? reader["Qty"].ToString() : "0";
                        tempobj.BasicAmt = reader["Amount"] != null ? reader["Amount"].ToString() : "0";
                        tempobj.TaxAmt = reader["TaxAmount"] != null ? reader["TaxAmount"].ToString() : "0";
                        tempobj.Tax = reader["TaxPer"] != null ? reader["TaxPer"].ToString() : "0";
                        tempobj.TotalAmt = reader["NetPayable"] != null ? reader["NetPayable"].ToString() : "";
                        tempobj.Rate = reader["Rate"] != null ? reader["Rate"].ToString() : "";
                        tempobj.ReturnTo = reader["ReturnToName"] != null ? reader["ReturnToName"].ToString() : "";
                        tempobj.BillNo = reader["STNo"] != null ? reader["STNo"].ToString() : "";
                        report.Add(tempobj);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return report;
        }

        public List<SalesReport> GetWalletHistory(string FromDate, string ToDate, string PartyCode)
        {
            DateTime StartDate = DateTime.Now.AddYears(-5);
            DateTime EndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(FromDate) && (!string.IsNullOrEmpty(ToDate)))
            {
                if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                {
                    var SplitDate = FromDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    StartDate = StartDate.Date;
                }
                if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                {
                    var SplitDate = ToDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                    EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    EndDate = EndDate.Date;
                }
            }

            List<SalesReport> objWalletHistory = new List<SalesReport>();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    objWalletHistory = (from r in entity.WalletReport(PartyCode)
                                        where r.VoucherDateOnly >= StartDate && r.VoucherDateOnly <= EndDate
                                        select new SalesReport
                                        {
                                            RecordDate = r.VoucherDate,
                                            Reason = r.Narration,
                                            TotalAmount = r.DepositedAmt,
                                            TaxAmount = r.UsedAmt,
                                            NetPayable = r.Balance.ToString()
                                        }).ToList();
                    foreach (SalesReport obj in objWalletHistory)
                    {
                        obj.StrBillDate = obj.RecordDate.ToString("dd-MMM-yyyy");
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return (objWalletHistory);
        }

        public List<SalesReturnReport> GetSalesReturnReport(string FromDate, string ToDate, string ProductCode, string CategoryCode, string PartyCode, string PartyType, string Type)
        {
            var report = new List<SalesReturnReport>();
            try
            {
                DateTime StartDate = DateTime.Now.Date;
                DateTime EndDate = DateTime.Now.Date;
                if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
                {
                    var SplitDate = FromDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + SplitDate[0] + "/" + SplitDate[2];
                    StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    StartDate = StartDate.Date;
                }
                if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
                {
                    var SplitDate = ToDate.Split('-');
                    string NewDate = SplitDate[1] + "/" + SplitDate[0] + "/" + SplitDate[2];
                    EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                    EndDate = EndDate.Date;
                }

                string NewFromDate = StartDate.Date.ToString("dd-MMM-yyyy");
                string NewToDate = EndDate.Date.ToString("dd-MMM-yyyy");

                string WhereCondition = string.Empty;

                if (!string.IsNullOrEmpty(FromDate) && FromDate.ToUpper() != "ALL")
                {
                    WhereCondition = WhereCondition + " and c.ReturnDate>='" + NewFromDate + "'";
                }
                if (!string.IsNullOrEmpty(ToDate) && ToDate.ToUpper() != "ALL")
                {
                    WhereCondition = WhereCondition + " and c.ReturnDate<='" + NewToDate + "'";
                }
                if (!string.IsNullOrEmpty(ProductCode) && ProductCode.ToUpper() != "0")
                {
                    WhereCondition = WhereCondition + " and a.ProdId ='" + ProductCode + "'";
                }
                if (!string.IsNullOrEmpty(CategoryCode) && CategoryCode.ToUpper() != "0")
                {
                    WhereCondition = WhereCondition + " and b.CatId = '" + CategoryCode + "'";
                }
                if (!string.IsNullOrEmpty(PartyCode) && PartyCode.ToUpper() != "0")
                {
                    WhereCondition = WhereCondition + " and a.ReturnBy ='" + PartyCode + "'";
                }
                if (!string.IsNullOrEmpty(PartyType) && PartyType.ToUpper() != "ALL")
                {
                    if (PartyType.ToLower() == "customer")
                    {
                        WhereCondition = WhereCondition + " and c.Ftype ='GC'";
                    }
                    else if (PartyType.ToLower() == "distributor")
                    {
                        WhereCondition = WhereCondition + " and c.Ftype ='M'";
                    }
                    else
                    {
                        WhereCondition = WhereCondition + " and c.Ftype Not in('M','GC')";
                    }
                }

                string Sql = string.Empty;
                string InvConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(InvConnectionString);

                SqlCommand cmd = new SqlCommand();

                if (Type.ToLower() == "detail")
                {
                    Sql = "Select c.SReturnNo,Replace(Convert(varchar,c.ReturnDate,106),' ','-') as GRDate,c.ReturnBy,c.ReturnByName,a.ProdId ,a.ProductName,Sum(a.ReturnQty) as Qty,a.Rate as Rate,a.Tax as TaxPer,Sum(a.TaxAmount) as TaxAmount,Sum(a.Amount) as Amount,";
                    Sql = Sql + "Sum(a.Amount)+Sum(a.TaxAmount) as NetPayable,bill.UserBillNo as STNo,c.SReturnNo,Case When c.Ftype='M' then 'Distributor' else Case When c.FType='GC' then 'Customer' else Case When c.Ftype Not in('M','GC') then 'Party' end end end as PartyType,ReturnToName,'' StateName";
                    Sql = Sql + " From TrnSalesReturnDetail as a,TrnSalesReturnMain as c,M_ProductMaster as b,TrnBillMain as bill where  a.SReturnNo=c.SReturnNo And a.ProdId=b.ProdId AND c.OrderNo = bill.BillNo";
                    Sql = Sql + WhereCondition;
                    Sql = Sql + " Group By c.SReturnNo,c.ReturnDate,a.ProdId,a.ProductName,a.Tax,a.Rate,c.ReturnBy,c.ReturnByName,c.FType,ReturnToName,bill.UserBillNo Order By c.FType,a.ProdId,c.ReturnBy";
                }

                else
                {
                    Sql = "Select '' as SReturnNo,'' as GRDate, a.ReturnBy, '' As PartyName,'' As ReturnByName,'' As STNo, a.ProdId,c.ReturnToName,";
                    Sql = Sql + " a.ProductName,Sum(a.ReturnQty) as Qty,b.Dp as Rate,0 As TaxPer,Sum(a.TaxAmount) as TaxAmount, Sum(A.Amount) As Amount,";
                    Sql = Sql + " Sum(a.Amount+A.TaxAmount) as NetPayable,'' as STNNo,'' as BillNo, '' As PartyType,d.PartyCode + ' - ' + d.PartyName as SoldPartyName,'' StateName ";
                    Sql = Sql + " From TrnSalesReturnDetail as a,M_ProductMaster as b,TrnSalesReturnMain as c ,M_LedgerMaster as d,TrnBillMain as bill ";
                    Sql = Sql + " Where c.ReturnTo=d.PartyCode And a.SReturnNo=c.SReturnNo And a.ProdId=b.ProdId AND c.OrderNo = bill.BillNo ";
                    Sql = Sql + WhereCondition;
                    Sql = Sql + " Group By a.ReturnBy,c.ReturnToName,a.ProdId,a.ProductName,b.Dp,d.PartyCode + ' - ' + d.PartyName Order By a.ProdId";
                }

                cmd.CommandText = Sql;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesReturnReport tempobj = new SalesReturnReport();

                        tempobj.ProductCode = reader["ProdId"] != null ? reader["ProdId"].ToString() : "";
                        tempobj.ProductName = reader["ProductName"] != null ? reader["ProductName"].ToString() : "";
                        tempobj.STRNo = reader["SReturnNo"] != null ? reader["SReturnNo"].ToString() : "";
                        tempobj.STRDate = reader["GRDate"] != null ? reader["GRDate"].ToString() : "";
                        tempobj.ReturnBy = reader["ReturnBy"] != null ? reader["ReturnBy"].ToString() : "";
                        tempobj.ReturnByName = reader["ReturnByName"] != null ? reader["ReturnByName"].ToString() : "";
                        tempobj.Quantity = reader["Qty"] != null ? reader["Qty"].ToString() : "0";
                        tempobj.BasicAmt = reader["Amount"] != null ? reader["Amount"].ToString() : "0";
                        tempobj.TaxAmt = reader["TaxAmount"] != null ? reader["TaxAmount"].ToString() : "0";
                        tempobj.Tax = reader["TaxPer"] != null ? reader["TaxPer"].ToString() : "0";
                        tempobj.TotalAmt = reader["NetPayable"] != null ? reader["NetPayable"].ToString() : "";
                        tempobj.Rate = reader["Rate"] != null ? reader["Rate"].ToString() : "";
                        tempobj.ReturnTo = reader["ReturnToName"] != null ? reader["ReturnToName"].ToString() : "";
                        tempobj.BillNo = reader["STNo"] != null ? reader["STNo"].ToString() : "";
                        report.Add(tempobj);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return report;
        }

        public List<KitDetail> GetOfferSessList()
        {
            List<KitDetail> KidIDs = new List<KitDetail>();
            try
            {
                string AppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(AppConnectionString);
                string query = " Select DISTINCT SessID,SUBSTRING(DateName(month, DateAdd(month,month(Frmdate), 0) - 1),1,3)+','+ Cast(Year(Frmdate) as varchar) +' to '  + SUBSTRING(DateName(month, DateAdd(month, month(Todate), 0) - 1), 1, 3) + ',' + Cast(Year(Todate) as varchar) SessName FROM OfferAchvdMem";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = query;

                cmd.Connection = SC;
                SC.Close();
                SC.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        KidIDs.Add(new KitDetail { KitId = decimal.Parse(reader["SessID"].ToString()), KitName = reader["SessName"].ToString() });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return KidIDs;
        }

        public List<CustomerDetail> GetOfferReport(int SessID,int OfferUID)
        {
            List<CustomerDetail> objMem = new List<CustomerDetail>();
            try
            {
                string AppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(AppConnectionString);
                string cond = "";
                if (SessID > 0)
                    cond = " AND a.SessID='"+ SessID +"' ";
                if (OfferUID > 0)
                    cond = cond + "AND a.OfferUID='" + OfferUID + "'";
                string query = "Select SUBSTRING(DateName(month, DateAdd(month,month(a.Frmdate), 0) - 1),1,3)+','+ Cast(Year(a.Frmdate) as varchar) +' to '  + SUBSTRING(DateName(month, DateAdd(month, month(a.Todate), 0) - 1), 1, 3) + ',' + Cast(Year(a.Todate) as varchar) SessName,a.Formno,b.MemFirstName,CASE WHEN a.OfferUID=1 THEN 'On Rs. 2100' WHEN a.OfferUID=7 THEN 'On Rs. 5100' ELSE '' END as Offer FROM OfferAchvdMem a, M_MemberMaster b WHERE a.Formno = b.Formno " + cond;
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = query;
                cmd.Connection = SC;
                SC.Close();
                SC.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        objMem.Add(new CustomerDetail  { SelectedInvoiceType= reader["SessName"].ToString(), FormNo = decimal.Parse(reader["Formno"].ToString()), Name = reader["MemFirstName"].ToString() , Remarks= reader["Offer"].ToString() });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objMem;
        }

        public List<KitDetail> GetMonthSessnList()
        {
            List<KitDetail> KidIDs = new List<KitDetail>();
            try
            {
                string AppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                SqlConnection SC = new SqlConnection(AppConnectionString);
                string query = " Select DISTINCT SessID,SUBSTRING(DateName(month, DateAdd(month,month(Frmdate), 0) - 1),1,3)+','+ Cast(Year(Frmdate) as varchar)  SessName " +
" FROM M_MonthSessnMaster WHERE SessID >= 4 ORDER BY SessID Desc";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = query;

                cmd.Connection = SC;
                SC.Close();
                SC.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        KidIDs.Add(new KitDetail { KitId = decimal.Parse(reader["SessID"].ToString()), KitName = reader["SessName"].ToString() });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return KidIDs;
        }

        public List<OfferResult_> GetOfferStatus(int SessID, int OfferUID)
        {
            List<OfferResult_> obj = new List<OfferResult_>();
            try
            {
                
                using (var entity = new InventoryEntities())
                {
                    obj = (from s in entity.OfferResult(SessID, OfferUID) select new OfferResult_ {
                        IDNo=s.IDNo,
                        MemName=s.MemName,
                        City=s.City,
                        MobileNo=s.MobileNo,
                        Mnth1=s.Mnth1,
                        Mnth2 = s.Mnth2,
                        Mnth3 = s.Mnth3,
                        Mnth4 = s.Mnth4,
                        Mnth5 = s.Mnth5,
                        Mnth6 = s.Mnth6,
                        Status=s.Status
                    }).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return obj;
        }

        public List<SalesReport> GetPrevOfferReport(string PartyCode, int OfferUID)
        {
            List<SalesReport> objSaleDetail = new List<SalesReport>();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    objSaleDetail = (from r in entity.FranchiseOfferDetail(PartyCode)
                                        select new SalesReport
                                        {
                                            OfferUID=r.OfferUID,
                                            BillNo = r.OfferName ,
                                            IdNo = r.IdNo,
                                            PartyName= r.MemFirstName,
                                            MobileNO= r.Mobl,
                                            March_Sales= r.PrevTotalAmount.ToString(),
                                            Feb_Sales = r.PrevTotalAmt2.ToString(),
                                            Jan_Sales = r.PrevAmount3.ToString(),
                                        }).ToList();
                    if (OfferUID > 0)
                        objSaleDetail = objSaleDetail.Where(m => m.OfferUID == OfferUID).ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return objSaleDetail;
        }

        public List<LogInfo> GetLogReport(string FromDate, string ToDate, string User)
        {
            List<LogInfo> objLogDetail = new List<LogInfo>();

            DateTime StartDate = DateTime.Now; 
            DateTime EndDate = DateTime.Now;

            if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
            {
                var SplitDate = FromDate.Split('-');
                string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                StartDate = StartDate.Date;
            }
            if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
            {
                var SplitDate = ToDate.Split('-');
                string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                EndDate = EndDate.Date;
            }
            int userId = 0;
            if (!string.IsNullOrEmpty(User) && User != "All" && User != "0")
            {
                userId = int.Parse(User);
            }
            try
            {
                using (var entity = new InventoryEntities())
                {
                    var objloglist = (from r in entity.M_LogMaster select r).ToList();

                    if (!string.IsNullOrEmpty(FromDate) && FromDate != "All" && !string.IsNullOrEmpty(ToDate) && ToDate != "All")
                    {
                        objLogDetail = (from r in objloglist
                                        where r.RecTimeStamp.Date >= StartDate.Date && r.RecTimeStamp.Date <= EndDate.Date
                                        select new LogInfo
                                        {
                                            LogDate = r.RecTimeStamp,
                                            LogStr = r.Log,
                                            UserName = r.UserName,
                                            UserId = r.UserId

                                        }).ToList();
                    }
                    else
                    {
                        objLogDetail = (from r in objloglist                                        
                                        select new LogInfo
                                        {
                                            LogDate = r.RecTimeStamp,
                                            LogStr = r.Log,
                                            UserName = r.UserName,
                                            UserId = r.UserId

                                        }).ToList();
                    }
                    if (userId != 0)
                    {
                        objLogDetail = objLogDetail.Where(r => r.UserId == userId).ToList();
                    }
                }                                
            }
            catch (Exception ex)
            {

            }
            return objLogDetail;
        }


        public List<PaymentGatewayReport> GetPaymentGatewayReport(string FromDate, string ToDate, string User)
        {
            List<PaymentGatewayReport> objPaymentGateway = new List<PaymentGatewayReport>();

            DateTime StartDate = DateTime.Now;
            DateTime EndDate = DateTime.Now;

            if (!string.IsNullOrEmpty(FromDate) && FromDate != "All")
            {
                var SplitDate = FromDate.Split('-');
                string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                StartDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                StartDate = StartDate.Date;
            }
            if (!string.IsNullOrEmpty(ToDate) && ToDate != "All")
            {
                var SplitDate = ToDate.Split('-');
                string NewDate = SplitDate[1] + "/" + ("0" + SplitDate[0]).Substring(("0" + SplitDate[0]).Length - 2, 2) + "/" + SplitDate[2];
                EndDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                EndDate = EndDate.Date;
            }
            
            try
            {
                using (var entity = new InventoryEntities())
                {                    

                    if (!string.IsNullOrEmpty(FromDate) && FromDate != "All" && !string.IsNullOrEmpty(ToDate) && ToDate != "All")
                    {

                        objPaymentGateway = (from r in entity.TblPaymentGetWayRequests
                                             where r.invdate >= StartDate && r.invdate <= EndDate 
                                             join t in entity.M_LedgerMaster on r.IdNo equals t.PartyCode into payment
                                             from tt in payment.DefaultIfEmpty()
                                             select new PaymentGatewayReport
                                             {
                                                 IdNo = r.IdNo,
                                                 ORDER_ID = r.ORDER_ID,
                                                 TXNID = r.TXNID,
                                                 invdate = r.invdate,
                                                 PartyName = tt.PartyName,
                                                 reqAmt = r.reqAmt,
                                                 remarks = r.remarks,
                                                 mop = r.mop
                                             }).ToList();
                    }
                    else
                    {
                        objPaymentGateway = (from r in entity.TblPaymentGetWayRequests
                                             join t in entity.M_LedgerMaster on r.IdNo equals t.PartyCode into payment
                                             from tt in payment.DefaultIfEmpty()
                                             select new PaymentGatewayReport
                                             {
                                                 IdNo = r.IdNo,
                                                 ORDER_ID = r.ORDER_ID,
                                                 TXNID = r.TXNID,
                                                 invdate = r.invdate,
                                                 PartyName = tt.PartyName,
                                                 reqAmt = r.reqAmt,
                                                 remarks = r.remarks,
                                                 mop = r.mop
                                             }).ToList();
                    }
                    if (!string.IsNullOrEmpty(User))
                    {
                        objPaymentGateway = objPaymentGateway.Where(r => r.IdNo == User).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objPaymentGateway;
        }

    }
}
