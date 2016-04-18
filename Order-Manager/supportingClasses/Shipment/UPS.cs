using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using Order_Manager.channel.sears;
using System.Xml;

namespace Order_Manager.supportingClasses.Shipment
{
    /* 
     * A class that post shipment to UPS
     */
    public class Ups : Shipment
    {
        // field for credentials
        private readonly string ACCESS_LISCENSE_NUMBER;
        private readonly string USER_ID;
        private readonly string PASSWORD;
        private readonly string ACCOUNT_NUMBER;
        private readonly string SEARS_ACCOUNT_NUMBER;

        /* a Get for savepath for shipment label */
        public string SavePath { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\UPS_ShippingLabel";

        /* constructor that initialize UPS credentials */
        public Ups()
        {
            // get credentials from database
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ASCMcs))
            {
                SqlCommand command = new SqlCommand("SELECT Username, [Password], Field1_Value, Field2_Value, Field3_Value FROM ASCM_Credentials WHERE Source = 'UPS'", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                // allocate data
                USER_ID = reader.GetString(0);
                PASSWORD = reader.GetString(1);
                ACCOUNT_NUMBER = reader.GetString(2);
                SEARS_ACCOUNT_NUMBER = reader.GetString(3);
                ACCESS_LISCENSE_NUMBER = reader.GetString(4);
            }
        }

