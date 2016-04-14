using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Tamir.SharpSsh;

namespace Order_Manager.channel.giantTiger
{
    /*
     * A class that connect to Giant Tiger sftp server and manage all the orders for Giant Tiger
     */
    public class GiantTiger : Channel
    {
        // fields for directory on sftp server
        private const string GET_DIR = "/get";
        private const string SHIP_DIR = "/put/ship";
        private const string CANCEL_DIR = "/put/cancel";

        // field for directory on local
        private readonly string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\GiantTigerOrders";
        private readonly string newOrderDir;
        private readonly string completeOrderDir;

        // field for sftp connection
        private readonly Sftp sftp;

        /* constructor that initialize folders for csv feed */
        public GiantTiger()
        {
            #region Folder Check
            // check and generate folders
            if (!Directory.Exists(rootDir))
                Directory.CreateDirectory(rootDir);

            newOrderDir = rootDir + "\\GiantTigerNewOrders";
            if (!Directory.Exists(newOrderDir))
                Directory.CreateDirectory(newOrderDir);

            completeOrderDir = rootDir + "\\GiantTigerCompleteOrders";
            if (!Directory.Exists(completeOrderDir))
                Directory.CreateDirectory(completeOrderDir);
            #endregion

            // get credentials for giant tiger sftp log on and initialize the field
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ASCMcs))
            {
                SqlCommand command = new SqlCommand("SELECT Field1_Value, Field2_Value, Field3_Value FROM ASCM_Credentials WHERE Source = 'Vendornet' and Client = 'GiantTiger'", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                // initialize Sftp
                sftp = new Sftp(reader.GetString(0), reader.GetString(1), reader.GetString(2));
            }
        }

        #region Public Get
        /* a method that get all new order on the server and update to the database */
        public void GetOrder()
        {
            // get all the new file on the order directory to local new order storing directory
            // IEnumerable<string> orderCheck = CheckOrderFile();
            // GetOrder(newOrderDir, orderCheck);

            // return the transaction that haved not been processed
        }

        /* a method that return all the new order values */
        public GiantTigerValues[] GetAllNewOrder()
        {
            // local field for storing order values 
            List<GiantTigerValues> list = new List<GiantTigerValues>();
            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT PoNumber, WebOrderNo, OrderDate, ShipMethod, ShipToFirstName, ShipToLastName, ShipToStreet, ShipToAddress2, ShipToCity, ShipToZip, ShipToPhone, " +
                                                    "ShipToStoreNumber, OmsOrderNumber" +
                                                    "FROM GiantTiger_Order WHERE Complete ='False' ORDER BY PoNumber", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GiantTigerValues value = new GiantTigerValues();

                    string poNumber = reader.GetString(0);
                    value.PoNumber = poNumber;
                    value.WebOrderNo = reader.GetString(1);
                    value.OrderDate = reader.GetDateTime(2);
                    value.ShipMethod = reader.GetString(3);
                    value.ShipTo.Name = reader.GetString(4) + ' ' + reader.GetString(5);
                    value.ShipTo.Address1 = reader.GetString(6);
                    value.ShipTo.Address2 = reader.GetString(7);
                    value.ShipTo.City = reader.GetString(8);
                    value.ShipTo.PostalCode = reader.GetString(9);
                    value.ShipTo.DayPhone = reader.GetString(10);
                    value.StoreNumber = reader.GetString(11);
                    value.OmsOrderNumber = reader.GetString(12);

                    SqlDataAdapter adatper = new SqlDataAdapter("SELECT VendorSku, HostSku, Quantity, UnitCost, ClientItemId " +
                                                                "FROM GiantTiger_Order_Item WHERE PoNumber = \'" + poNumber + '\'', connection);
                    adatper.Fill(table);

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        value.VendorSku.Add(table.Rows[i][0].ToString());
                        value.HostSku.Add(table.Rows[i][1].ToString());
                        value.Quantity.Add(Convert.ToInt32(table.Rows[i][2]));
                        value.UnitCost.Add(Convert.ToDouble(table.Rows[i][3]));
                        value.ClientItemId.Add(table.Rows[i][4].ToString());
                    }

                    table.Reset();

