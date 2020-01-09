using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class Dashboard
    {
   
            public decimal TodayPurchase { get; set; }
            public decimal TodaysSaletoFr { get; set; }
            public decimal TodaysSaletoDr { get; set; }
            public decimal TodaysFrSale { get; set; }
            public decimal TodaysDrSale { get; set; }
            public decimal MonthPurchase { get; set; }
            public decimal MonthSaletoFr { get; set; }
            public decimal MonthSaletoDr { get; set; }
            public decimal MonthFrSale { get; set; }
            public decimal MonthDrSale { get; set; }
        public decimal WalletBal { get; set; }

    }
}