        #region Posting Methods
        /* a method that post shipment confirm request and return shipment digest and identification number*/
        public string[] PostShipmentConfirm(SearsValues value)
        {
            // set error to false
            Error = false;

            // const string shipmentConfirmUri = "https://wwwcie.ups.com/ups.app/xml/ShipConfirm";
            const string shipmentConfirmUri = "https://onlinetools.ups.com/ups.app/xml/ShipConfirm";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(shipmentConfirmUri);
            request.Method = "POST";
            request.ContentType = "application/xml";

            string textXml =
                "<?xml version=\"1.0\"?>" +
                "<AccessRequest xml:lang=\"en-US\">" +
                "<AccessLicenseNumber>" + ACCESS_LISCENSE_NUMBER + "</AccessLicenseNumber>" +
                "<UserId>" + USER_ID + "</UserId>" +
                "<Password>" + PASSWORD + "</Password>" +
                "</AccessRequest>" +
                "<?xml version=\"1.0\"?>" +
                "<ShipmentConfirmRequest xml:lang=\"en-U\">" +
                "<Request>" +
                "<RequestAction>ShipConfirm</RequestAction>" +
                "<RequestOption>validate</RequestOption>" +
                "</Request>" +
                "<Shipment>" +
                "<Shipper>" +
                "<Name>Ashlin BPG Marketing Inc</Name>" +
                "<PhoneNumber>9058553027</PhoneNumber>" +
                "<ShipperNumber>" + ACCOUNT_NUMBER + "</ShipperNumber>" +
                "<Address>" +
                "<AddressLine1>2351 Royal Windsor Dr</AddressLine1>" +
                "<City>Mississauga</City>" +
                "<StateProvinceCode>ON</StateProvinceCode>" +
                "<PostalCode>L5J4S7</PostalCode>" +
                "<CountryCode>CA</CountryCode>" +
                "</Address>" +
                "</Shipper>" +
                "<ShipTo>" +
                "<CompanyName>" + value.Recipient.Name + "</CompanyName>" +
                "<PhoneNumber>" + value.ShipTo.DayPhone + "</PhoneNumber>" +
                "<Address>" +
                "<AddressLine1>" + value.ShipTo.Address1 + "</AddressLine1>";
            if (value.ShipTo.Address2 != "")
                textXml += "<AddressLine2>" + value.ShipTo.Address2 + "</AddressLine2>";
            textXml +=
            "<City>" + value.ShipTo.City + "</City>" +
            "<StateProvinceCode>" + value.ShipTo.State + "</StateProvinceCode>" +
            "<PostalCode>" + value.ShipTo.PostalCode + "</PostalCode>" +
            "<CountryCode>CA</CountryCode>" +
            "</Address>" +
            "</ShipTo>" +
            "<PaymentInformation>" +
            "<BillThirdParty>" +
            "<BillThirdPartyShipper>" +
            "<AccountNumber>" + SEARS_ACCOUNT_NUMBER + "</AccountNumber>" +
            "<ThirdParty>" +
            "<Address>" +
            "<PostalCode>L5J4S7</PostalCode>" +
            "<CountryCode>CA</CountryCode>" +
            "</Address>" +
            "</ThirdParty>" +
            "</BillThirdPartyShipper>" +
            "</BillThirdParty>" +
            "</PaymentInformation>" +
            "<Service>";

            string code;
            switch (value.Package.Service)
            {
                case "UPS Standard":
                    code = "11";
                    break;
                case "UPS 3 Day Select":
                    code = "12";
                    break;
                case "UPS Worldwide Express":
                    code = "07";
                    break;
                default:
                    code = "01";
                    break;
            }

            textXml +=
            "<Code>" + code + "</Code>" +
            "</Service>" +
            "<Package>";

            switch (value.Package.PackageType)
            {
                case "Letter":
                    code = "01";
                    break;
                case "Express Box":
                    code = "21";
                    break;
                case "First Class":
                    code = "59";
                    break;
                default:
                    code = "02";
                    break;
            }

            textXml +=
            "<PackagingType>" +
            "<Code>" + code + "</Code>" +
            "</PackagingType>" +
            "<Dimensions>" +
            "<UnitOfMeasurement>" +
            "<Code>CM</Code>" +
            "</UnitOfMeasurement>" +
            "<Length>" + value.Package.Length + "</Length>" +
            "<Width>" + value.Package.Width + "</Width>" +
            "<Height>" + value.Package.Height + "</Height>" +
            "</Dimensions>" +
            "<PackageWeight>" +
            "<UnitOfMeasurement>" +
            "<Code>KGS</Code>" +
            "</UnitOfMeasurement>" +
            "<Weight>" + value.Package.Weight + "</Weight>" +
            "</PackageWeight>" +
            "</Package>" +
            "</Shipment>" +
            "<LabelSpecification>" +
            "<LabelPrintMethod>" +
            "<Code>GIF</Code>" +
            "</LabelPrintMethod>" +
            "<LabelImageFormat>" +
            "<Code>GIF</Code>" +
            "</LabelImageFormat>" +
            "</LabelSpecification>" +
            "</ShipmentConfirmRequest>";

            // turn request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(textXml);

            // send request
            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

            // get the response from the server
            HttpWebResponse response;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                // the case if server error -> set error indication
                Error = true;
                ErrorMessage = "Server Internal Error";
                return null;
            }

            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // read xml
            doc.LoadXml(result);

            // get status code
            XmlNode node = doc.SelectSingleNode("/ShipmentConfirmResponse/Response");
            string responseStatus = node["ResponseStatusCode"].InnerText;

            // get identification number and shipment digest
            string[] returnString = new string[2];
            if (responseStatus == "1")
            {
                node = doc.SelectSingleNode("/ShipmentConfirmResponse");

                returnString[0] = node["ShipmentIdentificationNumber"].InnerText;
                returnString[1] = node["ShipmentDigest"].InnerText;
            }
            else
            {
                // the case if bad request -> set error indication
                Error = true;
                ErrorMessage = "Error: " + node["Error"]["ErrorDescription"].InnerText;
                return null;
            }

            return returnString;
        }

