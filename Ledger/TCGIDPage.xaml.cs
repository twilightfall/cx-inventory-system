using Ledger.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for TCGIDPage.xaml
    /// </summary>
    public partial class TCGIDPage : Window
    {
        private static string SelectedSet;

        public TCGIDPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                SetBox.ItemsSource = (from data in App.root.Data
                                      select data.Code).ToList();

                NoIDGrid.ItemsSource = LoadSetsWithout().AsDataView();
                WithIDGrid.ItemsSource = LoadSetsWith().AsDataView();
            };

            SetBox.SelectionChanged += (s, ev) =>
            {
                if (SetBox.SelectedIndex != -1)
                {
                    SetNameBlock.Text = (from sets in App.root.Data
                                         where sets.Code == SetBox.SelectedItem.ToString()
                                         select sets.Name).FirstOrDefault();
                    SelectedSet = SetBox.SelectedItem.ToString().ToUpper();
                }
                else
                {
                    SetNameBlock.Text = string.Empty;
                    SelectedSet = null;
                }
                
                GetIDButton.IsEnabled = true;
            };
        }

        private async void GetIDButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetBox.SelectedIndex == -1)
            {
                MessageBox.Show("No search parameter selected. Select a set code and try again.");
                return;
            }
            else
            {
                GetIDButton.IsEnabled = false;
                var cards = await GetCardsAsync();
                var numname = (from card in cards
                               select new { card.Collector_number, card.Name }).ToList();
                string commtext;

                using (SqlConnection conn = new SqlConnection(App.connstring))
                {
                    conn.Open();
                    SqlCommand comm = new SqlCommand { Connection = conn, CommandType = CommandType.Text };

                    foreach (var item in cards)
                    {
                        commtext = string.Format("UPDATE Inventory set TCGProdId = '{0}' " +
                                                 "where SetCode = '{1}' " +
                                                 "and CollectorNumber = '{2}'", 
                                                 item.Tcgplayer_id, SetBox.SelectedItem.ToString().ToUpper(), item.Collector_number);
                        comm.CommandText = commtext;

                        var x = comm.ExecuteNonQuery();
                    }
                    conn.Close();
                    conn.Dispose();
                }

                NoIDGrid.ItemsSource = LoadSetsWithout().AsDataView();
                WithIDGrid.ItemsSource = LoadSetsWith().AsDataView();

                MessageBox.Show("Product IDs updated successfully", "NOTICE");
                SetBox.SelectedIndex = -1;
                GetIDButton.IsEnabled = true;
            }
        }

        public static async Task<List<Cards>> GetCardsAsync(Action<CardList> callBack = null)
        {
            var cardlist = new List<Cards>();
            HttpClient httpClient = new HttpClient();
            string nextUrl = "https://api.scryfall.com/cards/search?order=name&q=set%3A" + SelectedSet + "+unique%3Aprints";

            do
            {
                await httpClient.GetAsync(nextUrl)
                    .ContinueWith(async (searchTask) =>
                    {
                        var response = await searchTask;
                        if (response.IsSuccessStatusCode)
                        {
                            string jsonString = await response.Content.ReadAsStringAsync();
                            var res = JsonConvert.DeserializeObject<CardList>(jsonString);
                            if (res != null)
                            {
                                if (res.Data.Any())
                                    cardlist.AddRange(res.Data.ToList());
                                callBack?.Invoke(res);

                                nextUrl = (res.Has_more != false) ? res.Next_page : string.Empty;
                            }
                        }
                        else
                        {
                            nextUrl = string.Empty;
                        }
                    });
            } while (!string.IsNullOrEmpty(nextUrl));

            //var sorted = cardlist.OrderBy(x => Convert.ToInt32(x.Collector_number)).ToList();
            return cardlist;
        }

        public static DataTable LoadSetsWithout()
        {
            string commtext = @"select distinct SetCode, SetName from dbo.Inventory where TCGProdID is null";

            using (SqlConnection conn = new SqlConnection(App.connstring))
            {
                DataTable dt = new DataTable();
                SqlCommand comm = new SqlCommand(commtext, conn);
                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(comm)) { da.Fill(dt); }

                conn.Close();
                conn.Dispose();
                return dt;
            }
        }

        public static DataTable LoadSetsWith()
        {
            string commtext = @"select distinct SetCode, SetName from dbo.Inventory where TCGProdId is not null";

            using (SqlConnection conn = new SqlConnection(App.connstring))
            {
                DataTable dt = new DataTable();
                SqlCommand comm = new SqlCommand(commtext, conn);
                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(comm)) { da.Fill(dt); }
                conn.Close();
                conn.Dispose();
                return dt;
            }
        }
    }
}
