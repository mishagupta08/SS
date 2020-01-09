using InventoryManagement.App_Start;
using InventoryManagement.Business;
using InventoryManagement.Common;
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
    [Authorize]
    public class UserController : Controller
    {
        UserManager objUserManager = new UserManager();
        LogManager objLogManager = new LogManager();
        // GET: User
        [SessionExpire]
        public ActionResult UserMasterList()
        {
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "UserMasterList");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View();
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        [SessionExpire]
        public ActionResult AddUser(string IsActionName, string UserCode)
        {
            User objModel = new User();
            var result = objUserManager.GetPartyListForUsers();
            
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
            if (result.Count > 0)
            {
                objModel.FCode = result[0].PartyCode;
                objModel.GroupId = (int)objUserManager.GetPartyGroupId(result[0].PartyCode);
            }
            if (IsActionName == "Add")
            {
               
                
                objModel.ActiveStatus = "Y";
            }
            else
            {
                if (!string.IsNullOrEmpty(UserCode))
                {
                    int UId = int.Parse(UserCode);
                    objModel = objUserManager.GetUser(UId);

                }
            }
            objModel.IsActionName = IsActionName;
            
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "UserMasterList");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objModel);
            }
            else
                return RedirectToAction("Dashboard", "Home");
        }

        public ActionResult GetAllUserList()
        {
            List<User> objUserList = new List<User>();
            objUserList = objUserManager.GetAllUserList();
            var jsonUserList = Json(objUserList, JsonRequestBehavior.AllowGet);
            jsonUserList.MaxJsonLength = int.MaxValue;
            return jsonUserList;
        }
        [HttpPost]
        public ActionResult SaveUserDetails(User objModel)
        {
            ResponseDetail objResponse = new ResponseDetail();           
            objResponse = objUserManager.AddEditUserDetails(objModel, Session["LoginUser"] as User);

            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
            objLogManager.SaveLog(Session["LoginUser"] as User, objModel.IsActionName + " user - " + objModel.UserName, myIP + currentDate);

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }
        public ActionResult IsDuplicateUserName(string IsActionType, string UserId, string UserName)
        {
            return Json(objUserManager.IsDuplicateUserName(IsActionType, UserId, UserName), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPartyGroupId(string SelectedPartyCode)
        {
            return Json(objUserManager.GetPartyGroupId(SelectedPartyCode), JsonRequestBehavior.AllowGet);
        }
        [SessionExpire]
        public ActionResult SetUserRights()
        {
            UserPermissionMasterModel objUserList = new UserPermissionMasterModel();
            objUserList.UserList = objUserManager.GetUserList(false);
            if (objUserList.UserList != null && objUserList.UserList.Count() > 0)
            {
                ViewBag.Selecteduser = objUserList.UserList[0].UserId;
            }
            else
            {
                ViewBag.Selecteduser = 0;
            }
            var AccessTo = new UserController().UserCanAccess((Session["LoginUser"] as User).UserId, "SetUserRights");
            if (!string.IsNullOrEmpty(AccessTo))
            {
                ViewBag.UserCanAccess = AccessTo;
                return View(objUserList);
            }
            else
                return RedirectToAction("Dashboard", "Home");
          
        }
        

        public ActionResult GetPermissionList(string UserId)
        {
            decimal userID = 0;
            if (!(string.IsNullOrEmpty(UserId)))
            {
                userID = decimal.Parse(UserId);
            }
            return Json(objUserManager.ListUserPermittedMenus(userID),JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveUserRights(UserPermissionMasterModel PermittedList)
        {
            ResponseDetail objResponse = new ResponseDetail();
            List<UserPermissionMasterModel> objPermissionList = new List<UserPermissionMasterModel>();
            string Log = string.Empty;
            if (!string.IsNullOrEmpty(PermittedList.ListPermittedMenuList))
            {
                var objects = JArray.Parse(PermittedList.ListPermittedMenuList); // parse as array  
                foreach (JObject root in objects)
                {
                    UserPermissionMasterModel objTemp = new UserPermissionMasterModel();
                    foreach (KeyValuePair<String, JToken> app in root)
                    {
                        if (app.Key == "MenuId")
                        {
                            objTemp.MenuId = (decimal)app.Value;
                        }                        
                        else if (app.Key == "IsPermitted")
                        {
                            objTemp.IsPermitted = (bool)app.Value;
                        }
                        else if (app.Key == "IsEdit")
                        {
                            objTemp.IsEdit = (bool)app.Value;
                        }
                    }
                    objTemp.CurrentLoginUser = Session["LoginUser"] as User;
                    objTemp.UserId = PermittedList.UserId;

                    objPermissionList.Add(objTemp);
                }
                Log = "Saved user rights for userid - " + PermittedList.UserId;
            }
                objResponse = objUserManager.SetUserRights(objPermissionList);

            //Added log
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmssfff"); ;
            objLogManager.SaveLog(Session["LoginUser"] as User, Log , myIP + currentDate);

            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

        public bool CanUserAccessMenu(int UserID, string MenuFile)
        {
            return objUserManager.CanUserAccessMenu(UserID, MenuFile);
        }

        public string UserCanAccess(int UserID, string MenuFile)
        {
            return objUserManager.UserCanAccess(UserID, MenuFile);
        }

    }
}