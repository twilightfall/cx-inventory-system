using Ledger.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Windows;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for AddStocksPage.xaml
    /// </summary>
    public partial class AddStocksWindow : Window
    {
        public static List<Stock> InvList = new List<Stock>();

        public AddStocksWindow()
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

        private List<Stock> ImportInv(OleDbCommand dbCom, OleDbConnection dbCon)
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

            List<Stock> InvList = new List<Stock>();
            dbCom.Connection = dbCon;
            dbCon.Open();

            var reader = dbCom.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    InvList.Add(new Stock()
                    {
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
                catch (Exception)
                {
                    MessageBox.Show("Something went wrong. Double check upload file to ensure no blank spaces.", "WARNING!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    reader.Close();
                }
            }
            reader.Close();

            return InvList;
        }

        private void UploadDataButton_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(App.connstring))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand() { Connection = conn };
                
                foreach (var item in InvList)
                {
                    string commtext = $"update Inventory set " +
                                      $"NMQty = NMQty + {item.NMQty}, " +
                                      $"SPQty = SPQty + {item.SPQty}, " +
                                      $"PLDQty = PLDQty + {item.PLDQty}, " +
                                      $"HPQty = HPQty + {item.HPQty}, " +
                                      $"NMFoilQty = NMFoilQty + {item.NMFoilQty} " +
                                      $"SPFoilQty = SPFoilQty + {item.SPFoilQty}" +
                                      $"PLDFoilQty = PLDFoilQty + {item.PLDFoilQty} " +
                                      $"HPFoilQty = HPFoilQty + {item.HPFoilQty}" +
                                      $"where CardName = {item.CardName} and SetCode = {item.SetCode}";
                    comm.CommandText = commtext;

                    comm.ExecuteNonQuery();
                }
                MessageBox.Show("Stocks updated.");
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
