using InventoryManagement.App_Start;
using InventoryManagement.Business;
using InventoryManagement.Common;
using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace InventoryManagement.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        TransactionManager objTransactManager = new TransactionManager();
        ProductManager objProductManager = new ProductManager();
        ReportManager objReportManager = new ReportManager();
        UserManager objUserManager = new UserManager();
        LogManager objLogManager = new LogManager();
        // GET: Report
        [SessionExpire]
        public ActionResult StockReport()
        {
            StockReportModel objModel = new StockReportModel();
            objModel.ProductDetailsList = new List<ProductDetails>();

            //objModel.ProductDetailsList = objReportManager.GetAllProducts(0);
            //var jsonSerialiser = new JavaScriptSerializer();
            //if (objModel.ProductDetailsList.Count > 0)
            //{
            //    //var jsonProduct = jsonSerialiser.Serialize(objModel.ProductDetailsList);
            //    var jsonProduct=Json(objModel.ProductDetailsList, JsonRequestBehavior.AllowGet);
            //    jsonProduct.MaxJsonLength = int.MaxValue;
            //    ViewBag.ProductJsonList = jsonProduct;
            //}
            //else
            //{
            //    ViewBag.ProductJsonList = "";
            //}
           
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "StockReport"))
                return View(objModel);
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetStockReport(string CategoryCode, string ProductCode, string PartyCode, bool IsBatchWise, string StockType)
        {//**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<StockReportModel> objStockReportModel = new List<StockReportModel>();
            objStockReportModel = objReportManager.GetStockReport(CategoryCode, ProductCode, PartyCode, IsBatchWise, StockType);
            
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, "Generated stock report.", myIP + currentDate);
            
            return Json(objStockReportModel, JsonRequestBehavior.AllowGet);
        }

        //BillWise Sales Report
        [SessionExpire]
        public ActionResult SalesReport(string DirectView, string Buyer, string SoldBy,string Franchise,string Associate)
        {
            List<SelectListItem> objListInvoiceType = new List<SelectListItem>();
            objListInvoiceType.Add(new SelectListItem { Text = "All", Value = "" });
            objListInvoiceType.Add(new SelectListItem { Text = "Repurchase Invoice", Value = "RI" });
            objListInvoiceType.Add(new SelectListItem { Text = "Joining Invoice", Value = "JI" });
            objListInvoiceType.Add(new SelectListItem { Text = "Challan Invoice", Value = "CB" });
            ViewBag.InvoiceTypes = objListInvoiceType;            
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "SalesReport"))
            {
                ViewBag.DirectView = DirectView;
                ViewBag.SoldByParty = SoldBy;
                ViewBag.BuyerParty = Buyer;
                ViewBag.DirectViewFranchise = Franchise;
                ViewBag.DirectViewAssociate = Associate;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }


        [SessionExpire]
        public ActionResult DateSalesReport()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "DateSalesReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }
        [SessionExpire]
        public ActionResult ProductSalesReport()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "ProductSalesReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [SessionExpire]
        public ActionResult StockJvReport()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "StockJvReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [SessionExpire]
        public ActionResult ProductTransferbyID(string DirectView)
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "ProductTransferbyID"))
            {
                ViewBag.DirectView = DirectView;                
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }


        [HttpPost]
        public ActionResult GetStockJvReport(string FromDate, string ToDate, string PartyCode,string ViewType)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<StockJv> objStockJv = new List<StockJv>();
            objStockJv = objReportManager.GetStockJvReport(FromDate, ToDate, PartyCode, ViewType);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated stock jv report.", myIP + currentDate);
            var jsonStock = Json(objStockJv, JsonRequestBehavior.AllowGet);
            jsonStock.MaxJsonLength = int.MaxValue;
            return jsonStock;

        }

        [HttpPost]
        public ActionResult GetSalesReport(string FromDate, string ToDate, string CustomerId, string ProductCode, string CategoryCode, string PartyCode, string BType, string SalesType, string InvoiceType,string BillNo, string FType,decimal OfferUID)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<SalesReport> objSalesList = new List<SalesReport>();
            objSalesList = objReportManager.GetSalesReport(FromDate, ToDate, CustomerId, ProductCode, CategoryCode, PartyCode, BType, SalesType, InvoiceType, BillNo,FType, OfferUID);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated "+ SalesType + " Sales report.", myIP + currentDate);
            var jsonProduct = Json(objSalesList, JsonRequestBehavior.AllowGet);

            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
            //string returnresult = "";
            //if (string.IsNullOrEmpty(FromDate))
            //{
            //    returnresult = ToDate;
            //}
            //else if(string.IsNullOrEmpty(ToDate))
            //{
            //    returnresult = FromDate;
            //}
            //else
            //{
            //    returnresult = "All right";
            //}
            //return Json(returnresult, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult DeletedSalesReport()
        {
            List<SelectListItem> objListInvoiceType = new List<SelectListItem>();
            objListInvoiceType.Add(new SelectListItem { Text = "All", Value = "" });
            objListInvoiceType.Add(new SelectListItem { Text = "Repurchase Invoice", Value = "RI" });
            objListInvoiceType.Add(new SelectListItem { Text = "Joining Invoice", Value = "JI" });
            ViewBag.InvoiceTypes = objListInvoiceType;
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "DeletedSalesReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }
        [HttpPost]
        public ActionResult GetDeletedSalesReport(string FromDate, string ToDate, string CustomerId,  string PartyCode, string BType,  string InvoiceType, string BillNo, string FType, decimal OfferUID,int DltDateWise)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<SalesReport> objSalesList = new List<SalesReport>();
            objSalesList = objReportManager.GetDeletedSalesReport(FromDate, ToDate, CustomerId, PartyCode, BType,  InvoiceType, BillNo, FType, OfferUID,DltDateWise);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated Deleted Sales report.", myIP + currentDate);
            var jsonProduct = Json(objSalesList, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;         
        }

        [HttpPost]
        public ActionResult GetDltBillProduct(string UID)
        {
            List<ProductModel> objProductList = new List<ProductModel>();
            objProductList = objReportManager.GetDltBillProduct(UID);
            var jsonProduct = Json(objProductList, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
        }

        [HttpPost]
        public ActionResult GetPurchaseSummary(string FromDate, string ToDate, string PartyCode, string SupplierCode, string ReportType,string InvoiceNo)
        {
            List<PurchaseReport> objPurchaseList = new List<PurchaseReport>();
            objPurchaseList = objReportManager.GetPurchaseSummary(FromDate, ToDate, PartyCode, SupplierCode, ReportType, InvoiceNo);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated purchase summary.", myIP + currentDate);
            var jsonProduct = Json(objPurchaseList, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
            
        }

        [SessionExpire]
        public ActionResult PurchaseInvoicePrint(string Pm)
        {
            List<PurchaseReport> objInvoice = new List<PurchaseReport>();
            if (Session["LoginUser"] != null)
            {
                var base64DecodedBytes = System.Convert.FromBase64String(Pm);
                string InvoiceNoValue = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);
                objInvoice = objTransactManager.GetPurchaseInvoice(InvoiceNoValue);
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, "Generated Purchase Invoice for - "+ InvoiceNoValue, myIP + currentDate);
            }            
            return View(objInvoice);
        }
        public ActionResult GetAllCategory()
        {
            List<CategoryDetails> objCategory = new List<CategoryDetails>();
            objCategory = objProductManager.GetCategoryList("Y");

            return Json(objCategory, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAllProduct(decimal CategoryCode)
        {
            List<ProductDetails> objProduct = new List<ProductDetails>();
            objProduct = objReportManager.GetAllProducts(CategoryCode);
            var jsonProduct = Json(objProduct, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;

        }
        [HttpPost]
        public ActionResult GetTransferredProduct() {
            List<ProductDetails> objProduct = new List<ProductDetails>();
            objProduct = objReportManager.GetTransferredProduct();
            var jsonProduct = Json(objProduct, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
        }
        //public ActionResult GetAllParty()
        //{
        //    List<PartyModel> objparty = new List<PartyModel>();
        //    objparty = objReportManager.GetAllParty();
        //    return Json(objparty, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetAllParty()
        {
            List<PartyModel> objparty = new List<PartyModel>();
            string LoginPartyCode = "";
            decimal LoginStateCode = 0;
            if (Session["LoginUser"] != null)
            {
                LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                LoginStateCode = (Session["LoginUser"] as User).StateCode;
            }
            objparty = objTransactManager.GetAllParty(LoginPartyCode, LoginStateCode);
            return Json(objparty, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllPartyListForReports()
        {
            List<PartyModel> objparty = new List<PartyModel>();
            
            objparty = objReportManager.GetAllParty();
            return Json(objparty, JsonRequestBehavior.AllowGet);
        }
        [SessionExpire]
        public ActionResult PurchaseSummary(string DirectView)
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "PurchaseSummary"))
            {
                ViewBag.DirectView = DirectView;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }
       
        public ActionResult GetSupplier()
        {
            List<PartyModel> objListSupplier = new List<PartyModel>();
            decimal LoginStateCode = (Session["LoginUser"] as User).StateCode;
            string LoginPartyCode= (Session["LoginUser"] as User).PartyCode;
            objListSupplier = objTransactManager.GetAllSupplierList(LoginPartyCode,LoginStateCode);
            return Json(objListSupplier, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult PurchaseDetailSummary(string DirectView)
        {           
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "PurchaseDetailSummary"))
            {
                ViewBag.DirectView = DirectView;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetMonthWisePurchaseSummary(string Year, bool IsQuantity, bool IsAmount, string PartyCode, string SupplierCode)
        {
            List<PurchaseReport> objPurchaseList = new List<PurchaseReport>();
            objPurchaseList = objReportManager.GetMonthWisePurchaseSummary(Year,IsQuantity, IsAmount, PartyCode, SupplierCode);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated monthwwise purchase summary", myIP + currentDate);
            var jsonProduct = Json(objPurchaseList, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;

        }

        [HttpPost]
        public ActionResult GetMonthWiseSalesSummary(string Year, bool IsQuantity, bool IsAmount, string PartyCode)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<SalesReport> objSalesList = new List<SalesReport>();
            objSalesList = objReportManager.GetMonthWiseSalesSummary(Year, IsQuantity, IsAmount, PartyCode);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated monthwise sale summary", myIP + currentDate);
            var jsonProduct = Json(objSalesList, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;

        }

        [HttpPost]
        public ActionResult GetPurchaseDetailSummary(string FromDate, string ToDate, string PartyCode, string SupplierCode, string ProductCode)
        {
            List<PurchaseReport> objPurchaseList = new List<PurchaseReport>();
            objPurchaseList = objReportManager.GetPurchaseDetailSummary(FromDate, ToDate, PartyCode, SupplierCode, ProductCode);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated purchase detail report", myIP + currentDate);
            var jsonProduct = Json(objPurchaseList, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;

        }
        [SessionExpire]
        public ActionResult OrderHistory()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "OrderHistory"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }
        [SessionExpire]
        public ActionResult MonthlySummary()
        {
            var KitIdlist = objReportManager.GetMonthSessnList();
            List<SelectListItem> KidIsListObj = new List<SelectListItem>();
              KidIsListObj.Add(new SelectListItem { Text = "--ALL--", Value = "" });
            foreach (var obj in KitIdlist)
            {
                KidIsListObj.Add(new SelectListItem { Text = obj.KitName, Value = obj.KitId.ToString() });
            }
            ViewBag.OfferSessList = KidIsListObj;
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "MonthlySummary"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetMonthlyReport(string PartyCode, string BillType, string ProdType, string mnth)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode=="" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            if (PartyCode.ToLower() == "all" || PartyCode == "0")
                PartyCode = "";
            List<MonthlySumm> objSumm = new List<MonthlySumm>();
            objSumm = objReportManager.GetMonthlyReport(PartyCode, BillType, ProdType, mnth);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated monthly sales report.", myIP + currentDate);
            var jsonProduct = Json(objSumm, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
        }

        [SessionExpire]
        public ActionResult ProductSummary()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "ProductSummary"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }
        [HttpPost]
        public ActionResult GetProductSummary(string FromDate, string ToDate) {
            List<ProductSummary> objSumm = new List<ProductSummary>();
            objSumm = objReportManager.GetProductSummary(FromDate, ToDate);
            var jsonproduct = Json(objSumm, JsonRequestBehavior.AllowGet);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated product summary report", myIP + currentDate);
            jsonproduct.MaxJsonLength = int.MaxValue;
            return jsonproduct;
        }

        [SessionExpire]
        public ActionResult MonthWiseSalesSummary()
        {
            List<string> objYear = new List<string>();
            objYear = objReportManager.GetSalesYearList();
            List<SelectListItem> objList = new List<SelectListItem>();
            if (objYear.Count() > 0)
            {
                objList.Add(new SelectListItem
                {
                    Text = "All",
                    Value = "0"
                });
            }
            else
            {
                objList.Add(new SelectListItem
                {
                    Text = "--No Record--",
                    Value = "0"
                });
            }
            foreach (var obj in objYear)
            {
                objList.Add(new SelectListItem
                {
                    Text = obj,
                    Value = obj
                });
            }
            ViewBag.ListYear = objList;
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "MonthWiseSalesSummary"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [SessionExpire]
        public ActionResult MonthWisePurchaseSummary()
        {
            List<string> objYear = new List<string>();
            objYear = objReportManager.GetYearList();
            List<SelectListItem> objList = new List<SelectListItem>();
            if (objYear.Count() > 0)
            {
                objList.Add(new SelectListItem
                {
                    Text = "All",
                    Value = "0"
                });
            }
            else
            {
                objList.Add(new SelectListItem
                {
                    Text= "--No Record--",
                    Value = "0"
                });
            }
            foreach (var obj in objYear)
            {
                objList.Add(new SelectListItem
                {
                    Text = obj,
                    Value = obj
                });
            }
            ViewBag.ListYear = objList;
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "MonthWisePurchaseSummary"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [SessionExpire]
        public ActionResult StockReceiptReport()
        {
            StockReportModel objModel = new StockReportModel();
            objModel.ProductDetailsList = new List<ProductDetails>();
            objModel.StateList = objReportManager.GetStateList();
            string LoginPartyName = (Session["LoginUser"] as User).PartyName;
            string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
            int LoginGroupId = (Session["LoginUser"] as User).GroupId;
            objModel.PartyName = LoginPartyName;
            objModel.PartyCode = LoginPartyCode;
            objModel.GroupId = LoginGroupId;
            objModel.CategoryList = objProductManager.GetCategoryList("Y");
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "StockReceiptReport"))
                return View(objModel);
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetStockReceiptReport(string CategoryCode, string ProductCode, string PartyCode, string StateCode, string FromDate, string ToDate, bool isSummary)
        {
            List<StockReportModel> objStockReportModel = new List<StockReportModel>();
            string LoginPartyCode = "";
            if (Session["LoginUser"] != null)
            {
                LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
            }
            objStockReportModel = objReportManager.GetStockReceiptReport(CategoryCode, ProductCode, PartyCode, StateCode, FromDate, ToDate, LoginPartyCode, isSummary);

            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated stock recipt report", myIP + currentDate);

            var jsonProduct = Json(objStockReportModel, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
            //return Json(objStockReportModel, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult PartyWiseBalanceReport()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "PartyWiseBalanceReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }
        [HttpPost]
        public ActionResult GetPartyWiseWalletReport(string FromDate, string ToDate, string PartyCode, string ViewType)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<PartyWiseWalletDetails> objWalletDetails = new List<PartyWiseWalletDetails>();
            objWalletDetails = objReportManager.GetPartyWiseWalletReport(FromDate, ToDate,PartyCode,ViewType);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated party wallet report", myIP + currentDate);

            var jsonProduct = Json(objWalletDetails, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;           
        }
        [SessionExpire]
        public ActionResult PaymentSummary()
        {
            PaymentSummary objPaymentSummary = new PaymentSummary();
            try
            {
                if (Session["LoginUser"] != null)
                {
                    objPaymentSummary.PaymentMode = objReportManager.GetPaymodeList();
                    string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                    string LoginPartyName = (Session["LoginUser"] as User).PartyName;
                    int LoginGroupId = (Session["LoginUser"] as User).GroupId;
                    objPaymentSummary.PartyName = LoginPartyName;
                    objPaymentSummary.PartyCode = LoginPartyCode;
                    objPaymentSummary.GroupId = LoginGroupId;
                }
            }
            catch (Exception ex)
            {

            }
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "PaymentSummary"))
                return View(objPaymentSummary);
            else
                return RedirectToAction("Dashboard", "Home");
           
        }

        [HttpPost]
        public ActionResult GetPaymentSummaryReport(string FromDate, string ToDate, string PartyCode, string Type)
        {
            List<PaymentSummaryReport> objWalletDetails = new List<PaymentSummaryReport>();
            objWalletDetails = objReportManager.GetPaymentSummaryReport(FromDate, ToDate, PartyCode, Type);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated payment summary report" + Type, myIP + currentDate);
            var jsonProduct = Json(objWalletDetails, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
        }
        [SessionExpire]
        public ActionResult SaleRegister()
        {
            PaymentSummary objPaymentSummary = new PaymentSummary();
            try
            {
                if (Session["LoginUser"] != null)
                {
                    string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                }                
            }
            catch (Exception ex)
            {

            }
            
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "SaleRegister"))
                return View(objPaymentSummary);
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetSaleRegisterReport(string FromDate, string ToDate, string PartyCode)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<SaleRegister> objWalletDetails = new List<SaleRegister>();
            objWalletDetails = objReportManager.GetSaleRegisterReport(FromDate, ToDate, PartyCode);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated Sale Register report", myIP + currentDate);
            var jsonProduct = Json(objWalletDetails, JsonRequestBehavior.AllowGet);            
            return jsonProduct;
        }

        [SessionExpire]
        public ActionResult SalesReturnReport()
        {
            try
            {
                if (Session["LoginUser"] != null)
                {
                    string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                }
            }
            catch (Exception ex)
            {

            }
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "SalesReturnReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetSalesReturnReport(string FromDate, string ToDate, string ProductCode, string CategoryCode, string PartyCode, string PartyType, string Type)
        {
            List<SalesReturnReport> objWalletDetails = new List<SalesReturnReport>();
            objWalletDetails = objReportManager.GetSalesReturnReport(FromDate, ToDate, ProductCode, CategoryCode, PartyCode, PartyType, Type);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated sale return report", myIP + currentDate);
            var jsonProduct = Json(objWalletDetails, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
        }

        [SessionExpire]
        public ActionResult PurchaseReturnReport()
        {
            try
            {
                if (Session["LoginUser"] != null)
                {
                    string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                }
            }
            catch (Exception ex)
            {

            }
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "PurchaseReturnReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetPurchaseReturnReport(string FromDate, string ToDate, string ProductCode, string CategoryCode, string SupplierCode, string Type)
        {
            List<SalesReturnReport> objWalletDetails = new List<SalesReturnReport>();
            objWalletDetails = objReportManager.GetPurchaseReturnReport(FromDate, ToDate, ProductCode, CategoryCode, SupplierCode, Type);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated purchase return report", myIP + currentDate);
            var jsonProduct = Json(objWalletDetails, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
        }

        [SessionExpire]
        public ActionResult PartyTargetReport()
        {
            try
            {
                if (Session["LoginUser"] != null)
                {
                    string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                }

                List<SelectListItem> objCategoryList = new List<SelectListItem>();
                var result = objProductManager.GetCategoryList("Y");
                SubCategoryDetails model = new SubCategoryDetails();
                bool f = true;
                foreach (var item in result)
                {
                    SelectListItem tempobj = new SelectListItem();
                    //SelectListItem tempobj1 = new SelectListItem();
                    tempobj.Text = item.CategoryName;
                    tempobj.Value = item.CategoryId.ToString();
                    if (f == true)
                    {
                        f = false;
                        model.CategoryId = int.Parse(item.CategoryId.ToString());
                        //model.SubCatId = int.Parse(item.ToString());
                    }

                    objCategoryList.Add(tempobj);
                }

                ViewBag.ListCategory = objCategoryList;
            }
            catch (Exception ex)
            {

            }
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "PartyTargetReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult TransProductReport(string FromDate, string ToDate, string ProdCode)
        {
            List<TransferredProduct> objTransList = new List<TransferredProduct>();
            objTransList = objReportManager.TransProductReport(FromDate, ToDate, ProdCode);
            var jsonProduct = Json(objTransList, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;

        }

        [SessionExpire]
        public ActionResult WalletReport()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "WalletReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetWalletHistory(string FromDate, string ToDate, string PartyCode)
        {
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"] || PartyCode == "" || PartyCode.ToUpper() == "ALL")
                PartyCode = CurrentPartyCode;

            List<SalesReport> objSalesReport = new List<SalesReport>();
            objSalesReport = objReportManager.GetWalletHistory(FromDate, ToDate, PartyCode);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated wallet history report.", myIP + currentDate);
            return Json(objSalesReport, JsonRequestBehavior.AllowGet);
        }
        [SessionExpire]
        public ActionResult EarlyRiser()
        {
            var KitIdlist = objReportManager.GetOfferSessList();
            List<SelectListItem> KidIsListObj = new List<SelectListItem>();
            KidIsListObj.Add(new SelectListItem { Text = "--ALL IDs--", Value = "0" });
            foreach (var obj in KitIdlist)
            {
                KidIsListObj.Add(new SelectListItem { Text = obj.KitName, Value = obj.KitId.ToString() });
            }
            ViewBag.OfferSessList = KidIsListObj;
          
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "EarlyRiser"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetOfferReport(int SessID,int OfferUID)
        {
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated offer report.", myIP + currentDate);
            return Json(objReportManager.GetOfferReport(SessID, OfferUID), JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult OfferDetailReport()
        {
            var KitIdlist = objReportManager.GetMonthSessnList();
            List<SelectListItem> KidIsListObj = new List<SelectListItem>();
          //  KidIsListObj.Add(new SelectListItem { Text = "--ALL IDs--", Value = "0" });
            foreach (var obj in KitIdlist)
            {
                KidIsListObj.Add(new SelectListItem { Text = obj.KitName, Value = obj.KitId.ToString() });
            }
            ViewBag.OfferSessList = KidIsListObj;

            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "OfferDetailReport"))
                return View("OfferMemStatus");
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetOfferStatus(int SessID, int OfferUID)
        {
            return Json(objReportManager.GetOfferStatus(SessID, OfferUID), JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult WalletRequestReport()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "WalletRequestReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }
        [SessionExpire]
        public ActionResult DateWiseStockReport()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "DateWiseStockReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetDateWiseStockReport(string CategoryCode, string ProductCode, string PartyCode, string FromDate, string ToDate)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<StockReportModel> objStockReportModel = new List<StockReportModel>();
            objStockReportModel = objReportManager.GetDateWiseStockReport(CategoryCode, ProductCode, PartyCode, FromDate, ToDate);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated datewise stock report.", myIP + currentDate);
            return Json(objStockReportModel, JsonRequestBehavior.AllowGet);
        }
        [SessionExpire]
        public ActionResult DailyStockReport()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "DailyStockReport"))
                return View();
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetDailyStockReport(string CategoryCode, string ProductCode, string PartyCode, string FromDate, string ToDate)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<StockReportModel> objStockReportModel = new List<StockReportModel>();
            objStockReportModel = objReportManager.GetDailyStockReport(CategoryCode, ProductCode, PartyCode, FromDate, ToDate);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated daily stock report.", myIP + currentDate);
            return Json(objStockReportModel, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult PrevMonthOfferDetail()
        {
            if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "PrevMonthOfferDetail"))
                return View("PrevOfferReport");
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetPrevofferReport(string PartyCode, int OfferUID)
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;
            if (PartyCode == null)
                PartyCode = "";
            if (PartyCode==""|| CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<SalesReport> objOfferReport = new List<SalesReport>();
            objOfferReport = objReportManager.GetPrevOfferReport(PartyCode,  OfferUID);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated pervious month offer bill report", myIP + currentDate);
            return Json(objOfferReport, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult LogReport()
        {
            //if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "LogReport"))
            SalesReport objsalesreport = new Entity.Common.SalesReport();
            objsalesreport.UserList = objUserManager.GetUserList(true);
            return View("LogReport", objsalesreport);
            //else
            //    return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult GetLogReport(string FromDate, string ToDate, string User)
        {
            
            List<LogInfo> objOfferReport = new List<LogInfo>();
            objOfferReport = objReportManager.GetLogReport(FromDate, ToDate, User);
            
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Generated log report", myIP + currentDate);

            return Json(objOfferReport, JsonRequestBehavior.AllowGet);
        }
    }
}