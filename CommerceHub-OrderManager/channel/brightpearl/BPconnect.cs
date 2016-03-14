using CommerceHub_OrderManager.channel.sears;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using CommerceHub_OrderManager.supportingClasses;

namespace CommerceHub_OrderManager.channel.brightpearl
{
    /* 
     * A class that post order request to brightpearl
     */
    public class BPconnect
    {
        // fields for brightpearl integration
        private GetRequest get;
        private PostRequest post;

        // public field for accessing the status of the current progress
        public string Status { get; set; }

        /* constructor that initialize request objects*/
        public BPconnect()
        {
            // initialize API authentication
            SqlConnection authenticationConnection = new SqlConnection(Properties.Settings.Default.ASCMcs);
            SqlCommand getAuthetication = new SqlCommand("SELECT Field3_Value, Field1_Value FROM ASCM_Credentials WHERE Source = \'Brightpearl Testing\';", authenticationConnection);
            authenticationConnection.Open();
            SqlDataReader reader = getAuthetication.ExecuteReader();
            reader.Read();
            string appRef = reader.GetString(0);
            string appToken = reader.GetString(1);
            authenticationConnection.Close();

            // initializes request fields
            get = new GetRequest(appRef, appToken);
            post = new PostRequest(appRef, appToken);

            // set status to default -> nothing
            Status = "";
        }

        /* a method that post sears order to brightpearl on sears account */
        public void postOrder(SearsValues value, int[] cancelList)
        {
            // check if the order is cancelled entirely -> if it is just return no need to post it
            if (cancelList.Length >= value.LineCount)
                return;

            #region Posting Order to Sears Account on BP
            // field for receipt
            double total = value.TrxBalanceDue;

            // initialize order BPvalues object
            BPvalues orderValue = new BPvalues(value.Recipient, value.TransactionID, value.CustOrderDate, 7, 7, null, null, 0, 0, 0, 0);

            // post order
            string orderId = post.postOrderRequest("2854", orderValue);
            Status = "Getting order ID";
                if (orderId == "Error")
                {
                    Status = "Error occur during order post";
                    do
                    {
                        Thread.Sleep(5000);
                        orderId = post.postOrderRequest("2854", orderValue);
                    } while (orderId == "Error");
                }

                // calculate the total amount when excluding the cancelled items
                for (int i = 0; i < value.LineCount; i++)
                {
                    // boolean flag to see if the item is cancelled
                    bool cancelled = false;

                    // check if the item is cancelled or not
                    foreach (int j in cancelList.Where(j => j == i))
                    {
                        // substract the item's price
                        total -= value.LineBalanceDue[j];

                        cancelled = true;
                        break;
                    }

                    // the case if not cancel post it to brightpearl
                    if (cancelled) continue;
                    // GST, HST, PST
                    double tax = value.GST_HST_Extended[i] + value.PST_Extended[i] + value.GST_HST_Total[i] + value.PST_Total[i];

                    // initialize item BPvalues object
                    double netPrice = value.UnitPrice[i] * value.TrxQty[i];
                    BPvalues itemValue = new BPvalues(value.Recipient, null, DateTime.Today, 7, 7, value.TrxVendorSKU[i], value.Description[i], value.TrxQty[i], netPrice, tax, value.LineBalanceDue[i]);

                    // post order row
                    string orderRowId = post.postOrderRowRequest(orderId, itemValue);
                    Status = "Getting order row ID";
                    if (orderRowId == "Error")
                    {
                        Status = "Error occur during order row post " + i;
                        do
                        {
                            Thread.Sleep(5000);
                            orderRowId = post.postOrderRowRequest(orderId, itemValue);
                        } while (orderRowId == "Error");
                    }

                    // post reservation
                    string reservation = post.postReservationRequest(orderId, orderRowId, itemValue);
                    Status = "Posting reservation request " + i;
                    if (reservation == "503")
                    {
                        Status = "Error occur during reservation post " + i;
                        do
                        {
                            Thread.Sleep(5000);
                            reservation = post.postReservationRequest(orderId, orderRowId, itemValue);
                        } while (reservation == "503");
                    }
                }

                // set total paid to bp value
                orderValue.TotalPaid = total;

                // post receipt
                post.postReceipt(orderId, "2854", orderValue);
                Status = "Posting receipt";
                if (post.HasError)
                {
                    Status = "Error occur during receipt post";
                    do
                    {
                        Thread.Sleep(5000);
                        post.postReceipt(orderId, "2854", orderValue);
                    } while (post.HasError);
                }
            #endregion
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
            while (text[i] != '"' && text[i] != ',' && text[i] != '}')
                i++;

            return text.Substring(0, i);
        }
        #endregion

