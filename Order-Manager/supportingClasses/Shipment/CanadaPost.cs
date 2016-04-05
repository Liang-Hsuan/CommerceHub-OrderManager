using Order_Manager.channel.shop.ca;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace Order_Manager.supportingClasses.Shipment
{
    public class CanadaPost : Shipment
    {
        // fields for credentials
        private readonly string USER_ID;
        private readonly string PASSWORD;
        private readonly string CUSTOMER_NUMBER;
        private readonly string CONTRACT_NUMBER;

        /* Get for save path for shipment label and manifest */
        public string SavePathLabelShopCa { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ShopCa_ShippingLabel";
        public string SavePathManifestShopCa { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ShopCa_ShippingManifest";

        /* constructor that initialize Canada Post credentials */
        public CanadaPost()
        {
            // get credentials from database
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ASCMcs))
            {
                SqlCommand command = new SqlCommand("SELECT Field1_Value, Field2_Value, Field3_Value, Field4_Value FROM ASCM_Credentials WHERE Source = 'Canada Post' and Client = 'ASHLIN-BPG MARKETING Inc.'", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                // allocate data
                USER_ID = reader.GetString(0);
                PASSWORD = reader.GetString(1);
                CUSTOMER_NUMBER = reader.GetString(2);
                CONTRACT_NUMBER = reader.GetString(3);
            }
        }

        #region API Methods
        /* a method that create shipment and return [0] tracking pin, [1] self link, [2] label link */
        public string[] createShipment(ShopCaValues value)
        {
            // set error to false
            Error = false;

            // string uri = "https://ct.soa-gw.canadapost.ca/rs/" + CUSTOMER_NUMBER + '/' + CUSTOMER_NUMBER + "/shipment";
            string uri = "https://soa-gw.canadapost.ca/rs/" + CUSTOMER_NUMBER + '/' + CUSTOMER_NUMBER + "/shipment";

            // create http post request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(USER_ID + ":" + PASSWORD)));
            request.Method = "POST";
            request.ContentType = "application/vnd.cpc.shipment-v8+xml";
            request.Accept = "application/vnd.cpc.shipment-v8+xml";

            string textXml =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<shipment xmlns=\"http://www.canadapost.ca/ws/shipment-v8\">" +
                "<group-id>" + DateTime.Today.ToString("yyyyMMdd") + "</group-id>" +
                "<requested-shipping-point>L5J4S7</requested-shipping-point>" +
                "<delivery-spec>";
            string serviceCode;
            switch (value.Package.Service)
            {
                case "Regular Parcel":
                    serviceCode = "DOM.RP";
                    break;
                case "Xpresspost":
                    serviceCode = "DOM.XP";
                    break;
                case "Priority":
                    serviceCode = "DOM.PC";
                    break;
                default:
                    serviceCode = "DOM.EP";
                    break;
            }
            textXml +=
                 "<service-code>" + serviceCode + "</service-code>" +
                 "<sender>" +
                 "<company>Ashlin BPG Marketing</company>" +
                 "<contact-phone>(905) 855-3027</contact-phone>" +
                 "<address-details>" +
                 "<address-line-1>2351 Royal Windsor Dr</address-line-1>" +
                 "<city>Mississauga</city>" +
                 "<prov-state>ON</prov-state>" +
                 "<country-code>CA</country-code>" +
                 "<postal-zip-code>L5J4S7</postal-zip-code>" +
                 "</address-details>" +
                 "</sender>" +
                 "<destination>" +
                 "<name>" + value.ShipTo.Name + "</name>" +
                 "<address-details>" +
                 "<address-line-1>" + value.ShipTo.Address1 + "</address-line-1>";
            if (value.ShipTo.Address2 != "")
                textXml += "<address-line-2>" + value.ShipTo.Address2 + "</address-line-2>";
            textXml +=
                 "<city>" + value.ShipTo.City + "</city>" +
                 "<prov-state>" + value.ShipTo.State + "</prov-state>" +
                 "<country-code>CA</country-code>" +
                 "<postal-zip-code>" + value.ShipTo.PostalCode + "</postal-zip-code>" +
                 "</address-details>" +
                 "</destination>" +
                 "<parcel-characteristics>" +
                 "<weight>" + Math.Round(value.Package.Weight, 4) + "</weight>" +
                 "<dimensions>" +
                 "<length>" + value.Package.Length + "</length>" +
                 "<width>" + value.Package.Width + "</width>" +
                 "<height>" + value.Package.Height + "</height>" +
                 "</dimensions>" +
                 "</parcel-characteristics>" +
                 "<print-preferences>" +
                 "<output-format>4x6</output-format>" +
                 "</print-preferences>" +
                 "<preferences>" +
                 "<show-packing-instructions>true</show-packing-instructions>" +
                 "</preferences>" +
                 "<settlement-info>" +
                 "<contract-id>" + CONTRACT_NUMBER + "</contract-id>" +
                 "<intended-method-of-payment>Account</intended-method-of-payment>" +
                 "</settlement-info>" +
                 "</delivery-spec>" +
                 "</shipment>";


            // turn request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(textXml);

            // send request
            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

            // get the response from the server
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                // set error to true
                Error = true;

                // the case if it is a bad request -> return the error message
                using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                    ErrorMessage = reader.ReadToEnd();

                return null;
            }

            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // get tracking pin, refund link and label link
            string[] returnString = new string[3];
            doc.LoadXml(result);

            // get namespace
            var xmlns = doc.DocumentElement.Attributes["xmlns"];
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("ns", xmlns.Value);

            // getting tracking pin
            XmlNode node = doc.SelectSingleNode("/ns:shipment-info", namespaceManager);
            returnString[0] = node["tracking-pin"].InnerText;

            // self and label links
            node = doc.SelectSingleNode("/ns:shipment-info/ns:links", namespaceManager);
            foreach (XmlNode child in node)
            {
                if (child.Attributes["rel"].Value == "self")
                    returnString[1] = child.Attributes["href"].Value;
                else if (child.Attributes["rel"].Value == "label")
                    returnString[2] = child.Attributes["href"].Value;
            }

            return returnString;
        }

        /* a method that get artifect in pdf binary format */
        public byte[] getArtifact(string labelLink)
        {
            // post request to uri
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(labelLink);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(USER_ID + ":" + PASSWORD)));
            request.Method = "GET";
            request.Accept = "application/pdf";

            // get the response from the server
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // read all the byte from response 
            MemoryStream memoryStream = new MemoryStream();
            response.GetResponseStream().CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        /* a method that delete shipment */
        public void deleteShipment(string selfLink)
        {
            // set error to false
            Error = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(selfLink);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(USER_ID + ":" + PASSWORD)));
            request.Method = "DELETE";

            // get the response from the server
            try
            {
                request.GetResponse();
            }
            catch (WebException wex)
            {
                // set error to true
                Error = true;

                // the case if anything wrong
                using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                    ErrorMessage = reader.ReadToEnd();
            }
        }

        /* a method that post transmit shipment */
        public string[] transmitShipments(string groupId)
        {
            // set error to false
            Error = false;

            // string uri = "https://ct.soa-gw.canadapost.ca/rs/" + CUSTOMER_NUMBER + '/' + CUSTOMER_NUMBER + "/manifest";
            string uri = "https://soa-gw.canadapost.ca/rs/" + CUSTOMER_NUMBER + '/' + CUSTOMER_NUMBER + "/manifest";

            // create http post request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(USER_ID + ":" + PASSWORD)));
            request.Method = "POST";
            request.ContentType = "application/vnd.cpc.manifest-v8+xml";
            request.Accept = "application/vnd.cpc.manifest-v8+xml";

            string textXml =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<transmit-set xmlns=\"http://www.canadapost.ca/ws/manifest-v8\">" +
                "<group-ids>" +
                "<group-id>" + groupId + "</group-id>" +
                "</group-ids>" +
                "<detailed-manifests>true</detailed-manifests>" +
                "<method-of-payment>Account</method-of-payment>" +
                "<manifest-address>" +
                "<manifest-company>Ashlin BPG Marketing</manifest-company>" +
                "<phone-number>(905) 855-3027</phone-number>" +
                "<address-details>" +
                "<address-line-1>2351 Royal Windsor Dr</address-line-1>" +
                "<city>Mississauga</city>" +
                "<prov-state>ON</prov-state>" +
                "<postal-zip-code>L5J4S7</postal-zip-code>" +
                "</address-details>" +
                "</manifest-address>" +
                "</transmit-set>";

            // turn request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(textXml);

            // send request
            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

            // get the response from the server
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                // set error to true
                Error = true;

                // the case if it is a bad request -> return the error message
                using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                    ErrorMessage = reader.ReadToEnd();

                return null;
            }

            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // load xml
            doc.Load(result);

            // get namespace
            var xmlns = doc.DocumentElement.Attributes["xmlns"];
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("ns", xmlns.Value);

            // get all links
            XmlNode node = doc.SelectSingleNode("/ns:manifests", namespaceManager);

            return (from XmlNode child in node select child.Attributes["href"].Value).ToArray();
        }

        /* a method that get artifact link for the given manifest link */
        public string getManifest(string manifestLink)
        {
            // set error to false
            Error = false;

            // post request to uri
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(manifestLink);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(USER_ID + ":" + PASSWORD)));
            request.Method = "GET";
            request.Accept = "application/vnd.cpc.manifest-v8+xml";

            // get the response from the server
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                // set error to true
                Error = true;

                // the case if it is a bad request -> return the error message
                using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                    ErrorMessage = reader.ReadToEnd();

                return null;
            }

            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // load xml
            doc.LoadXml(result);

            // get namespace
            var xmlns = doc.DocumentElement.Attributes["xmlns"];
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("ns", xmlns.Value);

            // get all links
            XmlNode node = doc.SelectSingleNode("/ns:manifest/ns:links", namespaceManager);

            return node.Cast<XmlNode>().Where(child => child.Attributes["rel"].Value == "artifact").Select(child => child.Attributes["href"].Value).FirstOrDefault();
        }
        #endregion

        /* a method that turn binary string into pdf file */
        public void exportLabel(byte[] binary, string orderId, bool label, bool preview)
        {
            // create path
            string file;
            if (label)
            {
                // label case
                file = SavePathLabelShopCa + "\\" + orderId + ".pdf";

                // check if the save directory exist -> if not create it
                if (!File.Exists(SavePathLabelShopCa))
                    Directory.CreateDirectory(SavePathLabelShopCa);
            }
            else
            {
                // manifest case
                file = SavePathManifestShopCa + "\\" + orderId + ".pdf";

                // check if the save directory exist -> if not create it
                if (!File.Exists(SavePathManifestShopCa))
                    Directory.CreateDirectory(SavePathManifestShopCa);
            }

            // save pdf
            File.WriteAllBytes(file, binary);

            // show the pdf if user want to preview
            if (preview)
            {
                if (System.Diagnostics.Process.GetProcessesByName(file).Length < 1)
                    System.Diagnostics.Process.Start(file);
            }
        }
    }
}
