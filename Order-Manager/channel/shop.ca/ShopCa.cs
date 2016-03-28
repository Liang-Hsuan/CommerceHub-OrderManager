﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Xml;
using Tamir.SharpSsh;

namespace Order_Manager.channel.shop.ca
{
    /*
     * A class that connect to Shop.ca sftp server and manage all the orders for Shop.ca
     */
    public class ShopCa
    {
        // fields for directory on sftp server
        private const string SHIPMENT_DIR = "toclient/order";
        private const string CONFIRM_DIR = "fromclient/order_status";

        // field for directory on local
        private readonly string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\ShopCaOrders";
        private readonly string newOrderDir;
        private readonly string completeOrderDir;

        // field for sftp connection
        private readonly Sftp sftp;

        /* constructor that initialize folders for xml feed */
        public ShopCa()
        {
            #region Folder Check
            // check and generate folders
            if (!Directory.Exists(rootDir))
                Directory.CreateDirectory(rootDir);

            newOrderDir = rootDir + "\\ShopCaNewOrders";
            if (!Directory.Exists(newOrderDir))
                Directory.CreateDirectory(newOrderDir);

            completeOrderDir = rootDir + "\\ShopCaCompleteOrders";
            if (!Directory.Exists(completeOrderDir))
                Directory.CreateDirectory(completeOrderDir);
            #endregion

            // get credentials for shop.ca sftp log on and initialize the field
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ASCMcs))
            {
                SqlCommand command = new SqlCommand("SELECT Field1_Value, Username, Password From ASCM_Credentials WHERE Source='Shop.ca - SFTP';", connection);
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
            string[] orderCheck = checkOrderFile();
            getOrder(newOrderDir, orderCheck);

            // read all the text of the file in the local directory
            Dictionary<string,string> dic = getOrderFileText();

            // return the transaction that haved not been processed
            dic = getOrderId(dic);
            dic = checkOrder(dic);

            // get information for each unprocessed order and update the them to the database
            foreach (KeyValuePair<string, string> keyValue in dic)
            {
                ShopCaValues value = GenerateValue(keyValue.Key, keyValue.Value);
                addNewOrder(value);
            }
        }

        /* a method that return all the new order values */
        public ShopCaValues[] GetAllNewOrder()
        {
            // local field for storing order values 
            List<ShopCaValues> list = new List<ShopCaValues>();
            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT OrderId, SupplierId, StoreName, OrderCreateDate, GrandTotal, TotalPrice, TotalTax, TotalShippingCost, TotalDiscount, BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToPostalCode, BillToPhone, " +
                                                    "ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToPostalCode, ShipToPhone, OptionIn, ShippingMethod " +
                                                    "FROM ShopCa_Order WHERE Complete ='False' ORDER BY OrderId;", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ShopCaValues value = new ShopCaValues();

                    string orderId = reader.GetString(0);
                    value.OrderId = orderId;
                    value.SupplierId = reader.GetString(1);
                    value.StoreName = reader.GetString(2);
                    value.OrderCreateDate = reader.GetDateTime(3);
                    value.GrandTotal = reader.GetDecimal(4);
                    value.TotalPrice = reader.GetDecimal(5);
                    value.TotalTax = reader.GetDecimal(6);
                    value.TotalShippingCost = reader.GetDecimal(7);
                    value.TotalDiscount = reader.GetDecimal(8);
                    value.BillTo.Name = reader.GetString(9);
                    value.BillTo.Address1 = reader.GetString(10);
                    value.BillTo.Address2 = reader.GetString(11);
                    value.BillTo.City = reader.GetString(12);
                    value.BillTo.State = reader.GetString(13);
                    value.BillTo.PostalCode = reader.GetString(14);
                    value.BillTo.DayPhone = reader.GetString(15);
                    value.ShipTo.Name = reader.GetString(16);
                    value.ShipTo.Address1 = reader.GetString(17);
                    value.ShipTo.Address2 = reader.GetString(18);
                    value.ShipTo.City = reader.GetString(19);
                    value.ShipTo.State = reader.GetString(20);
                    value.ShipTo.PostalCode = reader.GetString(21);
                    value.ShipTo.DayPhone = reader.GetString(22);
                    value.Option = reader.GetBoolean(23);
                    value.ShippingMethod = reader.GetString(24);

                    SqlDataAdapter adatper = new SqlDataAdapter("SELECT OrderItemId, Sku, Title, Ssrp, SsrpTax, Quantity, ItemPrice, ExtendedItemPrice, ItemTax, ItemShippingCost, ItemDiscount " +
                                                                "FROM ShopCa_Order_Item WHERE OrderId = \'" + orderId + "\';", connection);
                    adatper.Fill(table);

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        value.OrderItemId.Add(table.Rows[i][0].ToString());
                        value.Sku.Add(table.Rows[i][1].ToString());
                        value.Title.Add(table.Rows[i][2].ToString());
                        value.Ssrp.Add(Convert.ToDecimal(table.Rows[i][3]));
                        value.SsrpTax.Add(Convert.ToDecimal(table.Rows[i][4]));
                        value.Quantity.Add(Convert.ToInt32(table.Rows[i][5]));
                        value.ItemPrice.Add(Convert.ToDecimal(table.Rows[i][6]));
                        value.ExtendedItemPrice.Add(Convert.ToDecimal(table.Rows[i][7]));
                        value.ItemTax.Add(Convert.ToDecimal(table.Rows[i][8]));
                        value.ItemShippingCost.Add(Convert.ToDecimal(table.Rows[i][9]));
                        value.ItemDiscount.Add(Convert.ToDecimal(table.Rows[i][10]));
                    }

