using InventoryManagement.API.Controllers;
using InventoryManagement.Entity.Common;

namespace InventoryManagement.DataAccess
{
    public class LogRepository
    {
        LogAPIController objAPI = new LogAPIController();
        public void SaveLog(User user,string log,string uid)
        {
            objAPI.SaveLog(user,log,uid);
        }
    }
}