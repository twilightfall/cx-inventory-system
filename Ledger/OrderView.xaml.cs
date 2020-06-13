using Ledger.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : Window
    {
        public OrderView()
        {
            InitializeComponent();
        }

        public OrderView(Order order)
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                order.OrderItems = new List<OrderItem>();
                Title = $"Order details for {order.CustomerName} dated {order.DatePlaced.ToShortDateString()}";

                using (SqlConnection conn = new SqlConnection(App.connstring))
                {
                    conn.Open();

                    string selectItems = $"select i.OrderId, c.CustName, i.ItemId, v.CardName, v.SetCode, i.IsFoil, i.Quality, i.Quantity, i.UnitPrice, i.TotalPrice " +
                                         $"from OrderItems i inner join Customer c on i.CustId = c.CustId " +
                                         $"inner join Inventory v on i.ItemId = v.InventoryId " +
                                         $"where i.OrderId = {order.OrderId}";

                    SqlCommand getOrderItemsComm = new SqlCommand(selectItems, conn);

                    using (SqlDataReader reader = getOrderItemsComm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                order.OrderItems.Add(new OrderItem()
                                {
                                    OrderId = Convert.ToInt32(reader["OrderId"]),
                                    InvId = Convert.ToInt32(reader["ItemId"]),
                                    SetCode = reader["SetCode"].ToString(),
                                    IsFoil = reader["IsFoil"].ToString(),
                                    Qty = Convert.ToInt32(reader["Quantity"]),
                                    Quality = reader["Quality"].ToString(),
                                    CardName = reader["CardName"].ToString(),
                                    UnitPrice = Convert.ToDouble(reader["UnitPrice"]),
                                    TotalPrice = Convert.ToDouble(reader["TotalPrice"])
                                });
                            }
                        }
                    }

                    OrderItemsGrid.ItemsSource = order.OrderItems;
                    OrderItemsGrid.Columns[1].Visibility = Visibility.Collapsed;
                    OrderItemsGrid.Columns[2].Header = "Quantity";

                    TotalBlock.Text = order.OrderTotal.ToString();
                }
            };
        }
    }
}
