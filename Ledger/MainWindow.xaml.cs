using Ledger.Classes;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private DataTable dPending;
        //private DataTable dComplete;

        private List<Order> pending = new List<Order>();
        private List<Order> complete = new List<Order>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                RefreshOrderList();

                LoadingBlock.Text = "LOADING DATA. PLEASE WAIT.";
                
                ProgBar.IsIndeterminate = true;

                #region TO BE UPDATED
                //IsolatedStorageFile isf = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                //isf.DeleteFile("TcgAuth.txt");

                //if (isf.FileExists("TcgAuth.txt"))
                //{
                //    using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream("TcgAuth.txt", FileMode.Open, isf))
                //    {
                //        using (StreamReader reader = new StreamReader(fileStream))
                //        {
                //            string authString = reader.ReadToEnd();

                //            var auth = JsonConvert.DeserializeObject<TCGAuth>(authString);

                //            if (DateTime.Compare(DateTime.Now, DateTime.Parse(auth.Expires)) > 0)
                //                App.Bearer = auth.Access_token;
                //            else
                //            {
                //                using (StreamWriter writer = new StreamWriter(fileStream))
                //                {
                //                    var newAuth = await TCGAuth.GetBearerToken();
                //                    var authJson = JsonConvert.SerializeObject(newAuth);
                //                    App.Bearer = newAuth.Access_token;

                //                    writer.Write(authJson);
                //                }
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    using (IsolatedStorageFileStream fileStream2 = new IsolatedStorageFileStream("TcgAuth.txt", FileMode.CreateNew, isf))
                //    {
                //        using (StreamWriter writer2 = new StreamWriter(fileStream2))
                //        {
                //            var newBearer = await TCGAuth.GetBearerToken();
                //            var bearerJson = JsonConvert.SerializeObject(newBearer);
                //            App.Bearer = newBearer.Access_token;
                //            writer2.Write(bearerJson);
                //        }
                //    }
                //} 
                #endregion

                #region GET SET CODES
                if (App.root == null)
                {
                    try
                    {
                        var bearer = await TCGAuth.GetBearerToken();
                        App.Bearer = bearer.Access_token;
                        App.root = await App.GetSetObjects();

                        ProgBar.IsIndeterminate = false;
                        LoadingBlock.Text = string.Empty;

                        OrdersTab.IsEnabled = true;
                        AddOrderButton.IsEnabled = true;
                        ViewOrderButton.IsEnabled = true;
                        ImportButton.IsEnabled = true;
                        ViewStocksButton.IsEnabled = true;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Something went wrong. Please ensure internet connectivity and reopen the app");

                        ProgBar.IsIndeterminate = false;
                    }
                }
                #endregion
            };
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("USE ONLY FOR INITIAL INVENTORY IMPORT!");

            ImportStocksWindow import = new ImportStocksWindow() { Owner = this };
            import.ShowDialog();
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            OrderDetailsWindow order = new OrderDetailsWindow() { Owner = this };
            var result = order.ShowDialog();
            if (result == false)
            {
                RefreshOrderList();
            }
        }

        private void ViewStocksButton_Click(object sender, RoutedEventArgs e)
        {
            StocksWindow stocks = new StocksWindow() { Owner = this };
            stocks.ShowDialog();
            //await GetBearerToken();
            //await Call();
            //await Get2();
            //await GetPrice();
        }

        #region TCGPLAYER API CODE
        private static async Task Get2()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://api.tcgplayer.com/v1.36.0/catalog/categories/1/search/"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "bearer V3JcxzI4DDykrHHolRUBbg-yC-5G0yScJaUAWMS5VPMRpSs74KW_t2Ksv6fr31InnctDdXiKr8v77BshQ2FtNYG00GjpeEi40sT1zfvULK2UwqBUMh7Oyu2JRr3XXc8oeouVLEtOuF0di9MIcHCcX7fAHgtkPGkDfXIDmIuyS9RVS-xho7RtU1ZL4AGn7eR6hmCLu3Kotng0INll5Y0IRJ_8xp5KWW_P9_DJ7BhCHbMr7cy1Knt5ObI9O0YUXkVPeW0wDcUggGUx0HPU7PbNBgX31QlfuN12V0Q7PgG5SwYF1eH-IfuON-HhTZetwBZL94fKbA");
                    request.Content = new StringContent(@"{""sort"": ""Relevance"",""limit"": 10,""offset"": 0,""filters"": [ { ""name"": ""ProductName"", ""values"": [ ""Chalice of the Void"" ]    } ]}", Encoding.UTF8, "application/json");

                    var response = await httpClient.SendAsync(request);
                    var tokres = await response.Content.ReadAsStringAsync();
                    var t = JsonConvert.DeserializeObject(tokres);
                }
            }
        }

        private static async Task Get()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "http://api.tcgplayer.com/v1.36.0/catalog/categories/1/search/manifest"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "bearer V3JcxzI4DDykrHHolRUBbg-yC-5G0yScJaUAWMS5VPMRpSs74KW_t2Ksv6fr31InnctDdXiKr8v77BshQ2FtNYG00GjpeEi40sT1zfvULK2UwqBUMh7Oyu2JRr3XXc8oeouVLEtOuF0di9MIcHCcX7fAHgtkPGkDfXIDmIuyS9RVS-xho7RtU1ZL4AGn7eR6hmCLu3Kotng0INll5Y0IRJ_8xp5KWW_P9_DJ7BhCHbMr7cy1Knt5ObI9O0YUXkVPeW0wDcUggGUx0HPU7PbNBgX31QlfuN12V0Q7PgG5SwYF1eH-IfuON-HhTZetwBZL94fKbA");
                    //request.Content = new StringContent("")

                    var response = await httpClient.SendAsync(request);
                    var tokres = await response.Content.ReadAsStringAsync();
                    var t = JsonConvert.DeserializeObject(tokres);
                }
            }
        }

        private static async Task Call()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "http://api.tcgplayer.com/v1.36.0/catalog/categories"))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");
                    request.Headers.TryAddWithoutValidation("Authorization", "bearer V3JcxzI4DDykrHHolRUBbg-yC-5G0yScJaUAWMS5VPMRpSs74KW_t2Ksv6fr31InnctDdXiKr8v77BshQ2FtNYG00GjpeEi40sT1zfvULK2UwqBUMh7Oyu2JRr3XXc8oeouVLEtOuF0di9MIcHCcX7fAHgtkPGkDfXIDmIuyS9RVS-xho7RtU1ZL4AGn7eR6hmCLu3Kotng0INll5Y0IRJ_8xp5KWW_P9_DJ7BhCHbMr7cy1Knt5ObI9O0YUXkVPeW0wDcUggGUx0HPU7PbNBgX31QlfuN12V0Q7PgG5SwYF1eH-IfuON-HhTZetwBZL94fKbA");

                    var response = await httpClient.SendAsync(request);
                    var tokres = await response.Content.ReadAsStringAsync();
                    var t = JsonConvert.DeserializeObject(tokres);
                }
            }
        }
        #endregion

        private void OrdersTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompletedTab.IsSelected)
            {
                PendingOrdersList.SelectedIndex = -1;
                CompleteOrderButton.IsEnabled = false;
                CancelOrderButton.IsEnabled = false;
            }
            else
            {
                CompletedOrdersList.SelectedIndex = -1;
                CompleteOrderButton.IsEnabled = true;
                CancelOrderButton.IsEnabled = true;
            }
        }

        private void RefreshOrderList()
        {
            try
            {
                SqlConnection conn = new SqlConnection(App.connstring);
                string commPending = $"select o.OrderId, c.CustName, o.DatePlaced, o.IsCompleted, o.TotalOrder " +
                                     $"from Orders o inner join Customer c on o.CustId = c.CustId where o.IsCompleted = 'N'";
                string commComplete = $"select o.OrderId, c.CustName, o.DatePlaced, o.DateCompleted, o.IsCompleted, o.TotalOrder " +
                                      $"from Orders o inner join Customer c on o.CustId = c.CustId where o.IsCompleted = 'Y'";
                conn.Open();

                pending.Clear();
                complete.Clear();

                SqlCommand pendingCommand = new SqlCommand(commPending, conn);
                SqlCommand completeCommand = new SqlCommand(commComplete, conn);

                using (SqlDataReader reader = pendingCommand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            pending.Add(new Order()
                            {
                                OrderId = Convert.ToInt32(reader["OrderId"]),
                                CustomerName = reader["CustName"].ToString(),
                                DatePlaced = DateTime.Parse(reader["DatePlaced"].ToString()),
                                IsCompleted = reader["IsCompleted"].ToString(),
                                OrderTotal = Convert.ToDouble(reader["TotalOrder"])
                            });
                        }
                    }

                    PendingOrdersList.ItemsSource = pending;
                    PendingOrdersList.Items.Refresh();
                }

                using (SqlDataReader reader2 = completeCommand.ExecuteReader())
                {
                    if (reader2.HasRows)
                    {
                        while (reader2.Read())
                        {
                            complete.Add(new Order()
                            {
                                DateCompleted = DateTime.Parse(reader2["DateCompleted"].ToString()),
                                OrderId = Convert.ToInt32(reader2["OrderId"]),
                                CustomerName = reader2["CustName"].ToString(),
                                DatePlaced = DateTime.Parse(reader2["DatePlaced"].ToString()),
                                IsCompleted = reader2["IsCompleted"].ToString(),
                                OrderTotal = Convert.ToDouble(reader2["TotalOrder"])
                            });
                        }
                    }

                    CompletedOrdersList.ItemsSource = complete;
                    CompletedOrdersList.Items.Refresh();
                }

                conn.Close();
                conn.Dispose();
            }
            catch (Exception)
            {
                MessageBox.Show("Database error. Please restore from a backup.");
            }
        }

        private void CompleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            List<OrderItem> orderItems = new List<OrderItem>();

            StringBuilder sb = new StringBuilder();

            if (PendingOrdersList.SelectedIndex != -1)
            {
                SqlConnection conn = new SqlConnection(App.connstring);
                conn.Open();

                string selectItems = $"select i.OrderId, c.CustName, i.ItemId, v.CardName, i.IsFoil, i.Quality, i.Quantity, i.UnitPrice, i.TotalPrice " +
                                     $"from OrderItems i inner join Customer c on i.CustId = c.CustId " +
                                     $"inner join Inventory v on i.ItemId = v.InventoryId " +
                                     $"where i.OrderId = {(PendingOrdersList.SelectedItem as Order).OrderId}";

                SqlCommand getOrderItemsComm = new SqlCommand(selectItems, conn);

                using (SqlDataReader reader = getOrderItemsComm.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            
                            orderItems.Add(new OrderItem()
                            {
                                OrderId = Convert.ToInt32(reader["OrderId"]),
                                InvId = Convert.ToInt32(reader["ItemId"]),
                                IsFoil = reader["IsFoil"].ToString(),
                                Qty = Convert.ToInt32(reader["Quantity"]),
                                Quality = reader["Quality"].ToString(),
                                CardName = reader["CardName"].ToString(),
                            });
                        }

                        var result = pending.Find(x => x.OrderId == (PendingOrdersList.SelectedItem as Order).OrderId);
                        result.OrderItems = orderItems;
                    }
                }

                foreach (var item in pending[PendingOrdersList.SelectedIndex].OrderItems)
                {
                    if (sb.Length == 0)
                        sb.Append($"update Inventory set ");

                    switch (item.Quality)
                    {
                        case "NM":
                            if (item.IsFoil == "N")
                            {
                                sb.Append($"NMQty = NMQty - '{item.Qty}' where InventoryId = '{item.InvId}'");
                                break;
                            }
                            else if (item.IsFoil == "Y")
                            {
                                sb.Append($"NMFoilQty = NMFoilQty - '{item.Qty}' where InventoryId = '{item.InvId}'");
                                break;
                            }
                            break;
                        case "SP":
                            if (item.IsFoil == "N")
                            {
                                sb.Append($"SPQty = SPQty - '{item.Qty}' where InventoryId = '{item.InvId}'");
                                break;
                            }
                            else if (item.IsFoil == "Y")
                            {
                                sb.Append($"SPFoilQty = SPFoilQty - '{item.Qty}' where InventoryId = '{item.InvId}'");
                                break;
                            }
                            break;
                        case "PLD":
                            if (item.IsFoil == "N")
                            {
                                sb.Append($"PLDQty = PLDQty - '{item.Qty}' where InventoryId = '{item.InvId}'");
                                break;
                            }
                            else if (item.IsFoil == "Y")
                            {
                                sb.Append($"PLDFoilQty = PLDFoilQty - '{item.Qty}' where InventoryId = '{item.InvId}'");
                                break;
                            }
                            break;
                        case "HP":
                            if (item.IsFoil == "N")
                            {
                                sb.Append($"HPQty = HPQty - '{item.Qty}' where InventoryId = '{item.InvId}'");
                                break;
                            }
                            else if (item.IsFoil == "Y")
                            {
                                sb.Append($"HPFoilQty = HPFoilQty - '{item.Qty}' where InventoryId = '{item.InvId}'");
                                break;
                            }
                            break;
                    }

                    string updateString = sb.ToString();

                    SqlCommand updateStockComm = new SqlCommand(updateString, conn);
                    updateStockComm.ExecuteNonQuery();

                    sb.Clear();
                }

                SqlCommand updateOrderComm = new SqlCommand($"update Orders set IsCompleted = 'Y', DateCompleted = '{DateTime.Now}' " +
                                                            $"where OrderId = '{(PendingOrdersList.SelectedItem as Order).OrderId}'", conn);

                updateOrderComm.ExecuteNonQuery();

                PendingOrdersList.SelectedIndex = -1;

                conn.Close();
                conn.Dispose();
                MessageBox.Show("Order completed.");
                RefreshOrderList();
            }
        }

        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (PendingOrdersList.SelectedIndex != -1)
            {
                var result = MessageBox.Show("Do you wish to delete the selected order? This cannot be undone.", "WARNING!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(App.connstring))
                    {
                        conn.Open();
                        int oID = (PendingOrdersList.SelectedItem as Order).OrderId;

                        SqlCommand deleteComm = new SqlCommand($"delete from Orders where OrderId = '{oID}'", conn);
                        deleteComm.ExecuteNonQuery();

                        MessageBox.Show("Order deleted.");
                        conn.Close();
                        conn.Dispose();
                    }
                }

                RefreshOrderList(); 
            }
        }

        private void ViewOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Order pendingOrder = (PendingOrdersList.SelectedItem as Order);
            Order completeOrder = (CompletedOrdersList.SelectedItem as Order);

            OrderView view;

            if (pendingOrder != null)
            {
                view = new OrderView(pendingOrder) { Owner = this };
                view.ShowDialog();
            }
            else if (completeOrder != null)
            {
                view = new OrderView(completeOrder) { Owner = this };
                view.ShowDialog();
            }
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
