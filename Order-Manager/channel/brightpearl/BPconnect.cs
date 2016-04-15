using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Order_Manager.channel.sears;
using Order_Manager.supportingClasses.Address;
using Order_Manager.channel.shop.ca;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Order_Manager.channel.giantTiger;

namespace Order_Manager.channel.brightpearl
{
    /* 
     * A class that post order request to brightpearl
     */
    public class BPconnect
    {
        // fields for brightpearl integration
        private GetRequest get;
        private readonly PostRequest post;

        // public field for accessing the status of the current progress
        public string Status { get; private set; } = "";

        /* constructor that initialize request objects*/
        public BPconnect()
        {
            // initialize API authentication
            SqlConnection authenticationConnection = new SqlConnection(Properties.Settings.Default.ASCMcs);
            SqlCommand getAuthetication = new SqlCommand("SELECT Field3_Value, Field1_Value FROM ASCM_Credentials WHERE Source = \'Brightpearl\';", authenticationConnection);
            authenticationConnection.Open();
            SqlDataReader reader = getAuthetication.ExecuteReader();
            reader.Read();
            string appRef = reader.GetString(0);
            string appToken = reader.GetString(1);
            authenticationConnection.Close();

            // initializes request fields
            get = new GetRequest(appRef, appToken);
            post = new PostRequest(appRef, appToken);
        }

        /* a method that post sears order to brightpearl on sears account */
        public void PostOrder(SearsValues value, int[] cancelList)
        {
            // check if the order is cancelled entirely -> if it is just return no need to post it
            if (cancelList.Length >= value.LineCount)
                return;

            #region Posting Order to Sears Account on BP
            // initialize order BPvalues object
            BPvalues orderValue = new BPvalues(value.Recipient, value.CustOrderNumber, value.CustOrderDate, 1, 7, null, null, 0, 0, 0, 0);

            // post order
            string orderId = post.PostOrderRequest("2854", orderValue);
            Status = "Getting order ID - Sears";
            if (post.HasError)
            {
                Status = "Error occur during order post - Sears";
                do
                {
                    Thread.Sleep(5000);
                    orderId = post.PostOrderRequest("2854", orderValue);
                } while (post.HasError);
            }

            // calculate the total amount when excluding the cancelled items
            for (int i = 0; i < value.LineCount; i++)
            {
                // the case if not cancel post it to brightpearl
                if (cancelList.Where(j => j == i).Any()) continue;

                #region Tax Determination
                double tax;
                switch (value.ShipTo.State)
                {
                    case "NB":
                        tax = 0.13;
                        break;
                    case "NF":
                        tax = 0.15;
                        break;
                    case "NL":
                        tax = 0.15;
                        break;
                    case "NS":
                        tax = 0.15;
                        break;
                    case "ON":
                        tax = 0.13;
                        break;
                    case "PEI":
                        tax = 0.14;
                        break;
                    case "BC":
                        tax = 0.05;
                        break;
                    case "MAN":
                        tax = 0.05;
                        break;
                    case "PQ":
                        tax = 0.05;
                        break;
                    case "QC":
                        tax = 0.05;
                        break;
                    case "SK":
                        tax = 0.05;
                        break;
                    case "AB":
                        tax = 0.05;
                        break;
                    case "NV":
                        tax = 0.05;
                        break;
                    case "YK":
                        tax = 0.05;
                        break;
                    default:
                        tax = 0;
                        break;
                }
                #endregion

                // initialize BPvalues object -> no need total paid ( this is unit cost & no recipt )
                BPvalues itemValue = new BPvalues(value.Recipient, null, DateTime.Today, 1, 7, value.TrxVendorSku[i], value.Description[i], value.TrxQty[i], value.TrxUnitCost[i], value.TrxUnitCost[i] * tax, 0);

                // post order row
                string orderRowId = post.PostOrderRowRequest(orderId, itemValue);
                Status = "Getting order row ID";
                if (post.HasError)
                {
                    Status = "Error occur during order row post " + i + " - Sears";
                    do
                    {
                        Thread.Sleep(5000);
                        orderRowId = post.PostOrderRowRequest(orderId, itemValue);
                    } while (post.HasError);
                }

                // post reservation
                post.PostReservationRequest(orderId, orderRowId, itemValue);
                Status = "Posting reservation request " + i;
                if (!post.HasError) continue;

                Status = "Error occur during reservation post " + i + "- Sears";
                do
                {
                    Thread.Sleep(5000);
                    post.PostReservationRequest(orderId, orderRowId, itemValue);
                } while (post.HasError);
            }
            #endregion
        }

