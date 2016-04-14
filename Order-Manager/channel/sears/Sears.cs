using Order_Manager.supportingClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Tamir.SharpSsh;

namespace Order_Manager.channel.sears
{
    /*
     * A class that connect to Sears sftp server and manage all the orders for sears
     */
    public class Sears : Channel
    {
        // fields for directory on sftp server
        public const string SHIPMENT_DIR = "outgoing/orders/searscanada";
        public const string CONFIRM_DIR = "incoming/confirms/searscanada";

        // field for directory on local
        private readonly string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SearsOrders";
        private readonly string newOrderDir;
        private readonly string completeOrderDir;

        // field for sftp connection
        private readonly Sftp sftp;

        /* constructor that initialize folders for xml feed */
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

            // get credentials for sears sftp log on and initialize the field
            using (SqlConnection authCon = new SqlConnection(Properties.Settings.Default.ASCMcs))
            {
                SqlCommand command = new SqlCommand("SELECT Field1_Value, Field2_Value, Field3_Value FROM ASCM_Credentials WHERE Source = 'CommerceHub' and Client = 'Sears';", authCon);
                authCon.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                // initialize Sftp
                sftp = new Sftp(reader.GetString(2), reader.GetString(0), reader.GetString(1));
            }
        }

        #region Public Get
        /* a method that get all new order on the server and update to the database */
        public void GetOrder()
        {
            // get all the new file on the order directory to local new order storing directory
            string[] orderCheck = CheckOrderFile();
            GetOrder(newOrderDir, orderCheck);

            // read all the text of the file in the local directory
            string[] textXml = GetOrderFileText();

            // return the transaction that haved not been processed
            orderCheck = GetTransactionId(textXml);
            orderCheck = CheckTransaction(orderCheck);

            // get information for each unprocessed transaction and update the them to the database
            foreach (string transaction in orderCheck)
            {
                SearsValues value = GenerateValue(transaction, textXml);
                AddNewOrder(value);
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
                                                    "FROM Sears_Order WHERE Complete ='False' ORDER BY TransactionId;", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SearsValues value = new SearsValues();

                    string transactionId = reader.GetString(0);
                    value.TransactionId = transactionId;
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
                        value.TrxVendorSku.Add(table.Rows[i][2].ToString());
                        value.TrxMerchantSku.Add(table.Rows[i][3].ToString());
                        value.Upc.Add(table.Rows[i][4].ToString());
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
                string date = DateTime.Today.ToString("yyyy-MM-dd");
                SqlCommand command = new SqlCommand("SELECT TransactionId, TrackingNumber, ShipmentIdentificationNumber FROM Sears_Order " +
                                                    "WHERE TrackingNumber != '' AND (ShipDate = \'" + date + "\' OR CompleteDate = \'" + date + "\')", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    SearsValues value = new SearsValues
                    {
                        TransactionId = reader.GetString(0),
                        Package =
                        {
                            TrackingNumber = reader.GetString(1),
                            IdentificationNumber = reader.GetString(2)
                        }
                    };

                    list.Add(value);
                }
            }

            return list.ToArray();
        }
        #endregion