                    table.Reset();

                    list.Add(value);
                }
            }

            return list.ToArray();
        }
        #endregion

        /* a method that mark the order as shipped but not posting a confirm order to shop.ca only for local reference */
        public void PostShip(string trackingNumber, string refundLink, string labelLink, string orderId)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("UPDATE ShopCa_Order SET TrackingNumber = \'" + trackingNumber + "\', RefundLink = \'" + refundLink + "\', LabelLink = \'" + labelLink + "\' "
                                                  + "WHERE TransactionId = \'" + orderId + "\';", connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        #region Number of Orders and Shipments
        /* methods that return the number of order and shipment from the given date */
        public int GetNumberOfOrder(DateTime time)
        {
            int count;

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM ShopCa_Order WHERE OrderCreateDate = \'" + time.ToString("yyyy-MM-dd") + "\';", connection);
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
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM ShopCa_Order WHERE Complete = 'True' AND OrderCreateDate = \'" + time.ToString("yyyy-MM-dd") + "\';", connection);
                connection.Open();

                count = (int)command.ExecuteScalar();
            }

            return count;
        }
        #endregion

        #region Get Order Infomation
        /* method that get the new order from sftp server */
        private void getOrder(string filePath, string[] fileList)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();

            // download the files from the given list
            foreach (string file in fileList)
            {
                if (file == "." || file == "..") continue;
                sftp.Get(SHIPMENT_DIR + "/" + file, filePath + "\\" + file);

                // after download the file delete it on the server (no need it anymore)
                // ServerDelete.delete(sftp.Host, sftp.Username, sftp.Password, SHIPMENT_DIR + "/" + file);
            }

            sftp.Close();
        }

        /* method that get the shipment order file name from sftp server */
        private string[] getOrderName(string serverDir)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();
            ArrayList list = sftp.GetFileList(serverDir);
            sftp.Close();

            // retrieve all the files name to the list
            List<string> fileList = new List<string>();
            foreach (var item in list)
                fileList.Add(item.ToString());

            return fileList.ToArray();
        }

        /* method that get all the order in the file with the given dictionary <filePath, text> and return dictionary <orderId, filepath> */
        public static Dictionary<string, string> getOrderId(Dictionary<string, string> fileText)
        {
            // local field for storing data
            Dictionary<string, string> list = new Dictionary<string, string>();

            // get all order id and its path
            foreach (KeyValuePair<string, string> keyValue in fileText)
            {
                // get the text
                string copy = keyValue.Value;

                // add order id and its file path to dictionary
                while (copy.Contains("<order_id>"))
                {
                    copy = substringMethod(copy, "<order_id>", 10);
                    list.Add(getTarget(copy), keyValue.Key);
                }
            }

            return list;
        }

        /* method that return all the text of order xml feed with dictionary <filePath, text> */
        private Dictionary<string, string> getOrderFileText()
        {
            // get all the order file on local
            DirectoryInfo dirInfo = new DirectoryInfo(newOrderDir);
            FileInfo[] filesLocal = dirInfo.GetFiles("*.xml");       // getting all file that have been on local

            // read all the text of the file in the local directory and add the path for the file
            Dictionary<string, string> textXml = new Dictionary<string, string>();
            foreach (FileInfo file in filesLocal)
                textXml.Add(newOrderDir + "\\" + file, File.ReadAllText(newOrderDir + "\\" + file));

            return textXml;
        }
        #endregion

        #region Check Order Methods
        /* return all the new order file name on the server */
        private string[] checkOrderFile()
        {
            // get all order file on local
            DirectoryInfo dirInfo = new DirectoryInfo(newOrderDir);
            FileInfo[] filesLocal = dirInfo.GetFiles("*.xml");       // getting all file that have been on local
            List<string> neworderList = new List<string>();

            // get all order file on server
            string[] fileOnServer = getOrderName(SHIPMENT_DIR);

            // check the number of new order on the server compare to ones on the computer
            foreach (string file1 in fileOnServer)
            {
                // local bool flag to indicate if new order has beed found or not
                bool found = false;

                // starting checking if there is duplicate
                foreach (FileInfo file2 in filesLocal)
                {
                    if (file1 == file2.ToString())
                    {
                        found = true;
                        break;
                    }
                }

                // if there is no duplicate write the new file and increment the number
                if (!found)
                    neworderList.Add(file1);
            }

            return neworderList.ToArray();
        }

        /* a method that receive all the current order and check the duplicate then only return the ones that have not been processed 
        -> receive and return dictionary <orderId, filePath> */
        public Dictionary<string, string> checkOrder(Dictionary<string, string> allOrderList)
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
            Dictionary<string, string> newOrderList = new Dictionary<string, string>();
            foreach (KeyValuePair<string,string> keyValue in allOrderList)
            {
                // local bool flag to indicate if new order has beed found or not
                bool found = false;

                // starting checking if there is duplicate
                foreach (string completeOrder in completeOrderList)
                {
                    if (keyValue.Key == completeOrder)
                    {
                        found = true;
                        break;
                    }
                }

                // if there is no duplicate add the new file
                if (!found)
                    newOrderList.Add(keyValue.Key, keyValue.Value);
            }

            return newOrderList;
        }
        #endregion

        #region XML Generation
        /* a method that generate ShopCaValues object for the given order number (first version -> take from local) */
        public ShopCaValues GenerateValue(string targetOrder, string filePath)
        {
            //Load xml file content
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            int order_position = 1;
            int order_item_position = 1;

            // field for return
            ShopCaValues value = new ShopCaValues();

            #region Retrieve Data
            foreach (XmlNode parentnode in doc.DocumentElement.SelectSingleNode("/shop_ca_feed"))
            {
                if (parentnode.Name == "order")
                {
                    foreach (XmlNode order in doc.DocumentElement.SelectSingleNode("/shop_ca_feed/order[" + order_position + "]"))
                    {
                        // bool flag for determine if the order is the target -> default set to true
                        bool isTarget = true;

                        #region Order
                        switch (order.Name)
                        {
                            case "order_id":
                                // check if this order is the order that needs to be processed, if not set boolean flag to false
                                if (order.InnerText != targetOrder)
                                {
                                    isTarget = false;
                                    break;
                                }
                                value.OrderId = order.InnerText;
                                break;
                            case "supplier_id":
                                value.SupplierId = order.InnerText;
                                break;
                            case "store_name":
                                value.StoreName = order.InnerText;
                                break;
                            case "order_create_date":
                                value.OrderCreateDate = DateTime.ParseExact(order.InnerText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                break;
                            case "grand_total":
                                value.GrandTotal = decimal.Parse(order.InnerText);
                                break;
                            case "total_price":
                                value.TotalPrice = decimal.Parse(order.InnerText);
                                break;
                            case "total_tax":
                                value.TotalTax = decimal.Parse(order.InnerText);
                                break;
                            case "total_shipping_cost":
                                value.TotalShippingCost = decimal.Parse(order.InnerText);
                                break;
                            case "total_discount":
                                value.TotalDiscount = decimal.Parse(order.InnerText);
                                break;
                            case "billing_first_name":
                                value.BillTo.Name = order.InnerText;
                                break;
                            case "billing_last_name":
                                value.BillTo.Name += " " + order.InnerText;
                                break;
                            case "billing_phone_number":
                                value.BillTo.DayPhone = order.InnerText;
                                break;
                            case "billing_address_one":
                                value.BillTo.Address1 = order.InnerText;
                                break;
                            case "billing_address_two":
                                value.BillTo.Address2 = order.InnerText;
                                break;
                            case "billing_city":
                                value.BillTo.City = order.InnerText;
                                break;
                            case "billing_province":
                                value.BillTo.State = order.InnerText;
                                break;
                            case "billing_postalcode":
                                value.BillTo.PostalCode = order.InnerText;
                                break;
                            case "opt_in":
                                value.Option = bool.Parse(order.InnerText);
                                break;
                            #endregion

                            #region Order Item
                            case "order_item":
                                foreach (XmlNode subnode in doc.DocumentElement.SelectSingleNode("/shop_ca_feed/order[" + order_position + "]/order_item[" + order_item_position + "]"))
                                {
                                    switch (subnode.Name)
                                    {
                                        case "order_item_id":
                                            value.OrderItemId.Add(subnode.InnerText);
                                            break;
                                        case "sku":
                                            value.Sku.Add(subnode.InnerText);
                                            break;
                                        case "title":
                                            value.Title.Add(subnode.InnerText);
                                            break;
                                        case "ssrp":
                                            value.Ssrp.Add(decimal.Parse(subnode.InnerText));
                                            break;
                                        case "ssrp_tax":
                                            value.SsrpTax.Add(decimal.Parse(subnode.InnerText));
                                            break;
                                        case "quantity":
                                            decimal dec = decimal.Parse(subnode.InnerText);
                                            string tmp = dec.ToString("0.#");
                                            value.Quantity.Add(int.Parse(tmp));
                                            break;
                                        case "item_price":
                                            value.ItemPrice.Add(decimal.Parse(subnode.InnerText));
                                            break;
                                        case "extended_item_price":
                                            value.ExtendedItemPrice.Add(decimal.Parse(subnode.InnerText));
                                            break;
                                        case "item_tax":
                                            value.ItemTax.Add(decimal.Parse(subnode.InnerText));
                                            break;
                                        case "item_shipping_cost":
                                            value.ItemShippingCost.Add(decimal.Parse(subnode.InnerText));
                                            break;
                                        case "item_discount":
                                            value.ItemDiscount.Add(decimal.Parse(subnode.InnerText));
                                            break;
                                        case "shipping_first_name":
                                            if (order_item_position == 1)
                                                value.ShipTo.Name = subnode.InnerText;
                                            break;
                                        case "shipping_last_name":
                                            if (order_item_position == 1)
                                                value.ShipTo.Name += " " + subnode.InnerText;
                                            break;
                                        case "shipping_address_one":
                                            if (order_item_position == 1)
                                                value.ShipTo.Address1 = subnode.InnerText;
                                            break;
                                        case "shipping_address_two":
                                            if (order_item_position == 1)
                                                value.ShipTo.Address2 = subnode.InnerText;
                                            break;
                                        case "shipping_city":
                                            if (order_item_position == 1)
                                                value.ShipTo.City = subnode.InnerText;
                                            break;
                                        case "shipping_province":
                                            if (order_item_position == 1)
                                                value.ShipTo.State = subnode.InnerText;
                                            break;
                                        case "shipping_postalcode":
                                            if (order_item_position == 1)
                                                value.ShipTo.PostalCode = subnode.InnerText;
                                            break;
                                        case "shipping_phone":
                                            if (order_item_position == 1)
                                                value.ShipTo.DayPhone = subnode.InnerText;
                                            break;
                                        case "shipping_method":
                                            if (order_item_position == 1)
                                                value.ShippingMethod = subnode.InnerText;
                                            break;
                                    }
                                }
                                //increment position (for multiple order item)
                                order_item_position++;
                                break;
                        }
                        #endregion

                        // the case if it is not the target order -> skip to next order node
                        if (!isTarget)
                            break;
                    }
                }

                //adjust position of order item
                order_position++;
                order_item_position = 1;
            }
            #endregion

            return value;
        }
        /* second version -> take from database */
        public ShopCaValues GenerateValue(string targetOrder)
        {
            // local field for storing values
            ShopCaValues value = new ShopCaValues();

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT OrderId, SupplierId, StoreName, OrderCreateDate, GrandTotal, TotalPrice, TotalTax, TotalShippingCost, TotalDiscount, BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToPostalCode, BillToPhone, " 
                                                  + "ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToPostalCode, ShipToPhone, OptionIn, ShippingMethod, TrackingNumber " 
                                                  + "FROM ShopCa_Order WHERE OrderId = \'" + targetOrder + "\'", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                value.OrderId = reader.GetString(0);
                value.SupplierId = reader.GetString(1);
                value.StoreName = reader.GetString(2);
                value.OrderCreateDate = reader.GetDateTime(3);
                value.GrandTotal = reader.GetDecimal(4);
                value.TotalPrice = reader.GetDecimal(5);
                value.TotalTax = reader.GetDecimal(6);
                value.TotalShippingCost = reader.GetDecimal(7);
                value.TotalDiscount = reader.GetDecimal(8);
                value.BillTo.Name = reader.GetString(9);
                value.BillTo.Address1 = reader.GetString(10);
                value.BillTo.Address2 = reader.GetString(11);
                value.BillTo.City = reader.GetString(12);
                value.BillTo.State = reader.GetString(13);
                value.BillTo.PostalCode = reader.GetString(14);
                value.BillTo.DayPhone = reader.GetString(15);
                value.ShipTo.Name = reader.GetString(16);
                value.ShipTo.Address1 = reader.GetString(17);
                value.ShipTo.Address2 = reader.GetString(18);
                value.ShipTo.City = reader.GetString(19);
                value.ShipTo.State = reader.GetString(20);
                value.ShipTo.PostalCode = reader.GetString(21);
                value.ShipTo.DayPhone = reader.GetString(22);
                value.Option = reader.GetBoolean(23);
                value.ShippingMethod = reader.GetString(24);
                value.Package.TrackingNumber = reader.GetString(25);

                SqlDataAdapter adatper = new SqlDataAdapter("SELECT OrderItemId, Sku, Title, Ssrp, SsrpTax, Quantity, ItemPrice, ExtendedItemPrice, ItemTax, ItemShippingCost, ItemDiscount " +
                                                            "FROM ShopCa_Order_Item WHERE OrderId = \'" + targetOrder + "\' ORDER BY OrderItemId;", connection);
                DataTable table = new DataTable();
                adatper.Fill(table);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    value.OrderItemId.Add(table.Rows[i][0].ToString());
                    value.Sku.Add(table.Rows[i][1].ToString());
                    value.Title.Add(table.Rows[i][2].ToString());
                    value.Ssrp.Add(Convert.ToDecimal(table.Rows[i][3]));
                    value.SsrpTax.Add(Convert.ToDecimal(table.Rows[i][4]));
                    value.Quantity.Add(Convert.ToInt32(table.Rows[i][5]));
                    value.ItemPrice.Add(Convert.ToDecimal(table.Rows[i][6]));
                    value.ExtendedItemPrice.Add(Convert.ToDecimal(table.Rows[i][7]));
                    value.ItemTax.Add(Convert.ToDecimal(table.Rows[i][8]));
                    value.ItemShippingCost.Add(Convert.ToDecimal(table.Rows[i][9]));
                    value.ItemDiscount.Add(Convert.ToDecimal(table.Rows[i][10]));
                }
            }

            return value;
        }
        #endregion

        /* a method that add a new order to database */
        private static void addNewOrder(ShopCaValues value)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                // add new order to database
                SqlCommand command = new SqlCommand("INSERT INTO ShopCa_Order " +
                                                    "(OrderId, SupplierId, StoreName, OrderCreateDate, GrandTotal, TotalPrice, TotalTax, TotalShippingCost, TotalDiscount, BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToPostalCode, BillToPhone, ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToPostalCode, ShipToPhone, OptionIn, ShippingMethod, Complete, TrackingNumber) Values " +
                                                    "(\'" + value.OrderId + "\',\'" + value.SupplierId + "\',\'" + value.StoreName + "\',\'" + value.OrderCreateDate.ToString("yyyy-MM-dd") + "\'," + value.GrandTotal + "," + value.TotalPrice + "," + value.TotalTax + "," + value.TotalShippingCost  + "," + value.TotalDiscount + ",\'" + value.BillTo.Name.Replace("'", "''") + "\',\'" + value.BillTo.Address1.Replace("'", "''") + "\',\'" + value.BillTo.Address2.Replace("'", "''") + "\',\'" +
                                                    value.BillTo.City.Replace("'", "''") + "\',\'" + value.BillTo.State + "\',\'" + value.BillTo.PostalCode + "\',\'" + value.BillTo.DayPhone + "\',\'" + value.ShipTo.Name.Replace("'", "''") + "\',\'" + value.ShipTo.Address1.Replace("'", "''") + "\',\'" + value.ShipTo.Address2.Replace("'", "''") + "\',\'" + value.ShipTo.City.Replace("'", "''") + "\',\'" + value.ShipTo.State + "\',\'" + value.ShipTo.PostalCode + "\',\'" + value.ShipTo.DayPhone + "\',\'" +
                                                    value.Option + "\',\'" + value.ShippingMethod + "\',\'False\',\'" + value.Package.TrackingNumber + "\')", connection);
                connection.Open();
                command.ExecuteNonQuery();

                // add each item for the order to database
                for (int i = 0; i < value.OrderItemId.Count; i++)
                {
                    command = new SqlCommand("INSERT INTO ShopCa_Order_Item " +
                                             "(OrderId, OrderItemId, Sku, Title, Ssrp, SsrpTax, Quantity, ItemPrice, ExtendedItemPrice, ItemTax, ItemShippingCost, ItemDiscount, Shipped) Values" +
                                             "(\'" + value.OrderId + "\',\'" + value.OrderItemId[i] + "\',\'" + value.Sku[i] + "\',\'" + value.Title[i].Replace("'", "''") + "\'," + value.Ssrp[i] + "," + value.SsrpTax[i] + "," + value.Quantity[i] + "," +
                                             value.ItemPrice[i] + "," + value.ExtendedItemPrice[i] + "," + value.ItemTax[i] + "," + value.ItemShippingCost[i] + "," + value.ItemDiscount[i] + ",\'False\')", connection);
                    command.ExecuteNonQuery();
                }
            }
        }

        #region Supporting Methods
        /* a method that substring the given string */
        private static string substringMethod(string original, string startingString, int additionIndex)
        {
            return original.Substring(original.IndexOf(startingString) + additionIndex);
        }

        /* a method that get the next target token */
        private static string getTarget(string text)
        {
            int i = 0;
            while (text[i] != '<' && text[i] != '>' && text[i] != '"')
                i++;

            return text.Substring(0, i);
        }
        #endregion
    }
}