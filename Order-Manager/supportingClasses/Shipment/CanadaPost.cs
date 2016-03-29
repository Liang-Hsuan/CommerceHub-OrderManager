using Order_Manager.channel.shop.ca;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;

namespace Order_Manager.supportingClasses.Shipment
{
    public class CanadaPost : Shipment
    {
        // fields for credentials
        private readonly string USER_ID;
        private readonly string PASSWORD;
        private readonly string CUSTOMER_NUMBER;

        // field for save image path
        private readonly string savePathShopCa = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ShopCa_ShippingLabel";

        /* constructor that initialize Canada Post credentials */
        public CanadaPost()
        {
            // get credentials from database
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ASCMcs))
            {
                SqlCommand command = new SqlCommand("SELECT Field1_Value, Field2_Value, Field3_Value FROM ASCM_Credentials WHERE Source = 'Canada Post' and Client = 'ASHLIN-BPG MARKETING Inc.'", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                // allocate data
                USER_ID = reader.GetString(0);
                PASSWORD = reader.GetString(1);
                CUSTOMER_NUMBER = reader.GetString(2);
            }
        }

        #region Posting Methods
        /* a method that post shipment confirm request and return [0] tracking pin, [1] refund link, [2] label link */
        public string[] postShipmentConfirm(ShopCaValues value, Package package)
        {
            // set error to false
            Error = false;

            string shipmentConfirmUri = "https://ct.soa-gw.canadapost.ca/rs/" + CUSTOMER_NUMBER + "/ncshipment";
            // string shipmentConfirmUri = "https://soa-gw.canadapost.ca/rs/" + CUSTOMER_NUMBER + "/ncshipment";

            // create http post request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(shipmentConfirmUri);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(USER_ID + ":" + PASSWORD)));
            request.Method = "POST";
            request.ContentType = "application/vnd.cpc.ncshipment-v4+xml";
            request.Accept = "application/vnd.cpc.ncshipment-v4+xml";

            string textXML =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<non-contract-shipment xmlns=\"http://www.canadapost.ca/ws/ncshipment-v4\">" +
                "<delivery-spec>";
            string serviceCode;
            switch (package.Service)
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
            textXML +=
                "<service-code>" + serviceCode + "</service-code>" +
                "<sender>" +
                "<company>Ashlin BPG Marketing</company>" +
                "<contact-phone>9058553027</contact-phone>" +
                "<address-details>" +
                "<address-line-1>2351 Royal Windsor Dr</address-line-1>" +
                "<city>Mississauga</city>" +
                "<prov-state>ON</prov-state>" +
                "<postal-zip-code>L5J4S7</postal-zip-code>" +
                "</address-details>" +
                "</sender>" +
                "<destination>" +
                "<name>" + value.ShipTo.Name + "</name>" +
                "<address-details>" +
                "<address-line-1>" + value.ShipTo.Address1 + "</address-line-1>";
            if (value.ShipTo.Address2 != "")
                textXML += "<address-line-2>" + value.ShipTo.Address2 + "</address-line-2>";
            textXML +=
                "<city>" + value.ShipTo.City + "</city>" +
                "<prov-state>" + value.ShipTo.State + "</prov-state>" +
                "<country-code>CA</country-code>" +
                "<postal-zip-code>" + value.ShipTo.PostalCode + "</postal-zip-code>" +
                "</address-details>" +
                "</destination>" +
                "<options>" +
                "<option>" +
                "<option-code>SO</option-code>" +
                "</option>" +
                "</options>" +
                "<parcel-characteristics>" +
                "<weight>" + Math.Round(package.Weight, 3) + "</weight>" +
                "<dimensions>" +
                "<length>" + Math.Round(package.Length, 2) + "</length>" +
                "<width>" + Math.Round(package.Width, 2) + "</width>" +
                "<height>" + Math.Round(package.Height, 2) + "</height>" +
                "</dimensions>" +
                "</parcel-characteristics>" +
                "<preferences>" +
                "<show-packing-instructions>true</show-packing-instructions>" +
                "</preferences>" +
                "</delivery-spec>" +
                "</non-contract-shipment>";

            // turn request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(textXML);

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

            // tracking pin
            result = substringMethod(result, "<tracking-pin>", 14);
            returnString[0] = getTarget(result);

            // refund link
            result = substringMethod(result, "\"refund\"", 8);
            result = substringMethod(result, "href=", 6);
            returnString[1] = getTarget(result);

            // label link
            result = substringMethod(result, "\"label\"", 7);
            result = substringMethod(result, "href=", 6);
            returnString[2] = getTarget(result);

            return returnString;
        }

        /* a method that get artifect in pdf binary format */
        public byte[] getAritifect(string LabelLink)
        {
            // post request to uri
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(LabelLink);
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

        /* a method that post shipment shipment refund request */
        public void postShipmentVoid(string refundLink)
        {
            // set error to false
            Error = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(refundLink);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(USER_ID + ":" + PASSWORD)));
            request.Method = "POST";
            request.ContentType = "application/vnd.cpc.ncshipment-v4+xml";
            request.Accept = "application/vnd.cpc.ncshipment-v4+xml";

            const string textXML = "<non-contract-shipment-refund-request xmlns=\"http://www.canadapost.ca/ws/ncshipment-v4\">" +
                                   "<email>intern1002@ashlinbpg.com</email>" +
                                   "</non-contract-shipment-refund-request>";

            // turn request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(textXML);

            // send request
            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

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
        #endregion

        /* a method that turn binary string into pdf file */
        public void exportLabel(byte[] binary, string orderId, bool preview)
        {
            // create path
            string file = savePathShopCa + "\\" + orderId + ".pdf";

            // check if the save directory exist -> if not create it
            if (!File.Exists(savePathShopCa))
                Directory.CreateDirectory(savePathShopCa);

            // save pdf
            File.WriteAllBytes(file, binary);

            // show the pdf if user want to preview
            if (preview)
            {
                if (System.Diagnostics.Process.GetProcessesByName(file).Length < 1)
                    System.Diagnostics.Process.Start(file);
            }
        }

        /* a Get for savepath for shipment label */
        public string SavePathShopCa => savePathShopCa;
    }
}
