using Ledger.Classes;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for StocksWindow.xaml
    /// </summary>
    public partial class StocksWindow : Window
    {
        //private static SetList root = new SetList();

        public StocksWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
             {
                 try
                 {
                     var setCodes = (from code in App.root.Data select code.Code).ToList();

                     SetBox.ItemsSource = setCodes;
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show(ex.Message);
                     return;
                 }
             };

            SetBox.SelectionChanged += (s, ev) => { SetNameBox.Clear(); };

            SetNameBox.TextChanged += (s, ex) => { SetBox.SelectedIndex = -1; };

            NameSearchBox.TextChanged += (s, eh) =>
            {
                SetBox.SelectedIndex = -1;
                SetNameBox.Clear();
            };
        }

        private void SearchSetsButton_Click(object sender, RoutedEventArgs e)
        {
            //SearchNameButton.IsEnabled = false;
            InvGrid.ItemsSource = null;

            DataTable dt = new DataTable();
            SqlConnection sConn;
            string comm;

            if (SetBox.SelectedIndex.Equals(-1) && string.IsNullOrWhiteSpace(SetNameBox.Text))
            {
                MessageBox.Show("No search parameter selected", "WARNING", MessageBoxButton.OK);
                return;
            }
            else if (SetBox.SelectedIndex > -1)
            {
                comm = "Select * from dbo.Inventory where SetCode = '" + SetBox.SelectedItem.ToString().ToUpper() + "'";
                sConn = new SqlConnection(App.connstring);
                SqlCommand sComm = new SqlCommand(comm, sConn);
                sConn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(sComm))
                {
                    da.Fill(dt);
                    dt.Columns.Remove("InventoryId");
                    InvGrid.ItemsSource = dt.AsDataView();
                }
                sConn.Close();
                sConn.Dispose();
            }
            else if (!string.IsNullOrWhiteSpace(SetNameBox.Text))
            {
                comm = "Select * from dbo.Inventory where SetName like '%" + SetNameBox.Text + "%'";
                sConn = new SqlConnection(App.connstring);
                SqlCommand sComm = new SqlCommand(comm, sConn);
                sConn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(sComm))
                {
                    da.Fill(dt);
                    dt.Columns.Remove("InventoryId");
                    InvGrid.ItemsSource = dt.AsDataView();
                }
                sConn.Close();
                sConn.Dispose();
            }

            SetBox.SelectedIndex = -1;
            SetNameBox.Clear();
            NameSearchBox.Clear();
            //SearchNameButton.IsEnabled = true;
        }

        private void SearchNameButton_Click(object sender, RoutedEventArgs e)
        {
            //SearchSetsButton.IsEnabled = false;
            InvGrid.ItemsSource = null;

            DataTable dt = new DataTable();
            string comm;

            if (!string.IsNullOrWhiteSpace(NameSearchBox.Text))
            {
                comm = "Select * from dbo.Inventory where CardName like '%" + NameSearchBox.Text + "%'";
            }
            else
            {
                MessageBox.Show("No search parameter selected", "WARNING", MessageBoxButton.OK);
                return;
            }

            SqlConnection sConn = new SqlConnection(App.connstring);
            SqlCommand sComm = new SqlCommand(comm, sConn);

            sConn.Open();
            using (SqlDataAdapter da = new SqlDataAdapter(sComm))
            {
                da.Fill(dt);
                dt.Columns.Remove("InventoryId");
                InvGrid.ItemsSource = dt.AsDataView();
            }
            sConn.Close();
            sConn.Dispose();

            NameSearchBox.Clear();
            //SearchSetsButton.IsEnabled = true;
        }

        private void GetIDButton_Click(object sender, RoutedEventArgs e)
        {
            TCGIDPage IDPage = new TCGIDPage() { Owner = this };
            IDPage.ShowDialog();
        }

        private void AddStocksButton_Click(object sender, RoutedEventArgs e)
        {
            AddStocksWindow addS = new AddStocksWindow() { Owner = this };
            addS.ShowDialog();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            ExportWindow exp = new ExportWindow() { Owner = this };
            exp.ShowDialog();
        }
    }
}
