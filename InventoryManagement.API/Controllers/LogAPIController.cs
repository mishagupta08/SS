using System;
using System.Web.Mvc;
using InventoryManagement.Entity.Common;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Controllers
{
    public class LogAPIController : Controller
    {
        // GET: LogAPI
        public void SaveLog(User user, string log,string uid)
        {
            try {
                using (var entity = new InventoryEntities())
                {
                    M_LogMaster objlog = new M_LogMaster();
                    objlog.UID = uid;
                    objlog.RecTimeStamp = DateTime.Now;
                    objlog.Log = log;
                    objlog.UserId = user.UserId;
                    objlog.UserName = user.UserName;
                    objlog.PartyCode = user.PartyCode;
                    objlog.PartyName = user.PartyName;

                    entity.M_LogMaster.Add(objlog);
                    entity.SaveChanges();
                }                   
            }
            catch (Exception ex)
            {

            }  
        }
    }
}