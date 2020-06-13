using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ledger.Classes
{
    public class OrderItem
    {
        public int OrderId { get; set; }
        public int InvId { get; set; }
        public int Qty { get; set; }
        public string CardName { get; set; }
        public string SetCode { get; set; }
        public string IsFoil { get; set; }
        public string Quality { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }

        public OrderItem()
        {

        }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public DateTime DatePlaced { get; set; }
        public DateTime DateCompleted { get; set; }
        public string IsCompleted { get; set; }
        public double OrderTotal { get; set; }

        public Order()
        {

        }
    }
}
