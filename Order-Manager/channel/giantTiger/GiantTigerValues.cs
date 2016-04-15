using Order_Manager.supportingClasses.Address;
using Order_Manager.supportingClasses.Shipment;
using System;
using System.Collections.Generic;

namespace Order_Manager.channel.giantTiger
{
    /*
     * A class for storing giant tiger order values
     */
    public class GiantTigerValues
    {
        // fields for order details
        public string PoNumber { get; set; }
        public string WebOrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShipMethod { get; set; }
        public Address ShipTo { get; set; }
        public string StoreNumber { get; set; }
        public string OmsOrderNumber { get; set; }
        public Package Package { get; set; }

        // fields for item details
        public List<string> VendorSku { get; set; }
        public List<string> HostSku { get; set; }
        public List<string> ClientItemId { get; set; }
        public List<double> UnitCost { get; set; }
        public List<int> Quantity { get; set; }

        /* first constructor that take no argument */
        public GiantTigerValues()
        {
            PoNumber = "";
            WebOrderNo = "";
            OrderDate = DateTime.Today;
            ShipMethod = "";
            ShipTo = new Address();
            StoreNumber = "";
            OmsOrderNumber = "";
            Package = new Package();

            VendorSku = new List<string>();
            HostSku = new List<string>();
            ClientItemId = new List<string>();
            UnitCost = new List<double>();
            Quantity = new List<int>();
        }

        /* second constructor that take all the parametoers */
        public GiantTigerValues(string poNumber, string webOrderNo, DateTime orderDate, string shipMethod, Address shipTo, string storeName, string omsOrderNumber, Package package,
                                List<string> vendorSku, List<string> hostSku, List<string> clientItemId, List<double> unitCost, List<int> quantity)
        {
            PoNumber = poNumber;
            WebOrderNo = webOrderNo;
            OrderDate = orderDate;
            ShipMethod = shipMethod;
            ShipTo = shipTo;
            StoreNumber = storeName;
            OmsOrderNumber = omsOrderNumber;
            Package = package;

            VendorSku = vendorSku;
            HostSku = hostSku;
            ClientItemId = clientItemId;
            UnitCost = unitCost;
            Quantity = quantity;
        }
    }
}
