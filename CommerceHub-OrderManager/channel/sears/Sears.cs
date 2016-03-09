using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Tamir.SharpSsh;

namespace CommerceHub_OrderManager.channel.sears
{
    /*
     * A class that connect to Sears sftp server and manage all the orders for sears
     */
    public class Sears
    {
        // fields for directory on sftp server
        public const string SHIPMENT_DIR = "outgoing/orders/searscanada";
        public const string CONFIRM_DIR = "incoming/confirms/searscanada";

        // field for directory on local
        private readonly string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SearsOrders";
        private readonly string newOrderDir;
        private readonly string completeOrderDir;

        // field for sftp connection
        private Sftp sftp = new Sftp("ashlinbpg.sftp-test.commercehub.com", "ashlinbpg", "Pay4Examine9Rather$");

        /* constructor that restore the data and initialize folders for xml feed */
        public Sears()
        {
            #region Folder Check
            // check and generate folders
            if (!Directory.Exists(rootDir))
                Directory.CreateDirectory(rootDir);

            newOrderDir = rootDir + "\\SearsNewOrders";
            if (!Directory.Exists(newOrderDir))
                Directory.CreateDirectory(newOrderDir);

            completeOrderDir = rootDir + "\\SearsCompleteOrders";
            if (!Directory.Exists(completeOrderDir))
                Directory.CreateDirectory(completeOrderDir);
            #endregion
        }

        /* a method that get all new order on the server and update to the database */
        public void GetOrder()
        {
            // get all the new file on the order directory to local new order storing directory
            string[] orderCheck = checkOrderFile();
            getOrder(newOrderDir, orderCheck);

            // read all the text of the file in the local directory
            string[] textXML = getOrderFileText();

            // return the transaction that haved not been processed
            orderCheck = getTransactionId(textXML);
            orderCheck = checkTransaction(orderCheck);

            // get information for each unprocessed transaction and update the them to the database
            foreach (string transaction in orderCheck)
            {
                SearsValues value = generateValue(transaction, textXML);
                addNewOrder(value);
            }
        }

        /* a method that return all the new order values */
        public SearsValues[] GetAllNewOrder()
        {
            // local field for storing order values 
            List<SearsValues> list = new List<SearsValues>();
            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT TransactionId, LineCount, PoNumber, TrxBalanceDue, ServiceLevel, OrderDate, paymentMethod, CustOrderNumber, CustOrderDate, PackSlipmessage, BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToPostalCode, BillToPhone, " +
                                                    "RecipientName, RecipientAddress1, RecipientAddress2, RecipientCity, RecipientState, RecipientPostalCode, RecipientPhone, ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToPostalCode, ShipToPhone, FreightLane, Spur " +
                                                    "FROM Sears_Order WHERE Complete =\'False\' ORDER BY TransactionId;", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SearsValues value = new SearsValues();

                    string transactionId = reader.GetString(0);
                    value.TransactionID = transactionId;
                    value.LineCount = reader.GetInt32(1);
                    value.PoNumber = reader.GetString(2);
                    value.TrxBalanceDue = Convert.ToDouble(reader.GetValue(3));
                    value.ServiceLevel = reader.GetString(4);
                    value.OrderDate = reader.GetDateTime(5);
                    value.PaymentMethod = reader.GetString(6);
                    value.CustOrderNumber = reader.GetString(7);
                    value.CustOrderDate = reader.GetDateTime(8);
                    value.PackSlipMessage = reader.GetString(9);
                    value.BillTo.Name = reader.GetString(10);
                    value.BillTo.Address1 = reader.GetString(11);
                    value.BillTo.Address2 = reader.GetString(12);
                    value.BillTo.City = reader.GetString(13);
                    value.BillTo.State = reader.GetString(14);
                    value.BillTo.PostalCode = reader.GetString(15);
                    value.BillTo.DayPhone = reader.GetString(16);
                    value.Recipient.Name = reader.GetString(17);
                    value.Recipient.Address1 = reader.GetString(18);
                    value.Recipient.Address2 = reader.GetString(19);
                    value.Recipient.City = reader.GetString(20);
                    value.Recipient.State = reader.GetString(21);
                    value.Recipient.PostalCode = reader.GetString(22);
                    value.Recipient.DayPhone = reader.GetString(23);
                    value.ShipTo.Name = reader.GetString(24);
                    value.ShipTo.Address1 = reader.GetString(25);
                    value.ShipTo.Address2 = reader.GetString(26);
                    value.ShipTo.City = reader.GetString(27);
                    value.ShipTo.State = reader.GetString(28);
                    value.ShipTo.PostalCode = reader.GetString(29);
                    value.ShipTo.DayPhone = reader.GetString(30);
                    value.FreightLane = reader.GetString(31);
                    value.Spur = reader.GetString(32);

                    SqlDataAdapter adatper = new SqlDataAdapter("SELECT LineBalanceDue, MerchantLineNumber, TrxVendorSKU, TrxMerchantSKU, UPC, TrxQty, TrxUnitCost, Description1, Description2, UnitPrice, LineHandling, " +
                                                                "ExpectedShipDate, GST_HST_Extended, PST_Extended, GST_HST_Total, PST_Total, EncodedPrice, ReceivingInstructions " +
                                                                "FROM Sears_Order_Item WHERE TransactionId = \'" + transactionId + "\' ORDER BY MerchantLineNumber;", connection);
                    
                    adatper.Fill(table);

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        value.LineBalanceDue.Add(Convert.ToDouble(table.Rows[i][0]));
                        value.MerchantLineNumber.Add(Convert.ToInt32(table.Rows[i][1]));
                        value.TrxVendorSKU.Add(table.Rows[i][2].ToString());
                        value.TrxMerchantSKU.Add(table.Rows[i][3].ToString());
                        value.UPC.Add(table.Rows[i][4].ToString());
                        value.TrxQty.Add(Convert.ToInt32(table.Rows[i][5]));
                        value.TrxUnitCost.Add(Convert.ToDouble(table.Rows[i][6]));
                        value.Description.Add(table.Rows[i][7].ToString());
                        value.Description2.Add(table.Rows[i][8].ToString());
                        value.UnitPrice.Add(Convert.ToDouble(table.Rows[i][9]));
                        value.LineHandling.Add(Convert.ToDouble(table.Rows[i][10]));
                        value.ExpectedShipDate.Add(Convert.ToDateTime(table.Rows[i][11]));
                        value.GST_HST_Extended.Add(Convert.ToDouble(table.Rows[i][12]));
                        value.PST_Extended.Add(Convert.ToDouble(table.Rows[i][13]));
                        value.GST_HST_Total.Add(Convert.ToDouble(table.Rows[i][14]));
                        value.PST_Total.Add(Convert.ToDouble(table.Rows[i][15]));
                        value.EncodedPrice.Add(table.Rows[i][16].ToString());
                        value.ReceivingInstructions.Add(table.Rows[i][17].ToString());
                    }

