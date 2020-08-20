using InventoryManagement.API.Controllers;
using InventoryManagement.DataAccess.Contract;
using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static InventoryManagement.Entity.Common.StockReportModel;

namespace InventoryManagement.DataAccess
{
    public class ReportRepository: IReportRepository
    {
        ReportAPIController objReportAPI = new ReportAPIController();
        public List<ProductDetails> GetAllProducts(decimal CategoryCode)
        {
            return (objReportAPI.GetAllProducts(CategoryCode));
        }
        public List<ProductDetails> GetTransferredProduct()
        {
            return (objReportAPI.GetTransferredProduct());
        }
        public List<PartyModel> GetAllParty()
        {
            return (objReportAPI.GetAllParty());
        }
        public List<StockReportModel> GetStockReport(string CategoryCode, string ProductCode, string PartyCode, bool IsBatchWise, string StockType)
        {
            return (objReportAPI.GetStockReport(CategoryCode,ProductCode,PartyCode,IsBatchWise,StockType));
        }
        public List<SalesReport> GetSalesReport(string FromDate, string ToDate, string CustomerId, string ProductCode, string CategoryCode, string PartyCode, string BType, string SalesType, string InvoiceType , string BillNo, string FType,decimal OfferUID)
        {
            return (objReportAPI.GetSalesReport(FromDate, ToDate, CustomerId, ProductCode, CategoryCode, PartyCode, BType, SalesType,InvoiceType, BillNo, FType,OfferUID));
        }
        
        public List<SalesReport> GetDeletedSalesReport(string FromDate, string ToDate, string CustomerId,  string PartyCode, string BType,  string InvoiceType, string BillNo, string FType, decimal OfferUID, int DltDateWise)
        {
            return (objReportAPI.GetDeletedSalesReport(FromDate, ToDate, CustomerId, PartyCode, BType,  InvoiceType, BillNo, FType, OfferUID, DltDateWise));
        }
        public List<ProductModel> GetDltBillProduct(string UID)
        {
            return (objReportAPI.GetDltBillProduct(UID));
        }
        public List<StockJv> GetStockJvReport(string FromDate, string ToDate, string PartyCode,string ViewType)
        {
            return (objReportAPI.GetStockJvReport(FromDate, ToDate, PartyCode, ViewType));
        }
        public List<PurchaseReport> GetPurchaseSummary(string FromDate, string ToDate, string PartyCode, string SupplierCode, string ReportType,string InvoiceNo)
        {
            return (objReportAPI.GetPurchaseSummary(FromDate,ToDate,PartyCode,SupplierCode,ReportType, InvoiceNo));
        }
       
        public List<PurchaseReport> GetPurchaseDetailSummary(string FromDate, string ToDate, string PartyCode, string SupplierCode, string ProductCode)
        {
            return (objReportAPI.GetPurchaseDetailSummary(FromDate,ToDate,PartyCode,SupplierCode,ProductCode));
        }
        public List<string> GetYearList()
        {
            return (objReportAPI.GetYearList());
        }
       public  List<PurchaseReport> GetMonthWisePurchaseSummary(string Year, bool IsQuantity, bool IsAmount, string PartyCode, string SupplierCode)
        {
            return (objReportAPI.GetMonthWisePurchaseSummary(Year,IsQuantity,IsAmount,PartyCode,SupplierCode));
        }
       public List<string> GetSalesYearList()
       {
            return (objReportAPI.GetSalesYearList());
        }
       public List<SalesReport> GetMonthWiseSalesSummary(string Year, bool IsQuantity, bool IsAmount, string PartyCode)
        {
            return (objReportAPI.GetMonthWiseSalesSummary(Year, IsQuantity, IsAmount, PartyCode));
        }
        public List<StockReportModel> GetStockReceiptReport(string CategoryCode, string ProductCode, string PartyCode, string StateCode, string FromDate, string ToDate, string LoginPartyCode, bool isSummary)
        {
            return (objReportAPI.GetStockReceiptReport(CategoryCode,ProductCode,PartyCode,StateCode,FromDate,ToDate,LoginPartyCode,isSummary));
        }
        public List<SelectListItem> GetStateList()
        {
            return (objReportAPI.GetStateList());
        }
        public List<PartyWiseWalletDetails> GetPartyWiseWalletReport(string FromDate, string ToDate, string PartyCode, string ViewType)
        {
            return (objReportAPI.GetPartyWiseWalletReport(FromDate, ToDate, PartyCode, ViewType));
        }
        public List<PaymentSummaryReport> GetPaymentSummaryReport(string FromDate, string ToDate, string PartyCode, string Type)
        {
            return (objReportAPI.GetPaymentSummaryReport(FromDate, ToDate, PartyCode, Type));
        }

