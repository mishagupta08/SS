using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Entity.Common
{
    public class ProdAttributes
    {
            public int PrdAttrId { get; set; }
            public string Pcode { get; set; }
            public string Attribute_Name { get; set; }
            public List<ProdAttributeFields> fields{ get; set; }
    }

    public class ProdAttributeFields
    {
        public decimal Slno { get; set; }
        public string pcode { get; set; }
        public string Attribute_Name { get; set; }
        public string Field { get; set; }
        public string Sessid { get; set; }
    }

    public class ProdItemCodes
    {
        public decimal stockAvailable { get; set; }
        public int ItemId { get; set; }
        public string ItemCode1 { get; set; }
        public string PCode { get; set; }
        public Nullable<int> RPCId { get; set; }
        public string Attrib1 { get; set; }
        public string Attrib2 { get; set; }
        public string Attrib3 { get; set; }
        public string Attrib4 { get; set; }
        public string Attrib5 { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> myChoice { get; set; }
        public string descr { get; set; }
        public string Attrib6 { get; set; }
        public string Attrib7 { get; set; }
        public string Attrib8 { get; set; }
        public string Attrib9 { get; set; }
        public string Attrib10 { get; set; }
    }
}