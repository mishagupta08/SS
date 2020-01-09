using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class LogInfo
    {
        public DateTime LogDate { get; set; }
        public string LogStr { get; set; }
        public string UserName { get; set; }
        public decimal UserId { get; set; }
    }
}