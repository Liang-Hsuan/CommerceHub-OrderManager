using System.Xml;

namespace Order_Manager.supportingClasses.Shipment
{
    /*
     * A abstract class that defines some methods for all carrier classes
     */
    public abstract class Shipment
    {
        // field for xml processing
        protected XmlDocument doc = new XmlDocument();

        // fields error indication
        public bool Error { get; protected set; }
        public string ErrorMessage { get; protected set; }

        #region Supporting Methods
        /* a method that substring the given string */
        protected static string substringMethod(string original, string startingString, int additionIndex)
        {
            return original.Substring(original.IndexOf(startingString) + additionIndex);
        }

        /* a method that get the next target token */
        protected static string getTarget(string text)
        {
            int i = 0;
            while (text[i] != '<' && text[i] != '>' && text[i] != '"')
                i++;

            return text.Substring(0, i);
        }
        #endregion
    }
}
