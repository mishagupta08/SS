﻿using System;
using System.Activities.Expressions;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;
using ShoppingService;
using StocksService;
using MemberService;
using AdminService;
using Utilities;
using paytm;

public partial class SCAPPResponse : System.Web.UI.Page
{
    string responseparm = "";
    string resmerchantrefno = "";
    string lbl_MID = string.Empty;
    string lbl_TXNID = string.Empty;
    string lbl_ORDERID = string.Empty;
    string lbl_BANKTXNID = string.Empty;
    string lbl_TXNAMOUNT = string.Empty;
    string lbl_CURRENCY = string.Empty;
    string lbl_STATUS = string.Empty;
    string lbl_RESPCODE = string.Empty;
    string lbl_RESPMSG = string.Empty;
    string lbl_TXNDATE = string.Empty;
    string lbl_GATEWAYNAME = string.Empty;
    string lbl_BANKNAME = string.Empty;
    string lbl_PAYMENTMODE = string.Empty;
    string lbl_CHECKSUMHASH = string.Empty;
    string lbl_EMAIL = string.Empty;
    string lbl_MOBILE_NO = string.Empty;
    string lbl_CUST_ID = string.Empty;
    protected void Page_Load(object sender, EventArgs e)
    {

        try
        {
            lbl_MID = Request.Form["MID"];
            lbl_TXNID = Request.Form["TXNID"];
            lbl_ORDERID = Request.Form["ORDERID"];
            lbl_BANKTXNID = Request.Form["BANKTXNID"];
            lbl_TXNAMOUNT = Request.Form["TXNAMOUNT"];
            lbl_CURRENCY = Request.Form["CURRENCY"];
            lbl_STATUS = Request.Form["STATUS"];
            lbl_RESPCODE = Request.Form["RESPCODE"];
            lbl_RESPMSG = Request.Form["RESPMSG"];
            lbl_TXNDATE = Request.Form["TXNDATE"];
            lbl_GATEWAYNAME = Request.Form["GATEWAYNAME"];
            lbl_BANKNAME = Request.Form["BANKNAME"];
            lbl_PAYMENTMODE = Request.Form["PAYMENTMODE"];
            lbl_CHECKSUMHASH = Request.Form["CHECKSUMHASH"];
            //lbl_EMAIL = Request.Form["EMAIL"];
            //lbl_MOBILE_NO = Request.Form["MOBILE_NO"];
            //lbl_CUST_ID = Request.Form["CUST_ID"];
            List<KeyValuePair<string, string>> postparamslist = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < Request.Form.Keys.Count; i++)
            {
                KeyValuePair<string, string> postparam = new KeyValuePair<string, string>(Request.Form.Keys[i],
                                                                                          Request.Form[i]);
                postparamslist.Add(postparam);
            }

            foreach (KeyValuePair<string, string> param in postparamslist)
            {
                if (param.Key == "ORDERID") resmerchantrefno = param.Value != null ? param.Value : "";
                responseparm += (param.Key + ":" + param.Value + "|");
            }
            LogResponse(resmerchantrefno, responseparm);
            //LogResponse("SUCCESS-0", responseparm);
            if (lbl_STATUS == "TXN_SUCCESS")
            {
                ////var objmem = new MemberServiceClient();
                ////var objsc = new ShoppingServiceClient();
                //LogResponse("SUCCESS-1", responseparm);
                if (IsValidChecksum())
                {
                    // LogResponse("SUCCESS-2", responseparm);
                    ////var objutil = new UtilitiesClient();
                   // var dtonline = objmem.GetSCOnline(lbl_ORDERID);
                    if (dtonline.Rows.Count > 0)
                    {
                        //LogResponse("SUCCESS-3", responseparm);
                        var dt = objsc.ScOrderPG(lbl_ORDERID, Convert.ToInt32(dtonline.Rows[0]["regid"].ToString()), "PAYTM",
                                                 dtonline.Rows[0]["shpcharge"].ToString(), "A", "",
                                                 dtonline.Rows[0]["scmemtype"].ToString());
                        if (dt.Rows.Count > 0)
                        {
                            if (dt.Rows[0]["res"].ToString() == "")
                            {
                                if (Convert.ToDecimal(dtonline.Rows[0]["amount"].ToString()) ==
                                    Convert.ToDecimal(lbl_TXNAMOUNT))
                                {

                                    int cnt = objmem.SCOnlineInsert("UPDATE", 0, "0", "", lbl_ORDERID,
                                                                    "SUCCESS", "SUCCESS", lbl_TXNID,
                                                                    dt.Rows[0]["billno"].ToString(), "", "0", "", "");
                                    //LogResponse("SUCCESS-4", "");
                                    LogOnlineData("SUCCESS");
                                    //lbl_MID = Request.Form["MID"];
                                    //lbl_TXNID = Request.Form["TXNID"];
                                    //lbl_ORDERID = Request.Form["ORDERID"];
                                    //lbl_BANKTXNID = Request.Form["BANKTXNID"];
                                    //lbl_TXNAMOUNT = Request.Form["TXNAMOUNT"];
                                    //lbl_CURRENCY = Request.Form["CURRENCY"];
                                    //lbl_STATUS = Request.Form["STATUS"];
                                    //lbl_RESPCODE = Request.Form["RESPCODE"];
                                    //lbl_RESPMSG = Request.Form["RESPMSG"];
                                    //lbl_TXNDATE = Request.Form["TXNDATE"];
                                    //lbl_GATEWAYNAME = Request.Form["GATEWAYNAME"];
                                    //lbl_BANKNAME = Request.Form["BANKNAME"];
                                    //lbl_PAYMENTMODE = Request.Form["PAYMENTMODE"];
                                    //lbl_CHECKSUMHASH = Request.Form["CHECKSUMHASH"];
                                    //lbl_EMAIL = Request.Form["EMAIL"];
                                    //lbl_MOBILE_NO = Request.Form["MOBILE_NO"];
                                    //lbl_CUST_ID = Request.Form["CUST_ID"];
                                    Response.AddHeader("Content-type", "application/json");
                                    Response.Write("{\"ERRORMESSAGE\":\"PAYMENT SUCCESSFULLY for Order id:" + lbl_ORDERID + "\",\"ERRORCODE\":\"0\",\"ORDER_ID\":\"" + lbl_ORDERID + "\",\"AMOUNT\":\"" + lbl_TXNAMOUNT + "\",\"TXNID\":\"" + lbl_TXNID + "\",\"BANKTXNID\":\"" + lbl_BANKTXNID + "\",\"CURRENCY\":\"" + lbl_CURRENCY + "\",\"STATUS\":\"" + lbl_STATUS + "\",\"RESPCODE\":\"" + lbl_RESPCODE + "\",\"RESPMSG\":\"" + lbl_RESPMSG + "\",\"PAYMENTMODE\":\"" + lbl_PAYMENTMODE + "\",\"EMAIL\":\"" + lbl_EMAIL + "\",\"MOBILE\":\"" + lbl_MOBILE_NO + "\",\"CUST_ID\":\"" + lbl_CUST_ID + "\"}");
                                    //ScriptManager.RegisterClientScriptBlock(this, GetType(), "clientScript",
                                    //                                        "javascript:repurchaseInvoice('" +
                                    //                                        dt.Rows[0]["reqno"] + "','" +
                                    //                                        dt.Rows[0]["billno"] + "')", true);

                                }
                                else
                                {
                                    //LogResponse("SUCCESS-5", "");
                                    //payment success bill failed
                                    int cnt = objmem.SCOnlineInsert("UPDATE", 0, "0", "", lbl_ORDERID,
                                                                    "SUCCESS", "PAYMENT SUCCESS BILL FAILED",
                                                                    lbl_TXNID, "", "", "0", "", "");

                                    LogOnlineData("PAYMENT SUCCESS BILL FAILED Amount Mismatch");
                                    Response.AddHeader("Content-type", "application/json");
                                    Response.Write("{\"ERRORMESSAGE\":\"PAYMENT SUCCESS BILL FAILED Amount Mismatch for Order id:" + lbl_ORDERID + "\",\"ERRORCODE\":\"1\"}");
                                    //lblerrormsg.Text =
                                    //    "PAYMENT SUCCESS BILL FAILED Amount Mismatch for Order id: " +
                                    //    lbl_ORDERID.Text;
                                }
                            }
                        }
                        else
                        {
                            //payment success bill failed
                            //LogResponse("SUCCESS-6", "");
                            int cnt = objmem.SCOnlineInsert("UPDATE", 0, "0", "", lbl_ORDERID, "SUCCESS",
                                                            "PAYMENT SUCCESS BILL FAILED", lbl_TXNID, "", "",
                                                            "0", "", "");

                            LogOnlineData("PAYMENT SUCCESS BILL FAILED");
                            Response.AddHeader("Content-type", "application/json");
                            Response.Write("{\"ERRORMESSAGE\":\"PAYMENT SUCCESS BILL FAILED for Order id:" + lbl_ORDERID + "\",\"ERRORCODE\":\"1\"}");
                            //lblerrormsg.Text = "PAYMENT SUCCESS BILL FAILED for Order id: " + lbl_ORDERID.Text;
                        }

                    }
                    else
                    {
                        //LogResponse("SUCCESS-7", "");
                        //payment success bill failed
                        int cnt = objmem.SCOnlineInsert("UPDATE", 0, "0", "", lbl_ORDERID, "SUCCESS",
                                                        "PAYMENT SUCCESS BILL FAILED", lbl_TXNID, "", "", "0",
                                                        "", "");

                        LogOnlineData("PAYMENT SUCCESS BILL FAILED");
                        Response.AddHeader("Content-type", "application/json");
                        Response.Write("{\"ERRORMESSAGE\":\"PAYMENT SUCCESS BILL FAILED for Order id:" + lbl_ORDERID + "\",\"ERRORCODE\":\"1\"}");
                    }


                }
                else
                {
                    //LogResponse("SUCCESS-8", "");
                    //payment success bill failed
                    int cnt = objmem.SCOnlineInsert("UPDATE", 0, "0", "", lbl_ORDERID, "SUCCESS",
                                                    "PAYMENT SUCCESS BILL FAILED", lbl_TXNID, "", "", "0", "", "");

                    LogOnlineData("PAYMENT SUCCESS BILL FAILED Checksum Mismatch");
                    Response.AddHeader("Content-type", "application/json");
                    Response.Write("{\"ERRORMESSAGE\":\"PAYMENT SUCCESS BILL FAILED Checksum Mismatch for orderId:" + lbl_ORDERID + "\",\"ERRORCODE\":\"1\"}");
                }

            }
            else if (lbl_STATUS == "PENDING") //response pending with the gateway
            {
                //var objmem = new MemberServiceClient();

                //int cnt = objmem.SCOnlineInsert("UPDATEPGPENDING", 0, "0", "", lbl_ORDERID.Text, "PGPENDING",
                //                                "PGPENDING", lbl_TXNID.Text, "", "", "0", "");
                // LogResponse("SUCCESS-9", "");
                LogOnlineData("PGPENDING-pending with gateway");
                Response.AddHeader("Content-type", "application/json");
                Response.Write("{\"ERRORMESSAGE\":\"pending with gateway:" + lbl_ORDERID + "\",\"ERRORCODE\":\"1\"}");
            }
            else
            {
                // LogResponse("SUCCESS-10", "");
                //Response.Redirect("OnlineError.aspx?errmsg=Transaction Failed.");
                Response.AddHeader("Content-type", "application/json");
                Response.Write("{\"ERRORMESSAGE\":\"pending with gateway:" + lbl_ORDERID + "\",\"ERRORCODE\":\"1\"}");
            }
            //Response.AddHeader("Content-type", "application/json");
            //Response.Write("{\"ERRORMESSAGE\":\"PAYMENT SUCCESSFULLY for Order id:" + lbl_ORDERID + "\",\"ERRORCODE\":\"0\",\"ORDER_ID\":\"" + lbl_ORDERID + "\",\"AMOUNT\":\"" + lbl_TXNAMOUNT + "\",\"TXNID\":\"" + lbl_TXNID + "\",\"BANKTXNID\":\"" + lbl_BANKTXNID + "\",\"CURRENCY\":\"" + lbl_CURRENCY + "\",\"STATUS\":\"" + lbl_STATUS + "\",\"RESPCODE\":\"" + lbl_RESPCODE + "\",\"RESPMSG\":\"" + lbl_RESPMSG + "\",\"PAYMENTMODE\":\"" + lbl_PAYMENTMODE + "\",\"EMAIL\":\"" + lbl_EMAIL + "\",\"MOBILE\":\"" + lbl_MOBILE_NO + "\",\"CUST_ID\":\"" + lbl_CUST_ID + "\"}");
        }
        //}
        //}
        catch (Exception ex)
        {
            // LogResponse("SUCCESS-10-ERROR", "");
            LogResponse(!string.IsNullOrEmpty(resmerchantrefno) ? resmerchantrefno : "Order ID getting empty",
                        ex.Message);
        }


    }

    public string ConvertedDate(string Date)
    {
        var SDate = Date.Split('/');
        return SDate[0] + "/" + SDate[1] + "/" + SDate[2];
        // return Date;
    }


    public bool IsValidChecksum()
    {
        bool isvalid = false;
        string MerchantKey = ConfigurationManager.AppSettings["MerchantKey"];

        //Dictionary<string, string> parameters = new Dictionary<string, string>();
        //string paytmChecksum = "";
        //foreach (string key in Request.Form.Keys)
        //{
        //    parameters.Add(key.Trim(), Request.Form[key].Trim());
        //}

        //if (parameters.ContainsKey("CHECKSUMHASH"))
        //{
        //    paytmChecksum = parameters["CHECKSUMHASH"];
        //    parameters.Remove("CHECKSUMHASH");
        //}

        //if (CheckSum.verifyCheckSum(MerchantKey, parameters, paytmChecksum))
        //{
        //    isvalid = true;
        //}

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        try
        {
            string paytmChecksum = "";
            foreach (string key in Request.Form.Keys)
            {
                if (Request.Form[key].Contains("|"))
                {
                    parameters.Add(key.Trim(), "");
                }
                else
                {
                    parameters.Add(key.Trim(), Request.Form[key].Trim());
                }
            }
            if (parameters.ContainsKey("CHECKSUMHASH"))
            {
                paytmChecksum = parameters["CHECKSUMHASH"];
                paytmChecksum = paytmChecksum.Replace(" ", "+");
                parameters.Remove("CHECKSUMHASH");
            }

            //if (CheckSum.verifyCheckSum(PaytmConstants.MERCHANT_KEY, parameters, paytmChecksum))
            //{
            //    isvalid = true;
            //}
            //else
            //{
            //    isvalid = false;
            //}
            if (CheckSum.verifyCheckSum(MerchantKey, parameters, paytmChecksum))
            {
                isvalid = true;
            }

        }
        catch (Exception ex)
        {
            //parameters.Add("IS_CHECKSUM_VALID", "N");


        }
        return isvalid;
    }
    public void LogOnlineData(string remarks)
    {
        var objmem = new MemberServiceClient();
        int cnt = objmem.LogOnlineDataNew(Request.Form["SUBS_ID"] == null ? "" : Request.Form["SUBS_ID"],
            lbl_MID,
            lbl_TXNID,
            lbl_ORDERID,
            lbl_BANKTXNID,
            lbl_TXNAMOUNT,
            lbl_CURRENCY,
            lbl_STATUS,
            lbl_RESPCODE,
            lbl_RESPMSG,
            lbl_TXNDATE,
            lbl_GATEWAYNAME,
            lbl_BANKNAME == null ? "" : lbl_BANKNAME,
            lbl_PAYMENTMODE,
            Request.Form["PROMO_CAMP_ID"] == null ? "" : Request.Form["PROMO_CAMP_ID"],
            Request.Form["PROMO_STATUS"] == null ? "" : Request.Form["PROMO_STATUS"],
            Request.Form["PROMO_RESPCODE"] == null ? "" : Request.Form["PROMO_RESPCODE"],
            lbl_CHECKSUMHASH,
            remarks);
    }
    public void LogResponse(string merrefno, string responsevalues)
    {
        var objmem = new MemberServiceClient();
        int cnt = objmem.LogResponse(merrefno, responsevalues);
    }
}