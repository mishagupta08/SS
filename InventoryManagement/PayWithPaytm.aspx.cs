using paytm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace InventoryManagement
{
    public partial class PayWithPaytm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string UniQID = "", regid = "", amount = "0", Mobile = "", shpcharge = "", scmemtype = "", email = "", coupon = "";
            if (Request["UniQID"] != null)
            {
                UniQID = Request["UniQID"];
            }
            if (Request["regid"] != null)
            {
                regid = Request["regid"];
            }
            if (Request["amount"] != null)
            {
                amount = Request["amount"];
            }
            if (Request["Mobile"] != null)
            {
                Mobile = Request["Mobile"];
            }
            if (Request["shpcharge"] != null)
            {
                shpcharge = Request["shpcharge"];
            }
            if (Request["scmemtype"] != null)
            {
                scmemtype = Request["scmemtype"];
            }
            if (Request["email"] != null)
            {
                email = Request["email"];
            }
            if (Request["coupon"] != null)
            {
                coupon = Request["coupon"];
            }
            if (!Page.IsPostBack)
            {
                //using (var objmem = new MemberServiceClient())
                //{

                   // var cnt = objmem.SCOnlineInsert("INSERT", Convert.ToInt32(regid), amount, Mobile, UniQID, "PENDING", "PENDING", "", "", "PAYTM", shpcharge, scmemtype, coupon);
                    PaymentGateWay(UniQID, regid, amount, Mobile, scmemtype, email);
                //}
            }
        }

        public void PaymentGateWay(string ORDER_ID, string regid, string amount, string Mobile, string scmemtype, string email)
        {
            string mobile = Mobile;

            email = (email != "" && email != null) ? email : "support@sarsomail.net";
            //string email = "support@sarsomail.net";
            string orderid = ORDER_ID;
            string return_url = ConfigurationManager.AppSettings["SCreturn_url"];
            string REQUEST_TYPE = "DEFAULT";
            string MID = ConfigurationManager.AppSettings["MID"];
            string CHANNEL_ID = ConfigurationManager.AppSettings["CHANNEL_ID"];
            string INDUSTRY_TYPE_ID = ConfigurationManager.AppSettings["INDUSTRY_TYPE_ID"];
            string WEBSITE = ConfigurationManager.AppSettings["WEBSITE"];
            string MerchantKey = ConfigurationManager.AppSettings["MerchantKey"];
            string custId = regid;
            string TXN_AMOUNT = amount;
            //string TXN_AMOUNT = "1.00";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("REQUEST_TYPE", REQUEST_TYPE);
            parameters.Add("MID", MID);
            parameters.Add("CHANNEL_ID", CHANNEL_ID);
            parameters.Add("INDUSTRY_TYPE_ID", INDUSTRY_TYPE_ID);
            parameters.Add("WEBSITE", WEBSITE);
            parameters.Add("EMAIL", email);
            parameters.Add("MOBILE_NO", mobile);
            parameters.Add("CALLBACK_URL", return_url);
            parameters.Add("CUST_ID", custId);
            parameters.Add("ORDER_ID", orderid);
            parameters.Add("TXN_AMOUNT", TXN_AMOUNT);

            string paytmURL = ConfigurationManager.AppSettings["PaytmgwayURL"] + "?orderid=" + orderid;

            string checksum = CheckSum.generateCheckSum(MerchantKey, parameters);


            string outputHTML = "<html>";
            outputHTML += "<head>";
            outputHTML += "<title>Merchant Check Out Page</title>";
            outputHTML += "</head>";
            outputHTML += "<body>";
            outputHTML += "<center><h1>Please do not refresh this page...</h1></center>";
            outputHTML += "<form method='post' action='" + paytmURL + "' name='f1'>";
            outputHTML += "<table border='1'>";
            outputHTML += "<tbody>";

            foreach (string key in parameters.Keys)
            {
                outputHTML += "<input type='hidden' name='" + key + "' value='" + parameters[key] + "'>'";
            }

            outputHTML += "<input type='hidden' name='CHECKSUMHASH' value='" + checksum + "'>";
            outputHTML += "</tbody>";
            outputHTML += "</table>";
            outputHTML += "<script type='text/javascript'>";
            outputHTML += "document.f1.submit();";
            outputHTML += "</script>";
            outputHTML += "</form>";
            outputHTML += "</body>";
            outputHTML += "</html>";

            Response.Write(outputHTML);

        }
    }
}