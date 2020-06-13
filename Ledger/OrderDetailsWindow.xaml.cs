using Ledger.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for OrderDetailsWindow.xaml
    /// </summary>
    public partial class OrderDetailsWindow : Window
    {
        private static readonly Regex _regex = new Regex("^[0-9 ]+$");
        private static TCGPrice prices;
        private double foilprice;
        private double nonfoilprice;
        private string sCode;
        private List<OrderItem> orderItems = new List<OrderItem>();
        private int InvId, NMQ, SPQ, PQ, HPQ, NMFQ, SPFQ, PFQ, HPFQ;
        private string Foil;

        public OrderDetailsWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to close this window?", "WARNING!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
            };

            Loaded += (s, e) =>
            {
                var setCodes = (from code in App.root.Data select code.Code).ToList();

                SetCodeBox.ItemsSource = setCodes;

                OrderItemGrid.ItemsSource = orderItems;
                OrderItemGrid.Columns.RemoveAt(0);

                QualityCombo.ItemsSource = new List<string>() { "NM", "SP", "PLD", "HP" };
            };

            SetCodeBox.SelectionChanged += (s, e) => { SetNameBox.Clear(); };
            SetNameBox.TextChanged += (s, e) => { SetCodeBox.SelectedIndex = -1; };
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand comm = new SqlCommand();

            string nameSearch = $"select TCGProdId, InventoryId, CollectorNumber, CardName, SetCode, SetName, " +
                                $"NMQty, SPQty, PLDQty, HPQty, NMFoilQty, SPFoilQty, PLDFoilQty, HPFoilQty from Inventory " +
                                $"where CardName like '%{CardNameBox.Text}%'";

            if (DefaultRad.IsChecked == true || (DefaultRad.IsChecked == false && SetCodeRad.IsChecked == false && SetNameRad.IsChecked == false))
                comm.CommandText = nameSearch;
            else if (SetCodeRad.IsChecked == true)
            {
                string nameCodeSearch = $"select TCGProdId, InventoryId, CollectorNumber, CardName, SetCode, SetName, " +
                                        $"NMQty, SPQty, PLDQty, HPQty, NMFoilQty, SPFoilQty, PLDFoilQty, HPFoilQty from Inventory " +
                                        $"where CardName like '%{CardNameBox.Text}%' and SetCode = {SetCodeBox.SelectedItem.ToString().ToUpper()}";
                comm.CommandText = nameCodeSearch;
            }
            else if (SetNameRad.IsChecked == true)
            {
                string nameSetSearch = $"select TCGProdId, InventoryId, CollectorNumber, CardName, SetCode, SetName, " +
                                       $"NMQty, SPQty, PLDQty, HPQty, NMFoilQty, SPFoilQty, PLDFoilQty, HPFoilQty from Inventory " +
                                       $"where CardName like '%{CardNameBox.Text}%' and SetName like '%{SetNameBox.Text}%'";
                comm.CommandText = nameSetSearch;
            }

            using (SqlConnection conn = new SqlConnection(App.connstring))
            {
                comm.Connection = conn;
                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(comm))
                {

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    SearchGrid.ItemsSource = dt.AsDataView();
                    SearchGrid.Columns[1].Visibility = Visibility.Collapsed;
                    SearchGrid.Columns[5].Visibility = Visibility.Collapsed;
                }
                conn.Close();
                conn.Dispose();
            }

            CardNameBox.Text = string.Empty;
            SetCodeBox.SelectedIndex = -1;
            SetNameBox.Text = string.Empty;

            ClearFields();
        }

        private async void SearchGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dGrid = sender as DataGrid;

            try
            {
                if (dGrid.SelectedItem is DataRowView dRow)
                {
                    NSBlock.Text = dRow[3].ToString();
                    SBlock.Text = dRow[5].ToString();
                    sCode = dRow[4].ToString();

                    InvId = Convert.ToInt32(dRow[1]);
                    NMQ = Convert.ToInt32(dRow[6]);
                    SPQ = Convert.ToInt32(dRow[7]);
                    PQ = Convert.ToInt32(dRow[8]);
                    HPQ = Convert.ToInt32(dRow[9]);
                    NMFQ = Convert.ToInt32(dRow[10]);
                    SPFQ = Convert.ToInt32(dRow[11]);
                    PFQ = Convert.ToInt32(dRow[12]);
                    HPFQ = Convert.ToInt32(dRow[13]);

                    prices = await Update(Convert.ToInt32(dRow[0]));

                    nonfoilprice = (from price in prices.Results
                                    where price.SubTypeName == "Normal"
                                    select Math.Round(Convert.ToDouble(price.MidPrice * 50) / 5.0) * 5).FirstOrDefault();
                    foilprice = (from price in prices.Results
                                 where price.SubTypeName == "Foil"
                                 select Math.Round(Convert.ToDouble(price.MidPrice * 50) / 5.0) * 5).FirstOrDefault();

                    QualityCombo.SelectedIndex = -1;
                    NFRad.IsChecked = false;
                    FRad.IsChecked = false;
                    QtyBox.Text = string.Empty;
                    PriceBox.Text = string.Empty;
                    TotalBox.Text = string.Empty;

                    QtyBox.IsEnabled = true;
                    PriceBox.IsEnabled = true;
                    TotalBox.IsEnabled = true;
                    FRad.IsEnabled = true;
                    NFRad.IsEnabled = true;
                    QualityCombo.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Make sure that TCG Product IDs have been extracted for these products! Go to View Stocks > Get TCG Product IDs to begin.", "WARNING!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        private void FoilingChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rad = sender as RadioButton;

            if (rad.Name == "NFRad")
            {
                Foil = "N";
                if (QualityCombo.SelectedIndex != -1)
                {
                    switch (QualityCombo.SelectedIndex)
                    {
                        case 0:
                            PriceBox.Text = nonfoilprice.ToString();
                            break;
                        case 1:
                            PriceBox.Text = Math.Ceiling(nonfoilprice * .9).ToString();
                            break;
                        case 2:
                            PriceBox.Text = Math.Ceiling(nonfoilprice * .8).ToString();
                            break;
                        case 3:
                            PriceBox.Text = Math.Ceiling(nonfoilprice * .7).ToString();
                            break;
                        default:
                            PriceBox.Text = string.Empty;
                            break;
                    } 
                }
            }
            else if (rad.Name == "FRad")
            {
                Foil = "Y";
                if (QualityCombo.SelectedIndex != -1)
                {
                    switch (QualityCombo.SelectedIndex)
                    {
                        case 0:
                            PriceBox.Text = foilprice.ToString();
                            break;
                        case 1:
                            PriceBox.Text = Math.Ceiling(foilprice * .9).ToString();
                            break;
                        case 2:
                            PriceBox.Text = Math.Ceiling(foilprice * .8).ToString();
                            break;
                        case 3:
                            PriceBox.Text = Math.Ceiling(foilprice * .7).ToString();
                            break;
                        default:
                            PriceBox.Text = string.Empty;
                            break;
                    } 
                }
            }
        }

        private void QualityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as ComboBox).SelectedIndex)
            {
                case 0:
                    if (Foil == "N")
                    {
                        PriceBox.Text = nonfoilprice.ToString();
                        break;
                    }
                    else if (Foil == "Y")
                    {
                        PriceBox.Text = foilprice.ToString();
                        break;
                    }
                    else break;
                case 1:
                    if (Foil == "N")
                    {
                        PriceBox.Text = Math.Ceiling(nonfoilprice * .9).ToString();
                        break;
                    }
                    else if (Foil == "Y")
                    {
                        PriceBox.Text = Math.Ceiling(foilprice * .9).ToString();
                        break;
                    }
                    else break;
                case 2:
                    if (Foil == "N")
                    {
                        PriceBox.Text = Math.Ceiling(nonfoilprice * .8).ToString();
                        break;
                    }
                    else if (Foil == "Y")
                    {
                        PriceBox.Text = Math.Ceiling(foilprice * .8).ToString();
                        break;
                    }
                    else break;
                case 3:
                    if (Foil == "N")
                    {
                        PriceBox.Text = Math.Ceiling(nonfoilprice * .7).ToString();
                        break;
                    }
                    else if (Foil == "Y")
                    {
                        PriceBox.Text = Math.Ceiling(foilprice * .7).ToString();
                        break;
                    }
                    else break;
                default:
                    PriceBox.Text = string.Empty;
                    break;
            }
        }
       
        private void QtyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (SearchGrid.SelectedIndex != -1 && (NFRad.IsChecked == true || FRad.IsChecked == true) && QualityCombo.SelectedIndex != -1)
            {
                if (_regex.IsMatch(box.Text) == true && double.TryParse(PriceBox.Text, out double price) == true)
                {
                    TotalBox.Text = (Convert.ToDouble(PriceBox.Text) * Convert.ToDouble(QtyBox.Text)).ToString();
                }
                else
                {
                    TotalBox.Text = string.Empty;
                    return;
                }
            }
            else
            {
                PriceBox.Text = string.Empty;
                TotalBox.Text = string.Empty;
            }
        }

        private async Task<TCGPrice> Update(int TcgId)
        {
            ProgBar.IsIndeterminate = true;

            var prices = await App.GetPrice(TcgId);

            ProgBar.IsIndeterminate = false;

            return prices;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            SearchGrid.SelectedIndex = -1;
            QualityCombo.SelectedIndex = -1;
            NFRad.IsChecked = false;
            FRad.IsChecked = false;
            QtyBox.Text = string.Empty;
            PriceBox.Text = string.Empty;
            TotalBox.Text = string.Empty;
            NSBlock.Text = string.Empty;
            SBlock.Text = string.Empty;
            Foil = string.Empty;
            NFRad.IsEnabled = false;
            FRad.IsEnabled = false;
            QualityCombo.IsEnabled = false;
            QtyBox.IsEnabled = false;
            PriceBox.IsEnabled = false;
            TotalBox.IsEnabled = false;
            Foil = string.Empty;
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Convert.ToInt32(QtyBox.Text);
                Convert.ToDouble(PriceBox.Text);
                Convert.ToDouble(TotalBox.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Please check that all prices, quantities, and card quality are correct.", "WARNING!", MessageBoxButton.OK, MessageBoxImage.Warning);
                QtyBox.Text = string.Empty;
                PriceBox.Text = string.Empty;
                TotalBox.Text = string.Empty;
                NFRad.IsChecked = false;
                FRad.IsChecked = false;
                QualityCombo.SelectedIndex = -1;
                return;
            }

            switch (Foil)
            {
                case "N":
                    switch (QualityCombo.SelectedIndex)
                    {
                        case 0:
                            if (Convert.ToInt32(QtyBox.Text) > NMQ)
                            {
                                MessageBox.Show("You are attempting to add more than what is in stock.");
                                QtyBox.Text = string.Empty;
                                return;
                            }
                            break;
                        case 1:
                            if (Convert.ToInt32(QtyBox.Text) > SPQ)
                            {
                                MessageBox.Show("You are attempting to add more than what is in stock.");
                                QtyBox.Text = string.Empty;
                                return;
                            }
                            break;
                        case 2:
                            if (Convert.ToInt32(QtyBox.Text) > PQ)
                            {
                                MessageBox.Show("You are attempting to add more than what is in stock.");
                                QtyBox.Text = string.Empty;
                                return;
                            }
                            break;
                        case 3:
                            if (Convert.ToInt32(QtyBox.Text) > HPQ)
                            {
                                MessageBox.Show("You are attempting to add more than what is in stock.");
                                QtyBox.Text = string.Empty;
                                return;
                            }
                            break;
                    }
                    break;
                case "Y":
                    switch (QualityCombo.SelectedIndex)
                    {
                        case 0:
                            if (Convert.ToInt32(QtyBox.Text) > NMFQ)
                            {
                                MessageBox.Show("You are attempting to add more than what is in stock.");
                                QtyBox.Text = string.Empty;
                                return;
                            }
                            break;
                        case 1:
                            if (Convert.ToInt32(QtyBox.Text) > SPFQ)
                            {
                                MessageBox.Show("You are attempting to add more than what is in stock.");
                                QtyBox.Text = string.Empty;
                                return;
                            }
                            break;
                        case 2:
                            if (Convert.ToInt32(QtyBox.Text) > PFQ)
                            {
                                MessageBox.Show("You are attempting to add more than what is in stock.");
                                QtyBox.Text = string.Empty;
                                return;
                            }
                            break;
                        case 3:
                            if (Convert.ToInt32(QtyBox.Text) > HPFQ)
                            {
                                MessageBox.Show("You are attempting to add more than what is in stock.");
                                QtyBox.Text = string.Empty;
                                return;
                            }
                            break;
                    }
                    break;
            }

            OrderItem item = new OrderItem()
            {
                InvId = InvId,
                CardName = NSBlock.Text,
                SetCode = sCode,
                Qty = Convert.ToInt32(QtyBox.Text),
                Quality = QualityCombo.SelectedItem.ToString(),
                UnitPrice = Convert.ToDouble(PriceBox.Text),
                TotalPrice = Convert.ToDouble(TotalBox.Text),
                IsFoil = Foil
            };

            orderItems.Add(item);

            OrderItemGrid.Items.Refresh();
            OrderTotalBox.Text = (from items in orderItems
                                  select items.TotalPrice).Sum().ToString();

            QtyBox.IsEnabled = false;
            PriceBox.IsEnabled = false;
            TotalBox.IsEnabled = false;
            FRad.IsEnabled = false;
            NFRad.IsEnabled = false;
            QualityCombo.IsEnabled = false;
            ClearFields();
        }
        
        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrderItemGrid.SelectedIndex != -1)
            {
                orderItems.RemoveAt(OrderItemGrid.SelectedIndex);
                OrderItemGrid.Items.Refresh();
                OrderTotalBox.Text = (from items in orderItems
                                      select items.TotalPrice).Sum().ToString();

                if (orderItems.Count <= 0)
                    PlaceOrderButton.IsEnabled = false;
            }
        }

        private void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustNameBox.Text == string.Empty)
            {
                MessageBox.Show("Please make sure customer name is not blank!");
                return;
            }

            if (orderItems.Count <= 0)
            {
                MessageBox.Show("Please add items to order.");
                return;
            }

            int CustNum, CurrOrder;

            SqlConnection conn = new SqlConnection(App.connstring);
            conn.Open();
            SqlCommand comm = new SqlCommand($"select * from Customer where CustName = '{CustNameBox.Text}'", conn);

            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(comm))
            {
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                if (dataTable.Rows.Count == 0)
                {
                    SqlCommand insertComm = new SqlCommand($"insert into Customer (CustName) values ('{CustNameBox.Text}')", conn);
                    insertComm.ExecuteNonQuery();
                    SqlCommand getID = new SqlCommand($"select max(CustId) from Customer", conn);
                    CustNum = (int)getID.ExecuteScalar();
                }
                else
                    CustNum = dataTable.Rows[0].Field<int>(0);
            }

            string addString = $"insert into Orders (CustId, DatePlaced, IsCompleted, TotalOrder) values " +
                               $"('{CustNum}', '{DateTime.Now}', 'N', '{Convert.ToDouble(OrderTotalBox.Text)}')";
            SqlCommand AddOrder = new SqlCommand(addString, conn);
            AddOrder.ExecuteNonQuery();
            SqlCommand getOrder = new SqlCommand($"select max(OrderId) from Orders", conn);
            CurrOrder = (int)getOrder.ExecuteScalar();

            foreach (var item in orderItems)
            {
                string itemsString = $"insert into OrderItems values" +
                                     $"('{CurrOrder}', '{CustNum}', '{item.InvId}', '{item.IsFoil}', " +
                                     $"'{item.Quality}', '{item.Qty}', '{item.UnitPrice}') ";
                SqlCommand insertItems = new SqlCommand(itemsString, conn);
                insertItems.ExecuteNonQuery();
            }

            MessageBox.Show("Order placed!");
            conn.Close();
            conn.Dispose();

            Close();
        }
        
        private void CopyDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            if (OrderItemGrid.ItemsSource != null)
            {
                foreach (var item in orderItems)
                {
                    sb.AppendLine($"{item.Qty.ToString()} - {item.CardName} - {item.SetCode.ToUpper()} - {item.IsFoil} - {item.UnitPrice}/{item.TotalPrice}");
                }
                sb.AppendLine($"Total: {(from items in orderItems select items.TotalPrice).Sum().ToString()}");
            }

            string clippy = sb.ToString();
            Clipboard.SetText(clippy);
            MessageBox.Show("Order details copied.");
        }
    }
}