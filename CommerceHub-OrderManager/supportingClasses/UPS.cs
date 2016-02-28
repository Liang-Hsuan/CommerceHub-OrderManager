using CommerceHub_OrderManager.channel.sears;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;

namespace CommerceHub_OrderManager.supportingClasses
{
    public class UPS
    {
        // field for credentials
        private const string ACCESS_LISCENSE_NUMBER = "0D03B5751F524086";
        private const string USER_ID = "leonmaandbee";
        private const string PASSWORD = "Whatthefuck630";
        private const string ACCOUNT_NUMBER = "15XR35";
        private const string SEARS_ACCOUNT_NUMBER = "A9725A";

        /* a constructor that do nothing */
        public UPS()
        {
            // just in case
        }

        #region Posting Methods
        /* a method that post shipment confirm request and return shipment digest */
        public string postShipmentConfirm(SearsValues value)
        {
            string shipmentConfirmUri = "https://wwwcie.ups.com/ups.app/xml/ShipConfirm";

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
                "<CompanyName>Sears</CompanyName>" +
                "<PhoneNumber>" + value.Recipient.DayPhone + "</PhoneNumber>" +
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
            "<Service>" +
            "<Code>01</Code>" +
            "</Service>" +
            "<Package>" +
            "<PackagingType>" +
            "<Code>02</Code>" +
            "</PackagingType>" +
            "<Dimensions>" +
            "<UnitOfMeasurement>" +
            "<Code>CM</Code>" +
            "</UnitOfMeasurement>" +
            "<Length>10</Length>" +
            "<Width>10</Width>" +
            "<Height>10</Height>" +
            "</Dimensions>" +
            "<PackageWeight>" +
            "<UnitOfMeasurement>" +
            "<Code>KGS</Code>" +
            "</UnitOfMeasurement>" +
            "<Weight>1</Weight>" +
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
            {
                requestStream.Write(postBytes, 0, postBytes.Length);
            }

            // get the response from the server
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            // get the response stattus
            result = substringMethod(result, "ResponseStatusCode", 19);
            string responseStatus = getTarget(result);

            // get shipment identification number and shipment digest
            string identificationNumber = "";
            string digest = "";
            if (responseStatus == "1")
            {
                result = substringMethod(result, "ShipmentIdentificationNumber", 29);
                identificationNumber = getTarget(result);

                result = substringMethod(result, "ShipmentDigest", 15);
                digest = getTarget(result);
            }
            else
            {
                return "Error";
            }

            return digest;
        }

        /* a method that post shipment accept request and return base64 image string */
        public string postShipmentAccept(string shipmentDigest)
        {
            string shipmentAcceptmUri = "https://wwwcie.ups.com/ups.app/xml/ShipAccept";

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
            {
                requestStream.Write(postBytes, 0, postBytes.Length);
            }

            // get the response from the server
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            // get the response stattus
            result = substringMethod(result, "ResponseStatusCode", 19);
            string responseStatus = getTarget(result);

            // get tracking number and image
            string trackingNumber = "";
            string image = "";
            if (responseStatus == "1")
            {
                result = substringMethod(result, "TrackingNumber", 15);
                trackingNumber = getTarget(result);

                result = substringMethod(result, "GraphicImage", 13);
                image = getTarget(result);
            }
            else
            {
                return "Error";
            }

            return image;
        }
        #endregion

        /* a method that turn base64 string into GIF format image */
        public void exportLabel(string base64String, string transactionId)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);

            // save image
            try
            {
                // check if the save directory exist -> if not create it
                if (!File.Exists(@"C:\Users\Intern1001\Desktop\Sears_ShippingLabel"))
                    Directory.CreateDirectory(@"C:\Users\Intern1001\Desktop\Sears_ShippingLabel");

                image.Save(@"C:\Users\Intern1001\Desktop\Sears_ShippingLabel\" + transactionId + ".gif", System.Drawing.Imaging.ImageFormat.Gif);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #region Supporting Methods
        /* a method that substring the given string */
        private string substringMethod(string original, string startingString, int additionIndex)
        {
            string copy = original;
            copy = original.Substring(original.IndexOf(startingString) + additionIndex);

            return copy;
        }

        /* a method that get the next target token */
        private string getTarget(string text)
        {
            int i = 0;
            while (text[i] != '<' && text[i] != '>' && text[i] != '"')
            {
                i++;
            }

            return text.Substring(0, i);
        }
        #endregion
    }
}