        /* a method that post shop.ca order to brightpearl on shop.ca account */
        public void PostOrder(ShopCaValues value, int[] cancelList)
        {
            // check if the order is cancelled entirely -> if it is just return no need to post it
            if (cancelList.Length >= value.OrderItemId.Count)
                return;

            #region Posting Order to Shop.ca Account on BP
            // initialize order BPvalues object
            BPvalues orderValue = new BPvalues(value.ShipTo, value.OrderId, value.OrderCreateDate, 15, 1, null, null, 0, 0, 0, 0);

            // post order
            string orderId = post.PostOrderRequest("2897", orderValue);
            Status = "Getting order ID";
            if (post.HasError)
            {
                Status = "Error occur during order post - Shop.ca";
                do
                {
                    Thread.Sleep(5000);
                    orderId = post.PostOrderRequest("2897", orderValue);
                } while (post.HasError);
            }

            // calculate the total amount when excluding the cancelled items
            for (int i = 0; i < value.OrderItemId.Count; i++)
            {
                // the case if not cancel post it to brightpearl
                if (cancelList.Where(j => j == i).Any()) continue;

                // initialize BPvalues object
                BPvalues itemValue = new BPvalues(value.ShipTo, null, DateTime.Today, 15, 1, value.Sku[i], value.Title[i], value.Quantity[i], Convert.ToDouble(value.ExtendedItemPrice[i]), Convert.ToDouble(value.ItemTax[i]), 0);

                // post order row
                string orderRowId = post.PostOrderRowRequest(orderId, itemValue);
                Status = "Getting order row ID";
                if (post.HasError)
                {
                    Status = "Error occur during order row post " + i + "- Shop.ca";
                    do
                    {
                        Thread.Sleep(5000);
                        orderRowId = post.PostOrderRowRequest(orderId, itemValue);
                    } while (post.HasError);
                }

                // post reservation
                post.PostReservationRequest(orderId, orderRowId, itemValue);
                Status = "Posting reservation request " + i;
                if (!post.HasError) continue;

                Status = "Error occur during reservation post " + i + "- Shop.ca";
                do
                {
                    Thread.Sleep(5000);
                    post.PostReservationRequest(orderId, orderRowId, itemValue);
                } while (post.HasError);
            }
            #endregion
        }