        public List<SaleRegister> GetSaleRegisterReport(string FromDate, string ToDate, string PartyCode)
        {
            return (objReportAPI.GetSaleRegisterReport(FromDate, ToDate, PartyCode));
        }

        public List<PaymentMode> GetPaymodeList()
        {
            return (objReportAPI.GetPaymodeList());
        }

        public List<SalesReturnReport> GetSalesReturnReport(string FromDate, string ToDate, string ProductCode, string CategoryCode, string PartyCode, string PartyType, string Type)
        {
            return (objReportAPI.GetSalesReturnReport(FromDate, ToDate, ProductCode, CategoryCode, PartyCode, PartyType, Type));
        }
        public List<SalesReturnReport> GetPurchaseReturnReport(string FromDate, string ToDate, string ProductCode, string CategoryCode, string SupplierCode, string Type)
        {
            return (objReportAPI.GetPurchaseReturnReport(FromDate, ToDate, ProductCode, CategoryCode, SupplierCode, Type));
        }
        public List<MonthlySumm> GetMonthlyReport(string PartyCode, string BillType,string ProdType, string mnth)
        {
            return (objReportAPI.GetMonthlyReport(PartyCode,  BillType, ProdType,  mnth));
        }

        public List<TransferredProduct> TransProductReport(string FromDate, string ToDate, string ProdCode)
        {
            return (objReportAPI.TransProductReport(FromDate, ToDate, ProdCode));
        }
        public List<ProductSummary> GetProductSummary(string FromDate, string ToDate)
        {
            return (objReportAPI.GetProductSummary(FromDate, ToDate));
        }
        public List<SalesReport> GetWalletHistory(string FromDate, string ToDate, string PartyCode)
        { return (objReportAPI.GetWalletHistory(FromDate, ToDate, PartyCode)); }
        public List<KitDetail> GetOfferSessList()
        {
            return (objReportAPI.GetOfferSessList());
        }
        
             public List<KitDetail> GetMonthSessnList()
        {
            return (objReportAPI.GetMonthSessnList());
        }
        public List<CustomerDetail> GetOfferReport(int SessID,int OfferUID)
        {
            return objReportAPI.GetOfferReport(SessID, OfferUID);
        }

        public List<OfferResult_> GetOfferStatus(int SessID, int OfferUID)
        {
            return objReportAPI.GetOfferStatus(SessID, OfferUID);
        }
        public List<StockReportModel> GetDateWiseStockReport(string CategoryCode, string ProductCode, string PartyCode, string FromDate, string ToDate)
        {
            return (objReportAPI.GetDateWiseStockReport(CategoryCode, ProductCode, PartyCode, FromDate, ToDate));
        }
        public List<StockReportModel> GetDailyStockReport(string CategoryCode, string ProductCode, string PartyCode, string FromDate, string ToDate)
        {
            return (objReportAPI.GetDailyStockReport(CategoryCode, ProductCode, PartyCode, FromDate, ToDate));
        }
        public List<SalesReport> GetPrevOfferReport(string PartyCode , int OfferUID)
        {
            return objReportAPI.GetPrevOfferReport(PartyCode, OfferUID);
        }
        public List<LogInfo> GetLogReport(string FromDate, string ToDate, string User)
        {
            return objReportAPI.GetLogReport(FromDate, ToDate,User);
        }

        public List<PaymentGatewayReport> GetPaymentGatewayReport(string FromDate, string ToDate, string User)
        {
            return objReportAPI.GetPaymentGatewayReport(FromDate, ToDate, User);
        }

    }
}