                    list.Add(value);
                }
            }

            return list.ToArray();
        }

        /* a method that return all shipped order */
        public GiantTigerValues[] GetAllShippedOrder()
        {
            return null;
        }
        #endregion

        #region Ship and Void
        /* a method that mark the order as shipped but not posting a confirm order to shop.ca only for local reference */
        public void PostShip(string trackingNumber, string selfLink, string labelLink, string poNumber)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("UPDATE GiantTiger_Order SET TrackingNumber = \'" + trackingNumber + "\', SelfLink = \'" + selfLink + "\', LabelLink = \'" + labelLink + "\', "
                                                  + "ShipDate = \'" + DateTime.Today.ToString("yyyy-MM-dd") + "\' WHERE PoNumber = \'" + poNumber + '\'', connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        /* a method that mark the order as end of day, so it cannot be voided anymore */
        public void PostShip(bool endOfDay, DateTime shipDate)
        {
            string date = shipDate.ToString("yyyy-MM-dd");
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("UPDATE GiantTiger_Order SET EndofDay = \'" + endOfDay + "\' WHERE ShipDate = \'" + date + "\' OR CompleteDate = \'" + date + "\'", connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        /* a method that mark the order as cancelled but not posting a cancel order to shop.ca only for local reference */
        public void PostVoid(string[] poNumber)
        {
            // generate the range 
            string candidate = poNumber.Aggregate("(", (current, id) => current + ('\'' + id + "\',"));
            candidate = candidate.Remove(candidate.Length - 1) + ')';

            // update to not shipped 
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                // for entire order cancellation
                SqlCommand command = new SqlCommand("UPDATE GiantTiger_Order SET TrackingNumber = '', SelfLink = '', LabelLink = '', ShipDate = NULL WHERE OrderId IN " + candidate, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        #endregion

        #region Number of Orders and Shipments
        /* methods that return the number of order and shipment from the given date */
        public int GetNumberOfOrder(DateTime time)
        {
            int count;

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM GiantTiger_Order WHERE OrderDate = \'" + time.ToString("yyyy-MM-dd") + '\'', connection);
                connection.Open();

                count = (int)command.ExecuteScalar();
            }

            return count;
        }
        public int GetNumberOfShipped(DateTime time)
        {
            int count;

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM GiantTiger_Order WHERE Complete = 'True' AND OrderDate = \'" + time.ToString("yyyy-MM-dd") + '\'', connection);
                connection.Open();

                count = (int)command.ExecuteScalar();
            }

            return count;
        }
        #endregion

        #region Get Order Infomation
        /* method that get the new order from sftp server */
        private void GetOrder(string filePath, IEnumerable<string> fileList)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();

            foreach (string file in fileList)
            {
                if (file == "." || file == "..") continue;

                // change file to txt and save file to local
                string fileNameCsv = file.Replace("txt", "csv");
                sftp.Get(SHIP_DIR + "/" + file, filePath + "\\" + fileNameCsv);

                // after download the file delete it on the server (no need it anymore)
                // ServerDelete.Delete(sftp.Host, sftp.Username, sftp.Password, SHIP_DIR + "/" + file);
            }

            sftp.Close();
        }

        /* method that get the shipment order file name from sftp server */
        private IEnumerable<string> GetOrderName(string serverDir)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();
            ArrayList list = sftp.GetFileList(serverDir);
            sftp.Close();

            // retrieve all the files name to the list
            return (from object item in list select item.ToString()).ToArray();
        }

        /* method that return <poNumber, filepath> */
        private Dictionary<string, string> GetPoNumber()
        {
            // get all the order file on local
            DirectoryInfo dirInfo = new DirectoryInfo(newOrderDir);
            FileInfo[] filesLocal = dirInfo.GetFiles("*.csv");       // getting all file that have been on local

            // initialize field for return
            Dictionary<string, string> dic = new Dictionary<string, string>();

            // retrieving data
            foreach (FileInfo file in filesLocal)
            {
                // read the csv file to get all po number
                TextFieldParser parser = new TextFieldParser(file.FullName);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                
            }

            return null;
        }
        #endregion

        #region Check Order Methods
        /* return all the new order file name on the server */
        private IEnumerable<string> CheckOrderFile()
        {
            // get all order file on local
            DirectoryInfo dirInfo = new DirectoryInfo(newOrderDir);
            FileInfo[] filesLocal = dirInfo.GetFiles("*.csv");       // getting all file that have been on local

            // get all order file on server
            IEnumerable<string> fileOnServer = GetOrderName(SHIP_DIR);

            // check the number of new order on the server compare to ones on the computer
            return (from file1 in fileOnServer let found = filesLocal.Select(file2 => file2.ToString()).Any(file2Copy => file1.Remove(file1.LastIndexOf('.')) == file2Copy.Remove(file2Copy.LastIndexOf('.'))) where !found select file1).ToArray();
        }

        /* a method that receive all the current order and check the duplicate then only return the ones that have not been processed 
        -> receive and return dictionary <orderId, filePath> */
        private static Dictionary<string, string> CheckOrder(Dictionary<string, string> allOrderList)
        {
            // get all complete order id 
            List<string> completeOrderList = new List<string>();
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT OrderId FROM ShopCa_Order;", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    completeOrderList.Add(reader.GetString(0));
            }

            // compare allOrderList's order id with the completeOrderList's order id
            return (from keyValue in allOrderList let found = completeOrderList.Any(completeOrder => keyValue.Key == completeOrder) where !found select keyValue).ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value);
        }
        #endregion

        #region CSV Generation
        /* a method that generate csv order and upload to the sftp server and update database */
        public void GenerateCsv(GiantTigerValues value, Dictionary<int, string> cancelList)
        {
            
        }

        /* a method that generate GiantTigerValues object for the given po number (first version -> take from local) */
        public GiantTigerValues GenerateValue(string targetPo, string filePath)
        {
            // field for return
            GiantTigerValues value = new GiantTigerValues();

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // local field for header data
                Dictionary<int, string> headerList = new Dictionary<int, string>();

                // getting headers and its coressponding index
                string[] fields = parser.ReadFields();
                int length = fields.Length;
                for (int i = 0; i < length; i++)
                    headerList.Add(i, fields[i]);

                // getting order items
                while (!parser.EndOfData)
                {
                    // get the data for the order
                    string[] data = parser.ReadFields();

                    // the case if the order is not desired -> skip this
                    if (targetPo != data[0]) continue;

                    for (int i = 0; i < length; i++)
                    {
                        switch (headerList[i])
                        {
                            case "PO#":
                                value.PoNumber = data[i];
                                break;
                            case "WebOrderNo":
                                value.WebOrderNo = data[i];
                                break;
                            case "OrderDate":
                                value.OrderDate = DateTime.ParseExact(data[i], "M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                break;
                            case "ShipMethod":
                                value.ShipMethod = data[i];
                                break;
                            case "Ship-ToFName":
                                value.ShipTo.Name = data[i];
                                break;
                            case "Ship-ToLName":
                                value.ShipTo.Name += ' ' + data[i];
                                break;
                            case "Ship-ToStreet":
                                value.ShipTo.Address1 = data[i];
                                break;
                            case "Ship-ToAddr2":
                                value.ShipTo.Address2 = data[i];
                                break;
                            case "Ship-ToCity":
                                value.ShipTo.City = data[i];
                                break;
                            case "ShipToState":
                                value.ShipTo.State = data[i];
                                break;
                            case "Ship-ToZip":
                                value.ShipTo.PostalCode = data[i];
                                break;
                            case "Ship-ToPhone":
                                value.ShipTo.DayPhone = data[i];
                                break;
                            case "Store#":
                                value.StoreNumber = data[i];
                                break;
                            case "VendorSKU#":
                                value.VendorSku.Add(data[i]);
                                break;
                            case "Quantity":
                                value.Quantity.Add(int.Parse(data[i]));
                                break;
                            case "UnitCost":
                                value.UnitCost.Add(double.Parse(data[i]));
                                break;
                            case "HostSKU#":
                                value.HostSku.Add(data[i]);
                                break;
                            case "ClientItemID":
                                value.ClientItemId.Add(data[i]);
                                break;
                            case "OMSOrder#":
                                value.OmsOrderNumber = data[i];
                                break;
                        }
                    }
                }
            }

            return value;
        }
        /* second version -> take from database */
        public GiantTigerValues GenerateValue(string targetPo)
        {
            // local field for storing values
            GiantTigerValues value = new GiantTigerValues();

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT PoNumber, WebOrderNo, OrderDate, ShipMethod, ShipToFirstName, ShipToLastName, ShipToStreet, ShipToAddress2, ShipToCity, ShipToZip, ShipToPhone, " +
                                                    "ShipToStoreNumber, OmsOrderNumber, TrackingNumber, SelfLink, LabelLink " +
                                                    "FROM GiantTiger_Order WHERE PoNumber = \'" + targetPo + '\'', connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                string poNumber = reader.GetString(0);
                value.PoNumber = poNumber;
                value.WebOrderNo = reader.GetString(1);
                value.OrderDate = reader.GetDateTime(2);
                value.ShipMethod = reader.GetString(3);
                value.ShipTo.Name = reader.GetString(4) + ' ' + reader.GetString(5);
                value.ShipTo.Address1 = reader.GetString(6);
                value.ShipTo.Address2 = reader.GetString(7);
                value.ShipTo.City = reader.GetString(8);
                value.ShipTo.PostalCode = reader.GetString(9);
                value.ShipTo.DayPhone = reader.GetString(10);
                value.StoreNumber = reader.GetString(11);
                value.OmsOrderNumber = reader.GetString(12);
                value.Package.TrackingNumber = reader.GetString(13);
                value.Package.SelfLink = reader.GetString(14);
                value.Package.LabelLink = reader.GetString(15);

                SqlDataAdapter adatper = new SqlDataAdapter("SELECT VendorSku, HostSku, Quantity, UnitCost, ClientItemId " +
                                                            "FROM GiantTiger_Order_Item WHERE PoNumber = \'" + poNumber + "\' ORDER BY ClientItemId", connection);
                DataTable table = new DataTable();
                adatper.Fill(table);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    value.VendorSku.Add(table.Rows[i][0].ToString());
                    value.HostSku.Add(table.Rows[i][1].ToString());
                    value.Quantity.Add(Convert.ToInt32(table.Rows[i][2]));
                    value.UnitCost.Add(Convert.ToDouble(table.Rows[i][3]));
                    value.ClientItemId.Add(table.Rows[i][4].ToString());
                }
            }

            return value;
        }
        #endregion

        /* a method that add a new order to database */
        private static void AddNewOrder(GiantTigerValues value)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                // add new order to database
                string firstName = value.ShipTo.Name.Remove(value.ShipTo.Name.IndexOf(' '));
                string lastName = value.ShipTo.Name.Substring(value.ShipTo.Name.IndexOf(' '));
                SqlCommand command = new SqlCommand("INSERT INTO GiantTiger_Order " +
                                                    "(PoNumber, WebOrderNo, OrderDate, ShipMethod, ShipToFirstName, ShipToLastName, ShipToStreet, ShipToAddress2, ShipToCity, ShipToState, ShipToZip, ShipToCountry, ShipToPhone, ShipToStoreNumber, OmsOrderNumber, Complete, TrackingNumber, SelfLink, LabelLink, EndofDay) Values " +
                                                    "(\'" + value.PoNumber + "\',\'" + value.WebOrderNo + "\',\'" + value.OrderDate.ToString("yyyy-MM-dd") + "\',\'" + value.ShipMethod + "\',\'" + firstName.Replace("'", "''") + "\',\'" + lastName.Replace("'", "''") + "\',\'" + value.ShipTo.Address1.Replace("'", "''") + "\',\'" + value.ShipTo.Address2.Replace("'", "''") + "\',\'" + value.ShipTo.City.Replace("'", "''") +
                                                    "\',\'" + value.ShipTo.State + "\',\'" + value.ShipTo.PostalCode + "\','Canada',\'" + value.ShipTo.DayPhone + "\',\'" + value.StoreNumber + "\',\'" + value.OmsOrderNumber + "\','False',\'" + value.Package.TrackingNumber + "\',\'" + value.Package.LabelLink + "\',\'" + value.Package.LabelLink + "\','False')", connection);
                connection.Open();
                command.ExecuteNonQuery();

                // add each item for the order to database
                for (int i = 0; i < value.VendorSku.Count; i++)
                {
                    command.CommandText = "INSERT INTO GiantTiger_Order_Item " +
                                          "(PoNumber, VendorSku, HostSku, Quantity, UnitCost, ClientItemId, Shipped, Cancelled) Values" +
                                          "(\'" + value.PoNumber + "\',\'" + value.VendorSku[i] + "\',\'" + value.HostSku[i] + "\'," + value.Quantity[i] + ',' + value.UnitCost[i] + ",\'" + value.ClientItemId[i] + "\','False', 'False')";
                    command.ExecuteNonQuery();
                }
            }
        }

        /* a method that delete obsolete orders in database and clear all local files */
        public void Delete()
        {
            // Local Delete
            new DirectoryInfo(newOrderDir).Delete(true);
        }
    }
}
