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
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace InventoryManagement.Controllers
{
    
    public class InvoiceController : Controller
    {
        TransactionManager objTransacManager = new TransactionManager();
        public ActionResult DownloadPdf(string Pm, string id)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                var base64DecodedBytes = System.Convert.FromBase64String(Pm);
                string BillNoValue = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);
                string CurrentPartyCode = "";
                DistributorBillModel model = objTransacManager.getInvoice(BillNoValue, CurrentPartyCode,id);
                if (model == null)
                {
                    model = new DistributorBillModel();
                }
                if (model.BillNo != null)
                {
                    string result = RazorViewToString.RenderRazorViewToString(this, "../Transaction/InvoicePrint", model);

                    string imagePath = System.Web.HttpContext.Current.Server.MapPath("~/images");
                    imagePath = Path.Combine(imagePath, "logo.png");

                    result = result.Replace("/images/logo.png", imagePath);

                    StringReader sr = new StringReader(result);
                    Document pdfDoc = new Document(PageSize.A4, 25f, 25f, 0f, 0f);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    pdfDoc.Close();
                    return File(stream.ToArray(), "application/pdf", "Invoice_"+ model.BillNo + ".pdf");
                }
                else
                {
                    string imagePath = System.Web.HttpContext.Current.Server.MapPath("~/images");
                    imagePath = Path.Combine(imagePath, "NoImage.jpg");

                    //result = result.Replace("/images/NoImage.jpg", imagePath);
                    return File(imagePath, "Invoice.pdf");
                }
            }
        }
    }
}