using InventoryManagement.App_Start;
using InventoryManagement.Business;
using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace InventoryManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // GET: Home
        DashboardManager objDashboardManager = new DashboardManager();
        LogManager objLogManager = new LogManager();

        [SessionExpire]
        public ActionResult Dashboard()
        {
            Dashboard obj = new Dashboard();
            string LoginPartyCode = "";
            if (Session["LoginUser"] != null)
            {
                LoginPartyCode = (Session["LoginUser"] as User).PartyCode;
            }
            obj = objDashboardManager.GetDashboard(LoginPartyCode);

            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            objLogManager.SaveLog(Session["LoginUser"] as User, "Fetched dashboard details for user.", myIP + currentDate);

            return View(obj);
        }
    }
}