        #region Ship and Void 
        /* a method that mark the order as shipped but not readlly posting a confirm order to sears only for local reference */
        public void PostShip(string trackingNumber, string shipmentIdentificationNumber, string transactionId)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("UPDATE Sears_Order SET TrackingNumber = \'" + trackingNumber + "\', ShipmentIdentificationNumber = \'" + shipmentIdentificationNumber + "\', " 
                                                  + "ShipDate = \'" + DateTime.Today.ToString("yyyy-MM-dd") + "\' WHERE TransactionId = \'" + transactionId + "\';", connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        /* a method that mark the order as cancelled but not really posting a cancel order to sears only for local reference */
        public void PostVoid(string[] transactionId)
        {
            // generate the range 
            string candidate = transactionId.Aggregate("(", (current, id) => current + ('\'' + id + "\',"));
            candidate = candidate.Remove(candidate.Length - 1) + ')';

            // update to not shipped 
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                // for entire order cancellation
                SqlCommand command = new SqlCommand("UPDATE Sears_Order SET TrackingNumber = '', ShipmentIdentificationNumber = '', ShipDate = NULL WHERE TransactionId IN " + candidate, connection);
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
                SqlCommand command = new SqlCommand( "SELECT COUNT(*) FROM Sears_Order WHERE CustOrderDate = \'" + time.ToString("yyyy-MM-dd") + "\';", connection);
                connection.Open();

                count = (int) command.ExecuteScalar();
            }

            return count;
        }
        public int GetNumberOfShipped(DateTime time)
        {
            int count;

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
        private void GetOrder(string filePath, IEnumerable<string> fileList)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();

            foreach (string file in fileList)
            {
                if (file == "." || file == "..") continue;

                // change file to txt and save file to local
                string fileNameTxt = file.Replace("neworders", "txt");
                sftp.Get(SHIPMENT_DIR + "/" + file, filePath + "\\" + fileNameTxt);

                // after download the file delete it on the server (no need it anymore)
                ServerDelete.Delete(sftp.Host, sftp.Username, sftp.Password, SHIPMENT_DIR + "/" + file);
            }

            sftp.Close();
        }

        /* method that get the new order from sftp server */
        private IEnumerable<string> GetOrderName(string serverDir)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();
            ArrayList list = sftp.GetFileList(serverDir);
            sftp.Close();

