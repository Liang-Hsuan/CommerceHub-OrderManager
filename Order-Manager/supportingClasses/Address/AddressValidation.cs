using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace Order_Manager.supportingClasses.Address
{
    /*
     * A class that validate the given address
     */
    public static class AddressValidation
    {
        // local fields for web request
        private static WebRequest request;
        private static HttpWebResponse response;

        /* the main method of the object that return if the address is valid or not */
        public static bool validate(Address address)
        {
            // generate uri
            string uri = "https://maps.googleapis.com/maps/api/geocode/json?address=";

            uri += address.Address1.Replace(' ', '+') + ",";
            uri += address.City.Replace(' ', '+') + ",";
            uri += address.State.Replace(' ', '+') + ",";
            uri += address.PostalCode.Replace(' ', '+');

            uri += "&key=AIzaSyASdOsY2T3vBfYn1lBE5VQl7nZ-ivp1vKs";
            
            // post request to web server
            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";

            // get the response from the server
            response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = streamReader.ReadToEnd();

            // deserialize json to key value
            var info = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(result);

            // only has 1 result will be the correct value
            return info["results"].Count == 1;
        }
    }
}
