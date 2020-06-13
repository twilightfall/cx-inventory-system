using System;
using System.Windows;
using System.Data.OleDb;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using Ledger.Classes;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for ImportStocksWindow.xaml
    /// </summary>
    public partial class ImportStocksWindow : Window
    {
        public static List<Inventory> InvList = new List<Inventory>();

        public ImportStocksWindow()
        {
            InitializeComponent();

            Closing += (s, e) => {
                if (Dgrid.ItemsSource != null)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to close this window?", "WARNING!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel)
                        e.Cancel = true; 
                }
            };
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileD = new OpenFileDialog() { DefaultExt = ".xlsx", Filter = "(.xlsx)|*.xlsx" };

            var file = fileD.ShowDialog();

            try
            {
                OleDbConnection conn = new OleDbConnection();
                OleDbCommand comm = new OleDbCommand();

                string filepath = fileD.FileName;
                string connstring = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + filepath + ";Extended Properties=\"Excel 12.0 Xml; HDR=Yes;IMEX=1\"";

                conn.ConnectionString = connstring;
                comm.CommandText = "Select * from [Sheet1$]";

                InvList = ImportInv(comm, conn);

                Dgrid.ItemsSource = InvList;
                UploadDataButton.IsEnabled = true;
            }
            catch (Exception)
            {
                return;
            }
        }

        private List<Inventory> ImportInv(OleDbCommand dbCom, OleDbConnection dbCon)
        {
            #region OLD CODE
            /*OleDbDataAdapter da = new OleDbDataAdapter(dbCom);
                DataSet excelset = new DataSet();

                using (dbCon)
                {
                    dbCon.Open();
                    da.Fill(excelset);
                }
                dbCon.Close();
                return excelset.Tables[0];*/ 
            #endregion

            List<Inventory> InvList = new List<Inventory>();
            dbCom.Connection = dbCon;
            dbCon.Open();

            var reader = dbCom.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    InvList.Add(new Inventory()
                    {
                        CollectorNumber = Convert.ToString(reader["CollectorNumber"]),
                        CardName = Convert.ToString(reader["CardName"]),
                        SetCode = Convert.ToString(reader["SetCode"]),
                        SetName = Convert.ToString(reader["SetName"]),
                        NMQty = Convert.ToInt32(reader["NMQty"]),
                        SPQty = Convert.ToInt32(reader["SPQty"]),
                        PLDQty = Convert.ToInt32(reader["PLDQty"]),
                        HPQty = Convert.ToInt32(reader["HPQty"]),
                        NMFoilQty = Convert.ToInt32(reader["NMFoilQty"]),
                        SPFoilQty = Convert.ToInt32(reader["SPFoilQty"]),
                        PLDFoilQty = Convert.ToInt32(reader["PLDFoilQty"]),
                        HPFoilQty = Convert.ToInt32(reader["HPFoilQty"])
                    });
                }
                catch (Exception x)
                {
                    MessageBox.Show("Something went wrong. Double check upload file to ensure no blank spaces.", "WARNING!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    throw;
                }
            }
            reader.Close();
            
            return InvList;
        }

        private void UploadDataButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            dt = InvList.ToDataTable();
            dt.Columns.Remove("InventoryId");
            dt.Columns.Remove("TCGProdId");

            SqlConnection conn = new SqlConnection(App.connstring);

            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                conn.Open();
                bulk.DestinationTableName = "Inventory";
                bulk.ColumnMappings.Add("CollectorNumber", "CollectorNumber");
                bulk.ColumnMappings.Add("CardName", "CardName");
                bulk.ColumnMappings.Add("SetCode", "SetCode");
                bulk.ColumnMappings.Add("SetName", "SetName");
                bulk.ColumnMappings.Add("NMQty", "NMQty");
                bulk.ColumnMappings.Add("SPQty", "SPQty");
                bulk.ColumnMappings.Add("PLDQty", "PLDQty");
                bulk.ColumnMappings.Add("HPQty", "HPQty");
                bulk.ColumnMappings.Add("NMFoilQty", "NMFoilQty");
                bulk.ColumnMappings.Add("SPFoilQty", "SPFoilQty");
                bulk.ColumnMappings.Add("PLDFoilQty", "PLDFoilQty");
                bulk.ColumnMappings.Add("HPFoilQty", "HPFoilQty");
                try
                {
                    bulk.WriteToServer(dt);
                }
                catch (Exception)
                {
                }
            }
            conn.Close();
            conn.Dispose();

            MessageBox.Show("File successfully uploaded to database", "SUCCESS", MessageBoxButton.OK);

            Dgrid.ItemsSource = null;
            InvList = null;
            UploadDataButton.IsEnabled = false;
        }

        
    }
}