        /* 
         * A class that Get request from brightpearl
         */
        private class GetRequest
        {
            // fields for web request
            private WebRequest request;
            private HttpWebResponse response;

            // fields for credentials
            private string appRef;
            private string appToken;

            /* constructor to initialize the web request of app reference and app token */
            public GetRequest(string appRef, string appToken)
            {
                this.appRef = appRef;
                this.appToken = appToken;
            }

            /* a method that return customer id from given firstname and lastname*/
            public string getCustomerId(string firstName, string lastName, string postalCode)
            {
                #region Contact ID Get
                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlintest/contact-service/contact-search?firstName=" + firstName + "&lastName=" + lastName;

                // post request to uri
                request = WebRequest.Create(uri);
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);
                request.Method = "GET";

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();

                // read all the text from JSON response
                string textJSON;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    textJSON = streamReader.ReadToEnd();
                }

                // check if there is result return or not
                textJSON = substringMethod(textJSON, "resultsReturned", 17);
                int number = Convert.ToInt32(getTarget(textJSON));

                // the case if there is no result found, just return 
                if (number < 1)
                    return null;

                // start getting customer id
                // this is for the first id
                string[] list = new string[number];
                textJSON = substringMethod(textJSON, "results\":", 11);
                list[0] = getTarget(textJSON);
                textJSON = substringMethod(textJSON, "],[", 3);

                // proceed to next token and get the id (if have more than 1)
                for (int i = 1; i < number; i++)
                {
                    list[i] = getTarget(textJSON);

                    // proceed to next token
                    textJSON = substringMethod(textJSON, "],[", 3);
                }
                #endregion

                #region Postal Code Compare
                // generate uri for getting more information on the customer id found to compare result
                uri = "https://ws-use.brightpearl.com/public-api/ashlintest/contact-service/contact/";

                for (int i = 0; i < number; i++)
                    uri += list[i] + ',';

                uri = uri.Remove(uri.LastIndexOf(',')) + "?includeOptional=customFields,postalAddresses";
                Console.WriteLine(uri);
                Console.ReadLine();

                request = WebRequest.Create(uri);
                request.Headers.Add("brightpearl-app-ref", "ashlintest_intern-1002");
                request.Headers.Add("brightpearl-account-token", "aZroyMTQ7Lf3EygEbyvXYTsYnDB7S4HjgHjuxjbMA00=");
                request.Method = "GET";

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();

                // read all the text from JSON response
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    textJSON = streamReader.ReadToEnd();
                }

                // looping through each customer's postal code to see if the cutomer exist in these IDs
                for (int i = 0; i < number; i++)
                {
                    // cut the string to the cloest id 
                    textJSON = substringMethod(textJSON, "\"contactId\":", 11);

                    // get the text only for the current contact id
                    string copy;
                    if (textJSON.Contains("contactId"))
                        copy = textJSON.Remove(textJSON.IndexOf("contactId"));
                    else
                        copy = textJSON;

                    // postal code get
                    if (copy.Contains("postalCode"))
                    {
                        copy = substringMethod(copy, "postalCode", 13);
                        copy = getTarget(copy);
                    }
                    else
                        copy = "";

                    if (postalCode.Replace(" ", string.Empty) == copy.Replace(" ", string.Empty))
                        return list[i];
                }
                #endregion

