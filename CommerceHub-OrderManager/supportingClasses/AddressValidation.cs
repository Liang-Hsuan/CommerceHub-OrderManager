using System.IO;
using System.Linq;
using System.Net;

namespace CommerceHub_OrderManager.supportingClasses
{
    /*
     * A class that validate the given address
     */
    public class AddressValidation
    {
        // local fields for web request
        private WebRequest request;
        private HttpWebResponse response;

        /* the main method of the object that return if the address is valid or not */
        public bool validate(Address address)
        {
            // generate uri
            string uri = "https://maps.googleapis.com/maps/api/geocode/xml?address=";

            uri += replaceSpace(address.Address1) + ",";
            uri += replaceSpace(address.City) + ",";
            uri += replaceSpace(address.State) + ",";
            uri += replaceSpace(address.PostalCode);

            uri += "&key=AIzaSyASdOsY2T3vBfYn1lBE5VQl7nZ-ivp1vKs";
            
            // post request to web server
            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";

            // get the response from the server
            response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            // counting the number of result -> the count must be 1 in order to be valid
            int count = 0;
            while (result.Contains("<result>"))
            {
                count++;
                result = substringMethod(result, "</result>", 8);
            }

            // the case if the total results more than 1 -> invalid address
            if (count != 1)
                return false;

            return true;
        }

        #region Supporting Methods
        /* a method that replace space with '+' character */
        private static string replaceSpace(string original)
        {
            // replace space with '+'
            return new string(original.Select(ch => ch == ' ' ? '+' : ch).ToArray());
        }

        /* a method that substring the given string */
        private static string substringMethod(string original, string startingString, int additionIndex)
        {
            return  original.Substring(original.IndexOf(startingString) + additionIndex);
        }
        #endregion
    }
}
