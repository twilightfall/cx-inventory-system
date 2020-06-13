using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ledger.Classes
{
    class Export
    {
        public string CollectorNumber { get; set; }
        public string CardName { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }

        public int NMQty { get; set; }
        public int SPQty { get; set; }
        public int PLDQty { get; set; }
        public int HPQty { get; set; }
        public double NMPrice { get; set; }
        public double SPPrice { get; set; }
        public double PLDPrice { get; set; }
        public double HPPrice { get; set; }

        public int NMFoilQty { get; set; }
        public int SPFoilQty { get; set; }
        public int PLDFoilQty { get; set; }
        public int HPFoilQty { get; set; }
        public double NMFoilPrice { get; set; }
        public double SPFoilPrice { get; set; }
        public double PLDFoilPrice { get; set; }
        public double HPFoilPrice { get; set; }

        public Export()
        {

        }
    }
}