                    table.Reset();

                    list.Add(value);
                }
            }

            return list.ToArray();
        }

        /* a method that return all shipped order */
        public SearsValues[] GetAllShippedOrder()
        {
            // local field for storing shipment value 
            List<SearsValues> list = new List<SearsValues>();

            // grab all shipped 
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT TransactionId, TrackingNumber, ShipmentIdentificationNumber FROM Sears_Order " +
                                                    "WHERE TrackingNumber != '' AND CompleteDate = \'" + DateTime.Today.ToString("yyyy-MM-dd") + "\';", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    SearsValues value = new SearsValues();
                    value.TransactionID = reader.GetString(0);
                    value.Package.TrackingNumber = reader.GetString(1);
                    value.Package.IdentificationNumber = reader.GetString(2);

                    list.Add(value);
                }
            }

            return list.ToArray();
        }

        /* a method that mark the order as cancelled but not really posting a cancel order to sears only for local reference */
        public void PostCancel(string[] transactionId)
        {
            // generate the range 
            string candidate = "(";
            foreach (string id in transactionId)
                candidate += '\'' + id + "\',";
            candidate = candidate.Remove(candidate.Length - 1) + ')';

            // update to not shipped 
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                // for entire order cancellation
                SqlCommand command = new SqlCommand("UPDATE Sears_Order SET TrackingNumber = '' WHERE TransactionId IN " + candidate, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        #region Number of Orders and Shipments
        /* methods that return the number of order and shipment from the given date */
        public int GetNumberOfOrder(DateTime time)
        {
            int count = 0;

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Sears_Order WHERE CustOrderDate = \'" + time.ToString("yyyy-MM-dd") + "\';", connection);
                connection.Open();

                count = (int) command.ExecuteScalar();
            }


            return count;
        }
        public int GetNumberOfShipped(DateTime time)
        {
            int count = 0;

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Sears_Order WHERE Complete = 'True' AND CustOrderDate = \'" + time.ToString("yyyy-MM-dd") + "\';", connection);
                connection.Open();

                count = (int)command.ExecuteScalar();
            }

            return count;
        }
        #endregion

        #region Get Order Information
        /* method that get the new order from sftp server */
        private void getOrder(string filePath, string[] fileList)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();

            foreach (string file in fileList)
            {
                if (file == "." || file == "..") continue;
                // change file to txt 
                string fileNameTxt = file.Replace("neworders", "txt");
                sftp.Get(SHIPMENT_DIR + "/" + file, filePath + "//" + fileNameTxt);
            }

            sftp.Close();
        }

        /* method that get the new order from sftp server */
        private string[] getOrderName(string serverDir)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();
            ArrayList list = sftp.GetFileList(serverDir);

            // field for storing each batch number for the file on sftp server
            List<string> bactch = new List<string>();

            // retrieve all the files name to the list
            foreach (var item in list)
            {
                string itemName = item.ToString();

                if (itemName != "." && itemName != "..")
                    bactch.Add(itemName);
            }

            sftp.Close();

            return bactch.ToArray();
        }

        /* method that get all the transaction in the file */
        private string[] getTransactionId(string[] fileText)
        {
            // local field for storing data
            List<string> list = new List<string>();

            // get all transaction id
            foreach (string text in fileText)
            {
                string copy = text;

                while (copy.Contains("hubOrder transactionID"))
                {
                    copy = substringMethod(copy, "transactionID", 15);
                    list.Add(getTarget(copy));
                }
            }

            return list.ToArray();
        }

        /* method that return all the text of order xml feed */
        private string[] getOrderFileText()
        {
            // get all the order file on local
            DirectoryInfo dirInfo = new DirectoryInfo(newOrderDir);
            FileInfo[] filesLocal = dirInfo.GetFiles("*.txt");       // getting all file that have been on local

            // read all the text of the file in the local directory
            int fileCount = filesLocal.Length;
            string[] textXML = new string[fileCount];      // xml text for each file
            for (int i = 0; i < fileCount; i++)
                textXML[i] = File.ReadAllText(newOrderDir + filesLocal[i]);

            return textXML;
        }
        #endregion

        #region Checking Order Methods
        /* give the information about the new orders and also return all the new order files on server */
        private string[] checkOrderFile()
        {
            // get all order file on local
            DirectoryInfo dirInfo = new DirectoryInfo(newOrderDir);
            FileInfo[] filesLocal = dirInfo.GetFiles("*.txt");       // getting all file that have been on local
            List<string> neworderList = new List<string>();

            // get all order file on server
            string[] fileOnServer = getOrderName(SHIPMENT_DIR);

            // check the number of new order on the server compare to ones on the computer
            foreach (string file1 in fileOnServer)
            {
                // checking if there is duplicate
                bool found = filesLocal.Select(file2 => file2.ToString()).Any(file2Copy => file1.Remove(file1.LastIndexOf('.')) == file2Copy.Remove(file2Copy.LastIndexOf('.')));

                // if there is no duplicate write the new file and increment the number
                if (!found)
                    neworderList.Add(file1);
            }

            return neworderList.ToArray();
        }

        /* a method that receive all the current transaction and check the duplicate then only return the ones that have not been added to the database */
        private string[] checkTransaction(string[] allTransactionList)
        {
            // get all complete transaction 
            List<string> completeTransactionList = new List<string>();
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT TransactionId FROM Sears_Order ORDER BY TransactionId;", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    completeTransactionList.Add(reader.GetString(0));
            }

            // local field for storing incomplete transaction
            List<string> newtransactionList = new List<string>();

            // check the number of new order on the server compare to ones on the computer
            foreach (string transaction in allTransactionList)
            {
                // checking if ther is duplication
                bool found = completeTransactionList.Any(complete => complete.Contains(transaction));

                // if there is no duplicate write the new file and increment the number
                if (!found)
                    newtransactionList.Add(transaction);
            }

            return newtransactionList.ToArray();
        }
        #endregion

        #region XML Generation
        /* a method that generate xml order and upload to the sftp server and update database */
        public void generateXML(SearsValues value, Dictionary<int, string> cancelList)
        {
            // get other necessary values
            value.VendorInvoiceNumber = getInvoiceNumber();
            value.PackageDetailID = getPackageId();

            // fields for database update
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs);
            SqlCommand command;
            connection.Open();

            #region XML Generation and Item Database Update
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                         "<ConfirmMessageBatch>" +
                         "<partnerID roleType=\"vendor\" name=\"Ashlin BPG Marketing, Inc.\">ashlinbpg</partnerID>" +
                         "<hubConfirm transactionID=\"" + value.TransactionID + "\">" +
                         "<participatingParty roleType=\"merchant\" name=\"Sears Canada\" participationCode=\"To:\">searscanada</participatingParty>" +
                         "<partnerTrxID>" + value.PartnerTrxID + "</partnerTrxID>" +
                         "<partnerTrxDate>" + DateTime.Today.ToString("yyyyMMdd") + "</partnerTrxDate>" +
                         "<poNumber>" + value.PoNumber + "</poNumber>";

            // the case if there is at least one order will be shipped -> add trx balance due
            if (cancelList.Count < value.LineCount)
            {
                // substract the amount that the orders are cancelled
                for (int i = 0; i < value.LineCount; i++)
                {
                    foreach (int j in cancelList.Keys)
                    {
                        if (j == i)
                        {
                            value.TrxBalanceDue -= value.LineBalanceDue[i];
                            break;
                        }
                    }
                }

                xml += "<trxBalanceDue>" + value.TrxBalanceDue + "</trxBalanceDue>" +
                       "<trxData>" +
                       "<vendorsInvoiceNumber>" + value.VendorInvoiceNumber + "</vendorsInvoiceNumber>" +
                       "</trxData>";
            }

            for (int i = 1; i <= value.LineCount; i++)
            {
                bool isCancelled = false;

                // check if the order is cancel
                foreach (int j in cancelList.Keys)
                {
                    if (j == i - 1)
                    {
                        string reason;

                        switch (cancelList[j])
                        {
                            case "Incorrect Ship To Address":
                                reason = "bad_address";
                                break;
                            case "Incorrect SKU":
                                reason = "bad_sku​";
                                break;
                            case "Cancelled at Merchant's Request":
                                reason = "merchant_request";
                                break;
                            case "Cannot fulfill the order in time":
                                reason = "fulfill_time_expired";
                                break;
                            case "Cannot Ship as Ordered":
                                reason = "cannot_meet_all_reqs";
                                break;
                            case "Invalid Item Cost":
                                reason = "invalid_item_cost";
                                break;
                            case "Merchant detected fraud":
                                reason = "merchant_detected_fraud";
                                break;
                            case "Order missing information":
                                reason = "info_missing";
                                break;
                            case "Out of Stock":
                                reason = "out_of_stock";
                                break;
                            case "Product Has Been Discontinued":
                                reason = "discontinued";
                                break;
                            default:
                                reason = "other";
                                break;
                        }

                        xml +=
                            "<hubAction>" +
                            "<action>v_cancel</action>" +
                            "<actionCode>" + reason + "</actionCode>";

                        // update item to cancelled to database
                        command = new SqlCommand("UPDATE Sears_Order_Item SET Cancelled = 'True' WHERE TransactionId = \'" + value.TransactionID +
                                                 "\' AND MerchantLineNumber = \'" + value.MerchantLineNumber[j] + "\';", connection);
                        command.ExecuteNonQuery();

                        isCancelled = true;
                        break;
                    }
                }

                if (!isCancelled)
                {
                    xml +=
                    "<hubAction>" +
                    "<action>v_ship</action>";

                    command = new SqlCommand("UPDATE Sears_Order_Item SET Shipped = 'True' WHERE TransactionId = \'" + value.TransactionID +
                                             "\' AND MerchantLineNumber = \'" + value.MerchantLineNumber[i - 1] + "\';", connection);
                    command.ExecuteNonQuery();
                }
                xml +=
                "<merchantLineNumber>" + value.MerchantLineNumber[i - 1] + "</merchantLineNumber>" +
                "<trxVendorSKU>" + value.TrxVendorSKU[i - 1] + "</trxVendorSKU>" +
                "<trxMerchantSKU>" + value.TrxMerchantSKU[i - 1] + "</trxMerchantSKU>" +
                "<UPC>" + value.UPC[i - 1] + "</UPC>" +
                "<trxQty>" + value.TrxQty[i - 1] + "</trxQty>" +
                "<trxUnitCost>" + value.TrxUnitCost[i - 1] + "</trxUnitCost>";
                if (!isCancelled)
                    xml += "<packageDetailLink packageDetailID=\"" + value.PackageDetailID + "\"/>";
                xml +=
                "</hubAction>";
            }

            // the case if there is at least one order will be shipped
            if (cancelList.Count < value.LineCount)
            {
                xml +=
                    "<packageDetail packageDetailID=\"" + value.PackageDetailID + "\">" +
                    "<shipDate>" + DateTime.Today.ToString("yyyyMMdd") /*value.ExpectedShipDate[0].ToString("yyyyMMdd")*/ + "</shipDate>" +
                    "<serviceLevel1>" + value.ServiceLevel + "</serviceLevel1>" +
                    "<trackingNumber>" + value.Package.TrackingNumber + "</trackingNumber>" +
                    "</packageDetail>";
            }
            xml +=
                "</hubConfirm>" +
                "<messageCount>1</messageCount>" +
                "</ConfirmMessageBatch>";

            // convert txt to xsd file
            string path = completeOrderDir + "//" + value.TransactionID + ".xsd";
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(xml);
            writer.Close();
            #endregion

            // master database update
            command = new SqlCommand("UPDATE Sears_Order SET VendorInvoiceNumber = \'" + value.VendorInvoiceNumber + "\', PakageDetailId = \'" + value.PackageDetailID + "\', TrackingNumber = \'" + value.Package.TrackingNumber + "\', CompleteDate = \'" +
                                     DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss") + "\', ShipmentIdentificationNumber = \'" + value.Package.IdentificationNumber + "\', Complete = 'True' WHERE TransactionId = \'" + value.TransactionID + "\';", connection);
            command.ExecuteNonQuery();
            connection.Close();

            // upload file to sftp server
            // sftp.Connect();
            // sftp.Put(@"C:\Users\Intern1001\Desktop\F15_Intern\Orders\SearsCompleteOrders\" + value.TransactionID + ".xsd", confirmDir);
            // sftp.Close();
        }

        /* a method that generate SearsValues object for the given transaction number (first version -> take from local) */
        public SearsValues generateValue(string targetTransaction, string[] textXml)
        {
            // local field for storing values
            SearsValues value = new SearsValues();

            foreach (string text in textXml)
            {
                // field target text file that has the transaction
                if (text.Contains(targetTransaction))
                {
                    string copy = text;

                    // transaction id
                    copy = substringMethod(copy, targetTransaction, 0);
                    copy = copy.Remove(copy.IndexOf("/hubOrder"));
                    value.TransactionID = getTarget(copy);

                    // line count
                    copy = substringMethod(copy, "lineCount", 10);
                    value.LineCount = Convert.ToInt32(getTarget(copy));

                    // po number
                    copy = substringMethod(copy, "poNumber", 9);
                    value.PoNumber = getTarget(copy);

                    // order date
                    copy = substringMethod(copy, "orderDate", 10);
                    value.OrderDate = DateTime.ParseExact(getTarget(copy), "yyyyMMdd", CultureInfo.InvariantCulture);

                    // payment method
                    copy = substringMethod(copy, "paymentMethod", 14);
                    value.PaymentMethod = getTarget(copy);

                    // ship to person place id
                    copy = substringMethod(copy, "shipTo personPlaceID", 22);
                    string shipToId = getTarget(copy);

                    // bill to person place id 
                    copy = substringMethod(copy, "billTo personPlaceID", 22);
                    string billToId = getTarget(copy);

                    // customer person place id 
                    copy = substringMethod(copy, "customer personPlaceID", 26);
                    string customerPlaceId = getTarget(copy);

                    // cust order number
                    copy = substringMethod(copy, "custOrderNumber", 16);
                    value.CustOrderNumber = getTarget(copy);

                    // cust order date
                    copy = substringMethod(copy, "custOrderDate", 14);
                    value.CustOrderDate = DateTime.ParseExact(getTarget(copy), "yyyyMMdd", CultureInfo.InvariantCulture);

                    // pack slip message
                    copy = substringMethod(copy, "packslipMessage", 16);
                    value.PackSlipMessage = getTarget(copy);

                    // get info for each line count
                    for (int i = 1; i <= value.LineCount; i++)
                    {
                        // order line number
                        copy = substringMethod(copy, "merchantLineNumber", 19);
                        value.MerchantLineNumber.Add(Convert.ToInt32(getTarget(copy)));

                        // trx qty
                        copy = substringMethod(copy, "qtyOrdered", 11);
                        value.TrxQty.Add(Convert.ToInt32(getTarget(copy)));

                        // upc
                        copy = substringMethod(copy, "UPC", 4);
                        value.UPC.Add(getTarget(copy));

                        // description 
                        copy = substringMethod(copy, "description", 12);
                        value.Description.Add(getTarget(copy));

                        // description 2
                        copy = substringMethod(copy, "description2", 13);
                        value.Description2.Add(getTarget(copy));

                        // merchant sku
                        copy = substringMethod(copy, "merchantSKU", 12);
                        value.TrxMerchantSKU.Add(getTarget(copy));

                        // vendor sku
                        copy = substringMethod(copy, "vendorSKU", 10);
                        value.TrxVendorSKU.Add(getTarget(copy));

                        // unit price
                        copy = substringMethod(copy, "unitPrice", 10);
                        value.UnitPrice.Add(Convert.ToDouble(getTarget(copy)));

                        // unit price
                        copy = substringMethod(copy, "unitCost", 9);
                        value.TrxUnitCost.Add(Convert.ToDouble(getTarget(copy)));

                        // line handling
                        if (copy.Contains("lineHandling"))
                        {
                            copy = substringMethod(copy, "lineHandling", 13);
                            value.LineHandling.Add(Convert.ToDouble(getTarget(copy)));
                        }

                        if (i == 1)
                        {
                            // service level / shipping code
                            copy = substringMethod(copy, "shippingCode", 13);
                            value.ServiceLevel = getTarget(copy);
                        }

                        // trx balance due (plus line balance due)
                        copy = substringMethod(copy, "lineBalanceDue", 15);
                        value.LineBalanceDue.Add(Convert.ToDouble(getTarget(copy)));
                        value.TrxBalanceDue += Convert.ToDouble(getTarget(copy));

                        // expected ship date
                        copy = substringMethod(copy, "expectedShipDate", 17);
                        value.ExpectedShipDate.Add(DateTime.ParseExact(getTarget(copy), "yyyyMMdd", CultureInfo.InvariantCulture));

                        // gst and hst extended
                        copy = substringMethod(copy, "GST_HST_Extended", 16);
                        copy = substringMethod(copy, ">", 1);
                        value.GST_HST_Extended.Add(Convert.ToDouble(getTarget(copy)));

                        // pst extended
                        copy = substringMethod(copy, "PST_Extended", 12);
                        copy = substringMethod(copy, ">", 1);
                        value.PST_Extended.Add(Convert.ToDouble(getTarget(copy)));

                        // gst and hst
                        copy = substringMethod(copy, "GST_HST_Total_H", 16);
                        copy = substringMethod(copy, ">", 1);
                        value.GST_HST_Total.Add(Convert.ToDouble(getTarget(copy)));

                        // pst
                        copy = substringMethod(copy, "PST_Total_H", 13);
                        copy = substringMethod(copy, ">", 1);
                        value.PST_Total.Add(Convert.ToDouble(getTarget(copy)));

                        // encoded price 
                        copy = substringMethod(copy, "encodedPrice", 13);
                        value.EncodedPrice.Add(getTarget(copy));

                        // ps receiving instructions
                        copy = substringMethod(copy, "psReceivingInstructions", 24);
                        value.ReceivingInstructions.Add(getTarget(copy));

                        string copyCopy = copy;

                        #region Bill To Address
                        // bill to name
                        copyCopy = substringMethod(copyCopy, billToId, billToId.Length);
                        copyCopy = substringMethod(copyCopy, "name1", 6);
                        value.BillTo.Name = getTarget(copyCopy);

                        // bill to address
                        copyCopy = copyCopy.Remove(copyCopy.IndexOf("personPlace>"));
                        copyCopy = substringMethod(copyCopy, "address1", 9);
                        value.BillTo.Address1 = getTarget(copyCopy);
                        if (copyCopy.Contains("address2"))
                        {
                            copyCopy = substringMethod(copyCopy, "address2", 9);
                            value.BillTo.Address2 = getTarget(copyCopy);
                        }

                        // bill to city
                        copyCopy = substringMethod(copyCopy, "city", 5);
                        value.BillTo.City = getTarget(copyCopy);

                        // bill to state
                        copyCopy = substringMethod(copyCopy, "state", 6);
                        value.BillTo.State = getTarget(copyCopy);

                        // bill to postal code
                        copyCopy = substringMethod(copyCopy, "postalCode", 11);
                        value.BillTo.PostalCode = getTarget(copyCopy);

                        // bill to phone
                        copyCopy = substringMethod(copyCopy, "dayPhone", 9);
                        value.BillTo.DayPhone = getTarget(copyCopy);
                        #endregion

                        copyCopy = copy;

                        #region Recipient Address
                        // recipient name
                        copyCopy = substringMethod(copyCopy, customerPlaceId, billToId.Length);
                        copyCopy = substringMethod(copyCopy, "name1", 6);
                        value.Recipient.Name = getTarget(copyCopy);

                        // recipient address
                        copyCopy = copyCopy.Remove(copyCopy.IndexOf("personPlace>"));
                        copyCopy = substringMethod(copyCopy, "address1", 9);
                        value.Recipient.Address1 = getTarget(copyCopy);
                        if (copyCopy.Contains("address2"))
                        {
                            copyCopy = substringMethod(copyCopy, "address2", 9);
                            value.Recipient.Address2 = getTarget(copyCopy);
                        }

                        // recipient city
                        copyCopy = substringMethod(copyCopy, "city", 5);
                        value.Recipient.City = getTarget(copyCopy);

                        // recipient state
                        copyCopy = substringMethod(copyCopy, "state", 6);
                        value.Recipient.State = getTarget(copyCopy);

                        // recipient postal code
                        copyCopy = substringMethod(copyCopy, "postalCode", 11);
                        value.Recipient.PostalCode = getTarget(copyCopy);

                        // recipient phone
                        copyCopy = substringMethod(copyCopy, "dayPhone", 9);
                        value.Recipient.DayPhone = getTarget(copyCopy);
                        #endregion

                        copyCopy = copy;

                        #region Ship To Address
                        // ship to name
                        copyCopy = substringMethod(copyCopy, shipToId, billToId.Length);
                        copyCopy = substringMethod(copyCopy, "name1", 6);
                        value.ShipTo.Name = getTarget(copyCopy);

                        // ship to
                        copyCopy = copyCopy.Remove(copyCopy.IndexOf("personPlace>"));
                        copyCopy = substringMethod(copyCopy, "address1", 9);
                        value.ShipTo.Address1 = getTarget(copyCopy);
                        if (copyCopy.Contains("address2"))
                        {
                            copyCopy = substringMethod(copyCopy, "address2", 9);
                            value.ShipTo.Address2 = getTarget(copyCopy);
                        }

                        // ship to
                        copyCopy = substringMethod(copyCopy, "city", 5);
                        value.ShipTo.City = getTarget(copyCopy);

                        // ship to
                        copyCopy = substringMethod(copyCopy, "state", 6);
                        value.ShipTo.State = getTarget(copyCopy);

                        // ship to
                        copyCopy = substringMethod(copyCopy, "postalCode", 11);
                        value.ShipTo.PostalCode = getTarget(copyCopy);

                        // ship to
                        if (copyCopy.Contains("dayPhone"))
                        {
                            copyCopy = substringMethod(copyCopy, "dayPhone", 9);
                            value.ShipTo.DayPhone = getTarget(copyCopy);
                        }
                        #endregion

                        // freight lane & spur -> only if exist
                        if (copyCopy.Contains("attnLine"))
                        {
                            copyCopy = substringMethod(copyCopy, "attnLine", 9);
                            string attention = getTarget(copyCopy);

                            int index = 0;
                            while ((char.IsLetter(attention[index]) || char.IsNumber(attention[index])) && attention[index] != ' ' && attention[index] != '_')
                                index++;
                            value.FreightLane = attention.Substring(0, index);
                            while (!char.IsNumber(attention[index]))
                                index++;
                            value.Spur = attention.Substring(index);
                        }
                    }

                    break;
                }
            }

            return value;
        }
        /* second version -> take from database */
        public SearsValues generateValue(string targetTransaction)
        {
            // local field for storing values
            SearsValues value = new SearsValues();

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT LineCount, PoNumber, TrxBalanceDue, ServiceLevel, OrderDate, paymentMethod, CustOrderNumber, CustOrderDate, PackSlipmessage, BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToPostalCode, BillToPhone, " +
                                                    "RecipientName, RecipientAddress1, RecipientAddress2, RecipientCity, RecipientState, RecipientPostalCode, RecipientPhone, ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToPostalCode, ShipToPhone, FreightLane, Spur " + 
                                                    "FROM Sears_Order WHERE TransactionId = \'" + targetTransaction + "\'", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                value.TransactionID = targetTransaction;
                value.LineCount = reader.GetInt32(0);
                value.PoNumber = reader.GetString(1);
                value.TrxBalanceDue = Convert.ToDouble(reader.GetValue(2));
                value.ServiceLevel = reader.GetString(3);
                value.OrderDate = reader.GetDateTime(4);
                value.PaymentMethod = reader.GetString(5);
                value.CustOrderNumber = reader.GetString(6);
                value.CustOrderDate = reader.GetDateTime(7);
                value.PackSlipMessage = reader.GetString(8);
                value.BillTo.Name = reader.GetString(9);
                value.BillTo.Address1 = reader.GetString(10);
                value.BillTo.Address2 = reader.GetString(11);
                value.BillTo.City = reader.GetString(12);
                value.BillTo.State = reader.GetString(13);
                value.BillTo.PostalCode = reader.GetString(14);
                value.BillTo.DayPhone = reader.GetString(15);
                value.Recipient.Name = reader.GetString(16);
                value.Recipient.Address1 = reader.GetString(17);
                value.Recipient.Address2 = reader.GetString(18);
                value.Recipient.City = reader.GetString(19);
                value.Recipient.State = reader.GetString(20);
                value.Recipient.PostalCode = reader.GetString(21);
                value.Recipient.DayPhone = reader.GetString(22);
                value.ShipTo.Name = reader.GetString(23);
                value.ShipTo.Address1 = reader.GetString(24);
                value.ShipTo.Address2 = reader.GetString(25);
                value.ShipTo.City = reader.GetString(26);
                value.ShipTo.State = reader.GetString(27);
                value.ShipTo.PostalCode = reader.GetString(28);
                value.ShipTo.DayPhone = reader.GetString(29);
                value.FreightLane = reader.GetString(30);
                value.Spur = reader.GetString(31);

                SqlDataAdapter adatper = new SqlDataAdapter("SELECT LineBalanceDue, MerchantLineNumber, TrxVendorSKU, TrxMerchantSKU, UPC, TrxQty, TrxUnitCost, Description1, Description2, UnitPrice, LineHandling, " +
                                                                "ExpectedShipDate, GST_HST_Extended, PST_Extended, GST_HST_Total, PST_Total, EncodedPrice, ReceivingInstructions " +
                                                                "FROM Sears_Order_Item WHERE TransactionId = \'" + targetTransaction + "\' ORDER BY MerchantLineNumber;", connection);
                DataTable table = new DataTable();
                adatper.Fill(table);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    value.LineBalanceDue.Add(Convert.ToDouble(table.Rows[i][0]));
                    value.MerchantLineNumber.Add(Convert.ToInt32(table.Rows[i][1]));
                    value.TrxVendorSKU.Add(table.Rows[i][2].ToString());
                    value.TrxMerchantSKU.Add(table.Rows[i][3].ToString());
                    value.UPC.Add(table.Rows[i][4].ToString());
                    value.TrxQty.Add(Convert.ToInt32(table.Rows[i][5]));
                    value.TrxUnitCost.Add(Convert.ToDouble(table.Rows[i][6]));
                    value.Description.Add(table.Rows[i][7].ToString());
                    value.Description2.Add(table.Rows[i][8].ToString());
                    value.UnitPrice.Add(Convert.ToDouble(table.Rows[i][9]));
                    value.LineHandling.Add(Convert.ToDouble(table.Rows[i][10]));
                    value.ExpectedShipDate.Add(Convert.ToDateTime(table.Rows[i][11]));
                    value.GST_HST_Extended.Add(Convert.ToDouble(table.Rows[i][12]));
                    value.PST_Extended.Add(Convert.ToDouble(table.Rows[i][13]));
                    value.GST_HST_Total.Add(Convert.ToDouble(table.Rows[i][14]));
                    value.PST_Total.Add(Convert.ToDouble(table.Rows[i][15]));
                    value.EncodedPrice.Add(table.Rows[i][16].ToString());
                    value.ReceivingInstructions.Add(table.Rows[i][17].ToString());
                }
            }

            return value;
        }
        #endregion

        /* a method that add a new transaction order to database */
        private void addNewOrder(SearsValues value)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                // add new transaction to order
                SqlCommand command = new SqlCommand("INSERT INTO Sears_Order " +
                                                    "(TransactionId, LineCount, PoNumber, TrxBalanceDue, VendorInvoiceNumber, PakageDetailId, ServiceLevel, TrackingNumber, ShipmentIdentificationNumber, OrderDate, PaymentMethod, CustOrderNumber, CustOrderDate, PackSlipMessage, BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToPostalCode, BillToPhone, RecipientName, RecipientAddress1, RecipientAddress2, RecipientCity, RecipientState, RecipientPostalCode, RecipientPhone, ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToPostalCode, ShipToPhone, FreightLane, Spur, Complete) Values " +
                                                    "(\'" + value.TransactionID + "\'," + value.LineCount + ",\'" + value.PoNumber + "\'," + value.TrxBalanceDue + ",\'" + value.VendorInvoiceNumber + "\',\'" + value.PackageDetailID + "\',\'" + value.ServiceLevel + "\',\'" + value.Package.TrackingNumber + "\',\'" + value.Package.IdentificationNumber + "\', \'" + value.OrderDate.ToString("yyyy-MM-dd") + "\',\'" + value.PaymentMethod + "\',\'" + value.CustOrderNumber + "\',\'" + value.CustOrderDate.ToString("yyyy-MM-dd") + "\',\'" + value.PackSlipMessage + "\',\'" +
                                                    value.BillTo.Name + "\',\'" + value.BillTo.Address1 + "\',\'" + value.BillTo.Address2 + "\',\'" + value.BillTo.City + "\',\'" + value.BillTo.State + "\',\'" + value.BillTo.PostalCode + "\',\'" + value.BillTo.DayPhone + "\',\'" + value.Recipient.Name + "\',\'" + value.Recipient.Address1 + "\',\'" + value.Recipient.Address2 + "\',\'" + value.Recipient.City + "\',\'" + value.Recipient.State + "\',\'" + value.Recipient.PostalCode + "\',\'" + value.Recipient.DayPhone + "\',\'" + value.ShipTo.Name + "\',\'" + value.ShipTo.Address1 + "\',\'" + value.ShipTo.Address2 + "\',\'" + value.ShipTo.City + "\',\'" + value.ShipTo.State + "\',\'" + value.ShipTo.PostalCode + "\',\'" + value.ShipTo.DayPhone +
                                                    "\',\'" + value.FreightLane + "\',\'" + value.Spur + "\',\'False\')", connection);
                connection.Open();
                command.ExecuteNonQuery();

                // add each item for the order to database
                for (int i = 0; i < value.LineCount; i++)
                {
                    command = new SqlCommand("INSERT INTO Sears_Order_Item " +
                                             "(TransactionId, LineBalanceDue, MerchantLineNumber, TrxVendorSKU, TrxMerchantSKU, UPC, TrxQty, TrxUnitCost, Description1, Description2, UnitPrice, LineHandling, ExpectedShipDate, GST_HST_Extended, PST_Extended, GST_HST_Total, PST_Total, EncodedPrice, ReceivingInstructions, Shipped, Cancelled) Values" +
                                             "(\'" + value.TransactionID + "\'," + value.LineBalanceDue[i] + "," + value.MerchantLineNumber[i] + ",\'"+ value.TrxVendorSKU[i] + "\',\'" + value.TrxMerchantSKU[i] + "\',\'" + value.UPC[i] + "\'," + value.TrxQty[i] + "," + value.TrxUnitCost[i] + ",\'" + value.Description[i].Replace("'","''") + "\',\'" + value.Description2[i].Replace("'", "''") +
                                             "\'," + value.UnitPrice[i] + "," + value.LineHandling[i] + ",\'" + value.ExpectedShipDate[i].ToString("yyyy-MM-dd") + "\',\'" + value.GST_HST_Extended[i] + "\',\'" + value.PST_Extended[i] + "\',\'" + value.GST_HST_Total[i] + "\',\'" + value.PST_Total[i] + "\',\'" + value.EncodedPrice[i] + "\',\'" + value.ReceivingInstructions[i] + "\',\'False\',\'False\')", connection);
                    command.ExecuteNonQuery();
                }
            }
        }

        #region Supporting Methods
        /* a method that get invoice number */
        private static string getInvoiceNumber()
        {
            // get iterator
            int iterator = !Properties.Settings.Default.Date.Equals(DateTime.Today) ? 1 : Properties.Settings.Default.Iterator;

            // create invoice number
            string invoice = "100";
            invoice += DateTime.Today.ToString("yyyyMMdd");

            for (int i = 0; i < 3 - iterator.ToString().Length; i++)
                invoice += "0";

            invoice += iterator.ToString();

            // save iterator
            iterator++;
            Properties.Settings.Default.Iterator = iterator;

            return invoice;
        }

        /* a method that get package id */
        private static string getPackageId()
        {
            // get iterator
            int iterator = !Properties.Settings.Default.Date.Equals(DateTime.Today) ? 1 : Properties.Settings.Default.Iterator;

            iterator++;
            return "SearsPack" + DateTime.Today.ToString("ddMMyy") + iterator;
        }

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
