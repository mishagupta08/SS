using InventoryManagement.App_Start;
using InventoryManagement.Business;
using InventoryManagement.Entity.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace InventoryManagement.Controllers
{

    public class OfferController : Controller
    {
        OfferManager objOfferManager = new OfferManager();
        LogManager objLogManager = new LogManager();
        TransactionManager objTransacManager = new TransactionManager();

        [SessionExpire]
        public ActionResult Offers()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "Offers");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public ActionResult GetAllOffers()
        {
            return Json(objOfferManager.GetAllOffers(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetValidOfferList(string Doj, string UpgradeDate, string IsFirstBill, string ActiveStatus, string FormNo)
        {
            return Json(objOfferManager.GetValidOfferList(Doj, UpgradeDate, IsFirstBill, ActiveStatus, FormNo), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveOffer(Offer ObjOffer)
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
                            else if (app.Key == "BVApplied")
                            {
                                objTemp.IsBvApplied = app.Value.ToString();
                            }
                            else if (app.Key == "OnMRP")
                            {
                                objTemp.OnMRP = app.Value.ToString();
                            }
                            else if (app.Key == "IsBuyProduct")
                            {
                                objTemp.IsParent = false;
                            }
                            else if (app.Key == "PVVal")
                            {
                                objTemp.PVValue = (decimal?)app.Value;
                            }
                            else if (app.Key == "PVPer")
                            {
                                objTemp.PVPer = (decimal?)app.Value;
                            }
                            else if (app.Key == "Discount")
                            {
                                objTemp.Discount = (decimal?)app.Value; ///Convert.ToDecimal( (string) app.Value );
                            }
                            else if (app.Key == "DiscountPer")
                            {
                                objTemp.DiscountPer = (decimal?)app.Value;
                            }
                            else if (app.Key == "Scheme")
                            {
                                objTemp.Scheme = (string)app.Value;
                            }
                            else if (app.Key == "Rupee")
                            {
                                objTemp.Rupee = (decimal?)app.Value;
                            }
                            else if (app.Key == "Confirm")
                            {
                                objTemp.Confirm = (string)app.Value;
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
                            else if (app.Key == "FreeQty")
                            {
                                objTemp.Qty = (decimal)app.Value;
                            }
                            else if (app.Key == "OnMRP")
                            {
                                objTemp.OnMRP = app.Value.ToString();
                            }
                            else if (app.Key == "IsBuyProduct")
                            {
                                objTemp.IsParent = true;
                            }
                            else if (app.Key == "IEProduct")
                            {
                                var t = app.Value.ToString();
                                if (t == "Include")
                                {
                                    objTemp.IncludeInOffer = true;
                                }
                                else
                                {
                                    objTemp.IncludeInOffer = false;
                                }
                            }

                        }
                        ObjOffer.objProductList.Add(objTemp);
                    }
                }
            }
            var detail = objOfferManager.SaveOffer(ObjOffer);
            if (detail.ResponseStatus == "OK")
            {
                //Added log
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                objLogManager.SaveLog(Session["LoginUser"] as User, ObjOffer.ActionName + "Offer" + ObjOffer.OfferName, myIP + currentDate);
            }
            return Json(detail, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult OfferMaster(string ActionName, string OfferCode)
        {
            List<SelectListItem> Activeoption = new List<SelectListItem>();
            Activeoption.Add(new SelectListItem() { Text = "Active", Value = "Y" });
            Activeoption.Add(new SelectListItem() { Text = "Deactive", Value = "N" });
            ViewBag.ActiveOptions = Activeoption;

            List<SelectListItem> option = new List<SelectListItem>();
            //option.Add(new SelectListItem() { Text = "All", Value = "A" });
            option.Add(new SelectListItem() { Text = "No", Value = "A" });

            ViewBag.DropDownOptions = option;

            List<SelectListItem> idStatus = new List<SelectListItem>();
            idStatus.Add(new SelectListItem() { Text = "All", Value = "A" });
            ViewBag.idStatus = idStatus;

            List<SelectListItem> ForBillType = new List<SelectListItem>();
            ForBillType.Add(new SelectListItem() { Text = "Any Bill", Value = "All" });
            ViewBag.ForBillType = ForBillType;

            List<SelectListItem> ForDateType = new List<SelectListItem>();
            ForDateType.Add(new SelectListItem() { Text = "Range", Value = "R" });

            ViewBag.ForDateType = ForDateType;

            List<SelectListItem> OfferType = new List<SelectListItem>();
            OfferType.Add(new SelectListItem() { Text = "Buy this Get that", Value = "BuythisGetthat" });
            ViewBag.OfferTypeOptions = OfferType;

            List<SelectListItem> Days = new List<SelectListItem>();
            for (int i = 1; i <= 31; i++)
            {
                Days.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString(), Selected = (i == 1 ? true : false) });
            }
            ViewBag.Days = Days;

            //List<Offer> OList = objTransacManager.GetAllBuyThisGetThatOfferList("Active",true);
            List<SelectListItem> CombineOfferList = new List<SelectListItem>();
            //foreach (var rec in OList)
            //{
            //    CombineOfferList.Add(new SelectListItem
            //    {
            //        Text = rec.OfferName,
            //        Value = rec.AID.ToString(),
            //        Selected = false
            //    });
            //}
            ViewBag.CombineOfferListOptions = CombineOfferList;

            Offer objoffer = new Offer();
            objoffer.ActionName = ActionName;

            if (!string.IsNullOrEmpty(OfferCode))
            {
                int code = int.Parse(OfferCode);
                objoffer = objOfferManager.getOfferDetail(code, null);
                objoffer.ActionName = ActionName;
                if (!string.IsNullOrEmpty(objoffer.CombineWithOffer))
                {
                    var combineList = objoffer.CombineWithOffer.Split(',');
                    foreach (var rec in CombineOfferList)
                    {
                        if (combineList.Contains(rec.Value))
                        {
                            rec.Selected = true;
                        }
                    }
                }
            }

            ViewBag.CombineOfferListOptions = CombineOfferList;


            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "Offers");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objoffer);
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public ActionResult getOfferDetail(int id, string CustId)
        {
            return Json(objOfferManager.getOfferDetail(id, CustId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetOfferProductsList(int id)
        {
            return Json(objOfferManager.GetOfferProductsList(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CheckOfferName(string offername, int offerid)
        {
            return Json(objOfferManager.CheckOfferName(offername, offerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckbtgtOffers(string checkwith, string check)
        {
            return Json(objOfferManager.CheckbtgtOffers(checkwith, check), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getProductsForOffer(string OfferID, string OType)
        {
            return Json(objOfferManager.getProductsForOffer(OfferID, OType), JsonRequestBehavior.AllowGet);
        }


    }
}