        /* a method that post giant tiger order to brightpearl on giant tiger account */
        public void PostOrder(GiantTigerValues value, int[] cancelList)
        {
            // check if the order is cancelled entirely -> if it is just return no need to post it
            if (cancelList.Length >= value.VendorSku.Count)
                return;

            #region Posting Order to Shop.ca Account on BP
            // initialize order BPvalues object
            BPvalues orderValue = new BPvalues(value.ShipTo, value.PoNumber, value.OrderDate, 12, 1, null, null, 0, 0, 0, 0);

            // post order
            string orderId = post.PostOrderRequest("10115", orderValue);
            Status = "Getting order ID";
            if (post.HasError)
            {
                Status = "Error occur during order post - Giant Tiger";
                do
                {
                    Thread.Sleep(5000);
                    orderId = post.PostOrderRequest("10115", orderValue);
                } while (post.HasError);
            }

            // calculate the total amount when excluding the cancelled items
            for (int i = 0; i < value.VendorSku.Count; i++)
            {
                // the case if not cancel post it to brightpearl
                if (cancelList.Where(j => j == i).Any()) continue;

                #region Tax Determination
                double tax;
                switch (value.ShipTo.State)
                {
                    case "NB":
                        tax = 0.13;
                        break;
                    case "NF":
                        tax = 0.15;
                        break;
                    case "NL":
                        tax = 0.15;
                        break;
                    case "NS":
                        tax = 0.15;
                        break;
                    case "ON":
                        tax = 0.13;
                        break;
                    case "PEI":
                        tax = 0.14;
                        break;
                    case "BC":
                        tax = 0.05;
                        break;
                    case "MAN":
                        tax = 0.05;
                        break;
                    case "PQ":
                        tax = 0.05;
                        break;
                    case "QC":
                        tax = 0.05;
                        break;
                    case "SK":
                        tax = 0.05;
                        break;
                    case "AB":
                        tax = 0.05;
                        break;
                    case "NV":
                        tax = 0.05;
                        break;
                    case "YK":
                        tax = 0.05;
                        break;
                    default:
                        tax = 0;
                        break;
                }
                #endregion

                // initialize BPvalues object -> no need total paid ( this is unit cost & no recipt )
                BPvalues itemValue = new BPvalues(value.ShipTo, null, DateTime.Today, 12, 1, value.VendorSku[i], "Host SKU: " + value.HostSku[i], value.Quantity[i], value.UnitCost[i], value.UnitCost[i] * tax, 0);

                // post order row
                string orderRowId = post.PostOrderRowRequest(orderId, itemValue);
                Status = "Getting order row ID";
                if (post.HasError)
                {
                    Status = "Error occur during order row post " + i + "- Giant Tiger";
                    do
                    {
                        Thread.Sleep(5000);
                        orderRowId = post.PostOrderRowRequest(orderId, itemValue);
                    } while (post.HasError);
                }

                // post reservation
                post.PostReservationRequest(orderId, orderRowId, itemValue);
                Status = "Posting reservation request " + i;
                if (!post.HasError) continue;

                Status = "Error occur during reservation post " + i + "- Giant Tiger";
                do
                {
                    Thread.Sleep(5000);
                    post.PostReservationRequest(orderId, orderRowId, itemValue);
                } while (post.HasError);
            }
            #endregion
        }

        #region Supporting Methods
        /* a method that substring the given string */
        private static string SubstringMethod(string original, string startingString, int additionIndex)
        {
            return original.Substring(original.IndexOf(startingString, StringComparison.Ordinal) + additionIndex);
        }

        /* a method that get the next target token */
        private static string GetTarget(string text)
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
            private readonly string appRef;
            private readonly string appToken;

            /* constructor to initialize the web request of app reference and app token */
            public GetRequest(string appRef, string appToken)
            {
                this.appRef = appRef;
                this.appToken = appToken;
            }

            /* a method that return customer id from given firstname and lastname*/
            public string GetCustomerId(string firstName, string lastName, string postalCode)
            {
                #region Contact ID Get
                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlin/contact-service/contact-search?firstName=" + firstName + "&lastName=" + lastName;

                // post request to uri
                request = WebRequest.Create(uri);
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);
                request.Method = "GET";

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();

                // read all the text from JSON response
                string textJson;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    textJson = streamReader.ReadToEnd();

                // deserialize json to key value
                var info = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(textJson);

                // get the number of result
                int number = info["response"]["metaData"]["resultsAvailable"];

                // the case there is no customer exists
                if (number < 1)
                    return null;

                // start getting id
                string[] list = new string[number];
                for (int i = 0; i < number; i++)
                    list[i] = info["response"]["results"][i][0].ToString();
                #endregion

                #region Postal Code Compare
                // generate uri for getting more information on the customer id found to compare result
                uri = "https://ws-use.brightpearl.com/public-api/ashlin/contact-service/contact/";

                for (int i = 0; i < number; i++)
                    uri += list[i] + ',';

                uri = uri.Remove(uri.LastIndexOf(',')) + "?includeOptional=customFields,postalAddresses";

                request = WebRequest.Create(uri);
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);
                request.Method = "GET";

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();

                // read all the text from JSON response
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    textJson = streamReader.ReadToEnd();

                // deserialize json to key value
                info = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(textJson);

                // looping through each customer's postal code to see if the cutomer exist in these IDs
                for (int i = 0; i < number; i++)
                {
                    // get address id to get postal code
                    string address = info["response"][i]["postAddressIds"]["DEF"].ToString();

                    // get postal code
                    address = info["response"][0]["postalAddresses"][address]["postalCode"];

                    if (string.Equals(postalCode.Replace(" ", string.Empty), address.Replace(" ", string.Empty), StringComparison.CurrentCultureIgnoreCase))
                        return list[i];
                }
                #endregion