            // retrieve all the files name to the list
            return list.Cast<object>().Select(item => item.ToString()).Where(itemName => itemName != "." && itemName != "..").ToArray();
        }

        /* method that get all the transaction in the file */
        private static string[] GetTransactionId(IEnumerable<string> fileText)
        {
            // local field for storing data
            List<string> list = new List<string>();

            // get all transaction id
            foreach (string text in fileText)
            {
                string copy = text;

                while (copy.Contains("hubOrder transactionID"))
                {
                    copy = SubstringMethod(copy, "transactionID", 15);
                    list.Add(GetTarget(copy));
                }
            }

            return list.ToArray();
        }

        /* method that return all the text of order xml feed */
        private string[] GetOrderFileText()
        {
            // get all the order file on local
            DirectoryInfo dirInfo = new DirectoryInfo(newOrderDir);
            FileInfo[] filesLocal = dirInfo.GetFiles("*.txt");       // getting all file that have been on local

            // read all the text of the file in the local directory
            int fileCount = filesLocal.Length;
            string[] textXml = new string[fileCount];      // xml text for each file
            for (int i = 0; i < fileCount; i++)
                textXml[i] = File.ReadAllText(newOrderDir + "\\" + filesLocal[i]);

            return textXml;
        }
        #endregion

        #region Checking Order Methods
        /* give the information about the new orders and also return all the new order files on server */
        private string[] CheckOrderFile()
        {
            // get all order file on local
            DirectoryInfo dirInfo = new DirectoryInfo(newOrderDir);
            FileInfo[] filesLocal = dirInfo.GetFiles("*.txt");       // getting all file that have been on local

            // get all order file on server
            IEnumerable<string> fileOnServer = GetOrderName(SHIPMENT_DIR);

            // check the number of new order on the server compare to ones on the computer
            return (from file1 in fileOnServer let found = filesLocal.Select(file2 => file2.ToString()).Any(file2Copy => file1.Remove(file1.LastIndexOf('.')) == file2Copy.Remove(file2Copy.LastIndexOf('.'))) where !found select file1).ToArray();
        }

        /* a method that receive all the current transaction and check the duplicate then only return the ones that have not been added to the database */
        private string[] CheckTransaction(string[] allTransactionList)
        {
            // input error check
            if (allTransactionList == null) throw new ArgumentNullException(nameof(allTransactionList));

            // get all complete transaction 
            List<string> completeTransactionList = new List<string>();
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT TransactionId FROM Sears_Order;", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    completeTransactionList.Add(reader.GetString(0));
            }

            // check the number of new transaction on the server compare to ones on the database
            return (from transaction in allTransactionList let found = completeTransactionList.Any(complete => complete.Contains(transaction)) where !found select transaction).ToArray();
        }
        #endregion

        #region XML Generation
        /* a method that generate xml order and upload to the sftp server and update database */
        public void GenerateXml(SearsValues value, Dictionary<int, string> cancelList)
        {
            // get other necessary values
            value.VendorInvoiceNumber = GetInvoiceNumber();
            value.PackageDetailId = GetPackageId();

            // fields for database update
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs);
            SqlCommand command = new SqlCommand {Connection = connection};
            connection.Open();

            #region XML Generation and Item Database Update
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                         "<ConfirmMessageBatch>" +
                         "<partnerID roleType=\"vendor\" name=\"Ashlin BPG Marketing, Inc.\">ashlinbpg</partnerID>" +
                         "<hubConfirm transactionID=\"" + value.TransactionId + "\">" +
                         "<participatingParty roleType=\"merchant\" name=\"Sears Canada\" participationCode=\"To:\">searscanada</participatingParty>" +
                         "<partnerTrxID>" + value.PartnerTrxId + "</partnerTrxID>" +
                         "<partnerTrxDate>" + DateTime.Today.ToString("yyyyMMdd") + "</partnerTrxDate>" +
                         "<poNumber>" + value.PoNumber + "</poNumber>";

            // the case if there is at least one order will be shipped -> add trx balance due
            if (cancelList.Count < value.LineCount)
            {
                // substract the amount that the orders are cancelled
                for (int i = 0; i < value.LineCount; i++)
                {
                    if (cancelList.Keys.Any(j => j == i))
                        value.TrxBalanceDue -= value.LineBalanceDue[i];
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
                foreach (int j in cancelList.Keys.Where(j => j == i - 1))
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
                    command.CommandText = "UPDATE Sears_Order_Item SET Cancelled = 'True' WHERE TransactionId = \'" + value.TransactionId +
                                             "\' AND MerchantLineNumber = \'" + value.MerchantLineNumber[j] + "\'";
                    command.ExecuteNonQuery();

                    isCancelled = true;
                    break;
                }

                if (!isCancelled)
                {
                    xml +=
                    "<hubAction>" +
                    "<action>v_ship</action>";

                    command.CommandText = "UPDATE Sears_Order_Item SET Shipped = 'True' WHERE TransactionId = \'" + value.TransactionId +
                                          "\' AND MerchantLineNumber = \'" + value.MerchantLineNumber[i - 1] + "\'";
                    command.ExecuteNonQuery();
                }
                xml +=
                "<merchantLineNumber>" + value.MerchantLineNumber[i - 1] + "</merchantLineNumber>" +
                "<trxVendorSKU>" + value.TrxVendorSku[i - 1] + "</trxVendorSKU>" +
                "<trxMerchantSKU>" + value.TrxMerchantSku[i - 1] + "</trxMerchantSKU>" +
                "<UPC>" + value.Upc[i - 1] + "</UPC>" +
                "<trxQty>" + value.TrxQty[i - 1] + "</trxQty>" +
                "<trxUnitCost>" + value.TrxUnitCost[i - 1] + "</trxUnitCost>";
                if (!isCancelled)
                    xml += "<packageDetailLink packageDetailID=\"" + value.PackageDetailId + "\"/>";
                xml +=
                "</hubAction>";
            }

            // the case if there is at least one order will be shipped
            if (cancelList.Count < value.LineCount)
            {
                xml +=
                    "<packageDetail packageDetailID=\"" + value.PackageDetailId + "\">" +
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
            string path = completeOrderDir + "\\" + value.TransactionId + ".xsd";
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(xml);
            writer.Close();
            #endregion

            // master database update
            command.CommandText = "UPDATE Sears_Order SET VendorInvoiceNumber = \'" + value.VendorInvoiceNumber + "\', PakageDetailId = \'" + value.PackageDetailId + "\', TrackingNumber = \'" + value.Package.TrackingNumber + "\', CompleteDate = \'" +
                                   DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss") + "\', ShipmentIdentificationNumber = \'" + value.Package.IdentificationNumber + "\', Complete = 'True' WHERE TransactionId = \'" + value.TransactionId + "\'";
            command.ExecuteNonQuery();
            connection.Close();

            // upload file to sftp server
            sftp.Connect();
            sftp.Put(path, CONFIRM_DIR);
            sftp.Close();
        }

        /* a method that generate SearsValues object for the given transaction number (first version -> take from local) */
        public SearsValues GenerateValue(string targetTransaction, string[] textXml)
        {
            // local field for storing values
            SearsValues value = new SearsValues();

            foreach (string text in textXml)
            {
                if (!text.Contains(targetTransaction)) continue;

                // field target text file that has the transaction
                string copy = text;

                // transaction id
                copy = SubstringMethod(copy, targetTransaction, 0);
                copy = copy.Remove(copy.IndexOf("/hubOrder>"));
                value.TransactionId = GetTarget(copy);

                // line count
                copy = SubstringMethod(copy, "lineCount", 10);
                value.LineCount = Convert.ToInt32(GetTarget(copy));

                // po number
                copy = SubstringMethod(copy, "poNumber", 9);
                value.PoNumber = GetTarget(copy);

                // order date
                copy = SubstringMethod(copy, "orderDate", 10);
                value.OrderDate = DateTime.ParseExact(GetTarget(copy), "yyyyMMdd", CultureInfo.InvariantCulture);

                // payment method
                copy = SubstringMethod(copy, "paymentMethod", 14);
                value.PaymentMethod = GetTarget(copy);

                // ship to person place id
                copy = SubstringMethod(copy, "shipTo personPlaceID", 22);
                string shipToId = GetTarget(copy);

                // bill to person place id 
                copy = SubstringMethod(copy, "billTo personPlaceID", 22);
                string billToId = GetTarget(copy);

                // customer person place id 
                copy = SubstringMethod(copy, "customer personPlaceID", 24);
                string customerPlaceId = GetTarget(copy);

                // cust order number
                copy = SubstringMethod(copy, "custOrderNumber", 16);
                value.CustOrderNumber = GetTarget(copy);

                // cust order date
                copy = SubstringMethod(copy, "custOrderDate", 14);
                value.CustOrderDate = DateTime.ParseExact(GetTarget(copy), "yyyyMMdd", CultureInfo.InvariantCulture);

                // pack slip message
                copy = SubstringMethod(copy, "packslipMessage", 16);
                value.PackSlipMessage = GetTarget(copy);

                // get info for each line count
                for (int i = 1; i <= value.LineCount; i++)
                {
                    // order line number
                    copy = SubstringMethod(copy, "merchantLineNumber", 19);
                    value.MerchantLineNumber.Add(Convert.ToInt32(GetTarget(copy)));

                    // trx qty
                    copy = SubstringMethod(copy, "qtyOrdered", 11);
                    value.TrxQty.Add(Convert.ToInt32(GetTarget(copy)));

                    // upc
                    if (copy.Contains("UPC"))
                    {
                        copy = SubstringMethod(copy, "UPC", 4);
                        value.Upc.Add(GetTarget(copy));
                    }
                    else
                        value.Upc.Add("");

                    // description 
                    copy = SubstringMethod(copy, "description", 12);
                    value.Description.Add(GetTarget(copy));

                    // description 2
                    if (copy.Contains("description2"))
                    {
                        copy = SubstringMethod(copy, "description2", 13);
                        value.Description2.Add(GetTarget(copy));
                    }
                    else
                        value.Description2.Add("");

                    // merchant sku
                    copy = SubstringMethod(copy, "merchantSKU", 12);
                    value.TrxMerchantSku.Add(GetTarget(copy));

                    // vendor sku
                    copy = SubstringMethod(copy, "vendorSKU", 10);
                    value.TrxVendorSku.Add(GetTarget(copy));

                    // unit price
                    copy = SubstringMethod(copy, "unitPrice", 10);
                    value.UnitPrice.Add(Convert.ToDouble(GetTarget(copy)));

                    // unit price
                    copy = SubstringMethod(copy, "unitCost", 9);
                    value.TrxUnitCost.Add(Convert.ToDouble(GetTarget(copy)));

                    // line handling
                    if (copy.Contains("lineHandling"))
                    {
                        copy = SubstringMethod(copy, "lineHandling", 13);
                        value.LineHandling.Add(Convert.ToDouble(GetTarget(copy)));
                    }
                    else
                        value.LineHandling.Add(0);

                    if (i == 1)
                    {
                        // service level / shipping code
                        copy = SubstringMethod(copy, "shippingCode", 13);
                        value.ServiceLevel = GetTarget(copy);
                    }

                    // trx balance due (plus line balance due)
                    copy = SubstringMethod(copy, "lineTotal", 10);
                    value.LineBalanceDue.Add(Convert.ToDouble(GetTarget(copy)));
                    value.TrxBalanceDue += Convert.ToDouble(GetTarget(copy));

                    // expected ship date
                    copy = SubstringMethod(copy, "expectedShipDate", 17);
                    value.ExpectedShipDate.Add(DateTime.ParseExact(GetTarget(copy), "yyyyMMdd", CultureInfo.InvariantCulture));

                    // gst and hst extended
                    copy = SubstringMethod(copy, "GST_HST_Extended", 16);
                    copy = SubstringMethod(copy, ">", 1);
                    value.GST_HST_Extended.Add(Convert.ToDouble(GetTarget(copy)));

                    // pst extended
                    copy = SubstringMethod(copy, "PST_Extended", 12);
                    copy = SubstringMethod(copy, ">", 1);
                    value.PST_Extended.Add(Convert.ToDouble(GetTarget(copy)));

                    // gst and hst
                    copy = SubstringMethod(copy, "GST_HST_Total", 16);
                    copy = SubstringMethod(copy, ">", 1);
                    value.GST_HST_Total.Add(Convert.ToDouble(GetTarget(copy)));

                    // pst
                    copy = SubstringMethod(copy, "PST_Total", 13);
                    copy = SubstringMethod(copy, ">", 1);
                    value.PST_Total.Add(Convert.ToDouble(GetTarget(copy)));

                    // encoded price 
                    copy = SubstringMethod(copy, "encodedPrice", 13);
                    value.EncodedPrice.Add(GetTarget(copy));

                    // ps receiving instructions
                    copy = SubstringMethod(copy, "psReceivingInstructions", 24);
                    value.ReceivingInstructions.Add(GetTarget(copy));
                }

                string copyCopy = copy;

                #region Bill To Address
                // bill to name
                copyCopy = SubstringMethod(copyCopy, billToId, billToId.Length);
                copyCopy = SubstringMethod(copyCopy, "name1", 6);
                value.BillTo.Name = GetTarget(copyCopy);

                // bill to address
                copyCopy = copyCopy.Remove(copyCopy.IndexOf("personPlace>"));
                copyCopy = SubstringMethod(copyCopy, "address1", 9);
                value.BillTo.Address1 = GetTarget(copyCopy);
                if (copyCopy.Contains("address2"))
                {
                    copyCopy = SubstringMethod(copyCopy, "address2", 9);
                    value.BillTo.Address2 = GetTarget(copyCopy);
                }

                // bill to city
                copyCopy = SubstringMethod(copyCopy, "city", 5);
                value.BillTo.City = GetTarget(copyCopy);

                // bill to state
                copyCopy = SubstringMethod(copyCopy, "state", 6);
                value.BillTo.State = GetTarget(copyCopy);

                // bill to postal code
                copyCopy = SubstringMethod(copyCopy, "postalCode", 11);
                value.BillTo.PostalCode = GetTarget(copyCopy);

                // bill to phone
                copyCopy = SubstringMethod(copyCopy, "dayPhone", 9);
                value.BillTo.DayPhone = GetTarget(copyCopy);
                #endregion

                copyCopy = copy;

                #region Recipient Address
                // recipient name
                copyCopy = SubstringMethod(copyCopy, customerPlaceId, customerPlaceId.Length);
                copyCopy = SubstringMethod(copyCopy, "name1", 6);
                value.Recipient.Name = GetTarget(copyCopy);

                // recipient address
                copyCopy = copyCopy.Remove(copyCopy.IndexOf("personPlace>"));
                copyCopy = SubstringMethod(copyCopy, "address1", 9);
                value.Recipient.Address1 = GetTarget(copyCopy);
                if (copyCopy.Contains("address2"))
                {
                    copyCopy = SubstringMethod(copyCopy, "address2", 9);
                    value.Recipient.Address2 = GetTarget(copyCopy);
                }

                // recipient city
                copyCopy = SubstringMethod(copyCopy, "city", 5);
                value.Recipient.City = GetTarget(copyCopy);

                // recipient state
                copyCopy = SubstringMethod(copyCopy, "state", 6);
                value.Recipient.State = GetTarget(copyCopy);

                // recipient postal code
                copyCopy = SubstringMethod(copyCopy, "postalCode", 11);
                value.Recipient.PostalCode = GetTarget(copyCopy);

                // recipient phone
                copyCopy = SubstringMethod(copyCopy, "dayPhone", 9);
                value.Recipient.DayPhone = GetTarget(copyCopy);
                #endregion

                copyCopy = copy;

                #region Ship To Address
                // ship to name
                copyCopy = SubstringMethod(copyCopy, shipToId, shipToId.Length);
                copyCopy = SubstringMethod(copyCopy, "name1", 6);
                value.ShipTo.Name = GetTarget(copyCopy);

                // ship to
                copyCopy = copyCopy.Remove(copyCopy.IndexOf("personPlace>"));
                copyCopy = SubstringMethod(copyCopy, "address1", 9);
                value.ShipTo.Address1 = GetTarget(copyCopy);
                if (copyCopy.Contains("address2"))
                {
                    copyCopy = SubstringMethod(copyCopy, "address2", 9);
                    value.ShipTo.Address2 = GetTarget(copyCopy);
                }

                // ship to city
                copyCopy = SubstringMethod(copyCopy, "city", 5);
                value.ShipTo.City = GetTarget(copyCopy);

                // ship to state
                copyCopy = SubstringMethod(copyCopy, "state", 6);
                value.ShipTo.State = GetTarget(copyCopy);

                // ship to postal code
                copyCopy = SubstringMethod(copyCopy, "postalCode", 11);
                value.ShipTo.PostalCode = GetTarget(copyCopy);

                // ship to day phone
                if (copyCopy.Contains("dayPhone"))
                {
                    copyCopy = SubstringMethod(copyCopy, "dayPhone", 9);
                    value.ShipTo.DayPhone = GetTarget(copyCopy);
                }

                // ship to partner person place id
                if (copyCopy.Contains("partnerPersonPlaceId"))
                {
                    copyCopy = SubstringMethod(copyCopy, "partnerPersonPlaceId", 21);
                    value.PartnerPersonPlaceId = GetTarget(copyCopy);
                }
                #endregion

                // freight lane & spur -> only if exist
                if (copyCopy.Contains("attnLine"))
                {
                    copyCopy = SubstringMethod(copyCopy, "attnLine", 9);
                    string attention = GetTarget(copyCopy);

                    int index = 0;
                    while ((char.IsLetter(attention[index]) || char.IsNumber(attention[index])) && attention[index] != ' ' && attention[index] != '_')
                        index++;
                    value.FreightLane = attention.Substring(0, index);
                    while (!char.IsNumber(attention[index]))
                        index++;
                    value.Spur = attention.Substring(index);
                }

                break;
            }

            return value;
        }
        /* second version -> take from database */
        public SearsValues GenerateValue(string targetTransaction)
        {
            // local field for storing values
            SearsValues value = new SearsValues();

            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                SqlCommand command = new SqlCommand("SELECT LineCount, PoNumber, TrxBalanceDue, ServiceLevel, OrderDate, paymentMethod, CustOrderNumber, CustOrderDate, PackSlipmessage, BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToPostalCode, BillToPhone, " +
                                                    "RecipientName, RecipientAddress1, RecipientAddress2, RecipientCity, RecipientState, RecipientPostalCode, RecipientPhone, ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToPostalCode, ShipToPhone, PartnerPersonPlaceId, FreightLane, Spur, " + 
                                                    "TrackingNumber, ShipmentIdentificationNumber FROM Sears_Order WHERE TransactionId = \'" + targetTransaction + "\'", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                value.TransactionId = targetTransaction;
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
                if (!reader.IsDBNull(30))
                    value.PartnerPersonPlaceId = reader.GetString(30);
                value.FreightLane = reader.GetString(31);
                value.Spur = reader.GetString(32);
                value.Package.TrackingNumber = reader.GetString(33);
                value.Package.IdentificationNumber = reader.GetString(34);

                SqlDataAdapter adatper = new SqlDataAdapter("SELECT LineBalanceDue, MerchantLineNumber, TrxVendorSKU, TrxMerchantSKU, UPC, TrxQty, TrxUnitCost, Description1, Description2, UnitPrice, LineHandling, " +
                                                            "ExpectedShipDate, GST_HST_Extended, PST_Extended, GST_HST_Total, PST_Total, EncodedPrice, ReceivingInstructions " +
                                                            "FROM Sears_Order_Item WHERE TransactionId = \'" + targetTransaction + "\' ORDER BY MerchantLineNumber;", connection);
                DataTable table = new DataTable();
                adatper.Fill(table);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    value.LineBalanceDue.Add(Convert.ToDouble(table.Rows[i][0]));
                    value.MerchantLineNumber.Add(Convert.ToInt32(table.Rows[i][1]));
                    value.TrxVendorSku.Add(table.Rows[i][2].ToString());
                    value.TrxMerchantSku.Add(table.Rows[i][3].ToString());
                    value.Upc.Add(table.Rows[i][4].ToString());
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

        /* a method that delete obsolete orders in database and clear all local files */
        public void Delete()
        {
            #region Database Delete
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                // get all the transaction id that are obsolete
                SqlCommand command = new SqlCommand("SELECT TransactionId FROM Sears_Order WHERE CompleteDate < \'" + DateTime.Today.AddDays(-120).ToString("yyyy-MM-dd") + "\'", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                // add transaction id range
                string range = "(";
                while (reader.Read())
                    range += '\'' + reader.GetString(0) + "\',";
                reader.Close();

                // the case if has something to delete
                if (range != "(")
                {
                    range = range.Remove(range.Length - 1) + ')';

                    // delete items
                    command.CommandText = "DELETE FROM Sears_Order_Item WHERE TransactionId IN " + range;
                    command.ExecuteNonQuery();

                    // delete orders
                    command.CommandText = "DELETE FROM Sears_Order WHERE TransactionId IN " + range;
                    command.ExecuteNonQuery();
                }
            }
            #endregion

            // Local Delete
            new DirectoryInfo(newOrderDir).Delete(true);
        }

        /* a method that add a new transaction order to database */
        private static void AddNewOrder(SearsValues value)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.CHcs))
            {
                // add new transaction to order
                SqlCommand command = new SqlCommand("INSERT INTO Sears_Order " +
                                                    "(TransactionId, LineCount, PoNumber, TrxBalanceDue, VendorInvoiceNumber, PakageDetailId, ServiceLevel, TrackingNumber, ShipmentIdentificationNumber, OrderDate, PaymentMethod, CustOrderNumber, CustOrderDate, PackSlipMessage, BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToPostalCode, BillToPhone, RecipientName, RecipientAddress1, RecipientAddress2, RecipientCity, RecipientState, RecipientPostalCode, RecipientPhone, ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToPostalCode, ShipToPhone, PartnerPersonPlaceId, FreightLane, Spur, Complete) Values " +
                                                    "(\'" + value.TransactionId + "\'," + value.LineCount + ",\'" + value.PoNumber + "\'," + value.TrxBalanceDue + ",\'" + value.VendorInvoiceNumber + "\',\'" + value.PackageDetailId + "\',\'" + value.ServiceLevel + "\',\'" + value.Package.TrackingNumber + "\',\'" + value.Package.IdentificationNumber + "\', \'" + value.OrderDate.ToString("yyyy-MM-dd") + "\',\'" + value.PaymentMethod + "\',\'" + value.CustOrderNumber + "\',\'" + value.CustOrderDate.ToString("yyyy-MM-dd") + "\',\'" + value.PackSlipMessage + "\',\'" +
                                                    value.BillTo.Name.Replace("'","''") + "\',\'" + value.BillTo.Address1.Replace("'", "''") + "\',\'" + value.BillTo.Address2.Replace("'", "''") + "\',\'" + value.BillTo.City.Replace("'", "''") + "\',\'" + value.BillTo.State + "\',\'" + value.BillTo.PostalCode + "\',\'" + value.BillTo.DayPhone + "\',\'" + value.Recipient.Name.Replace("'", "''") + "\',\'" + value.Recipient.Address1.Replace("'", "''") + "\',\'" + value.Recipient.Address2.Replace("'", "''") + "\',\'" + value.Recipient.City.Replace("'", "''") + "\',\'" + value.Recipient.State + "\',\'" + value.Recipient.PostalCode + "\',\'" + value.Recipient.DayPhone + "\',\'" + 
                                                    value.ShipTo.Name.Replace("'", "''") + "\',\'" + value.ShipTo.Address1.Replace("'", "''") + "\',\'" + value.ShipTo.Address2.Replace("'", "''") + "\',\'" + value.ShipTo.City.Replace("'", "''") + "\',\'" + value.ShipTo.State + "\',\'" + value.ShipTo.PostalCode + "\',\'" + value.ShipTo.DayPhone +"\',\'" + value.PartnerPersonPlaceId + "\',\'" + value.FreightLane + "\',\'" + value.Spur + "\',\'False\')", connection);
                connection.Open();
                command.ExecuteNonQuery();

                // add each item for the order to database
                for (int i = 0; i < value.LineCount; i++)
                {
                    command.CommandText = "INSERT INTO Sears_Order_Item " +
                                          "(TransactionId, LineBalanceDue, MerchantLineNumber, TrxVendorSKU, TrxMerchantSKU, UPC, TrxQty, TrxUnitCost, Description1, Description2, UnitPrice, LineHandling, ExpectedShipDate, GST_HST_Extended, PST_Extended, GST_HST_Total, PST_Total, EncodedPrice, ReceivingInstructions, Shipped, Cancelled) Values" +
                                          "(\'" + value.TransactionId + "\'," + value.LineBalanceDue[i] + "," + value.MerchantLineNumber[i] + ",\'" + value.TrxVendorSku[i] + "\',\'" + value.TrxMerchantSku[i] + "\',\'" + value.Upc[i] + "\'," + value.TrxQty[i] + "," + value.TrxUnitCost[i] + ",\'" + value.Description[i].Replace("'", "''") + "\',\'" + value.Description2[i].Replace("'", "''") +
                                          "\'," + value.UnitPrice[i] + "," + value.LineHandling[i] + ",\'" + value.ExpectedShipDate[i].ToString("yyyy-MM-dd") + "\',\'" + value.GST_HST_Extended[i] + "\',\'" + value.PST_Extended[i] + "\',\'" + value.GST_HST_Total[i] + "\',\'" + value.PST_Total[i] + "\',\'" + value.EncodedPrice[i] + "\',\'" + value.ReceivingInstructions[i] + "\',\'False\',\'False\')";
                    command.ExecuteNonQuery();
                }
            }
        }

        #region Supporting Methods
        /* a method that get invoice number */
        private static string GetInvoiceNumber()
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
        private static string GetPackageId()
        {
            // get iterator
            int iterator = !Properties.Settings.Default.Date.Equals(DateTime.Today) ? 1 : Properties.Settings.Default.Iterator;

            iterator++;
            return "SearsPack" + DateTime.Today.ToString("ddMMyy") + iterator;
        }

        /* a method that substring the given string */
        private static string SubstringMethod(string original, string startingString, int additionIndex)
        {
            return original.Substring(original.IndexOf(startingString, StringComparison.Ordinal) + additionIndex);
        }

        /* a method that get the next target token */
        private static string GetTarget(string text)
        {
            int i = 0;
            while (text[i] != '<' && text[i] != '>' && text[i] != '"')
                i++;

            return text.Substring(0, i);
        }
        #endregion
    }
}
