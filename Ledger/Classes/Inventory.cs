using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ledger.Classes
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public string CollectorNumber { get; set; }
        public int TCGProdId { get; set; }
        public string CardName { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public int NMQty { get; set; }
        public int SPQty { get; set; }
        public int PLDQty { get; set; }
        public int HPQty { get; set; }
        public int NMFoilQty { get; set; }
        public int SPFoilQty { get; set; }
        public int PLDFoilQty { get; set; }
        public int HPFoilQty { get; set; }

        public Inventory()
        {

        }
    }

    public class Stock
    {
        public string CardName { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public int NMQty { get; set; }
        public int SPQty { get; set; }
        public int PLDQty { get; set; }
        public int HPQty { get; set; }
        public int NMFoilQty { get; set; }
        public int SPFoilQty { get; set; }
        public int PLDFoilQty { get; set; }
        public int HPFoilQty { get; set; }

        public Stock()
        {

        }
    }
}