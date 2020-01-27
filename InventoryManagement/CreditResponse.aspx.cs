using InventoryManagement.API.Models;
using InventoryManagement.Business;
using InventoryManagement.Entity.Common;
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
    public partial class CreditResponse : System.Web.UI.Page
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
            TransactionManager objTransacManager = new TransactionManager();
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
                lbl_EMAIL = Request.Form["EMAIL"];
                lbl_MOBILE_NO = Request.Form["MOBILE_NO"];
                lbl_CUST_ID = Request.Form["CUST_ID"];
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
                //LogResponse(resmerchantrefno, responseparm);
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
                        List<TblPaymentGetWayRequest> tblPayment = new List<TblPaymentGetWayRequest>();
                        PaytmGateway payment = new PaytmGateway();
                        payment.ORDER_ID = lbl_ORDERID;
                        payment.PaymentStatus = lbl_PAYMENTMODE;
                        payment.BillStatus = lbl_PAYMENTMODE;
                        payment.TxnId = lbl_TXNID;
                        tblPayment = objTransacManager.GetCreditRequest(lbl_ORDERID);
                        payment.regid = tblPayment[0].IdNo;
                        payment.amount = lbl_TXNAMOUNT;
                        if (tblPayment[0].ORDER_ID != "")
                        {
                            string resp = string.Empty;
                            resp = objTransacManager.CreditRequestOnlineInsert(payment);
                            if (resp == "OK")
                            {
                                lblerrormsg.Text = "Payment is successfuly done ! your transaction id is " + lbl_TXNID + " click <a href = http://franchise.sarsobiz.net/Report/WalletReport>here</a>" + " to check your wallet balance.";
                            }
                            else
                            {
                                lblerrormsg.Text = "some error occored please contact to administrator.";
                            }
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        lblerrormsg.Text = "some error occored please contact to administrator.";
                    }
                }
                else
                {
                    lblerrormsg.Text = "some error occored please contact to administrator.";
                }
            }
            //}
            //}
            catch (Exception ex)
            {
                // LogResponse("SUCCESS-10-ERROR", "");
                //LogResponse(!string.IsNullOrEmpty(resmerchantrefno) ? resmerchantrefno : "Order ID getting empty",ex.Message);
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
        //public void LogOnlineData(string remarks)
        //{
        //    var objmem = new MemberServiceClient();
        //    int cnt = objmem.LogOnlineDataNew(Request.Form["SUBS_ID"] == null ? "" : Request.Form["SUBS_ID"],
        //        lbl_MID,
        //        lbl_TXNID,
        //        lbl_ORDERID,
        //        lbl_BANKTXNID,
        //        lbl_TXNAMOUNT,
        //        lbl_CURRENCY,
        //        lbl_STATUS,
        //        lbl_RESPCODE,
        //        lbl_RESPMSG,
        //        lbl_TXNDATE,
        //        lbl_GATEWAYNAME,
        //        lbl_BANKNAME == null ? "" : lbl_BANKNAME,
        //        lbl_PAYMENTMODE,
        //        Request.Form["PROMO_CAMP_ID"] == null ? "" : Request.Form["PROMO_CAMP_ID"],
        //        Request.Form["PROMO_STATUS"] == null ? "" : Request.Form["PROMO_STATUS"],
        //        Request.Form["PROMO_RESPCODE"] == null ? "" : Request.Form["PROMO_RESPCODE"],
        //        lbl_CHECKSUMHASH,
        //        remarks);
        //}
        //public void LogResponse(string merrefno, string responsevalues)
        //{
        //    var objmem = new MemberServiceClient();
        //    int cnt = objmem.LogResponse(merrefno, responsevalues);
        //}
    }
}