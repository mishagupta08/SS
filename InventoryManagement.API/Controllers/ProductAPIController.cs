﻿using InventoryManagement.API.Models;
using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace InventoryManagement.API.Controllers
{
    public class ProductAPIController : ApiController
    {
        public ResponseDetail AddCategoryDetails(CategoryDetails model)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objResponse.ResponseStatus = "FAIL";
            objResponse.ResponseMessage = "Something went wrong!";
            try
            {
                using (var entity = new InventoryEntities())
                {
                    M_CatMaster objDTCategory = new M_CatMaster();
                    objDTCategory = (from category in entity.M_CatMaster where category.CatId == model.CategoryId select category).FirstOrDefault();
                    if (objDTCategory == null)
                    {
                        objDTCategory = new M_CatMaster();
                    }
                    if (model.IsAdd != "Delete")
                    {
                        if (model.IsAdd == "Add")
                        {
                            decimal CategoryId = (from r in entity.M_CatMaster select r.CatId).DefaultIfEmpty(0).Max();
                            CategoryId = CategoryId + 1;
                            objDTCategory.CatId = (int)CategoryId;
                        }
                        objDTCategory.CatName = model.CategoryName;
                        objDTCategory.CatDescription = string.IsNullOrEmpty(model.Description) ? "" : model.Description;
                        objDTCategory.ActiveStatus = model.IsActive ? "Y" : "N";
                        objDTCategory.RecTimeStamp = DateTime.Now;
                        objDTCategory.UserId = model.UserDetails.UserId;
                        objDTCategory.IsForPC = "";
                        objDTCategory.DelCharge = "";
                        objDTCategory.Company = "";
                        objDTCategory.LastModified = DateTime.Now.ToString();
                        objDTCategory.WebSequence = 0;
                        objDTCategory.Remarks = "";
                        objDTCategory.Prefix = "";
                        objDTCategory.OnWebSite = "Y";

                        //objDTCategory.AlterID = model.UserDetails.UserId;

                        if (model.IsAdd == "Add")
                        {
                            objDTCategory.RecTimeStamp = DateTime.Now;
                            entity.M_CatMaster.Add(objDTCategory);
                        }

                    }
                    else
                    {
                        if (objDTCategory != null)
                        {
                            //  entity.M_CatMaster.Remove(objDTCategory);
                            objDTCategory.ActiveStatus = "N";
                        }
                    }
                    try
                    {
                        int isSaved = entity.SaveChanges();
                        if (isSaved > 0)
                        {
                            if (model.IsAdd == "Add")
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Saved Successfully!";
                            }
                            else if (model.IsAdd == "Edit")
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Updated Successfully!";

                            }
                            else
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Deleted Successfully!";

                            }
                        }
                        else
                        {
                            objResponse.ResponseStatus = "Failed";
                            objResponse.ResponseMessage = "Something went wrong!";

                        }
                    }
                    catch (DbEntityValidationException ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objResponse;
        }

        public ResponseDetail AddSubCategoryDetails(SubCategoryDetails model)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objResponse.ResponseStatus = "FAIL";
            objResponse.ResponseMessage = "Something went wrong!";
            try
            {
                using (var entity = new InventoryEntities())
                {
                    M_SubCatMaster objDTSubCategory = new M_SubCatMaster();
                    objDTSubCategory = (from subcategory in entity.M_SubCatMaster where subcategory.SubCatId == model.SubCatId select subcategory).FirstOrDefault();
                    if (objDTSubCategory == null)
                    {
                        objDTSubCategory = new M_SubCatMaster();
                    }
                    if (model.IsAdd != "Delete")
                    {
                        if (model.IsAdd == "Add")
                        {
                            decimal subCategoryId = (from r in entity.M_SubCatMaster select r.SubCatId).DefaultIfEmpty(0).Max();
                            subCategoryId = subCategoryId + 1;
                            objDTSubCategory.SubCatId = (int)subCategoryId;
                        }
                        objDTSubCategory.Remarks = "";
                        objDTSubCategory.OfferHtml = "";
                        objDTSubCategory.IsForPC = "Y";
                        objDTSubCategory.OnWebSite = "Y";
                        objDTSubCategory.Description = string.IsNullOrEmpty(model.Description) ? "" : model.Description;
                        //objDTSubCategory.AlterID = model.UserDetails.UserId;
                        objDTSubCategory.SubCatName = model.subCategoryName;
                        //objDTSubCategory.Description = model.Description;
                        objDTSubCategory.CatId = model.CategoryId;
                        objDTSubCategory.ActiveStatus = model.IsActive ? "Y" : "N";

                        objDTSubCategory.UserId = model.UserDetails.UserId;
                        objDTSubCategory.LastModified = DateTime.Now.ToString();
                        if (model.IsAdd == "Add")
                        {
                            objDTSubCategory.RecTimeStamp = DateTime.Now;
                            entity.M_SubCatMaster.Add(objDTSubCategory);
                        }
                    }
                    else
                    {
                        if (objDTSubCategory != null)
                        {
                            //entity.M_SubCatMaster.Remove(objDTSubCategory);
                            objDTSubCategory.ActiveStatus = "N";
                        }
                    }
                    try
                    {
                        int isSaved = entity.SaveChanges();
                        if (isSaved > 0)
                        {
                            if (model.IsAdd == "Add")
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Saved Successfully!";
                            }
                            else if (model.IsAdd == "Edit")
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Updated Successfully!";
                            }
                            else
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Deleted Successfully!";
                            }
                        }
                        else
                        {
                            objResponse.ResponseStatus = "Failed";
                            objResponse.ResponseMessage = "Something went wrong!";

                        }
                    }
                    catch (DbEntityValidationException ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objResponse;
        }

        public ResponseDetail IsMasterExists(CheckDuplicateModel model)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objResponse.ResponseStatus = "FAIL";
            objResponse.ResponseMessage = "Match found!";
            try
            {
                using (var entity = new InventoryEntities())
                {

                    switch (model.masterTable)
                    {
                        case "":
                            objResponse.ResponseStatus = "FAIL";
                            objResponse.ResponseMessage = "Something went wrong!";
                            break;
                        case "CategoryMaster":
                            if (!string.IsNullOrEmpty(model.masterName))
                            {
                                var result = (from cm in entity.M_CatMaster where cm.CatName.ToLower().Equals(model.masterName.ToLower()) select cm.CatName).FirstOrDefault();
                                if (result == null)
                                {
                                    objResponse.ResponseStatus = "OK";
                                    objResponse.ResponseMessage = "Match not found!";
                                }
                                else if (model.isAdd == "Edit" || model.isAdd == "Delete")
                                {
                                    objResponse.ResponseStatus = "OK";
                                    objResponse.ResponseMessage = "Match not found!";


                                }
                                else
                                {
                                    objResponse.ResponseStatus = "FAIL";
                                    objResponse.ResponseMessage = "This Category Name already exists!";
                                }
                            }
                            else
                            {
                                objResponse.ResponseStatus = "FAIL";
                                objResponse.ResponseMessage = "Something went wrong!";
                            }
                            break;
                        case "SubCategoryMaster":
                            var result1 = (from cm in entity.M_SubCatMaster where cm.CatId == model.CategoryId && cm.SubCatName == model.masterName select cm).FirstOrDefault();
                            if (result1 == null)
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Match not found!";
                            }
                            else if (model.isAdd == "Edit" || model.isAdd == "Delete")
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Match not found!";


                            }
                            else
                            {
                                objResponse.ResponseStatus = "FAIL";
                                objResponse.ResponseMessage = "This Sub Category already exists!";

                            }
                            break;
                        case "ProductMaster":
                            var result2 = (from cm in entity.M_ProductMaster where cm.CatId == model.CategoryId && cm.SubCatId == model.SubCategoryId && cm.ProductName==model.masterName select cm).FirstOrDefault();
                            if (result2 == null)
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Match not found!";
                            }
                            else if (model.isAdd == "Edit" || model.isAdd == "Delete")
                            {
                                objResponse.ResponseStatus = "OK";
                                objResponse.ResponseMessage = "Match not found!";


                            }
                            else
                            {
                                objResponse.ResponseStatus = "FAIL";
                                objResponse.ResponseMessage = "This Product already exists!";

                            }
                            break;
                        default:
                            objResponse.ResponseStatus = "FAIL";
                            objResponse.ResponseMessage = "Something went wrong!";
                            break;

                    }


                }
            }
            catch (Exception ex)
            {

            }
            return objResponse;
        }

        public List<CategoryDetails> GetCategoryList(string ActiveFlag)
        {
            List<CategoryDetails> objCategoryList = new List<CategoryDetails>();

            try
            {
                using (var entity = new InventoryEntities())
                {
                    if (!string.IsNullOrEmpty(ActiveFlag))
                    {
                        objCategoryList = (from category in entity.M_CatMaster
                                           where category.ActiveStatus == ActiveFlag
                                           select new CategoryDetails
                                           {
                                               CategoryId = (int)category.CatId,
                                               CategoryName = category.CatName,
                                               Description = category.CatDescription,
                                               IsActive = category.ActiveStatus == "Y" ? true : false
                                           }
                                           ).ToList();
                    }
                    else
                    {
                        objCategoryList = (from category in entity.M_CatMaster
                                               // where category.ActiveStatus == "Y"
                                           select new CategoryDetails
                                           {
                                               CategoryId = (int)category.CatId,
                                               CategoryName = category.CatName,
                                               Description = category.CatDescription,
                                               IsActive = category.ActiveStatus == "Y" ? true : false
                                           }
                                          ).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return objCategoryList;
        }

        public List<SubCategoryDetails> GetSubcategoryDetails(int CategoryId, string ActiveStatus)
        {
            List<SubCategoryDetails> objSubCategoryDetails = new List<SubCategoryDetails>();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    if (!string.IsNullOrEmpty(ActiveStatus))
                    {
                        if (CategoryId != 0)
                        {
                            objSubCategoryDetails = (from c in entity.M_CatMaster
                                                         //join sc in entity.SubCategoryMasters on c.CategoryId equals sc.ParentCategoryId
                                                     join sc in entity.M_SubCatMaster on c.CatId equals sc.CatId
                                                     where sc.CatId == CategoryId && sc.ActiveStatus == ActiveStatus && c.ActiveStatus == "Y"
                                                     select new SubCategoryDetails
                                                     {
                                                         SubCategoryId = (int)sc.AId,
                                                         SubCatId = (int)sc.SubCatId,
                                                         CategoryId = (int)sc.CatId,
                                                         CategoryName = c.CatName,
                                                         IsActive = sc.ActiveStatus == "Y" ? true : false,
                                                         Description = sc.Description,
                                                         subCategoryName = sc.SubCatName
                                                     }).ToList();
                        }
                        else
                        {
                            objSubCategoryDetails = (from c in entity.M_CatMaster
                                                         //join sc in entity.SubCategoryMasters on c.CategoryId equals sc.ParentCategoryId
                                                     join sc in entity.M_SubCatMaster on c.CatId equals sc.CatId
                                                     where sc.ActiveStatus == ActiveStatus && c.ActiveStatus=="Y"
                                                     select new SubCategoryDetails
                                                     {
                                                         SubCategoryId = (int)sc.AId,
                                                         SubCatId = (int)sc.SubCatId,
                                                         CategoryId = (int)sc.CatId,
                                                         CategoryName = c.CatName,
                                                         Description = sc.Description,
                                                         IsActive = sc.ActiveStatus == "Y" ? true : false,
                                                         subCategoryName = sc.SubCatName
                                                     }).ToList();
                        }
                    }
                    else
                    {
                        if (CategoryId != 0)
                        {
                            objSubCategoryDetails = (from c in entity.M_CatMaster
                                                         //join sc in entity.SubCategoryMasters on c.CategoryId equals sc.ParentCategoryId
                                                     join sc in entity.M_SubCatMaster on c.CatId equals sc.CatId
                                                     where sc.CatId == CategoryId && c.ActiveStatus == "Y"
                                                     select new SubCategoryDetails
                                                     {
                                                         SubCategoryId = (int)sc.AId,
                                                         SubCatId = (int)sc.SubCatId,
                                                         CategoryId = (int)sc.CatId,
                                                         CategoryName = c.CatName,
                                                         IsActive = sc.ActiveStatus == "Y" ? true : false,
                                                         subCategoryName = sc.SubCatName,
                                                         Description = sc.Description,
                                                     }).ToList();
                        }
                        else
                        {
                            objSubCategoryDetails = (from c in entity.M_CatMaster
                                                         //join sc in entity.SubCategoryMasters on c.CategoryId equals sc.ParentCategoryId
                                                     join sc in entity.M_SubCatMaster on c.CatId equals sc.CatId
                                                     // where sc.ActiveStatus == ActiveStatus
                                                     where c.ActiveStatus == "Y"
                                                     select new SubCategoryDetails
                                                     {
                                                         SubCategoryId = (int)sc.AId,
                                                         SubCatId = (int)sc.SubCatId,
                                                         CategoryId = (int)sc.CatId,
                                                         CategoryName = c.CatName,
                                                         IsActive = sc.ActiveStatus == "Y" ? true : false,
                                                         subCategoryName = sc.SubCatName,
                                                         Description = sc.Description
                                                     }).ToList();
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return objSubCategoryDetails;
        }

        public int MaxProductCode()
        {
            decimal maxCode = 1000;
            try
            {
                using (var entity = new InventoryEntities())
                {
                    maxCode = (from result in entity.M_ProductMaster where result.PType!="K" select result.ProductCode).DefaultIfEmpty(0).Max();

                }

            }
            catch (Exception ex)
            {

            }
            if (maxCode == 0)
            {
                maxCode = 1000;
            }
            return ((int)maxCode + 1);
        }

        public int MaxKitProductCode()
        {
            decimal maxCode = 9000;
            try
            {
                using (var entity = new InventoryEntities())
                {
                    maxCode = (from result in entity.M_ProductMaster where result.PType=="K" select result.ProductCode).DefaultIfEmpty(0).Max();

                }

            }
            catch (Exception ex)
            {

            }
            if (maxCode == 0)
            {
                maxCode = 9000;
            }
            return ((int)maxCode + 1);
        }

        public int MaxBarCode()
        {
            decimal maxCode = 1000000;
            string maxCodeStr = "0";
            try
            {
                using (var entity = new InventoryEntities())
                {
                    maxCode = (from result in entity.M_BarCodeMaster where result.BType == "W" select result.BCode).DefaultIfEmpty(1000000).Max();

                }

            }
            catch (Exception ex)
            {

            }
            //if (!string.IsNullOrEmpty(maxCodeStr)&& maxCodeStr!="0")
            //{
            //maxCode = decimal.Parse(maxCodeStr);
            //}
            if (maxCode == 0)
            {
                maxCode = 1000000;
            }
            return ((int)maxCode + 1);
        }

        public ResponseDetail SaveProductMaster(ProductDetails model)
        {
            ResponseDetail objResponse = new ResponseDetail();
            M_ProductMaster objDTProduct = new M_ProductMaster();
            M_BarCodeMaster objDTBarcode = new M_BarCodeMaster();
            M_TaxMaster objDTTax = new M_TaxMaster();
            TrnStockJv objDtStock = new TrnStockJv();
            Im_CurrentStock objDtCurrentStock = new Im_CurrentStock();
            string objversion = "";
            M_FiscalMaster objFiscalMaster = new M_FiscalMaster();
            int i = 0;
            // decimal ProductBarcodeId = 0;
            //decimal ProductTaxId = 0 ;
            decimal BatchCode = 1000000;
            decimal StockJNo = 1001;
            objResponse.ResponseMessage = "Something went wrong!";
            objResponse.ResponseStatus = "FAILED";
            try
            {
                using (var entity = new InventoryEntities())
                {
                    if (model != null)
                    {
                        objFiscalMaster = (from result in entity.M_FiscalMaster where result.ActiveStatus == "Y" select result).FirstOrDefault();
                        objDTProduct = (from result in entity.M_ProductMaster where result.ProductCode == model.ProductCode select result).FirstOrDefault();
                        objversion = (from result in entity.M_NewHOVersionInfo select result.VersionNo).FirstOrDefault();
                        if (objDTProduct == null)
                        {
                            objDTProduct = new M_ProductMaster();
                            model.ProductCode = MaxProductCode();
                        }

                        i = 0;
                       
                        objDTProduct.ProductCode = Convert.ToDecimal(model.UserDefinedCode);
                        objDTProduct.ProductName = model.ProductName;
                        objDTProduct.ProdId = model.ProductCode.ToString();
                        objDTProduct.PV = model.PV;
                        objDTProduct.RP = model.RP;
                        objDTProduct.SpclOffer = !string.IsNullOrEmpty(model.SpecialOffer) ? model.SpecialOffer : "";
                        objDTProduct.HotSell = !string.IsNullOrEmpty(model.HotSell) ? model.HotSell : "";
                        objDTProduct.ImagePath = string.IsNullOrEmpty(model.ProductImagePath) ? "" : model.ProductImagePath;
                        objDTProduct.IsImage = string.IsNullOrEmpty(model.ProductImagePath) ? "N" : "Y";
                        objDTProduct.ActiveStatus = model.IsActive ? "Y" : "N";
                        objDTProduct.OnWebSite = model.OnWebsite;
                        objDTProduct.MsgText = string.IsNullOrEmpty(model.Message)?"":model.Message;
                        objDTProduct.MsgStatus = !string.IsNullOrEmpty(model.MessageStatus) ? model.MessageStatus : "";
                        // objDTProduct.MinQty = model.MinQty;
                        objDTProduct.IMEINo = model.MinQty.ToString();
                        objDTProduct.ProdCommssn = model.ProductCommission;
                        objDTProduct.Discount = model.DiscountPer;
                        objDTProduct.DiscInRs = model.DiscountInRs;
                        objDTProduct.UserProdId = string.IsNullOrEmpty(model.UserDefinedCode)?"":model.UserDefinedCode;
                        objDTProduct.SubCatId = model.SubCatgeoryId;
                        objDTProduct.CatId = model.CategoryId;
                        objDTProduct.ProductDesc = model.ProductDescription;
                        objDTProduct.CV = model.CV;
                        objDTProduct.BV = model.BV;
                        objDTProduct.HSNCode = string.IsNullOrEmpty(model.HSNCode) ? "" : model.HSNCode; 
                        if (model.UserDetails != null)
                        {
                            objDTProduct.UserId = model.UserDetails.UserId;
                        }
                        objDTProduct.RecTimeStamp = DateTime.Now;

                        objDTProduct.GenerateBy = model.UserDetails.PartyCode;
                        objDTProduct.BrandCode = 0;
                        objDTProduct.ProductType = "P";
                        objDTProduct.Prefix = "";
                        objDTProduct.ItemType = "";
                        objDTProduct.BuyingTax = 0;
                        objDTProduct.Weight = model.Weight;
                        objDTProduct.PurchaseRate = model.ProductBarcodeDetails.PurchaseRate;
                        objDTProduct.DP1 = 0;
                        objDTProduct.OtherStateDP = 0;
                        objDTProduct.Exp = 0;
                        objDTProduct.Costing = 0;
                        objDTProduct.FundPoint = 0;
                        if (model.DiscountPer > 0 || model.DiscountInRs > 0)
                        {
                            objDTProduct.IsDiscount = "Y";
                        }
                        else if (model.DiscountPer > 0 && model.DiscountInRs == 0)
                        {
                            objDTProduct.IsDiscount = "Y";
                        }
                        else if (model.DiscountPer == 0 && model.DiscountInRs > 0)
                        {
                            objDTProduct.IsDiscount = "Y";
                        }
                        else
                        {
                            objDTProduct.IsDiscount = "N";
                        }
                        objDTProduct.VDiscount = 0;
                        objDTProduct.GRate = 0;
                        objDTProduct.GMCharge = 0;
                        objDTProduct.GMType = "";
                        objDTProduct.IsCardIssue = "N";
                        objDTProduct.Remarks = "";
                        objDTProduct.TaxType = "I";
                        decimal BarcodeId = (from result in entity.M_BarCodeMaster select result.BId).DefaultIfEmpty(1000000).Max();
                        BarcodeId = BarcodeId + 1;
                        objDTProduct.BId = BarcodeId;
                        objDTProduct.Imported = "N";
                        objDTProduct.BNo = "0";
                        objDTProduct.PType = "G";
                        if (model.ProductBarcodeDetails.BarcodeType == "System Generated")
                        { objDTProduct.BarcodeType = "W"; }
                        else
                        {
                            objDTProduct.BarcodeType = "O";
                        }

                        objDTProduct.BCode = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? decimal.Parse(model.ProductBarcodeDetails.Barcode) : 0;
                        objDTProduct.Barcode = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                        objDTProduct.OpStockQty = model.ProductCurrentStockDetails.OpeningStockQty;
                        objDTProduct.LastModified = "";
                        objDTProduct.Val1 = 0;
                        objDTProduct.Val2 = 0;
                        objDTProduct.Company = "U";
                        objDTProduct.PurchaseFrom = "WR";
                        objDTProduct.PurchaseStore = "WR";
                        objDTProduct.IsFlexible = "N";
                        objDTProduct.IsForPC = "N";
                        objDTProduct.SubQty = 0;
                        objDTProduct.CalcKitRate = "N";
                        objDTProduct.FlexiQty = 0;
                        objDTProduct.ImgPath1 = "";
                        objDTProduct.ImgPath2 = "";
                        objDTProduct.ImgPath3 = "";
                        objDTProduct.ImgPath4 = "";
                        objDTProduct.AlterID = model.UserDetails.UserId;
                        // objDTProduct.UnitID = 3;
                        // objDTProduct.UnitName = "Pc.";
                        objDTProduct.MRP = model.ProductBarcodeDetails.MRP;
                        objDTProduct.DP = model.ProductBarcodeDetails.DP;
                        objDTProduct.IsExpired = model.ProductBarcodeDetails.IsExpirable;
                        DateTime MfgDate = DateTime.Now;
                        DateTime ExpDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(model.ProductBarcodeDetails.MfgDateStr))
                        {

                            var SplitDate = model.ProductBarcodeDetails.MfgDateStr.Split('-');                          
                            string NewDate = (SplitDate[1].Length == 1 ? "0" + SplitDate[1] : SplitDate[1]) + "/" + (SplitDate[0].Length == 1 ? "0" + SplitDate[0] : SplitDate[0]) + "/" + SplitDate[2];
                            MfgDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                            MfgDate = MfgDate.Date;
                        }
                        if (!string.IsNullOrEmpty(model.ProductBarcodeDetails.ExpDateStr))
                        {

                            var SplitDate = model.ProductBarcodeDetails.ExpDateStr.Split('-');                            
                            string NewDate = (SplitDate[1].Length == 1 ? "0" + SplitDate[1] : SplitDate[1]) + "/" + (SplitDate[0].Length == 1 ? "0" + SplitDate[0] : SplitDate[0]) + "/" + SplitDate[2];
                            ExpDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                            ExpDate = ExpDate.Date;
                        }
                        objDTProduct.MfgDate = MfgDate.Date;
                        objDTProduct.ExpDate = ExpDate.Date;
                        // objDTProduct.ProductBarcodeId = ProductBarcodeId;
                        // objDTProduct.ProductTaxId = ProductTaxId;
                        bool isBarcodeMasterSave = true, isTaxMasterSave = true;
                        objDTProduct.OrderDesc = "";
                        objDTProduct.GenerateBy = model.UserDetails.UserName;
                        objDTProduct.HSNCode = "";
                        entity.M_ProductMaster.Add(objDTProduct);
                        try
                        {
                            i = entity.SaveChanges();

                            if (i > 0)
                            {
                                if (model.ProductBarcodeDetails != null)
                                {
                                    objDTBarcode.BId = BarcodeId;
                                    objDTBarcode.SupplierCode = "0";
                                    objDTBarcode.BCode = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? decimal.Parse(model.ProductBarcodeDetails.Barcode) : 0;
                                    objDTBarcode.Company = "";
                                    objDTBarcode.Imported = "N";
                                    objDTBarcode.LastModified = "";
                                    objDTBarcode.DP1 = 0;
                                    objDTBarcode.Val1 = 0;
                                    objDTBarcode.Val2 = 0;


                                    objDTBarcode.BarCode = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    objDTBarcode.BatchNo = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    objDTBarcode.BarCodeType = model.ProductBarcodeDetails.BarcodeType;
                                    objDTBarcode.DP = model.ProductBarcodeDetails.DP;
                                    if (model.ProductBarcodeDetails.IsExpirable == "Y")
                                        objDTBarcode.ExpDate = ExpDate.Date;
                                    else
                                    {
                                        objDTBarcode.ExpDate = DateTime.Now;
                                    }
                                    objDTBarcode.MfgDate = MfgDate.Date;
                                    objDTBarcode.MRP = model.ProductBarcodeDetails.MRP;
                                    objDTBarcode.IsExpired = model.ProductBarcodeDetails.IsExpirable;
                                    objDTBarcode.ProdId = model.ProductCode.ToString();
                                    objDTBarcode.PRate = model.ProductBarcodeDetails.PurchaseRate;
                                    objDTBarcode.Remarks = string.IsNullOrEmpty(model.ProductBarcodeDetails.Remarks) ? "" : model.ProductBarcodeDetails.Remarks;
                                    objDTBarcode.ActiveStatus = model.IsActive ? "Y" : "N";
                                    objDTBarcode.GenerateDate = DateTime.Now;
                                    if (model.UserDetails != null)
                                    {
                                        objDTBarcode.GenerateBy = model.UserDetails.UserName;
                                        objDTBarcode.UserId = model.UserDetails.UserId;
                                        //objDTBarcode.AlterID = model.UserDetails.UserId;
                                    }
                                    objDTBarcode.RecTimeStamp = DateTime.Now;

                                    if (model.ProductBarcodeDetails.BarcodeType == "System Generated")
                                    { objDTBarcode.BType = "W"; }
                                    else
                                    {
                                        objDTBarcode.BType = "O";
                                    }

                                    //BatchCode = (from result in entity.BarcodeMasters select result.BatchCode).Max()??1000000;

                                    //objDTBarcode.BatchCode = BatchCode + 1;
                                    objDTBarcode.BatchNo = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    BatchCode = (!string.IsNullOrEmpty(objDTBarcode.BatchNo)) ? decimal.Parse(objDTBarcode.BatchNo) : 1000001;
                                    entity.M_BarCodeMaster.Add(objDTBarcode);
                                    try
                                    {
                                        i = entity.SaveChanges();
                                        if (i > 0)
                                        {
                                            //ProductBarcodeId = (from result in entity.M_BarCodeMaster select result.AId).Max();
                                            isBarcodeMasterSave = true;
                                        }
                                        else
                                        {
                                            isBarcodeMasterSave = false;
                                        }
                                    }
                                    catch (DbEntityValidationException ex)
                                    {

                                    }
                                }

                                if (model.ProductTaxDetails != null)
                                {
                                    objDTTax.WithCForm = 0;
                                    objDTTax.CstTax = 0;
                                    objDTTax.ActiveStatus = model.IsActive ? "Y" : "N";
                                    objDTTax.VatTax = model.ProductTaxDetails.GSTTax;
                                    objDTTax.ProdName = model.ProductName;
                                    objDTTax.Imported = "N";
                                    objDTTax.LastModified = DateTime.Now.ToString();
                                    objDTTax.Remarks = "";
                                    objDTTax.Company = "";
                                    //objDTTax.GeneratedDate = DateTime.Now;
                                    objDTTax.RecTimeStamp = DateTime.Now;

                                    objDTTax.AValue = 0;
                                    objDTTax.STax = 0;
                                    if (model.UserDetails != null)
                                    {
                                        // objDTTax.StateCode = model.UserDetails.StateCode;
                                        objDTTax.StateCode = (int)(from r in entity.M_CompanyMaster select r.CompState).FirstOrDefault();
                                        objDTTax.UserId = model.UserDetails.UserId;
                                        objDTTax.GenerateBy = model.UserDetails.UserName;
                                    }
                                    objDTTax.ProdCode = model.ProductCode.ToString();
                                    i = 0;
                                    entity.M_TaxMaster.Add(objDTTax);
                                    try
                                    {
                                        i = entity.SaveChanges();
                                        if (i > 0)
                                        {
                                            //ProductTaxId = (from result in entity.M_TaxMaster select result.AId).Max();
                                            isTaxMasterSave = true;
                                        }
                                        else
                                        {
                                            isTaxMasterSave = false;
                                        }
                                    }
                                    catch (DbEntityValidationException ex)
                                    {

                                    }
                                }


                                // i = 0;
                                objDtStock.Barcode = model.ProductBarcodeDetails.Barcode;
                                objDtStock.BatchNo = BatchCode.ToString();
                                if (model.UserDetails != null)
                                {
                                    objDtStock.UserId = model.UserDetails.UserId;
                                    objDtStock.UserName = model.UserDetails.UserName;
                                }
                                objDtStock.RecTimeStamp = DateTime.Now;
                                if (objFiscalMaster != null)
                                    objDtStock.FSessId = objFiscalMaster.FSessId;
                                else
                                    objDtStock.FSessId = 0;
                                var res = (from result in entity.TrnStockJvs select result).ToList();
                                if (res.Count == 0)
                                    objDtStock.JNo = 1001;
                                else
                                {
                                    decimal MaxJNo = (from r in res select r.JNo).DefaultIfEmpty(0).Max();
                                    objDtStock.JNo = MaxJNo + 1;
                                }
                                objDtStock.JvNo = "OPN/" + objDtStock.JNo;
                                StockJNo = objDtStock.JNo == 0 ? 1001 : objDtStock.JNo;
                                objDtStock.ProdId = model.ProductCode.ToString();
                                objDtStock.ProductName = (from result in entity.M_ProductMaster where result.ProductCode == model.ProductCode select result.ProductName).FirstOrDefault() ?? "";
                                objDtStock.ProdType = "P";
                                objDtStock.Qty = model.ProductCurrentStockDetails.OpeningStockQty;
                                objDtStock.Remarks = "Opening Stock Of Product Registration";
                                objDtStock.RefNo = objDtStock.JvNo;
                                objDtStock.JType = "O";
                                objDtStock.ActiveStatus = model.IsActive ? "Y" : "N";
                                objDtStock.Version = objversion;
                                if (model.UserDetails != null)
                                {
                                    objDtStock.PartyName = model.UserDetails.PartyName;
                                    objDtStock.FCode = model.UserDetails.FCode;
                                    objDtStock.SoldBy = model.UserDetails.FCode;
                                }

                                objDtStock.StockDate = DateTime.Now;
                                try
                                {
                                    entity.TrnStockJvs.Add(objDtStock);
                                    i = 0;
                                    i = entity.SaveChanges();
                                    if (i > 0)
                                    {

                                        //if (model.UserDetails != null)
                                        //{
                                        //    objDtCurrentStock.FCode = model.UserDetails.FCode;
                                        //}
                                        //if (objFiscalMaster != null)
                                        //{
                                        //    objDtCurrentStock.FSessId = objFiscalMaster.FSessId;
                                        //}
                                        //objDtCurrentStock.ActiveStatus = model.IsActive ? "Y" : "N";
                                        //objDtCurrentStock.ProdId = model.ProductCode.ToString();
                                        //objDtCurrentStock.ProdType = "P";
                                        //objDtCurrentStock.Qty = model.ProductCurrentStockDetails.OpeningStockQty;
                                        //objDtCurrentStock.RefNo = "OPN/" + StockJNo;
                                        //objDtCurrentStock.SupplierCode = model.UserDetails.FCode;
                                        //objDtCurrentStock.Version = objversion;
                                        //objDtCurrentStock.Remarks = "Opening Stock For Product Registration";
                                        //objDtCurrentStock.EntryBy = objDtCurrentStock.FCode;
                                        //objDtCurrentStock.RecTimeStamp = DateTime.Now;
                                        //objDtCurrentStock.BillType = "OP";
                                        //objDtCurrentStock.BType = "OP";
                                        //objDtCurrentStock.SType = "I";
                                        //objDtCurrentStock.StockFor = "Gen.Thr.TRG.OpBl";
                                        //objDtCurrentStock.IsDisp = "N";
                                        //objDtCurrentStock.InvoiceNo = "";
                                        //objDtCurrentStock.Barcode = model.ProductBarcodeDetails.Barcode;
                                        //objDtCurrentStock.BatchCode = BatchCode.ToString();
                                        //if (model.UserDetails != null)
                                        //{
                                        //    objDtCurrentStock.UserId = model.UserDetails.UserId;
                                        //    objDtCurrentStock.GroupId = model.UserDetails.GroupId;
                                        //}
                                        //objDtCurrentStock.StockDate = DateTime.Now;


                                        //entity.Im_CurrentStock.Add(objDtCurrentStock);
                                        //i = 0;
                                        //i = entity.SaveChanges();

                                        objResponse.ResponseStatus = "OK";
                                        objResponse.ResponseMessage = "Saved Successfully!";

                                    }
                                    else
                                    {
                                        objResponse.ResponseStatus = "FAILED";
                                        objResponse.ResponseMessage = "Something went wrong!";
                                    }

                                }
                                catch (DbEntityValidationException e)
                                {
                                    foreach (var eve in e.EntityValidationErrors)
                                    {
                                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                        foreach (var ve in eve.ValidationErrors)
                                        {
                                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                                ve.PropertyName, ve.ErrorMessage);
                                        }
                                    }
                                }
                            }
                            
                        }
                        catch (DbEntityValidationException ex)
                        {

                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objResponse;
        }
        public ResponseDetail EditProductMaster(ProductDetails model)
        {
            ResponseDetail objResponse = new ResponseDetail();
            M_ProductMaster objDTProduct = new M_ProductMaster();
            M_BarCodeMaster objDTBarcode = new M_BarCodeMaster();
            M_TaxMaster objDTTax = new M_TaxMaster();
            TrnStockJv objDTStock = new TrnStockJv();
            Im_CurrentStock objCurrentStock = new Im_CurrentStock();
            objResponse.ResponseMessage = "Something Went Wrong!";
            objResponse.ResponseStatus = "FAILED";
            int i = 0;
            try
            {
                using (var entity = new InventoryEntities())
                {
                    objDTProduct = (from p in entity.M_ProductMaster where p.ProductCode == model.ProductCode select p).FirstOrDefault();
                    if (objDTProduct != null)
                    {
                        if (model != null)
                        {
                            i = 0;
                            objDTProduct.ProductCode = model.ProductCode;
                            objDTProduct.ProductName = model.ProductName;
                            objDTProduct.ProdId = model.ProductCode.ToString();
                            objDTProduct.PV = model.PV;
                            objDTProduct.RP = model.RP;
                            objDTProduct.SpclOffer = string.IsNullOrEmpty(model.SpecialOffer) ? "" : model.SpecialOffer;
                            objDTProduct.HotSell = string.IsNullOrEmpty(model.HotSell) ? "" : model.HotSell;
                            objDTProduct.ImagePath = string.IsNullOrEmpty(model.ProductImagePath) ? "" : model.ProductImagePath;
                            objDTProduct.IsImage = string.IsNullOrEmpty(model.ProductImagePath) ? "N" : "Y";
                            objDTProduct.ActiveStatus = model.IsActive ? "Y" : "N";
                            objDTProduct.OnWebSite = model.OnWebsite;
                            objDTProduct.MsgText = string.IsNullOrEmpty(model.Message) ? "" : model.Message;
                            objDTProduct.MsgStatus = string.IsNullOrEmpty(model.MessageStatus) ? "" : model.MessageStatus;
                            objDTProduct.Weight = model.Weight;
                           objDTProduct.IMEINo = model.MinQty.ToString();
                            objDTProduct.ProdCommssn = model.ProductCommission;
                            objDTProduct.Discount = model.DiscountPer;
                            objDTProduct.DiscInRs = model.DiscountInRs;
                            objDTProduct.UserProdId = string.IsNullOrEmpty(model.UserDefinedCode) ? "" : model.UserDefinedCode;
                            objDTProduct.SubCatId = model.SubCatgeoryId;
                            objDTProduct.CatId = model.CategoryId;
                            objDTProduct.ProductDesc = model.ProductDescription;
                            objDTProduct.CV = model.CV;
                            objDTProduct.BV = model.BV;
                            objDTProduct.HSNCode = string.IsNullOrEmpty(model.HSNCode) ? "" : model.HSNCode;
                            objDTProduct.GenerateBy = model.UserDetails.PartyCode;

                            objDTProduct.PurchaseRate = model.ProductBarcodeDetails.PurchaseRate;

                            if (model.DiscountPer > 0 || model.DiscountInRs > 0)
                            {
                                objDTProduct.IsDiscount = "Y";
                            }
                            else if (model.DiscountPer > 0 && model.DiscountInRs == 0)
                            {
                                objDTProduct.IsDiscount = "Y";
                            }
                            else if (model.DiscountPer == 0 && model.DiscountInRs > 0)
                            {
                                objDTProduct.IsDiscount = "Y";
                            }
                            else
                            {
                                objDTProduct.IsDiscount = "N";
                            }


                            //if (model.ProductBarcodeDetails.BarcodeType == "System Generated")
                            //{ objDTProduct.BarcodeType = "W"; }
                            //else
                            //{
                            //    objDTProduct.BarcodeType = "O";
                            //}

                            //objDTProduct.BCode = (string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? decimal.Parse(model.ProductBarcodeDetails.Barcode) : 0;
                            // objDTProduct.Barcode = (string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                            // objDTProduct.OpStockQty = model.ProductCurrentStockDetails.OpeningStockQty;
                            if (model.UserDetails != null)
                            {
                                objDTProduct.LastModified = model.UserDetails.UserName;
                            }


                            objDTProduct.AlterID = model.UserDetails.UserId;

                            objDTProduct.MRP = model.ProductBarcodeDetails.MRP;
                            objDTProduct.DP = model.ProductBarcodeDetails.DP;
                            objDTProduct.IsExpired = model.ProductBarcodeDetails.IsExpirable;
                            DateTime MfgDate = DateTime.Now;
                            DateTime ExpDate = DateTime.Now;
                            if (!string.IsNullOrEmpty(model.ProductBarcodeDetails.MfgDateStr))
                            {
                                var SplitDate = model.ProductBarcodeDetails.MfgDateStr.Split('-');
                                string NewDate = (SplitDate[1].Length == 1 ? "0" + SplitDate[1] : SplitDate[1]) + "/" + (SplitDate[0].Length == 1 ? "0" + SplitDate[0] : SplitDate[0]) + "/" + SplitDate[2];
                                MfgDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                                MfgDate = MfgDate.Date;
                            }
                            if (!string.IsNullOrEmpty(model.ProductBarcodeDetails.ExpDateStr))
                            {
                                var SplitDate = model.ProductBarcodeDetails.ExpDateStr.Split('-');
                                string NewDate = (SplitDate[1].Length == 1 ? "0" + SplitDate[1] : SplitDate[1]) + "/" + (SplitDate[0].Length == 1 ? "0" + SplitDate[0] : SplitDate[0]) + "/" + SplitDate[2];
                                ExpDate = Convert.ToDateTime(DateTime.ParseExact(NewDate, "MM/dd/yyyy", CultureInfo.InvariantCulture));
                                ExpDate = ExpDate.Date;
                            }
                            objDTProduct.MfgDate = MfgDate.Date;
                            objDTProduct.ExpDate = ExpDate.Date;
                            i = entity.SaveChanges();
                            //if (i > 0)
                            //{
                                //barcode details
                                string ProductCodeStr = model.ProductCode.ToString();
                                objDTBarcode = (from b in entity.M_BarCodeMaster where b.ProdId == ProductCodeStr && b.BarCode == model.ProductBarcodeDetails.ExisitingBarcode select b).FirstOrDefault();
                                if (objDTBarcode != null)
                                {
                                    //objDTBarcode.BCode = (string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? decimal.Parse(model.ProductBarcodeDetails.Barcode) : 0;
                                    if (model.UserDetails != null)
                                    {
                                        objDTBarcode.LastModified = model.UserDetails.UserName;
                                    }

                                    //objDTBarcode.BarCode = (string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    // objDTBarcode.BatchNo = (string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    // objDTBarcode.BarCodeType = model.ProductBarcodeDetails.BarcodeType;
                                    objDTBarcode.DP = model.ProductBarcodeDetails.DP;
                                    if (model.ProductBarcodeDetails.IsExpirable == "Y")
                                        objDTBarcode.ExpDate = ExpDate.Date;
                                    else
                                    {
                                        objDTBarcode.ExpDate = DateTime.Now;
                                    }
                                    objDTBarcode.MfgDate = MfgDate.Date;
                                    objDTBarcode.MRP = model.ProductBarcodeDetails.MRP;
                                    objDTBarcode.IsExpired = model.ProductBarcodeDetails.IsExpirable;
                                    objDTBarcode.ProdId = model.ProductCode.ToString();
                                    objDTBarcode.PRate = model.ProductBarcodeDetails.PurchaseRate;
                                    objDTBarcode.Remarks = string.IsNullOrEmpty(model.ProductBarcodeDetails.Remarks) ? "" : model.ProductBarcodeDetails.Remarks;
                                    objDTBarcode.ActiveStatus = model.IsActive ? "Y" : "N";
                                    if (model.ProductBarcodeDetails.BarcodeType == "System Generated")
                                    { objDTBarcode.BType = "W"; }
                                    else
                                    {
                                        objDTBarcode.BType = "O";
                                    }

                                    //BatchCode = (from result in entity.BarcodeMasters select result.BatchCode).Max()??1000000;

                                    //objDTBarcode.BatchCode = BatchCode + 1;
                                    // objDTBarcode.BatchNo = (string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    i = 0;
                                    i = entity.SaveChanges();
                                    //if (i > 0)
                                    //{
                                        //tax details
                                        //string ProductCodeStr = model.ProductCode.ToString();
                                        if (model.ProductTaxDetails != null)
                                        {
                                            objDTTax = (from t in entity.M_TaxMaster where t.ProdCode == ProductCodeStr  select t).FirstOrDefault();//&& t.StateCode == model.UserDetails.StateCode
                                    if (objDTTax != null)
                                            {
                                                objDTTax.ActiveStatus = model.IsActive ? "Y" : "N";
                                                objDTTax.VatTax = model.ProductTaxDetails.GSTTax;
                                                objDTTax.ProdName = model.ProductName;
                                                objDTTax.LastModified = DateTime.Now.ToString();
                                                if (model.UserDetails != null)
                                                {
                                                   // objDTTax.StateCode = model.UserDetails.StateCode;
                                                    objDTTax.UserId = model.UserDetails.UserId;
                                                    objDTTax.GenerateBy = model.UserDetails.UserName;
                                                }
                                                objDTTax.ProdCode = model.ProductCode.ToString();
                                                i = 0;
                                                i = entity.SaveChanges();
                                                //if (i > 0)
                                                //{
                                                    //objResponse.ResponseStatus = "OK";
                                                    //objResponse.ResponseMessage = "Updated Succesfully!";
                                                    objDTStock = (from s in entity.TrnStockJvs where s.ProdId == ProductCodeStr && s.JType == "O" select s).FirstOrDefault();
                                                    if (objDTStock != null)
                                                    {
                                                        //objDTStock.Barcode = model.ProductBarcodeDetails.Barcode;
                                                        // objDTStock.BatchNo = objDTStock.Barcode.ToString();
                                                        objDTStock.ActiveStatus = model.IsActive ? "Y" : "N";
                                                        objDTStock.ProductName = (from result in entity.M_ProductMaster where result.ProductCode == model.ProductCode select result.ProductName).FirstOrDefault() ?? "";
                                                        if (model.UserDetails != null)
                                                        {
                                                            objDTStock.PartyName = model.UserDetails.PartyName;
                                                            objDTStock.FCode = model.UserDetails.FCode;
                                                            objDTStock.SoldBy = model.UserDetails.FCode;
                                                        }

                                                        // objDTStock.StockDate = DateTime.Now;
                                                        i = 0;
                                                        i = entity.SaveChanges();
                                                        //if (i > 0)
                                                        //{
                                                            //objCurrentStock = (from c in entity.Im_CurrentStock where c.ProdId == ProductCodeStr && c.BType == "O" select c).FirstOrDefault();
                                                            //if (objCurrentStock != null)
                                                            //{
                                                            //    if (model.UserDetails != null)
                                                            //    {
                                                            //        objCurrentStock.FCode = model.UserDetails.FCode;
                                                            //    }

                                                            //    objCurrentStock.ActiveStatus = model.IsActive ? "Y" : "N";
                                                            //    objCurrentStock.ProdId = model.ProductCode.ToString();

                                                            //    //objCurrentStock.Qty = model.ProductCurrentStockDetails.OpeningStockQty;

                                                            //    objCurrentStock.SupplierCode = model.UserDetails.FCode;


                                                            //    objCurrentStock.EntryBy = objCurrentStock.FCode;

                                                            //    //objCurrentStock.Barcode = model.ProductBarcodeDetails.Barcode;
                                                            //    // objCurrentStock.BatchCode = objCurrentStock.Barcode.ToString();
                                                            //    if (model.UserDetails != null)
                                                            //    {
                                                            //        objCurrentStock.UserId = model.UserDetails.UserId;
                                                            //        objCurrentStock.GroupId = model.UserDetails.GroupId;
                                                            //    }

                                                            //    i = 0;
                                                            //    i = entity.SaveChanges();

                                                            objResponse.ResponseStatus = "OK";
                                                            objResponse.ResponseMessage = "Updated Successfully!";
                                                        //}
                                                        //objResponse.ResponseStatus = "OK";
                                                        //objResponse.ResponseMessage = "Updated Successfully!";
                                                        //else
                                                        //{
                                                        //    objResponse.ResponseStatus = "FAILED";
                                                        //    objResponse.ResponseMessage = "Something went wrong!";
                                                        //}
                                                    }
                                                }
                                            }
                                        //}
                                        //else
                                        //{
                                        //    //objResponse.ResponseStatus = "FAILED";
                                        //    //objResponse.ResponseMessage = "Something went wrong!";
                                        //}
                                        //}
                                    }
                            //}
                            objResponse.ResponseStatus = "OK";
                            objResponse.ResponseMessage = "Updated Successfully!";
                        }
                            }

                        }
                    //}
               // }
            }

            catch (Exception ex)
            {
                objResponse.ResponseStatus = "FAILED";
                objResponse.ResponseMessage = "Something went wrong.";
            }
            return objResponse;
        }
        public ResponseDetail DeleteProductMaster(ProductDetails model)
        {
            ResponseDetail objResponse = new ResponseDetail();
            M_ProductMaster objDTProduct = new M_ProductMaster();
            M_BarCodeMaster objDTBarcode = new M_BarCodeMaster();
            M_TaxMaster objDTTax = new M_TaxMaster();
            TrnStockJv objDtStock = new TrnStockJv();
            Im_CurrentStock objDtCurrentStock = new Im_CurrentStock();
            int i = 0;
            try
            {
                using (var entity = new InventoryEntities())
                {
                    objDTProduct = (from p in entity.M_ProductMaster where p.ProductCode == model.ProductCode select p).FirstOrDefault();
                    if (objDTProduct != null)
                    {
                        objDTProduct.ActiveStatus = "N";
                        i = 0;
                        i = entity.SaveChanges();
                        if (i > 0)
                        {
                            string ProductCodeStr = model.ProductCode.ToString();
                            objDTBarcode = (from b in entity.M_BarCodeMaster where b.ProdId == ProductCodeStr && b.BarCode == model.ProductBarcodeDetails.ExisitingBarcode select b).FirstOrDefault();
                            if (objDTBarcode != null)
                            {
                                objDTBarcode.ActiveStatus = "N";
                                i = 0;
                                i = entity.SaveChanges();
                                if (i > 0)
                                {
                                    objDTTax = (from t in entity.M_TaxMaster where t.ProdCode == ProductCodeStr select t).FirstOrDefault();// && t.StateCode == model.UserDetails.StateCode
                                    if (objDTTax != null)
                                    {
                                        objDTTax.ActiveStatus = "N";
                                        i = 0;
                                        i = entity.SaveChanges();
                                        if (i > 0)
                                        {
                                            objDtStock = (from s in entity.TrnStockJvs where s.ProdId == ProductCodeStr && s.JType == "O" select s).FirstOrDefault();
                                            if (objDtStock != null)
                                            {
                                                objDtStock.ActiveStatus = "N";
                                                i = 0;
                                                i = entity.SaveChanges();
                                                if (i > 0)
                                                {
                                                    objDtCurrentStock = (from c in entity.Im_CurrentStock where c.ProdId == ProductCodeStr && c.BType == "O" select c).FirstOrDefault();
                                                    if (objDtCurrentStock != null)
                                                    {
                                                        objDtCurrentStock.ActiveStatus = "N";
                                                        i = 0;
                                                        i = entity.SaveChanges();
                                                        if (i > 0)
                                                        {
                                                            objResponse.ResponseStatus = "OK";
                                                            objResponse.ResponseMessage = "Deleted Succesfully!";
                                                        }
                                                        else
                                                        {
                                                            objResponse.ResponseStatus = "OK";
                                                            objResponse.ResponseMessage = "Something went wrong!";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objResponse;
        }
        public List<ProductDetails> GetProductList(decimal LoginStateCode)
        {
            List<ProductDetails> objProductList = new List<ProductDetails>();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    objProductList = (from product in entity.M_ProductMaster
                                      join barcode in entity.M_BarCodeMaster on product.ProdId equals barcode.ProdId
                                      into barcodes
                                      from r in barcodes.Take(1)
                                          //join stockJv in entity.TrnStockJvs on product.ProdId equals stockJv.ProdId
                                          //join currentStock in entity.Im_CurrentStock on product.ProdId equals currentStock.ProdId
                                      join category in entity.M_CatMaster on product.CatId equals category.CatId
                                      join subcategory in entity.M_SubCatMaster on product.SubCatId equals subcategory.SubCatId
                                      from tax in entity.M_TaxMaster where tax.ProdCode== product.ProdId
                                     // where tax.StateCode==LoginStateCode
                                      //where currentStock.BillType == "OP"
                                      select new ProductDetails
                                      {

                                          ProductId = (int)product.AId,
                                          BV = product.BV,
                                          CategoryId = (int)product.CatId,
                                          ProductCategoryDetails = new CategoryDetails
                                          {
                                              CategoryId = (int)category.CatId,
                                              CategoryName = category.CatName
                                          },
                                          CV = product.CV,
                                          DiscountInRs = product.DiscInRs,
                                          DiscountPer = product.Discount,
                                          HotSell = product.HotSell,
                                          IsActive = product.ActiveStatus == "Y" ? true : false,
                                          Message = product.MsgText,
                                          MessageStatus = product.MsgStatus,
                                          Weight=product.Weight,
                                          MinQtyStr = product.IMEINo,
                                          OnWebsite = product.OnWebSite,
                                          ProductCode = (int)product.ProductCode,
                                          ProductCodeStr = product.ProdId,
                                          ProductCommission = product.ProdCommssn,
                                          ProductDescription = product.ProductDesc,
                                          ProductImagePath = product.ImagePath,
                                          ProductName = product.ProductName,
                                          PV = product.PV,
                                          RP = product.RP,
                                          SpecialOffer = product.SpclOffer,
                                          SubCatgeoryId = (int)product.SubCatId,
                                          UserDefinedCode = product.UserProdId,
                                          HSNCode=product.HSNCode,
                                          ProductBarcodeDetails = new BarcodeDetails
                                          {
                                              ExisitingBarcode = r.BarCode,
                                              Barcode = r.BarCode,
                                              BarcodeType = r.BarCodeType,
                                              BType = r.BType,
                                              DP = r.DP,
                                              ExpDate = r.ExpDate,
                                              GenerateDate = r.GenerateDate,
                                              GeneratedBy = r.GenerateBy,
                                              IsActive = r.ActiveStatus == "Y" ? true : false,
                                              IsExpirable = r.IsExpired,
                                              MfgDate = r.MfgDate,
                                              MRP = r.MRP,
                                              //ProductId=barcode.ProdId,
                                              PurchaseRate = r.PRate,
                                              Remarks = r.Remarks,
                                              UserId = (int)r.UserId
                                          },
                                          ProductCurrentStockDetails = new CurrentStockModel
                                          {
                                              OpeningStockQty = product.OpStockQty
                                          },
                                          ProductSubCategoryDetails = new SubCategoryDetails
                                          {
                                              SubCategoryId = (int)subcategory.SubCatId,
                                              subCategoryName = subcategory.SubCatName
                                          },
                                          ProductTaxDetails = new TaxDetails
                                          {
                                              GSTTax = tax.VatTax,

                                          },

                                      }

                                    ).Distinct().ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return objProductList;
        }

        public ProductDetails GetProductDetail(decimal ProductId,decimal LoginStateCode)
        {
            ProductDetails objProductList = new ProductDetails();
            try
            {
                using (var entity = new InventoryEntities())
                {
                    objProductList = (from product in entity.M_ProductMaster
                                      join barcode in entity.M_BarCodeMaster on product.ProdId equals barcode.ProdId
                                      
                                      //join stockJv in entity.TrnStockJvs on product.ProdId equals stockJv.ProdId
                                      //join currentStock in entity.Im_CurrentStock on product.ProdId equals currentStock.ProdId
                                      join category in entity.M_CatMaster on product.CatId equals category.CatId
                                      join subcategory in entity.M_SubCatMaster on product.SubCatId equals subcategory.SubCatId

                                      where product.ProductCode == ProductId
                                      from tax in entity.M_TaxMaster where tax.ProdCode== product.ProdId //&& tax.StateCode==LoginStateCode
                                      //from stockJv in entity.TrnStockJvs where stockJv.ProdId==product.ProdId
                                      //from currentStock in entity.Im_CurrentStock where currentStock.ProdId== product.ProdId
                                      select new ProductDetails
                                      {

                                          ProductId = (int)product.AId,
                                          BV = product.BV,
                                          CategoryId = (int)product.CatId,
                                          ProductCategoryDetails = new CategoryDetails
                                          {
                                              CategoryId = (int)category.CatId,
                                              CategoryName = category.CatName
                                          },
                                          CV = product.CV,
                                          DiscountInRs = product.DiscInRs,
                                          DiscountPer = product.Discount,
                                          HotSell = product.HotSell,
                                          IsActive = product.ActiveStatus == "Y" ? true : false,
                                          Message = product.MsgText,
                                          MessageStatus = product.MsgStatus,
                                          MinQtyStr = product.IMEINo,
                                          OnWebsite = product.OnWebSite,
                                          ProductCode = (int)product.ProductCode,
                                          ProductCodeStr = product.ProdId,
                                          ProductCommission = product.ProdCommssn,
                                          ProductDescription = product.ProductDesc,
                                          ProductImagePath = product.ImagePath,
                                          ProductName = product.ProductName,
                                          PV = product.PV,
                                          RP = product.RP,
                                          Weight=product.Weight,
                                          HSNCode=product.HSNCode,
                                          SpecialOffer = product.SpclOffer,
                                          SubCatgeoryId = (int)product.SubCatId,
                                          UserDefinedCode = product.UserProdId,
                                          ProductBarcodeDetails = new BarcodeDetails
                                          {
                                              ExisitingBarcode = barcode.BarCode,
                                              Barcode = barcode.BarCode,
                                              BarcodeType = barcode.BarCodeType,
                                              BType = barcode.BType,
                                              DP = barcode.DP,
                                              ExpDate = barcode.ExpDate,
                                              GenerateDate = barcode.GenerateDate,
                                              GeneratedBy = barcode.GenerateBy,
                                              IsActive = barcode.ActiveStatus == "Y" ? true : false,
                                              IsExpirable = barcode.IsExpired,
                                              MfgDate = barcode.MfgDate,
                                              MRP = barcode.MRP,
                                              //ProductId=barcode.ProdId,
                                              PurchaseRate = barcode.PRate,
                                              Remarks = barcode.Remarks,
                                              UserId = (int)barcode.UserId

                                          },
                                          ProductCurrentStockDetails = new CurrentStockModel
                                          {
                                              OpeningStockQty = product.OpStockQty
                                          },
                                          ProductSubCategoryDetails = new SubCategoryDetails
                                          {
                                              SubCategoryId = (int)subcategory.SubCatId,
                                              subCategoryName = subcategory.SubCatName
                                          },
                                          ProductTaxDetails = new TaxDetails
                                          {
                                              GSTTax = tax.VatTax,

                                          },

                                      }


                                    ).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {

            }
            return objProductList;
        }

        public List<Package> GetAllPackages()
        {
            List<Package> packageList = new List<Package>();
            try
            {
                using (var entities = new InventoryEntities())
                {
                    packageList = entities.M_KitMaster.Select(r => new Package
                    {
                        KitId = r.KitId,
                        KitName = r.KitName,
                        KitAmount = r.KitAmount,
                        ActiveStatus = r.ActiveStatus,
                        BV = r.BV
                    }).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return packageList;
        }

        public Package GetPackageDetail(int KitId)
        {
            Package objpackage = new Package();
            try
            {
                using (var entities = new InventoryEntities())
                {
                    objpackage = entities.M_KitMaster.Where(r => r.KitId == KitId).Select(r => new Package
                    {
                        KitId = r.KitId,
                        KitAmount = r.KitAmount,
                        ActiveStatus = r.ActiveStatus,
                        KitName = r.KitName,
                        KitUnit = r.KitUnit
                    }).FirstOrDefault();

                    objpackage.objProductList = new List<ProductModel>();
                    objpackage.objProductList = entities.M_KitProductDetail.Where(r => r.KitId == KitId).Select(r => new ProductModel
                    {
                        ProdId = r.ProdId,
                        Rate = r.Rate,
                        Quantity = r.Qty,
                    }).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return objpackage;
        }

        public ResponseDetail SavePackage(Package objPackage)
        {
            ResponseDetail objResponse = new ResponseDetail();
            objResponse.ResponseStatus = "Failed";
            objResponse.ResponseMessage = "Something went wrong!";
            decimal kitId = 0;
            decimal kpId = 0;
            try
            {
                using (var entities = new InventoryEntities())
                {
                    M_KitMaster objKit = new M_KitMaster();

                    if (objPackage.IsAdd == "Edit")
                    {
                        objKit = entities.M_KitMaster.Where(r => r.KitId == objPackage.KitId).FirstOrDefault();
                        var kitprod = entities.M_KitProductDetail.Where(r => r.KitId == objPackage.KitId).ToList();
                        entities.M_KitProductDetail.RemoveRange(kitprod);
                    }
                    else
                    {
                        kpId = entities.M_KitProductDetail.Select(r => r.KPId).DefaultIfEmpty(0).Max();
                        kitId = entities.M_KitMaster.Select(r => r.KitId).DefaultIfEmpty(1000).Max();
                        kitId = kitId + 1;
                    }

                    objKit.KitId = kitId;
                    objKit.KitName = objPackage.KitName;
                    objKit.KitUnit = objPackage.KitUnit;
                    objKit.KitAmount = objPackage.KitAmount;
                    objKit.ActiveStatus = objPackage.ActiveStatus;
                    objKit.UserId = objPackage.UserId;
                    objKit.UserCode = "";
                    objKit.LastModified = DateTime.Now.ToString();
                    objKit.RecTimeStamp = DateTime.Now;
                    objKit.Remarks = "";
                    entities.M_KitMaster.Add(objKit);

                    foreach (var product in objPackage.objProductList)
                    {
                        M_KitProductDetail objProduct = new M_KitProductDetail();


                        kpId = kpId + 1;
                        objProduct.KPId = kpId;
                        objProduct.KitId = kitId;
                        objProduct.ProdId = product.ProdCode;
                        objProduct.Itemcode = product.itemCode;
                        objProduct.Barcode = product.Barcode;
                        objProduct.ActiveStatus = "Y";
                        objProduct.LastModified = DateTime.Now.ToString();

                        objProduct.Rate = product.MRP ?? 0;
                        objProduct.RecTimeStatus = DateTime.Now;
                        objProduct.UserId = objPackage.UserId;
                        objProduct.Qty = Convert.ToInt16(product.Quantity);
                        entities.M_KitProductDetail.Add(objProduct);
                    }

                    ProductDetails objProd = new ProductDetails();
                    objProd.BrandCode = (int)kitId;
                    objProd.ProductName = objPackage.KitName;
                    objProd.CategoryId = 101;
                    objProd.SubCatgeoryId = 1;
                    objProd.IsActive = true;
                    objProd.ProductTaxDetails = new TaxDetails();
                    objProd.ProductTaxDetails.GSTTax = 0;
                    objProd.ProductBarcodeDetails = new BarcodeDetails();
                    objProd.ProductBarcodeDetails.BarcodeType = "System Generated";
                    objProd.ProductBarcodeDetails.Barcode = Convert.ToString(MaxBarCode());
                    objProd.ProductBarcodeDetails.MRP = objPackage.KitAmount;
                    objProd.ProductBarcodeDetails.DP = objPackage.KitAmount;
                    objProd.ProductBarcodeDetails.IsExpirable = "N";
                    objProd.ProductCategoryDetails = new CategoryDetails();
                    objProd.ProductCurrentStockDetails = new CurrentStockModel();
                    objProd.ProductCurrentStockDetails.OpeningStockQty = 0;
                    objProd.BV = 0;
                    objProd.CV = 0;
                    objProd.PV = objPackage.objProduct.TotalPV;
                    objProd.RP = 0;
                    objProd.DiscountPer = 0;
                    objProd.DiscountInRs = 0;
                    objProd.ProductCommission = 0;
                    objProd.Message = "";
                    objProd.MessageStatus = "";
                    objProd.OnWebsite = "Y";
                    objProd.SpecialOffer = "N";
                    objProd.HotSell = "N";
                    objProd.ProductImagePath = "";
                    objProd.MinQty = 0;
                    objProd.Weight = 0;
                    objProd.HSNCode = "";
                    objProd.UserDetails = objPackage.UserDetails;
                    objProd.UserDetails.UserName = objPackage.UserDetails.UserName;

                    var IsSave = saveProduct(objProd);

                    int i = entities.SaveChanges();
                    if (i > 0)
                    {
                        objResponse.ResponseStatus = "OK";
                        objResponse.ResponseMessage = "Saved Successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.ResponseStatus = "Failed";
                objResponse.ResponseMessage = "Something went wrong!";
            }
            return objResponse;
        }



        public bool saveProduct(ProductDetails model)
        {
            bool IsSave = false;
            M_ProductMaster objDTProduct = new M_ProductMaster();
            itemcode objItemcode = new itemcode();
            M_BarCodeMaster objDTBarcode = new M_BarCodeMaster();
            M_TaxMaster objDTTax = new M_TaxMaster();
            TrnStockJv objDtStock = new TrnStockJv();
            Im_CurrentStock objDtCurrentStock = new Im_CurrentStock();
            string objversion = "";
            M_FiscalMaster objFiscalMaster = new M_FiscalMaster();
            int i = 0;
            decimal BatchCode = 1000000;
            decimal StockJNo = 1001;
           
            try
            {
                using (var entity = new InventoryEntities())
                {
                    if (model != null)
                    {
                        objFiscalMaster = (from result in entity.M_FiscalMaster where result.ActiveStatus == "Y" select result).FirstOrDefault();
                        objDTProduct = (from result in entity.M_ProductMaster where result.ProductCode == model.ProductCode select result).FirstOrDefault();
                        objversion = (from result in entity.M_NewHOVersionInfo select result.VersionNo).FirstOrDefault();
                        if (objDTProduct == null)
                        {
                            objDTProduct = new M_ProductMaster();
                            objItemcode = new itemcode();
                            model.ProductCode = MaxKitProductCode();
                        }

                        i = 0;

                        objDTProduct.ProductCode = model.ProductCode;
                        objDTProduct.ProductName = model.ProductName;
                        objDTProduct.ProdId = model.ProductCode.ToString();
                        objDTProduct.PV = model.PV;
                        objDTProduct.RP = model.RP;
                        objDTProduct.SpclOffer = !string.IsNullOrEmpty(model.SpecialOffer) ? model.SpecialOffer : "";
                        objDTProduct.HotSell = !string.IsNullOrEmpty(model.HotSell) ? model.HotSell : "";
                        objDTProduct.ImagePath = string.IsNullOrEmpty(model.ProductImagePath) ? "" : model.ProductImagePath;
                        objDTProduct.IsImage = string.IsNullOrEmpty(model.ProductImagePath) ? "N" : "Y";
                        objDTProduct.ActiveStatus = model.IsActive ? "Y" : "N";
                        objDTProduct.OnWebSite = model.OnWebsite;
                        objDTProduct.MsgText = string.IsNullOrEmpty(model.Message) ? "" : model.Message;
                        objDTProduct.MsgStatus = !string.IsNullOrEmpty(model.MessageStatus) ? model.MessageStatus : "";
                        // objDTProduct.MinQty = model.MinQty;
                        objDTProduct.IMEINo = model.MinQty.ToString();
                        objDTProduct.ProdCommssn = model.ProductCommission;
                        objDTProduct.Discount = model.DiscountPer;
                        objDTProduct.DiscInRs = model.DiscountInRs;
                        objDTProduct.UserProdId = string.IsNullOrEmpty(model.UserDefinedCode) ? "" : model.UserDefinedCode;
                        objDTProduct.SubCatId = model.SubCatgeoryId;
                        objDTProduct.CatId = model.CategoryId;
                        objDTProduct.ProductDesc = model.ProductDescription;
                        objDTProduct.CV = model.CV;
                        objDTProduct.BV = model.BV;
                        objDTProduct.HSNCode = string.IsNullOrEmpty(model.HSNCode) ? "" : model.HSNCode;

                        objItemcode.PCode = model.ProductCode.ToString();
                        objItemcode.ItemCode1 = model.ProductCode.ToString();
                        objItemcode.descr = model.ProductName;
                        objItemcode.Status = false;
                        objItemcode.myChoice = false;
                        objItemcode.Attrib1 = "";
                        objItemcode.Attrib2 = "";
                        objItemcode.Attrib3 = "";
                        objItemcode.Attrib4 = "";
                        objItemcode.Attrib5 = "";
                        objItemcode.Attrib6 = "";
                        objItemcode.Attrib7 = "";
                        objItemcode.Attrib8 = "";
                        objItemcode.Attrib9 = "";
                        objItemcode.Attrib10 = "";
                        objItemcode.CreationDate = DateTime.Now;
                        objItemcode.RPCId = model.SubCatgeoryId;

                        if (model.UserDetails != null)
                        {
                            objDTProduct.UserId = model.UserDetails.UserId;
                        }
                        objDTProduct.RecTimeStamp = DateTime.Now;

                        objDTProduct.GenerateBy = model.UserDetails.PartyCode;
                        objDTProduct.BrandCode = model.BrandCode;
                        objDTProduct.ProductType = "P";
                        objDTProduct.Prefix = "";
                        objDTProduct.ItemType = "";
                        objDTProduct.BuyingTax = 0;
                        objDTProduct.Weight = model.Weight;
                        objDTProduct.PurchaseRate = model.ProductBarcodeDetails.PurchaseRate;
                        objDTProduct.DP1 = 0;
                        objDTProduct.OtherStateDP = 0;
                        objDTProduct.Exp = 0;
                        objDTProduct.Costing = 0;
                        objDTProduct.FundPoint = 0;
                        if (model.DiscountPer > 0 || model.DiscountInRs > 0)
                        {
                            objDTProduct.IsDiscount = "Y";
                        }
                        else if (model.DiscountPer > 0 && model.DiscountInRs == 0)
                        {
                            objDTProduct.IsDiscount = "Y";
                        }
                        else if (model.DiscountPer == 0 && model.DiscountInRs > 0)
                        {
                            objDTProduct.IsDiscount = "Y";
                        }
                        else
                        {
                            objDTProduct.IsDiscount = "N";
                        }
                        objDTProduct.VDiscount = 0;
                        objDTProduct.GRate = 0;
                        objDTProduct.GMCharge = 0;
                        objDTProduct.GMType = "";
                        objDTProduct.IsCardIssue = "N";
                        objDTProduct.Remarks = "";
                        objDTProduct.TaxType = "I";
                        decimal BarcodeId = (from result in entity.M_BarCodeMaster select result.BId).DefaultIfEmpty(1000000).Max();
                        BarcodeId = BarcodeId + 1;
                        objDTProduct.BId = BarcodeId;
                        objDTProduct.Imported = "N";
                        objDTProduct.BNo = "0";
                        objDTProduct.PType = "K";
                        if (model.ProductBarcodeDetails.BarcodeType == "System Generated")
                        { objDTProduct.BarcodeType = "W"; }
                        else
                        {
                            objDTProduct.BarcodeType = "O";
                        }

                        objDTProduct.BCode = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? decimal.Parse(model.ProductBarcodeDetails.Barcode) : 0;
                        objDTProduct.Barcode = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                        objDTProduct.OpStockQty = model.ProductCurrentStockDetails.OpeningStockQty;
                        objDTProduct.LastModified = "";
                        objDTProduct.Val1 = 0;
                        objDTProduct.Val2 = 0;
                        objDTProduct.Company = "U";
                        objDTProduct.PurchaseFrom = "WR";
                        objDTProduct.PurchaseStore = "WR";
                        objDTProduct.IsFlexible = "N";
                        objDTProduct.IsForPC = "N";
                        objDTProduct.SubQty = 0;
                        objDTProduct.CalcKitRate = "N";
                        objDTProduct.FlexiQty = 0;
                        objDTProduct.ImgPath1 = "";
                        objDTProduct.ImgPath2 = "";
                        objDTProduct.ImgPath3 = "";
                        objDTProduct.ImgPath4 = "";
                        objDTProduct.AlterID = model.UserDetails.UserId;
                        objDTProduct.MRP = model.ProductBarcodeDetails.MRP;
                        objDTProduct.DP = model.ProductBarcodeDetails.DP;
                        objDTProduct.IsExpired = model.ProductBarcodeDetails.IsExpirable;
                        DateTime MfgDate = DateTime.Now;
                        DateTime ExpDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(model.ProductBarcodeDetails.MfgDateStr))
                        {
                            var SplitDate = model.ProductBarcodeDetails.MfgDateStr.Split('-');
                            string NewDate = SplitDate[1] + "/" + SplitDate[0] + "/" + SplitDate[2];
                            MfgDate = Convert.ToDateTime(NewDate);
                            MfgDate = MfgDate.Date;
                        }
                        if (!string.IsNullOrEmpty(model.ProductBarcodeDetails.ExpDateStr))
                        {
                            var SplitDate = model.ProductBarcodeDetails.ExpDateStr.Split('-');
                            string NewDate = SplitDate[1] + "/" + SplitDate[0] + "/" + SplitDate[2];
                            ExpDate = Convert.ToDateTime(NewDate);
                            ExpDate = ExpDate.Date;
                        }
                        objDTProduct.MfgDate = MfgDate.Date;
                        objDTProduct.ExpDate = ExpDate.Date;
                        bool isBarcodeMasterSave = true, isTaxMasterSave = true;
                        objDTProduct.OrderDesc = "";
                        objDTProduct.GenerateBy = model.UserDetails.UserName;
                        objDTProduct.HSNCode = "";
                        entity.M_ProductMaster.Add(objDTProduct);
                        entity.itemcodes.Add(objItemcode);

                        try
                        {
                            i = entity.SaveChanges();

                            if (i > 0)
                            {
                                if (model.ProductBarcodeDetails != null)
                                {
                                    objDTBarcode.BId = BarcodeId;
                                    objDTBarcode.SupplierCode = "0";
                                    objDTBarcode.BCode = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? decimal.Parse(model.ProductBarcodeDetails.Barcode) : 0;
                                    objDTBarcode.Company = "";
                                    objDTBarcode.Imported = "N";
                                    objDTBarcode.LastModified = "";
                                    objDTBarcode.DP1 = 0;
                                    objDTBarcode.Val1 = 0;
                                    objDTBarcode.Val2 = 0;


                                    objDTBarcode.BarCode = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    objDTBarcode.BatchNo = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    objDTBarcode.BarCodeType = model.ProductBarcodeDetails.BarcodeType;
                                    objDTBarcode.DP = model.ProductBarcodeDetails.DP;
                                    if (model.ProductBarcodeDetails.IsExpirable == "Y")
                                        objDTBarcode.ExpDate = ExpDate.Date;
                                    else
                                    {
                                        objDTBarcode.ExpDate = DateTime.Now;
                                    }
                                    objDTBarcode.MfgDate = MfgDate.Date;
                                    objDTBarcode.MRP = model.ProductBarcodeDetails.MRP;
                                    objDTBarcode.IsExpired = model.ProductBarcodeDetails.IsExpirable;
                                    objDTBarcode.ProdId = model.ProductCode.ToString();
                                    objDTBarcode.PRate = model.ProductBarcodeDetails.PurchaseRate;
                                    objDTBarcode.Remarks = string.IsNullOrEmpty(model.ProductBarcodeDetails.Remarks) ? "" : model.ProductBarcodeDetails.Remarks;
                                    objDTBarcode.ActiveStatus = model.IsActive ? "Y" : "N";
                                    objDTBarcode.GenerateDate = DateTime.Now;
                                    if (model.UserDetails != null)
                                    {
                                        objDTBarcode.GenerateBy = model.UserDetails.UserName;
                                        objDTBarcode.UserId = model.UserDetails.UserId;
                                        //objDTBarcode.AlterID = model.UserDetails.UserId;
                                    }
                                    objDTBarcode.RecTimeStamp = DateTime.Now;

                                    if (model.ProductBarcodeDetails.BarcodeType == "System Generated")
                                    { objDTBarcode.BType = "W"; }
                                    else
                                    {
                                        objDTBarcode.BType = "O";
                                    }

                                    objDTBarcode.BatchNo = (!string.IsNullOrEmpty(model.ProductBarcodeDetails.Barcode)) ? model.ProductBarcodeDetails.Barcode : "0";
                                    BatchCode = (!string.IsNullOrEmpty(objDTBarcode.BatchNo)) ? decimal.Parse(objDTBarcode.BatchNo) : 1000001;
                                    entity.M_BarCodeMaster.Add(objDTBarcode);
                                    try
                                    {
                                        i = entity.SaveChanges();
                                        if (i > 0)
                                        {
                                            isBarcodeMasterSave = true;
                                        }
                                        else
                                        {
                                            isBarcodeMasterSave = false;
                                        }
                                    }
                                    catch (DbEntityValidationException ex)
                                    {

                                    }
                                }

                                if (model.ProductTaxDetails != null)
                                {
                                    objDTTax.WithCForm = 0;
                                    objDTTax.CstTax = 0;
                                    objDTTax.ActiveStatus = model.IsActive ? "Y" : "N";
                                    objDTTax.VatTax = model.ProductTaxDetails.GSTTax;
                                    objDTTax.ProdName = model.ProductName;
                                    objDTTax.Imported = "N";
                                    objDTTax.LastModified = DateTime.Now.ToString();
                                    objDTTax.Remarks = "";
                                    objDTTax.Company = "";
                                    //objDTTax.GeneratedDate = DateTime.Now;
                                    objDTTax.RecTimeStamp = DateTime.Now;

                                    objDTTax.AValue = 0;
                                    objDTTax.STax = 0;
                                    if (model.UserDetails != null)
                                    {
                                        // objDTTax.StateCode = model.UserDetails.StateCode;
                                        objDTTax.StateCode = (int)(from r in entity.M_CompanyMaster select r.CompState).FirstOrDefault();
                                        objDTTax.UserId = model.UserDetails.UserId;
                                        objDTTax.GenerateBy = model.UserDetails.UserName;
                                    }
                                    objDTTax.ProdCode = model.ProductCode.ToString();
                                    i = 0;
                                    entity.M_TaxMaster.Add(objDTTax);
                                    try
                                    {
                                        i = entity.SaveChanges();
                                        if (i > 0)
                                        {
                                            //ProductTaxId = (from result in entity.M_TaxMaster select result.AId).Max();
                                            isTaxMasterSave = true;
                                        }
                                        else
                                        {
                                            isTaxMasterSave = false;
                                        }
                                    }
                                    catch (DbEntityValidationException ex)
                                    {

                                    }
                                }


                                // i = 0;
                                objDtStock.Barcode = model.ProductBarcodeDetails.Barcode;
                                objDtStock.BatchNo = BatchCode.ToString();
                                if (model.UserDetails != null)
                                {
                                    objDtStock.UserId = model.UserDetails.UserId;
                                    objDtStock.UserName = model.UserDetails.UserName;
                                }
                                objDtStock.RecTimeStamp = DateTime.Now;
                                if (objFiscalMaster != null)
                                    objDtStock.FSessId = objFiscalMaster.FSessId;
                                else
                                    objDtStock.FSessId = 0;
                                var res = (from result in entity.TrnStockJvs select result).ToList();
                                if (res.Count == 0)
                                    objDtStock.JNo = 1001;
                                else
                                {
                                    decimal MaxJNo = (from r in res select r.JNo).DefaultIfEmpty(0).Max();
                                    objDtStock.JNo = MaxJNo + 1;
                                }
                                objDtStock.JvNo = "OPN/" + objDtStock.JNo;
                                StockJNo = objDtStock.JNo == 0 ? 1001 : objDtStock.JNo;
                                objDtStock.ProdId = model.ProductCode.ToString();
                                objDtStock.ProductName = (from result in entity.M_ProductMaster where result.ProductCode == model.ProductCode select result.ProductName).FirstOrDefault() ?? "";
                                objDtStock.ProdType = "P";
                                objDtStock.Qty = model.ProductCurrentStockDetails.OpeningStockQty;
                                objDtStock.Remarks = "Opening Stock Of Product Registration";
                                objDtStock.RefNo = objDtStock.JvNo;
                                objDtStock.JType = "O";
                                objDtStock.ActiveStatus = model.IsActive ? "Y" : "N";
                                objDtStock.Version = objversion;
                                if (model.UserDetails != null)
                                {
                                    objDtStock.PartyName = model.UserDetails.PartyName;
                                    objDtStock.FCode = model.UserDetails.PartyCode;
                                    objDtStock.SoldBy = model.UserDetails.PartyCode;
                                }

                                objDtStock.StockDate = DateTime.Now;
                                try
                                {
                                    entity.TrnStockJvs.Add(objDtStock);
                                    i = 0;
                                    i = entity.SaveChanges();
                                    if (i > 0)
                                    {
                                        IsSave = true;

                                    }
                                    else
                                    {
                                        IsSave = false;
                                    }

                                }
                                catch (DbEntityValidationException e)
                                {
                                    foreach (var eve in e.EntityValidationErrors)
                                    {
                                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                        foreach (var ve in eve.ValidationErrors)
                                        {
                                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                                ve.PropertyName, ve.ErrorMessage);
                                        }
                                    }
                                }
                            }

                        }
                        catch (DbEntityValidationException ex)
                        {

                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return IsSave;
        }
    }
}
