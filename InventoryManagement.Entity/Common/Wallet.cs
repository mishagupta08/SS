using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class Wallet
    {
        public string FCode { get; set; }
        public Decimal Amount { get; set; }
        public string DrCr { get; set; }
        public string Narration { get; set; }
    }
}