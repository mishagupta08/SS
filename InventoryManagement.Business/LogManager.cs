using InventoryManagement.API.Controllers;
using InventoryManagement.DataAccess;
using InventoryManagement.Entity.Common;

namespace InventoryManagement.Business
{
    public class LogManager
    {
        LogRepository objRepo = new LogRepository();
        public void SaveLog(User user, string log, string uid)
        {
            objRepo.SaveLog(user, log, uid);
        }
    }
}