using InventoryManagement.App_Start;
using InventoryManagement.Business;
using InventoryManagement.Common;
using InventoryManagement.Entity.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using NPOI.SS.UserModel;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        // GET: Transaction

        TransactionManager objTransacManager = new TransactionManager();
        ProductManager objProductManager = new ProductManager();
        LogManager objLogManager = new LogManager();
        RegistrationManager objRegistrationManager = new RegistrationManager();
        [HttpPost]
        public ActionResult payPaytm(PaytmGateway objpay)
        {
            objpay.Mobile = (Session["LoginUser"] as User).MobileNo;
            objpay.email = (Session["LoginUser"] as User).Email;
            objpay.regid = (Session["LoginUser"] as User).PartyCode;
            objpay.action = "INSERT";
            var result = objTransacManager.CreditRequestOnlineInsert(objpay);
            Response.Redirect(@"~/PayWithPaytm.aspx?Mobile=" + objpay.Mobile + "&UniQID=" + objpay.UniQID + "&shpcharge=" + objpay.shpcharge + "&ORDER_ID=" + objpay.ORDER_ID + "&regid=" + objpay.regid + "&scmemtype=" + objpay.scmemtype + "&amount=" + objpay.amount + "&coupon=" + objpay.coupon + "&email=" + objpay.email);
            return new EmptyResult();
            // return View("Test.aspx?Mobile=" + objpay.Mobile);
        }

        [SessionExpire]
        public ActionResult DistributorBill()
        {
            DistributorBillModel objDistributorModel = new DistributorBillModel();
            try
            {
                objDistributorModel.objCustomer = new CustomerDetail();
                objDistributorModel.objProduct = new ProductModel();

                List<SelectListItem> objBankList = new List<SelectListItem>();
                var result = objTransacManager.GetBankList();
                objDistributorModel.objProduct.PayDetails = new PayDetails();
                foreach (var obj in result)
                {
                    if (obj.BankCode == 0)
                    {
                        objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString(), Selected = true });
                        objDistributorModel.objProduct.PayDetails.BDBankName = obj.BankCode.ToString();
                    }
                    else
                    {
                        objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString() });
                    }
                }
                ViewBag.BankNames = objBankList;
                List<SelectListItem> CardTypes = new List<SelectListItem>();
                CardTypes.Add(new SelectListItem { Text = "Credit Card", Value = "CC" });
                CardTypes.Add(new SelectListItem { Text = "Debit Card", Value = "DB" });
                ViewBag.CardTypes = CardTypes;

                objDistributorModel.objProduct.PayDetails.CardType = "CC";

                //List<SelectListItem> objListCustomerTypes = new List<SelectListItem>();
                //objListCustomerTypes.Add(new SelectListItem { Text = "New", Value = "New" });
                //objListCustomerTypes.Add(new SelectListItem { Text = "Existing", Value = "Existing" });
                //ViewBag.CustomerType = objListCustomerTypes;

                //ViewBag.ConfigDetails = objTransacManager.GetConfigDetails();
                //objDistributorModel.objConfigDetails = objTransacManager.GetConfigDetails();

                objDistributorModel.objCustomer.CustomerType = "New";
                var KitIdlist = objTransacManager.GetKitIdList();
                List<SelectListItem> KidIsListObj = new List<SelectListItem>();
                KidIsListObj.Add(new SelectListItem { Text = "--Select Kit--", Value = "0" });
                foreach (var obj in KitIdlist)
                {
                    KidIsListObj.Add(new SelectListItem { Text = obj.KitName, Value = obj.KitId.ToString() });
                }
                ViewBag.objKitList = KidIsListObj;

                List<SelectListItem> Offer = new List<SelectListItem>();
                Offer.Add(new SelectListItem { Text = "Choose Offer", Value = "0" });
                ViewBag.OfferList = Offer;


                objDistributorModel.objCustomer.KitId = 0;
                List<StateModel> objStateMdl = new List<StateModel>();
                objStateMdl = objRegistrationManager.GetStateList();
                List<SelectListItem> StateList = new List<SelectListItem>();
                foreach (var obj in objStateMdl)
                {
                    if (obj.StateCode != 0)
                    {
                        StateList.Add(new SelectListItem
                        {
                            Text = obj.StateName,
                            Value = obj.StateCode.ToString()
                        });
                    }
                }
                ViewBag.StateList = StateList;
             //   objModel.StateCode = objModel.StateList.Where(r => r.IsCompanyState == true).Select(m => m.StateCode).FirstOrDefault();

                //objDistributorModel.objCustomer.IsRegisteredCustomer = true;
                //objDistributorModel.objCustomer.ReferenceIdNo = InventorySession.LoginUser.PartyCode;
                //objDistributorModel.objCustomer.ReferenceName = InventorySession.LoginUser.PartyName;
                InventorySession.StoredDistributorValues = null;

            }
            catch (Exception ex)
            {
                LogError(ex);
                Console.Write("Exception:", ex.Message);
            }                       

            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "DistributorBill");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objDistributorModel);
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }
        public ActionResult GetKitDescription(decimal KitId)
        {
            return Json(objTransacManager.GetKitDescription(KitId), JsonRequestBehavior.AllowGet);
        }
        public void LogError(Exception ex)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", "DistributorBill");
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = Server.MapPath("~/ErrorLog/ErrorLog.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
        [SessionExpire]
        public ActionResult PartyBill()
        {
            DistributorBillModel objDistributorModel = new DistributorBillModel();
            objDistributorModel.objCustomer = new CustomerDetail();
            objDistributorModel.objProduct = new ProductModel();
            List<SelectListItem> objBankList = new List<SelectListItem>();
            var result = objTransacManager.GetBankList();
            objDistributorModel.objProduct.PayDetails = new PayDetails();
            foreach (var obj in result)
            {
                if (obj.BankCode == 0)
                {
                    objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString(), Selected = true });
                    objDistributorModel.objProduct.PayDetails.BDBankName = obj.BankCode.ToString();
                }
                else
                {
                    objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString() });
                }
            }
            ViewBag.BankNames = objBankList;
            List<SelectListItem> CardTypes = new List<SelectListItem>();
            CardTypes.Add(new SelectListItem { Text = "Credit Card", Value = "CC" });
            CardTypes.Add(new SelectListItem { Text = "Debit Card", Value = "DB" });
            ViewBag.CardTypes = CardTypes;

            objDistributorModel.objProduct.PayDetails.CardType = "CC";

            //List<SelectListItem> objListCustomerTypes = new List<SelectListItem>();
            //objListCustomerTypes.Add(new SelectListItem { Text = "New", Value = "New" });
            //objListCustomerTypes.Add(new SelectListItem { Text = "Existing", Value = "Existing" });
            //ViewBag.CustomerType = objListCustomerTypes;

            //ViewBag.ConfigDetails = objTransacManager.GetConfigDetails();
            //objDistributorModel.objConfigDetails = objTransacManager.GetConfigDetails();

            objDistributorModel.objCustomer.CustomerType = "New";
            //objDistributorModel.objCustomer.IsRegisteredCustomer = true;
            //objDistributorModel.objCustomer.ReferenceIdNo = InventorySession.LoginUser.PartyCode;
            //objDistributorModel.objCustomer.ReferenceName = InventorySession.LoginUser.PartyName;
            
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "PartyBill");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objDistributorModel);
            }
            else
                return RedirectToAction("Dashboard", "Home");

        }

        [SessionExpire]
        public ActionResult CustomerBill()
        {
            DistributorBillModel objDistributorModel = new DistributorBillModel();
            objDistributorModel.objCustomer = new CustomerDetail();
            objDistributorModel.objProduct = new ProductModel();
            List<SelectListItem> objBankList = new List<SelectListItem>();
            var result = objTransacManager.GetBankList();
            objDistributorModel.objProduct.PayDetails = new PayDetails();
            foreach (var obj in result)
            {
                if (obj.BankCode == 0)
                {
                    objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString(), Selected = true });
                    objDistributorModel.objProduct.PayDetails.BDBankName = obj.BankCode.ToString();
                }
                else
                {
                    objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString() });
                }
            }
            ViewBag.BankNames = objBankList;
            List<SelectListItem> CardTypes = new List<SelectListItem>();
            CardTypes.Add(new SelectListItem { Text = "Credit Card", Value = "CC" });
            CardTypes.Add(new SelectListItem { Text = "Debit Card", Value = "DB" });
            ViewBag.CardTypes = CardTypes;

            objDistributorModel.objProduct.PayDetails.CardType = "CC";



            objDistributorModel.objCustomer.CustomerType = "New";
            objDistributorModel.objCustomer.IsBillOnMrp = true;

            
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "CustomerBill");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objDistributorModel);
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult SaveDistributorBill(DistributorBillModel objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            if (objModel != null)
            {
                objModel.objListProduct = new List<ProductModel>();
                if (!string.IsNullOrEmpty(objModel.objProductListStr))
                {
                    var objects = JArray.Parse(objModel.objProductListStr); // parse as array  
                    foreach (JObject root in objects)
                    {
                        ProductModel objTemp = new ProductModel();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            // var appName = app.Key;
                            //    var ProductGrid = [{"AvailStock":"", "SNo": "", "Code": "", "ProductName": "", "MRP": "", "DP": "", "Rate": "","BatchNo":"", "Barcode": "", "RP": "", "BV": "", "CV": "", "PV": "", "Qty": "", "RPValue": "", "BVValue": "", "CVValue": "", "PVValue": "", "CommsnPer": "", "CommsnAmt": "", "DiscPer": "", "DiscAmt": "", "Amount": "", "TaxType": "", "TaxPer": "", "TaxAmt": "", "TotalAmount": ""}];
                            if (app.Key == "Code")
                            {
                                objTemp.ProdCode = (int)app.Value;
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = (string)app.Value;
                            }
                            else if (app.Key == "Rate")
                            {
                                objTemp.Rate = (decimal)app.Value;
                            }
                            else if (app.Key == "Barcode")
                            {
                                objTemp.Barcode = app.Value.ToString();
                            }
                            else if (app.Key == "BatchNo")
                            {
                                objTemp.BatchNo = app.Value.ToString();
                            }
                            else if (app.Key == "MRP")
                            {
                                objTemp.MRP = (decimal?)app.Value;
                            }
                            else if (app.Key == "Qty")
                            {
                                objTemp.Quantity = (decimal)app.Value;
                            }
                            else if (app.Key == "FreeQty")
                            {
                                objTemp.FreeQty = (decimal)app.Value;
                            }
                            else if (app.Key == "PV")
                            {
                                objTemp.PV = (decimal)app.Value;
                            }
                            else if (app.Key == "CV")
                            {
                                objTemp.CV = (decimal)app.Value;
                            }
                            else if (app.Key == "BV")
                            {
                                objTemp.BV = (decimal)app.Value;
                            }
                            else if (app.Key == "RP")
                            {
                                objTemp.RP = (decimal)app.Value;
                            }
                            else if (app.Key == "DP")
                            {
                                objTemp.DP = (decimal)app.Value;
                            }
                            else if (app.Key == "CVValue")
                            {
                                objTemp.CVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.PVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "BVValue")
                            {
                                objTemp.BVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscPer")
                            {
                                objTemp.DiscPer = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscAmt")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxAmt")
                            {
                                objTemp.TaxAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxPer")
                            {
                                objTemp.TaxPer = (decimal)app.Value;
                            }
                            else if (app.Key == "Amount")
                            {
                                objTemp.Amount = (decimal)app.Value;
                            }
                            else if (app.Key == "TotalAmount")
                            {
                                objTemp.TotalAmount = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxType")
                            {
                                objTemp.TaxType = (string)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;

                            }
                            else if (app.Key == "AvailStock")
                            {
                                objTemp.StockAvailable = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnPer")
                            {
                                objTemp.CommissionPer = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnAmt")
                            {
                                objTemp.CommissionAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "RPValue")
                            {
                                objTemp.RPValue = (decimal)app.Value;
                            }
                            else if (app.Key == "ProductTye")
                            {
                                objTemp.ProductTye = (string)app.Value;
                            }
                            else if (app.Key == "Weight")
                            {
                                objTemp.Weight = (decimal)app.Value;
                            }
                            else if (app.Key == "WeightVal")
                            {
                                objTemp.WeightVal = (decimal)app.Value;
                            }
                        }
                        objModel.objListProduct.Add(objTemp);
                    }
                    objModel.objCustomer.UserDetails = Session["LoginUser"] as User;
                    // Retrive the Name of HOST
                    string hostName = Dns.GetHostName();
                    // Get the IP  
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
                    objModel.objProduct.UID = myIP + currentDate;
                    objResponse = objTransacManager.SaveDistributorBill(objModel);
                    if (objResponse.ResponseStatus == "OK")
                   {
                        //Added log                        
                        objLogManager.SaveLog(Session["LoginUser"] as User, "Created "+ objModel.BillType + " bill - " + objResponse.ResponseDetailsToPrint.BillNo, myIP + currentDate);
                    }
                }
            }
            else
            {
                objResponse.ResponseMessage = "Something went wrong!";
                objResponse.ResponseStatus = "FAILED";
            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult AddLessStockJV(string JvType)
        {
            StockJv objModel = new StockJv();
            objModel.objListGroup = new List<GroupModel>();
            objModel.objListGroup = objTransacManager.GetGroupList();
            objModel.GroupId = objModel.objListGroup[0].GroupId;
            //objModel.objPartyList = new List<PartyModel>();
            //string LoginPartyCode = InventorySession.LoginUser.PartyCode;
            //decimal StateCode = InventorySession.LoginUser.StateCode;
            //objModel.objPartyList = objTransacManager.GetAllParty(LoginPartyCode, StateCode);
            if (!string.IsNullOrEmpty(JvType))
            {
                if (JvType == "Add")
                {
                    objModel.isAdd = true;
                }
                else
                {
                    objModel.isAdd = false;
                }
            }


            

            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "AddLessStockJV");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objModel);
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult SaveStockJv(StockJv objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            if (objModel != null)
            {
                objModel.objListProduct = new List<ProductModel>();
                if (!string.IsNullOrEmpty(objModel.objProductListStr))
                {
                    var objects = JArray.Parse(objModel.objProductListStr); // parse as array  
                    foreach (JObject root in objects)
                    {
                        ProductModel objTemp = new ProductModel();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            // var appName = app.Key;
                            //    var ProductGrid = [{"AvailStock":"", "SNo": "", "Code": "", "ProductName": "", "MRP": "", "DP": "", "Rate": "","BatchNo":"", "Barcode": "", "RP": "", "BV": "", "CV": "", "PV": "", "Qty": "", "RPValue": "", "BVValue": "", "CVValue": "", "PVValue": "", "CommsnPer": "", "CommsnAmt": "", "DiscPer": "", "DiscAmt": "", "Amount": "", "TaxType": "", "TaxPer": "", "TaxAmt": "", "TotalAmount": ""}];
                            if (app.Key == "Code")
                            {
                                objTemp.ProdCode = (int)app.Value;
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = (string)app.Value;
                            }

                            else if (app.Key == "Barcode")
                            {
                                objTemp.Barcode = app.Value.ToString();
                            }
                            else if (app.Key == "BatchNo")
                            {
                                objTemp.BatchNo = app.Value.ToString();
                            }

                            else if (app.Key == "Qty")
                            {
                                objTemp.Quantity = (decimal)app.Value;
                            }

                        }
                        objModel.objListProduct.Add(objTemp);
                    }
                    objModel.LoginUser = Session["LoginUser"] as User;


                    objResponse = objTransacManager.SaveStockJv(objModel);

                    if (objResponse.ResponseStatus == "OK")
                    {
                        //Added log
                        string hostName = Dns.GetHostName();
                        string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                        string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        objLogManager.SaveLog(Session["LoginUser"] as User, objModel.isAdd?"Add ":"Less " + " stock - ", myIP + currentDate);
                    }

                }

            }
            else
            {
                objResponse.ResponseMessage = "Something went wrong!";
                objResponse.ResponseStatus = "FAILED";
            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCustInfo(string IdNo)
        {
            CustomerDetail model = new CustomerDetail();
            model = objTransacManager.GetCustInfo(IdNo);
            return Json(model, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult GetProductInfo(string SearchType, string data, bool isCForm, string BillType, bool IsBillOnMrp)
        {
            List<ProductModel> model = new List<ProductModel>();
            model = objTransacManager.GetproductInfo(SearchType, data, isCForm, BillType, (Session["LoginUser"] as User).StateCode, (Session["LoginUser"] as User).PartyCode, IsBillOnMrp);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllProductNames()
        {
            string CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;
            bool RestrictedproductsAlso = false;
            if (CurrentPartyCode == System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                RestrictedproductsAlso = true;

            List<string> model = objTransacManager.GetAutocompleteProductNames(RestrictedproductsAlso);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductNamesOnly()
        {
            List<string> model = objTransacManager.GetAutocompProductsOnly();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

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
            objparty = objTransacManager.GetAllParty(LoginPartyCode, LoginStateCode);
            return Json(objparty, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllSupplier()
        {
            List<PartyModel> objparty = new List<PartyModel>();
            string LoginPartyCode = "";
            decimal LoginStateCode = 0;
            if (Session["LoginUser"] != null)
            {
                LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                LoginStateCode = (Session["LoginUser"] as User).StateCode;
            }
            objparty = objTransacManager.GetAllSupplierList(LoginPartyCode, LoginStateCode);
            return Json(objparty, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult InvoicePrint(string Pm)
        {
            DistributorBillModel model = new DistributorBillModel();
            if (Session["LoginUser"] != null)
            {
                var base64DecodedBytes = System.Convert.FromBase64String(Pm);
                string BillNoValue = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);

                //if (InventorySession.StoredDistributorValues != null)
                //{
                //    model = InventorySession.StoredDistributorValues;
                //}
                string CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;
                model = objTransacManager.getInvoice(BillNoValue, CurrentPartyCode,"F");
                if (model == null)
                {
                    model = new DistributorBillModel();
                }

                
                    //Added log
                    string hostName = Dns.GetHostName();
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    objLogManager.SaveLog(Session["LoginUser"] as User, "Invoice Print for Bill - " + model.BillNo, myIP + currentDate);
                
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SendOTP(string MobileNo, string TotalBillAmount)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objResponse = await Task.Run(() => (objTransacManager.SendOTP(MobileNo, TotalBillAmount)));
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult PurchaseInvoice()
        {           
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "PurchaseInvoice");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult SavePurchaseInvoice(DistributorBillModel objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            if (objModel != null)
            {
                objModel.objListProduct = new List<ProductModel>();
                try
                {
                    if (!string.IsNullOrEmpty(objModel.objProductListStr))
                    {
                        var objects = JArray.Parse(objModel.objProductListStr); // parse as array  
                        foreach (JObject root in objects)
                        {
                            ProductModel objTemp = new ProductModel();
                            foreach (KeyValuePair<String, JToken> app in root)
                            {
                                // var appName = app.Key;
                                //    var ProductGrid = [{"AvailStock":"", "SNo": "", "Code": "", "ProductName": "", "MRP": "", "DP": "", "Rate": "","BatchNo":"", "Barcode": "", "RP": "", "BV": "", "CV": "", "PV": "", "Qty": "", "RPValue": "", "BVValue": "", "CVValue": "", "PVValue": "", "CommsnPer": "", "CommsnAmt": "", "DiscPer": "", "DiscAmt": "", "Amount": "", "TaxType": "", "TaxPer": "", "TaxAmt": "", "TotalAmount": ""}];
                                if (app.Key == "Code")
                                {
                                    objTemp.ProdCode = (int)app.Value;
                                }
                                else if (app.Key == "ProductName")
                                {
                                    objTemp.ProductName = (string)app.Value;
                                }
                                else if (app.Key == "Rate")
                                {
                                    objTemp.Rate = (decimal)app.Value;
                                }
                                else if (app.Key == "Barcode")
                                {
                                    objTemp.Barcode = app.Value.ToString();
                                }
                                else if (app.Key == "BatchNo")
                                {
                                    objTemp.BatchNo = app.Value.ToString();
                                }
                                else if (app.Key == "MRP")
                                {
                                    objTemp.MRP = (decimal?)app.Value;
                                }
                                else if (app.Key == "Qty")
                                {
                                    objTemp.Quantity = (decimal)app.Value;
                                }
                                else if (app.Key == "PV")
                                {
                                    objTemp.PV = (decimal)app.Value;
                                }
                                else if (app.Key == "CV")
                                {
                                    objTemp.CV = (decimal)app.Value;
                                }
                                else if (app.Key == "BV")
                                {
                                    objTemp.BV = (decimal)app.Value;
                                }
                                else if (app.Key == "RP")
                                {
                                    objTemp.RP = (decimal)app.Value;
                                }
                                else if (app.Key == "DP")
                                {
                                    objTemp.DP = (decimal)app.Value;
                                }
                                else if (app.Key == "CVValue")
                                {
                                    objTemp.CVValue = (decimal)app.Value;
                                }
                                else if (app.Key == "PVValue")
                                {
                                    objTemp.PVValue = (decimal)app.Value;
                                }
                                else if (app.Key == "BVValue")
                                {
                                    objTemp.BVValue = (decimal)app.Value;
                                }
                                else if (app.Key == "DiscPer")
                                {
                                    objTemp.DiscPer = (decimal)app.Value;
                                }
                                else if (app.Key == "DiscAmt")
                                {
                                    objTemp.DiscAmt = (decimal)app.Value;
                                }
                                else if (app.Key == "TaxAmt")
                                {
                                    objTemp.TaxAmt = (decimal)app.Value;
                                }
                                else if (app.Key == "TaxPer")
                                {
                                    objTemp.TaxPer = (decimal)app.Value;
                                }
                                else if (app.Key == "Amount")
                                {
                                    objTemp.Amount = (decimal)app.Value;
                                }
                                else if (app.Key == "TotalAmount")
                                {
                                    objTemp.TotalAmount = (decimal)app.Value;
                                }
                                else if (app.Key == "TaxType")
                                {
                                    objTemp.TaxType = (string)app.Value;
                                }
                                else if (app.Key == "PVValue")
                                {
                                    objTemp.DiscAmt = (decimal)app.Value;

                                }
                                else if (app.Key == "AvailStock")
                                {
                                    objTemp.StockAvailable = (decimal)app.Value;
                                }
                                else if (app.Key == "CommsnPer")
                                {
                                    objTemp.CommissionPer = (decimal)app.Value;
                                }
                                else if (app.Key == "CommsnAmt")
                                {
                                    objTemp.CommissionAmt = (decimal)app.Value;
                                }
                                else if (app.Key == "RPValue")
                                {
                                    objTemp.RPValue = (decimal)app.Value;
                                }

                            }
                            objModel.objListProduct.Add(objTemp);
                        }
                        objModel.objCustomer.UserDetails = Session["LoginUser"] as User;
                        // Retrive the Name of HOST
                        string hostName = Dns.GetHostName();
                        // Get the IP  
                        string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                        string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
                        objModel.objProduct.UID = myIP + currentDate;
                        objResponse = objTransacManager.SavePurchaseInvoice(objModel);

                        //Added log
                        
                        objLogManager.SaveLog(Session["LoginUser"] as User, "Created Purchase Invoice - " + objModel.BillNo, myIP + currentDate);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult OrderCreation()
        {
            PartyOrderModel objPartyModel = new PartyOrderModel();

            objPartyModel.objProduct = new ProductModel();
            List<SelectListItem> objBankList = new List<SelectListItem>();
            var result = objTransacManager.GetBankList();
            objPartyModel.objProduct.PayDetails = new PayDetails();
            foreach (var obj in result)
            {
                if (obj.BankCode == 0)
                {
                    objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString(), Selected = true });
                    objPartyModel.objProduct.PayDetails.BDBankName = obj.BankCode.ToString();
                }
                else
                {
                    objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString() });
                }
            }
            ViewBag.BankNames = objBankList;
            List<SelectListItem> CardTypes = new List<SelectListItem>();
            CardTypes.Add(new SelectListItem { Text = "Credit Card", Value = "Credit Card" });
            CardTypes.Add(new SelectListItem { Text = "Debit Card", Value = "Debit Card" });
            ViewBag.CardTypes = CardTypes;

            objPartyModel.objProduct.PayDetails.CardType = "Credit Card";
            objPartyModel.objProduct.PayDetails.PayMode = "BD";
            string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
            objPartyModel.OrderNo = objTransacManager.GetOrderNo(LoginPartyCode);
            objPartyModel.PartyWalletBalance = objTransacManager.GetPartyWalletBalance(LoginPartyCode);
            objPartyModel.OrderBy = LoginPartyCode;
            objPartyModel.OrderTo = (Session["LoginUser"] as User).ParentPartyCode;


            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "OrderCreation");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objPartyModel);
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult SavePartyOrderDetails(PartyOrderModel objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            if (objModel != null)
            {
                objModel.objListProduct = new List<ProductModel>();
                if (!string.IsNullOrEmpty(objModel.objProductListStr))
                {
                    var objects = JArray.Parse(objModel.objProductListStr); // parse as array  
                    foreach (JObject root in objects)
                    {
                        ProductModel objTemp = new ProductModel();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            // var appName = app.Key;
                            //    var ProductGrid = [{"AvailStock":"", "SNo": "", "Code": "", "ProductName": "", "MRP": "", "DP": "", "Rate": "","BatchNo":"", "Barcode": "", "RP": "", "BV": "", "CV": "", "PV": "", "Qty": "", "RPValue": "", "BVValue": "", "CVValue": "", "PVValue": "", "CommsnPer": "", "CommsnAmt": "", "DiscPer": "", "DiscAmt": "", "Amount": "", "TaxType": "", "TaxPer": "", "TaxAmt": "", "TotalAmount": ""}];
                            if (app.Key == "Code")
                            {
                                objTemp.ProdCode = (int)app.Value;
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = (string)app.Value;
                            }
                            else if (app.Key == "Rate")
                            {
                                objTemp.Rate = (decimal)app.Value;
                            }
                            else if (app.Key == "Barcode")
                            {
                                objTemp.Barcode = app.Value.ToString();
                            }
                            else if (app.Key == "BatchNo")
                            {
                                objTemp.BatchNo = app.Value.ToString();
                            }
                            else if (app.Key == "MRP")
                            {
                                objTemp.MRP = (decimal?)app.Value;
                            }
                            else if (app.Key == "Qty")
                            {
                                objTemp.Quantity = (decimal)app.Value;
                            }
                            else if (app.Key == "PV")
                            {
                                objTemp.PV = (decimal)app.Value;
                            }
                            else if (app.Key == "CV")
                            {
                                objTemp.CV = (decimal)app.Value;
                            }
                            else if (app.Key == "BV")
                            {
                                objTemp.BV = (decimal)app.Value;
                            }
                            else if (app.Key == "RP")
                            {
                                objTemp.RP = (decimal)app.Value;
                            }
                            else if (app.Key == "DP")
                            {
                                objTemp.DP = (decimal)app.Value;
                            }
                            else if (app.Key == "CVValue")
                            {
                                objTemp.CVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.PVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "BVValue")
                            {
                                objTemp.BVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscPer")
                            {
                                objTemp.DiscPer = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscAmt")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxAmt")
                            {
                                objTemp.TaxAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxPer")
                            {
                                objTemp.TaxPer = (decimal)app.Value;
                            }
                            else if (app.Key == "Amount")
                            {
                                objTemp.Amount = (decimal)app.Value;
                            }
                            else if (app.Key == "TotalAmount")
                            {
                                objTemp.TotalAmount = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxType")
                            {
                                objTemp.TaxType = (string)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;

                            }
                            else if (app.Key == "AvailStock")
                            {
                                objTemp.StockAvailable = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnPer")
                            {
                                objTemp.CommissionPer = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnAmt")
                            {
                                objTemp.CommissionAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "RPValue")
                            {
                                objTemp.RPValue = (decimal)app.Value;
                            }

                        }
                        objModel.objListProduct.Add(objTemp);
                    }
                    objModel.LoginUser = Session["LoginUser"] as User;
                    // Retrive the Name of HOST
                    string hostName = Dns.GetHostName();
                    // Get the IP  
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmm");
                    objModel.objProduct.UID = "";
                    objResponse = objTransacManager.SavePartyOrderDetails(objModel);

                    //Added log                    
                    objLogManager.SaveLog(Session["LoginUser"] as User, "Placed order number- "+ objModel.OrderNo, myIP + currentDate);

                }
            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult PendingOrder()
        {            

            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "PendingOrder");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [SessionExpire]
        public ActionResult DispatchOrder()
        {           
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "DispatchOrder");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }
        public ActionResult GetOrderList(string FromDate, string ToDate, string PartyCode, string ViewType, string IdNo, string OrderNo, string DispMode, bool notOfferOrder)
        {
            string CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;
            string IsAdmin = (Session["LoginUser"] as User).IsAdmin;
            //if (IsAdmin != "Y")
            if (OrderNo != "" && OrderNo != "0" && OrderNo.ToUpper() != "ALL")
            {
                FromDate = "All"; ToDate = "All"; PartyCode = "0"; IdNo = "0";
            }
            else
            {
                if (CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                    PartyCode = CurrentPartyCode;
            }
            List<DisptachOrderModel> objDispatchOrderList = new List<DisptachOrderModel>();
            objDispatchOrderList = objTransacManager.GetDispatchOrderList(FromDate, ToDate, PartyCode, ViewType, IdNo, OrderNo, DispMode,notOfferOrder);
            return Json(objDispatchOrderList, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult DispatchProduct()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "DispatchProduct");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }
       

        public ActionResult RejectOrder(string OrderNo, string RejectReason)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objResponse = objTransacManager.RejectOrder(OrderNo, RejectReason, (Session["LoginUser"] as User).UserId);
            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Rejected Order No - " + OrderNo, myIP + currentDate);
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrderProduct(string OrderNo)
        {
            List<ProductModel> objModel = new List<ProductModel>();
            objModel = objTransacManager.GetOrderProduct(OrderNo, (Session["LoginUser"] as User).PartyCode);
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveDispatchOrderDetails(DisptachOrderModel objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            List<DisptachOrderModel> objDispatchList = new List<DisptachOrderModel>();
            try
            {
                if (objModel != null)
                {

                    if (!string.IsNullOrEmpty(objModel.OrderList))
                    {
                        var objects = JArray.Parse(objModel.OrderList); // parse as array  
                        foreach (JObject root in objects)
                        {
                            DisptachOrderModel objTemp = new DisptachOrderModel();
                            foreach (KeyValuePair<String, JToken> app in root)
                            {
                                // var appName = app.Key;
                                //    var ProductGrid = [{"AvailStock":"", "SNo": "", "Code": "", "ProductName": "", "MRP": "", "DP": "", "Rate": "","BatchNo":"", "Barcode": "", "RP": "", "BV": "", "CV": "", "PV": "", "Qty": "", "RPValue": "", "BVValue": "", "CVValue": "", "PVValue": "", "CommsnPer": "", "CommsnAmt": "", "DiscPer": "", "DiscAmt": "", "Amount": "", "TaxType": "", "TaxPer": "", "TaxAmt": "", "TotalAmount": ""}];
                                if (app.Key == "OrderNo")
                                {
                                    objTemp.OrderNo = (int)app.Value;
                                }
                                else if (app.Key == "SoldBy")
                                {
                                    objTemp.SoldBy = (Session["LoginUser"] as User).PartyCode;
                                }
                                else if (app.Key == "IsDispatched")
                                {
                                    objTemp.IsDispatched = (bool)app.Value;
                                }


                            }
                            objDispatchList.Add(objTemp);
                        }

                        objResponse = objTransacManager.SaveDispatchOrderdetails(objDispatchList);

                        if (objResponse.ResponseStatus == "OK")
                        {
                            //Added log
                            string hostName = Dns.GetHostName();
                            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            objLogManager.SaveLog(Session["LoginUser"] as User, "Dispatched following orders - " + string.Join(",",objDispatchList.Select(r => r.OrderNo).ToArray()), myIP + currentDate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RejectFranchiseOrder(string OrderNo, string RejectReason)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objResponse = objTransacManager.RejectFranchiseOrder(OrderNo, RejectReason, (Session["LoginUser"] as User).UserId);
            if (objResponse.ResponseStatus == "OK")
            {
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, "Reject Franchise Order - " + OrderNo, myIP + currentDate);
            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetOrderDetails(string OrderBy, string OrderTo, string Status)
        {
            string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
            List<PartyOrderModel> objOrderList = objTransacManager.GetOrderList(OrderBy, OrderTo, Status);
            return Json(objOrderList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetOrderProductDetails(string OrderNo, string OrderBy)
        {
            List<ProductModel> objOrderList = objTransacManager.GetOrderProductList(OrderNo, OrderBy);
            return Json(objOrderList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDistOrderProductList(string OrderNo)
        {
            List<ProductModel> objOrderList = objTransacManager.GetDistOrderProductList(OrderNo);
            return Json(objOrderList, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult PayviaPayTm()
        //{
        //    String merchantKey = “merchantKey value” ;
        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    parameters.Add("MID", "mid value");
        //    parameters.Add("CHANNEL_ID", "channel id value");
        //    parameters.Add("INDUSTRY_TYPE_ID", "industry value");
        //    parameters.Add("WEBSITE", "website value");
        //    parameters.Add("EMAIL", "email value");
        //    parameters.Add("MOBILE_NO", "mobile value");
        //    parameters.Add("CUST_ID", "cust id");
        //    parameters.Add("ORDER_ID", "order id");
        //    parameters.Add("TXN_AMOUNT", "amount");
        //    parameters.Add("CALLBACK_URL", "url"); //This parameter is not mandatory. Use this to pass the callback url dynamically.

        //    string checksum = CheckSum.generateCheckSum(merchantKey, parameters);


        //    string paytmURL = "https://securegw-stage.paytm.in/theia/processTransaction?orderid=" + orderid;
        //}

        [HttpPost]
        public ActionResult SaveDispatchOrder(PartyOrderModel objPartyOrderModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            if (objPartyOrderModel != null)
            {
                objPartyOrderModel.objListProduct = new List<ProductModel>();
                if (!string.IsNullOrEmpty(objPartyOrderModel.objProductListStr))
                {
                    var objects = JArray.Parse(objPartyOrderModel.objProductListStr); // parse as array  
                    foreach (JObject root in objects)
                    {
                        ProductModel objTemp = new ProductModel();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            // var appName = app.Key;
                            //    var ProductGrid = [{"AvailStock":"", "SNo": "", "Code": "", "ProductName": "", "MRP": "", "DP": "", "Rate": "","BatchNo":"", "Barcode": "", "RP": "", "BV": "", "CV": "", "PV": "", "Qty": "", "RPValue": "", "BVValue": "", "CVValue": "", "PVValue": "", "CommsnPer": "", "CommsnAmt": "", "DiscPer": "", "DiscAmt": "", "Amount": "", "TaxType": "", "TaxPer": "", "TaxAmt": "", "TotalAmount": ""}];
                            if (app.Key == "Code")
                            {
                                objTemp.ProdCode = (int)app.Value;
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = (string)app.Value;
                            }
                            else if (app.Key == "Rate")
                            {
                                objTemp.Rate = (decimal)app.Value;
                            }
                            else if (app.Key == "Barcode")
                            {
                                objTemp.Barcode = app.Value.ToString();
                            }
                            else if (app.Key == "BatchNo")
                            {
                                objTemp.BatchNo = app.Value.ToString();
                            }
                            else if (app.Key == "MRP")
                            {
                                objTemp.MRP = (decimal?)app.Value;
                            }
                            else if (app.Key == "Qty")
                            {
                                objTemp.Quantity = (decimal)app.Value;
                            }
                            else if (app.Key == "PV")
                            {
                                objTemp.PV = (decimal)app.Value;
                            }
                            else if (app.Key == "CV")
                            {
                                objTemp.CV = (decimal)app.Value;
                            }
                            else if (app.Key == "BV")
                            {
                                objTemp.BV = (decimal)app.Value;
                            }
                            else if (app.Key == "RP")
                            {
                                objTemp.RP = (decimal)app.Value;
                            }
                            else if (app.Key == "DP")
                            {
                                objTemp.DP = (decimal)app.Value;
                            }
                            else if (app.Key == "CVValue")
                            {
                                objTemp.CVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.PVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "BVValue")
                            {
                                objTemp.BVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscPer")
                            {
                                objTemp.DiscPer = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscAmt")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxAmt")
                            {
                                objTemp.TaxAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxPer")
                            {
                                objTemp.TaxPer = (decimal)app.Value;
                            }
                            else if (app.Key == "Amount")
                            {
                                objTemp.Amount = (decimal)app.Value;
                            }
                            else if (app.Key == "TotalAmount")
                            {
                                objTemp.TotalAmount = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxType")
                            {
                                objTemp.TaxType = (string)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;

                            }
                            else if (app.Key == "AvailStock")
                            {
                                objTemp.StockAvailable = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnPer")
                            {
                                objTemp.CommissionPer = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnAmt")
                            {
                                objTemp.CommissionAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "RPValue")
                            {
                                objTemp.RPValue = (decimal)app.Value;
                            }
                            else if (app.Key == "OfferUID")
                            {
                                objTemp.OfferUID = (decimal)app.Value;
                            }
                            else if (app.Key == "ProductType")
                            {
                                objTemp.ProductType = app.Value.ToString();
                            }
                        }
                        objPartyOrderModel.objListProduct.Add(objTemp);
                    }
                    objPartyOrderModel.LoginUser = Session["LoginUser"] as User;
                    // Retrive the Name of HOST
                    string hostName = Dns.GetHostName();
                    // Get the IP  
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
                    objPartyOrderModel.objProduct.UID = myIP + currentDate;
                    objResponse = objTransacManager.SaveDispatchOrder(objPartyOrderModel);
                    //if (objResponse.ResponseStatus == "OK")
                    //{
                    //    InventorySession.StoredDistributorValues = objResponse.ResponseDetailsToPrint;
                    //}
                    if (objResponse.ResponseStatus == "OK")
                    {                       
                        objLogManager.SaveLog(Session["LoginUser"] as User, "Dispatched order - " + objPartyOrderModel.OrderNo, myIP + currentDate);
                    }
                }

            }
            else
            {
                objResponse.ResponseMessage = "Something went wrong!";
                objResponse.ResponseStatus = "FAILED";
            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult DeleteBills()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "DeleteBills");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public ActionResult DeleteBill(string BillNo, string FsessId, string Reason)
        {
            ResponseDetail objResponse = new ResponseDetail();
            List<SalesReport> objBillList = new List<SalesReport>();
            try
            {
                decimal UserId = (Session["LoginUser"] as User).UserId;
                objResponse = objTransacManager.DeleteBills(BillNo, FsessId, UserId, Reason);
                if (objResponse.ResponseStatus == "OK")
                {
                    //Added log
                    string hostName = Dns.GetHostName();
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");                    
                    objLogManager.SaveLog(Session["LoginUser"] as User, "Delete Bill - " + BillNo, myIP + currentDate);
                }
            }
            catch (Exception ex)
            {

            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult SalesReturn()
        {
            try
            {
                if (Session["LoginUser"] != null)
                {
                    string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                    string returnNo = objTransacManager.GetSalesReturnNumber(LoginPartyCode);
                    ViewBag.returnNo = returnNo;
                }
            }
            catch (Exception ex)
            {

            }
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "SalesReturn");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");

        }

        public ActionResult GetListOfPartyBills(string party, string partyType)
        {
            List<PartyBill> objResponse = new List<PartyBill>();
            try
            {
                string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                objResponse = objTransacManager.GetBillList(partyType, party, LoginPartyCode);
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListOfSupplierBills(string supplier)
        {
            List<PartyBill> objResponse = new List<PartyBill>();
            try
            {
                objResponse = objTransacManager.GetListOfSupplierBills(supplier);
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetListOfBillProducts(string BillNo)
        {
            DistributorBillModel objResponse = new DistributorBillModel();
            try
            {
                objResponse = objTransacManager.getInvoice(BillNo, "","F");
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse.objListProduct, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetBillDetail(string BillNo)
        {
            PartyBill objResponse = new PartyBill();
            try
            {
                objResponse = objTransacManager.GetBillDetail(BillNo, (Session["LoginUser"] as User).PartyCode);
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetPurchaseDetail(string BillNo)
        //{
        //    PurchaseBill objResponse = new PurchaseBill();
        //    try
        //    {
        //        objResponse = objTransacManager.GetPurchaseDetail(BillNo);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return Json(objResponse, JsonRequestBehavior.AllowGet);
        //}



        [HttpPost]
        public ActionResult SaveReturnOrder(SalesReturnModel objPartyOrderModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            if (objPartyOrderModel != null)
            {
                objPartyOrderModel.ProductList = new List<ProductModel>();
                if (!string.IsNullOrEmpty(objPartyOrderModel.objProductListStr))
                {
                    var objects = JArray.Parse(objPartyOrderModel.objProductListStr); // parse as array  
                    foreach (JObject root in objects)
                    {
                        ProductModel objTemp = new ProductModel();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            if (app.Key == "ProductCode")
                            {
                                objTemp.ProductCodeStr = (string)app.Value;
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = (string)app.Value;
                            }
                            else if (app.Key == "Rate")
                            {
                                objTemp.Rate = (decimal)app.Value;
                            }
                            else if (app.Key == "Barcode")
                            {
                                objTemp.Barcode = app.Value.ToString();
                            }
                            else if (app.Key == "BatchNo")
                            {
                                objTemp.BatchNo = app.Value.ToString();
                            }
                            else if (app.Key == "MRP")
                            {
                                objTemp.MRP = (decimal?)app.Value;
                            }
                            else if (app.Key == "Qty")
                            {
                                objTemp.Quantity = (decimal)app.Value;
                            }
                            else if (app.Key == "PV")
                            {
                                objTemp.PV = (decimal)app.Value;
                            }
                            else if (app.Key == "CV")
                            {
                                objTemp.CV = (decimal)app.Value;
                            }
                            else if (app.Key == "BV")
                            {
                                objTemp.BV = (decimal)app.Value;
                            }
                            else if (app.Key == "RP")
                            {
                                objTemp.RP = (decimal)app.Value;
                            }
                            else if (app.Key == "DP")
                            {
                                objTemp.DP = (decimal)app.Value;
                            }
                            else if (app.Key == "CVValue")
                            {
                                objTemp.CVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.PVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "BVValue")
                            {
                                objTemp.BVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscPer")
                            {
                                objTemp.DiscPer = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscAmt")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxAmount")
                            {
                                objTemp.TaxAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "GST")
                            {
                                objTemp.GSTPer = (decimal)app.Value;
                            }
                            else if (app.Key == "Amount")
                            {
                                objTemp.Amount = (decimal)app.Value;
                            }
                            else if (app.Key == "TotalAmount")
                            {
                                objTemp.TotalAmount = (decimal)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "CommissionPer")
                            {
                                objTemp.CommissionPer = (decimal)app.Value;
                            }
                            else if (app.Key == "CommissionAmt")
                            {
                                objTemp.CommissionAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "RPValue")
                            {
                                objTemp.RPValue = (decimal)app.Value;
                            }
                            else if (app.Key == "ReturnQty")
                            {
                                objTemp.ReturnQty = (decimal)app.Value;
                            }

                        }
                        objPartyOrderModel.ProductList.Add(objTemp);
                    }
                    objPartyOrderModel.LoggedInUserId = (Session["LoginUser"] as User).UserId;
                    objPartyOrderModel.EntryBy = (Session["LoginUser"] as User).PartyCode;
                    // Retrive the Name of HOST
                    string hostName = Dns.GetHostName();
                    // Get the IP  
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
                    objPartyOrderModel.LoggedInUserIP = myIP;
                    objResponse = objTransacManager.SaveOrderReturn(objPartyOrderModel);
                    if (objResponse.ResponseStatus == "OK")
                    {
                        //Added log                       
                        objLogManager.SaveLog(Session["LoginUser"] as User, "Return Order - " + objPartyOrderModel.OrderNo, myIP + currentDate);
                    }

                }

            }
            else
            {
                objResponse.ResponseMessage = "Something went wrong!";
                objResponse.ResponseStatus = "FAILED";
            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult SavePurchaseReturnOrder(PurchaseReturnModel objPartyOrderModel)
        //{
        //    ResponseDetail objResponse = new ResponseDetail();
        //    if (objPartyOrderModel != null)
        //    {
        //        objPartyOrderModel.ProductList = new List<ProductModel>();
        //        if (!string.IsNullOrEmpty(objPartyOrderModel.objProductListStr))
        //        {
        //            var objects = JArray.Parse(objPartyOrderModel.objProductListStr); // parse as array  
        //            foreach (JObject root in objects)
        //            {
        //                ProductModel objTemp = new ProductModel();
        //                foreach (KeyValuePair<String, JToken> app in root)
        //                {
        //                    if (app.Key == "ProductCode")
        //                    {
        //                        objTemp.ProductCodeStr = (string)app.Value;
        //                    }
        //                    else if (app.Key == "ProductName")
        //                    {
        //                        objTemp.ProductName = (string)app.Value;
        //                    }
        //                    else if (app.Key == "Rate")
        //                    {
        //                        objTemp.Rate = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "Barcode")
        //                    {
        //                        objTemp.Barcode = app.Value.ToString();
        //                    }
        //                    else if (app.Key == "BatchNo")
        //                    {
        //                        objTemp.BatchNo = app.Value.ToString();
        //                    }
        //                    else if (app.Key == "MRP")
        //                    {
        //                        objTemp.MRP = (decimal?)app.Value;
        //                    }
        //                    else if (app.Key == "Qty")
        //                    {
        //                        objTemp.Quantity = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "PV")
        //                    {
        //                        objTemp.PV = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "CV")
        //                    {
        //                        objTemp.CV = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "BV")
        //                    {
        //                        objTemp.BV = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "RP")
        //                    {
        //                        objTemp.RP = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "DP")
        //                    {
        //                        objTemp.DP = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "CVValue")
        //                    {
        //                        objTemp.CVValue = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "PVValue")
        //                    {
        //                        objTemp.PVValue = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "BVValue")
        //                    {
        //                        objTemp.BVValue = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "DiscPer")
        //                    {
        //                        objTemp.DiscPer = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "DiscAmt")
        //                    {
        //                        objTemp.DiscAmt = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "TaxAmount")
        //                    {
        //                        objTemp.TaxAmt = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "GST")
        //                    {
        //                        objTemp.GSTPer = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "Amount")
        //                    {
        //                        objTemp.Amount = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "TotalAmount")
        //                    {
        //                        objTemp.TotalAmount = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "PVValue")
        //                    {
        //                        objTemp.DiscAmt = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "CommissionPer")
        //                    {
        //                        objTemp.CommissionPer = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "CommissionAmt")
        //                    {
        //                        objTemp.CommissionAmt = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "RPValue")
        //                    {
        //                        objTemp.RPValue = (decimal)app.Value;
        //                    }
        //                    else if (app.Key == "ReturnQty")
        //                    {
        //                        objTemp.ReturnQty = (decimal)app.Value;
        //                    }

        //                }
        //                objPartyOrderModel.ProductList.Add(objTemp);
        //            }
        //            objPartyOrderModel.LoggedInUserId = (Session["LoginUser"] as User).UserId;
        //            objPartyOrderModel.EntryBy = (Session["LoginUser"] as User).PartyCode;
        //            // Retrive the Name of HOST
        //            string hostName = Dns.GetHostName();
        //            // Get the IP  
        //            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
        //            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
        //            objPartyOrderModel.LoggedInUserIP = myIP;
        //            objResponse = objTransacManager.SavePurchaseReturnOrder(objPartyOrderModel);
        //        }
        //    }
        //    else
        //    {
        //        objResponse.ResponseMessage = "Something went wrong!";
        //        objResponse.ResponseStatus = "FAILED";
        //    }

        //    return Json(objResponse, JsonRequestBehavior.AllowGet);
        //}
        [SessionExpire]
        public ActionResult DeletePurchaseInvoice()
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
           var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "DeletePurchaseInvoice");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }
        public ActionResult DeletePurchaseInvoices(string InwardNo, decimal FsessId, string Reason)
        {
            ResponseDetail objResponse = new ResponseDetail();
            List<SalesReport> objBillList = new List<SalesReport>();
            try
            {
                decimal UserId = (Session["LoginUser"] as User).UserId;
                objResponse = objTransacManager.DeletePurchaseInvoice(InwardNo, FsessId, UserId, Reason);
                if (objResponse.ResponseStatus == "OK")
                {
                    //Added log
                    string hostName = Dns.GetHostName();
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    objLogManager.SaveLog(Session["LoginUser"] as User, "Delete Purchase Invoice - " + InwardNo, myIP + currentDate);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult PartyTargetMaster()
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
           
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "PartyTargetMaster");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public ActionResult SavePartyTargetDetails(PartyTargetMaster objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            try
            {
                if (Session["LoginUser"] != null)
                {
                    int userid = (Session["LoginUser"] as User).UserId;
                    objModel.UserID = userid;
                    objResponse = objTransacManager.SavePartyTargetDetails(objModel);
                    if (objResponse.ResponseStatus == "OK")
                    {
                        //Added log
                        string hostName = Dns.GetHostName();
                        string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                        string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        objLogManager.SaveLog(Session["LoginUser"] as User, "Saved Party Target  for category Id- " + objModel.CatId, myIP + currentDate);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult UpdateDeliveryDetail()
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
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "UpdateDeliveryDetail");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objPaymentSummary);
            }
            else
                return RedirectToAction("Dashboard", "Home");

        }
        [SessionExpire]
        public ActionResult OldBills()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "OldBills");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        public ActionResult GetOldBills(string FromDate, string ToDate, string IdNo, string OrderNo)
        {
            string CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;
            string IsAdmin = (Session["LoginUser"] as User).IsAdmin;
            string PartyCode = "ALL";
            if (CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            List<OldBills> objDispatchOrderList = new List<OldBills>();
            objDispatchOrderList = objTransacManager.GetOldBills(FromDate, ToDate, IdNo, OrderNo, PartyCode);
            return Json(objDispatchOrderList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBillProducts(string billNo)
        {

            List<ProductModel> objDispatchOrderList = new List<ProductModel>();
            objDispatchOrderList = objTransacManager.GetBillProducts(billNo);
            return Json(objDispatchOrderList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRecordToUpdateDelDetails(string FromDate, string ToDate, string PartyCode, string Fcode, string status, string ordtype)
        {
            string CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;
            string IsAdmin = (Session["LoginUser"] as User).IsAdmin;
            //string PartyCode = "ALL";
            if (CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            List<SalesReport> objResponse = objTransacManager.GetRecordToUpdateDelDetails(FromDate, ToDate, PartyCode, Fcode, status, ordtype);
            var jsonProduct = Json(objResponse, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
        }
        public ActionResult UpdateDeliveryDetails(UpdateDeliveryDetails obj)
        {
            ResponseDetail objResponse = new ResponseDetail();
            try
            {
                if (Session["LoginUser"] != null)
                {
                    obj.LoggedInUser = (Session["LoginUser"] as User).UserId;
                }
                obj.DeliverDetailList = new List<SalesReport>();
                if (!string.IsNullOrEmpty(obj.ListObjHidden))
                {
                    var objects = JArray.Parse(obj.ListObjHidden); // parse as array  
                    foreach (JObject root in objects)
                    {
                        SalesReport objTemp = new SalesReport();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {

                            if (app.Key == "BillNo")
                            {
                                objTemp.BillNo = (string)app.Value;
                            }
                            else if (app.Key == "BillDate")
                            {
                                objTemp.BillDate = (app.Value.ToString() != "" && app.Value != null && app.Value.Type != JTokenType.Null) ? Convert.ToDateTime(DateTime.ParseExact(app.Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture)) : (DateTime?)null;
                            }
                            else if (app.Key == "SoldBy")
                            {
                                objTemp.SoldBy = (string)app.Value;
                            }
                            else if (app.Key == "PartyName")
                            {
                                objTemp.PartyName = app.Value.ToString();
                            }
                            else if (app.Key == "PartyCode")
                            {
                                objTemp.PartyCode = app.Value.ToString();
                            }
                            else if (app.Key == "Name")
                            {
                                objTemp.Name = (string)app.Value;
                            }
                            else if (app.Key == "CourierName")
                            {
                                objTemp.CourierName = (string)app.Value;
                            }
                            else if (app.Key == "DocWeight")
                            {
                                objTemp.DocWeight = (string)app.Value;
                            }
                            else if (app.Key == "DocketNo")
                            {
                                objTemp.DocketNo = (string)app.Value;
                            }
                            else if (app.Key == "DocketDate")
                            {
                                objTemp.DocketDate = (app.Value.ToString() != "" && app.Value != null && app.Value.Type != JTokenType.Null) ? Convert.ToDateTime(DateTime.ParseExact(app.Value.ToString(), "dd-MMM-yyyy", CultureInfo.InvariantCulture)) : (DateTime?)null;
                            }
                            else if (app.Key == "DOD")
                            {
                                objTemp.DOD = (app.Value.ToString() != "" && app.Value != null && app.Value.Type != JTokenType.Null) ? Convert.ToDateTime(DateTime.ParseExact(app.Value.ToString(), "dd-MMM-yyyy", CultureInfo.InvariantCulture)) : (DateTime?)null;
                            }
                            else if (app.Key == "DelvAddress")
                            {
                                objTemp.DelvAddress = (string)app.Value;
                            }
                            else if (app.Key == "CID")
                            {
                                objTemp.CID = (string)app.Value;
                            }
                            else if (app.Key == "DispDate")
                            {
                                objTemp.DispDate = (app.Value.ToString() != "" && app.Value != null && app.Value.Type != JTokenType.Null) ? Convert.ToDateTime(DateTime.ParseExact(app.Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture)) : (DateTime?)null;
                            }

                            else if (app.Key == "NetPayable")
                            {
                                objTemp.NetPayable = (string)app.Value;
                            }
                            else if (app.Key == "MobileNO")
                            {
                                objTemp.MobileNO = (string)app.Value;
                            }
                            else if (app.Key == "OrderNo")
                            {
                                objTemp.OrderNo = (string)app.Value;
                            }
                        }
                        obj.DeliverDetailList.Add(objTemp);
                    }
                }

                objResponse = objTransacManager.UpdateDeliveryDetails(obj);
                if (objResponse.ResponseStatus == "OK")
                {
                    //Added log
                    string hostName = Dns.GetHostName();
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    objLogManager.SaveLog(Session["LoginUser"] as User, "Updated delivery details for following bills- " + string.Join(",",obj.DeliverDetailList.Select(r=>r.BillNo)), myIP + currentDate);
                }
            }
            catch (Exception ex)
            {

            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult Upload()
        {
            UpdateDeliveryDetails objResponse = new UpdateDeliveryDetails();
            objResponse.ErrorMessage = "";
            try
            {
                HttpPostedFileBase upload = null;
                string filename = string.Empty;
                if (Request.Files.Count > 0)
                {
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        filename = Path.GetFileName(Request.Files[i].FileName);
                        upload = files[i];
                    }
                    if (upload != null && upload.ContentLength > 0)
                    {
                        try
                        {
                            DataTable dt = GetDataTableFromSpreadsheet(upload, filename, false);
                            objResponse.DeliverDetailList = SaveExcelReportData(dt);
                            objResponse.ErrorMessage = "OK";
                        }
                        catch (Exception e)
                        {
                            objResponse.ErrorMessage = "error--" + e.Message + "--Inner Exception--" + e.InnerException;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                objResponse.ErrorMessage = "error--" + e.Message + "--Inner Exception--" + e.InnerException;
            }

            var jsonProduct = Json(objResponse, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;
        }

        private static ISheet GetFileStream(HttpPostedFileBase MyExcelStream, string filename1, bool ReadOnly)
        {

            HttpPostedFileBase files = MyExcelStream; //Read the Posted Excel File  
            ISheet sheet; //Create the ISheet object to read the sheet cell values  
            string filename = Path.GetFileName(files.FileName); //get the uploaded file name  
            var fileExt = Path.GetExtension(filename); //get the extension of uploaded excel file  
            if (fileExt == ".xls")
            {
                HSSFWorkbook hssfwb = new HSSFWorkbook(files.InputStream); //HSSWorkBook object will read the Excel 97-2000 formats  
                sheet = hssfwb.GetSheetAt(0); //get first Excel sheet from workbook  
            }
            else
            {
                XSSFWorkbook hssfwb = new XSSFWorkbook(files.InputStream); //XSSFWorkBook will read 2007 Excel format  
                sheet = hssfwb.GetSheetAt(0); //get first Excel sheet from workbook   
            }

            return sheet;
        }
        
        public static DataTable GetDataTableFromSpreadsheet(HttpPostedFileBase MyExcelStream, string filename, bool ReadOnly)
        {

            try
            {
                var sh = GetFileStream(MyExcelStream, filename, ReadOnly);
                var dtExcelTable = new DataTable();
                dtExcelTable.Rows.Clear();
                dtExcelTable.Columns.Clear();
                var headerRow = sh.GetRow(0);
                int colCount = headerRow.LastCellNum;
                for (var c = 0; c < colCount; c++)
                    dtExcelTable.Columns.Add(headerRow.GetCell(c).ToString());
                var i = 1;
                var currentRow = sh.GetRow(i);
                while (currentRow != null)
                {
                    var dr = dtExcelTable.NewRow();
                    for (var j = 0; j < currentRow.Cells.Count; j++)
                    {
                        var cell = currentRow.GetCell(j);

                        if (cell != null)
                            switch (cell.CellType)
                            {
                                case CellType.Numeric:
                                    if (DateUtil.IsCellDateFormatted(cell))
                                    {
                                        DateTime date = cell.DateCellValue;
                                        dr[j] = date.ToString("dd/MM/yyyy");
                                    }
                                    else
                                    {
                                        dr[j] = cell.NumericCellValue.ToString();
                                    }
                                    break;
                                case CellType.String:
                                    dr[j] = cell.StringCellValue;
                                    break;
                                case CellType.Blank:
                                    dr[j] = string.Empty;
                                    break;
                            }
                    }
                    dtExcelTable.Rows.Add(dr);
                    i++;
                    currentRow = sh.GetRow(i);
                }
                return dtExcelTable;
            }
            catch (Exception e)
            {
                throw;
            }
        }


        /// <summary>
        /// Save qci report data
        /// </summary>
        /// <param name="dt"></param>
        public List<SalesReport> SaveExcelReportData(DataTable dt)
        {
            var SalesReportList = new List<SalesReport>();
            try
            {
                if (dt == null)
                {
                    return null;
                }

                var userDetail = new SalesReport();
                DataColumnCollection columns = dt.Columns;

                foreach (DataRow row in dt.Rows)
                {

                    userDetail = new SalesReport();
                    if (columns.Contains("Bill No"))
                    {
                        userDetail.BillNo = Convert.ToString(row["Bill No"]);
                    }
                    if (columns.Contains("Party Name"))
                    {
                        userDetail.PartyName = Convert.ToString(row["Party Name"]);
                    }
                    if (columns.Contains("ID No"))
                    {
                        userDetail.PartyCode = Convert.ToString(row["ID No"]);
                    }
                    if (columns.Contains("Bill By"))
                    {
                        userDetail.SoldBy = Convert.ToString(row["Bill By"]);
                    }
                    if (columns.Contains("Name"))
                    {
                        userDetail.Name = Convert.ToString(row["Name"]);
                    }
                    if (columns.Contains("Courier"))
                    {
                        userDetail.CourierName = Convert.ToString(row["Courier"]);
                    }
                    if (columns.Contains("Weight"))
                    {
                        userDetail.DocWeight = Convert.ToString(row["Weight"]);
                    }
                    if (columns.Contains("Docket No"))
                    {
                        userDetail.DocketNo = Convert.ToString(row["Docket No"]);
                    }
                    if (columns.Contains("Delv Address"))
                    {
                        userDetail.DelvAddress = Convert.ToString(row["Delv Address"]);
                    }
                    if (columns.Contains("CID"))
                    {
                        userDetail.CID = Convert.ToString(row["CID"]);
                    }
                    if (columns.Contains("Net Pay"))
                    {
                        userDetail.NetPayable = Convert.ToString(row["Net Pay"]);
                    }
                    if (columns.Contains("Mobile No"))
                    {
                        userDetail.MobileNO = Convert.ToString(row["Mobile No"]);
                    }
                    if (columns.Contains("Order No"))
                    {
                        userDetail.OrderNo = Convert.ToString(row["Order No"]);
                    }

                    try
                    {
                        if (columns.Contains("Disp Date"))
                        {
                            var date = Convert.ToString(row["Disp Date"]);
                            if (!String.IsNullOrEmpty(date) && !date.StartsWith("#"))
                            {
                                userDetail.DispDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    try
                    {
                        if (columns.Contains("Bill Date"))
                        {
                            var date = Convert.ToString(row["Bill Date"]);
                            if (!String.IsNullOrEmpty(date) && !date.StartsWith("#"))
                            {
                                userDetail.BillDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    try
                    {
                        if (columns.Contains("Docket Date"))
                        {
                            var date = Convert.ToString(row["Docket Date"]);
                            if (!String.IsNullOrEmpty(date) && !date.StartsWith("#"))
                            {
                                userDetail.DocketDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    try
                    {
                        if (columns.Contains("DOD"))
                        {
                            var date = Convert.ToString(row["DOD"]);
                            if (!String.IsNullOrEmpty(date) && !date.StartsWith("#"))
                            {
                                userDetail.DOD = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    SalesReportList.Add(userDetail);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return SalesReportList;
        }

        [HttpPost]
        public ActionResult CheckForOffer(DistributorBillModel objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            if (objModel != null)
            {
                objModel.objListProduct = new List<ProductModel>();
                if (!string.IsNullOrEmpty(objModel.objProductListStr))
                {
                    var objects = JArray.Parse(objModel.objProductListStr); // parse as array  
                    foreach (JObject root in objects)
                    {
                        ProductModel objTemp = new ProductModel();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            if (app.Key == "Code")
                            {
                                objTemp.ProdCode = (int)app.Value;
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = (string)app.Value;
                            }
                            else if (app.Key == "Rate")
                            {
                                objTemp.Rate = (decimal)app.Value;
                            }
                            else if (app.Key == "Barcode")
                            {
                                objTemp.Barcode = app.Value.ToString();
                            }
                            else if (app.Key == "BatchNo")
                            {
                                objTemp.BatchNo = app.Value.ToString();
                            }
                            else if (app.Key == "MRP")
                            {
                                objTemp.MRP = (decimal?)app.Value;
                            }
                            else if (app.Key == "Qty")
                            {
                                objTemp.Quantity = (decimal)app.Value;
                            }
                            else if (app.Key == "PV")
                            {
                                objTemp.PV = (decimal)app.Value;
                            }
                            else if (app.Key == "CV")
                            {
                                objTemp.CV = (decimal)app.Value;
                            }
                            else if (app.Key == "BV")
                            {
                                objTemp.BV = (decimal)app.Value;
                            }
                            else if (app.Key == "RP")
                            {
                                objTemp.RP = (decimal)app.Value;
                            }
                            else if (app.Key == "DP")
                            {
                                objTemp.DP = (decimal)app.Value;
                            }
                            else if (app.Key == "CVValue")
                            {
                                objTemp.CVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.PVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "BVValue")
                            {
                                objTemp.BVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscPer")
                            {
                                objTemp.DiscPer = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscAmt")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxAmt")
                            {
                                objTemp.TaxAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxPer")
                            {
                                objTemp.TaxPer = (decimal)app.Value;
                            }
                            else if (app.Key == "Amount")
                            {
                                objTemp.Amount = (decimal)app.Value;
                            }
                            else if (app.Key == "TotalAmount")
                            {
                                objTemp.TotalAmount = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxType")
                            {
                                objTemp.TaxType = (string)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;

                            }
                            else if (app.Key == "AvailStock")
                            {
                                objTemp.StockAvailable = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnPer")
                            {
                                objTemp.CommissionPer = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnAmt")
                            {
                                objTemp.CommissionAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "RPValue")
                            {
                                objTemp.RPValue = (decimal)app.Value;
                            }
                            else if (app.Key == "SubCatId")
                            {
                                objTemp.SubCatId = (decimal)app.Value;
                            }
                        }
                        objModel.objListProduct.Add(objTemp);
                    }
                    objModel.objCustomer.UserDetails = Session["LoginUser"] as User;
                    // Retrive the Name of HOST
                    string hostName = Dns.GetHostName();
                    // Get the IP  
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
                    objModel.objProduct.UID = myIP + currentDate;
                    objResponse = objTransacManager.CheckForOffer(objModel);
                }

            }
            else
            {
                objResponse.ResponseMessage = "Something went wrong!";
                objResponse.ResponseStatus = "FAILED";
            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult PackUnpack()
        {
            PackUnpack objPackUnpack = new PackUnpack();
            try
            {
                if (Session["LoginUser"] != null)
                {
                    int LoginPartyId = (Session["LoginUser"] as User).UserId;
                    string LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
                    objPackUnpack.kitlist = objTransacManager.GetKitList();
                    objPackUnpack.UserId = LoginPartyId;
                    objPackUnpack.UserCode = LoginPartyCode;
                }
            }
            catch (Exception ex)
            {

            }
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "PackUnpack");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objPackUnpack);
            }
            else
                return RedirectToAction("Dashboard", "Home");

        }

        public ActionResult GetPackUnpackProducts(string PackUnpack, decimal KitId, string prodID)
        {
            string LoginPartyCode = string.Empty;
            if (Session["LoginUser"] != null)
            {
                LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
            }
            List<PackUnpackProduct> objResponse = objTransacManager.GetPackUnpackProducts(PackUnpack, KitId, prodID, LoginPartyCode);
            var jsonProduct = Json(objResponse, JsonRequestBehavior.AllowGet);
            jsonProduct.MaxJsonLength = int.MaxValue;
            return jsonProduct;

        }

        [HttpPost]
        public ActionResult SavePackUnpack(PackUnpack obj)
        {
            ResponseDetail objResponse = new ResponseDetail();
            try
            {
                obj.productList = new List<PackUnpackProduct>();
                if (!string.IsNullOrEmpty(obj.productstring))
                {
                    var objects = JArray.Parse(obj.productstring); // parse as array  
                    foreach (JObject root in objects)
                    {
                        PackUnpackProduct objTemp = new PackUnpackProduct();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            if (app.Key == "ProductId")
                            {
                                objTemp.ProductId = (string)app.Value;
                            }
                            if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = (string)app.Value;
                            }
                            if (app.Key == "Qunatity")
                            {
                                objTemp.Qunatity = (string)app.Value;
                            }
                            if (app.Key == "AvailStock")
                            {
                                objTemp.AvailStock = (string)app.Value;
                            }
                        }
                        obj.productList.Add(objTemp);
                    }
                }
                objResponse = objTransacManager.SavePackUnpack(obj);
                if (objResponse.ResponseStatus == "OK")
                {
                    //Added log
                    string hostName = Dns.GetHostName();
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    objLogManager.SaveLog(Session["LoginUser"] as User, obj.PackOrUnpack + " for kit -" + obj.kitName, myIP + currentDate);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult WalletRequest()
        {
            WalletRequest objReq = new WalletRequest();
            List<SelectListItem> objBankList = new List<SelectListItem>();
            var result = objTransacManager.GetBankList();
            foreach (var obj in result)
            {
                if (obj.BankCode == 0)
                    objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString(), Selected = true });
                else
                    objBankList.Add(new SelectListItem { Text = obj.BankName, Value = obj.BankCode.ToString() });
            }
            ViewBag.BankNames = objBankList;

            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "WalletRequest");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }
        [HttpPost]
        public ActionResult GetMPaymodes()
        {
            List<PaymodeModel> objPaymodes = new List<PaymodeModel>();
            objPaymodes = objTransacManager.GetMPaymodes();
            return Json(objPaymodes, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetMBanks()
        {
            List<BankModel> objBanks = new List<BankModel>();
            objBanks = objTransacManager.GetMBanks();
            return Json(objBanks, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GenerateRequest(WalletRequest objWallet)
        {
            Response.Redirect(@"~/Test.aspx?Amount=" + objWallet.Amount);
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult SaveWalletRequest(WalletRequest objWallet, HttpPostedFileBase upload)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objResponse.ResponseStatus = "Failed";
            objResponse.ResponseMessage = "Something went wrong.";
            if (upload != null)
            {
                var path = Server.MapPath("~/images/WalletReqs/");
                //var paths = path.Split(':');
                //if (paths != null || paths.Count() > 0)
                //{
                //    path = paths[0] + ":\\" + "WalletReqs";
                //}
                if (upload != null && upload.FileName != null)
                {
                    if (!Directory.Exists(path))
                    {
                        //DirectoryInfo dInfo = new DirectoryInfo(path);
                        //DirectorySecurity dSecurity = dInfo.GetAccessControl();
                        //dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                        //dInfo.SetAccessControl(dSecurity);
                        Directory.CreateDirectory(path);
                    }
                    //string myfile = Guid.NewGuid() + "-" + BannerImage.FileName;
                    string myfile = Guid.NewGuid() + "-" + Path.GetFileName(upload.FileName);
                    myfile = myfile.Replace(" ", "").Replace(" ", "");
                    var FlName = Path.Combine(path, myfile);
                    upload.SaveAs(FlName);
                    objWallet.ScannedFileName = myfile; //HttpContext.Request.Url.Host + "/images/WalletReqs/"+ myfile;
                }
            }
            string resp=objTransacManager.SaveWalletRequest(objWallet);
            if (resp == "OK") {
                objResponse.ResponseStatus = "OK";
                objResponse.ResponseMessage = "Saved Successfully!";

                if (objResponse.ResponseStatus == "OK")
                {
                    //Added log
                    string hostName = Dns.GetHostName();
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    objLogManager.SaveLog(Session["LoginUser"] as User, "Added wallet request for - "+ objWallet.ReqByName,myIP + currentDate);
                }
            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }


        [SessionExpire]
        public ActionResult Courier()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "Courier");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public ActionResult CourierList()
        {
            List<Courier> objModel = new List<Courier>();
            objModel = objTransacManager.GetCouierList();
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }


        [SessionExpire]
        public ActionResult CourierMaster()
        {
            Courier objModel = new Courier();

            List<SelectListItem> objActiveStatus = new List<SelectListItem>();
            objActiveStatus.Add(new SelectListItem
            {
                Text = "Yes",
                Value = "Y"
            });

            objActiveStatus.Add(new SelectListItem
            {
                Text = "No",
                Value = "N"
            });
            ViewBag.ActiveStatus = objActiveStatus;

            return View(objModel);
        }

        public ActionResult IsDuplicateCourierName(string Name)
        {
            return Json(objTransacManager.IsDuplicateCouirerName(Name), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveCourierDetails(Courier objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objModel.UserId = (Session["LoginUser"] as User).UserId;
            objResponse = objTransacManager.SaveCourierDetails(objModel);
            if (objResponse.ResponseStatus == "OK")
            {
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, "Saved Courier Details for - "+ objModel.Name, myIP + currentDate);
            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult ApproveWalletRequest()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "ApproveWalletRequest");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [SessionExpire]
        public ActionResult AddCourierDetail (decimal ID)
        {
            Courier obj = new Entity.Common.Courier();
            obj.ID = ID;

            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "AddCourierDetail");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(obj);
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public ActionResult GetAllWalletRequest(string PartyCode,string dateType,string FromDate,string ToDate,string IsApproved )
        {
            //**Added on 24Nov18
            string CurrentPartyCode = "";
            if (Session["LoginUser"] != null)
                CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;

            if (PartyCode == "" || CurrentPartyCode != System.Web.Configuration.WebConfigurationManager.AppSettings["WRPartyCode"])
                PartyCode = CurrentPartyCode;
            //**
            List<WalletRequest> objModel = new List<WalletRequest>();
            objModel = objTransacManager.GetAllWalletRequest( PartyCode,  dateType,  FromDate,  ToDate,  IsApproved);
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveApproveWaletRequest(WalletRequest objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            List<WalletRequest> objwallet = new List<Entity.Common.WalletRequest>();
            string logString = string.Empty;
            if (!string.IsNullOrEmpty(objModel.GridString))
            {
                var objects = JArray.Parse(objModel.GridString); // parse as array  
                foreach (JObject root in objects)
                {
                    WalletRequest objTemp = new WalletRequest();
                    foreach (KeyValuePair<String, JToken> app in root)
                    {
                        if (app.Key == "ReqNo")
                        {
                            objTemp.ReqNo = (string)app.Value;
                            logString += objTemp.ReqNo;
                        }
                        if (app.Key == "IsApproved")
                        {
                            if ((string)app.Value.ToString().ToUpper() == "REJECT")
                            {
                                objTemp.IsApproved = "R";
                                logString += "- Rejected";
                            }
                            else if ((string)app.Value.ToString().ToUpper() == "PENDING")
                            {
                                objTemp.IsApproved = "N";
                                logString += "- PENDING";
                            }                        
                            else if ((string)app.Value.ToString().ToUpper() == "Approve".ToUpper())
                            { 
                                objTemp.IsApproved = "Y";
                                logString += "- Approved";
                            }
                             else
                                objTemp.IsApproved = (string)app.Value;
                        }
                        if (app.Key == "ApproveRemark")
                        {
                            objTemp.ApproveRemark = (string)app.Value;
                        }
                        objTemp.ApproveBy = (Session["LoginUser"] as User).UserId;
                    }
                    objwallet.Add(objTemp);
                }
                objResponse = objTransacManager.SaveApproveWaletRequest(objwallet);
                if (objResponse.ResponseStatus == "OK")
                {
                    //Added log
                    string hostName = Dns.GetHostName();
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    objLogManager.SaveLog(Session["LoginUser"] as User, "Updated following wallet request as -" + logString, myIP + currentDate);
                }
            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }       
        [HttpGet]
        public ActionResult GetCouierDetailList(decimal ID)
        {
            List<Courier> objModel = new List<Courier>();
            objModel = objTransacManager.GetCouierDetailList(ID);
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveCourierAmount(Courier objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objModel.UserId = (Session["LoginUser"] as User).UserId;
            objResponse = objTransacManager.SaveCourierAmount(objModel);
            if (objResponse.ResponseStatus == "OK")
            {
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, "Saved courier details for - " + objModel.Name, myIP + currentDate);
            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CourierDetailByweight(int CourierId, int Weight)
        {
            Courier objResult = new Entity.Common.Courier();
            objResult = objTransacManager.CourierDetailByweight( CourierId,  Weight);
            return Json(objResult, JsonRequestBehavior.AllowGet);
        }
        [SessionExpire]
        public ActionResult UpdateDate()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "UpdateDate");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        public ActionResult UpdateBillDate(SalesReport objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            List<SalesReport> objBillList = new List<SalesReport>();
            try
            {
                int UserID = (Session["LoginUser"] as User).UserId;
                if (objModel != null)
                {

                    if (!string.IsNullOrEmpty(objModel.BillList))
                    {
                        var objects = JArray.Parse(objModel.BillList); // parse as array  
                        foreach (JObject root in objects)
                        {
                            SalesReport objTemp = new SalesReport();
                            foreach (KeyValuePair<String, JToken> app in root)
                            {
                                // var appName = app.Key;
                                //    var ProductGrid = [{"AvailStock":"", "SNo": "", "Code": "", "ProductName": "", "MRP": "", "DP": "", "Rate": "","BatchNo":"", "Barcode": "", "RP": "", "BV": "", "CV": "", "PV": "", "Qty": "", "RPValue": "", "BVValue": "", "CVValue": "", "PVValue": "", "CommsnPer": "", "CommsnAmt": "", "DiscPer": "", "DiscAmt": "", "Amount": "", "TaxType": "", "TaxPer": "", "TaxAmt": "", "TotalAmount": ""}];
                                if (app.Key == "BillNo")
                                {
                                    objTemp.BillNo = app.Value.ToString();
                                }
                                else if (app.Key == "SoldBy")
                                {
                                    objTemp.SoldBy = (Session["LoginUser"] as User).PartyCode;
                                }
                                else if (app.Key == "IsUpdate")
                                {
                                    objTemp.IsUpdate = (bool)app.Value;
                                }
                                else if (app.Key == "RecordDate")
                                {
                                    objTemp.BillDate = (DateTime)app.Value;
                                }

                            }
                            objTemp.BillDate = objModel.RecordDate;
                            objBillList.Add(objTemp);

                           
                        }

                        objResponse = objTransacManager.UpdateBillDate(objBillList,UserID);
                        if (objResponse.ResponseStatus == "OK")
                        {
                            //Added log
                            string hostName = Dns.GetHostName();
                            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            objLogManager.SaveLog(Session["LoginUser"] as User, "Updated Bill date for - " + objBillList.Select(r=>r.BillNo).ToArray(), myIP + currentDate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult OfferDynamic(string ActionName, string OfferCode)
        {
            List<SelectListItem> Activeoption = new List<SelectListItem>();
            Activeoption.Add(new SelectListItem() { Text = "Active", Value = "Y" });
            Activeoption.Add(new SelectListItem() { Text = "Deactive", Value = "N" });
            ViewBag.ActiveOptions = Activeoption;

            List<SelectListItem> option = new List<SelectListItem>();
            option.Add(new SelectListItem() { Text = "All", Value = "A" });
            option.Add(new SelectListItem() { Text = "Yes", Value = "Y" });
            //option.Add(new SelectListItem() { Text = "No", Value = "N" });
            ViewBag.DropDownOptions = option;

            List<SelectListItem> idStatus = new List<SelectListItem>();
            idStatus.Add(new SelectListItem() { Text = "All", Value = "A" });
            idStatus.Add(new SelectListItem() { Text = "Active", Value = "Y" });
            idStatus.Add(new SelectListItem() { Text = "Deactive", Value = "N" });
            ViewBag.idStatus = idStatus;

            List<SelectListItem> ForBillType = new List<SelectListItem>();
            ForBillType.Add(new SelectListItem() { Text = "All", Value = "All" });
            ForBillType.Add(new SelectListItem() { Text = "Repurchase", Value = "Repurchase" });
            ForBillType.Add(new SelectListItem() { Text = "FirstBill", Value = "FirstBill" });
            ViewBag.ForBillType = ForBillType;

            List<SelectListItem> ForDateType = new List<SelectListItem>();
            ForDateType.Add(new SelectListItem() { Text = "Date", Value = "D" });
            ForDateType.Add(new SelectListItem() { Text = "Range", Value = "R" });            
            ViewBag.ForDateType = ForDateType;

            Offer objoffer = new Offer();
           

            if (!string.IsNullOrEmpty(OfferCode))
            {
                int code = int.Parse(OfferCode);
                objoffer = objTransacManager.getOfferDetail(code);
                objoffer.ActionName = ActionName;
            }

            
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "OfferOnValueMaster");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objoffer);
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        public ActionResult GetProductList()
        {
            decimal LoginStateCode = (Session["LoginUser"] as User).StateCode;
            List<ProductDetails> objProductList = objProductManager.GetProductList(LoginStateCode);
            var list = (from r in objProductList
                        select new
                        {
                            ProductName = r.ProductName,
                            ProductCode = r.ProductCodeStr
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveDynamicOffer(Offer objdetails)
        {
            ResponseDetail detail = new ResponseDetail();
            detail = objTransacManager.saveOfferDynamic(objdetails);
            if (detail.ResponseStatus == "OK")
            {
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, "Created new offer on bv" , myIP + currentDate);
            }
            return Json(detail, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult ExtraPVOffer(string ActionName, string OfferCode)
        {
            List<SelectListItem> Activeoption = new List<SelectListItem>();
            Activeoption.Add(new SelectListItem() { Text = "Active", Value = "Y" });
            Activeoption.Add(new SelectListItem() { Text = "Deactive", Value = "N" });
            ViewBag.ActiveOptions = Activeoption;
            Offer objoffer = new Offer();


            if (!string.IsNullOrEmpty(OfferCode))
            {
                int code = int.Parse(OfferCode);
                objoffer = objTransacManager.getOfferDetail(code);
                objoffer.ActionName = ActionName;
            }
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "ExtraPVOfferMaster");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objoffer);
            }
            else
                return RedirectToAction("Dashboard", "Home");
                      
        }

        

        [SessionExpire]
        public ActionResult OfferOnValueMaster()
        {
           
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "OfferOnValueMaster");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [SessionExpire]
        public ActionResult ExtraPVOfferMaster()
        {
           var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "ExtraPVOfferMaster");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public ActionResult GetAllExtraPVOfferList()
        {
            return Json(objTransacManager.GetAllExtraPVOfferList(),JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveExtraPVOffer(Offer ObjOffer)
        {

            if (ObjOffer != null)
            {
                ObjOffer.objProductList = new List<OfferProduct>();
                if (!string.IsNullOrEmpty(ObjOffer.ProductString))
                {
                    var objects = JArray.Parse(ObjOffer.ProductString); // parse as array  
                    foreach (JObject root in objects)
                    {
                        OfferProduct objTemp = new OfferProduct();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            if (app.Key == "ProdCode")
                            {
                                objTemp.ProductCode = app.Value.ToString();
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = app.Value.ToString();
                            }
                            else if (app.Key == "FreeQty")
                            {
                                objTemp.Qty = (decimal)app.Value;
                            }
                            else if (app.Key == "PVVal")
                            {
                                objTemp.PVValue = (decimal?)app.Value;
                            }
                            else if (app.Key == "PVPer")
                            {
                                objTemp.PVPer = (decimal?)app.Value;
                            }

                        }
                        ObjOffer.objProductList.Add(objTemp);
                    }
                }
            }
            var detail = objTransacManager.SaveExtraPVOffer(ObjOffer);
            if (detail.ResponseStatus == "OK")
            {
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, "Created new extra PV offer", myIP + currentDate);
            }
            return Json(detail, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllOfferOnValueList()
        {
            return Json(objTransacManager.GetAllOfferOnValueList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getfreeproducts(int id)
        {
            return Json(objTransacManager.getfreeproducts(id), JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet]
        public ActionResult getExtraPVfreeproducts(int id)
        {
            return Json(objTransacManager.getExtraPVfreeproducts(id,false), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPartyBalance()
        {
            return Json(objTransacManager.GetPartyBalance(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CheckForExtraPVOffer(DistributorBillModel objModel)
        {
            List<OfferProduct> objResponse = new List<OfferProduct>();
            if (objModel != null)
            {
                objModel.objListProduct = new List<ProductModel>();
                if (!string.IsNullOrEmpty(objModel.objProductListStr))
                {
                    var objects = JArray.Parse(objModel.objProductListStr); // parse as array  
                    foreach (JObject root in objects)
                    {
                        ProductModel objTemp = new ProductModel();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            if (app.Key == "Code")
                            {
                                objTemp.ProdCode = (int)app.Value;
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = (string)app.Value;
                            }
                            else if (app.Key == "Rate")
                            {
                                objTemp.Rate = (decimal)app.Value;
                            }
                            else if (app.Key == "Barcode")
                            {
                                objTemp.Barcode = app.Value.ToString();
                            }
                            else if (app.Key == "BatchNo")
                            {
                                objTemp.BatchNo = app.Value.ToString();
                            }
                            else if (app.Key == "MRP")
                            {
                                objTemp.MRP = (decimal?)app.Value;
                            }
                            else if (app.Key == "Qty")
                            {
                                objTemp.Quantity = (decimal)app.Value;
                            }
                            else if (app.Key == "PV")
                            {
                                objTemp.PV = (decimal)app.Value;
                            }
                            else if (app.Key == "CV")
                            {
                                objTemp.CV = (decimal)app.Value;
                            }
                            else if (app.Key == "BV")
                            {
                                objTemp.BV = (decimal)app.Value;
                            }
                            else if (app.Key == "RP")
                            {
                                objTemp.RP = (decimal)app.Value;
                            }
                            else if (app.Key == "DP")
                            {
                                objTemp.DP = (decimal)app.Value;
                            }
                            else if (app.Key == "CVValue")
                            {
                                objTemp.CVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.PVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "BVValue")
                            {
                                objTemp.BVValue = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscPer")
                            {
                                objTemp.DiscPer = (decimal)app.Value;
                            }
                            else if (app.Key == "DiscAmt")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxAmt")
                            {
                                objTemp.TaxAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxPer")
                            {
                                objTemp.TaxPer = (decimal)app.Value;
                            }
                            else if (app.Key == "Amount")
                            {
                                objTemp.Amount = (decimal)app.Value;
                            }
                            else if (app.Key == "TotalAmount")
                            {
                                objTemp.TotalAmount = (decimal)app.Value;
                            }
                            else if (app.Key == "TaxType")
                            {
                                objTemp.TaxType = (string)app.Value;
                            }
                            else if (app.Key == "PVValue")
                            {
                                objTemp.DiscAmt = (decimal)app.Value;

                            }
                            else if (app.Key == "AvailStock")
                            {
                                objTemp.StockAvailable = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnPer")
                            {
                                objTemp.CommissionPer = (decimal)app.Value;
                            }
                            else if (app.Key == "CommsnAmt")
                            {
                                objTemp.CommissionAmt = (decimal)app.Value;
                            }
                            else if (app.Key == "RPValue")
                            {
                                objTemp.RPValue = (decimal)app.Value;
                            }
                            else if (app.Key == "SubCatId")
                            {
                                objTemp.SubCatId = (decimal)app.Value;
                            }
                        }
                        objModel.objListProduct.Add(objTemp);
                    }
                    objModel.objCustomer.UserDetails = Session["LoginUser"] as User;
                    // Retrive the Name of HOST
                    string hostName = Dns.GetHostName();
                    // Get the IP  
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
                    objModel.objProduct.UID = myIP + currentDate;
                    objResponse = objTransacManager.CheckForExtraPVOffer(objModel);
                }

            }
            else
            {
              
            }

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }
        [SessionExpire]
        public ActionResult AddWallet()
        {
            try
            {
                string CurrentPartyCode="";
                if (Session["LoginUser"] != null)
                {
                     CurrentPartyCode = (Session["LoginUser"] as User).PartyCode;
                }

                var result = objTransacManager.GetPartyBalance();

                List<SelectListItem> PartyList = new List<SelectListItem>();
                foreach (var obj in result)
                {
                    PartyList.Add(new SelectListItem
                    {
                        Text = obj.PartyName,
                        Value = obj.PartyCode.ToString()
                    });
                }
                ViewBag.PartyList = PartyList;
                if (new UserController().CanUserAccessMenu((Session["LoginUser"] as User).UserId, "AddWallet"))
                    return View();
                else
                    return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            
        }

        public ActionResult DebitCreditWallet(Wallet objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();
            try
            {
                if (objModel != null)
                {

                    objResponse = objTransacManager.DebitCreditWallet(objModel);

                    if (objResponse.ResponseStatus == "OK")
                    {
                        //Added log
                        string hostName = Dns.GetHostName();
                        string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                        string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        objLogManager.SaveLog(Session["LoginUser"] as User, objModel.DrCr +" wallet balance for "+ objModel.FCode, myIP + currentDate);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
       public ActionResult BuythisGetthatOffer(string ActionName, string OfferCode)
       {
           List<SelectListItem> Activeoption = new List<SelectListItem>();
           Activeoption.Add(new SelectListItem() { Text = "Active", Value = "Y" });
           Activeoption.Add(new SelectListItem() { Text = "Deactive", Value = "N" });
           ViewBag.ActiveOptions = Activeoption;
           Offer objoffer = new Offer();


           if (!string.IsNullOrEmpty(OfferCode))
           {
               int code = int.Parse(OfferCode);
               objoffer = objTransacManager.getOfferDetail(code);
             
           }
           return View(objoffer);
       }

        [SessionExpire]
        public ActionResult BuythisGetthatOfferMaster()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "BuythisGetthatOfferMaster");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }
        
        [HttpGet]
        public ActionResult GetAllBuyThisGetThatOfferList()
        {
            return Json(objTransacManager.GetAllBuyThisGetThatOfferList(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult SaveBuyThisGetThatOffer(Offer ObjOffer)
        {
            if (ObjOffer != null)
            {
                ObjOffer.objProductList = new List<OfferProduct>();
                if (!string.IsNullOrEmpty(ObjOffer.ProductString))
                {
                    var objects = JArray.Parse(ObjOffer.ProductString); // parse as array  
                    foreach (JObject root in objects)
                    {
                        OfferProduct objTemp = new OfferProduct();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            if (app.Key == "ProdCode")
                            {
                                objTemp.ProductCode = app.Value.ToString();
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = app.Value.ToString();
                            }
                            else if (app.Key == "FreeQuantity")
                            {
                                objTemp.Qty = (decimal)app.Value;
                            }
                            else if (app.Key == "BVApplied")
                            {
                                objTemp.IsBvApplied = app.Value.ToString();
                            }
                            else if (app.Key == "BuyProduct")
                            {
                                objTemp.IsParent = false;
                            }
                        }
                        ObjOffer.objProductList.Add(objTemp);
                    }
                }

                if (!string.IsNullOrEmpty(ObjOffer.ParentProductString))
                {
                    var objects = JArray.Parse(ObjOffer.ParentProductString); // parse as array  
                    foreach (JObject root in objects)
                    {
                        OfferProduct objTemp = new OfferProduct();
                        foreach (KeyValuePair<String, JToken> app in root)
                        {
                            if (app.Key == "ProdCode")
                            {
                                objTemp.ProductCode = app.Value.ToString();
                            }
                            else if (app.Key == "ProductName")
                            {
                                objTemp.ProductName = app.Value.ToString();
                            }
                            else if (app.Key == "BuyQuantity")
                            {
                                objTemp.Qty = (decimal)app.Value;
                            }                            
                            else if (app.Key == "BuyProduct")
                            {
                                objTemp.IsParent = true; 
                            }
                        }
                        ObjOffer.objProductList.Add(objTemp);
                    }
                }
            }
            var detail = objTransacManager.SaveBuyThisGetThatOffer(ObjOffer);
            if (detail.ResponseStatus == "OK")
            {
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, ObjOffer.ActionName + " Buy This Get That Offer", myIP + currentDate);
            }
            return Json(detail, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getOfferBuyproducts(int id)
        {
            return Json(objTransacManager.getExtraPVfreeproducts(id, true), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFWalletBalance(string partyCode)
        {
            return Json(objTransacManager.GetFWalletBalance(partyCode), JsonRequestBehavior.AllowGet);
        }
    }
}

