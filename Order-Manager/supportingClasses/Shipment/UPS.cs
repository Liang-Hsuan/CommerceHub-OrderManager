using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using Order_Manager.channel.sears;

namespace Order_Manager.supportingClasses.Shipment
{
    /* 
     * A class that post shipment to UPS
     */
    public class UPS : Shipment
    {
        // field for credentials
        private readonly string ACCESS_LISCENSE_NUMBER;
        private readonly string USER_ID;
        private readonly string PASSWORD;
        private readonly string ACCOUNT_NUMBER;
        private readonly string SEARS_ACCOUNT_NUMBER;

        /* a Get for savepath for shipment label */
        public string SavePathSears { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Sears_ShippingLabel";

        /* constructor that initialize UPS credentials */
        public UPS()
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
        public string[] postShipmentConfirm(SearsValues value)
        {
            // set error to false
            Error = false;

            // const string shipmentConfirmUri = "https://wwwcie.ups.com/ups.app/xml/ShipConfirm";
            const string shipmentConfirmUri = "https://onlinetools.ups.com/ups.app/xml/ShipConfirm";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(shipmentConfirmUri);
            request.Method = "POST";
            request.ContentType = "application/xml";

            string textXML =
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
                textXML += "<AddressLine2>" + value.ShipTo.Address2 + "</AddressLine2>";
            textXML +=
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

            textXML +=
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

            textXML +=
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
            byte[] postBytes = Encoding.UTF8.GetBytes(textXML);

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

            // get the response stattus
            result = substringMethod(result, "ResponseStatusCode", 19);
            string responseStatus = getTarget(result);

            // get identification number and shipment digest
            string[] returnString = new string[2];
            if (responseStatus == "1")
            {
                result = substringMethod(result, "ShipmentIdentificationNumber", 29);
                returnString[0] = getTarget(result);

                result = substringMethod(result, "ShipmentDigest", 15);
                returnString[1] = getTarget(result);
            }
            else
            {
                // the case if bad request -> set error indication
                Error = true;
                ErrorMessage = "Error: " + getTarget(substringMethod(result, "ErrorDescription", 17));
                return null;
            }

            return returnString;
        }

        /* a method that post shipment accept request and return base64 image string and tracking number*/
        public string[] postShipmentAccept(string shipmentDigest)
        {
            // set error to false
            Error = false;

            // const string shipmentAcceptmUri = "https://wwwcie.ups.com/ups.app/xml/ShipAccept";
            const string shipmentAcceptmUri = "https://onlinetools.ups.com/ups.app/xml/ShipAccept";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(shipmentAcceptmUri);
            request.Method = "POST";
            request.ContentType = "application/xml";

            string textXML =
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
            byte[] postBytes = Encoding.UTF8.GetBytes(textXML);

            // send request
            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

            // get the response from the server
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // get the response stattus
            result = substringMethod(result, "ResponseStatusCode", 19);
            string responseStatus = getTarget(result);

            // get tracking number and image
            string[] text = new string[2];
            if (responseStatus == "1")
            {
                result = substringMethod(result, "TrackingNumber", 15);
                text[0] = getTarget(result);

                result = substringMethod(result, "GraphicImage", 13);
                text[1] = getTarget(result);
            }
            else
            {
                // the case if server error -> set error indication
                Error = true;
                ErrorMessage = "Server Internal Error";
                return null;
            }

            return text;
        }

        /* a method that post shipment void request and check if the void request is success */
        public void postShipmentVoid(string identificationNumber)
        {
            // set error to false
            Error = false;

            // const string shipmentVoidUri = "https://wwwcie.ups.com/ups.app/xml/Void";
            const string shipmentVoidUri = "https://onlinetools.ups.com/ups.app/xml/Void";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(shipmentVoidUri);
            request.Method = "POST";
            request.ContentType = "application/xml";

            string textXML =
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
            byte[] postBytes = Encoding.UTF8.GetBytes(textXML);

            // send request
            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

            // get the response from the server
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // get the response stattus
            result = substringMethod(result, "ResponseStatusCode", 19);
            string responseStatus = getTarget(result);

            // the case is bad request -> set error indication
            if (responseStatus == "1") return;
            Error = true;
            ErrorMessage =  "Error: " + getTarget(substringMethod(result, "ErrorDescription", 17));
        }
        #endregion

        /* a method that turn base64 string into GIF format image */
        public void exportLabel(string base64String, string transactionId, bool preview)
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
            if (!File.Exists(SavePathSears))
                Directory.CreateDirectory(SavePathSears);

            // save the image
            string file = SavePathSears + "\\" + transactionId + ".gif";
            image.Save(file, System.Drawing.Imaging.ImageFormat.Gif);

            // show the image if user want to preview
            if (preview)
            {
                if (System.Diagnostics.Process.GetProcessesByName(file).Length < 1)
                    System.Diagnostics.Process.Start(file);
            }
        }
    }
}