                return null;
            }

            /* a method that return product id from given sku */
            public string getProductId(string sku)
            {
                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlintest/product-service/product-search?SKU=" + sku;

                // post request to uri
                request = WebRequest.Create(uri);
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);
                request.Method = "GET";

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();

                // read all the text from JSON response
                string textJSON;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    textJSON = streamReader.ReadToEnd();
                }

                // check if there is result return or not
                textJSON = substringMethod(textJSON, "resultsReturned", 17);
                if (Convert.ToInt32(getTarget(textJSON)) < 1)
                    return null;

                // getting product id
                textJSON = substringMethod(textJSON, "\"results\":", 12);

                return getTarget(textJSON);
            }
        }

        /* 
         * A class that Post request to brightpearl 
         */
        private class PostRequest
        {
            // fields for web request
            private HttpWebRequest request;
            private HttpWebResponse response;

            // fields for credentials
            private string appRef;
            private string appToken;

            // field for telling client if there is error occur
            public bool HasError { get; set; }

            /* constructor to initialize the web request of app reference and app token */
            public PostRequest(string appRef, string appToken)
            {
                this.appRef = appRef;
                this.appToken = appToken;

                HasError = false;
            }

            /* post new address to API */
            public string postAddressRequest(Address address)
            {
                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlintest/contact-service/postal-address";

                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // generate the JSON file for address post
                string textJSON = "{\"addressLine1\":\"" + address.Address1 + "\",\"addressLine2\":\"" + address.Address2 + "\",\"addressLine3\":\"" + address.City + "\",\"addressLine4\":\"" + address.State + "\",\"postalCode\":\"" + address.PostalCode + "\",\"countryIsoCode\":\"CAN\"}";

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJSON);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();
                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                result = substringMethod(result, ":", 1);
                return getTarget(result);  //return the addresss ID
            }

            /* post new customer to API */
            public string postContactRequest(string addressID, BPvalues value)
            {
                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlintest/contact-service/contact";

                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // generate JSON file for contact post
                string textJSON = "{\"firstName\":\"" + value.Address.Name.Remove(value.Address.Name.IndexOf(' ')) + "\",\"lastName\":\"" + value.Address.Name.Substring(value.Address.Name.IndexOf(' ') + 1) + "\",\"postAddressIds\":{\"DEF\":" + addressID + ",\"BIL\":" + addressID + ",\"DEL\":" + addressID + "}," + 
                                  "\"communication\":{\"telephones\":{\"PRI\":\"" + value.Address.DayPhone + "\"}},\"relationshipToAccount\":{\"isSupplier\": false,\"isStaff\":false,\"isCustomer\":true},\"financialDetails\":{\"priceListId\": 5}}";

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJSON);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();
                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                result = substringMethod(result, ":", 1);
                return getTarget(result);  //return the contact ID
            }

            /* post new order to API */
            public string postOrderRequest(string contactID, BPvalues value)
            {
                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlintest/order-service/order";

                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // generate JSON file for order post
                string textJSON = "{\"orderTypeCode\":\"SO\",\"reference\":\"" + value.Reference + "\",\"placeOn\":\"" + value.PlaceOn.ToString("yyyy-MM-dd") + "T00:00:00+00:00\",\"orderStatus\":{\"orderStatusId\":2}," + "\"delivery\":{\"deliveryDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(' ', 'T') + "+00:00\",\"shippingMethodId\":7},\"currency\":{\"orderCurrencyCode\":\"CAD\"},\"parties\":{\"customer\":{\"contactId\":" + 
                                  contactID + "},\"delivery\":{\"addressFullName\":\"" + value.Address.Name + "\",\"addressLine1\":\"" + value.Address.Address1 + "\",\"addressLine2\":\"" + value.Address.Address2 + "\",\"addressLine3\":\"" + value.Address.City + "\",\"addressLine4\":\"" + value.Address.State + "\",\"postalCode\":\"" + value.Address.PostalCode + "\",\"countryIsoCode\":\"CAN\",\"telephone\":\"" + value.Address.DayPhone + "\"}},\"assignment\":{\"current\":{\"channelId\":" + value.ChannelId + "}}}";


                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJSON);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }

                // get the response from the server
                try    // might have server internal error, so do it in try and catch
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch    // HTTP response 500
                {
                    return "Error";    // cannot post order, return error instead
                }

                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                result = substringMethod(result, ":", 1);
                return getTarget(result);  //return the order ID
            }

            /* post new order row to API */
            public string postOrderRowRequest(string orderID, BPvalues value)
            {
                // get product id
                GetRequest get = new GetRequest(appRef, appToken);
                string productId = get.getProductId(value.SKU);

                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlintest/order-service/order/" + orderID + "/row";
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // tax determination
                string taxCode;
                switch (value.Address.State)
                {
                    case "NB":
                        taxCode = "NB";
                        break;
                    case "NF":
                        taxCode = "NF";
                        break;
                    case "NL":
                        taxCode = "NL";
                        break;
                    case "NS":
                        taxCode = "NS";
                        break;
                    case "ON":
                        taxCode = "ON";
                        break;
                    case "PEI":
                        taxCode = "PEI";
                        break;
                    case "BC":
                        taxCode = "BC";
                        break;
                    case "MAN":
                        taxCode = "MAN";
                        break;
                    case "PQ":
                        taxCode = "PQ";
                        break;
                    case "SK":
                        taxCode = "SK";
                        break;
                    case "AB":
                        taxCode = "AB";
                        break;
                    case "NV":
                        taxCode = "NV";
                        break;
                    case "YK":
                        taxCode = "YK";
                        break;
                    default:
                        taxCode = "N";
                        break;
                }

                // generate JSON file for order row post
                string textJSON;
                if (productId != null)
                    textJSON = "{\"productId\":\"" + productId + "\",\"quantity\":{\"magnitude\":\"" + value.Quantity + "\"},\"rowValue\":{\"taxCode\":\"" + taxCode + "\",\"rowNet\":{\"value\":\"" + Math.Round(value.RowNet, 4) + "\"},\"rowTax\":{\"value\":\"" + Math.Round(value.RowTax, 4) + "\"}}}";
                else
                    textJSON = "{\"productName\":\"" + value.ProductName + " " + value.SKU  + "\",\"quantity\":{\"magnitude\":\"" + value.Quantity + "\"},\"rowValue\":{\"taxCode\":\"" + taxCode + "\",\"rowNet\":{\"value\":\"" + Math.Round(value.RowNet, 4) + "\"},\"rowTax\":{\"value\":\"" + Math.Round(value.RowTax, 4) + "\"}}}";


                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJSON);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }

                // get the response from server
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                    return "Error";     // 503 Server Unabailable
                }
                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                result = substringMethod(result, ":", 1);
                return getTarget(result);  //return the order row ID
            }

            /* post reservation request to API return the message*/
            public string postReservationRequest(string orderID, string orderRowID, BPvalues value)
            {
                // get product id
                GetRequest get = new GetRequest(appRef, appToken);
                string productId = get.getProductId(value.SKU);

                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlintest/warehouse-service/order/" + orderID + "/reservation/warehouse/2";
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // generate JSON file for order row post
                string textJSON;
                if (productId != null)
                    textJSON = "{\"products\": [{\"productId\": \"" + productId + "\",\"salesOrderRowId\": \"" + orderRowID + "\",\"quantity\":\"" + value.Quantity + "\"}]}";
                else
                    return null;

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJSON);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }

                // get response from the server to see if there has error or not
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        response = e.Response as HttpWebResponse;
                        if ((int)response.StatusCode == 503)
                            return "Error";    // web server 503 server unavailable
                    }
                }

                return null;
            }

            /* post receipt to API */
            public void postReceipt(string orderID, string contactID, BPvalues value)
            {
                // reset boolean flag to false 
                HasError = false;

                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlintest/accounting-service/sales-receipt";
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                string textJSON = "{\"orderId\":\"" + orderID + "\",\"customerId\":\"" + contactID + "\",\"received\":{\"currency\":\"CAD\",\"value\":\"" + Math.Round(value.TotalPaid, 4) + "\"},\"bankAccountNominalCode\":\"1001\",\"channelId\":" + value.ChannelId + ",\"taxDate\":\"" + value.PlaceOn.ToString("yyyy-MM-dd") + "T00:00:00+00:00\"}";

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJSON);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }

                // get response from server to see if there is error or not
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                    HasError = true;
                    return;
                }

                // reset has error to false just in case
                HasError = false;
            }
        }
    }
}
