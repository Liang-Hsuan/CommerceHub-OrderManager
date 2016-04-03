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
    }
}