                return null;
            }

            /* a method that return product id from given sku */
            public string GetProductId(string sku)
            {
                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlin/product-service/product-search?SKU=" + sku;

                // post request to uri
                request = WebRequest.Create(uri);
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);
                request.Method = "GET";

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();

                // read all the text from JSON response
                string textJson;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    textJson = streamReader.ReadToEnd();

                // deserialize json to key value
                var info = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(textJson);

                // the case there is no product exists
                return info["response"]["metaData"]["resultsAvailable"] < 1 ? null : info["response"]["results"][0][0].ToString();
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
            private readonly string appRef;
            private readonly string appToken;

            // field for telling client if there is error occur
            public bool HasError { get; private set; }

            /* constructor to initialize the web request of app reference and app token */
            public PostRequest(string appRef, string appToken)
            {
                this.appRef = appRef;
                this.appToken = appToken;

                HasError = false;
            }

            /* post new address to API */
            public string PostAddressRequest(Address address)
            {
                const string uri = "https://ws-use.brightpearl.com/2.0.0/ashlin/contact-service/postal-address";

                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // generate the JSON file for address post
                string textJson = "{\"addressLine1\":\"" + address.Address1 + "\",\"addressLine2\":\"" + address.Address2 + "\",\"addressLine3\":\"" + address.City + "\",\"addressLine4\":\"" + address.State + "\",\"postalCode\":\"" + address.PostalCode + "\",\"countryIsoCode\":\"CAN\"}";

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJson);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(postBytes, 0, postBytes.Length);

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();
                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    result = streamReader.ReadToEnd();

                result = SubstringMethod(result, ":", 1);
                return GetTarget(result);  //return the addresss ID
            }

            /* post new customer to API */
            public string PostContactRequest(string addressId, BPvalues value)
            {
                const string uri = "https://ws-use.brightpearl.com/2.0.0/ashlin/contact-service/contact";

                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // generate JSON file for contact post
                string textJson = "{\"firstName\":\"" + value.Address.Name.Remove(value.Address.Name.IndexOf(' ')) + "\",\"lastName\":\"" + value.Address.Name.Substring(value.Address.Name.IndexOf(' ') + 1) + "\",\"postAddressIds\":{\"DEF\":" + addressId + ",\"BIL\":" + addressId + ",\"DEL\":" + addressId + "}," + 
                                  "\"communication\":{\"telephones\":{\"PRI\":\"" + value.Address.DayPhone + "\"}},\"relationshipToAccount\":{\"isSupplier\": false,\"isStaff\":false,\"isCustomer\":true},\"financialDetails\":{\"priceListId\":3}}";

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJson);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(postBytes, 0, postBytes.Length);

                // get the response from the server
                response = (HttpWebResponse)request.GetResponse();
                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    result = streamReader.ReadToEnd();

                result = SubstringMethod(result, ":", 1);
                return GetTarget(result);  //return the contact ID
            }

            /* post new order to API */
            public string PostOrderRequest(string contactId, BPvalues value)
            {
                // reset boolean flag to false 
                HasError = false;

                const string uri = "https://ws-use.brightpearl.com/2.0.0/ashlin/order-service/order";

                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // generate JSON file for order post
                string textJson = "{\"orderTypeCode\":\"SO\",\"reference\":\"" + value.Reference + "\",\"priceListId\":3,\"placeOn\":\"" + value.PlaceOn.ToString("yyyy-MM-dd") + "T00:00:00+00:00\",\"orderStatus\":{\"orderStatusId\":2}," + "\"delivery\":{\"deliveryDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(' ', 'T') + "+00:00\",\"shippingMethodId\":" + value.DeliveryId + "},\"currency\":{\"orderCurrencyCode\":\"CAD\"},\"parties\":{\"customer\":{\"contactId\":" + 
                                  contactId + "},\"delivery\":{\"addressFullName\":\"" + value.Address.Name + "\",\"addressLine1\":\"" + value.Address.Address1 + "\",\"addressLine2\":\"" + value.Address.Address2 + "\",\"addressLine3\":\"" + value.Address.City + "\",\"addressLine4\":\"" + value.Address.State + "\",\"postalCode\":\"" + value.Address.PostalCode + "\",\"countryIsoCode\":\"CAN\",\"telephone\":\"" + value.Address.DayPhone + "\"}},\"assignment\":{\"current\":{\"channelId\":" + value.ChannelId + "}}}";

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJson);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(postBytes, 0, postBytes.Length);

                // get the response from the server
                try    // might have server internal error, so do it in try and catch
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch    // HTTP response 500
                {
                    HasError = true;
                    return "Error";    // cannot post order, return error instead
                }

                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    result = streamReader.ReadToEnd();

                result = SubstringMethod(result, ":", 1);
                return GetTarget(result);  //return the order ID
            }

            /* post new order row to API */
            public string PostOrderRowRequest(string orderId, BPvalues value)
            {
                // reset boolean flag to false 
                HasError = false;

                // get product id
                string productId = new GetRequest(appRef, appToken).GetProductId(value.Sku);

                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlin/order-service/order/" + orderId + "/row";
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
                    case "QC":
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
                string textJson;
                if (productId != null)
                    textJson = "{\"productId\":\"" + productId + "\",\"quantity\":{\"magnitude\":\"" + value.Quantity + "\"},\"rowValue\":{\"taxCode\":\"" + taxCode + "\",\"rowNet\":{\"value\":\"" + Math.Round(value.RowNet, 4) + "\"},\"rowTax\":{\"value\":\"" + Math.Round(value.RowTax, 4) + "\"}}}";
                else
                    textJson = "{\"productName\":\"" + value.ProductName + " --- " + value.Sku  + "\",\"quantity\":{\"magnitude\":\"" + value.Quantity + "\"},\"rowValue\":{\"taxCode\":\"" + taxCode + "\",\"rowNet\":{\"value\":\"" + Math.Round(value.RowNet, 4) + "\"},\"rowTax\":{\"value\":\"" + Math.Round(value.RowTax, 4) + "\"}}}";


                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJson);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(postBytes, 0, postBytes.Length);

                // get the response from server
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                    HasError = true;
                    return "Error";     // 503 Server Unabailable
                }

                string result;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    result = streamReader.ReadToEnd();

                result = SubstringMethod(result, ":", 1);
                return GetTarget(result);  //return the order row ID
            }

            /* post reservation request to API return the message*/
            public void PostReservationRequest(string orderId, string orderRowId, BPvalues value)
            {
                // reset boolean flag to false 
                HasError = false;

                // get product id
                GetRequest get = new GetRequest(appRef, appToken);
                string productId = get.GetProductId(value.Sku);

                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlin/warehouse-service/order/" + orderId + "/reservation/warehouse/2";
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                // generate JSON file for order row post
                string textJson;
                if (productId != null)
                    textJson = "{\"products\": [{\"productId\": \"" + productId + "\",\"salesOrderRowId\": \"" + orderRowId + "\",\"quantity\":\"" + value.Quantity + "\"}]}";
                else
                    return;

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJson);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(postBytes, 0, postBytes.Length);

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
                            HasError = true;    // web server 503 server unavailable
                    }
                }
            }

            /* post receipt to API */
            public void PostReceipt(string orderId, string contactId, BPvalues value)
            {
                // reset boolean flag to false 
                HasError = false;

                string uri = "https://ws-use.brightpearl.com/2.0.0/ashlin/accounting-service/sales-receipt";
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("brightpearl-app-ref", appRef);
                request.Headers.Add("brightpearl-account-token", appToken);

                string textJson = "{\"orderId\":\"" + orderId + "\",\"customerId\":\"" + contactId + "\",\"received\":{\"currency\":\"CAD\",\"value\":\"" + Math.Round(value.TotalPaid, 4) + "\"},\"bankAccountNominalCode\":\"1001\",\"channelId\":" + value.ChannelId + ",\"taxDate\":\"" + value.PlaceOn.ToString("yyyy-MM-dd") + "T00:00:00+00:00\"}";

                // turn request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(textJson);

                // send request
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(postBytes, 0, postBytes.Length);

                // get response from server to see if there is error or not
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                    HasError = true;
                }
            }
        }
    }
}
