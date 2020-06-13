using Ledger.Classes;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private static string SelectedSet;
        private static List<Export> InvList = new List<Export>();

        public ExportWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                SetBox.ItemsSource = (from data in App.root.Data select data.Code).ToList();
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

                GetPricesButton.IsEnabled = true;
            };
        }

        private async void GetPricesButton_Click(object sender, RoutedEventArgs e)
        {
            GetPricesButton.IsEnabled = false;
            ExportButton.IsEnabled = false;
            UpdateButton.IsEnabled = false;
            PriceGrid.ItemsSource = null;

            DataTable dt = new DataTable();
            string dataJson;

            List<Inventory> Inventory = new List<Inventory>();
            List<Result> PriceLists = new List<Result>();

            using (SqlConnection conn = new SqlConnection(App.connstring))
            {
                SqlCommand pullComm = new SqlCommand($"select * from Inventory where SetCode = '{SetBox.SelectedItem.ToString().ToUpper()}'", conn);
                conn.Open();

                using (SqlDataAdapter da = new SqlDataAdapter(pullComm))
                {
                    da.Fill(dt);
                    dataJson = JsonConvert.SerializeObject(dt, Formatting.Indented);

                    Inventory = JsonConvert.DeserializeObject<List<Inventory>>(dataJson);
                }

                StatBlock.Text = $"0/{Inventory.Count}";
                ProgBar.Maximum = Inventory.Count;

                try
                {
                    foreach (var item in Inventory)
                    {
                        var prices = await App.GetPrice(item.TCGProdId);
                        PriceLists.AddRange(prices.Results);
                        ProgBar.Value += 1;
                        StatBlock.Text = $"{ProgBar.Value}/{Inventory.Count}";
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Please check your internet connection and try again.");
                    ProgBar.Value = 0;
                    GetPricesButton.IsEnabled = true;

                    conn.Close();
                    conn.Dispose();
                }

                MergeData(Inventory, PriceLists);

                StatBlock.Text = "Completed";
                ProgBar.Value = 0;
                GetPricesButton.IsEnabled = true;
                ExportButton.IsEnabled = true;
                UpdateButton.IsEnabled = true;

                conn.Close();
                conn.Dispose();
            }
        }

        private void MergeData(List<Inventory> inventory, List<Result> priceLists)
        {
            var nonfoil = from nf in priceLists
                          where nf.SubTypeName == "Normal"
                          select new { Id = nf.ProductId, Mid = nf.MidPrice };

            List<Price> nfprice = new List<Price>();
            List<Price> fprice = new List<Price>();

            foreach (var item in nonfoil)
            {
                nfprice.Add(new Price() { ID = item.Id, MidPrice = Math.Round(Convert.ToDouble(item.Mid * 50)/5.0)*5 });
            }

            var foil = from f in priceLists
                       where f.SubTypeName == "Foil"
                       select new { Id = f.ProductId, Mid = f.MidPrice };

            foreach (var item in foil)
            {
                fprice.Add(new Price() { ID = item.Id, MidPrice = Math.Round(Convert.ToDouble(item.Mid * 50)/5.0)*5 });
            }

            var nfjoin = from nf in nfprice
                         join card in inventory on nf.ID equals card.TCGProdId
                         select new { card.CardName, nf.ID, NMPrice = nf.MidPrice };

            var fulljoin = from f in fprice
                           join card in nfjoin on f.ID equals card.ID
                           select new { card.CardName, f.ID, card.NMPrice, NMFoil = f.MidPrice };

            InvList = (from data in fulljoin
                       join card in inventory on data.ID equals card.TCGProdId
                       select new Export
                       {
                           CollectorNumber = card.CollectorNumber,
                           CardName = card.CardName,
                           SetCode = card.SetCode,
                           SetName = card.SetName,
                           NMQty = card.NMQty,
                           SPQty = card.SPQty,
                           PLDQty = card.PLDQty,
                           HPQty = card.HPQty,
                           NMPrice = data.NMPrice,
                           SPPrice = Math.Ceiling(data.NMPrice * .9),
                           PLDPrice = Math.Ceiling(data.NMPrice * .8),
                           HPPrice = Math.Ceiling(data.NMPrice * .7),
                           NMFoilQty = card.NMFoilQty,
                           SPFoilQty = card.SPFoilQty,
                           PLDFoilQty = card.PLDFoilQty,
                           HPFoilQty = card.HPFoilQty,
                           NMFoilPrice = data.NMFoil,
                           SPFoilPrice = Math.Ceiling(data.NMFoil * .9),
                           PLDFoilPrice = Math.Ceiling(data.NMFoil * .8),
                           HPFoilPrice = Math.Ceiling(data.NMFoil * .7)
                       }).ToList();

            PriceGrid.ItemsSource = InvList;
        }

        private void PriceGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PriceGrid.SelectedIndex != -1)
            {
                Export expItem = PriceGrid.SelectedItem as Export;

                NMBlock.Text = expItem.NMPrice.ToString();
                SPBlock.Text = expItem.SPPrice.ToString();
                PLDBlock.Text = expItem.PLDPrice.ToString();
                HPBlock.Text = expItem.HPPrice.ToString();

                NMFoilBlock.Text = expItem.NMFoilPrice.ToString();
                SPFoilBlock.Text = expItem.SPFoilPrice.ToString();
                PLDFoilBlock.Text = expItem.PLDFoilPrice.ToString();
                HPFoilBlock.Text = expItem.HPFoilPrice.ToString(); 
            }
            else
            {
                NMBlock.Text = string.Empty; 
                SPBlock.Text = string.Empty;
                PLDBlock.Text = string.Empty;
                HPBlock.Text = string.Empty;
                NMFoilBlock.Text = string.Empty;
                SPFoilBlock.Text = string.Empty;
                PLDFoilBlock.Text = string.Empty;
                HPFoilBlock.Text = string.Empty;
            }

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InvList[PriceGrid.SelectedIndex].NMPrice = Convert.ToDouble(NMBlock.Text);
                InvList[PriceGrid.SelectedIndex].SPPrice = Convert.ToDouble(SPBlock.Text);
                InvList[PriceGrid.SelectedIndex].PLDPrice = Convert.ToDouble(PLDBlock.Text);
                InvList[PriceGrid.SelectedIndex].HPPrice = Convert.ToDouble(HPBlock.Text);
                InvList[PriceGrid.SelectedIndex].NMFoilPrice = Convert.ToDouble(NMFoilBlock.Text);
                InvList[PriceGrid.SelectedIndex].SPFoilPrice = Convert.ToDouble(SPFoilBlock.Text);
                InvList[PriceGrid.SelectedIndex].PLDFoilPrice = Convert.ToDouble(PLDFoilBlock.Text);
                InvList[PriceGrid.SelectedIndex].HPFoilPrice = Convert.ToDouble(HPFoilBlock.Text);

                PriceGrid.SelectedIndex = -1;
                PriceGrid.Items.Refresh();

                MessageBox.Show("Prices updated.");
            }
            catch (Exception)
            {
                MessageBox.Show("Please ensure that the new prices are in numbers only!");
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog()
            { 
                Filter = "(*.csv)|*.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                OverwritePrompt = true
            };

            if (fileDialog.ShowDialog() == true)
            {
                var csv = new StringBuilder();
                csv.AppendLine("CollectorNumber, CardName, SetCode, SetName, NMqty, SPqty, PLDqty, HPqty, NMprice, SPprice, PLDprice, HPprice, NMFqty, SPFqty, PLDFqty, HPFqty, NMFprice, SPFprice, PLDFprice, HPFprice");

                foreach (var item in InvList)
                {
                    var newLine = $"{item.CollectorNumber}, {item.CardName}, {item.SetCode.ToUpper()}, {item.SetName}, " +
                                  $"{item.NMQty.ToString()}, {item.SPQty.ToString()}, {item.PLDQty.ToString()}, {item.HPQty.ToString()}, " +
                                  $"{item.NMPrice.ToString()}, {item.SPPrice.ToString()}, {item.PLDPrice.ToString()}, {item.HPPrice.ToString()}, " +
                                  $"{item.NMFoilQty.ToString()}, {item.SPFoilQty.ToString()}, {item.PLDFoilQty.ToString()}, {item.HPFoilQty.ToString()}, " +
                                  $"{item.NMFoilPrice.ToString()}, {item.SPFoilPrice.ToString()}, {item.PLDFoilPrice.ToString()}, {item.HPFoilPrice.ToString()}";
                    csv.AppendLine(newLine);
                }

                File.WriteAllText(fileDialog.FileName, csv.ToString());
            }

            
            
        }
    }
}