        /* a method that post shipment accept request and return base64 image string and tracking number*/
        public string[] PostShipmentAccept(string shipmentDigest)
        {
            // set error to false
            Error = false;

            // const string shipmentAcceptmUri = "https://wwwcie.ups.com/ups.app/xml/ShipAccept";
            const string shipmentAcceptmUri = "https://onlinetools.ups.com/ups.app/xml/ShipAccept";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(shipmentAcceptmUri);
            request.Method = "POST";
            request.ContentType = "application/xml";

            string textXml =
               "<?xml version=\"1.0\"?>" +
               "<AccessRequest xml:lang=\"en-US\">" +
               "<AccessLicenseNumber>" + ACCESS_LISCENSE_NUMBER + "</AccessLicenseNumber>" +
               "<UserId>" + USER_ID + "</UserId>" +
               "<Password>" + PASSWORD + "</Password>" +
               "</AccessRequest>" +
               "<?xml version=\"1.0\"?>" +
               "<ShipmentAcceptRequest>" +
               "<Request>" +
               "<RequestAction>ShipAccept</RequestAction>" +
               "</Request>" +
               "<ShipmentDigest>" + shipmentDigest +
               "</ShipmentDigest></ShipmentAcceptRequest>";

            // turn request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(textXml);

            // send request
            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

            // get the response from the server
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // read xml
            doc.LoadXml(result);

            // get status code
            XmlNode node = doc.SelectSingleNode("/ShipmentAcceptResponse/Response");
            string responseStatus = node["ResponseStatusCode"].InnerText;

            // get tracking number and image
            string[] text = new string[2];
            if (responseStatus == "1")
            {
                node = doc.SelectSingleNode("/ShipmentAcceptResponse/ShipmentResults/PackageResults");

                text[0] = node["TrackingNumber"].InnerText;
                text[1] = node["LabelImage"]["GraphicImage"].InnerText;
            }
            else
            {
                // the case if server error -> set error indication
                Error = true;
                ErrorMessage = "Error: " + node["Error"]["ErrorDescription"].InnerText;
                return null;
            }

            return text;
        }

        /* a method that post shipment void request and check if the void request is success */
        public void PostShipmentVoid(string identificationNumber)
        {
            // set error to false
            Error = false;

            // const string shipmentVoidUri = "https://wwwcie.ups.com/ups.app/xml/Void";
            const string shipmentVoidUri = "https://onlinetools.ups.com/ups.app/xml/Void";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(shipmentVoidUri);
            request.Method = "POST";
            request.ContentType = "application/xml";

            string textXml =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<AccessRequest xml:lang=\"en-US\">" +
                "<AccessLicenseNumber>" + ACCESS_LISCENSE_NUMBER + "</AccessLicenseNumber>" +
                "<UserId>" + USER_ID + "</UserId>" +
                "<Password>" + PASSWORD + "</Password>" +
                "</AccessRequest>" +
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<VoidShipmentRequest>" +
                "<Request>" +
                "<RequestAction>1</RequestAction>" +
                "</Request>" +
                "<ShipmentIdentificationNumber>" + identificationNumber + "</ShipmentIdentificationNumber>" +
                "</VoidShipmentRequest>";

            // turn request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(textXml);

            // send request
            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

            // get the response from the server
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // read xml
            doc.LoadXml(result);

            // get status code
            XmlNode node = doc.SelectSingleNode("/VoidShipmentResponse/Response");
            string responseStatus = node["ResponseStatusCode"].InnerText;

            if (responseStatus == "1") return;

            // the case is bad request -> set error indication
            Error = true;
            ErrorMessage = "Error: " + node["Error"]["ErrorDescription"].InnerText;
        }
        #endregion

        /* a method that turn base64 string into GIF format image */
        public void ExportLabel(string base64String, string transactionId, bool preview)
        { 
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);

            // save image
            // check if the save directory exist -> if not create it
            if (!File.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            // save the image
            string file = SavePath + "\\" + transactionId + ".gif";
            image.Save(file, System.Drawing.Imaging.ImageFormat.Gif);

            // show the image if user want to preview
            if (!preview) return;
            if (System.Diagnostics.Process.GetProcessesByName(file).Length < 1)
                System.Diagnostics.Process.Start(file);
        }
    }
}
